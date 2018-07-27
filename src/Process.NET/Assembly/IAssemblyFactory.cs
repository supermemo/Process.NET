using System;
using System.Threading.Tasks;
using Process.NET.Assembly.Assemblers;
using Process.NET.Memory;
using Process.NET.Threads;

namespace Process.NET.Assembly
{
  public interface IAssemblyFactory : IDisposable
  {
    IAssembler Assembler { get; set; }
    IProcess   Process   { get; }

    AssemblyTransaction BeginTransaction(bool          autoExecute     = true,
                                         IRemoteThread executingThread = null);

    AssemblyTransaction BeginTransaction(IntPtr        address,
                                         bool          autoExecute     = true,
                                         IRemoteThread executingThread = null);

    IntPtr Execute(IntPtr address);

    IntPtr Execute(IntPtr  address,
                   dynamic parameter);

    IntPtr Execute(IntPtr                          address,
                   Native.Types.CallingConventions callingConvention,
                   IRemoteThread                   executingThread);

    IntPtr Execute(IntPtr                          address,
                   Native.Types.CallingConventions callingConvention,
                   IRemoteThread                   executingThread = null,
                   params dynamic[]                parameters);

    T Execute<T>(IntPtr  address);

    T Execute<T>(IntPtr  address,
                 dynamic parameter);

    T Execute<T>(IntPtr                          address,
                 Native.Types.CallingConventions callingConvention,
                 IRemoteThread                   executingThread);

    T Execute<T>(IntPtr                          address,
                 Native.Types.CallingConventions callingConvention,
                 IRemoteThread                   executingThread = null,
                 params dynamic[]                parameters);

    Task<IntPtr> ExecuteAsync(IntPtr        address,
                              IRemoteThread executingThread = null);

    Task<IntPtr> ExecuteAsync(IntPtr  address,
                              dynamic parameter);

    Task<IntPtr> ExecuteAsync(IntPtr                          address,
                              Native.Types.CallingConventions callingConvention,
                              IRemoteThread                   executingThread = null,
                              params dynamic[]                parameters);

    Task<T> ExecuteAsync<T>(IntPtr        address,
                            IRemoteThread executingThread = null);

    Task<T> ExecuteAsync<T>(IntPtr  address,
                            dynamic parameter);

    Task<T> ExecuteAsync<T>(IntPtr                          address,
                            Native.Types.CallingConventions callingConvention,
                            IRemoteThread                   executingThread = null,
                            params dynamic[]                parameters);

    (IAllocatedMemory, ExecutionContext) Inject(string[]      asm,
                                                IRemoteThread executingThread = null);

    (IAllocatedMemory, ExecutionContext) Inject(string        asm,
                                                IRemoteThread executingThread = null);

    ExecutionContext Inject(string[]      asm,
                            IntPtr        address,
                            IRemoteThread executingThread = null);

    ExecutionContext Inject(string        asm,
                            IntPtr        address,
                            IRemoteThread executingThread = null);

    IntPtr InjectAndExecute(string[]      asm,
                            IRemoteThread executingThread = null);

    IntPtr InjectAndExecute(string        asm,
                            IRemoteThread executingThread = null);

    IntPtr InjectAndExecute(string[]      asm,
                            IntPtr        address,
                            IRemoteThread executingThread = null);

    IntPtr InjectAndExecute(string        asm,
                            IntPtr        address,
                            IRemoteThread executingThread = null);

    T InjectAndExecute<T>(string[]      asm,
                          IRemoteThread executingThread = null);

    T InjectAndExecute<T>(string        asm,
                          IRemoteThread executingThread = null);

    T InjectAndExecute<T>(string[]      asm,
                          IntPtr        address,
                          IRemoteThread executingThread = null);

    T InjectAndExecute<T>(string        asm,
                          IntPtr        address,
                          IRemoteThread executingThread = null);

    Task<IntPtr> InjectAndExecuteAsync(string[]      asm,
                                       IRemoteThread executingThread = null);

    Task<IntPtr> InjectAndExecuteAsync(string        asm,
                                       IRemoteThread executingThread = null);

    Task<IntPtr> InjectAndExecuteAsync(string[]      asm,
                                       IntPtr        address,
                                       IRemoteThread executingThread = null);

    Task<IntPtr> InjectAndExecuteAsync(string        asm,
                                       IntPtr        address,
                                       IRemoteThread executingThread = null);

    Task<T> InjectAndExecuteAsync<T>(string[]      asm,
                                     IRemoteThread executingThread = null);

    Task<T> InjectAndExecuteAsync<T>(string        asm,
                                     IRemoteThread executingThread = null);

    Task<T> InjectAndExecuteAsync<T>(string[]      asm,
                                     IntPtr        address,
                                     IRemoteThread executingThread = null);

    Task<T> InjectAndExecuteAsync<T>(string        asm,
                                     IntPtr        address,
                                     IRemoteThread executingThread = null);
  }
}
