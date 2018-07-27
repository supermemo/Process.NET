using System;
using System.Linq;
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

    public PatternScanResult Find(IMemoryPattern pattern)
    {
      switch (pattern.PatternType)
      {
        case MemoryPatternType.Function:
          return FindFunctionPattern(pattern);

        case MemoryPatternType.Data:
          return FindDataPattern(pattern);

        case MemoryPatternType.Call:
          return FindCallPattern(pattern);

        default:
          throw new InvalidOperationException($"Invalid MemoryPatternType {pattern.PatternType}");
      }
    }

    #endregion




    #region Methods

    private PatternScanResult FindFunctionPattern(IMemoryPattern pattern)
    {
      var patternData       = Data;
      var patternDataLength = patternData.Length;

      for (var offset = 0; offset < patternDataLength; offset++)
      {
        if (
          pattern.GetMask()
                 .Where((m,
                         b) => m == 'x' && pattern.GetBytes()[b] != patternData[b + offset])
                 .Any())
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

    private PatternScanResult FindDataPattern(IMemoryPattern pattern)
    {
      var patternData  = Data;
      var patternBytes = pattern.GetBytes();
      var patternMask  = pattern.GetMask();

      var result = new PatternScanResult();

      for (var offset = 0; offset < patternData.Length; offset++)
      {
        if (patternMask.Where((m,
                               b) => m == 'x' && patternBytes[b] != patternData[b + offset]).Any())
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

    private PatternScanResult FindCallPattern(IMemoryPattern pattern)
    {
      var patternData  = Data;
      var patternBytes = pattern.GetBytes();
      var patternMask  = pattern.GetMask();

      var result = new PatternScanResult();

      for (var offset = 0; offset < patternData.Length; offset++)
      {
        if (patternMask.Where((m,
                               b) => m == 'x' && patternBytes[b] != patternData[b + offset]).Any())
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
