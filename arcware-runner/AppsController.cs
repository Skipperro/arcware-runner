using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace arcware_runner
{
    public class AppsController
    {
        private static AppsController _instance = new();
        public Dictionary<Guid, ArcwareProcess> SubProcesses = new();

        public AppsController()
        {
            try
            {
                Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.High;
                Console.WriteLine("Priority was successfully set to HIGH!");
            }
            catch
            {
                Console.WriteLine("WARNING: Unable to set priority to high. Please consider starting this software with elevated permissions.");
            }
            
        }

        public static AppsController I => _instance ??= new AppsController();

        public void Reset()
        {
            var templist = this.SubProcesses.Keys;
            foreach (var g in templist)
            {
                this.KillSubProcess(g);
            }
            this.SubProcesses = new Dictionary<Guid, ArcwareProcess>();
        }

        public Guid StartSubProcess(ProcessStartInfo startInfo)
        {
            var g = Guid.NewGuid();
            var proc = new ArcwareProcess();
            proc.StartInfo = startInfo;
            proc.StartInfo.RedirectStandardOutput = true;
            proc.StartInfo.RedirectStandardError = true;
            SubProcesses.Add(g, proc);
            var started = proc.Start();
            if (started) return g;
            throw new Exception($"Unable to start process [{g}]!");
        }

        public void RemoveSubProcess(Guid guid)
        {
            if (SubProcesses.ContainsKey(guid))
            {
                KillSubProcess(guid);
                SubProcesses.Remove(guid);
            }
        }

        public void KillSubProcess(Guid guid)
        {
            SubProcesses[guid].Kill(true);
        }

        public string GetSubProcessOutput(Guid guid)
        {
            return SubProcesses[guid].CompleteOutput;
        }
        public string GetSubProcessErrorOutput(Guid guid)
        {
            return SubProcesses[guid].CompleteErrorOutput;
        }

        public CommandResult RunCommand(string command, string arguments, int executionLimit = 10000)
        {
            var proc = new Process();
            proc.StartInfo.FileName = command;
            proc.StartInfo.Arguments = arguments;
            proc.StartInfo.RedirectStandardOutput = true;
            proc.StartInfo.RedirectStandardError = true;

            var completeCommand = $"{command} {arguments}";

            if (!proc.Start())
            {
                return new CommandResult(completeCommand, false, false, "", "", 0);
            }

            var sw = new Stopwatch();
            sw.Start();
            proc.WaitForExit(executionLimit);
            sw.Stop();
            var finished = true;
            if (!proc.HasExited)
            {
                proc.Kill(true);
                finished = false;
            }
            return new CommandResult(completeCommand, true, finished, proc.StandardOutput.ReadToEnd(), proc.StandardError.ReadToEnd(), (uint)sw.ElapsedMilliseconds); 
        }
    }

    public class ArcwareProcess : Process
    {
        private string _completeOutput = "";
        private string _completeErrorOutput = "";
        public string CompleteOutput
        {
            get
            {
                _completeOutput += StandardOutput.ReadToEnd();
                return _completeOutput;
            }
        }
        public string CompleteErrorOutput
        {
            get
            {
                _completeErrorOutput += StandardError.ReadToEnd();
                return _completeErrorOutput;
            }
        }
    }

    public class CommandResult
    {
        public CommandResult()
        {
            // For serialization
        }

        public CommandResult(string command, bool started, bool finished, string output, string error, uint runtime)
        {
            Command = command ?? throw new ArgumentNullException(nameof(command));
            Started = started;
            Finished = finished;
            Output = output ?? throw new ArgumentNullException(nameof(output));
            Error = error ?? throw new ArgumentNullException(nameof(error));
            Runtime = runtime;
        }

        public string Command { get; set; }
        public bool Started { get; set; }
        public bool Finished { get; set; }
        public string Output { get; set; }
        public string Error { get; set; }
        public uint Runtime { get; set; }
    }
}