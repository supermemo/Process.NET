#region License & Metadata

// The MIT License (MIT)
// 
// Permission is hereby granted, free of charge, to any person obtaining a
// copy of this software and associated documentation files (the "Software"),
// to deal in the Software without restriction, including without limitation
// the rights to use, copy, modify, merge, publish, distribute, sublicense,
// and/or sell copies of the Software, and to permit persons to whom the 
// Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
// DEALINGS IN THE SOFTWARE.
// 
// 
// Created On:   2018/11/25 13:18
// Modified On:  2018/11/25 13:28
// Modified By:  Alexis

#endregion




using System;
using Process.NET.Extensions;

namespace Process.NET.Memory
{
  public class OffsetPtr
  {
    #region Constructors

    public OffsetPtr(IntPtr addrPtr,
                     int    offset = 0)
    {
      AddrPtr = addrPtr;
      Offset  = offset;
    }

    public OffsetPtr(ObjPtr objPtr,
                     int    offset = 0)
    {
      ObjectPtr = objPtr;
      Offset    = offset;
    }

    #endregion




    #region Properties & Fields - Public

    public IntPtr AddrPtr   { get; }
    public ObjPtr ObjectPtr { get; }
    public int    Offset    { get; }

    #endregion




    #region Methods

    public T Read<T>(IMemory memory,
                     int     secondOffset = 0)
    {
      IntPtr baseAddr = ObjectPtr != null
        ? new IntPtr(memory.Read<int>(ObjectPtr))
        : AddrPtr;

      return memory.Read<T>(baseAddr + Offset);
    }

    #endregion
  }
}
