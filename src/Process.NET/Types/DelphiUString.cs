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
// Created On:   2018/11/25 16:38
// Modified On:  2018/11/25 16:44
// Modified By:  Alexis

#endregion




using System;
using System.Text;
using Process.NET.Marshaling;
using Process.NET.Memory;

namespace Process.NET.Types
{
  // http://docwiki.embarcadero.com/RADStudio/Tokyo/en/String_Types_(Delphi)
  public class DelphiUString : IMarshallableValue
  {
    #region Constants & Statics

    private const int LeftPadding = 16;

    #endregion




    #region Constructors

    public DelphiUString(string text)
    {
      Text = text;
    }

    #endregion




    #region Properties & Fields - Public

    public string Text { get; set; }

    #endregion




    #region Methods Impl

    /// <inheritdoc />
    public int GetSize() => Encoding.Unicode.GetByteCount(Text + '\0') + LeftPadding;

    /// <inheritdoc />
    public void Write(IAllocatedMemory memory)
    {
      // Base of allocated chunk ?
      memory.Write(0,
                   memory.BaseAddress.ToInt32());

      // Code page
      memory.Write(4,
                   (short)0x04B0);

      // Bytes per character
      memory.Write(6,
                   (short)2);

      // Reference count. Set counter to prevent garbage cleaning.
      memory.Write(8,
                   1);

      // Text length
      memory.Write(12,
                   Text.Length);

      // Write text
      memory.Write(LeftPadding,
                   Text,
                   Encoding.Unicode);
    }

    /// <inheritdoc />
    public IntPtr GetReference(IntPtr baseAddr)
    {
      return baseAddr + LeftPadding;
    }

    #endregion
  }
}
