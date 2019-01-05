using Process.NET.Assembly;

namespace Process.NET.Execution
{
  public interface IRemoteExecution
  {
    bool Initialized { get; }
    IAssemblyFactory Factory { get; }

    void Initialize();
  }
}