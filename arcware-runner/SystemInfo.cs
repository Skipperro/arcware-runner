using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.NetworkInformation;
using Hardware.Info;
using Newtonsoft.Json;

namespace arcware_runner
{
    public class SystemInfo
    {
        private static readonly HardwareInfo HInfo = new HardwareInfo();

        public List<CPU> CPUs { get; set; }
        public List<GPU> GPUs { get; set; }
        public MBO Mainboard { get; set; }
        public List<Drive> Drives { get; set; }
        public ulong TotalMemory { get; set; }
        public List<string> InternalIPs { get; set; }
        public string ExternalIP { get; set; }
        public string OS { get; set; }
        public List<SystemProcess> Processes { get; set; }

        public class CPU
        {
            public CPU()
            {
                // For serialization
            }

            public CPU(string name, int cores, int threads)
            {
                Name = name ?? throw new ArgumentNullException(nameof(name));
                Cores = cores;
                Threads = threads;
            }

            public string Name { get; set; }
            public int Cores { get; set; }
            public int Threads { get; set; }
        }
        public class GPU
        {
            public GPU()
            {
                // For serialization
            }

            public GPU(string name)
            {
                Name = name ?? throw new ArgumentNullException(nameof(name));
            }

            public string Name { get; set; }
        }
        public class MBO
        {
            public MBO()
            {
                // For serialization
            }

            public MBO(string manufacturer, string model, string serialNumber)
            {
                Manufacturer = manufacturer ?? throw new ArgumentNullException(nameof(manufacturer));
                Model = model ?? throw new ArgumentNullException(nameof(model));
                SerialNumber = serialNumber ?? throw new ArgumentNullException(nameof(serialNumber));
            }

            public string Manufacturer { get; set; }
            public string Model { get; set; }
            public string SerialNumber { get; set; }
        }
        public class Drive
        {
            public Drive(string name, string type, string label, string fileSystem, ulong totalSpace,
                ulong availableSpace)
            {
                Name = name ?? throw new ArgumentNullException(nameof(name));
                Type = type ?? throw new ArgumentNullException(nameof(type));
                Label = label ?? throw new ArgumentNullException(nameof(label));
                FileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
                TotalSpace = totalSpace;
                AvailableSpace = availableSpace;
            }

            public Drive()
            {
                // For serialization
            }

            public string Name { get; set; }
            public string Type { get; set; }
            public string Label { get; set; }
            public string FileSystem { get; set; }
            public ulong TotalSpace { get; set; }
            public ulong AvailableSpace { get; set; }
        }

        public class SystemProcess
        {
            public SystemProcess()
            {
                // For serialization
            }

            public SystemProcess(Process proc)
            {
                this.Id = proc.Id;
                this.Name = proc.ProcessName;
                this.Hangs = !proc.Responding;
                this.Prio = proc.BasePriority.ToString();
                this.StartTime = proc.StartTime;
            }

            public DateTime StartTime { get; set; }
            public string Prio { get; set; }
            public bool Hangs { get; set; }
            public string Name { get; set; }
            public int Id { get; set; }
        }
        public SystemInfo()
        {
            // For serialization
        }
        
