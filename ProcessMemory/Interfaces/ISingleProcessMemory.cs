using System;
using System.Text;
using System.Collections.Generic;
using Common.Enums;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProcessMemory.Interfaces
{
    public interface ISingleProcessMemory : IDisposable
    {
        protected const string c_mainModuleName = "mainModule";

        public bool WriteBytesToOffsets(IEnumerable<long> offsets, IEnumerable<byte> bytesToWrite, string moduleName = c_mainModuleName);

        public bool WriteBytesToOffset(long offset, IEnumerable<byte> bytesToWrite, string moduleName = c_mainModuleName);

        public bool WriteByteToOffsets(IEnumerable<long> offsets, byte byteToWrite, string moduleName = c_mainModuleName);

        public bool WriteByteToOffset(long offset, byte byteToWrite, string moduleName = c_mainModuleName);

        public bool WriteIntToOffsets(IEnumerable<long> offsets, int numToWrite, string moduleName = c_mainModuleName);

        public bool WriteIntToOffset(long offset, int numToWrite, string moduleName = c_mainModuleName);

        public bool WriteFloatToOffsets(IEnumerable<long> offsets, float numToWrite, string moduleName = c_mainModuleName);

        public bool WriteFloatToOffset(long offset, float numToWrite, string moduleName = c_mainModuleName);

        public bool WriteDoubleToOffsets(IEnumerable<long> offsets, double numToWrite, string moduleName = c_mainModuleName);

        public bool WriteDoubleToOffset(long offset, double numToWrite, string moduleName = c_mainModuleName);

        public bool WriteStringToOffsets(IEnumerable<long> offsets, string stringToWrite, Encoding encoding, string moduleName = c_mainModuleName);

        public bool WriteStringToOffset(long offset, string stringToWrite, Encoding encoding, string moduleName = c_mainModuleName);

        public bool WriteLongToOffset(long offset, long numToWrite, string moduleName = c_mainModuleName);

        public bool WriteLongToOffsets(IEnumerable<long> offsets, long numToWrite, string moduleName = c_mainModuleName);

        public IEnumerable<byte> ReadBytesFromOffsets(IEnumerable<long> offsets, int numOfBytesToRead, string moduleName = c_mainModuleName);

        public IEnumerable<byte> ReadBytesFromOffset(long offset, int numOfBytesToRead, string moduleName = c_mainModuleName);

        public byte ReadByteFromOffsets(IEnumerable<long> offsets, string moduleName = c_mainModuleName);

        public byte ReadByteFromOffset(long offset, string moduleName = c_mainModuleName);

        public int ReadIntFromOffset(long offset, string moduleName = c_mainModuleName);

        public int ReadIntFromOffsets(IEnumerable<long> offsets, string moduleName = c_mainModuleName);

        public float ReadFloatFromOffset(long offset, string moduleName = c_mainModuleName);

        public float ReadFloatFromOffsets(IEnumerable<long> offsets, string moduleName = c_mainModuleName);

        public double ReadDoubleFromOffset(long offset, string moduleName = c_mainModuleName);

        public double ReadDoubleFromOffsets(IEnumerable<long> offsets, string moduleName = c_mainModuleName);

        public string ReadStringFromOffset(long offset, int numOfCharsToRead, Encoding encoding, string moduleName = c_mainModuleName);

        public string ReadStringFromOffsets(IEnumerable<long> offsets, int numOfCharsToRead, Encoding encoding, string moduleName = c_mainModuleName);

        public long ReadLongFromOffset(long offset, string moduleName = c_mainModuleName);

        public long ReadLongFromOffsets(IEnumerable<long> offsets, string moduleName = c_mainModuleName);

        public bool FreezeValue(IEnumerable<long> offsets, float value, string moduleName = c_mainModuleName);

        public bool FreezeValue(long offset, float value, string moduleName = c_mainModuleName);

        public bool FreezeValue(IEnumerable<long> offsets, double value, string moduleName = c_mainModuleName);

        public bool FreezeValue(long offset, double value, string moduleName = c_mainModuleName);

        public bool FreezeValue(IEnumerable<long> offsets, string value, Encoding encoding, string moduleName = c_mainModuleName);

        public bool FreezeValue(long offset, string value, Encoding encoding, string moduleName = c_mainModuleName);

        public bool FreezeValue(IEnumerable<long> offsets, int value, string moduleName = c_mainModuleName);

        public bool FreezeValue(long offset, int value, string moduleName = c_mainModuleName);

        public bool FreezeValue(IEnumerable<long> offsets, byte value, string moduleName = c_mainModuleName);

        public bool FreezeValue(long offset, byte value, string moduleName = c_mainModuleName);

        public bool FreezeValue(long offset, long value, string moduleName = c_mainModuleName);

        public bool UnFreezeValue(long offset, string moduleName = c_mainModuleName);

        public bool UnFreezeValue(IEnumerable<long> offsets, string moduleName = c_mainModuleName);

        public bool IsProcessOpen();

        public bool ChangeMemoryProtection(IntPtr address, int size, MemoryProtection memoryProtection, out MemoryProtection oldMemoryProtection);

        public bool ChangeMemoryProtection(long offset, int size, MemoryProtection memoryProtection, out MemoryProtection oldMemoryProtection, string moduleName = c_mainModuleName);

        public bool ChangeMemoryProtection(IEnumerable<long> offsets, int size, MemoryProtection memoryProtection, out MemoryProtection oldMemoryProtection, string moduleName = c_mainModuleName);

        public IntPtr GetAddress(IEnumerable<long> offsets, string moduleName = c_mainModuleName);

        public IntPtr GetAddress(long offset, string moduleName = c_mainModuleName);
    }
}
