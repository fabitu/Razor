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

using Assistant.Filters;
using Assistant.Scripts.Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Rebar;

namespace Assistant.Scripts.Helpers
{
  public static class CommandHelper
  {

    /// <summary>
    /// Common logic for dclicktype and targettype to find items by name
    /// </summary>
    /// <param name="name"></param>
    /// <param name="backpack"></param>
    /// <param name="inRange"></param>
    /// <param name="hue"></param>
    /// <returns></returns>
    public static int GetTotalItemsByName(string name, bool backpack, bool bank, Serial container, int hue)
    {
      List<Item> items = new List<Item>();

      if (backpack && World.Player.Backpack != null) // search backpack only
      {
        items.AddRange(World.Player.Backpack.FindItemsByName(name, true).Where(item => !Interpreter.CheckIgnored(item.Serial)));
        var itemRightHand = World.Player.GetItemOnLayer(Layer.RightHand);
        var itemLeftHand = World.Player.GetItemOnLayer(Layer.LeftHand);
        if (itemRightHand?.Name.Equals(name) == true)
        {
          items.Add(itemRightHand);
        }
        if (itemLeftHand?.Name.Equals(name) == true)
        {
          items.Add(itemLeftHand);
        }
      }
      else if (bank) // search backpack only
      {
        if (World.Player.Bank == null)
        {
          World.Player.Say(Config.GetInt("SysColor"), "Bank");
        }
        if (World.Player.Bank != null)
          items.AddRange(World.Player.Bank.FindItemsByName(name, true).Where(item => !Interpreter.CheckIgnored(item.Serial)));
      }
      else
      {
        var findContainer = World.FindItem(container);
        if (findContainer != null)
        {
          var itens = findContainer.Contains.Where(x => x.Name == name && Interpreter.CheckIgnored(x.Serial)).ToList();
          items.AddRange(itens);
        }
      }

      if (hue > -1)
      {
        items.RemoveAll(item => item.Hue != hue);
      }

      return items.Count();
    }

    public static uint SetTimeOut(string raw, int i, uint timeout)
    {
      string suffix = raw.Substring(i);

      switch (suffix)
      {
        case "":
          // default: milliseconds
          break;

        case "s":
        case "sec":
        case "secs":
        case "second":
        case "seconds":
          timeout *= 1000;
          break;

        case "m":
        case "min":
        case "mins":
        case "minute":
        case "minutes":
          timeout *= 60000;
          break;

        default:
          throw new RunTimeError($"Invalid time unit: {suffix}");
      }

      return timeout;
    }
    /// <summary>
    /// Common logic for dclicktype and targettype to find items by name
    /// </summary>
    /// <param name="name"></param>
    /// <param name="backpack"></param>
    /// <param name="inRange"></param>
    /// <param name="hue"></param>
    /// <returns></returns>
    public static List<Item> GetItemsByName(string name, bool backpack, bool bank, bool inRange, int hue)
    {
      List<Item> items = new List<Item>();

      if (backpack && World.Player.Backpack != null) // search backpack only
      {
        items.AddRange(World.Player.Backpack.FindItemsByName(name, true).Where(item => !Interpreter.CheckIgnored(item.Serial)));
      }
      else if (bank) // search backpack only
      {
        if (World.Player.Bank == null)
        {
          World.Player.Say(Config.GetInt("SysColor"), "Bank");
        }
        if (World.Player.Bank != null)
          items.AddRange(World.Player.Bank.FindItemsByName(name, true).Where(item => !Interpreter.CheckIgnored(item.Serial)));
      }
      else if (inRange) // inrange includes both backpack and within 2 tiles
      {
        items.AddRange(World.FindItemsByName(name).Where(item =>
            !item.IsInBank && !Interpreter.CheckIgnored(item.Serial) && (Utility.InRange(World.Player.Position, item.Position, 2) ||
                               item.RootContainer == World.Player)));
      }
      else
      {
        items.AddRange(World.FindItemsByName(name).Where(item => !item.IsInBank && !Interpreter.CheckIgnored(item.Serial)));
      }

      if (hue > -1)
      {
        items.RemoveAll(item => item.Hue != hue);
      }

      return items;
    }


