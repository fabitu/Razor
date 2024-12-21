using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assistant.Scripts.Engine
{
  internal class Scope
  {
    private Dictionary<string, Variable> _namespace = new Dictionary<string, Variable>();
    private readonly HashSet<Serial> _ignoreList = new HashSet<Serial>();

    public readonly ASTNode StartNode;
    public readonly Scope Parent;

    public Scope(Scope parent, ASTNode start)
    {
      Parent = parent;
      StartNode = start;
    }

    public Variable GetVariable(string name)
    {
      Variable arg;

      if (_namespace.TryGetValue(name, out arg))
        return arg;

      return null;
    }

    public void SetVariable(string name, Variable val)
    {
      _namespace[name] = val;
    }

    public void ClearVariable(string name)
    {
      _namespace.Remove(name);
    }

    public bool ExistVariable(string name)
    {
      return _namespace.ContainsKey(name);
    }

    public void AddIgnore(Serial serial)
    {
      _ignoreList.Add(serial);
    }

    public void AddIgnoreRange(List<Serial> serials)
    {
      serials.ForEach(AddIgnore);
    }
    public void RemoveIgnore(Serial serial)
    {
      if (_ignoreList.Contains(serial))
        _ignoreList.Remove(serial);
    }

    public void RemoveIgnoreRange(List<Serial> serials)
    {
      _ignoreList.RemoveWhere(serials.Contains);
    }

    public void ClearIgnore()
    {
      _ignoreList.Clear();
    }

    public bool CheckIgnored(Serial serial)
    {
      return _ignoreList.Contains(serial);
    }

  }
}
