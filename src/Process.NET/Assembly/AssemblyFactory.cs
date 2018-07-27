using System;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Process.NET.Assembly.Assemblers;
using Process.NET.Assembly.CallingConventions;
using Process.NET.Marshaling;
using Process.NET.Memory;
using Process.NET.Native.Types;
using Process.NET.Threads;
using Process.NET.Utilities;
using ExecutionContext = Process.NET.Threads.ExecutionContext;

namespace Process.NET.Assembly
{
  public class AssemblyFactory : IAssemblyFactory
  {
    #region Constants & Statics

    protected const int ThreadHijackBytePreCodeLength     = 2;
    protected const int ThreadHijackByteCodeLength        = 22;
    protected const int ThreadHijackByteCodeAndDataLength = ThreadHijackByteCodeLength + 5;

    #endregion




    #region Constructors

    /// <summary>Initializes a new instance of the <see cref="AssemblyFactory" /> class.</summary>
    /// <param name="process">The process.</param>
    /// <param name="assembler">The assembler.</param>
    public AssemblyFactory(IProcess   process,
                           IAssembler assembler)
    {
      Process   = process;
      Assembler = assembler;
    }

    /// <summary>Releases all resources used by the <see cref="AssemblyFactory" /> object.</summary>
    public void Dispose()
    {
      // Nothing to dispose... yet
    }

    #endregion




    #region Properties Impl - Public

    /// <summary>Gets or sets the assembler.</summary>
    /// <value>The assembler.</value>
    public IAssembler Assembler { get; set; }

    /// <summary>Gets the process.</summary>
    /// <value>The process.</value>
    public IProcess Process { get; }

    #endregion




    #region Methods Impl

    /// <summary>
    ///   Begins a new transaction to inject and execute assembly code into the process at the
    ///   specified address.
    /// </summary>
    /// <param name="address">The address where the assembly code is injected.</param>
    /// <param name="autoExecute">
    ///   Indicates whether the assembly code is executed once the object is
    ///   disposed.
    /// </param>
    /// <param name="executingThread">Thread to hijack. Will create a new thread if null.</param>
    /// <returns>The return value is a new transaction.</returns>
    public AssemblyTransaction BeginTransaction(IntPtr        address,
                                                bool          autoExecute     = true,
                                                IRemoteThread executingThread = null)
    {
      return new AssemblyTransaction(this,
                                     address,
                                     autoExecute,
                                     executingThread);
    }

    /// <summary>Begins a new transaction to inject and execute assembly code into the process.</summary>
    /// <param name="autoExecute">
    ///   Indicates whether the assembly code is executed once the object is
    ///   disposed.
    /// </param>
    /// <param name="executingThread">Thread to hijack. Will create a new thread if null.</param>
    /// <returns>The return value is a new transaction.</returns>
    public AssemblyTransaction BeginTransaction(bool          autoExecute     = true,
                                                IRemoteThread executingThread = null)
    {
      return new AssemblyTransaction(this,
                                     autoExecute,
                                     executingThread);
    }

    /// <summary>Executes the assembly code located in the remote process at the specified address.</summary>
    /// <param name="address">The address where the assembly code is located.</param>
    /// <returns>The return value is the exit code of the thread created to execute the assembly code.</returns>
    public IntPtr Execute(IntPtr address)
    {
      return Execute<IntPtr>(address);
    }

    /// <summary>Executes the assembly code located in the remote process at the specified address.</summary>
    /// <param name="address">The address where the assembly code is located.</param>
    /// <returns>The return value is the exit code of the thread created to execute the assembly code.</returns>
    public T Execute<T>(IntPtr address)
    {
      // Execute and join the code in a new thread
      var executingThread = Process.ThreadFactory.CreateAndJoin(address);

      // Return the exit code of the thread
      return executingThread.GetExitCode<T>();
    }

