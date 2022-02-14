using System;
using System.Text;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using System.ComponentModel;
using ProcessMemory.Interfaces;
using Utils;
using static Utils.Conversions;
using static Utils.ExtensionsMethods.IntPtrExtensionMethod;
using static Common.kernel32Methods;
using System.Threading;
using System.Threading.Tasks;
using Common.Enums;

namespace ProcessMemory
{
    public class SingleProcessMemory : ISingleProcessMemory
    {
        #region private properties and fields
        private ITracer m_tracer;

        private Process m_process;

        private bool m_is64Bit;

        private int m_zeroPad = 16;

        private string m_debugFilePath;

        private string m_processName;

        private IDictionary<string, CancellationTokenSource> m_offsetsToCancellationTokenSourceMapping;
        #endregion

        #region constructors

        private SingleProcessMemory()
        {
            m_tracer = Tracer.GetTracer();
            m_offsetsToCancellationTokenSourceMapping = new Dictionary<string, CancellationTokenSource>();
        }

        /// <summary>
        /// Returns an instance of SingleProcessMemory associated with the first process named: <processName>
        /// </summary>
        /// <param name="processName">The process name</param>
        public SingleProcessMemory(string processName) : this() 
        {
            m_process = Process.GetProcessesByName(processName).FirstOrDefault();
            m_processName = processName;
            if (m_process != null) 
            {
                m_is64Bit = IsProcess64Bit();
                m_zeroPad = m_is64Bit ? 16 : 8;
            }
        }

        /// <summary>
        /// Returns an instance of SingleProcessMemory associated with the process id: <processId>
        /// </summary>
        /// <param name="processId">the process Id</param>
        public SingleProcessMemory(int processId) : this()
        {
            m_process = Process.GetProcessById(processId);
            if (m_process == null) 
            {
                throw new ArgumentException($"A process with the pid: {processId} doesn't exist.");
            }
            m_processName = m_process.ProcessName;
            m_is64Bit = IsProcess64Bit();
            m_zeroPad = m_is64Bit ? 16 : 8;
        }

        public SingleProcessMemory(int processId, string debugFilePath, bool createDebugFileOnMiss = true) :this(processId)
        {
            FileStream fileStream = null;
            m_debugFilePath = debugFilePath;
            try
            {
                if (createDebugFileOnMiss)
                {
                    fileStream = File.Open(m_debugFilePath, FileMode.OpenOrCreate);
                }
                else
                {
                    fileStream = File.Open(m_debugFilePath, FileMode.Open);
                }
            }
            catch (Exception exc) 
            {
                m_tracer.TraceError(exc, $"failed to open file at path: {m_debugFilePath}.");
                throw;
            }
            
            m_tracer.AddListener(new TextWriterTraceListener(fileStream, "Debug File Listener"));
        }

        public SingleProcessMemory(string processName, string debugFilePath, bool createDebugFileOnMiss = true): this(processName)
        {
            FileStream fileStream = null;
            m_debugFilePath = debugFilePath;
            try
            {
                if (createDebugFileOnMiss)
                {
                    fileStream = File.Open(m_debugFilePath, FileMode.OpenOrCreate);
                }
                else
                {
                    fileStream = File.Open(m_debugFilePath, FileMode.Open);
                }
            }
            catch (Exception exc)
            {
                m_tracer.TraceError(exc, $"failed to open file at path: {m_debugFilePath}.");
                throw;
            }
            m_tracer.AddListener(new TextWriterTraceListener(fileStream, "Debug File Listener"));
        }

        #endregion

        #region public methods
        public bool WriteBytesToOffset(long offset, IEnumerable<byte> value, string moduleName)
        {
            return WriteBytesToOffsets(new long[] { offset }, value, moduleName);
        }

        public bool WriteByteToOffsets(IEnumerable<long> offsets, byte value, string moduleName)
        {
            return WriteBytesToOffsets(offsets, new byte[] { value }, moduleName);
        }

        public bool WriteByteToOffset(long offset, byte value, string moduleName)
        {
            return WriteBytesToOffsets(new long[] { offset }, new byte[] { value }, moduleName);
        }

        public bool WriteIntToOffsets(IEnumerable<long> offsets, int value, string moduleName)
        {
            return WriteBytesToOffsets(offsets, BitConverter.GetBytes(value), moduleName);
        }

