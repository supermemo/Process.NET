using System;

namespace Process.NET.Extensions
{
  public static class StringEx
  {
    public static bool AnyEx(this string s, Func<char, int, bool> predicate)
    {
      for (int i = 0; i < s.Length; i++)
        if (predicate(s[i], i))
          return true;

      return false;
    }
  }
}
