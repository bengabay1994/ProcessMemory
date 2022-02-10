using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics;
using ProcessMemory;
using ProcessMemory.Interfaces;
using System.Collections.Generic;
using System.Text;
using System;
using System.Runtime.InteropServices;

namespace SingleProcessMemoryTests
{
    [TestClass]
    public class UnitTest1
    {
        private string processName;
        private Process process;
        private ISingleProcessMemory singleProcessMemory;
        private IntPtr addressOfTestedMemory;
        private long offsetToTestedMemory;

        [TestInitialize]
        public void initTests() 
        {
            process = Process.GetCurrentProcess();
            processName = process.ProcessName;
            singleProcessMemory = new SingleProcessMemory(processName);
            addressOfTestedMemory = Marshal.AllocHGlobal(4096 * 3); // allocate 3 pages to work with
            offsetToTestedMemory = addressOfTestedMemory.ToInt64() - process.MainModule.BaseAddress.ToInt64();
            preparePointer(1024); // init a pointer to the second page at offset 1024 in the first page.
        }

        [TestMethod]
        public void WriteBytesToOffsetsTest()
        {
            // Arrange
            string wordToWrite = "HelloWorld!";
            IEnumerable<long> offsets = new List<long>(){ offsetToTestedMemory+1024, 10 };
            IEnumerable<byte> BytesToWrite = Encoding.Unicode.GetBytes("HelloWorld!");

            // Act
            bool isWriteSucceeded = singleProcessMemory.WriteBytesToOffsets(offsets, BytesToWrite);
            string wordWritten = Marshal.PtrToStringUni(addressOfTestedMemory + 4096 + 10);

            // Assert
            Assert.IsTrue(isWriteSucceeded);
            Assert.AreEqual(wordToWrite, wordWritten);
        }

        [TestMethod]
        public void WriteBytesToOffsetTest()
        {
            // Arrange
            string wordToWrite = "HelloWorld!";
            IEnumerable<byte> BytesToWrite = Encoding.Unicode.GetBytes("HelloWorld!");

            // Act
            bool isWriteSucceeded = singleProcessMemory.WriteBytesToOffset(offsetToTestedMemory, BytesToWrite);
            string wordWritten = Marshal.PtrToStringUni(addressOfTestedMemory);

            // Assert
            Assert.IsTrue(isWriteSucceeded);
            Assert.AreEqual(wordToWrite, wordWritten);
        }

        private void preparePointer(int offset) 
        {
            IntPtr pointToHere = (IntPtr)(addressOfTestedMemory.ToInt64() + 4096);
            Marshal.WriteIntPtr(addressOfTestedMemory, offset, pointToHere);
        }
    }
}