    /// <summary>Executes the assembly code located in the remote process at the specified address.</summary>
    /// <param name="address">The address where the assembly code is located.</param>
    /// <param name="parameter">The parameter used to execute the assembly code.</param>
    /// <returns>The return value is the exit code of the thread created to execute the assembly code.</returns>
    public T Execute<T>(IntPtr  address,
                        dynamic parameter)
    {
      // Execute and join the code in a new thread
      var thread = Process.ThreadFactory.CreateAndJoin(address,
                                                       parameter);

      // Return the exit code of the thread
      return thread.GetExitCode<T>();
    }

    /// <summary>Executes the assembly code located in the remote process at the specified address.</summary>
    /// <param name="address">The address where the assembly code is located.</param>
    /// <param name="parameter">The parameter used to execute the assembly code.</param>
    /// <returns>The return value is the exit code of the thread created to execute the assembly code.</returns>
    public IntPtr Execute(IntPtr  address,
                          dynamic parameter)
    {
      return Execute<IntPtr>(address,
                             parameter);
    }

    /// <summary>Executes the assembly code located in the remote process at the specified address.</summary>
    /// <param name="address">The address where the assembly code is located.</param>
    /// <param name="callingConvention">
    ///   The calling convention used to execute the assembly code with
    ///   the parameters.
    /// </param>
    /// <param name="executingThread">Thread to hijack.</param>
    /// <returns>The return value is the exit code of the thread created to execute the assembly code.</returns>
    public T Execute<T>(IntPtr                          address,
                        Native.Types.CallingConventions callingConvention,
                        IRemoteThread                   executingThread)
    {
      // Start a transaction
      AssemblyTransaction t;
      using (t = BeginTransaction(true,
                                  executingThread))
      {
        // Get the object dedicated to create mnemonics for the given calling convention
        var calling = CallingConventionSelector.Get(callingConvention);

        // Call the function
        t.AddLine(calling.FormatCalling(address));

        // Add the return mnemonic
        t.AddLine("retn");
      }

      return t.GetExitCode<T>();
    }

    /// <summary>Executes the assembly code located in the remote process at the specified address.</summary>
    /// <param name="address">The address where the assembly code is located.</param>
    /// <param name="callingConvention">
    ///   The calling convention used to execute the assembly code with
    ///   the parameters.
    /// </param>
    /// <param name="executingThread">Thread to hijack. Will create a new thread if null.</param>
    /// <param name="parameters">An array of parameters used to execute the assembly code.</param>
    /// <returns>The return value is the exit code of the thread created to execute the assembly code.</returns>
    public T Execute<T>(IntPtr                          address,
                        Native.Types.CallingConventions callingConvention,
                        IRemoteThread                   executingThread = null,
                        params dynamic[]                parameters)
    {
      // Marshal the parameters
      var marshalledParameters =
        parameters.Select(p => MarshalValue.Marshal(Process,
                                                    p)).Cast<IMarshalledValue>().ToArray();
      // Start a transaction
      AssemblyTransaction t;
      using (t = BeginTransaction(true,
                                  executingThread))
      {
        // Get the object dedicated to create mnemonics for the given calling convention
        var calling = CallingConventionSelector.Get(callingConvention);
        // Push the parameters
        t.AddLine(calling.FormatParameters(marshalledParameters.Select(p => p.Reference).ToArray()));
        // Call the function
        t.AddLine(calling.FormatCalling(address));
        // Clean the parameters
        if (calling.Cleanup == CleanupTypes.Caller)
          t.AddLine(calling.FormatCleaning(marshalledParameters.Length));
        // Add the return mnemonic
        t.AddLine("retn");
      }

      // Clean the marshalled parameters
      foreach (var parameter in marshalledParameters)
        parameter.Dispose();

      // Return the exit code
      return t.GetExitCode<T>();
    }

    /// <summary>Executes the assembly code located in the remote process at the specified address.</summary>
    /// <param name="address">The address where the assembly code is located.</param>
    /// <param name="callingConvention">
    ///   The calling convention used to execute the assembly code with
    ///   the parameters.
    /// </param>
    /// <param name="executingThread">Thread to hijack. Will create a new thread if null.</param>
    /// <param name="parameters">An array of parameters used to execute the assembly code.</param>
    /// <returns>The return value is the exit code of the thread created to execute the assembly code.</returns>
    public IntPtr Execute(IntPtr                          address,
                          Native.Types.CallingConventions callingConvention,
                          IRemoteThread                   executingThread = null,
                          params dynamic[]                parameters)
    {
      return Execute<IntPtr>(address,
                             callingConvention,
                             executingThread,
                             parameters);
    }

