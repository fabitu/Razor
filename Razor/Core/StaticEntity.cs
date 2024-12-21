using Assistant.Gumps;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Text;
using System.Threading.Tasks;

namespace Assistant.Core
{
  public class StaticEntity : UOEntity
  {
    public ushort Graphic { get; set; }
    public string Name { get; set; }

    public StaticEntity(ushort graphic, Point3D point3D, Serial ser) : base(ser)
    {
      Graphic = graphic;
      Position = point3D;
    }
  }
}