        public bool WriteIntToOffset(long offset, int value, string moduleName)
        {
            return WriteBytesToOffsets(new long[] { offset }, BitConverter.GetBytes(value), moduleName);
        }

        public bool WriteFloatToOffsets(IEnumerable<long> offsets, float value, string moduleName)
        {
            return WriteBytesToOffsets(offsets, BitConverter.GetBytes(value), moduleName);
        }

        public bool WriteFloatToOffset(long offset, float value, string moduleName)
        {
            return WriteBytesToOffsets(new long[] { offset }, BitConverter.GetBytes(value), moduleName);
        }

        public bool WriteDoubleToOffsets(IEnumerable<long> offsets, double value, string moduleName)
        {
            return WriteBytesToOffsets(offsets, BitConverter.GetBytes(value), moduleName);
        }

        public bool WriteDoubleToOffset(long offset, double value, string moduleName)
        {
            return WriteBytesToOffsets(new long[] { offset }, BitConverter.GetBytes(value), moduleName);
        }

        public bool WriteStringToOffsets(IEnumerable<long> offsets, string value, Encoding encoding, string moduleName)
        {
            value += "\0";
            return WriteBytesToOffsets(offsets, encoding.GetBytes(value), moduleName);
        }

        public bool WriteStringToOffset(long offset, string value, Encoding encoding, string moduleName)
        {
            value += "\0";
            return WriteBytesToOffsets(new long[] { offset }, encoding.GetBytes(value), moduleName);
        }

        public bool WriteLongToOffset(long offset, long value, string moduleName)
        {
            return WriteBytesToOffsets(new long[] { offset }, BitConverter.GetBytes(value), moduleName);
        }

        public bool WriteLongToOffsets(IEnumerable<long> offsets, long value, string moduleName)
        {
            return WriteBytesToOffsets(offsets, BitConverter.GetBytes(value), moduleName);
        }

        public bool WriteBytesToAddress(IntPtr address, IEnumerable<byte> value)
        {
            if (IsProcessOpen())
            {
                int numOfBytesToWrite = value.Count();
                if (address.IsPointerValid())
                {
                    bool didWriteSucceeded = WriteProcessMemory(m_process.Handle, address, value.ToArray(), numOfBytesToWrite, out int numOfBytesWritten);
                    if (didWriteSucceeded)
                    {
                        m_tracer.TraceInformation($"Write to address: {address.ToString($"X{m_zeroPad}")} has succeeded. number of bytes needed to write: {numOfBytesToWrite}, number of bytes succeeded to write: {numOfBytesWritten}");
                        return true;
                    }
                    m_tracer.TraceWarning($"Write to address: {address.ToString($"X{m_zeroPad}")} has failed. number of bytes tried to write: {numOfBytesToWrite}. number of bytes succeeded to write: {numOfBytesWritten}");
                }
                else
                {
                    m_tracer.TraceWarning($"address was invalid. address: {address.ToString($"X{m_zeroPad}")}");
                }
            }
            else
            {
                m_tracer.TraceWarning($"Process is not open.");
            }
            return false;
        }

        public bool WriteByteToAddress(IntPtr address, byte value)
        {
            return WriteBytesToAddress(address, new byte[] { value });
        }

        public bool WriteIntToAddress(IntPtr address, int value)
        {
            return WriteBytesToAddress(address, BitConverter.GetBytes(value));
        }

        public bool WriteLongToAddress(IntPtr address, long value)
        {
            return WriteBytesToAddress(address, BitConverter.GetBytes(value));
        }

        public bool WriteFloatToAddress(IntPtr address, float value)
        {
            return WriteBytesToAddress(address, BitConverter.GetBytes(value));
        }

        public bool WriteDoubleToAddress(IntPtr address, double value)
        {
            return WriteBytesToAddress(address, BitConverter.GetBytes(value));
        }

        public bool WriteStringToAddress(IntPtr address, string value, Encoding encoding)
        {
            value += "\0";
            return WriteBytesToAddress(address, encoding.GetBytes(value));
        }

        public bool WriteBytesToOffsets(IEnumerable<long> offsets, IEnumerable<byte> bytesToWrite, string moduleName)
        {
            IntPtr address = GetAddress(offsets, moduleName);
            return WriteBytesToAddress(address, bytesToWrite);
        }

        public IEnumerable<byte> ReadBytesFromOffsets(IEnumerable<long> offsets, int numOfBytesToRead, string moduleName)
        {
            IntPtr address = GetAddress(offsets, moduleName);
            return ReadBytesFromAddress(address, numOfBytesToRead);
        }

