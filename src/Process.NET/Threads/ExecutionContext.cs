using System;

namespace Process.NET.Threads
{
  public class ExecutionContext
  {
    #region Properties & Fields - Public

    public IRemoteThread Thread     { get; set; }
    public IntPtr        SignalAddr { get; set; }
    public IntPtr        RetAddr    { get; set; }

    #endregion
  }
}
