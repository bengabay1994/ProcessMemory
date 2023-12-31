using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics;
using ProcessMemory;
using ProcessMemory.Interfaces;
using System.Collections.Generic;
using System.Text;
using System;
using System.Linq;
using System.Runtime.InteropServices;
using ProcessMemory.Enums;
using System.Threading;
using static ProcessMemory.Utils.ExtensionsMethods.IntPtrExtensionMethod;

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

        #region Testing Write Methods
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
            Assert.IsTrue(isWriteSucceeded);
            Assert.IsTrue(isWrite2Succeeded);
            Assert.AreEqual(num1, num1Read);
            Assert.AreEqual(num2, num2Read);
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
            Assert.IsTrue(isWriteSucceeded);
            Assert.IsTrue(isWrite2Succeeded);
            Assert.AreEqual(num1, num1Read);
            Assert.AreEqual(num2, num2Read);
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
            Assert.IsTrue(isWriteSucceeded);
            Assert.IsTrue(isWrite2Succeeded);
            Assert.AreEqual(num1, num1ReadF);
            Assert.AreEqual(num2, num2ReadF);
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
            Assert.IsTrue(isWriteSucceeded);
            Assert.IsTrue(isWrite2Succeeded);
            Assert.AreEqual(num1, num1ReadF);
            Assert.AreEqual(num2, num2ReadF);
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
            Assert.IsTrue(isWriteSucceeded);
            Assert.IsTrue(isWrite2Succeeded);
            Assert.AreEqual(num1, num1ReadD);
            Assert.AreEqual(num2, num2ReadD);
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
        public void WriteLongToOffsetTest() 
        {
            // Arrange
            long num1 = 31890674;
            long num2 = 12;
            int offset = 310;

            // Act
            bool isWriteSucceeded = singleProcessMemory.WriteLongToOffset(offsetToTestedMemory + offset, num1);
            bool isWrite2Succeeded = singleProcessMemory.WriteLongToOffset(offsetToTestedMemory + offset + sizeof(long), num2);
            long num1Read = Marshal.ReadInt64(addressOfTestedMemory + offset);
            long num2Read = Marshal.ReadInt64(addressOfTestedMemory + offset + sizeof(long));

            // Assert
            Assert.IsTrue(isWriteSucceeded);
            Assert.IsTrue(isWrite2Succeeded);
            Assert.AreEqual(num1, num1Read);
            Assert.AreEqual(num2, num2Read);
        }

        [TestMethod]
        public void WriteLongToOffsetsTest() 
        {
            // Arrange
            IEnumerable<long> offsets = new long[] { offsetToTestedMemory + 1024, 310 };
            IEnumerable<long> offsets2 = new long[] { offsetToTestedMemory + 1024, 318 };
            long num1 = 34;
            long num2 = 24583982745;

            // Act
            bool isWriteSucceeded = singleProcessMemory.WriteLongToOffsets(offsets, num1);
            bool isWrite2Succeeded = singleProcessMemory.WriteLongToOffsets(offsets2, num2);
            long num1Read = Marshal.ReadInt64(addressOfTestedMemory + 4096 + 310);
            long num2Read = Marshal.ReadInt64(addressOfTestedMemory + 4096 + 318);

            // Assert
            Assert.IsTrue(isWriteSucceeded);
            Assert.IsTrue(isWrite2Succeeded);
            Assert.AreEqual(num1, num1Read);
            Assert.AreEqual(num2, num2Read);
        }
        #endregion

        #region Testing Read Methods
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
            // Arrange
            int num = 150;
            int num2 = 543214;
            IEnumerable<long> offsets = new long[] { offsetToTestedMemory + 1024, 450 };
            IEnumerable<long> offsets2 = new long[] { offsetToTestedMemory + 1024, 454 };
            Marshal.WriteInt32(addressOfTestedMemory + 4096 + 450, num);
            Marshal.WriteInt32(addressOfTestedMemory + 4096 + 454, num2);

            // Act
            int numRead = singleProcessMemory.ReadIntFromOffsets(offsets);
            int num2Read = singleProcessMemory.ReadIntFromOffsets(offsets2);

            // Assert
            Assert.AreEqual(num, numRead);
            Assert.AreEqual(num2, num2Read);
        }

        [TestMethod]
        public void ReadFloatFromOffsetTest() 
        {
            // Arrange
            float num = 150.976f;
            float num2 = 350.251f;
            int offset = 500;
            var numAsBytes = BitConverter.GetBytes(num);
            var num2AsBytes = BitConverter.GetBytes(num2);
            for (int i = 0; i < 4; ++i) 
            {
                Marshal.WriteByte(addressOfTestedMemory + offset + i, numAsBytes[i]);
                Marshal.WriteByte(addressOfTestedMemory + offset + 4 + i, num2AsBytes[i]);
            }

            // Act
            float numRead = singleProcessMemory.ReadFloatFromOffset(offsetToTestedMemory + offset);
            float num2Read = singleProcessMemory.ReadFloatFromOffset(offsetToTestedMemory + offset + 4);

            // Assert
            Assert.AreEqual(num, numRead);
            Assert.AreEqual(num2, num2Read);
        }

        [TestMethod]
        public void ReadFloatFromOffsetsTest() 
        {
            // Arrange
            float num = 150.423f;
            float num2 = 5414.128f;
            IEnumerable<long> offsets = new long[] { offsetToTestedMemory + 1024, 500 };
            IEnumerable<long> offsets2 = new long[] { offsetToTestedMemory + 1024, 504 };
            var numAsBytes = BitConverter.GetBytes(num);
            var num2AsBytes = BitConverter.GetBytes(num2);
            for (int i = 0; i < 4; ++i)
            {
                Marshal.WriteByte(addressOfTestedMemory + 4096 + 500 + i, numAsBytes[i]);
                Marshal.WriteByte(addressOfTestedMemory + 4096 + 504 + i, num2AsBytes[i]);
            }

            // Act
            float numRead = singleProcessMemory.ReadFloatFromOffsets(offsets);
            float num2Read = singleProcessMemory.ReadFloatFromOffsets(offsets2);

            // Assert
            Assert.AreEqual(num, numRead);
            Assert.AreEqual(num2, num2Read);
        }

        [TestMethod]
        public void ReadStringFromOffsetTest() 
        {
            // Arrange
            string stringToRead = "I love reading";
            int offset = 550;
            var stringToReadAsBytes = Encoding.Unicode.GetBytes(stringToRead);
            int i = 0;
            foreach (var b in stringToReadAsBytes) 
            {
                Marshal.WriteByte(addressOfTestedMemory + offset + i++, b);
            }

            // Act
            string stringRead = singleProcessMemory.ReadStringFromOffset(offsetToTestedMemory + offset, stringToRead.Length, Encoding.Unicode);

            // Assert
            Assert.AreEqual(stringToRead, stringRead);
        }

        [TestMethod]
        public void ReadStringFromOffsetsTest() 
        {
            // Arrange
            string stringToRead = "I love pointers";
            IEnumerable<long> offsets = new long[] { offsetToTestedMemory + 1024, 550 };
            var stringToReadAsBytes = Encoding.Unicode.GetBytes(stringToRead);
            int i = 0;
            foreach (var b in stringToReadAsBytes)
            {
                Marshal.WriteByte(addressOfTestedMemory + 4096 + 550 + i++, b);
            }

            // Act
            string stringRead = singleProcessMemory.ReadStringFromOffsets(offsets, stringToRead.Length, Encoding.Unicode);

            // Assert
            Assert.AreEqual(stringToRead, stringRead);
        }

        [TestMethod]
        public void ReadDoubleFromOffsetTest() 
        {
            // Arrange
            double num = 159.97675;
            double num2 = 9972.9293;
            int offset = 650;
            var numAsBytes = BitConverter.GetBytes(num);
            var num2AsBytes = BitConverter.GetBytes(num2);
            for (int i = 0; i < 8; ++i)
            {
                Marshal.WriteByte(addressOfTestedMemory + offset + i, numAsBytes[i]);
                Marshal.WriteByte(addressOfTestedMemory + offset + 8 + i, num2AsBytes[i]);
            }

            // Act
            double numRead = singleProcessMemory.ReadDoubleFromOffset(offsetToTestedMemory + offset);
            double num2Read = singleProcessMemory.ReadDoubleFromOffset(offsetToTestedMemory + offset + 8);

            // Assert
            Assert.AreEqual(num, numRead);
            Assert.AreEqual(num2, num2Read);
        }

        [TestMethod]
        public void ReadDoubleFromOffsetsTest() 
        {
            // Arrange
            double num = 3113.423;
            double num2 = 12.1212;
            IEnumerable<long> offsets = new long[] { offsetToTestedMemory + 1024, 650 };
            IEnumerable<long> offsets2 = new long[] { offsetToTestedMemory + 1024, 658 };
            var numAsBytes = BitConverter.GetBytes(num);
            var num2AsBytes = BitConverter.GetBytes(num2);
            for (int i = 0; i < 8; ++i)
            {
                Marshal.WriteByte(addressOfTestedMemory + 4096 + 650 + i, numAsBytes[i]);
                Marshal.WriteByte(addressOfTestedMemory + 4096 + 658 + i, num2AsBytes[i]);
            }

            // Act
            double numRead = singleProcessMemory.ReadDoubleFromOffsets(offsets);
            double num2Read = singleProcessMemory.ReadDoubleFromOffsets(offsets2);

            // Assert
            Assert.AreEqual(num, numRead);
            Assert.AreEqual(num2, num2Read);
        }

        [TestMethod]
        public void ReadLongFromOffsetTest()
        {
            // Arrange
            long num1 = 6987546321;
            long num2 = 22;
            int offset = 700;
            Marshal.WriteInt64(addressOfTestedMemory + offset, num1);
            Marshal.WriteInt64(addressOfTestedMemory + offset + 8, num2);

            // Act
            long numRead = singleProcessMemory.ReadLongFromOffset(offsetToTestedMemory + offset);
            long num2Read = singleProcessMemory.ReadLongFromOffset(offsetToTestedMemory + offset + 8);

            // Assert
            Assert.AreEqual(num1, numRead);
            Assert.AreEqual(num2, num2Read);
        }

        [TestMethod]
        public void ReadLongFromOffsetsTest()
        {
            // Arrange
            long num = 150;
            long num2 = 5629545222;
            IEnumerable<long> offsets = new long[] { offsetToTestedMemory + 1024, 700 };
            IEnumerable<long> offsets2 = new long[] { offsetToTestedMemory + 1024, 708 };
            Marshal.WriteInt64(addressOfTestedMemory + 4096 + 700, num);
            Marshal.WriteInt64(addressOfTestedMemory + 4096 + 708, num2);

            // Act
            long numRead = singleProcessMemory.ReadLongFromOffsets(offsets);
            long num2Read = singleProcessMemory.ReadLongFromOffsets(offsets2);

            // Assert
            Assert.AreEqual(num, numRead);
            Assert.AreEqual(num2, num2Read);
        }

        #endregion

        #region Testing Change Protection Methods
        [TestMethod]
        public void ChangeProtectionTest() 
        {
            // Arrange
            IntPtr lastPage = addressOfTestedMemory + 8192;
            int num = 5;
            int num2 = 15;
            MemoryProtection readOnly = MemoryProtection.PageReadOnly;

            // Act
            bool writeWhenAble = singleProcessMemory.WriteIntToOffset(offsetToTestedMemory + 8192, num);
            bool writeWhenAble2 = singleProcessMemory.WriteIntToOffset(offsetToTestedMemory + 8192 + 4, num2);
            bool isChanged = singleProcessMemory.ChangeMemoryProtection(lastPage, 8, readOnly, out MemoryProtection original);
            bool writeWhenCant = singleProcessMemory.WriteIntToOffset(offsetToTestedMemory + 8192 + 4, num);
            int readNum = singleProcessMemory.ReadIntFromOffset(offsetToTestedMemory + 8192);
            int readNum2 = singleProcessMemory.ReadIntFromOffset(offsetToTestedMemory + 8192 + 4);
            bool isChanged2 = singleProcessMemory.ChangeMemoryProtection(lastPage, 8, original, out MemoryProtection update);

            // Assert
            Assert.IsTrue(writeWhenAble);
            Assert.IsTrue(writeWhenAble2);
            Assert.IsTrue(isChanged);
            Assert.AreEqual(original, MemoryProtection.PageReadWrite);
            Assert.IsFalse(writeWhenCant);
            Assert.AreEqual(num, readNum);
            Assert.AreEqual(num2, readNum2);
            Assert.IsTrue(isChanged2);
            Assert.AreEqual(update, readOnly);
        }

        #endregion

        #region Testing Freeze Methods
        [TestMethod]
        public void FullFreezeTest() 
        {
            // Arrange
            int inum = 100;
            long lnum = 5000;
            double dnum = 300.124;
            float fnum = 200.754f;
            byte bnum = 0x20;
            string snum = "23";
            int inumAfterUnFreeze = 101;
            long lnumAfterUnFreeze = 5001;
            double dnumAfterUnFreeze = 301.124;
            float fnumAfterUnFreeze = 201.754f;
            byte bnumAfterUnFreeze = 0x21;
            string snumAfterUnFreeze = "24";
            List<bool> condToCheckTrue = new List<bool>();
            var encoding = Encoding.Unicode;
            int baseOffset = 800;

            // Act
            condToCheckTrue.Add(singleProcessMemory.FreezeValue(offsetToTestedMemory + baseOffset, inum));
            condToCheckTrue.Add(singleProcessMemory.FreezeValue(offsetToTestedMemory + baseOffset + 4, lnum));
            condToCheckTrue.Add(singleProcessMemory.FreezeValue(offsetToTestedMemory + baseOffset + 12, dnum));
            condToCheckTrue.Add(singleProcessMemory.FreezeValue(offsetToTestedMemory + baseOffset + 20, fnum));
            condToCheckTrue.Add(singleProcessMemory.FreezeValue(offsetToTestedMemory + baseOffset + 24, bnum));
            condToCheckTrue.Add(singleProcessMemory.FreezeValue(offsetToTestedMemory + baseOffset + 25, snum, encoding));

            // writing different values to make sure the freeze will override them.
            condToCheckTrue.Add(singleProcessMemory.WriteIntToOffset(offsetToTestedMemory + baseOffset, inumAfterUnFreeze));
            condToCheckTrue.Add(singleProcessMemory.WriteLongToOffset(offsetToTestedMemory + baseOffset + 4, lnumAfterUnFreeze));
            condToCheckTrue.Add(singleProcessMemory.WriteDoubleToOffset(offsetToTestedMemory + baseOffset + 12, dnumAfterUnFreeze));
            condToCheckTrue.Add(singleProcessMemory.WriteFloatToOffset(offsetToTestedMemory + baseOffset + 20, fnumAfterUnFreeze));
            condToCheckTrue.Add(singleProcessMemory.WriteByteToOffset(offsetToTestedMemory + baseOffset + 24, bnumAfterUnFreeze));
            condToCheckTrue.Add(singleProcessMemory.WriteStringToOffset(offsetToTestedMemory + baseOffset + 25, snumAfterUnFreeze, encoding));

            Thread.Sleep(250); // let the freeze threads write the real values back

            int inumRead = singleProcessMemory.ReadIntFromOffset(offsetToTestedMemory + baseOffset);
            long lnumRead = singleProcessMemory.ReadLongFromOffset(offsetToTestedMemory + baseOffset + 4);
            double dnumRead = singleProcessMemory.ReadDoubleFromOffset(offsetToTestedMemory + baseOffset + 12);
            float fnumRead = singleProcessMemory.ReadFloatFromOffset(offsetToTestedMemory + baseOffset + 20);
            byte bnumRead = singleProcessMemory.ReadByteFromOffset(offsetToTestedMemory + baseOffset + 24);
            string snumRead = singleProcessMemory.ReadStringFromOffset(offsetToTestedMemory + baseOffset + 25, snum.Length, encoding);

            condToCheckTrue.Add(singleProcessMemory.UnFreezeValue(offsetToTestedMemory + baseOffset));
            condToCheckTrue.Add(singleProcessMemory.UnFreezeValue(offsetToTestedMemory + baseOffset + 4));
            condToCheckTrue.Add(singleProcessMemory.UnFreezeValue(offsetToTestedMemory + baseOffset + 12));
            condToCheckTrue.Add(singleProcessMemory.UnFreezeValue(offsetToTestedMemory + baseOffset + 20));
            condToCheckTrue.Add(singleProcessMemory.UnFreezeValue(offsetToTestedMemory + baseOffset + 24));
            condToCheckTrue.Add(singleProcessMemory.UnFreezeValue(offsetToTestedMemory + baseOffset + 25));

            Thread.Sleep(250); // let the cancellation token enough time to cancel the Freeze Address

            // writing different values to make sure the Unfreeze worked.
            condToCheckTrue.Add(singleProcessMemory.WriteIntToOffset(offsetToTestedMemory + baseOffset, inumAfterUnFreeze));
            condToCheckTrue.Add(singleProcessMemory.WriteLongToOffset(offsetToTestedMemory + baseOffset + 4, lnumAfterUnFreeze));
            condToCheckTrue.Add(singleProcessMemory.WriteDoubleToOffset(offsetToTestedMemory + baseOffset + 12, dnumAfterUnFreeze));
            condToCheckTrue.Add(singleProcessMemory.WriteFloatToOffset(offsetToTestedMemory + baseOffset + 20, fnumAfterUnFreeze));
            condToCheckTrue.Add(singleProcessMemory.WriteByteToOffset(offsetToTestedMemory + baseOffset + 24, bnumAfterUnFreeze));
            condToCheckTrue.Add(singleProcessMemory.WriteStringToOffset(offsetToTestedMemory + baseOffset + 25, snumAfterUnFreeze, encoding));

            int inumReadAfterUnFreeze = singleProcessMemory.ReadIntFromOffset(offsetToTestedMemory + baseOffset);
            long lnumReadAfterUnFreeze = singleProcessMemory.ReadLongFromOffset(offsetToTestedMemory + baseOffset + 4);
            double dnumReadAfterUnFreeze = singleProcessMemory.ReadDoubleFromOffset(offsetToTestedMemory + baseOffset + 12);
            float fnumReadAfterUnFreeze = singleProcessMemory.ReadFloatFromOffset(offsetToTestedMemory + baseOffset + 20);
            byte bnumReadAfterUnFreeze = singleProcessMemory.ReadByteFromOffset(offsetToTestedMemory + baseOffset + 24);
            string snumReadAfterUnFreeze = singleProcessMemory.ReadStringFromOffset(offsetToTestedMemory + baseOffset + 25, snum.Length, encoding);

            // Assert
            AreTrues(condToCheckTrue);
            Assert.AreEqual(inum, inumRead);
            Assert.AreEqual(lnum, lnumRead);
            Assert.AreEqual(dnum, dnumRead);
            Assert.AreEqual(fnum, fnumRead);
            Assert.AreEqual(bnum, bnumRead);
            Assert.AreEqual(snum, snumRead);
            Assert.AreEqual(inumAfterUnFreeze, inumReadAfterUnFreeze);
            Assert.AreEqual(lnumAfterUnFreeze, lnumReadAfterUnFreeze);
            Assert.AreEqual(dnumAfterUnFreeze, dnumReadAfterUnFreeze);
            Assert.AreEqual(fnumAfterUnFreeze, fnumReadAfterUnFreeze);
            Assert.AreEqual(bnumAfterUnFreeze, bnumReadAfterUnFreeze);
            Assert.AreEqual(snumAfterUnFreeze, snumReadAfterUnFreeze);
        }

        [TestMethod]
        public void FullFreezeTest2() 
        {
            // Arrange
            int inum = 100;
            long lnum = 5000;
            double dnum = 300.124;
            float fnum = 200.754f;
            byte bnum = 0x20;
            string snum = "23";
            int inumAfterUnFreeze = 101;
            long lnumAfterUnFreeze = 5001;
            double dnumAfterUnFreeze = 301.124;
            float fnumAfterUnFreeze = 201.754f;
            byte bnumAfterUnFreeze = 0x21;
            string snumAfterUnFreeze = "24";
            List<bool> condToCheckTrue = new List<bool>();
            var encoding = Encoding.Unicode;
            int baseOffset = 900;

            // Act
            condToCheckTrue.Add(singleProcessMemory.FreezeValue(getAddress, (object)baseOffset, "key1", inum));
            condToCheckTrue.Add(singleProcessMemory.FreezeValue(getAddress, (object)(baseOffset + 4), "key2", lnum));
            condToCheckTrue.Add(singleProcessMemory.FreezeValue(getAddress, (object)(baseOffset + 12), "key3", dnum));
            condToCheckTrue.Add(singleProcessMemory.FreezeValue(getAddress, (object)(baseOffset + 20), "key4", fnum));
            condToCheckTrue.Add(singleProcessMemory.FreezeValue(getAddress, (object)(baseOffset + 24), "key5", bnum));
            condToCheckTrue.Add(singleProcessMemory.FreezeValue(getAddress, (object)(baseOffset + 25), "key6", snum, encoding));

            // writing different values to make sure the freeze will override them.
            condToCheckTrue.Add(singleProcessMemory.WriteIntToAddress(addressOfTestedMemory + baseOffset, inumAfterUnFreeze));
            condToCheckTrue.Add(singleProcessMemory.WriteLongToAddress(addressOfTestedMemory + baseOffset + 4, lnumAfterUnFreeze));
            condToCheckTrue.Add(singleProcessMemory.WriteDoubleToAddress(addressOfTestedMemory + baseOffset + 12, dnumAfterUnFreeze));
            condToCheckTrue.Add(singleProcessMemory.WriteFloatToAddress(addressOfTestedMemory + baseOffset + 20, fnumAfterUnFreeze));
            condToCheckTrue.Add(singleProcessMemory.WriteByteToAddress(addressOfTestedMemory + baseOffset + 24, bnumAfterUnFreeze));
            condToCheckTrue.Add(singleProcessMemory.WriteStringToAddress(addressOfTestedMemory + baseOffset + 25, snumAfterUnFreeze, encoding));

            Thread.Sleep(500); // let the freeze threads write the real values back

            int inumRead = singleProcessMemory.ReadIntFromAddress(addressOfTestedMemory + baseOffset);
            long lnumRead = singleProcessMemory.ReadLongFromAddress(addressOfTestedMemory + baseOffset + 4);
            double dnumRead = singleProcessMemory.ReadDoubleFromAddress(addressOfTestedMemory + baseOffset + 12);
            float fnumRead = singleProcessMemory.ReadFloatFromAddress(addressOfTestedMemory + baseOffset + 20);
            byte bnumRead = singleProcessMemory.ReadByteFromAddress(addressOfTestedMemory + baseOffset + 24);
            string snumRead = singleProcessMemory.ReadStringFromAddress(addressOfTestedMemory + baseOffset + 25, snum.Length, encoding);

            condToCheckTrue.Add(singleProcessMemory.UnFreezeValue("key1"));
            condToCheckTrue.Add(singleProcessMemory.UnFreezeValue("key2"));
            condToCheckTrue.Add(singleProcessMemory.UnFreezeValue("key3"));
            condToCheckTrue.Add(singleProcessMemory.UnFreezeValue("key4"));
            condToCheckTrue.Add(singleProcessMemory.UnFreezeValue("key5"));
            condToCheckTrue.Add(singleProcessMemory.UnFreezeValue("key6"));

            Thread.Sleep(500); // let the cancellation token enough time to cancel the Freeze Address

            // writing different values to make sure the Unfreeze worked.
            condToCheckTrue.Add(singleProcessMemory.WriteIntToAddress(addressOfTestedMemory + baseOffset, inumAfterUnFreeze));
            condToCheckTrue.Add(singleProcessMemory.WriteLongToAddress(addressOfTestedMemory + baseOffset + 4, lnumAfterUnFreeze));
            condToCheckTrue.Add(singleProcessMemory.WriteDoubleToAddress(addressOfTestedMemory + baseOffset + 12, dnumAfterUnFreeze));
            condToCheckTrue.Add(singleProcessMemory.WriteFloatToAddress(addressOfTestedMemory + baseOffset + 20, fnumAfterUnFreeze));
            condToCheckTrue.Add(singleProcessMemory.WriteByteToAddress(addressOfTestedMemory + baseOffset + 24, bnumAfterUnFreeze));
            condToCheckTrue.Add(singleProcessMemory.WriteStringToAddress(addressOfTestedMemory + baseOffset + 25, snumAfterUnFreeze, encoding));

            int inumReadAfterUnFreeze = singleProcessMemory.ReadIntFromAddress(addressOfTestedMemory + baseOffset);
            long lnumReadAfterUnFreeze = singleProcessMemory.ReadLongFromAddress(addressOfTestedMemory + baseOffset + 4);
            double dnumReadAfterUnFreeze = singleProcessMemory.ReadDoubleFromAddress(addressOfTestedMemory + baseOffset + 12);
            float fnumReadAfterUnFreeze = singleProcessMemory.ReadFloatFromAddress(addressOfTestedMemory + baseOffset + 20);
            byte bnumReadAfterUnFreeze = singleProcessMemory.ReadByteFromAddress(addressOfTestedMemory + baseOffset + 24);
            string snumReadAfterUnFreeze = singleProcessMemory.ReadStringFromAddress(addressOfTestedMemory + baseOffset + 25, snum.Length, encoding);

            // Assert
            AreTrues(condToCheckTrue);
            Assert.AreEqual(inum, inumRead);
            Assert.AreEqual(lnum, lnumRead);
            Assert.AreEqual(dnum, dnumRead);
            Assert.AreEqual(fnum, fnumRead);
            Assert.AreEqual(bnum, bnumRead);
            Assert.AreEqual(snum, snumRead);
            Assert.AreEqual(inumAfterUnFreeze, inumReadAfterUnFreeze);
            Assert.AreEqual(lnumAfterUnFreeze, lnumReadAfterUnFreeze);
            Assert.AreEqual(dnumAfterUnFreeze, dnumReadAfterUnFreeze);
            Assert.AreEqual(fnumAfterUnFreeze, fnumReadAfterUnFreeze);
            Assert.AreEqual(bnumAfterUnFreeze, bnumReadAfterUnFreeze);
            Assert.AreEqual(snumAfterUnFreeze, snumReadAfterUnFreeze);
        }
        #endregion

        #region Memory Allocation Tests
        [TestMethod]
        public void AllocateMemoryTest() 
        {
            // Arrange
            int num = 10;
            long lnum = 29L;

            // Act
            IntPtr newMem = singleProcessMemory.AllocateMemory(12, MemoryProtection.PageExecuteReadWrite);
            bool isWriteSucceeded = singleProcessMemory.WriteIntToAddress(newMem, num);
            bool isWriteSucceeded2 = singleProcessMemory.WriteLongToAddress(newMem + 4, lnum);
            int numRead = singleProcessMemory.ReadIntFromAddress(newMem);
            long lnumRead = singleProcessMemory.ReadLongFromAddress(newMem + 4);

            // Assert
            Assert.IsTrue(newMem.IsPointerValid());
            Assert.IsTrue(isWriteSucceeded);
            Assert.IsTrue(isWriteSucceeded2);
            Assert.AreEqual(num, numRead);
            Assert.AreEqual(lnum, lnumRead);
        }
        #endregion

        #region Private Methods
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

        private IntPtr getAddress(object offset)
        {
            return addressOfTestedMemory + (int)offset;
        }

        private void AreTrues(IEnumerable<bool> checkForTrues) 
        {
            foreach (bool cond in checkForTrues) 
            {
                Assert.IsTrue(cond);
            }
        }

        #endregion
    }
}
