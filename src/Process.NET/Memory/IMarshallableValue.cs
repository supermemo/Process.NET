using System;
using System.Collections.Generic;
using System.Text;
using Process.NET.Memory;

namespace Process.NET.Marshaling
{
  public interface IMarshallableValue
  {
    int GetSize();
    void Write(IAllocatedMemory memory);
    IntPtr GetReference(IntPtr baseAddr);
  }
}