        public IEnumerable<byte> ReadBytesFromOffset(long offset, int numOfBytesToRead, string moduleName)
        {
            return ReadBytesFromOffsets(new long[] { offset }, numOfBytesToRead, moduleName);
        }

        public byte ReadByteFromOffsets(IEnumerable<long> offsets, string moduleName)
        {
            IEnumerable<byte> bytes = ReadBytesFromOffsets(offsets, 1, moduleName);
            if (bytes == null)
            {
                return 0x00;
            }
            return bytes.FirstOrDefault();
        }

        public byte ReadByteFromOffset(long offset, string moduleName)
        {
            return ReadByteFromOffsets(new long[] { offset }, moduleName);
        }

        public int ReadIntFromOffset(long offset, string moduleName)
        {
            return ReadIntFromOffsets(new long[] { offset }, moduleName);
        }

        public int ReadIntFromOffsets(IEnumerable<long> offsets, string moduleName)
        {
            IEnumerable<byte> bytes = ReadBytesFromOffsets(offsets, sizeof(int), moduleName);
            if (bytes == null)
            {
                return -1;
            }
            return BitConverter.ToInt32(bytes.ToArray());
        }

        public float ReadFloatFromOffset(long offset, string moduleName)
        {
            return ReadFloatFromOffsets(new long[] { offset }, moduleName);
        }

        public float ReadFloatFromOffsets(IEnumerable<long> offsets, string moduleName)
        {
            IEnumerable<byte> bytes = ReadBytesFromOffsets(offsets, sizeof(float), moduleName);
            if (bytes == null)
            {
                return -1;
            }
            return BitConverter.ToSingle(bytes.ToArray());
        }

        public double ReadDoubleFromOffset(long offset, string moduleName)
        {
            return ReadDoubleFromOffsets(new long[] { offset }, moduleName);
        }

        public double ReadDoubleFromOffsets(IEnumerable<long> offsets, string moduleName)
        {
            IEnumerable<byte> bytes = ReadBytesFromOffsets(offsets, sizeof(double), moduleName);
            if (bytes == null)
            {
                return -1;
            }
            return BitConverter.ToDouble(bytes.ToArray());
        }

        public long ReadLongFromOffset(long offset, string moduleName)
        {
            return ReadLongFromOffsets(new long[] { offset }, moduleName);
        }

        public long ReadLongFromOffsets(IEnumerable<long> offsets, string moduleName)
        {
            IEnumerable<byte> bytes = ReadBytesFromOffsets(offsets, sizeof(long), moduleName);
            if (bytes == null)
            {
                return -1;
            }
            return BitConverter.ToInt64(bytes.ToArray());
        }

        public string ReadStringFromOffset(long offset, int numOfCharsToRead, Encoding encoding, string moduleName)
        {
            return ReadStringFromOffsets(new long[] { offset }, numOfCharsToRead, encoding, moduleName);
        }

        public string ReadStringFromOffsets(IEnumerable<long> offsets, int numOfCharsToRead, Encoding encoding, string moduleName)
        {
            int bytePerChar = encoding.GetByteCount("a");
            IEnumerable<byte> bytes = ReadBytesFromOffsets(offsets, numOfCharsToRead * bytePerChar, moduleName);
            if (bytes == null)
            {
                return null;
            }
            return encoding.GetString(bytes.ToArray());
        }

        public IEnumerable<byte> ReadBytesFromAddress(IntPtr address, int numOfBytesToRead)
        {
            if (IsProcessOpen())
            {
                byte[] bytesRead = new byte[numOfBytesToRead];
                if (ReadProcessMemory(m_process.Handle, address, bytesRead, numOfBytesToRead, out int numOfBytesRead))
                {
                    m_tracer.TraceInformation($"Successfully read: {numOfBytesRead} from address: {address}");
                    return bytesRead;
                }
                m_tracer.TraceWarning($"Failed to read from address: {address.ToString($"X{m_zeroPad}")}, numOfBytesToRead: {numOfBytesToRead}, numOfBytesRead: {numOfBytesRead}");
            }
            else
            {
                m_tracer.TraceWarning($"Process is not open so no read has been done.");
            }
            return null;
        }

        public byte ReadByteFromAddress(IntPtr address)
        {
            IEnumerable<byte> bytes = ReadBytesFromAddress(address, 1);
            if (bytes == null)
            {
                return 0x00;
            }
            return bytes.FirstOrDefault();
        }

