using Assistant.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Assistant.Gumps.Internal
{
  public sealed class StaticInfoGump : Gump
  {
    private enum ItemInfoButtons
    {
      Okay,
      CopyName,
      CopySerial,
      CopyGraphic,
      CopyPosition,
      ItemName,
      Serial,
      Graphic,
      Position
    }

    private StaticEntity _obj { get; }
    public StaticInfoGump(StaticEntity obj) : base(271, 130, -1)
    {
      _obj = obj;

      Closable = true;
      Disposable = true;
      Movable = true;
      Resizable = false;
      Resend = true;

      AddPage(0);

      AddBackground(106, 70, 253, 189, 3600);

      AddLabel(197, 87, 154, "Static Data");
      AddItem(290, 98, obj.Graphic);

      //AddLabel(143, 115, 900, "Name:");
      AddLabel(142, 140, 900, "Serial:");
      AddLabel(142, 164, 900, "Graphic:");
      AddLabel(142, 188, 900, "Position:");

      AddButton(274, 217, 247, 248, (int)ItemInfoButtons.Okay, GumpButtonType.Reply, 0);
      //AddButton(124, 120, 2103, 2104, (int)ItemInfoButtons.CopyName, GumpButtonType.Reply, 0);
      AddButton(124, 143, 2103, 2104, (int)ItemInfoButtons.CopySerial, GumpButtonType.Reply, 0);
      AddButton(124, 168, 2103, 2104, (int)ItemInfoButtons.CopyGraphic, GumpButtonType.Reply, 0);
      AddButton(124, 191, 2103, 2104, (int)ItemInfoButtons.CopyPosition, GumpButtonType.Reply, 0);

      //AddTextEntry(219, 115, 116, 20, 62, (int)ItemInfoButtons.ItemName, $"{obj.Name}");
      AddTextEntry(219, 141, 116, 20, 62, (int)ItemInfoButtons.Serial, $"{obj.Serial}");
      AddTextEntry(219, 165, 116, 20, 62, (int)ItemInfoButtons.Graphic, $"{obj.Graphic}");
      AddTextEntry(219, 188, 116, 20, 62, (int)ItemInfoButtons.Position, $"{_obj.Position.X} {_obj.Position.Y} {_obj.Position.Z}");
    }

    public override void OnResponse(int buttonId, int[] switches, GumpTextEntry[] textEntries = null)
    {
      switch (buttonId)
      {
        //case (int)ItemInfoButtons.CopyName:
        //  Clipboard.SetText(_obj.Name);
        //  World.Player.SendMessage(MsgLevel.Force, Language.Format(LocString.ScriptCopied, _obj.Name), false);
        //  break;
        case (int)ItemInfoButtons.CopySerial:
          Clipboard.SetText(_obj.Serial.ToString());
          World.Player.SendMessage(MsgLevel.Force, Language.Format(LocString.ScriptCopied, _obj.Serial.ToString()), false);
          break;
        case (int)ItemInfoButtons.CopyGraphic:
          Clipboard.SetText(_obj.Graphic.ToString());
          World.Player.SendMessage(MsgLevel.Force, Language.Format(LocString.ScriptCopied, _obj.Graphic.ToString()), false);
          break;
        case (int)ItemInfoButtons.CopyPosition:
          Clipboard.SetText($"{_obj.Position.X} {_obj.Position.Y} {_obj.Position.Z}");
          World.Player.SendMessage(MsgLevel.Force, Language.Format(LocString.ScriptCopied, $"{_obj.Position.X} {_obj.Position.Y} {_obj.Position.Z}"), false);
          break;
        case (int)ItemInfoButtons.Okay:
          Resend = false;
          break;
      }

      base.OnResponse(buttonId, switches, textEntries);
    }
  }
}