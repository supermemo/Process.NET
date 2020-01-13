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
// Created On:   2018/06/05 16:02
// Modified On:  2018/12/09 15:59
// Modified By:  Alexis

#endregion




using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using Process.NET.Native.Types;

namespace Process.NET.Memory
{
  /// <summary>Class representing a pointer in the memory of the remote process.</summary>
  public class MemoryPointer : IEquatable<MemoryPointer>, IPointer
  {
    #region Properties & Fields - Non-Public

    private byte[]                   LastValue            { get; set; }
    private int                      ValueChangedOffset   { get; set; }
    private Timer                    ValueChangedTimer    { get; set; }
    private Mutex                    ValueChangedMutex    { get; set; } = new Mutex();
    private List<Func<byte[], bool>> ValueChangedHandlers { get; }      = new List<Func<byte[], bool>>();

    private int Frequency { get; set; } = int.MaxValue;

    #endregion




    #region Constructors

    /// <summary>Initializes a new instance of the <see cref="MemoryPointer" /> class.</summary>
    /// <param name="process">The reference of the <see cref="IProcess"></see></param>
    /// <param name="address">The location where the pointer points in the remote process.</param>
    public MemoryPointer(IProcess process,
                         IntPtr   address)
    {
      // Save the parameters
      Process     = process;
      BaseAddress = address;
    }

    /// <summary>Initializes a new instance of the <see cref="MemoryPointer" /> class.</summary>
    /// <param name="process">The reference of the <see cref="IProcess"></see></param>
    /// <param name="objPtr">A pointer to the location where the pointer points in the remote process.</param>
    public MemoryPointer(IProcess process,
                         ObjPtr   objPtr)
    {
      // Save the parameters
      Process     = process;
      BaseAddress = new IntPtr(objPtr.ReadInstanceAddress(process.Memory) + objPtr.Offset);
    }


    /// <inheritdoc />
    public virtual void Dispose()
    {
      lock (ValueChangedHandlers)
      {
        ValueChangedHandlers.Clear();

        ValueChangedTimer?.Dispose();
      }
    }

    #endregion




    #region Properties & Fields - Public

    /// <summary>The reference of the <see cref="IMemory" /> object.</summary>
    public IProcess Process { get; }

    #endregion




    #region Properties Impl - Public

    /// <summary>The address of the pointer in the remote process.</summary>
    public IntPtr BaseAddress { get; set; }

    /// <summary>Gets if the <see cref="MemoryPointer" /> is valid.</summary>
    public virtual bool IsValid => BaseAddress != IntPtr.Zero;

    public bool ValueChangedSuspended { get; private set; }

    #endregion




    #region Methods Impl

    /// <summary>Returns a string that represents the current object.</summary>
    public override string ToString()
    {
      return $"BaseAddress = 0x{BaseAddress.ToInt64():X}";
    }

    /// <summary>Determines whether the specified object is equal to the current object.</summary>
    public override bool Equals(object obj)
    {
      if (ReferenceEquals(null,
                          obj)) return false;
      if (ReferenceEquals(this,
                          obj)) return true;

      return obj.GetType() == GetType() && Equals((MemoryPointer)obj);
    }

    /// <summary>Serves as a hash function for a particular type.</summary>
    public override int GetHashCode()
    {
      // ReSharper disable once NonReadonlyMemberInGetHashCode
      return BaseAddress.GetHashCode() ^ Process.GetHashCode();
    }

    /// <summary>Returns a value indicating whether this instance is equal to a specified object.</summary>
    public bool Equals(MemoryPointer other)
    {
      if (ReferenceEquals(null,
                          other)) return false;

      return ReferenceEquals(this,
                             other) ||
        BaseAddress.Equals(other.BaseAddress) && Process.Equals(other.Process);
    }

    /// <summary>Reads the value of a specified length in the remote process.</summary>
    /// <param name="length"></param>
    /// <param name="offset"></param>
    /// <returns></returns>
    public byte[] Read(int length,
                       int offset)
    {
      return Process.Memory.Read(BaseAddress + offset,
                                 length);
    }

    /// <summary>Reads the value of a specified type in the remote process.</summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="offset">The offset where the value is read from the pointer.</param>
    /// <returns>A value.</returns>
    public T Read<T>(int offset)
    {
      return Process.Memory.Read<T>(BaseAddress + offset);
    }

