using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics;
using ProcessMemory;
using ProcessMemory.Interfaces;
using System.Collections.Generic;
using System.Text;
using System;
using System.Linq;
using System.Runtime.InteropServices;

namespace SingleProcessMemoryTests
{
    [TestClass]
    public class SingleProcessMemoryUnitTests
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
            IEnumerable<long> offsets = new List<long>() { offsetToTestedMemory + 1024, 10 };
            IEnumerable<byte> BytesToWrite = Encoding.Unicode.GetBytes(wordToWrite + "\0");

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

        [TestMethod]
        public void WriteIntToOffsetTest()
        {
            // Arrange
            int num1 = 20;
            int num2 = 40;
            int offset = 50;

            // Act
            bool isWriteSucceeded = singleProcessMemory.WriteIntToOffset(offsetToTestedMemory + offset, num1);
            bool isWrite2Succeeded = singleProcessMemory.WriteIntToOffset(offsetToTestedMemory + offset + 4, num2);
            int num1Read = Marshal.ReadInt32(addressOfTestedMemory + offset);
            int num2Read = Marshal.ReadInt32(addressOfTestedMemory + offset + 4);

            // Assert
            Assert.AreEqual(num1, num1Read);
            Assert.AreEqual(num2, num2Read);
            Assert.IsTrue(isWriteSucceeded);
            Assert.IsTrue(isWrite2Succeeded);
        }

        [TestMethod]
        public void WirteIntToOffsetsTest()
        {
            // Arrange
            IEnumerable<long> offsets = new long[] { offsetToTestedMemory + 1024, 50 };
            IEnumerable<long> offsets2 = new long[] { offsetToTestedMemory + 1024, 54 };
            int num1 = 30;
            int num2 = 24583745;

            // Act
            bool isWriteSucceeded = singleProcessMemory.WriteIntToOffsets(offsets, num1);
            bool isWrite2Succeeded = singleProcessMemory.WriteIntToOffsets(offsets2, num2);
            int num1Read = Marshal.ReadInt32(addressOfTestedMemory + 4096 + 50);
            int num2Read = Marshal.ReadInt32(addressOfTestedMemory + 4096 + 54);

            // Assert
            Assert.AreEqual(num1, num1Read);
            Assert.AreEqual(num2, num2Read);
            Assert.IsTrue(isWriteSucceeded);
            Assert.IsTrue(isWrite2Succeeded);
        }

        [TestMethod]
        public void WriteFloatToOffsetTest()
        {
            // Arrange
            float num1 = 200.3472F;
            float num2 = 3.14692847F;
            int offset = 100;

            // Act
            bool isWriteSucceeded = singleProcessMemory.WriteFloatToOffset(offsetToTestedMemory + offset, num1);
            bool isWrite2Succeeded = singleProcessMemory.WriteFloatToOffset(offsetToTestedMemory + offset + 4, num2);
            byte[] num1Read = new byte[4];
            byte[] num2Read = new byte[4];
            for (int i = 0; i < 4; ++i)
            {
                num1Read[i] = Marshal.ReadByte(addressOfTestedMemory + offset + i);
                num2Read[i] = Marshal.ReadByte(addressOfTestedMemory + offset + 4 + i);
            }
            float num1ReadF = BitConverter.ToSingle(num1Read);
            float num2ReadF = BitConverter.ToSingle(num2Read);

            // Assert
            Assert.AreEqual(num1, num1ReadF);
            Assert.AreEqual(num2, num2ReadF);
            Assert.IsTrue(isWriteSucceeded);
            Assert.IsTrue(isWrite2Succeeded);
        }