    /// <summary>Executes the assembly code located in the remote process at the specified address.</summary>
    /// <param name="address">The address where the assembly code is located.</param>
    /// <param name="callingConvention">
    ///   The calling convention used to execute the assembly code with
    ///   the parameters.
    /// </param>
    /// <param name="executingThread">Thread to hijack.</param>
    /// <returns>The return value is the exit code of the thread created to execute the assembly code.</returns>
    public IntPtr Execute(IntPtr                          address,
                          Native.Types.CallingConventions callingConvention,
                          IRemoteThread                   executingThread)
    {
      return Execute<IntPtr>(address,
                             callingConvention,
                             executingThread);
    }

    /// <summary>
    ///   Executes asynchronously the assembly code located in the remote process at the
    ///   specified address.
    /// </summary>
    /// <param name="address">The address where the assembly code is located.</param>
    /// <param name="executingThread">Thread to hijack. Will create a new thread if null.</param>
    /// <returns>
    ///   The return value is an asynchronous operation that return the exit code of the thread
    ///   created to execute the assembly code.
    /// </returns>
    public Task<T> ExecuteAsync<T>(IntPtr        address,
                                   IRemoteThread executingThread = null)
    {
      return Task.Run(() => Execute<T>(address,
                                       executingThread));
    }

    /// <summary>
    ///   Executes asynchronously the assembly code located in the remote process at the
    ///   specified address.
    /// </summary>
    /// <param name="address">The address where the assembly code is located.</param>
    /// <param name="executingThread">Thread to hijack. Will create a new thread if null.</param>
    /// <returns>
    ///   The return value is an asynchronous operation that return the exit code of the thread
    ///   created to execute the assembly code.
    /// </returns>
    public Task<IntPtr> ExecuteAsync(IntPtr        address,
                                     IRemoteThread executingThread = null)
    {
      return ExecuteAsync<IntPtr>(address,
                                  executingThread);
    }

    /// <summary>
    ///   Executes asynchronously the assembly code located in the remote process at the
    ///   specified address.
    /// </summary>
    /// <param name="address">The address where the assembly code is located.</param>
    /// <param name="executingThread">Thread to hijack. Will create a new thread if null.</param>
    /// <param name="parameter">The parameter used to execute the assembly code.</param>
    /// <returns>
    ///   The return value is an asynchronous operation that return the exit code of the thread
    ///   created to execute the assembly code.
    /// </returns>
    public Task<T> ExecuteAsync<T>(IntPtr  address,
                                   dynamic parameter)
    {
      return Task.Run(() => (Task<T>)Execute<T>(address,
                                                parameter));
    }

    /// <summary>
    ///   Executes asynchronously the assembly code located in the remote process at the
    ///   specified address.
    /// </summary>
    /// <param name="address">The address where the assembly code is located.</param>
    /// <param name="executingThread">Thread to hijack. Will create a new thread if null.</param>
    /// <param name="parameter">The parameter used to execute the assembly code.</param>
    /// <returns>
    ///   The return value is an asynchronous operation that return the exit code of the thread
    ///   created to execute the assembly code.
    /// </returns>
    public Task<IntPtr> ExecuteAsync(IntPtr  address,
                                     dynamic parameter)
    {
      return ExecuteAsync<IntPtr>(address,
                                  parameter);
    }

    /// <summary>
    ///   Executes asynchronously the assembly code located in the remote process at the
    ///   specified address.
    /// </summary>
    /// <param name="address">The address where the assembly code is located.</param>
    /// <param name="callingConvention">
    ///   The calling convention used to execute the assembly code with
    ///   the parameters.
    /// </param>
    /// <param name="executingThread">Thread to hijack. Will create a new thread if null.</param>
    /// <param name="parameters">An array of parameters used to execute the assembly code.</param>
    /// <returns>
    ///   The return value is an asynchronous operation that return the exit code of the thread
    ///   created to execute the assembly code.
    /// </returns>
    public Task<T> ExecuteAsync<T>(IntPtr                          address,
                                   Native.Types.CallingConventions callingConvention,
                                   IRemoteThread                   executingThread = null,
                                   params dynamic[]                parameters)
    {
      return Task.Run(() => Execute<T>(address,
                                       callingConvention,
                                       executingThread,
                                       parameters));
    }