        public int ReadIntFromAddress(IntPtr address)
        {
            IEnumerable<byte> bytes = ReadBytesFromAddress(address, sizeof(int));
            if (bytes == null)
            {
                return -1;
            }
            return BitConverter.ToInt32(bytes.ToArray());
        }

        public long ReadLongFromAddress(IntPtr address)
        {
            IEnumerable<byte> bytes = ReadBytesFromAddress(address, sizeof(long));
            if (bytes == null)
            {
                return -1;
            }
            return BitConverter.ToInt64(bytes.ToArray());
        }

        public float ReadFloatFromAddress(IntPtr address)
        {
            IEnumerable<byte> bytes = ReadBytesFromAddress(address, sizeof(float));
            if (bytes == null)
            {
                return -1;
            }
            return BitConverter.ToSingle(bytes.ToArray());
        }

        public double ReadDoubleFromAddress(IntPtr address)
        {
            IEnumerable<byte> bytes = ReadBytesFromAddress(address, sizeof(double));
            if (bytes == null)
            {
                return -1;
            }
            return BitConverter.ToDouble(bytes.ToArray());
        }

        public string ReadStringFromAddress(IntPtr address, int numOfCharsToRead, Encoding encoding)
        {
            int bytePerChar = encoding.GetByteCount("a");
            IEnumerable<byte> bytes = ReadBytesFromAddress(address, numOfCharsToRead * bytePerChar);
            if (bytes == null)
            {
                return null;
            }
            return encoding.GetString(bytes.ToArray());
        }

        public IntPtr AllocateMemory(int size, MemoryProtection memoryProtection, MemoryAllocationType memoryAllocationType = ISingleProcessMemory.c_defaultAllocationType) 
        {
            if (IsProcessOpen())
            {
                if (size > 0)
                {
                    return VirtualAllocEx(m_process.Handle, IntPtr.Zero, size, memoryAllocationType, memoryProtection);
                }
                else 
                {
                    m_tracer.TraceWarning($"Size of memory to allocat must be positive number");
                }
            }
            else 
            {
                m_tracer.TraceInformation($"Process is not open.");
            }
            return IntPtr.Zero;
        }

        public bool IsProcessOpen()
        {
            if (m_process != null && m_process.HasExited == false)
            {
                return true;
            }
            m_process = Process.GetProcessesByName(m_processName).FirstOrDefault();
            if (m_process == null)
            {
                return false;
            }
            m_is64Bit = IsProcess64Bit();
            m_zeroPad = m_is64Bit ? 16 : 8;
            return true;
        }

        public IntPtr GetAddress(IEnumerable<long> offsets, string moduleName) 
        {
            if (IsProcessOpen()) 
            {
                if (m_is64Bit)
                {
                    return Get64BitAddress(offsets, moduleName);
                }
                return Get32BitAddress(offsets, moduleName);
            }
            m_tracer.TraceWarning($"Process is not open.");
            return IntPtr.Zero;
        }
        public IntPtr GetAddress(long offset, string moduleName)
        {
            if (IsProcessOpen())
            {
                ProcessModule module = GetModule(moduleName);
                if (module == null)
                {
                    m_tracer.TraceWarning($"no module named: {moduleName} was loaded by the process named: {m_processName}");
                    return IntPtr.Zero;
                }
                return (IntPtr)(module.BaseAddress.ToInt64() + offset);
            }
            m_tracer.TraceWarning($"Process is not open.");
            return IntPtr.Zero;
        }

        public bool FreezeValue(IEnumerable<long> offsets, float value, string moduleName)
        {
            return FreezeValue(offsets, BitConverter.GetBytes(value), moduleName);
        }

        public bool FreezeValue(long offset, float value, string moduleName)
        {
            return FreezeValue(new long[] { offset }, BitConverter.GetBytes(value), moduleName);
        }

        public bool FreezeValue(IEnumerable<long> offsets, double value, string moduleName)
        {
            return FreezeValue(offsets, BitConverter.GetBytes(value), moduleName);
        }

        public bool FreezeValue(long offset, double value, string moduleName)
        {
            return FreezeValue(new long[] { offset }, BitConverter.GetBytes(value), moduleName);
        }

        public bool FreezeValue(IEnumerable<long> offsets, string value, Encoding encoding, string moduleName)
        {
            return FreezeValue(offsets, encoding.GetBytes(value), moduleName);
        }

