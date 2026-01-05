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
using System.Collections.Generic;
using System.Threading;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Rebar;
using static Ultima.FrameEdit;

namespace Assistant.Scripts
{
  public static class TargetCommands
  {
    public static void Register()
    {
      // Targets
      Interpreter.RegisterCommandHandler("target", Target); //Absolute Target

      Interpreter.RegisterCommandHandler("targettype", TargetType); //TargetTypeAction
      Interpreter.RegisterCommandHandler("targetrelloc", TargetRelLoc); //TargetRelLocAction
      Interpreter.RegisterCommandHandler("targetloc", TargetLocation);

      Interpreter.RegisterCommandHandler("waitfortarget", WaitForTarget); //WaitForTargetAction
      Interpreter.RegisterCommandHandler("wft", WaitForTarget); //WaitForTargetAction
    }

    private static bool Target(string command, Variable[] vars, bool quiet, bool force)
    {
      Mobile _mobile;
      if (vars.Length < 1)
      {
        throw new RunTimeError("Usage: target (serial) OR target (closest/random/next/prev [noto] [type]");
      }

      switch (vars[0].AsString())
      {
        case "self":
          _mobile = World.FindMobile(World.Player.Serial);

          if (_mobile != null)
          {
            Targeting.Target(_mobile);
          }

          break;
        case "last":
          if (Targeting.HasTarget && Targeting.LastTargetInfo != null)
          {
            _mobile = World.FindMobile(Targeting.LastTargetInfo.Serial);

            if (_mobile != null)
            {
              Targeting.Target(_mobile);
            }
          }
          else
          {
            // Targeting.
          }
          break;
        case "close":
        case "closest":
          CommandHelper.FindTarget(vars, true);

          break;

        case "rand":
        case "random":
          CommandHelper.FindTarget(vars, false, true);

          break;

        case "next":
          CommandHelper.FindTarget(vars, false, false, true);

          break;

        case "prev":
        case "previous":
          CommandHelper.FindTarget(vars, false, false, false, true);

          break;

        case "cancel":
          Targeting.CancelTarget();

          break;

        case "clear":
          Targeting.OnClearQueue();

          break;
        default:
          Serial serial = vars[0].AsSerial();

          if (serial != Serial.Zero) // Target a specific item or mobile
          {
            Item item = World.FindItem(serial);

            if (item != null)
            {
              Targeting.Target(item);
              return true;
            }

            _mobile = World.FindMobile(serial);

            if (_mobile != null)
            {
              Targeting.Target(_mobile);
            }
          }

          break;
      }

      return true;
    }

    private static bool TargetType(string command, Variable[] vars, bool quiet, bool force)
    {
      if (Targeting.FromGrabHotKey)
        return false;

      if (vars.Length < 1)
      {
        throw new RunTimeError("Usage: targettype ('name of item or mobile type'/'graphicId') [inrangecheck (true/false)/backpack] [hue]");
      }

      string gfxStr = vars[0].AsString();
      Serial gfx = Utility.ToUInt16(gfxStr, 0);
      List<Item> items;
      List<Mobile> mobiles = new List<Mobile>();

      bool inRangeCheck = false;
      bool backpack = false;
      bool bank = false;
      int hue = -1;

      if (vars.Length > 1)
      {
        if (vars.Length == 3)
        {
          hue = vars[2].AsInt();
        }

        if (vars[1].AsString().IndexOf("pack", StringComparison.OrdinalIgnoreCase) != -1)
        {
          backpack = true;
        }
        else if (vars[1].AsString().Equals("bank", StringComparison.OrdinalIgnoreCase))
        {
          bank = true;
        }
        else
        {
          inRangeCheck = vars[1].AsBool();
        }
      }

      // No graphic id, maybe searching by name?
      if (gfx == 0)
      {
        items = CommandHelper.GetItemsByName(gfxStr, backpack, bank, inRangeCheck, hue);

        if (items.Count == 0) // no item found, search mobile by name
        {
          mobiles = CommandHelper.GetMobilesByName(gfxStr, inRangeCheck);
        }
      }
      else // Provided graphic id for type, check backpack first (same behavior as DoubleClickAction in macros
      {
        ushort id = Utility.ToUInt16(gfxStr, 0);

        items = CommandHelper.GetItemsById(id, backpack, bank, inRangeCheck, hue);

        // Still no item? Mobile check!
        if (items.Count == 0)
        {
          mobiles = CommandHelper.GetMobilesById(id, inRangeCheck);
        }
      }

      if (items.Count > 0)
      {
        Targeting.Target(items[Utility.Random(items.Count)]);
      }
      else if (mobiles.Count > 0)
      {
        Targeting.Target(mobiles[Utility.Random(mobiles.Count)]);
      }
      else
      {
        CommandHelper.SendWarning(command, $"Item or mobile type '{gfxStr}' not found", quiet);
      }

      return true;
    }