    /// <summary>Reads an array of a specified type in the remote process.</summary>
    /// <typeparam name="T">The type of the values.</typeparam>
    /// <param name="offset">The offset where the values is read from the pointer.</param>
    /// <param name="count">The number of cells in the array.</param>
    /// <returns>An array.</returns>
    public T[] Read<T>(int offset,
                       int count)
    {
      return Process.Memory.Read<T>(BaseAddress + offset,
                                    count);
    }

    /// <summary>Write the byte value in the remote process.</summary>
    /// <param name="offset"></param>
    /// <param name="toWrite"></param>
    /// <returns></returns>
    public int Write(int    offset,
                     byte[] toWrite)
    {
      return Process.Memory.Write(BaseAddress + offset,
                                  toWrite);
    }

    /// <summary>Reads a string with a specified encoding in the remote process.</summary>
    /// <param name="encoding">The encoding used.</param>
    /// <param name="maxLength">
    ///   [Optional] The number of maximum bytes to read. The string is
    ///   automatically cropped at this end ('\0' char).
    /// </param>
    /// <param name="offset">The offset where the string is read from the pointer.</param>
    /// <returns>The string.</returns>
    public string Read(Encoding encoding,
                       int      maxLength,
                       int      offset)
    {
      return Process.Memory.Read(BaseAddress + offset,
                                 encoding,
                                 maxLength);
    }

    /// <summary>Writes the values of a specified type in the remote process.</summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="offset">The offset where the value is written from the pointer.</param>
    /// <param name="value">The value to write.</param>
    public void Write<T>(int offset,
                         T   value)
    {
      Process.Memory.Write(BaseAddress + offset,
                           value);
    }

    /// <summary>Writes an array of a specified type in the remote process.</summary>
    /// <typeparam name="T">The type of the values.</typeparam>
    /// <param name="offset">The offset where the values is written from the pointer.</param>
    /// <param name="array">The array to write.</param>
    public void Write<T>(int offset,
                         T[] array)
    {
      Process.Memory.Write(BaseAddress + offset,
                           array);
    }

    /// <summary>Writes a string with a specified encoding in the remote process.</summary>
    /// <param name="offset">The offset where the string is written from the pointer.</param>
    /// <param name="text">The text to write.</param>
    /// <param name="encoding">The encoding used.</param>
    public void Write(int      offset,
                      string   text,
                      Encoding encoding)
    {
      Process.Memory.Write(BaseAddress + offset,
                           text,
                           encoding);
    }

    public void RegisterValueChangedEventHandler<T>(Func<byte[], bool> eventHandler,
                                                    int                offset      = 0,
                                                    int                msFrequency = 200)
    {
      RegisterValueChangedEventHandler(eventHandler,
                                       Marshal.SizeOf(typeof(T)),
                                       offset,
                                       msFrequency);
    }

    public void RegisterValueChangedEventHandler(Func<byte[], bool> eventHandler,
                                                 int                size,
                                                 int                offset      = 0,
                                                 int                frequencyMs = 200)
    {
      lock (ValueChangedHandlers)
      {
        Frequency = Math.Min(frequencyMs,
                             Frequency);

        if (ValueChangedTimer == null)
        {
          ValueChangedOffset = offset;
          LastValue = Read(size,
                           ValueChangedOffset);
          ValueChangedTimer = new Timer(CheckValueChanged,
                                        null,
                                        frequencyMs,
                                        Timeout.Infinite);
        }

        ValueChangedHandlers.Add(eventHandler);
      }
    }

    public void UnregisterValueChangedEventHandler(Func<byte[], bool> eventHandler)
    {
      lock (ValueChangedHandlers)
      {
        ValueChangedHandlers.Remove(eventHandler);

        if (ValueChangedHandlers.Count == 0)
        {
          StopTimer();
          LastValue = null;
        }
      }
    }

    public bool SuspendTimer()
    {
      ValueChangedMutex.WaitOne();

      try
      {
        if (ValueChangedSuspended)
          return false;

        ValueChangedTimer.Change(Timeout.Infinite,
                                 Timeout.Infinite);
        ValueChangedSuspended = true;

        return true;
      }
      finally
      {
        ValueChangedMutex.ReleaseMutex();
      }
    }

