using System;
using System.Collections.Generic;
using System.Text;

namespace Process.NET.Utilities
{
  public class StringAndEncoding
  {
    public StringAndEncoding(string   text,
                             Encoding encoding)
    {
      Text = text;
      Encoding = encoding;
    }

    public string Text { get; set; }
    public Encoding Encoding { get; set; }

    public override string ToString()
    {
      return Text;
    }
  }
}