    /// <summary>
    /// Common logic for dclicktype and targettype to find items by name
    /// </summary>
    /// <param name="name"></param>
    /// <param name="backpack"></param>
    /// <param name="inRange"></param>
    /// <param name="hue"></param>
    /// <returns></returns>
    public static int GetTotalItemsById(ushort id, bool backpack, bool bank, Serial container, int hue)
    {
      List<Item> items = new List<Item>();

      if (backpack && World.Player.Backpack != null) // search backpack only
      {
        items.AddRange(World.Player.Backpack.FindItemsById(id, true).Where(item => !Interpreter.CheckIgnored(item.Serial)));
        var itemRightHand = World.Player.GetItemOnLayer(Layer.RightHand);
        var itemLeftHand = World.Player.GetItemOnLayer(Layer.LeftHand);
        if (itemRightHand?.ItemID.Equals(id) == true)
        {
          items.Add(itemRightHand);
        }
        if (itemLeftHand?.ItemID.Equals(id) == true)
        {
          items.Add(itemLeftHand);
        }
      }
      else if (bank) // search backpack only
      {
        if (World.Player.Bank == null)
        {
          World.Player.Say(Config.GetInt("SysColor"), "Bank");
        }
        if (World.Player.Bank != null)
          items.AddRange(World.Player.Bank.FindItemsById(id, true).Where(item => !Interpreter.CheckIgnored(item.Serial)));
      }
      else
      {
        var findContainer = World.FindItem(container);
        if (findContainer != null)
        {
          var itens = findContainer.Contains.Where(x => x.ItemID == id && !Interpreter.CheckIgnored(x.Serial)).ToList();
          items.AddRange(itens);
        }
      }

      if (hue > -1)
      {
        items.RemoveAll(item => item.Hue != hue);
      }

      return items.Count();
    }
    /// <summary>
    /// Common logic for dclicktype and targettype to find items by id
    /// </summary>
    /// <param name="id"></param>
    /// <param name="backpack"></param>
    /// <param name="inRange"></param>
    /// <param name="hue"></param>
    /// <returns></returns>
    public static List<Item> GetItemsById(ushort id, bool backpack, bool bank, bool inRange, int hue)
    {
      List<Item> items = new List<Item>();

      if (backpack && World.Player.Backpack != null)
      {
        var itemRightHand = World.Player.GetItemOnLayer(Layer.RightHand);
        var itemLeftHand = World.Player.GetItemOnLayer(Layer.LeftHand);
        if (itemRightHand?.ItemID.Equals(id) == true)
        {
          items.Add(itemRightHand);
        }
        else if (itemLeftHand?.ItemID.Equals(id) == true)
        {
          items.Add(itemLeftHand);
        }
        else
        {
          items.AddRange(World.Player.Backpack.FindItemsById(id, true).Where(item => !Interpreter.CheckIgnored(item.Serial)));
        }
      }
      else if (bank) // search backpack only
      {
        if (World.Player.Bank == null)
        {
          World.Player.Say(Config.GetInt("SysColor"), "Bank");
        }
        if (World.Player.Bank != null)
          items.AddRange(World.Player.Bank.FindItemsById(id, true).Where(item => !Interpreter.CheckIgnored(item.Serial)));
      }
      else if (inRange)
      {
        items.AddRange(World.FindItemsById(id).Where(item =>
            !item.IsInBank && !Interpreter.CheckIgnored(item.Serial) && (Utility.InRange(World.Player.Position, item.Position, 2) ||
                                                                      item.RootContainer == World.Player)));
      }
      else
      {
        items.AddRange(World.FindItemsById(id).Where(item => !item.IsInBank && !Interpreter.CheckIgnored(item.Serial)));
      }

      if (hue > -1)
      {
        items.RemoveAll(item => item.Hue != hue);
      }

      return items;
    }