    public bool RestartTimer(bool updateValue = false)
    {
      if (ValueChangedTimer == null)
        return false;

      if (updateValue)
        LastValue = Read(LastValue.Length,
                         ValueChangedOffset);

      ValueChangedTimer.Change(Frequency,
                               Timeout.Infinite);

      bool ret = ValueChangedSuspended;
      ValueChangedSuspended = false;

      return ret;
    }

    #endregion




    #region Methods

    private void StopTimer()
    {
      ValueChangedTimer?.Dispose();
      ValueChangedTimer = null;

      Frequency = int.MaxValue;
    }

    /// <summary>Changes the protection of the n next bytes in remote process.</summary>
    /// <param name="handle"></param>
    /// <param name="size">The size of the memory to change.</param>
    /// <param name="protection">The new protection to apply.</param>
    /// <param name="mustBeDisposed">
    ///   The resource will be automatically disposed when the finalizer
    ///   collects the object.
    /// </param>
    /// <returns>A new instance of the <see cref="MemoryProtection" /> class.</returns>
    public MemoryProtection ChangeProtection(SafeMemoryHandle      handle,
                                             int                   size,
                                             MemoryProtectionFlags protection     = MemoryProtectionFlags.ExecuteReadWrite,
                                             bool                  mustBeDisposed = true)
    {
      return new MemoryProtection(handle,
                                  BaseAddress,
                                  size,
                                  protection,
                                  mustBeDisposed);
    }

    /// <summary>Reads the value of a specified type in the remote process.</summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="offset">The offset where the value is read from the pointer.</param>
    /// <returns>A value.</returns>
    public T Read<T>(Enum offset)
    {
      return Read<T>(Convert.ToInt32(offset));
    }

    /// <summary>Reads the value of a specified type in the remote process.</summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <returns>A value.</returns>
    public T Read<T>()
    {
      return Read<T>(0);
    }

    /// <summary>Reads an array of a specified type in the remote process.</summary>
    /// <typeparam name="T">The type of the values.</typeparam>
    /// <param name="offset">The offset where the values is read from the pointer.</param>
    /// <param name="count">The number of cells in the array.</param>
    /// <returns>An array.</returns>
    public T[] Read<T>(Enum offset,
                       int  count)
    {
      return Read<T>(Convert.ToInt32(offset),
                     count);
    }

    /// <summary>Reads a string with a specified encoding in the remote process.</summary>
    /// <param name="offset">The offset where the string is read from the pointer.</param>
    /// <param name="encoding">The encoding used.</param>
    /// <param name="maxLength">
    ///   [Optional] The number of maximum bytes to read. The string is
    ///   automatically cropped at this end ('\0' char).
    /// </param>
    /// <returns>The string.</returns>
    public string Read(Enum     offset,
                       Encoding encoding,
                       int      maxLength = 512)
    {
      return Read(encoding,
                  maxLength,
                  Convert.ToInt32(offset));
    }

    /// <summary>Reads a string with a specified encoding in the remote process.</summary>
    /// <param name="encoding">The encoding used.</param>
    /// <param name="maxLength">
    ///   [Optional] The number of maximum bytes to read. The string is
    ///   automatically cropped at this end ('\0' char).
    /// </param>
    /// <returns>The string.</returns>
    public string Read(Encoding encoding,
                       int      maxLength = 512)
    {
      return Read(encoding,
                  maxLength,
                  0);
    }

    /// <summary>Reads a string using the encoding UTF8 in the remote process.</summary>
    /// <param name="offset">The offset where the string is read from the pointer.</param>
    /// <param name="maxLength">
    ///   [Optional] The number of maximum bytes to read. The string is
    ///   automatically cropped at this end ('\0' char).
    /// </param>
    /// <param name="encoding"></param>
    /// <returns>The string.</returns>
    public string Read(int      offset,
                       int      maxLength,
                       Encoding encoding)
    {
      return Process.Memory.Read(BaseAddress + offset,
                                 encoding,
                                 maxLength);
    }

    /// <summary>Reads a string using the encoding UTF8 in the remote process.</summary>
    /// <param name="offset">The offset where the string is read from the pointer.</param>
    /// <param name="maxLength">
    ///   [Optional] The number of maximum bytes to read. The string is
    ///   automatically cropped at this end ('\0' char).
    /// </param>
    /// <returns>The string.</returns>
    public string Read(Enum offset,
                       int  maxLength = 512)
    {
      return Read(Convert.ToInt32(offset),
                  maxLength,
                  Encoding.UTF8);
    }

