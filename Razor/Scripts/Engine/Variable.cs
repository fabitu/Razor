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

    // Treat the argument as a string
    public string AsString(bool resolve = true)
    {
      if (_value == null)
        throw new RunTimeError("Cannot convert argument to string");

      if (resolve)
      {
        //if (_value.Equals("bank", StringComparison.OrdinalIgnoreCase))
        //{
        //  var serial = World.Player?.Bank?.Serial.Value.ToString();
        //  if (serial != null)
        //    return serial;
        //}
        // Try to resolve it as a scoped variable first
        var arg = Interpreter.GetVariable(_value);
        if (arg != null)
          return arg.AsString();
      }

      return _value;
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
