namespace Process.NET.Patterns
{
  public class DwordCallPattern : DwordPattern
  {
    #region Constructors

    public DwordCallPattern(string pattern,
                            int    offset)
      : base(GetBytesFromDwordPattern(pattern),
             GetMaskFromDwordPattern(pattern),
             offset,
             MemoryPatternType.Call,
             pattern) { }

    #endregion
  }
}
