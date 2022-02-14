namespace ProcessMemory.Enums
{
    // To find more details about the constants please visit: https://docs.microsoft.com/en-us/windows/win32/api/memoryapi/nf-memoryapi-virtualallocex
    public enum MemoryAllocationType
    {
        MemCommit = 0x1000,
        MemReserve = 0x2000,
        MemReset = 0x80000,
        MemTopDown = 0x100000,
        MemResetUndo = 0x1000000,
        MemLargePages = 0x20000000,
        MemPhysical = 0x00400000,
    }
}
