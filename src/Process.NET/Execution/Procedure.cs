using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Process.NET.Assembly;
using Process.NET.Patterns;
using Process.NET.Threads;
using CallingConventions = Process.NET.Native.Types.CallingConventions;

namespace Process.NET.Execution
{
  public class Procedure<TDelegate> : IProcedure
    where TDelegate : class
  {
    public Procedure(
      string name,
      CallingConventions callingConvention,
      IMemoryPattern pattern)
    {
      CallingConvention = callingConvention;
      Pattern = pattern;
      Name = name;

      Invoke = GenerateDelegate();
    }
    
    private string Name { get; }
    public TDelegate Invoke { get; set; }

    public IntPtr BaseAddr { get; set; }
    public IAssemblyFactory Factory { get; set; }

    public CallingConventions CallingConvention { get; }
    public IMemoryPattern Pattern { get; }

    private void testdyn(IntPtr a1, IRemoteThread a2, bool a3)
    {
      dynamic[] dynargs = new dynamic[3];
      dynargs[0] = a1;
      dynargs[1] = a2;
      dynargs[2] = a3;
      
      Factory.Execute(BaseAddr, CallingConvention, null, dynargs);
    }

    private TDelegate GenerateDelegate()
    {
      var thisType = new List<Type> { GetType() };
      var delType = typeof(TDelegate);
      var delInvoke = delType.GetMethod("Invoke");
      var delRetType = delInvoke.ReturnType;
      var delArgsType = thisType.Concat(delInvoke.GetParameters().Select(p => p.ParameterType)).ToArray();
      var delArgsCount = delArgsType.Length;
      var hasRetType = delRetType != typeof(void);

      var dynMeth = new DynamicMethod(
        Name,
        delRetType,
        delArgsType,
        GetType(),
        true
      );

      var il = dynMeth.GetILGenerator(256);
      
      // IRemoteThread remoteThread;
      var remoteThread = il.DeclareLocal(typeof(IRemoteThread));

      // var args = new dynamic[delArgsCount - 1];
      il.Emit(OpCodes.Ldc_I4, delArgsCount - 1);
      il.Emit(OpCodes.Newarr, typeof(object));
      il.Emit(OpCodes.Stloc_0);
      
      int skip = 0;
      
      // arg0 == this
      for (int i = 1; i < delArgsCount; i++)
      {
        if (delArgsType[i] == typeof(IRemoteThread))
        {
          // remoteThread = arg{i}
          il.Emit(OpCodes.Ldarg, i);
          il.Emit(OpCodes.Starg, remoteThread);
          skip++;
        }

        else
        {
          // args[i - 1 - skip] = arg{i}
          il.Emit(OpCodes.Ldloc_0);                // args
          il.Emit(OpCodes.Ldc_I4, i - 1 - skip);   // [i - 1 - skip]
          il.Emit(OpCodes.Ldarg, i);               // arg{i}

          if (delArgsType[i].IsClass == false)
            il.Emit(OpCodes.Box, delArgsType[i]);

          il.Emit(OpCodes.Stelem_Ref); //, delArgsType[i]);
        }
      }
      
      var getFactory = GetType().GetProperty(nameof(Factory), BindingFlags.Public | BindingFlags.Instance).GetGetMethod();
      var getBaseAddr = GetType().GetProperty(nameof(BaseAddr), BindingFlags.Public | BindingFlags.Instance).GetGetMethod();
      var getCallConv = GetType().GetProperty(nameof(CallingConvention), BindingFlags.Public | BindingFlags.Instance).GetGetMethod();
      
      // Get Factory
      il.Emit(OpCodes.Ldarg_0);
      il.Emit(OpCodes.Callvirt, getFactory);
      
      // Get BaseAddr
      il.Emit(OpCodes.Ldarg_0);
      il.Emit(OpCodes.Call, getBaseAddr);
      
      // Get CallingConvention
      il.Emit(OpCodes.Ldarg_0);
      il.Emit(OpCodes.Call, getCallConv);

      // Load Remote
      //il.Emit(OpCodes.Ldloc, remoteThread);
      il.Emit(OpCodes.Ldnull);

      // Load args
      il.Emit(OpCodes.Ldloc_0);


      MethodInfo executeMethInfo = hasRetType
        ? ExecuteGenericMethodInfo.MakeGenericMethod(delRetType)
        : ExecuteMethodInfo;

      il.Emit(OpCodes.Callvirt, executeMethInfo);

      if (hasRetType)
      {
        //il.Emit(OpCodes.Stloc_1);
        //il.Emit(OpCodes.Ldloc_1);
      }
      else
        il.Emit(OpCodes.Pop);
      
      il.Emit(OpCodes.Ret);

      return (TDelegate)(object)dynMeth.CreateDelegate(typeof(TDelegate), this);
    }

    public T testg<T>(IntPtr baseAddr, CallingConventions callConv, IRemoteThread remoteThread, params dynamic[] args)
    {
      System.Diagnostics.Debug.WriteLine($"{baseAddr}, {callConv}, {remoteThread}");//, {args.Length}");

      return default(T);
    }

    public void testng(IntPtr baseAddr, CallingConventions callConv, IRemoteThread remoteThread, params dynamic[] args)
    {
      System.Diagnostics.Debug.WriteLine($"{baseAddr}, {callConv}, {remoteThread}");//, {args.Length}");
      
    }

    private static MethodInfo ExecuteMethodInfo { get; } = typeof(IAssemblyFactory).GetMethods()
          .Where(m => m.Name == "Execute" && m.ReturnType == typeof(IntPtr))
          .Select(x => new { m = x, p = x.GetParameters() })
          .Where(x => x.p.Length == 4
          && x.p[0].ParameterType == typeof(IntPtr)
          && x.p[1].ParameterType == typeof(Native.Types.CallingConventions)
          && x.p[2].ParameterType == typeof(IRemoteThread)
          && x.p[3].ParameterType == typeof(dynamic[]))
          .FirstOrDefault()?.m;

    private static MethodInfo ExecuteGenericMethodInfo { get; } = typeof(IAssemblyFactory).GetMethods()
          .Where(m => m.Name == "Execute" && m.IsGenericMethod)
          .Select(x => new { m = x, p = x.GetParameters() })
          .Where(x => x.p.Length == 4
          && x.p[0].ParameterType == typeof(IntPtr)
          && x.p[1].ParameterType == typeof(Native.Types.CallingConventions)
          && x.p[2].ParameterType == typeof(IRemoteThread)
          && x.p[3].ParameterType == typeof(dynamic[]))
          .FirstOrDefault()?.m;
  }
}