        public SystemInfo(bool loadData)
        {
            if (loadData) this.Reload();
        }
        public void Reload()
        {
            this.CPUs = new List<CPU>();
            this.GPUs = new List<GPU>();
            this.Drives = new List<Drive>();
            this.InternalIPs = new List<string>();
            HInfo.RefreshAll();
            foreach (var cpu in HInfo.CpuList)
            {
                this.CPUs.Add(new CPU(cpu.Name, (int)cpu.NumberOfCores, (int)cpu.NumberOfLogicalProcessors));
            }
            foreach (var gpu in HInfo.VideoControllerList)
            {
                this.GPUs.Add(new GPU(gpu.Description));
            }
            foreach (var mbo in HInfo.MotherboardList)
            {
                this.Mainboard = new MBO(mbo.Manufacturer, mbo.Product, mbo.SerialNumber);
            }
            this.TotalMemory = HInfo.MemoryStatus.TotalPhysical;
            foreach (var address in HardwareInfo.GetLocalIPv4Addresses(NetworkInterfaceType.Ethernet,
                OperationalStatus.Up))
            {
                IPAddress ipAddress = null;
                bool isValidIp = IPAddress.TryParse(address.ToString(), out ipAddress);
                if (isValidIp) InternalIPs.Add(address.ToString());
            }
                
            OS = "unknown";
            if (OperatingSystem.IsWindows()) OS = "Windows";
            if (OperatingSystem.IsLinux()) OS = "Linux";
            if (OperatingSystem.IsAndroid()) OS = "Android";
            if (OperatingSystem.IsMacOS()) OS = "Mac";
            
            DriveInfo[] allDrives = DriveInfo.GetDrives();
            foreach (DriveInfo d in allDrives)
            {
                if (d.IsReady)
                {
                   this.Drives.Add(new Drive(d.Name, d.DriveType.ToString(), d.VolumeLabel, d.DriveFormat, (ulong)d.TotalSize, (ulong)d.AvailableFreeSpace));
                }
            }

            this.ExternalIP = SystemInfo.GetExternalIP();

            this.Processes = new List<SystemProcess>();
            foreach (var p in Process.GetProcesses())
            {
                try
                {
                    this.Processes.Add(new SystemProcess(p));
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }

        }
        public static void WriteInfo()
        {
            HInfo.RefreshAll();

            Console.WriteLine(HInfo.MemoryStatus);

            foreach (var hardware in HInfo.BatteryList)
                Console.WriteLine(hardware);

            foreach (var hardware in HInfo.BiosList)
                Console.WriteLine(hardware);

            foreach (var hardware in HInfo.CpuList)
                Console.WriteLine(hardware);

            foreach (var drive in HInfo.DriveList)
            {
                Console.WriteLine(drive);

                foreach (var partition in drive.PartitionList)
                {
                    Console.WriteLine(partition);

                    foreach (var volume in partition.VolumeList)
                        Console.WriteLine(volume);
                }
            }

            foreach (var hardware in HInfo.KeyboardList)
                Console.WriteLine(hardware);

            foreach (var hardware in HInfo.MonitorList)
                Console.WriteLine(hardware);

            foreach (var hardware in HInfo.MotherboardList)
                Console.WriteLine(hardware);

            foreach (var hardware in HInfo.MouseList)
                Console.WriteLine(hardware);

            foreach (var hardware in HInfo.NetworkAdapterList)
                Console.WriteLine(hardware);

            foreach (var hardware in HInfo.PrinterList)
                Console.WriteLine(hardware);

            foreach (var hardware in HInfo.SoundDeviceList)
                Console.WriteLine(hardware);

            foreach (var hardware in HInfo.VideoControllerList)
                Console.WriteLine(hardware);


            foreach (var address in HardwareInfo.GetLocalIPv4Addresses(NetworkInterfaceType.Ethernet,
                OperationalStatus.Up))
                Console.WriteLine(address);

            Console.WriteLine();

            foreach (var address in HardwareInfo.GetLocalIPv4Addresses(OperationalStatus.Up))
                Console.WriteLine(address);

            Console.WriteLine();

            foreach (var address in HardwareInfo.GetLocalIPv4Addresses())
                Console.WriteLine(address);

        }
        public static string GetExternalIP()
        {
            var wc = new WebClient();
            try
            {
                var result = wc.DownloadString("http://ipinfo.io/ip");
            
                IPAddress ipAddress = null;
                var isValidIp = IPAddress.TryParse(result, out ipAddress);

                if (isValidIp) return result;
            }
            catch (WebException e)
            {
                Console.WriteLine(e.Message);
            }
            
            return "";
        }

        public string GetJson()
        {
            var json = JsonConvert.SerializeObject(this);
            return json;
        }
    }
}