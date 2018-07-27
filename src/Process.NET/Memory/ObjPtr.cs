using System;
using Process.NET.Extensions;

namespace Process.NET.Memory
{
  public class ObjPtr
  {
    #region Constructors

    public ObjPtr(IntPtr addrPtr,
                  int    offset = 0)
    {
      AddrPtr = addrPtr;
      Offset  = offset;
    }

    public ObjPtr(ObjPtr objPtr,
                  int    offset = 0)
    {
      ObjectPtr = objPtr;
      Offset = offset;
    }

    #endregion




    #region Properties & Fields - Public

    public IntPtr AddrPtr { get; }
    public ObjPtr ObjectPtr  { get; }
    public int    Offset  { get; }

    #endregion




    #region Methods

    public int GetInstanceAddress(IMemory memory)
    {
      return ObjectPtr != null
        ? memory.Read<int>(ObjectPtr)
        : memory.Read<int>(AddrPtr);
    }

    #endregion
  }
}
