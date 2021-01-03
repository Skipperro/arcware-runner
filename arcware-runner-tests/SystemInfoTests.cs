using System;
using arcware_runner;
using NUnit.Framework;

namespace arcware_runner_tests
{
    public class SystemInfoTests
    {
        [SetUp]
        public void Setup()
        {
            
        }

        [Test]
        public void SystemInfo_Looks_Valid()
        {
            var systemInfo = new SystemInfo(true);
            
            Assert.IsTrue(systemInfo.CPUs.Count > 0, "CPU List is empty!");
            foreach(var cpu in systemInfo.CPUs)
            {
                Assert.IsTrue(cpu != null, "CPU is null!");
                Assert.IsTrue(cpu.Name != null, "Detected CPU Name is null!");
                Assert.IsTrue(cpu.Name.Length > 5, "Detected CPU Name is too short! It was {0}", cpu.Name);
                Assert.IsTrue(cpu.Cores > 0, "Detected CPU have 0 cores!");
                Assert.IsTrue(cpu.Threads > 0, "Detected CPU have 0 threads!");
            }
            
            foreach(var gpu in systemInfo.GPUs)
            {
                Assert.IsTrue(gpu != null, "GPU is null!");
                Assert.IsTrue(gpu.Name != null, "Detected GPU Name is null!");
                Assert.IsTrue(gpu.Name.Length > 5, "Detected GPU Name is too short! It was {0}", gpu.Name);
            }
            
            Assert.IsTrue(systemInfo.TotalMemory > 0, "Available RAM capacity is zero!");
            
        }

        [Test]
        public void CPU_Creation_is_Proper()
        {
            Assert.DoesNotThrow(() =>
            {
                var cpu1 = new arcware_runner.SystemInfo.CPU();
            });
            var cpu = new arcware_runner.SystemInfo.CPU("Test CPU", 4, 8);
            Assert.IsTrue(cpu != null, "CPU is null!");
            Assert.IsTrue(cpu.Name == "Test CPU", "CPU name ({0}) is different than expected (Test CPU)!", cpu.Name);
            Assert.IsTrue(cpu.Cores == 4, "CPU core count ({0}) is different than defined (4)!", cpu.Cores);
            Assert.IsTrue(cpu.Threads == 8, "CPU thread count ({0}) is different than defined (8)!", cpu.Threads);

            Assert.Catch<ArgumentNullException>(() =>
                {
                    var cpu2 = new arcware_runner.SystemInfo.CPU(null, 4, 8);
                }, 
                "Passing empty name to CPU constructor doesn't throw exception!");
        }
        
        [Test]
        public void GPU_Creation_is_Proper()
        {
            Assert.DoesNotThrow(() =>
            {
                new arcware_runner.SystemInfo.GPU();
            });
            var gpu = new arcware_runner.SystemInfo.GPU("Test GPU");
            Assert.IsTrue(gpu.Name == "Test GPU", "GPU name ({0}) is different than expected (Test GPU)!", gpu.Name);

            Assert.Catch<ArgumentNullException>(() => new arcware_runner.SystemInfo.GPU(null), 
                "Passing empty name to GPU constructor doesn't throw exception!");
        }

        [Test]
        public void Serialization_Works()
        {
            var originalSI = new SystemInfo();
            originalSI.Reload();
            var json = originalSI.GetJson();
            var deserializedSI = Newtonsoft.Json.JsonConvert.DeserializeObject<SystemInfo>(json);
            Console.WriteLine(json);
            if (deserializedSI == null) throw new NullReferenceException("deserializedSI is null!");
            Assert.IsTrue(deserializedSI.Mainboard.Model == originalSI.Mainboard.Model, "Mainboard models don't match! Original: [{0}] Deserialized: [{1}]", originalSI.Mainboard.Model, deserializedSI.Mainboard.Model);
            Assert.IsTrue(deserializedSI.Mainboard.Manufacturer == originalSI.Mainboard.Manufacturer, "Mainboard manufacturer don't match! Original: [{0}] Deserialized: [{1}]", originalSI.Mainboard.Manufacturer, deserializedSI.Mainboard.Manufacturer);
            Assert.IsTrue(deserializedSI.CPUs.Count == originalSI.CPUs.Count, "CPU count don't match! Original: {0}, Deserialized: {1}", originalSI.CPUs.Count, deserializedSI.CPUs.Count);
            Assert.IsTrue(deserializedSI.CPUs[0].Name == originalSI.CPUs[0].Name, "CPU name don't match! Original: {0}, Deserialized: {1}", originalSI.CPUs[0].Name, deserializedSI.CPUs[0].Name);
            Assert.IsTrue(deserializedSI.TotalMemory == originalSI.TotalMemory, "Total memory don't match! Original: {0}, Deserialized: {1}", originalSI.TotalMemory, deserializedSI.TotalMemory);
            Assert.IsTrue(deserializedSI.GPUs.Count == originalSI.GPUs.Count, "GPU count don't match! Original: {0}, Deserialized: {1}", originalSI.GPUs.Count, deserializedSI.GPUs.Count);

            Assert.IsTrue(deserializedSI.Processes.Count > 10, "Process counts seems to be unrealistically low!");
            Assert.IsTrue(deserializedSI.Processes.Count == originalSI.Processes.Count, "Process counts don't match!");
        }
    }
}