using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Assistant.Scripts.Engine
{
  public class Variable
  {
    private string _value;

    public Variable(string value)
    {
      _value = value;
    }

    // Treat the argument as an integer
    public int AsInt()
    {
      if (_value == null)
        throw new RunTimeError("Cannot convert argument to int");

      // Try to resolve it as a scoped variable first
      var arg = Interpreter.GetVariable(_value);
      if (arg != null)
        return arg.AsInt();

      return TypeConverter.ToInt(_value);
    }

    // Treat the argument as an unsigned integer
    public uint AsUInt()
    {
      if (_value == null)
        throw new RunTimeError("Cannot convert argument to uint");

      // Try to resolve it as a scoped variable first
      var arg = Interpreter.GetVariable(_value);
      if (arg != null)
        return arg.AsUInt();

      return TypeConverter.ToUInt(_value);
    }

    public ushort AsUShort()
    {
      if (_value == null)
        throw new RunTimeError("Cannot convert argument to ushort");

      // Try to resolve it as a scoped variable first
      var arg = Interpreter.GetVariable(_value);
      if (arg != null)
        return arg.AsUShort();

      return TypeConverter.ToUShort(_value);
    }

    // Treat the argument as a serial or an alias. Aliases will
    // be automatically resolved to serial numbers.
    public uint AsSerial()
    {
      if (_value == null)
        throw new RunTimeError("Cannot convert argument to serial");

      // Try to resolve it as a scoped variable first
      var arg = Interpreter.GetVariable(_value);
      if (arg != null)
        return arg.AsSerial();

      // Resolve it as a global alias next
      uint serial = Interpreter.GetAlias(_value);
      if (serial != uint.MaxValue)
        return serial;

      try
      {
        return AsUInt();
      }
      catch (RunTimeError)
      { }

      return Serial.MinusOne;
    }

    public List<string> AsStringList(bool resolve = true, string caracter = "|")
    {
      var result = new List<string>();
      if (_value == null)
        throw new RunTimeError("Cannot convert argument to string");

      var rawValues = _value.Split(new string[] { caracter }, StringSplitOptions.RemoveEmptyEntries);
      foreach (var rawValue in rawValues)
      {
        if (resolve)
        {
          if (!rawValue.Contains("{{"))
          {
            var arg = Interpreter.GetVariable(rawValue);
            if (arg != null)
              result.Add(arg.AsString());
          }
          else
          {
            var arg = ResolvePipeVars(rawValue);
            result.Add(arg);
          }
        }
        else
        {
          result.Add(rawValue);
        }
      }

      return result;
    }    

    // Treat the argument as a string
    public string AsString(bool resolve = true)
    {
      if (_value == null)
        throw new RunTimeError("Cannot convert argument to string");

      if (resolve)
      {
        if (!_value.Contains("{{"))
        {
          var arg = Interpreter.GetVariable(_value);
          if (arg != null)
            return arg.AsString();
        }
        else
        {
          var arg = ResolvePipeVars(_value);
          return arg;
        }
      }

      return _value;
    }

    public static string ResolvePipeVars(string _value)
    {
      if (string.IsNullOrWhiteSpace(_value))
        return string.Empty;

      var parts = _value.Split(new[] { '|' }, StringSplitOptions.None);
      for (int i = 0; i < parts.Length; i++)
      {
        var token = parts[i]?.Trim() ?? string.Empty;

        // remove {{ ... }} se vier assim
        if (token.Length >= 4 &&
            token.StartsWith("{{", StringComparison.Ordinal) &&
            token.EndsWith("}}", StringComparison.Ordinal))
        {
          token = token.Substring(2, token.Length - 4).Trim();
        }

        var arg = Interpreter.GetVariable(token);

        // se existir variável, usa o valor; senão mantém o token original (sem chaves)
        parts[i] = arg != null ? arg.AsString() : token;
      }

      return string.Join("|", parts);
    }
    public bool AsBool()
    {
      if (_value == null)
        throw new RunTimeError("Cannot convert argument to bool");

      return TypeConverter.ToBool(_value);
    }

    public override bool Equals(object obj)
    {
      if (obj == null)
        return false;

      Variable arg = obj as Variable;

      if (arg == null)
        return false;

      return Equals(arg);
    }

    public bool Equals(Variable other)
    {
      if (other == null)
        return false;

      return (other._value == _value);
    }
  }
}