    /// <summary>
    ///   Executes asynchronously the assembly code located in the remote process at the
    ///   specified address.
    /// </summary>
    /// <param name="address">The address where the assembly code is located.</param>
    /// <param name="callingConvention">
    ///   The calling convention used to execute the assembly code with
    ///   the parameters.
    /// </param>
    /// <param name="executingThread">Thread to hijack. Will create a new thread if null.</param>
    /// <param name="parameters">An array of parameters used to execute the assembly code.</param>
    /// <returns>
    ///   The return value is an asynchronous operation that return the exit code of the thread
    ///   created to execute the assembly code.
    /// </returns>
    public Task<IntPtr> ExecuteAsync(IntPtr                          address,
                                     Native.Types.CallingConventions callingConvention,
                                     IRemoteThread                   executingThread = null,
                                     params dynamic[]                parameters)
    {
      return ExecuteAsync<IntPtr>(address,
                                  callingConvention,
                                  executingThread,
                                  parameters);
    }

    /// <summary>
    ///   Assembles mnemonics and injects the corresponding assembly code into the remote
    ///   process at the specified address.
    /// </summary>
    /// <param name="userAsm">The mnemonics to inject.</param>
    /// <param name="address">The address where the assembly code is injected.</param>
    /// <param name="executingThread">Thread to hijack. Will create a new thread if null.</param>
    public ExecutionContext Inject(string        userAsm,
                                   IntPtr        address,
                                   IRemoteThread executingThread = null)
    {
      var (asm, assembleAddr) = AdjustAsm(userAsm,
                                          address,
                                          executingThread);
      var asmBytes = Assembler.Assemble(asm,
                                        assembleAddr);

      return Inject(asmBytes,
                    address,
                    executingThread);

      //System.Diagnostics.Debug.WriteLine(
      //  $"Injected code ({address}): "
      //  + String.Join(" ",
      //                Process.Memory.Read(address,
      //                                    asmBytes.Length).Select(b => b.ToString("X2")))
      //  + $"\nASM:\n{asm}"
      //);
    }

    /// <summary>
    ///   Assembles mnemonics and injects the corresponding assembly code into the remote
    ///   process at the specified address.
    /// </summary>
    /// <param name="asm">An array containing the mnemonics to inject.</param>
    /// <param name="address">The address where the assembly code is injected.</param>
    /// <param name="executingThread">Thread to hijack. Will create a new thread if null.</param>
    public ExecutionContext Inject(string[]      asm,
                                   IntPtr        address,
                                   IRemoteThread executingThread = null)
    {
      return Inject(string.Join("\n",
                                asm),
                    address,
                    executingThread);
    }

    /// <summary>
    ///   Assembles mnemonics and injects the corresponding assembly code into the remote
    ///   process.
    /// </summary>
    /// <param name="asm">The mnemonics to inject.</param>
    /// <param name="executingThread">Thread to hijack. Will create a new thread if null.</param>
    /// <returns>The address where the assembly code is injected.</returns>
    public (IAllocatedMemory, ExecutionContext) Inject(string        asm,
                                                       IRemoteThread executingThread = null)
    {
      // Assemble the assembly code
      var asmBytes = Assembler.Assemble(asm);

      // Adjust length for optional thread hijacking byte code
      int codeLength = asmBytes.Length + (executingThread == null ? 0 : ThreadHijackByteCodeAndDataLength);

      // Allocate a chunk of memory to store the assembly code
      var memory = Process.MemoryFactory.Allocate(Randomizer.GenerateString(),
                                                  codeLength);
      // Inject the code
      var execContext = Inject(asm,
                               memory.BaseAddress,
                               executingThread);

      // Return the memory allocated
      return (memory, execContext);
    }

