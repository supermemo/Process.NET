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
// Created On:   2018/06/08 14:35
// Modified On:  2018/12/22 15:22
// Modified By:  Alexis

#endregion




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
      int instanceAddr = objPtr.ReadInstanceAddress(memory);

      return memory.Read<T>(new IntPtr(instanceAddr + objPtr.Offset + offset));
    }

    public static void Write<T>(this IMemory memory,
                                ObjPtr       objPtr,
                                T            value,
                                int          offset = 0)
    {
      int instanceAddr = objPtr.ReadInstanceAddress(memory);

      memory.Write<T>(new IntPtr(instanceAddr + objPtr.Offset + offset),
                      value);
    }

    #endregion
  }
}