    /// <summary>
    /// Common logic for dclicktype and targettype to find items by id
    /// </summary>
    /// <param name="id"></param>
    /// <param name="backpack"></param>
    /// <param name="inRange"></param>
    /// <param name="hue"></param>
    /// <returns></returns>
    public static int GetCountItemsById(ushort id, bool backpack, bool bank, Serial container, int hue)
    {
      List<Item> items = new List<Item>();

      if (backpack && World.Player.Backpack != null)
      {
        items.AddRange(World.Player.Backpack.FindItemsById(id, true).Where(item => !Interpreter.CheckIgnored(item.Serial)));

        var itemRightHand = World.Player.GetItemOnLayer(Layer.RightHand);
        var itemLeftHand = World.Player.GetItemOnLayer(Layer.LeftHand);
        if (itemRightHand?.ItemID == id)
        {
          items.Add(itemRightHand);
        }
        if (itemLeftHand?.ItemID == id)
        {
          items.Add(itemLeftHand);
        }
      }
      else if (bank) // search backpack only
      {
        if (World.Player.Bank == null)
        {
          World.Player.Say(Config.GetInt("SysColor"), "Bank");
        }
        if (World.Player.Bank != null)
          items.AddRange(World.Player.Bank.FindItemsById(id, true).Where(item => !Interpreter.CheckIgnored(item.Serial)));
      }
      else
      {
        var findContainer = World.FindItem(container);
        if (findContainer != null)
        {
          var itens = findContainer.Contains.Where(x => x.ItemID == (ItemID)id && Interpreter.CheckIgnored(x.Serial)).ToList();
          items.AddRange(itens);
        }
      }

      if (hue > -1)
      {
        items.RemoveAll(item => item.Hue != hue);
      }

      return items.Count();
    }

    /// <summary>
    /// Common logic for dclicktype and targettype to find mobiles by name
    /// </summary>
    /// <param name="name"></param>
    /// <param name="inRange"></param>
    /// <returns></returns>
    public static List<Mobile> GetMobilesByName(string name, bool inRange)
    {
      List<Mobile> mobiles = new List<Mobile>();

      mobiles.AddRange(inRange
          ? World.FindMobilesByName(name).Where(m => !Interpreter.CheckIgnored(m.Serial) && Utility.InRange(World.Player.Position, m.Position, 2))
          : World.FindMobilesByName(name).Where(m => !Interpreter.CheckIgnored(m.Serial)));

      return mobiles;
    }

    public static string GetNamedParameter(string input, string parameterName)
    {
      if (string.IsNullOrWhiteSpace(input) || string.IsNullOrWhiteSpace(parameterName))
        return null;

      var parts = input.Split(new string[] { "|" }, StringSplitOptions.RemoveEmptyEntries);

      foreach (var part in parts)
      {
        var idx = part.IndexOf(':');
        if (idx <= 0)
          continue;

        var name = part.Substring(0, idx).Trim();
        if (!name.Equals(parameterName, StringComparison.OrdinalIgnoreCase))
          continue;

        return part.Substring(idx + 1).Trim();
      }

      return null;
    }

    /// <summary>
    /// Common logic for dclicktype and targettype to find mobiles by id
    /// </summary>
    /// <param name="id"></param>
    /// <param name="inRange"></param>
    /// <returns></returns>
    public static List<Mobile> GetMobilesById(ushort id, bool inRange)
    {
      List<Mobile> mobiles = new List<Mobile>();

      mobiles.AddRange(inRange
          ? World.MobilesInRange().Where(m =>
              Utility.InRange(World.Player.Position, m.Position, 2) && m.Body == id &&
              !Interpreter.CheckIgnored(m.Serial))
          : World.MobilesInRange().Where(m => m.Body == id && !Interpreter.CheckIgnored(m.Serial)));

      return mobiles;
    }

    public static void SendWarning(string command, string message, bool quiet)
    {
      if (!quiet)
      {
        World.Player.SendMessage(MsgLevel.Warning, $"{command} - {message}");
      }
    }
    public static (string graphic, int hue) ParseGraphicAndHue(string value)
    {
      if (string.IsNullOrWhiteSpace(value))
        return (string.Empty, -1);

      var parts = value.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);

      var graphic = parts[0];
      var hue = parts.Length > 1 && int.TryParse(parts[1], out var h) ? h : -1;

      return (graphic, hue);
    }
    public static void SendMessage(string message, bool quiet)
    {
      if (!quiet)
      {
        World.Player.SendMessage(MsgLevel.Force, message);
      }
    }

    public static void SendInfo(string message, bool quiet)
    {
      if (!quiet)
      {
        World.Player.SendMessage(MsgLevel.Info, message);
      }
    }