    /// <summary>
    ///   Assembles mnemonics and injects the corresponding assembly code into the remote
    ///   process.
    /// </summary>
    /// <param name="asm">An array containing the mnemonics to inject.</param>
    /// <param name="executingThread">Thread to hijack. Will create a new thread if null.</param>
    /// <returns>The address where the assembly code is injected.</returns>
    public (IAllocatedMemory, ExecutionContext) Inject(string[]      asm,
                                                       IRemoteThread executingThread = null)
    {
      return Inject(string.Join("\n",
                                asm),
                    executingThread);
    }

    /// <summary>
    ///   Assembles, injects and executes the mnemonics into the remote process at the
    ///   specified address.
    /// </summary>
    /// <param name="asm">The mnemonics to inject.</param>
    /// <param name="address">The address where the assembly code is injected.</param>
    /// <param name="executingThread">Thread to hijack. Will create a new thread if null.</param>
    /// <returns>The return value is the exit code of the thread created to execute the assembly code.</returns>
    public T InjectAndExecute<T>(string        asm,
                                 IntPtr        address,
                                 IRemoteThread executingThread)
    {
      // Inject the assembly code
      var execContext = Inject(asm,
                               address,
                               executingThread);
      // Execute the code
      return Execute<T>(address,
                        execContext);
    }

    /// <summary>
    ///   Assembles, injects and executes the mnemonics into the remote process at the
    ///   specified address.
    /// </summary>
    /// <param name="asm">The mnemonics to inject.</param>
    /// <param name="address">The address where the assembly code is injected.</param>
    /// <param name="executingThread">Thread to hijack. Will create a new thread if null.</param>
    /// <returns>The return value is the exit code of the thread created to execute the assembly code.</returns>
    public IntPtr InjectAndExecute(string        asm,
                                   IntPtr        address,
                                   IRemoteThread executingThread)
    {
      return InjectAndExecute<IntPtr>(asm,
                                      address,
                                      executingThread);
    }

    /// <summary>
    ///   Assembles, injects and executes the mnemonics into the remote process at the
    ///   specified address.
    /// </summary>
    /// <param name="asm">An array containing the mnemonics to inject.</param>
    /// <param name="address">The address where the assembly code is injected.</param>
    /// <param name="executingThread">Thread to hijack. Will create a new thread if null.</param>
    /// <returns>The return value is the exit code of the thread created to execute the assembly code.</returns>
    public T InjectAndExecute<T>(string[]      asm,
                                 IntPtr        address,
                                 IRemoteThread executingThread)
    {
      return InjectAndExecute<T>(string.Join("\n",
                                             asm),
                                 address,
                                 executingThread);
    }

    /// <summary>
    ///   Assembles, injects and executes the mnemonics into the remote process at the
    ///   specified address.
    /// </summary>
    /// <param name="asm">An array containing the mnemonics to inject.</param>
    /// <param name="address">The address where the assembly code is injected.</param>
    /// <param name="executingThread">Thread to hijack. Will create a new thread if null.</param>
    /// <returns>The return value is the exit code of the thread created to execute the assembly code.</returns>
    public IntPtr InjectAndExecute(string[]      asm,
                                   IntPtr        address,
                                   IRemoteThread executingThread)
    {
      return InjectAndExecute<IntPtr>(asm,
                                      address,
                                      executingThread);
    }

    /// <summary>Assembles, injects and executes the mnemonics into the remote process.</summary>
    /// <param name="asm">The mnemonics to inject.</param>
    /// <param name="executingThread">Thread to hijack. Will create a new thread if null.</param>
    /// <returns>The return value is the exit code of the thread created to execute the assembly code.</returns>
    public T InjectAndExecute<T>(string        asm,
                                 IRemoteThread executingThread = null)
    {
      // Inject the assembly code
      var (memory, execContext) = Inject(asm,
                                         executingThread);
      try
      {
        // Execute the code
        var ret = Execute<T>(memory.BaseAddress,
                             execContext);

        return ret;
      }
      finally
      {
        memory?.Dispose();
      }
    }

