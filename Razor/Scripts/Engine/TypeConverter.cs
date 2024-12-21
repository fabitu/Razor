using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assistant.Scripts.Engine
{
  internal static class TypeConverter
  {
    public static int ToInt(string token)
    {
      int val;

      if (token.StartsWith("0x"))
      {
        if (int.TryParse(token.Substring(2), NumberStyles.HexNumber, Interpreter.Culture, out val))
          return val;
      }
      else if (int.TryParse(token, out val))
        return val;

      throw new RunTimeError("Cannot convert argument to int");
    }

    public static uint ToUInt(string token)
    {
      uint val;

      if (token.StartsWith("0x"))
      {
        if (uint.TryParse(token.Substring(2), NumberStyles.HexNumber, Interpreter.Culture, out val))
          return val;
      }
      else if (uint.TryParse(token, out val))
        return val;

      throw new RunTimeError("Cannot convert argument to uint");
    }

    public static ushort ToUShort(string token)
    {
      ushort val;

      if (token.StartsWith("0x"))
      {
        if (ushort.TryParse(token.Substring(2), NumberStyles.HexNumber, Interpreter.Culture, out val))
          return val;
      }
      else if (ushort.TryParse(token, out val))
        return val;

      throw new RunTimeError("Cannot convert argument to ushort");
    }

    public static double ToDouble(string token)
    {
      double val;

      if (double.TryParse(token, out val))
        return val;

      throw new RunTimeError("Cannot convert argument to double");
    }

    public static bool ToBool(string token)
    {
      bool val;

      if (bool.TryParse(token, out val))
        return val;

      throw new RunTimeError("Cannot convert argument to bool");
    }
  }
}
