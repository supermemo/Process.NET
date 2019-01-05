using Process.NET.Assembly;
using Process.NET.Native.Types;
using Process.NET.Patterns;
using System;

namespace Process.NET.Execution
{
  interface IProcedure
  {
    IntPtr BaseAddr { get; set; }
    IAssemblyFactory Factory { get; set; }

    CallingConventions CallingConvention { get; }
    IMemoryPattern Pattern { get; }
  }
}
