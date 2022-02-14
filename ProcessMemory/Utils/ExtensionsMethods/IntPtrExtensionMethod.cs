using System;

namespace ProcessMemory.Utils.ExtensionsMethods
{
    public static class IntPtrExtensionMethod
    {
        // modern OS doesn't load normal programs to address below 64kb.
        private static ulong MinValidAddress = 0x10000;

        ///<summary>
        ///Check if a pointer is holding a valid address to write to.
        ///</summary>
        ///<param name="ptr">address to be checked</param>
        public static bool IsPointerValid(this IntPtr ptr)
        {
            return (ulong)ptr >= MinValidAddress;
        }
    }
}
