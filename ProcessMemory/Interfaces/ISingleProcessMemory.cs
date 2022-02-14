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

        protected const MemoryAllocationType c_defaultAllocationType = MemoryAllocationType.MemCommit | MemoryAllocationType.MemReserve;

        public bool WriteBytesToAddress(IntPtr address, IEnumerable<byte> value);

        public bool WriteByteToAddress(IntPtr address, byte value);

        public bool WriteIntToAddress(IntPtr address, int value);

        public bool WriteLongToAddress(IntPtr address, long value);

        public bool WriteFloatToAddress(IntPtr address, float value);

        public bool WriteDoubleToAddress(IntPtr address, double value);

        public bool WriteStringToAddress(IntPtr address, string value, Encoding encoding);

        public bool WriteBytesToOffsets(IEnumerable<long> offsets, IEnumerable<byte> value, string moduleName = c_mainModuleName);

        public bool WriteBytesToOffset(long offset, IEnumerable<byte> value, string moduleName = c_mainModuleName);

        public bool WriteByteToOffsets(IEnumerable<long> offsets, byte value, string moduleName = c_mainModuleName);

        public bool WriteByteToOffset(long offset, byte value, string moduleName = c_mainModuleName);

        public bool WriteIntToOffsets(IEnumerable<long> offsets, int value, string moduleName = c_mainModuleName);

        public bool WriteIntToOffset(long offset, int value, string moduleName = c_mainModuleName);

        public bool WriteFloatToOffsets(IEnumerable<long> offsets, float value, string moduleName = c_mainModuleName);

        public bool WriteFloatToOffset(long offset, float value, string moduleName = c_mainModuleName);

        public bool WriteDoubleToOffsets(IEnumerable<long> offsets, double value, string moduleName = c_mainModuleName);

        public bool WriteDoubleToOffset(long offset, double value, string moduleName = c_mainModuleName);

        public bool WriteStringToOffsets(IEnumerable<long> offsets, string value, Encoding encoding, string moduleName = c_mainModuleName);

        public bool WriteStringToOffset(long offset, string value, Encoding encoding, string moduleName = c_mainModuleName);

        public bool WriteLongToOffset(long offset, long value, string moduleName = c_mainModuleName);

        public bool WriteLongToOffsets(IEnumerable<long> offsets, long value, string moduleName = c_mainModuleName);

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

        public IEnumerable<byte> ReadBytesFromAddress(IntPtr address, int numOfBytesToRead);

        public byte ReadByteFromAddress(IntPtr address);

        public int ReadIntFromAddress(IntPtr address);

        public long ReadLongFromAddress(IntPtr address);

        public float ReadFloatFromAddress(IntPtr address);

        public double ReadDoubleFromAddress(IntPtr address);

        public string ReadStringFromAddress(IntPtr address, int numOfCharsToRead, Encoding encoding);

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

        public IntPtr AllocateMemory(int size, MemoryProtection memoryProtection, MemoryAllocationType memoryAllocationType = c_defaultAllocationType);
    }
}