    private static bool TargetRelLoc(string command, Variable[] vars, bool quiet, bool force)
    {
      if (Targeting.FromGrabHotKey)
        return false;

      if (vars.Length < 2)
      {
        throw new RunTimeError("Usage: targetrelloc (x-offset) (y-offset) [defaultID]");
      }

      int xoffset = Utility.ToInt32(vars[0].AsString(), 0);
      int yoffset = Utility.ToInt32(vars[1].AsString(), 0);
      ushort defaultId = vars.Length == 3 ? vars[2].AsUShort() : (ushort)0;

      ushort x = (ushort)(World.Player.Position.X + xoffset);
      ushort y = (ushort)(World.Player.Position.Y + yoffset);
      short z = (short)World.Player.Position.Z;

      try
      {
        Ultima.HuedTile tile = Map.GetTileNear(World.Player.Map, x, y, z, defaultId);
        Targeting.Target(new Point3D(x, y, tile.Z), tile.ID);
      }
      catch (Exception e)
      {
        throw new RunTimeError($"{command} - Error Executing: {e.Message}");
      }

      return true;
    }

    private static bool TargetLocation(string command, Variable[] vars, bool quiet, bool force)
    {
      if (vars.Length < 2)
      {
        throw new RunTimeError("Usage: targetloc (x) (y) (z)");
      }

      Targeting.Target(new TargetInfo
      {
        Type = 1,
        Flags = 0,
        Serial = Serial.Zero,
        X = vars[0].AsInt(),
        Y = vars[1].AsInt(),
        Z = vars.Length == 3 ? vars[2].AsInt() : 0,
        Gfx = vars.Length == 4 ? vars[3].AsUShort() : (ushort)0
      });

      return true;
    }

    private static bool WaitForTarget(string command, Variable[] vars, bool quiet, bool force)
    {
      var result = false;
      uint timeout = 30000;
      if (Targeting.HasTarget)
      {
        Interpreter.ClearTimeout();
        return true;
      }

      if (vars.Length > 0)
      {
        string raw = vars[0].AsString().Trim().ToLowerInvariant();

        // separa número e sufixo
        int i = 0;
        while (i < raw.Length && char.IsDigit(raw[i]))
          i++;

        if (i == 0)
          throw new RunTimeError("Invalid timeout value");

        timeout = uint.Parse(raw.Substring(0, i));
        timeout = CommandHelper.SetTimeOut(raw, i, timeout);

        if (vars.Length > 1)
        {
          var ignoreWait = vars[1].AsBool();
          result = ignoreWait;
          if (ignoreWait)
          {
            if (vars.Length > 2)
            {
              var message = vars[2].AsString();
              World.Player.SendMessage(MsgLevel.Info, message);
            }
          }
        }
      }

      Interpreter.Timeout(vars.Length > 0 ? timeout : 30000, () => { return true; });


      return result;
    }
  }
}