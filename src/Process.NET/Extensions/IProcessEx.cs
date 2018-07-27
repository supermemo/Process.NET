using System;
using Process.NET.Memory;

namespace Process.NET.Extensions
{
  public static class IProcessEx
  {
    #region Methods

    public static T Read<T>(this IMemory memory,
                            ObjPtr       objPtr,
                            int          offset = 0)
    {
      int instanceAddr = objPtr.GetInstanceAddress(memory);

      return memory.Read<T>(new IntPtr(instanceAddr + objPtr.Offset + offset));
    }

    #endregion
  }
}
