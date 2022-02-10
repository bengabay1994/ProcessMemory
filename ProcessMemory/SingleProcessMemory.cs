﻿using System;
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
        public bool WriteBytesToOffset(long offset, IEnumerable<byte> bytesToWrite)
        {
            return WriteBytesToOffsets(new long[] { offset }, bytesToWrite);
        }

        public bool WriteByteToOffsets(IEnumerable<long> offsets, byte byteToWrite)
        {
            return WriteBytesToOffsets(offsets, new byte[] { byteToWrite });
        }

        public bool WriteByteToOffset(long offset, byte byteToWrite)
        {
            return WriteBytesToOffsets(new long[] { offset }, new byte[] { byteToWrite });
        }

        public bool WriteIntToOffsets(IEnumerable<long> offsets, int numToWrite)
        {
            return WriteBytesToOffsets(offsets, BitConverter.GetBytes(numToWrite));
        }

        public bool WriteIntToOffset(long offset, int numToWrite)
        {
            return WriteBytesToOffsets(new long[] { offset }, BitConverter.GetBytes(numToWrite));
        }

        public bool WriteFloatToOffsets(IEnumerable<long> offsets, float numToWrite)
        {
            return WriteBytesToOffsets(offsets, BitConverter.GetBytes(numToWrite));
        }

        public bool WriteFloatToOffset(long offset, float numToWrite)
        {
            return WriteBytesToOffsets(new long[] { offset }, BitConverter.GetBytes(numToWrite));
        }

        public bool WriteDoubleToOffsets(IEnumerable<long> offsets, double numToWrite)
        {
            return WriteBytesToOffsets(offsets, BitConverter.GetBytes(numToWrite));
        }

        public bool WriteDoubleToOffset(long offset, double numToWrite)
        {
            return WriteBytesToOffsets(new long[] { offset }, BitConverter.GetBytes(numToWrite));
        }

        public bool WriteStringToOffsets(IEnumerable<long> offsets, string stringToWrite, Encoding encoding)
        {
            stringToWrite += "\0";
            return WriteBytesToOffsets(offsets, encoding.GetBytes(stringToWrite));
        }

        public bool WriteStringToOffset(long offset, string stringToWrite, Encoding encoding)
        {
            stringToWrite += "\0";
            return WriteBytesToOffsets(new long[] { offset }, encoding.GetBytes(stringToWrite));
        }

        public bool WriteBytesToOffsets(IEnumerable<long> offsets, IEnumerable<byte> bytesToWrite)
        {
            if (IsProcessOpen())
            {
                int numOfBytesToWrite = bytesToWrite.Count();
                IntPtr address = GetAddress(offsets);
                if (address.IsPointerValid())
                {
                    bool didWriteSucceeded = WriteProcessMemory(m_process.Handle, address, bytesToWrite.ToArray(), numOfBytesToWrite, out int numOfBytesWritten);
                    if (didWriteSucceeded)
                    {
                        m_tracer.TraceInformation($"Write to address: {address.ToString($"X{m_zeroPad}")} has succeeded. number of bytes needed to write: {numOfBytesToWrite}, number of bytes succeeded to write: {numOfBytesWritten}");
                        return true;
                    }
                    m_tracer.TraceWarning($"Write to address: {address.ToString($"X{m_zeroPad}")} has failed. number of bytes tried to write: {numOfBytesToWrite}. number of bytes succeeded to write: {numOfBytesWritten}");
                }
                else
                {
                    m_tracer.TraceWarning($"address read from offsets: {GetOffsetsAsString(offsets)} was invalid. address: {address.ToString($"X{m_zeroPad}")}");
                }
            }
            else
            {
                m_tracer.TraceWarning($"Process is not open so no write has been done.");
            }
            return false;
        }

        public IEnumerable<byte> ReadBytesFromOffsets(IEnumerable<long> offsets, int numOfBytesToRead)
        {
            if (IsProcessOpen())
            {
                IntPtr address = GetAddress(offsets);
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

        public IEnumerable<byte> ReadBytesFromOffset(long offset, int numOfBytesToRead)
        {
            return ReadBytesFromOffsets(new long[] { offset }, numOfBytesToRead);
        }

        public byte ReadByteFromOffsets(IEnumerable<long> offsets)
        {
            IEnumerable<byte> bytes = ReadBytesFromOffsets(offsets, 1);
            if (bytes == null)
            {
                return 0x00;
            }
            return bytes.FirstOrDefault();
        }

        public byte ReadByteFromOffset(long offset)
        {
            return ReadByteFromOffsets(new long[] { offset });
        }

        public int ReadIntFromOffset(long offset)
        {
            return ReadIntFromOffsets(new long[] { offset });
        }

        public int ReadIntFromOffsets(IEnumerable<long> offsets)
        {
            IEnumerable<byte> bytes = ReadBytesFromOffsets(offsets, sizeof(int));
            if (bytes == null)
            {
                return -1;
            }
            return BitConverter.ToInt32(bytes.ToArray());
        }

        public float ReadFloatFromOffset(long offset)
        {
            return ReadFloatFromOffsets(new long[] { offset });
        }

        public float ReadFloatFromOffsets(IEnumerable<long> offsets)
        {
            IEnumerable<byte> bytes = ReadBytesFromOffsets(offsets, sizeof(float));
            if (bytes == null)
            {
                return -1;
            }
            return BitConverter.ToSingle(bytes.ToArray());
        }

        public double ReadDoubleFromOffset(long offset)
        {
            return ReadDoubleFromOffsets(new long[] { offset });
        }

        public double ReadDoubleFromOffsets(IEnumerable<long> offsets)
        {
            IEnumerable<byte> bytes = ReadBytesFromOffsets(offsets, sizeof(double));
            if (bytes == null)
            {
                return -1;
            }
            return BitConverter.ToDouble(bytes.ToArray());
        }

        public string ReadStringFromOffset(long offset, int numOfCharsToRead, Encoding encoding)
        {
            return ReadStringFromOffsets(new long[] { offset }, numOfCharsToRead, encoding);
        }

        public string ReadStringFromOffsets(IEnumerable<long> offsets, int numOfCharsToRead, Encoding encoding)
        {
            IEnumerable<byte> bytes = ReadBytesFromOffsets(offsets, numOfCharsToRead);
            if (bytes == null)
            {
                return null;
            }
            return encoding.GetString(bytes.ToArray());
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

        public IntPtr GetAddress(IEnumerable<long> offsets) 
        {
            if (IsProcessOpen()) 
            {
                if (m_is64Bit)
                {
                    return Get64BitAddress(offsets);
                }
                return Get32BitAddress(offsets);
            }
            m_tracer.TraceWarning($"Process is not open.");
            return IntPtr.Zero;
        }
        public IntPtr GetAddress(long offset)
        {
            if (IsProcessOpen())
            {
                return (IntPtr)(m_process.MainModule.BaseAddress.ToInt64() + offset);
            }
            m_tracer.TraceWarning($"Process is not open.");
            return IntPtr.Zero;
        }

        public bool FreezeValue(IEnumerable<long> offsets, float value)
        {
            return FreezeValue(offsets, BitConverter.GetBytes(value));
        }

        public bool FreezeValue(long offset, float value)
        {
            return FreezeValue(new long[] { offset }, BitConverter.GetBytes(value));
        }

        public bool FreezeValue(IEnumerable<long> offsets, double value)
        {
            return FreezeValue(offsets, BitConverter.GetBytes(value));
        }

        public bool FreezeValue(long offset, double value)
        {
            return FreezeValue(new long[] { offset }, BitConverter.GetBytes(value));
        }

        public bool FreezeValue(IEnumerable<long> offsets, string value, Encoding encoding)
        {
            return FreezeValue(offsets, encoding.GetBytes(value));
        }

        public bool FreezeValue(long offset, string value, Encoding encoding)
        {
            return FreezeValue(new long[] { offset }, encoding.GetBytes(value));
        }

        public bool FreezeValue(IEnumerable<long> offsets, int value)
        {
            return FreezeValue(offsets, BitConverter.GetBytes(value));
        }

        public bool FreezeValue(long offset, int value)
        {
            return FreezeValue(new long[] { offset }, BitConverter.GetBytes(value));
        }

        public bool FreezeValue(IEnumerable<long> offsets, byte value)
        {
            return FreezeValue(offsets, new byte[] { value });
        }

        public bool FreezeValue(long offset, byte value)
        {
            return FreezeValue(new long[] { offset }, new byte[] { value });
        }

        public bool UnFreezeValue(long offset)
        {
            return UnFreezeValue(new long[] { offset });
        }

        public bool UnFreezeValue(IEnumerable<long> offsets)
        {
            if (IsProcessOpen())
            {
                string key = GetOffsetsAsKey(offsets);
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

        public bool ChangeMemoryProtection(long offset, int size, MemoryProtection memoryProtection, out MemoryProtection oldMemoryProtection)
        {
            IntPtr address = GetAddress(offset);
            return ChangeMemoryProtection(address, size, memoryProtection, out oldMemoryProtection);
        }

        public bool ChangeMemoryProtection(IEnumerable<long> offsets, int size, MemoryProtection memoryProtection, out MemoryProtection oldMemoryProtection)
        {
            IntPtr address = GetAddress(offsets);
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

        private IntPtr Get32BitAddress(IEnumerable<long> offsets) 
        {
            int ptrSize = 4;
            byte[] bytesRead = new byte[ptrSize];
            IntPtr addressToRead = m_process.MainModule.BaseAddress;
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

        private IntPtr Get64BitAddress(IEnumerable<long> offsets)
        {
            int ptrSize = 8;
            byte[] bytesRead = new byte[ptrSize];
            IntPtr addressToRead = m_process.MainModule.BaseAddress;
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

        private bool FreezeValue(IEnumerable<long> offsets, byte[] value)
        {
            if (IsProcessOpen())
            {
                string key = GetOffsetsAsKey(offsets);
                if (!m_offsetsToCancellationTokenSourceMapping.ContainsKey(key))
                {
                    CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
                    m_offsetsToCancellationTokenSourceMapping.TryAdd(key, cancellationTokenSource);
                    Task.Factory.StartNew(() =>
                    {
                        int failingInARawCount = 0;
                        while (true)
                        {
                            if (!WriteBytesToOffsets(offsets, value))
                            {
                                m_tracer.TraceWarning($"Failed to write float value to address at offsets: {GetOffsetsAsString(offsets)}, value: {value}");
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
        #endregion
    }
}