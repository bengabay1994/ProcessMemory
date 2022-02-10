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
        public bool WriteBytesToOffsets(IEnumerable<long> offsets, IEnumerable<byte> bytesToWrite);

        public bool WriteBytesToOffset(long offset, IEnumerable<byte> bytesToWrite);

        public bool WriteByteToOffsets(IEnumerable<long> offsets, byte byteToWrite);

        public bool WriteByteToOffset(long offset, byte byteToWrite);

        public bool WriteIntToOffsets(IEnumerable<long> offsets, int numToWrite);

        public bool WriteIntToOffset(long offset, int numToWrite);

        public bool WriteFloatToOffsets(IEnumerable<long> offsets, float numToWrite);

        public bool WriteFloatToOffset(long offset, float numToWrite);

        public bool WriteDoubleToOffsets(IEnumerable<long> offsets, double numToWrite);

        public bool WriteDoubleToOffset(long offset, double numToWrite);

        public bool WriteStringToOffsets(IEnumerable<long> offsets, string stringToWrite, Encoding encoding);

        public bool WriteStringToOffset(long offset, string stringToWrite, Encoding encoding);

        public IEnumerable<byte> ReadBytesFromOffsets(IEnumerable<long> offsets, int numOfBytesToRead);

        public IEnumerable<byte> ReadBytesFromOffset(long offset, int numOfBytesToRead);

        public byte ReadByteFromOffsets(IEnumerable<long> offsets);

        public byte ReadByteFromOffset(long offset);

        public int ReadIntFromOffset(long offset);

        public int ReadIntFromOffsets(IEnumerable<long> offsets);

        public float ReadFloatFromOffset(long offset);

        public float ReadFloatFromOffsets(IEnumerable<long> offsets);

        public double ReadDoubleFromOffset(long offset);

        public double ReadDoubleFromOffsets(IEnumerable<long> offsets);

        public string ReadStringFromOffset(long offset, int numOfCharsToRead, Encoding encoding);

        public string ReadStringFromOffsets(IEnumerable<long> offsets, int numOfCharsToRead, Encoding encoding);

        public bool FreezeValue(IEnumerable<long> offsets, float value);

        public bool FreezeValue(long offset, float value);

        public bool FreezeValue(IEnumerable<long> offsets, double value);

        public bool FreezeValue(long offset, double value);

        public bool FreezeValue(IEnumerable<long> offsets, string value, Encoding encoding);

        public bool FreezeValue(long offset, string value, Encoding encoding);

        public bool FreezeValue(IEnumerable<long> offsets, int value);

        public bool FreezeValue(long offset, int value);

        public bool FreezeValue(IEnumerable<long> offsets, byte value);

        public bool FreezeValue(long offset, byte value);

        public bool UnFreezeValue(long offset);

        public bool UnFreezeValue(IEnumerable<long> offsets);

        public bool IsProcessOpen();

        public bool ChangeMemoryProtection(IntPtr address, int size, MemoryProtection memoryProtection, out MemoryProtection oldMemoryProtection);

        public bool ChangeMemoryProtection(long offset, int size, MemoryProtection memoryProtection, out MemoryProtection oldMemoryProtection);

        public bool ChangeMemoryProtection(IEnumerable<long> offsets, int size, MemoryProtection memoryProtection, out MemoryProtection oldMemoryProtection);

        public IntPtr GetAddress(IEnumerable<long> offsets);

        public IntPtr GetAddress(long offset);
    }
}