    /// <summary>
    /// Parse the script input to target the correct mobile
    /// </summary>
    /// <param name="args"></param>
    /// <param name="closest"></param>
    /// <param name="random"></param>
    /// <param name="next"></param>
    /// <param name="prev"></param>
    public static void FindTarget(Variable[] args, bool closest, bool random = false, bool next = false, bool prev = false)
    {
      ScriptManager.TargetFound = false;

      // Do a basic t
      if (args.Length == 1)
      {
        if (closest)
        {
          Targeting.TargetClosest();
        }
        else if (random)
        {
          Targeting.TargetRandAnyone();
        }
        else if (next)
        {
          Targeting.NextTarget();
        }
        else
        {
          Targeting.PrevTarget();
        }
      }
      else if ((next || prev) && args.Length == 2)
      {
        switch (args[1].AsString())
        {
          case "human":
          case "humanoid":

            if (next)
            {
              Targeting.NextTargetHumanoid();
            }
            else
            {
              Targeting.PrevTargetHumanoid();
            }

            break;
          case "monster":
            if (next)
            {
              Targeting.NextTargetMonster();
            }
            else
            {
              Targeting.PrevTargetMonster();
            }

            break;
          case "friend":
            if (next)
            {
              Targeting.NextTargetFriend();
            }
            else
            {
              Targeting.PrevTargetFriend();
            }

            break;
          case "friendly":
            if (next)
            {
              Targeting.NextTargetFriendly();
            }
            else
            {
              Targeting.PrevTargetFriendly();
            }

            break;
          case "nonfriendly":
            if (next)
            {
              Targeting.NextTargetNonFriend();
            }
            else
            {
              Targeting.PrevTargetNonFriend();
            }

            break;
          default:
            throw new RunTimeError($"Unknown target type: '{args[1].AsString()}' - Missing type? (human/monster)");
        }
      }
      else if (args.Length > 1)
      {
        string list = args[1].AsString();

        if (list.IndexOf('!') != -1)
        {
          FindTargetPriority(args, closest, random, next);
        }
        else if (list.IndexOf(',') != -1)
        {
          FindTargetNotoriety(args, closest, random, next);
        }
        else
        {
          FindTargetPriority(args, closest, random, next);
        }
      }
    }

    /// <summary>
    /// Find targets based on notoriety
    /// </summary>
    /// <param name="args"></param>
    /// <param name="closest"></param>
    /// <param name="random"></param>
    /// <param name="next"></param>
    private static void FindTargetNotoriety(Variable[] args, bool closest, bool random, bool next)
    {
      string[] notoList = args[1].AsString().Split(',');

      List<int> notoTypes = new List<int>();

      foreach (string noto in notoList)
      {
        Targeting.TargetType type = (Targeting.TargetType)Enum.Parse(typeof(Targeting.TargetType), noto, true);

        /*NonFriendly, //Attackable, Criminal, Enemy, Murderer
        Friendly, //Innocent, Guild/Ally 
        Red, //Murderer
        Blue, //Innocent
        Gray, //Attackable, Criminal
        Grey, //Attackable, Criminal
        Green, //GuildAlly
        Guild, //GuildAlly*/

        switch (type)
        {
          case Targeting.TargetType.Friendly:
            notoTypes.Add((int)Targeting.TargetType.Innocent);
            notoTypes.Add((int)Targeting.TargetType.GuildAlly);
            break;
          case Targeting.TargetType.NonFriendly:
            notoTypes.Add((int)Targeting.TargetType.Attackable);
            notoTypes.Add((int)Targeting.TargetType.Criminal);
            notoTypes.Add((int)Targeting.TargetType.Enemy);
            notoTypes.Add((int)Targeting.TargetType.Murderer);
            break;
          case Targeting.TargetType.Red:
            notoTypes.Add((int)Targeting.TargetType.Murderer);
            break;
          case Targeting.TargetType.Blue:
            notoTypes.Add((int)Targeting.TargetType.Innocent);
            break;
          case Targeting.TargetType.Gray:
          case Targeting.TargetType.Grey:
            notoTypes.Add((int)Targeting.TargetType.Attackable);
            notoTypes.Add((int)Targeting.TargetType.Criminal);
            break;
          case Targeting.TargetType.Green:
          case Targeting.TargetType.Guild:
            notoTypes.Add((int)Targeting.TargetType.GuildAlly);
            break;
          default:
            notoTypes.Add((int)type);
            break;
        }
      }

      if (args.Length == 3)
      {
        if (args[2].AsString().IndexOf("human", StringComparison.OrdinalIgnoreCase) != -1)
        {
          if (closest)
          {
            Targeting.ClosestHumanoidTarget(notoTypes.ToArray());
          }
          else if (random)
          {
            Targeting.RandomHumanoidTarget(notoTypes.ToArray());
          }
          else if (next)
          {
            Targeting.NextPrevTargetNotorietyHumanoid(true, notoTypes.ToArray());
          }
          else
          {
            Targeting.NextPrevTargetNotorietyHumanoid(false, notoTypes.ToArray());
          }
        }
        else if (args[2].AsString().IndexOf("monster", StringComparison.OrdinalIgnoreCase) != -1)
        {
          if (closest)
          {
            Targeting.ClosestMonsterTarget(notoTypes.ToArray());
          }
          else if (random)
          {
            Targeting.RandomMonsterTarget(notoTypes.ToArray());
          }
          else if (next)
          {
            Targeting.NextPrevTargetNotorietyMonster(true, notoTypes.ToArray());
          }
          else
          {
            Targeting.NextPrevTargetNotorietyMonster(false, notoTypes.ToArray());
          }
        }
      }
      else
      {
        if (closest)
        {
          Targeting.ClosestTarget(notoTypes.ToArray());
        }
        else if (random)
        {
          Targeting.RandomTarget(notoTypes.ToArray());
        }
        else if (next)
        {
          Targeting.NextPrevTargetNotoriety(true, notoTypes.ToArray());
        }
        else
        {
          Targeting.NextPrevTargetNotoriety(false, notoTypes.ToArray());
        }
      }
    }

