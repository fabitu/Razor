using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assistant.Scripts.Engine
{
  // Abstract Syntax Tree Node
  public class ASTNode
  {
    public readonly ASTNodeType Type;
    public readonly string Lexeme;
    public readonly ASTNode Parent;
    public readonly int LineNumber;
    public bool IsRecursive;

    internal LinkedListNode<ASTNode> _node;
    private LinkedList<ASTNode> _children;

    public ASTNode(ASTNodeType type, string lexeme, ASTNode parent, int lineNumber)
    {
      Type = type;
      if (lexeme != null)
      {
        Lexeme = lexeme;
        if (lexeme.Equals("walkto"))
          IsRecursive = true;
      }
      else
      {
        Lexeme = "";
      }
      Parent = parent;
      LineNumber = lineNumber;
    }

    public ASTNode Push(ASTNodeType type, string lexeme, int lineNumber)
    {
      var node = new ASTNode(type, lexeme, this, lineNumber);

      if (_children == null)
        _children = new LinkedList<ASTNode>();

      node._node = _children.AddLast(node);

      return node;
    }

    public ASTNode FirstChild()
    {
      if (_children == null || _children.First == null)
        return null;

      return _children.First.Value;
    }

    public ASTNode Next()
    {
      if (_node == null || _node.Next == null)
        return null;

      return _node.Next.Value;
    }

    public ASTNode Prev()
    {
      if (_node == null || _node.Previous == null)
        return null;

      return _node.Previous.Value;
    }
  }

  public enum ASTNodeType
  {
    // Keywords
    IF,
    ELSEIF,
    ELSE,
    ENDIF,
    WHILE,
    ENDWHILE,
    FOR,
    FOREACH,
    ENDFOR,
    BREAK,
    CONTINUE,
    STOP,
    REPLAY,

    // Operators
    EQUAL,
    NOT_EQUAL,
    LESS_THAN,
    LESS_THAN_OR_EQUAL,
    GREATER_THAN,
    GREATER_THAN_OR_EQUAL,
    IN,
    AS,

    // Logical Operators
    NOT,
    AND,
    OR,

    // Value types
    STRING,
    SERIAL,
    INTEGER,
    DOUBLE,
    LIST,

    // Modifiers
    QUIET, // @ symbol
    FORCE, // ! symbol

    // Everything else
    SCRIPT,
    STATEMENT,
    COMMAND,
    OPERAND,
    LOGICAL_EXPRESSION,
    UNARY_EXPRESSION,
    BINARY_EXPRESSION,
  }
}