        [TestMethod]
        public void WriteFloatToOffsetsTest()
        {
            // Arrange
            float num1 = 212.3472F;
            float num2 = 3.964857F;
            IEnumerable<long> offsets = new long[] { offsetToTestedMemory + 1024, 100 };
            IEnumerable<long> offsets2 = new long[] { offsetToTestedMemory + 1024, 104 };

            // Act
            bool isWriteSucceeded = singleProcessMemory.WriteFloatToOffsets(offsets, num1);
            bool isWrite2Succeeded = singleProcessMemory.WriteFloatToOffsets(offsets2, num2);
            byte[] num1Read = new byte[4];
            byte[] num2Read = new byte[4];
            for (int i = 0; i < 4; ++i)
            {
                num1Read[i] = Marshal.ReadByte(addressOfTestedMemory + 4096 + 100 + i);
                num2Read[i] = Marshal.ReadByte(addressOfTestedMemory + 4096 + 104 + i);
            }
            float num1ReadF = BitConverter.ToSingle(num1Read);
            float num2ReadF = BitConverter.ToSingle(num2Read);

            // Assert
            Assert.AreEqual(num1, num1ReadF);
            Assert.AreEqual(num2, num2ReadF);
            Assert.IsTrue(isWriteSucceeded);
            Assert.IsTrue(isWrite2Succeeded);
        }

        [TestMethod]
        public void WirteStringToOffsetTest()
        {
            // Arrange
            string str = "Write Me please";
            int offset = 150;

            // Act
            bool isWriteSucceeded = singleProcessMemory.WriteStringToOffset(offsetToTestedMemory + offset, str, Encoding.Unicode);
            string strRead = Marshal.PtrToStringUni(addressOfTestedMemory + offset);
            byte[] strAsBytes = Encoding.Unicode.GetBytes(strRead);

            // Assert
            Assert.IsTrue(isWriteSucceeded);
            Assert.AreEqual(str, strRead);
        }

        [TestMethod]
        public void WriteStringToOffsetsTest()
        {
            // Arrange
            string str = "Write Me please";
            IEnumerable<long> offsets = new long[] { offsetToTestedMemory + 1024, 150 };

            // Act
            bool isWriteSucceeded = singleProcessMemory.WriteStringToOffsets(offsets, str, Encoding.Unicode);
            string strRead = Marshal.PtrToStringUni(addressOfTestedMemory + 4096 + 150);

            // Assert
            Assert.IsTrue(isWriteSucceeded);
            Assert.AreEqual(str, strRead);
        }

        [TestMethod]
        public void WriteDoubleToOffsetTest()
        {
            // Arrange
            double num1 = 456.2341;
            double num2 = 109.872;
            int offset = 250;

            // Act
            bool isWriteSucceeded = singleProcessMemory.WriteDoubleToOffset(offsetToTestedMemory + offset, num1);
            bool isWrite2Succeeded = singleProcessMemory.WriteDoubleToOffset(offsetToTestedMemory + offset + 8, num2);
            byte[] num1Read = new byte[8];
            byte[] num2Read = new byte[8];
            for (int i = 0; i < 8; ++i)
            {
                num1Read[i] = Marshal.ReadByte(addressOfTestedMemory + offset + i);
                num2Read[i] = Marshal.ReadByte(addressOfTestedMemory + offset + 8 + i);
            }
            double num1ReadD = BitConverter.ToDouble(num1Read);
            double num2ReadD = BitConverter.ToDouble(num2Read);

            // Assert
            Assert.IsTrue(isWriteSucceeded);
            Assert.IsTrue(isWrite2Succeeded);
            Assert.AreEqual(num1, num1ReadD);
            Assert.AreEqual(num2, num2ReadD);
        }

        [TestMethod]
        public void WriteDoubleToOffsetsTest()
        {
            // Arrange
            double num1 = 212.3472;
            double num2 = 3.964857;
            IEnumerable<long> offsets = new long[] { offsetToTestedMemory + 1024, 250 };
            IEnumerable<long> offsets2 = new long[] { offsetToTestedMemory + 1024, 258 };

            // Act
            bool isWriteSucceeded = singleProcessMemory.WriteDoubleToOffsets(offsets, num1);
            bool isWrite2Succeeded = singleProcessMemory.WriteDoubleToOffsets(offsets2, num2);
            byte[] num1Read = new byte[8];
            byte[] num2Read = new byte[8];
            for (int i = 0; i < 8; ++i)
            {
                num1Read[i] = Marshal.ReadByte(addressOfTestedMemory + 4096 + 250 + i);
                num2Read[i] = Marshal.ReadByte(addressOfTestedMemory + 4096 + 258 + i);
            }
            double num1ReadD = BitConverter.ToDouble(num1Read);
            double num2ReadD = BitConverter.ToDouble(num2Read);

            // Assert
            Assert.AreEqual(num1, num1ReadD);
            Assert.AreEqual(num2, num2ReadD);
            Assert.IsTrue(isWriteSucceeded);
            Assert.IsTrue(isWrite2Succeeded);
        }