    /// <summary>
    /// Find a target based on a priority list of notorieties 
    /// </summary>
    /// <param name="args"></param>
    /// <param name="closest"></param>
    /// <param name="random"></param>
    /// <param name="next"></param>
    private static void FindTargetPriority(Variable[] args, bool closest, bool random, bool next)
    {
      string[] notoList = args[1].AsString().Split('!');

      foreach (string noto in notoList)
      {
        if (ScriptManager.TargetFound)
        {
          break;
        }

        switch (noto)
        {
          case "enemy":
            if (args.Length == 3)
            {
              if (args[2].AsString().IndexOf("human", StringComparison.OrdinalIgnoreCase) != -1)
              {
                if (closest)
                {
                  Targeting.TargetCloseEnemyHumanoid();
                }
                else if (random)
                {
                  Targeting.TargetRandEnemyHumanoid();
                }
                else if (next)
                {
                  Targeting.NextTargetEnemyHumanoid();
                }
                else
                {
                  Targeting.PrevTargetEnemyHumanoid();
                }
              }
              else if (args[2].AsString()
                  .IndexOf("monster", StringComparison.OrdinalIgnoreCase) != -1)
              {
                if (closest)
                {
                  Targeting.TargetCloseEnemyMonster();
                }
                else if (random)
                {
                  Targeting.TargetRandEnemyMonster();
                }
                else if (next)
                {
                  Targeting.NextTargetEnemyMonster();
                }
                else
                {
                  Targeting.PrevTargetEnemyMonster();
                }
              }
            }
            else
            {
              if (closest)
              {
                Targeting.TargetCloseEnemy();
              }
              else if (random)
              {
                Targeting.TargetRandEnemy();
              }
            }

            break;
          case "friend":
            if (closest)
            {
              Targeting.TargetClosestFriend();
            }
            else if (random)
            {
              Targeting.TargetRandFriend();
            }
            else if (next)
            {
              Targeting.NextTargetFriend();
            }
            else
            {
              Targeting.PrevTargetFriend();
            }

            break;
          case "friendly":
            if (args.Length == 3)
            {
              if (args[2].AsString().IndexOf("human", StringComparison.OrdinalIgnoreCase) != -1)
              {
                if (closest)
                {
                  Targeting.TargetCloseFriendlyHumanoid();
                }
                else if (random)
                {
                  Targeting.TargetRandFriendlyHumanoid();
                }
                else if (next)
                {
                  Targeting.NextTargetFriendlyHumanoid();
                }
                else
                {
                  Targeting.PrevTargetFriendlyHumanoid();
                }
              }
              else if (args[2].AsString()
                  .IndexOf("monster", StringComparison.OrdinalIgnoreCase) != -1)
              {
                if (closest)
                {
                  Targeting.TargetCloseFriendlyMonster();
                }
                else if (random)
                {
                  Targeting.TargetRandFriendlyMonster();
                }
                else if (next)
                {
                  Targeting.NextTargetFriendlyMonster();
                }
                else
                {
                  Targeting.PrevTargetFriendlyMonster();
                }
              }
            }
            else
            {
              if (closest)
              {
                Targeting.TargetCloseFriendly();
              }
              else if (random)
              {
                Targeting.TargetRandFriendly();
              }
            }

            break;
          case "gray":
          case "grey":
            if (args.Length == 3)
            {
              if (args[2].AsString().IndexOf("human", StringComparison.OrdinalIgnoreCase) != -1)
              {
                if (closest)
                {
                  Targeting.TargetCloseGreyHumanoid();
                }
                else if (random)
                {
                  Targeting.TargetRandGreyHumanoid();
                }
                else if (next)
                {
                  Targeting.NextTargetGreyHumanoid();
                }
                else
                {
                  Targeting.PrevTargetGreyHumanoid();
                }
              }
              else if (args[2].AsString()
                  .IndexOf("monster", StringComparison.OrdinalIgnoreCase) != -1)
              {
                if (closest)
                {
                  Targeting.TargetCloseGreyMonster();
                }
                else if (random)
                {
                  Targeting.TargetRandGreyMonster();
                }
                else if (next)
                {
                  Targeting.NextTargetGreyMonster();
                }
                else
                {
                  Targeting.PrevTargetGreyMonster();
                }
              }
            }
            else
            {
              if (closest)
              {
                Targeting.TargetCloseGrey();
              }
              else if (random)
              {
                Targeting.TargetRandGrey();
              }
            }

            break;
          case "criminal":
            if (args.Length == 3)
            {
              if (args[2].AsString().IndexOf("human", StringComparison.OrdinalIgnoreCase) != -1)
              {
                if (closest)
                {
                  Targeting.TargetCloseCriminalHumanoid();
                }
                else if (random)
                {
                  Targeting.TargetRandCriminalHumanoid();
                }
                else if (next)
                {
                  Targeting.NextTargetCriminalHumanoid();
                }
                else
                {
                  Targeting.PrevTargetCriminalHumanoid();
                }
              }
              else if (args[2].AsString()
                  .IndexOf("monster", StringComparison.OrdinalIgnoreCase) != -1)
              {
                if (closest)
                {
                  Targeting.TargetCloseCriminalMonster();
                }
                else if (random)
                {
                  Targeting.TargetRandCriminalMonster();
                }
                else if (next)
                {
                  Targeting.NextTargetCriminalMonster();
                }
                else
                {
                  Targeting.PrevTargetCriminalMonster();
                }
              }
            }
            else
            {
              if (closest)
              {
                Targeting.TargetCloseCriminal();
              }
              else if (random)
              {
                Targeting.TargetRandCriminal();
              }
            }

            break;
          case "blue":
          case "innocent":
            if (args.Length == 3)
            {
              if (args[2].AsString().IndexOf("human", StringComparison.OrdinalIgnoreCase) != -1)
              {
                if (closest)
                {
                  Targeting.TargetCloseInnocentHumanoid();
                }
                else if (random)
                {
                  Targeting.TargetRandInnocentHumanoid();
                }
                else if (next)
                {
                  Targeting.NextTargetInnocentHumanoid();
                }
                else
                {
                  Targeting.PrevTargetInnocentHumanoid();
                }
              }
              else if (args[2].AsString()
                  .IndexOf("monster", StringComparison.OrdinalIgnoreCase) != -1)
              {
                if (closest)
                {
                  Targeting.TargetCloseInnocentMonster();
                }
                else if (random)
                {
                  Targeting.TargetRandInnocentMonster();
                }
                else if (next)
                {
                  Targeting.NextTargetInnocentMonster();
                }
                else
                {
                  Targeting.PrevTargetInnocentMonster();
                }
              }
            }
            else
            {
              if (closest)
              {
                Targeting.TargetCloseInnocent();
              }
              else if (random)
              {
                Targeting.TargetRandInnocent();
              }
            }

            break;
          case "red":
          case "murderer":
            if (args.Length == 3)
            {
              if (args[2].AsString().IndexOf("human", StringComparison.OrdinalIgnoreCase) != -1)
              {
                if (closest)
                {
                  Targeting.TargetCloseRedHumanoid();
                }
                else if (random)
                {
                  Targeting.TargetRandRedHumanoid();
                }
                else if (next)
                {
                  Targeting.NextTargetMurdererHumanoid();
                }
                else
                {
                  Targeting.PrevTargetMurdererHumanoid();
                }
              }
              else if (args[2].AsString()
                  .IndexOf("monster", StringComparison.OrdinalIgnoreCase) != -1)
              {
                if (closest)
                {
                  Targeting.TargetCloseRedMonster();
                }
                else if (random)
                {
                  Targeting.TargetRandRedMonster();
                }
                else if (next)
                {
                  Targeting.NextTargetMurdererMonster();
                }
                else
                {
                  Targeting.PrevTargetMurdererMonster();
                }
              }
            }
            else
            {
              if (closest)
              {
                Targeting.TargetCloseRed();
              }
              else if (random)
              {
                Targeting.TargetRandRed();
              }
            }

            break;
          case "nonfriendly":
            if (args.Length == 3)
            {
              if (args[2].AsString().IndexOf("human", StringComparison.OrdinalIgnoreCase) != -1)
              {
                if (closest)
                {
                  Targeting.TargetCloseNonFriendlyHumanoid();
                }
                else if (random)
                {
                  Targeting.TargetRandNonFriendlyHumanoid();
                }
                else if (next)
                {
                  Targeting.NextTargetNonFriendlyHumanoid();
                }
                else
                {
                  Targeting.PrevTargetNonFriendlyHumanoid();
                }
              }
              else if (args[2].AsString()
                  .IndexOf("monster", StringComparison.OrdinalIgnoreCase) != -1)
              {
                if (closest)
                {
                  Targeting.TargetCloseNonFriendlyMonster();
                }
                else if (random)
                {
                  Targeting.TargetRandNonFriendlyMonster();
                }
                else if (next)
                {
                  Targeting.NextTargetNonFriendlyMonster();
                }
                else
                {
                  Targeting.PrevTargetNonFriendlyMonster();
                }
              }
            }
            else
            {
              if (closest)
              {
                Targeting.TargetCloseNonFriendly();
              }
              else if (random)
              {
                Targeting.TargetRandNonFriendly();
              }
            }

            break;
          default:
            throw new RunTimeError($"Unknown target type: '{args[1].AsString()}'");
        }
      }
    }