    /// <summary>Assembles, injects and executes the mnemonics into the remote process.</summary>
    /// <param name="asm">The mnemonics to inject.</param>
    /// <param name="executingThread">Thread to hijack. Will create a new thread if null.</param>
    /// <returns>The return value is the exit code of the thread created to execute the assembly code.</returns>
    public IntPtr InjectAndExecute(string        asm,
                                   IRemoteThread executingThread = null)
    {
      return InjectAndExecute<IntPtr>(asm,
                                      executingThread);
    }

    /// <summary>Assembles, injects and executes the mnemonics into the remote process.</summary>
    /// <param name="asm">An array containing the mnemonics to inject.</param>
    /// <param name="executingThread">Thread to hijack. Will create a new thread if null.</param>
    /// <returns>The return value is the exit code of the thread created to execute the assembly code.</returns>
    public T InjectAndExecute<T>(string[]      asm,
                                 IRemoteThread executingThread = null)
    {
      return InjectAndExecute<T>(string.Join("\n",
                                             asm),
                                 executingThread);
    }

    /// <summary>Assembles, injects and executes the mnemonics into the remote process.</summary>
    /// <param name="asm">An array containing the mnemonics to inject.</param>
    /// <param name="executingThread">Thread to hijack. Will create a new thread if null.</param>
    /// <returns>The return value is the exit code of the thread created to execute the assembly code.</returns>
    public IntPtr InjectAndExecute(string[]      asm,
                                   IRemoteThread executingThread = null)
    {
      return InjectAndExecute<IntPtr>(asm,
                                      executingThread);
    }

    /// <summary>
    ///   Assembles, injects and executes asynchronously the mnemonics into the remote process
    ///   at the specified address.
    /// </summary>
    /// <param name="asm">The mnemonics to inject.</param>
    /// <param name="address">The address where the assembly code is injected.</param>
    /// <param name="executingThread">Thread to hijack. Will create a new thread if null.</param>
    /// <returns>
    ///   The return value is an asynchronous operation that return the exit code of the thread
    ///   created to execute the assembly code.
    /// </returns>
    public Task<T> InjectAndExecuteAsync<T>(string        asm,
                                            IntPtr        address,
                                            IRemoteThread executingThread)
    {
      return Task.Run(() => InjectAndExecute<T>(asm,
                                                address,
                                                executingThread));
    }

    /// <summary>
    ///   Assembles, injects and executes asynchronously the mnemonics into the remote process
    ///   at the specified address.
    /// </summary>
    /// <param name="asm">The mnemonics to inject.</param>
    /// <param name="address">The address where the assembly code is injected.</param>
    /// <param name="executingThread">Thread to hijack. Will create a new thread if null.</param>
    /// <returns>
    ///   The return value is an asynchronous operation that return the exit code of the thread
    ///   created to execute the assembly code.
    /// </returns>
    public Task<IntPtr> InjectAndExecuteAsync(string        asm,
                                              IntPtr        address,
                                              IRemoteThread executingThread)
    {
      return InjectAndExecuteAsync<IntPtr>(asm,
                                           address,
                                           executingThread);
    }

    /// <summary>
    ///   Assembles, injects and executes asynchronously the mnemonics into the remote process
    ///   at the specified address.
    /// </summary>
    /// <param name="asm">An array containing the mnemonics to inject.</param>
    /// <param name="address">The address where the assembly code is injected.</param>
    /// <param name="executingThread">Thread to hijack. Will create a new thread if null.</param>
    /// <returns>
    ///   The return value is an asynchronous operation that return the exit code of the thread
    ///   created to execute the assembly code.
    /// </returns>
    public Task<T> InjectAndExecuteAsync<T>(string[]      asm,
                                            IntPtr        address,
                                            IRemoteThread executingThread)
    {
      return Task.Run(() => InjectAndExecute<T>(asm,
                                                address,
                                                executingThread));
    }