        public bool FreezeValue(long offset, string value, Encoding encoding, string moduleName)
        {
            return FreezeValue(new long[] { offset }, encoding.GetBytes(value), moduleName);
        }

        public bool FreezeValue(IEnumerable<long> offsets, int value, string moduleName)
        {
            return FreezeValue(offsets, BitConverter.GetBytes(value), moduleName);
        }

        public bool FreezeValue(long offset, int value, string moduleName)
        {
            return FreezeValue(new long[] { offset }, BitConverter.GetBytes(value), moduleName);
        }

        public bool FreezeValue(IEnumerable<long> offsets, byte value, string moduleName)
        {
            return FreezeValue(offsets, new byte[] { value }, moduleName);
        }

        public bool FreezeValue(long offset, byte value, string moduleName)
        {
            return FreezeValue(new long[] { offset }, new byte[] { value }, moduleName);
        }

        public bool FreezeValue(long offset, long value, string moduleName) 
        {
            return FreezeValue(new long[] { offset }, BitConverter.GetBytes(value), moduleName);
        }

        public bool UnFreezeValue(long offset, string moduleName)
        {
            return UnFreezeValue(new long[] { offset }, moduleName);
        }

        public bool UnFreezeValue(IEnumerable<long> offsets, string moduleName)
        {
            if (IsProcessOpen())
            {
                string key = GetOffsetsAsKey(offsets)+moduleName.ToLower();
                if (m_offsetsToCancellationTokenSourceMapping.ContainsKey(key))
                {
                    if (m_offsetsToCancellationTokenSourceMapping.TryGetValue(key, out CancellationTokenSource cancellationTokenSource))
                    {
                        cancellationTokenSource.Cancel();
                        m_offsetsToCancellationTokenSourceMapping.Remove(key);
                        return true;
                    }
                    m_tracer.TraceError($"Failed to get cancellationTokenSource for offsets: {GetOffsetsAsString(offsets)}");
                }
                else 
                {
                    m_tracer.TraceWarning($"No value is freezed on the address matched by this offsets: {GetOffsetsAsString(offsets)}");
                }
            }
            else 
            {
                m_tracer.TraceWarning($"Process is not open.");
            }
            return false;
        }

        public bool ChangeMemoryProtection(IntPtr address, int size, MemoryProtection memoryProtection, out MemoryProtection oldMemoryProtection)
        {
            if (size <= 0)
            {
                throw new ArgumentException($"Size must be a positive number.");
            }
            if (IsProcessOpen())
            {
                if (address.IsPointerValid())
                {
                    VirtualProtectEx(m_process.Handle, address, size, memoryProtection, out oldMemoryProtection);
                    return true;
                }
                else
                {
                    m_tracer.TraceWarning($"address was invalid. address: {address.ToString($"X{m_zeroPad}")}");
                }
            }
            else 
            {
                m_tracer.TraceWarning($"Process is not open.");
            }
            oldMemoryProtection = default;
            return false;
        }

        public bool ChangeMemoryProtection(long offset, int size, MemoryProtection memoryProtection, out MemoryProtection oldMemoryProtection, string moduleName)
        {
            IntPtr address = GetAddress(offset, moduleName);
            return ChangeMemoryProtection(address, size, memoryProtection, out oldMemoryProtection);
        }

        public bool ChangeMemoryProtection(IEnumerable<long> offsets, int size, MemoryProtection memoryProtection, out MemoryProtection oldMemoryProtection, string moduleName)
        {
            IntPtr address = GetAddress(offsets, moduleName);
            return ChangeMemoryProtection(address, size, memoryProtection, out oldMemoryProtection);
        }

        public void Dispose()
        {
            m_tracer.TraceInformation($"Disposing: {nameof(m_process)}");
            m_process?.Dispose();
        }

        // TODO: Add Write and Read methods for data type Long (Int64) also add it in the interface.

        #endregion

        #region private methods
        private bool IsProcess64Bit() 
        {
            bool isWow64 = false;
            bool isOs64Bit = Environment.Is64BitOperatingSystem;
            if (!IsWow64Process(m_process.Handle, out isWow64))
            {
                m_tracer.TraceWarning($"Failed to found out if the process is wow64. Process Handle: {m_process.Handle}");
                return isOs64Bit; // default is as Operating System.
            }
            return isOs64Bit && !isWow64;
        }

