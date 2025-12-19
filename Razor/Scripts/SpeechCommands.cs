#region license
// Razor: An Ultima Online Assistant
// Copyright (c) 2022 Razor Development Community on GitHub <https://github.com/markdwags/Razor>
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.
#endregion

using Assistant.Scripts.Engine;
using Assistant.Scripts.Helpers;
using System;

namespace Assistant.Scripts
{
  public static class SpeechCommands
  {
    public static void Register()
    {
      // Messages
      Interpreter.RegisterCommandHandler("say", Say);
      Interpreter.RegisterCommandHandler("msg", Say);
      Interpreter.RegisterCommandHandler("yell", Yell);
      Interpreter.RegisterCommandHandler("whisper", Whisper);
      Interpreter.RegisterCommandHandler("emote", Emote);
      Interpreter.RegisterCommandHandler("guild", Guild);
      Interpreter.RegisterCommandHandler("alliance", Alliance);
    }

    public static bool Say(string command, Variable[] vars, bool quiet, bool force)
    {
      if (vars.Length == 0)
      {
        throw new RunTimeError("Usage: say ('text') [color]");
      }
      var msg = CommandHelper.ReplaceStringInterpolations(vars[0].AsString());
      if (vars.Length == 1)
        World.Player.Say(Config.GetInt("SysColor"), msg);
      else
        World.Player.Say(Utility.ToInt32(vars[1].AsString(), 0), msg);

      return true;
    }

    public static bool Whisper(string command, Variable[] vars, bool quiet, bool force)
    {
      if (vars.Length == 0)
      {
        throw new RunTimeError("Usage: whisper ('text') [color]");
      }

      MessageType type = MessageType.Whisper & ~MessageType.Encoded;
      var msg = CommandHelper.ReplaceStringInterpolations(vars[0].AsString());
      if (vars.Length == 1)
        World.Player.Whisper(msg, World.Player.SpeechHue);
      else
        World.Player.Whisper(msg, Utility.ToInt32(vars[1].AsString(), 0));

      return true;
    }

    public static bool Yell(string command, Variable[] vars, bool quiet, bool force)
    {
      if (vars.Length == 0)
      {
        throw new RunTimeError("Usage: yell ('text') [color]");
      }
      var msg = CommandHelper.ReplaceStringInterpolations(vars[0].AsString());
      if (vars.Length == 1)
        World.Player.Yell(msg, World.Player.SpeechHue);
      else
        World.Player.Yell(msg, Utility.ToInt32(vars[1].AsString(), 0));

      return true;
    }

    public static bool Emote(string command, Variable[] vars, bool quiet, bool force)
    {
      if (vars.Length == 0)
      {
        throw new RunTimeError("Usage: emote ('text') [color]");
      }
      var msg = CommandHelper.ReplaceStringInterpolations(vars[0].AsString());

      if (vars.Length == 1)
        World.Player.Emote(msg, World.Player.SpeechHue);
      else
        World.Player.Emote(msg, Utility.ToInt32(vars[1].AsString(), 0));

      return true;
    }

    public static bool Guild(string command, Variable[] vars, bool quiet, bool force)
    {
      if (vars.Length == 0)
      {
        throw new RunTimeError("Usage: guild ('text')");
      }
      var msg = CommandHelper.ReplaceStringInterpolations(vars[0].AsString());
      if (vars.Length == 1)
        World.Player.Guild(msg, World.Player.SpeechHue);
      else
        World.Player.Guild(msg, Utility.ToInt32(vars[1].AsString(), 0));

      return true;
    }

    public static bool Alliance(string command, Variable[] vars, bool quiet, bool force)
    {
      if (vars.Length == 0)
      {
        throw new RunTimeError("Usage: alliance ('text')");
      }
      var msg = CommandHelper.ReplaceStringInterpolations(vars[0].AsString());
      if (vars.Length == 1)
        World.Player.Alliance(msg, World.Player.SpeechHue);
      else
        World.Player.Alliance(msg, Utility.ToInt32(vars[1].AsString(), 0));

      return true;
    }
  }
}
