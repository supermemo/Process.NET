using System;
using Process.NET.Extensions;
using Process.NET.Marshaling;
using Process.NET.Modules;

namespace Process.NET.Patterns
{
  public class PatternScanner : IPatternScanner
  {
    #region Properties & Fields - Non-Public

    private readonly IProcessModule _module;

    #endregion




    #region Constructors

    public PatternScanner(IProcessModule module)
    {
      _module = module;
      Data = module.Read(_module.Size,
                         0);
    }

    #endregion




    #region Properties & Fields - Public

    public byte[] Data { get; }

    #endregion




    #region Methods Impl

    public PatternScanResult Find(IMemoryPattern pattern, int hintAddr = 0)
    {
      switch (pattern.PatternType)
      {
        case MemoryPatternType.Function:
          return FindFunctionPattern(pattern, hintAddr);

        case MemoryPatternType.Data:
          return FindDataPattern(pattern, hintAddr);

        case MemoryPatternType.Call:
          return FindCallPattern(pattern, hintAddr);

        default:
          throw new InvalidOperationException($"Invalid MemoryPatternType {pattern.PatternType}");
      }
    }

    #endregion




    #region Methods

    private PatternScanResult FindFunctionPattern(IMemoryPattern pattern, int hintAddr = 0)
    {
      var patternData       = Data;
      var patternDataLength = patternData.Length;
      var patternBytes      = pattern.GetBytes();

      if (hintAddr > 0)
        if (hintAddr + patternBytes.Count > patternDataLength
          || pattern.GetMask()
                    // ReSharper disable once AccessToModifiedClosure
                    .AnyEx((m, b) => m == 'x' && patternBytes[b] != patternData[b + hintAddr]))
          hintAddr = 0;

      for (var offset = hintAddr; offset < patternDataLength; offset++)
      {
        if (pattern.GetMask()
                   // ReSharper disable once AccessToModifiedClosure
                   .AnyEx((m, b) => m == 'x' && patternBytes[b] != patternData[b + offset]))
          continue;

        return new PatternScanResult
        {
          BaseAddress = _module.BaseAddress + offset,
          ReadAddress = _module.BaseAddress + offset,
          Offset      = offset,
          Found       = true
        };
      }

      return new PatternScanResult
      {
        BaseAddress = IntPtr.Zero,
        ReadAddress = IntPtr.Zero,
        Offset      = 0,
        Found       = false
      };
    }

    private PatternScanResult FindDataPattern(IMemoryPattern pattern, int hintAddr = 0)
    {
      var patternData  = Data;
      var patternBytes = pattern.GetBytes();
      var patternMask  = pattern.GetMask();
      
      if (hintAddr > 0)
        if (hintAddr + patternBytes.Count > patternData.Length
          || patternMask.AnyEx((m,
                                // ReSharper disable once AccessToModifiedClosure
                                b) => m == 'x' && patternBytes[b] != patternData[b + hintAddr]))
          hintAddr = 0;

      var result = new PatternScanResult();

      for (var offset = hintAddr; offset < patternData.Length; offset++)
      {
        if (patternMask.AnyEx((m,
                               // ReSharper disable once AccessToModifiedClosure
                               b) => m == 'x' && patternBytes[b] != patternData[b + offset]))
          continue;

        // If this area is reached, the pattern has been found.
        result.Found       = true;
        result.ReadAddress = _module.Read<IntPtr>(offset + pattern.Offset);
        result.BaseAddress = new IntPtr(result.ReadAddress.ToInt64() - _module.BaseAddress.ToInt64());
        result.Offset      = offset;
        return result;
      }

      // If this is reached, the pattern was not found.
      result.Found       = false;
      result.Offset      = 0;
      result.ReadAddress = IntPtr.Zero;
      result.BaseAddress = IntPtr.Zero;
      return result;
    }

    private PatternScanResult FindCallPattern(IMemoryPattern pattern, int hintAddr = 0)
    {
      var patternData  = Data;
      var patternBytes = pattern.GetBytes();
      var patternMask  = pattern.GetMask();
      
      if (hintAddr > 0)
        if (hintAddr + patternBytes.Count > patternData.Length
          || patternMask.AnyEx((m,
                                // ReSharper disable once AccessToModifiedClosure
                                b) => m == 'x' && patternBytes[b] != patternData[b + hintAddr]))
          hintAddr = 0;

      var result = new PatternScanResult();

      for (var offset = hintAddr; offset < patternData.Length; offset++)
      {
        if (patternMask.AnyEx((m,
                               // ReSharper disable once AccessToModifiedClosure
                               b) => m == 'x' && patternBytes[b] != patternData[b + offset]))
          continue;

        // If this area is reached, the pattern has been found.
        result.Found       = true;
        result.ReadAddress = _module.Read<IntPtr>(offset + pattern.Offset);
        result.BaseAddress = new IntPtr(result.ReadAddress.ToInt64() + pattern.Offset + MarshalType<IntPtr>.Size + offset + _module.BaseAddress.ToInt64());
        result.Offset      = offset;
        return result;
      }

      // If this is reached, the pattern was not found.
      result.Found       = false;
      result.Offset      = 0;
      result.ReadAddress = IntPtr.Zero;
      result.BaseAddress = IntPtr.Zero;
      return result;
    }

    #endregion
  }
}
