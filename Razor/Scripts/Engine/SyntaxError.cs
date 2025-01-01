using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assistant.Scripts.Engine
{
  public class SyntaxError : Exception
  {
    public ASTNode Node;
    public string Line;
    public int LineNumber;

    public SyntaxError(ASTNode node, string error) : base(error)
    {
      Node = node;
      Line = null;
      LineNumber = 0;
    }

    public SyntaxError(string line, int lineNumber, ASTNode node, string error) : base(error)
    {
      Line = line;
      LineNumber = lineNumber;
      Node = node;
    }
  }
}