    /// <summary>Writes the values of a specified type in the remote process.</summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="offset">The offset where the value is written from the pointer.</param>
    /// <param name="value">The value to write.</param>
    public void Write<T>(Enum offset,
                         T    value)
    {
      Write(Convert.ToInt32(offset),
            value);
    }

    /// <summary>Writes the values of a specified type in the remote process.</summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="value">The value to write.</param>
    public void Write<T>(T value)
    {
      Write(0,
            value);
    }

    /// <summary>Writes an array of a specified type in the remote process.</summary>
    /// <typeparam name="T">The type of the values.</typeparam>
    /// <param name="offset">The offset where the values is written from the pointer.</param>
    /// <param name="array">The array to write.</param>
    public void Write<T>(Enum offset,
                         T[]  array)
    {
      Write(Convert.ToInt32(offset),
            array);
    }

    /// <summary>Writes an array of a specified type in the remote process.</summary>
    /// <typeparam name="T">The type of the values.</typeparam>
    /// <param name="array">The array to write.</param>
    public void Write<T>(T[] array)
    {
      Write(0,
            array);
    }

    /// <summary>Writes a string with a specified encoding in the remote process.</summary>
    /// <param name="offset">The offset where the string is written from the pointer.</param>
    /// <param name="text">The text to write.</param>
    /// <param name="encoding">The encoding used.</param>
    public void Write(Enum     offset,
                      string   text,
                      Encoding encoding)
    {
      Write(Convert.ToInt32(offset),
            text,
            encoding);
    }

    /// <summary>Writes a string with a specified encoding in the remote process.</summary>
    /// <param name="text">The text to write.</param>
    /// <param name="encoding">The encoding used.</param>
    public void Write(string   text,
                      Encoding encoding)
    {
      Write(0,
            text,
            encoding);
    }

    /// <summary>Writes a string using the encoding UTF8 in the remote process.</summary>
    /// <param name="offset">The offset where the string is written from the pointer.</param>
    /// <param name="text">The text to write.</param>
    public void Write(int    offset,
                      string text)
    {
      Process.Memory.Write(BaseAddress + offset,
                           text);
    }

    /// <summary>Writes a string using the encoding UTF8 in the remote process.</summary>
    /// <param name="offset">The offset where the string is written from the pointer.</param>
    /// <param name="text">The text to write.</param>
    public void Write(Enum   offset,
                      string text)
    {
      Write(Convert.ToInt32(offset),
            text);
    }

    /// <summary>Writes a string using the encoding UTF8 in the remote process.</summary>
    /// <param name="text">The text to write.</param>
    public void Write(string text)
    {
      Write(0,
            text);
    }

    public static bool operator ==(MemoryPointer left,
                                   MemoryPointer right)
    {
      return Equals(left,
                    right);
    }

    public static bool operator !=(MemoryPointer left,
                                   MemoryPointer right)
    {
      return !Equals(left,
                     right);
    }


    private void CheckValueChanged(object _)
    {
      ValueChangedMutex.WaitOne();

      try
      {
        if (ValueChangedSuspended)
          return;

        byte[] newValue = null;

        try
        {
          newValue = Read(LastValue.Length,
                          ValueChangedOffset);
        }
        catch (Win32Exception)
        {
          if (LastValue == null)
            return;
        }

        if (newValue?.SequenceEqual(LastValue) == false)
        {
          var toRm = new List<Func<byte[], bool>>();
          LastValue = newValue;

          lock (ValueChangedHandlers)
            foreach (var handler in ValueChangedHandlers)
              try
              {
                if (handler(newValue))
                  toRm.Add(handler);
              }
              catch
              {
                // ignored
              }

          foreach (var handler in toRm)
            UnregisterValueChangedEventHandler(handler);
        }

        if (Process.Native.HasExited)
          lock (ValueChangedHandlers)
          {
            StopTimer();
            ValueChangedHandlers.Clear();
          }

        else
          RestartTimer();
      }
      finally
      {
        ValueChangedMutex.ReleaseMutex();
      }
    }

    #endregion
  }
}