        [TestMethod]
        public void WriteByteToOffsetTest()
        {
            // Arrange
            byte byteToWrite = 0x60;
            byte byteToWrite2 = 0x41;
            int offset = 300;

            // Act
            bool isWriteSucceeded = singleProcessMemory.WriteByteToOffset(offsetToTestedMemory + offset, byteToWrite);
            bool isWrite2Succeeded = singleProcessMemory.WriteByteToOffset(offsetToTestedMemory + offset + 1, byteToWrite2);
            byte byteRead = Marshal.ReadByte(addressOfTestedMemory + offset);
            byte byteRead2 = Marshal.ReadByte(addressOfTestedMemory + offset + 1);

            // Assert
            Assert.IsTrue(isWriteSucceeded);
            Assert.IsTrue(isWrite2Succeeded);
            Assert.AreEqual(byteRead, byteToWrite);
            Assert.AreEqual(byteRead2, byteToWrite2);
        }

        [TestMethod]
        public void WriteByteToOffsetsTest()
        {
            // Arrange
            byte byteToWrite = 0x60;
            byte byteToWrite2 = 0x41;
            IEnumerable<long> offsets = new long[] { offsetToTestedMemory + 1024, 300 };
            IEnumerable<long> offsets2 = new long[] { offsetToTestedMemory + 1024, 301 };

            // Act
            bool isWriteSucceeded = singleProcessMemory.WriteByteToOffsets(offsets, byteToWrite);
            bool isWrite2Succeeded = singleProcessMemory.WriteByteToOffsets(offsets2, byteToWrite2);
            byte byteRead = Marshal.ReadByte(addressOfTestedMemory + 4096 + 300);
            byte byteRead2 = Marshal.ReadByte(addressOfTestedMemory + 4096 + 301);

            // Assert
            Assert.IsTrue(isWriteSucceeded);
            Assert.IsTrue(isWrite2Succeeded);
            Assert.AreEqual(byteRead, byteToWrite);
            Assert.AreEqual(byteRead2, byteToWrite2);
        }

        [TestMethod]
        public void ReadBytesFromOffsetsTest()
        {
            // Arrange
            byte[] bytesToRead = new byte[] { 0x41, 0x42, 0x60, 0xf3, 0x08, 0x00, 0xff };
            int offset = 350;
            int i = 0;
            foreach (byte b in bytesToRead) 
            {
                Marshal.WriteByte(addressOfTestedMemory + offset + i++, b);
            }

            // Act
            IEnumerable<byte> bytesRead = singleProcessMemory.ReadBytesFromOffset(offsetToTestedMemory + offset, bytesToRead.Length);

            // Assert
            Assert.IsNotNull(bytesRead);
            Assert.IsTrue(AreBytesEquals(bytesToRead, bytesRead));
        }

        [TestMethod]
        public void ReadBytesFromOffsetTest()
        {
            // Arrange
            byte[] bytesToRead = new byte[] { 0x41, 0x42, 0x60, 0xf3, 0x08, 0x00, 0xff };
            IEnumerable<long> offsets = new long[] { offsetToTestedMemory + 1024, 350 };
            int i = 0;
            foreach (byte b in bytesToRead)
            {
                Marshal.WriteByte(addressOfTestedMemory + 4096 + 350 + i++, b);
            }

            // Act
            IEnumerable<byte> bytesRead = singleProcessMemory.ReadBytesFromOffsets(offsets, bytesToRead.Length);

            // Assert
            Assert.IsNotNull(bytesRead);
            Assert.IsTrue(AreBytesEquals(bytesToRead, bytesRead));
        }

