namespace Process.NET.Patterns
{
  public class DwordDataPattern : DwordPattern
  {
    #region Constructors

    public DwordDataPattern(string pattern,
                            int    offset)
      : base(GetBytesFromDwordPattern(pattern),
             GetMaskFromDwordPattern(pattern),
             offset,
             MemoryPatternType.Data,
             pattern) { }

    #endregion
  }
}