    /// <summary>
    ///   Assembles, injects and executes asynchronously the mnemonics into the remote process
    ///   at the specified address.
    /// </summary>
    /// <param name="asm">An array containing the mnemonics to inject.</param>
    /// <param name="address">The address where the assembly code is injected.</param>
    /// <param name="executingThread">Thread to hijack. Will create a new thread if null.</param>
    /// <returns>
    ///   The return value is an asynchronous operation that return the exit code of the thread
    ///   created to execute the assembly code.
    /// </returns>
    public Task<IntPtr> InjectAndExecuteAsync(string[]      asm,
                                              IntPtr        address,
                                              IRemoteThread executingThread)
    {
      return InjectAndExecuteAsync<IntPtr>(asm,
                                           address,
                                           executingThread);
    }

    /// <summary>Assembles, injects and executes asynchronously the mnemonics into the remote process.</summary>
    /// <param name="asm">The mnemonics to inject.</param>
    /// <param name="executingThread">Thread to hijack. Will create a new thread if null.</param>
    /// <returns>
    ///   The return value is an asynchronous operation that return the exit code of the thread
    ///   created to execute the assembly code.
    /// </returns>
    public Task<T> InjectAndExecuteAsync<T>(string        asm,
                                            IRemoteThread executingThread = null)
    {
      return Task.Run(() => InjectAndExecute<T>(asm,
                                                executingThread));
    }

    /// <summary>Assembles, injects and executes asynchronously the mnemonics into the remote process.</summary>
    /// <param name="asm">The mnemonics to inject.</param>
    /// <param name="executingThread">Thread to hijack. Will create a new thread if null.</param>
    /// <returns>
    ///   The return value is an asynchronous operation that return the exit code of the thread
    ///   created to execute the assembly code.
    /// </returns>
    public Task<IntPtr> InjectAndExecuteAsync(string        asm,
                                              IRemoteThread executingThread = null)
    {
      return InjectAndExecuteAsync<IntPtr>(asm,
                                           executingThread);
    }

    /// <summary>Assembles, injects and executes asynchronously the mnemonics into the remote process.</summary>
    /// <param name="asm">An array containing the mnemonics to inject.</param>
    /// <param name="executingThread">Thread to hijack. Will create a new thread if null.</param>
    /// <returns>
    ///   The return value is an asynchronous operation that return the exit code of the thread
    ///   created to execute the assembly code.
    /// </returns>
    public Task<T> InjectAndExecuteAsync<T>(string[]      asm,
                                            IRemoteThread executingThread = null)
    {
      return Task.Run(() => InjectAndExecute<T>(asm,
                                                executingThread));
    }

    /// <summary>Assembles, injects and executes asynchronously the mnemonics into the remote process.</summary>
    /// <param name="asm">An array containing the mnemonics to inject.</param>
    /// <param name="executingThread">Thread to hijack. Will create a new thread if null.</param>
    /// <returns>
    ///   The return value is an asynchronous operation that return the exit code of the thread
    ///   created to execute the assembly code.
    /// </returns>
    public Task<IntPtr> InjectAndExecuteAsync(string[]      asm,
                                              IRemoteThread executingThread = null)
    {
      return InjectAndExecuteAsync<IntPtr>(asm,
                                           executingThread);
    }

    #endregion




    #region Methods

    private T WaitHijackedThreadSignal<T>(IntPtr           address,
                                          ExecutionContext executingContext)
    {
      AutoResetEvent ev = new AutoResetEvent(false);

      bool OnSignaled()
      {
        ev.Set();

        return true;
      }

      using (var ptr = Process[executingContext.SignalAddr])
      {
        ptr.RegisterValueChangedEventHandler(OnSignaled,
                                             MarshalType<T>.Size);

        executingContext.Thread.Resume();

        // TODO: Pass timeout by parameter
        bool success = ev.WaitOne(6000);

        return success
          ? Process.Memory.Read<T>(executingContext.RetAddr)
          : throw new InvalidOperationException("Hijacked thread method call timed out");
      }
    }

    /// <summary>Executes the assembly code located in the remote process at the specified address.</summary>
    /// <param name="address">The address where the assembly code is located.</param>
    /// <param name="executionContext">Execution context.</param>
    /// <returns>The return value is the exit code of the thread created to execute the assembly code.</returns>
    private T Execute<T>(IntPtr           address,
                         ExecutionContext executionContext)
    {
      if (executionContext.Thread == null)
      {
        // Execute and join the code in a new thread
        var executingThread = Process.ThreadFactory.CreateAndJoin(address);

        // Return the exit code of the thread
        return executingThread.GetExitCode<T>();
      }

      else
      {
        return WaitHijackedThreadSignal<T>(address,
                                           executionContext);
      }
    }