    //public static string ReplaceStringInterpolations(string stringWithPossibleInterpolation)
    //{
    //  Regex regex = new Regex(@"\{{(.*?)\}}");
    //  return regex.Replace(stringWithPossibleInterpolation, match =>
    //  {
    //    string varName = match.Groups[1].Value;
    //    Variable varContent = Interpreter.GetVariable(varName);

    //    return varContent?.AsString() ?? "<not found>";
    //  });
    //}

    private static readonly Regex _dateRegex =
     new Regex(
         @"(\[d:(?<format1>[^\]]*)\])|(\{\{d:(?<format2>[^\}]*)\}\})",
         RegexOptions.IgnoreCase | RegexOptions.Compiled
     );

    private static readonly Regex _varRegex =
        new Regex(@"\{{(.*?)\}}", RegexOptions.Compiled);

    public static string ReplaceStringInterpolations(string message)
    {
      if (string.IsNullOrEmpty(message))
        return message;

      // 1) Data: [D:...] ou {{d:...}}
      message = _dateRegex.Replace(message, m =>
      {
        // pega o grupo que foi preenchido
        var format =
            m.Groups["format1"].Success ? m.Groups["format1"].Value :
            m.Groups["format2"].Success ? m.Groups["format2"].Value :
            null;

        if (string.IsNullOrWhiteSpace(format))
          format = "HH:mm:ss";

        return DateTime.Now.ToString(format);
      });

      // 2) Variáveis: {{VAR}}
      message = _varRegex.Replace(message, match =>
      {
        string varName = match.Groups[1].Value;
        var handler = Interpreter.GetExpressionHandler(varName);
        if (handler != null)
        {
          try
          {
            var result = handler.Invoke(varName, new Variable[] { }, false, false);
            return result.ToString();
          }
          catch
          {
          }
        }


        Variable varContent = Interpreter.GetVariable(varName);

        return varContent?.AsString() ?? varName;
      });

      return message;
    }
  }
}