        [TestMethod]
        public void ReadByteFromOffsetsTest() 
        {
            // Arrange
            byte byteToRead = 0x41;
            byte byteToRead2 = 0xfe;
            IEnumerable<long> offsets = new long[] { offsetToTestedMemory + 1024, 400};
            IEnumerable<long> offsets2 = new long[] { offsetToTestedMemory + 1024, 401 };
            Marshal.WriteByte(addressOfTestedMemory + 4096 + 400, byteToRead);
            Marshal.WriteByte(addressOfTestedMemory + 4096 + 401, byteToRead2);

            // Act
            byte byteRead = singleProcessMemory.ReadByteFromOffsets(offsets);
            byte byte2Read = singleProcessMemory.ReadByteFromOffsets(offsets2);

            // Assert
            Assert.AreEqual(byteToRead, byteRead);
            Assert.AreEqual(byteToRead2, byte2Read);
        }

        [TestMethod]
        public void ReadByteFromOffsetTest() 
        {
            // Arrange
            byte byteToRead = 0x1f;
            byte byteToRead2 = 0x20;
            int offset = 400;
            Marshal.WriteByte(addressOfTestedMemory + offset, byteToRead);
            Marshal.WriteByte(addressOfTestedMemory + offset + 1, byteToRead2);

            // Act
            byte byteRead = singleProcessMemory.ReadByteFromOffset(offsetToTestedMemory + offset);
            byte byte2Read = singleProcessMemory.ReadByteFromOffset(offsetToTestedMemory + offset + 1);

            // Assert
            Assert.AreEqual(byteToRead, byteRead);
            Assert.AreEqual(byteToRead2, byte2Read);
        }


        [TestMethod]
        public void ReadIntFromOffsetTest()
        {
            // Arrange
            int num1 = 100;
            int num2 = 907;
            int offset = 450;
            Marshal.WriteInt32(addressOfTestedMemory + offset, num1);
            Marshal.WriteInt32(addressOfTestedMemory + offset + 4, num2);

            // Act
            int numRead = singleProcessMemory.ReadIntFromOffset(offsetToTestedMemory + offset);
            int num2Read = singleProcessMemory.ReadIntFromOffset(offsetToTestedMemory + offset + 4);

            // Assert
            Assert.AreEqual(num1, numRead);
            Assert.AreEqual(num2, num2Read);
        }

        [TestMethod]
        public void ReadIntFromOffsetsTest()
        {
            throw new NotImplementedException();
        }

        [TestMethod]
        public void ReadFloatFromOffsetTest() 
        {
            throw new NotImplementedException();
        }

        [TestMethod]
        public void ReadFloatFromOffsetsTest() 
        {
            throw new NotImplementedException();
        }

        [TestMethod]
        public void ReadStringFromOffsetTest() 
        {
            throw new NotImplementedException();
        }

        [TestMethod]
        public void ReadStringFromOffsetsTest() 
        {
            throw new NotImplementedException();
        }

        [TestMethod]
        public void ReadDoubleFromOffsetTest() 
        {
            throw new NotImplementedException();
        }

        [TestMethod]
        public void ReadDoubleFromOffsetsTest() 
        {
            throw new NotImplementedException();
        }

        private void preparePointer(int offset) 
        {
            IntPtr pointToHere = (IntPtr)(addressOfTestedMemory.ToInt64() + 4096);
            Marshal.WriteIntPtr(addressOfTestedMemory, offset, pointToHere);
        }

        private bool AreBytesEquals(IEnumerable<byte> bytes1, IEnumerable<byte> bytes2) 
        {
            int count1 = bytes1.Count();
            int count2 = bytes2.Count();
            if (count1 != count2) 
            {
                return false;
            }
            for (int i = 0; i < count1; ++i)
            {
                if (bytes1.ElementAt(i) != bytes2.ElementAt(i)) 
                {
                    return false;
                }
            }
            return true;
        }
    }
}
