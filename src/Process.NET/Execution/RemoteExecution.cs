using System;
using System.Collections.Generic;
using System.Reflection;
using Process.NET.Assembly;
using Process.NET.Assembly.Assemblers;
using Process.NET.Patterns;

namespace Process.NET.Execution
{
  /// <summary>
  /// Enable execution of remote procedures by loading IProcedure members from <typeparamref name="TExecDesc"/>
  /// </summary>
  /// <typeparam name="TExecDesc"></typeparam>
  public class RemoteExecution<TExecDesc> : IRemoteExecution
    where TExecDesc : class
  {
    public RemoteExecution(ProcessSharp<TExecDesc> process, bool initialize, Dictionary<string, int> cachedAddresses, object[] procedureConstructorParams)
    {
      Process = process;
      CachedAddresses = cachedAddresses ?? new Dictionary<string, int>();
      Factory = new AssemblyFactory(process,
                                    new Fasm32Assembler());

      if (initialize)
        Initialize(procedureConstructorParams);
    }

    public bool Initialized { get; private set; } = false;

    private ProcessSharp<TExecDesc> Process { get; }
    public Dictionary<string, int> CachedAddresses { get; }
    public IAssemblyFactory Factory { get; set; }

    public void Initialize(params object[] procedureConstructorParams)
    {
      var scanner = new PatternScanner(Process.ModuleFactory.MainModule);
      Process.Procedures = (TExecDesc)LoadDescriptionMembers(scanner, typeof(TExecDesc), null, null, procedureConstructorParams);

      Initialized = true;
    }

    private object LoadDescriptionMembers(PatternScanner scanner, Type type, object parent, object instance, object[] procedureConstructorParams = null)
    {
      Type procItfType = typeof(IProcedure);

      if (parent == null)
        instance = Activator.CreateInstance(type, procedureConstructorParams);

      //else if (instance == null)
      //{
      //  var constructor = type.GetConstructor(new[] { parent.GetType() });

      //  if (constructor != null)
      //    instance = constructor.Invoke(new[] { parent });

      //  else
      //    instance = Activator.CreateInstance(type);
      //}
      
      var props = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);

      foreach (var prop in props)
      {
        if (procItfType.IsAssignableFrom(prop.PropertyType))
        {
          IProcedure proc = (IProcedure)prop.GetValue(instance);

          var hintAddr = CachedAddresses.ContainsKey(proc.Pattern.PatternText)
            ? CachedAddresses[proc.Pattern.PatternText]
            : 0;
          var scanRes = scanner.Find(proc.Pattern, hintAddr);

          if (scanRes.Found == false)
            throw new ArgumentException($"Procedure {prop.DeclaringType.Name}.{prop.Name} could not be found.");

          proc.Factory = Factory;
          proc.BaseAddr = scanRes.BaseAddress;

          CachedAddresses[proc.Pattern.PatternText] = scanRes.Offset;
        }

        else if (prop.PropertyType.GetTypeInfo().IsClass)
        {
          var propInstance = prop.GetValue(instance);

          if (propInstance != null)
            LoadDescriptionMembers(scanner, prop.PropertyType, instance, propInstance);

          //if (propInstance == null)
          //  prop.SetValue(instance, subClassInst);
        }
      }

      return instance;
    }
  }
}
