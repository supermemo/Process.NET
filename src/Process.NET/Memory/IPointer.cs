using System;
using System.Text;

namespace Process.NET.Memory
{
  public interface IPointer : IDisposable
  {
    IntPtr BaseAddress { get; }
    bool   IsValid     { get; }

    byte[] Read(int length,
                int offset = 0);

    string Read(Encoding encoding,
                int      maxLength,
                int      offset = 0);

    T Read<T>(int offset = 0);

    T[] Read<T>(int offset,
                int length);

    int Write(int    offset,
              byte[] toWrite);

    void Write(int      offset,
               string   stringToWrite,
               Encoding encoding);

    void Write<T>(int offset,
                  T[] values);

    void Write<T>(int offset,
                  T   value);

    void RegisterValueChangedEventHandler<T>(Func<bool> eventHandler,
                                             int        offset      = 0,
                                             int        msFrequency = 200);

    void RegisterValueChangedEventHandler(Func<bool> eventHandler,
                                          int        size,
                                          int        offset      = 0,
                                          int        msFrequency = 200);

    void UnregisterValueChangedEventHandler(Func<bool> eventHandler);
  }
}
