using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Process.NET.Patterns
{
  public abstract class DwordPattern : IMemoryPattern
  {
    #region Properties & Fields - Non-Public

    protected readonly byte[] _bytes;
    protected readonly string _mask;

    #endregion




    #region Constructors

    protected DwordPattern(
      byte[]            bytes,
      string            mask,
      int               offset,
      MemoryPatternType type,
      string            pattern)
    {
      _bytes      = bytes;
      _mask       = mask;
      Offset      = offset;
      PatternType = type;
      PatternText = pattern;
    }

    #endregion




    #region Properties & Fields - Public

    public string PatternText { get; }

    #endregion




    #region Properties Impl - Public

    public int               Offset      { get; }
    public MemoryPatternType PatternType { get; }

    #endregion




    #region Methods Impl

    public override string ToString()
    {
      return PatternText;
    }

    public IList<byte> GetBytes()
    {
      return _bytes;
    }

    public string GetMask()
    {
      return _mask;
    }

    #endregion




    #region Methods

    protected static string GetMaskFromDwordPattern(string pattern)
    {
      var mask = pattern.Split(' ').Select(s => s.Contains('?') ? "?" : "x");

      return string.Concat(mask);
    }

    protected static byte[] GetBytesFromDwordPattern(string pattern)
    {
      return
        pattern.Split(' ')
               .Select(s => s.Contains('?')
                         ? (byte)0
                         : byte.Parse(s,
                                      NumberStyles.HexNumber))
               .ToArray();
    }

    #endregion
  }
}