        private IntPtr Get32BitAddress(IEnumerable<long> offsets, string moduleName) 
        {
            int ptrSize = 4;
            byte[] bytesRead = new byte[ptrSize];
            ProcessModule module = GetModule(moduleName);
            if (module == null)
            {
                m_tracer.TraceWarning($"no module named: {moduleName} was loaded by the process named: {m_processName}");
                return IntPtr.Zero;
            }
            IntPtr addressToRead = module.BaseAddress;
            foreach (var offset in offsets.SkipLast(1))
            {
                addressToRead = (IntPtr)(addressToRead.ToInt64() + offset);
                bool isReadSucceeded = ReadProcessMemory(m_process.Handle, addressToRead, bytesRead, ptrSize, out int NumberOfBytesRead);
                if (!isReadSucceeded)
                {
                    m_tracer.TraceWarning($"Failed to read from address: {addressToRead.ToString($"X{m_zeroPad}")}. num of bytes needed to read: {ptrSize}, num of bytes read: {NumberOfBytesRead}");
                    return IntPtr.Zero;
                }
                addressToRead = (IntPtr)BitConverter.ToInt32(bytesRead);
            }
            return (IntPtr)(addressToRead.ToInt64() + offsets.Last());
        }

        private IntPtr Get64BitAddress(IEnumerable<long> offsets, string moduleName)
        {
            int ptrSize = 8;
            byte[] bytesRead = new byte[ptrSize];
            ProcessModule module = GetModule(moduleName);
            if (module == null) 
            {
                m_tracer.TraceWarning($"no module named: {moduleName} was loaded by the process named: {m_processName}");
                return IntPtr.Zero;
            }
            IntPtr addressToRead = module.BaseAddress;
            foreach (var offset in offsets.SkipLast(1))
            {
                addressToRead = (IntPtr)(addressToRead.ToInt64() + offset);
                bool isReadSucceeded = ReadProcessMemory(m_process.Handle, addressToRead, bytesRead, ptrSize, out int NumberOfBytesRead);
                if (!isReadSucceeded)
                {
                    m_tracer.TraceWarning($"Failed to read from address: {addressToRead.ToString($"X{m_zeroPad}")}. num of bytes needed to read: {ptrSize}, num of bytes read: {NumberOfBytesRead}");
                    return IntPtr.Zero;
                }
                addressToRead = (IntPtr)BitConverter.ToInt64(bytesRead);
            }
            return (IntPtr)(addressToRead.ToInt64() + offsets.Last());
        }

        private bool FreezeValue(IEnumerable<long> offsets, byte[] value, string moduleName)
        {
            if (IsProcessOpen())
            {
                string key = GetOffsetsAsKey(offsets) + moduleName.ToLower();
                if (!m_offsetsToCancellationTokenSourceMapping.ContainsKey(key))
                {
                    CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
                    m_offsetsToCancellationTokenSourceMapping.TryAdd(key, cancellationTokenSource);
                    Task.Factory.StartNew(() =>
                    {
                        int failingInARawCount = 0;
                        while (!cancellationTokenSource.IsCancellationRequested)
                        {
                            if (!WriteBytesToOffsets(offsets, value, moduleName))
                            {
                                m_tracer.TraceWarning($"Failed to write value to address at offsets: {GetOffsetsAsString(offsets)}, value: {GetByteArrayAsHexString(value)}");
                                failingInARawCount++;
                            }
                            else
                            {
                                failingInARawCount = 0;
                            }
                            if (failingInARawCount >= 5)
                            {
                                cancellationTokenSource.Cancel();
                            }
                            Thread.Sleep(35);
                        }
                    },
                    cancellationTokenSource.Token);

                    return true;
                }
                else
                {
                    m_tracer.TraceWarning($"Address is already Freezed. please UnFreeze it before try to Freeze It again");
                }
                return false;
            }
            m_tracer.TraceWarning($"Process is not open.");
            return false;
        }

        private ProcessModule GetModule(string moduleName) 
        {
            if (moduleName.Equals(ISingleProcessMemory.c_mainModuleName, StringComparison.OrdinalIgnoreCase)) 
            {
                return m_process.MainModule;
            }
            foreach (ProcessModule module in m_process.Modules) 
            {
                if (module.ModuleName.Equals(moduleName, StringComparison.OrdinalIgnoreCase)) 
                {
                    return module;
                }
            }
            return null;
        }
        #endregion
    }
}