    private (string asm, IntPtr assembleAddr) AdjustAsm(string        asm,
                                                        IntPtr        Address,
                                                        IRemoteThread executingThread)
    {
      if (executingThread != null)
      {
        asm = asm.TrimEnd('\n',
                          '\r',
                          ' ',
                          '\t');

        if (asm.EndsWith("retn"))
          asm = asm.Substring(0,
                              asm.Length - 4);

        // TODO: Replace any retn inside asm with a jmp

        Address = new IntPtr(Address.ToInt32() + ThreadHijackBytePreCodeLength);
      }

      return (asm, Address);
    }

    private (byte[] asmBytes, int signalAddr, int retAddr) InjectThreadHijack(
      byte[]        asmBytes,
      IntPtr        address,
      IRemoteThread executingThread = null)
    {
      int signalAddr = 0;
      int retAddr    = 0;

      if (executingThread != null)
      {
        // TODO: This may cause an issue if execution is delayed
        if (executingThread.Suspend() == null)
          throw new InvalidOperationException("Thread is terminated");

        var threadContext = executingThread.Context;
        int restoreEip    = threadContext.Eip;

        signalAddr = address.ToInt32() + asmBytes.Length + ThreadHijackByteCodeLength;
        retAddr    = address.ToInt32() + asmBytes.Length + ThreadHijackByteCodeLength + 1;

        threadContext.Eip       = address.ToInt32();
        executingThread.Context = threadContext;

        StringBuilder preAsmBuilder = new StringBuilder();
        preAsmBuilder.AppendLine("pushad");
        preAsmBuilder.AppendLine("pushfd");

        StringBuilder postAsmBuilder = new StringBuilder();
        postAsmBuilder.AppendLine($"mov BYTE [0x{signalAddr:X8}], 1");
        postAsmBuilder.AppendLine($"mov DWORD [0x{retAddr:X8}], eax");
        postAsmBuilder.AppendLine("popfd");
        postAsmBuilder.AppendLine("popad");
        postAsmBuilder.AppendLine($"push {restoreEip}");
        postAsmBuilder.AppendLine("retn");

        var preAsmBytes  = Assembler.Assemble(preAsmBuilder.ToString());
        var postAsmBytes = Assembler.Assemble(postAsmBuilder.ToString());

        asmBytes = Combine(preAsmBytes,
                           asmBytes,
                           postAsmBytes);
      }

      return (asmBytes, signalAddr, retAddr);
    }

    private static byte[] Combine(params byte[][] arrays)
    {
      byte[] rv     = new byte[arrays.Sum(a => a.Length)];
      int    offset = 0;

      foreach (byte[] array in arrays)
      {
        Buffer.BlockCopy(array,
                         0,
                         rv,
                         offset,
                         array.Length);
        offset += array.Length;
      }

      return rv;
    }

    /// <summary>
    ///   Assembles mnemonics and injects the corresponding assembly code into the remote
    ///   process at the specified address.
    /// </summary>
    /// <param name="asmBytes"></param>
    /// <param name="address">The address where the assembly code is injected.</param>
    /// <param name="executingThread">Thread to hijack. Will create a new thread if null.</param>
    private ExecutionContext Inject(byte[]        asmBytes,
                                    IntPtr        address,
                                    IRemoteThread executingThread)
    {
      int signalAddr = 0;
      int retAddr    = 0;

      if (executingThread != null)
        (asmBytes, signalAddr, retAddr) = InjectThreadHijack(asmBytes,
                                                             address,
                                                             executingThread);

      Process.Memory.Write(address,
                           asmBytes);

      return new ExecutionContext
      {
        Thread     = executingThread,
        SignalAddr = new IntPtr(signalAddr),
        RetAddr    = new IntPtr(retAddr)
      };
    }

    #endregion
  }
}
