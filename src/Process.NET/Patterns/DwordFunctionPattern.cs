namespace Process.NET.Patterns
{
  public class DwordFunctionPattern : DwordPattern
  {
    #region Constructors

    public DwordFunctionPattern(string pattern)
      : base(GetBytesFromDwordPattern(pattern),
             GetMaskFromDwordPattern(pattern),
             0,
             MemoryPatternType.Function,
             pattern) { }

    #endregion
  }
}
