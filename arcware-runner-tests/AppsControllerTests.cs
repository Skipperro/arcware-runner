using System;
using System.Diagnostics;
using arcware_runner;
using NUnit.Framework;

namespace arcware_runner_tests
{
    public class AppsControllerTests
    {
        private AppsController _ac = new AppsController();

        [SetUp]
        public void Setup()
        {
            _ac = new AppsController();
        }


        [Test]
        public void Reset_Works()
        {
            _ac.StartSubProcess(new ProcessStartInfo("dir"));
            _ac.StartSubProcess(new ProcessStartInfo("dir"));
            Assert.IsTrue(_ac.SubProcesses.Count == 2, "Expected to have 2 SubProcesses, got {0}",
                _ac.SubProcesses.Count);
            _ac.Reset();
            Assert.IsTrue(_ac.SubProcesses.Count == 0, "Expected to have 0 SubProcesses, got {0}",
                _ac.SubProcesses.Count);
        }

        [Test]
        public void SubProcess_Management_Working_Windows()
        {
            if (OperatingSystem.IsWindows())
            {
                var guid = _ac.StartSubProcess(new ProcessStartInfo("dir"));
                _ac.SubProcesses[guid].WaitForExit();
                Assert.IsTrue(_ac.SubProcesses[guid].HasExited);
                _ac.RemoveSubProcess(guid);
                Assert.IsFalse(_ac.SubProcesses.ContainsKey(guid));

                guid = _ac.StartSubProcess(new ProcessStartInfo("ping", "127.0.0.1"));
                _ac.KillSubProcess(guid);
                Assert.IsTrue(_ac.SubProcesses[guid].HasExited);
                _ac.RemoveSubProcess(guid);
                Assert.IsFalse(_ac.SubProcesses.ContainsKey(guid));
            }
        }

        [Test]
        public void SubProcess_Management_Working_Linux()
        {
            if (OperatingSystem.IsWindows())
            {
                var guid = _ac.StartSubProcess(new ProcessStartInfo("ls"));
                _ac.SubProcesses[guid].WaitForExit();
                Assert.IsTrue(_ac.SubProcesses[guid].HasExited);
                _ac.RemoveSubProcess(guid);
                Assert.IsFalse(_ac.SubProcesses.ContainsKey(guid));

                guid = _ac.StartSubProcess(new ProcessStartInfo("ping", "127.0.0.1"));
                _ac.KillSubProcess(guid);
                Assert.IsTrue(_ac.SubProcesses[guid].HasExited);
                _ac.RemoveSubProcess(guid);
                Assert.IsFalse(_ac.SubProcesses.ContainsKey(guid));
            }
        }

        [Test]
        public void GetSubProcessOutput_Works()
        {
            var guid = Guid.Empty;
            if (OperatingSystem.IsLinux())
            {
                guid = _ac.StartSubProcess(new ProcessStartInfo("ls"));
            }

            if (OperatingSystem.IsWindows())
            {
                guid = _ac.StartSubProcess(new ProcessStartInfo("dir"));
            }

            var res = _ac.GetSubProcessOutput(guid);
            Assert.IsTrue(res == _ac.GetSubProcessOutput(guid));
        }

        [Test]
        public void GetSubProcessErrorOutput_Works()
        {
            var guid = _ac.StartSubProcess(new ProcessStartInfo("dir", "asdasd"));
            var res = _ac.GetSubProcessErrorOutput(guid);
            Assert.IsTrue(res == _ac.GetSubProcessErrorOutput(guid));
        }

        [Test]
        public void RunCommand_WorksOnLinux()
        {
            if (OperatingSystem.IsLinux())
            {
                var res = AppsController.I.RunCommand("ls", "");
                Assert.IsTrue(res.Started, "Simple ls command was unable to run!");
                if (res.Output != "") Console.WriteLine("OUTPUT: " + Environment.NewLine + res.Output);
                if (res.Error != "") Console.WriteLine("ERROR: " + Environment.NewLine + res.Error);
                Assert.IsTrue(res.Finished, "Simple ls command exited without finishing");
                Assert.IsTrue(res.Runtime < 2000, "Simple ls command took way to long to execute ({0} ms)",
                    res.Runtime);

                res = AppsController.I.RunCommand("echo", "Hello World!");
                Assert.IsTrue(res.Output.Trim() == "Hello World!",
                    "Output from echo command doesn't match!{0}Expected: Hello World!{0}Get: {1}", Environment.NewLine,
                    res.Output);
            }
        }

        [Test]
        public void RunCommand_WorksOnWindows()
        {
            if (OperatingSystem.IsWindows())
            {
                var res = AppsController.I.RunCommand("dir", "");
                Assert.IsTrue(res.Started, "Simple dir command was unable to run!");
                if (res.Output != "") Console.WriteLine("OUTPUT: " + Environment.NewLine + res.Output);
                if (res.Error != "") Console.WriteLine("ERROR: " + Environment.NewLine + res.Error);
                Assert.IsTrue(res.Finished, "Simple dir command exited without finishing");
                Assert.IsTrue(res.Runtime < 2000, "Simple dir command took way to long to execute ({0} ms)",
                    res.Runtime);
                
                res = AppsController.I.RunCommand("echo", "Hello World!");
                Assert.IsTrue(res.Output.Trim() == "Hello World!",
                    "Output from echo command doesn't match!{0}Expected: Hello World!{0}Get: {1}", Environment.NewLine,
                    res.Output);
            }
        }

        [Test]
        public void RunCommand_FailsWhenNeeded()
        {
            Assert.Catch(() => { AppsController.I.RunCommand("NotExistingSoftwareThatShouldFail", "Fake Arguments"); },
                "Fake command was somehow able to run without exception!");
            var res = AppsController.I.RunCommand("ping", "127.0.0.1", 5);
            Assert.IsTrue(res.Started, "Simple ping command was unable to run!");
            Assert.IsTrue(res.Runtime < 20, "Simple ping command was to slow to be killed!");
            Assert.IsFalse(res.Finished, "Simple ping command was finished and killed at the same time!");
        }
    }
}