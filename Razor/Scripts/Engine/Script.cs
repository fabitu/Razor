using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assistant.Scripts.Engine
{
  public class Script
  {
    public ASTNode _statement;

    public int CurrentLine
    {
      get
      {
        return _statement == null ? 0 : _statement.LineNumber;
      }
    }

    private Variable[] ConstructArguments(ref ASTNode node)
    {
      List<Variable> args = new List<Variable>();

      node = node.Next();

      while (node != null)
      {
        switch (node.Type)
        {
          case ASTNodeType.AND:
          case ASTNodeType.OR:
          case ASTNodeType.EQUAL:
          case ASTNodeType.NOT_EQUAL:
          case ASTNodeType.LESS_THAN:
          case ASTNodeType.LESS_THAN_OR_EQUAL:
          case ASTNodeType.GREATER_THAN:
          case ASTNodeType.GREATER_THAN_OR_EQUAL:
          case ASTNodeType.IN:
          case ASTNodeType.AS:
            return args.ToArray();
        }

        args.Add(new Variable(node.Lexeme));

        node = node.Next();
      }

      return args.ToArray();
    }

    // For now, the scripts execute directly from the
    // abstract syntax tree. This is relatively simple.
    // A more robust approach would be to "compile" the
    // scripts to a bytecode. That would allow more errors
    // to be caught with better error messages, as well as
    // make the scripts execute more quickly.
    public Script(ASTNode root)
    {
      // Set current to the first statement
      _statement = root.FirstChild();
    }

    public void Initialize()
    {
      Interpreter.PushScope(_statement);
    }

    public bool ExecuteNext()
    {
      if (_statement == null)
        return false;

      if (_statement.Type != ASTNodeType.STATEMENT)
        throw new RunTimeError("Invalid script");

      var node = _statement.FirstChild();

      if (node == null)
        throw new RunTimeError("Invalid statement");

      int depth = 0;

      switch (node.Type)
      {
        case ASTNodeType.IF:
          {
            Interpreter.PushScope(node);

            var expr = node.FirstChild();
            var result = EvaluateExpression(ref expr);

            // Advance to next statement
            Advance();

            // Evaluated true. Jump right into execution.
            if (result)
              break;

            // The expression evaluated false, so keep advancing until
            // we hit an elseif, else, or endif statement that matches
            // and try again.
            depth = 0;

            while (_statement != null)
            {
              node = _statement.FirstChild();

              if (node.Type == ASTNodeType.IF)
              {
                depth++;
              }
              else if (node.Type == ASTNodeType.ELSEIF)
              {
                if (depth == 0)
                {
                  expr = node.FirstChild();
                  result = EvaluateExpression(ref expr);

                  // Evaluated true. Jump right into execution
                  if (result)
                  {
                    Advance();
                    break;
                  }
                }
              }
              else if (node.Type == ASTNodeType.ELSE)
              {
                if (depth == 0)
                {
                  // Jump into the else clause
                  Advance();
                  break;
                }
              }
              else if (node.Type == ASTNodeType.ENDIF)
              {
                if (depth == 0)
                  break;

                depth--;
              }

              Advance();
            }

            if (_statement == null)
              throw new RunTimeError("If with no matching endif");

            break;
          }
        case ASTNodeType.ELSEIF:
          // If we hit the elseif statement during normal advancing, skip over it. The only way
          // to execute an elseif clause is to jump directly in from an if statement.
          depth = 0;

          while (_statement != null)
          {
            node = _statement.FirstChild();

            if (node.Type == ASTNodeType.IF)
            {
              depth++;
            }
            else if (node.Type == ASTNodeType.ENDIF)
            {
              if (depth == 0)
                break;

              depth--;
            }

            Advance();
          }

          if (_statement == null)
            throw new RunTimeError("If with no matching endif");

          break;
        case ASTNodeType.ENDIF:
          Interpreter.PopScope();
          Advance();
          break;
        case ASTNodeType.ELSE:
          // If we hit the else statement during normal advancing, skip over it. The only way
          // to execute an else clause is to jump directly in from an if statement.
          depth = 0;

          while (_statement != null)
          {
            node = _statement.FirstChild();

            if (node.Type == ASTNodeType.IF)
            {
              depth++;
            }
            else if (node.Type == ASTNodeType.ENDIF)
            {
              if (depth == 0)
                break;

              depth--;
            }

            Advance();
          }

          if (_statement == null)
            throw new RunTimeError("If with no matching endif");

          break;
        case ASTNodeType.WHILE:
          {
            // The iterator variable's name is the hash code of the for loop's ASTNode.
            var iterName = node.GetHashCode().ToString();

            // When we first enter the loop, push a new scope
            if (Interpreter.CurrentScope.StartNode != node)
            {
              Interpreter.PushScope(node);
              Interpreter.SetVariable(iterName, "0");
              Interpreter.SetVariable("index", "0");
            }
            else
            {
              // Increment the iterator argument
              var arg = Interpreter.GetVariable(iterName);
              var index = arg.AsUInt() + 1;
              Interpreter.SetVariable(iterName, index.ToString());
              Interpreter.SetVariable("index", index.ToString());
            }

            var expr = node.FirstChild();
            var result = EvaluateExpression(ref expr);

            // Advance to next statement
            Advance();

            // The expression evaluated false, so keep advancing until
            // we hit an endwhile statement.
            if (!result)
            {
              depth = 0;

              while (_statement != null)
              {
                node = _statement.FirstChild();

                if (node.Type == ASTNodeType.WHILE)
                {
                  depth++;
                }
                else if (node.Type == ASTNodeType.ENDWHILE)
                {
                  if (depth == 0)
                  {
                    Interpreter.PopScope();
                    // Go one past the endwhile so the loop doesn't repeat
                    Advance();
                    break;
                  }

                  depth--;
                }

                Advance();
              }
            }
            break;
          }
        case ASTNodeType.ENDWHILE:
          // Walk backward to the while statement
          _statement = _statement.Prev();

          depth = 0;

          while (_statement != null)
          {
            node = _statement.FirstChild();

            if (node.Type == ASTNodeType.ENDWHILE)
            {
              depth++;
            }
            else if (node.Type == ASTNodeType.WHILE)
            {
              if (depth == 0)
                break;

              depth--;
            }

            _statement = _statement.Prev();
          }

          if (_statement == null)
            throw new RunTimeError("Unexpected endwhile");

          break;
        case ASTNodeType.FOR:
          {
            // The iterator variable's name is the hash code of the for loop's ASTNode.
            var iterName = node.GetHashCode().ToString();

            // When we first enter the loop, push a new scope
            if (Interpreter.CurrentScope.StartNode != node)
            {
              Interpreter.PushScope(node);

              // Grab the arguments
              var max = node.FirstChild();

              if (max.Type != ASTNodeType.INTEGER)
                throw new RunTimeError("Invalid for loop syntax");

              // Create a dummy argument that acts as our loop variable
              Interpreter.SetVariable(iterName, "0");
              Interpreter.SetVariable("index", "0");
            }
            else
            {
              // Increment the iterator argument
              var arg = Interpreter.GetVariable(iterName);
              var index = arg.AsUInt() + 1;
              Interpreter.SetVariable(iterName, index.ToString());
              Interpreter.SetVariable("index", index.ToString());
            }

            // Check loop condition
            var i = Interpreter.GetVariable(iterName);

            // Grab the max value to iterate to
            node = node.FirstChild();
            var end = new Variable(node.Lexeme);

            if (i.AsUInt() < end.AsUInt())
            {
              // enter the loop
              Advance();
            }
            else
            {
              // Walk until the end of the loop
              Advance();

              depth = 0;

              while (_statement != null)
              {
                node = _statement.FirstChild();

                if (node.Type == ASTNodeType.FOR || node.Type == ASTNodeType.FOREACH)
                {
                  depth++;
                }
                else if (node.Type == ASTNodeType.ENDFOR)
                {
                  if (depth == 0)
                  {
                    Interpreter.PopScope();
                    // Go one past the end so the loop doesn't repeat
                    Advance();
                    break;
                  }

                  depth--;
                }

                Advance();
              }
            }
          }
          break;
        case ASTNodeType.FOREACH:
          {
            // foreach VAR in LIST
            // The iterator's name is the hash code of the for loop's ASTNode.
            var varName = node.FirstChild().Lexeme;
            var listName = node.FirstChild().Next().Lexeme;
            var iterName = node.GetHashCode().ToString();

            // When we first enter the loop, push a new scope
            if (Interpreter.CurrentScope.StartNode != node)
            {
              Interpreter.PushScope(node);

              // Create a dummy argument that acts as our iterator object
              Interpreter.SetVariable(iterName, "0");
              Interpreter.SetVariable("index", "0");

              // Make the user-chosen variable have the value for the front of the list
              var arg = Interpreter.GetListValue(listName, 0);

              if (arg != null)
                Interpreter.SetVariable(varName, arg.AsString());
              else
                Interpreter.ClearVariable(varName);
            }
            else
            {
              // Increment the iterator argument
              var idx = Interpreter.GetVariable(iterName).AsInt() + 1;
              Interpreter.SetVariable(iterName, idx.ToString()); ;
              Interpreter.SetVariable("index", idx.ToString());

              // Update the user-chosen variable
              var arg = Interpreter.GetListValue(listName, idx);

              if (arg != null)
                Interpreter.SetVariable(varName, arg.AsString());
              else
                Interpreter.ClearVariable(varName);
            }

            // Check loop condition
            var i = Interpreter.GetVariable(varName);

            if (i != null)
            {
              // enter the loop
              Advance();
            }
            else
            {
              // Walk until the end of the loop
              Advance();

              depth = 0;

              while (_statement != null)
              {
                node = _statement.FirstChild();

                if (node.Type == ASTNodeType.FOR ||
                    node.Type == ASTNodeType.FOREACH)
                {
                  depth++;
                }
                else if (node.Type == ASTNodeType.ENDFOR)
                {
                  if (depth == 0)
                  {
                    Interpreter.PopScope();
                    // Go one past the end so the loop doesn't repeat
                    Advance();
                    break;
                  }

                  depth--;
                }

                Advance();
              }
            }
            break;
          }
        case ASTNodeType.ENDFOR:
          // Walk backward to the for statement
          _statement = _statement.Prev();

          // track depth in case there is a nested for
          depth = 0;

          while (_statement != null)
          {
            node = _statement.FirstChild();

            if (node.Type == ASTNodeType.ENDFOR)
            {
              depth++;
            }
            else if (node.Type == ASTNodeType.FOR ||
                     node.Type == ASTNodeType.FOREACH)
            {
              if (depth == 0)
              {
                break;
              }
              depth--;
            }

            _statement = _statement.Prev();
          }

          if (_statement == null)
            throw new RunTimeError("Unexpected endfor");

          break;
        case ASTNodeType.BREAK:
          // Walk until the end of the loop
          Advance();

          depth = 0;

          while (_statement != null)
          {
            node = _statement.FirstChild();

            if (node.Type == ASTNodeType.WHILE ||
                node.Type == ASTNodeType.FOR ||
                node.Type == ASTNodeType.FOREACH)
            {
              depth++;
            }
            else if (node.Type == ASTNodeType.ENDWHILE ||
                node.Type == ASTNodeType.ENDFOR)
            {
              if (depth == 0)
              {
                // Go one past the end so the loop doesn't repeat
                Advance();
                break;
              }

              depth--;
            }

            Advance();
          }

          Interpreter.PopScope();
          break;
        case ASTNodeType.CONTINUE:
          // Walk backward to the loop statement
          _statement = _statement.Prev();

          depth = 0;

          while (_statement != null)
          {
            node = _statement.FirstChild();

            if (node.Type == ASTNodeType.ENDWHILE ||
                node.Type == ASTNodeType.ENDFOR)
            {
              depth++;
            }
            else if (node.Type == ASTNodeType.WHILE ||
                     node.Type == ASTNodeType.FOR ||
                     node.Type == ASTNodeType.FOREACH)
            {
              if (depth == 0)
                break;

              depth--;
            }

            _statement = _statement.Prev();
          }

          if (_statement == null)
            throw new RunTimeError("Unexpected continue");
          break;
        case ASTNodeType.STOP:
          _statement = null;
          break;
        case ASTNodeType.REPLAY:
          _statement = _statement.Parent.FirstChild();
          break;
        case ASTNodeType.QUIET:
        case ASTNodeType.FORCE:
        case ASTNodeType.COMMAND:
          if (ExecuteCommand(node))
            Advance();

          break;
      }

      return (_statement != null) ? true : false;
    }

    public void Advance()
    {
      Interpreter.ClearTimeout();
      _statement = _statement.Next();
    }

    private ASTNode EvaluateModifiers(ASTNode node, out bool quiet, out bool force, out bool not)
    {
      quiet = false;
      force = false;
      not = false;

      while (true)
      {
        switch (node.Type)
        {
          case ASTNodeType.QUIET:
            quiet = true;
            break;
          case ASTNodeType.FORCE:
            force = true;
            break;
          case ASTNodeType.NOT:
            not = true;
            break;
          default:
            return node;
        }

        node = node.Next();
      }
    }

    private bool ExecuteCommand(ASTNode node)
    {    
      node = EvaluateModifiers(node, out bool quiet, out bool force, out _);

      var handler = Interpreter.GetCommandHandler(node.Lexeme) ?? throw new RunTimeError("Unknown command");

      var cont = handler(node.Lexeme, ConstructArguments(ref node), quiet, force);

      if (node != null)
        throw new RunTimeError("Command did not consume all available arguments");   

      return cont;
    }

    private bool EvaluateExpression(ref ASTNode expr)
    {
      if (expr == null || (expr.Type != ASTNodeType.UNARY_EXPRESSION && expr.Type != ASTNodeType.BINARY_EXPRESSION && expr.Type != ASTNodeType.LOGICAL_EXPRESSION))
        throw new RunTimeError("No expression following control statement");

      var node = expr.FirstChild();

      if (node == null)
        throw new RunTimeError("Empty expression following control statement");

      switch (expr.Type)
      {
        case ASTNodeType.UNARY_EXPRESSION:
          return EvaluateUnaryExpression(ref node);
        case ASTNodeType.BINARY_EXPRESSION:
          return EvaluateBinaryExpression(ref node);
      }

      bool lhs = EvaluateExpression(ref node);

      node = node.Next();

      while (node != null)
      {
        // Capture the operator
        var op = node.Type;
        node = node.Next();

        if (node == null)
          throw new RunTimeError("Invalid logical expression");

        bool rhs;

        var e = node.FirstChild();

        switch (node.Type)
        {
          case ASTNodeType.UNARY_EXPRESSION:
            rhs = EvaluateUnaryExpression(ref e);
            break;
          case ASTNodeType.BINARY_EXPRESSION:
            rhs = EvaluateBinaryExpression(ref e);
            break;
          default:
            throw new RunTimeError("Nested logical expressions are not possible");
        }

        switch (op)
        {
          case ASTNodeType.AND:
            lhs = lhs && rhs;
            break;
          case ASTNodeType.OR:
            lhs = lhs || rhs;
            break;
          default:
            throw new RunTimeError("Invalid logical operator");
        }

        node = node.Next();
      }

      return lhs;
    }

    private bool CompareOperands(ASTNodeType op, IComparable lhs, IComparable rhs)
    {
      if (op == ASTNodeType.IN)
      {
        if (lhs.GetType() != typeof(string) || rhs.GetType() != typeof(string))
        {
          throw new RunTimeError("The 'in' operator only works on string operands.");
        }
      }
      else if (op == ASTNodeType.AS)
      {
        if (lhs.GetType() != typeof(uint))
        {
          throw new RunTimeError("The left hand side of an 'as' expression must evaluate to a serial");
        }

        if (rhs.GetType() != typeof(string))
        {
          throw new RunTimeError("The right hand side of an 'as' expression must evaluate to a string");
        }
      }
      else if (lhs.GetType() != rhs.GetType())
      {
        // Different types. Try to convert one to match the other.

        if (rhs is double)
        {
          // Special case for rhs doubles because we don't want to lose precision.
          lhs = (double)lhs;
        }
        else if (rhs is bool)
        {
          // Special case for rhs bools because we want to down-convert the lhs.
          var tmp = Convert.ChangeType(lhs, typeof(bool));
          lhs = (IComparable)tmp;
        }
        else
        {
          var tmp = Convert.ChangeType(rhs, lhs.GetType());
          rhs = (IComparable)tmp;
        }
      }

      try
      {
        // Evaluate the whole expression
        switch (op)
        {
          case ASTNodeType.EQUAL:
            return lhs.CompareTo(rhs) == 0;
          case ASTNodeType.NOT_EQUAL:
            return lhs.CompareTo(rhs) != 0;
          case ASTNodeType.LESS_THAN:
            return lhs.CompareTo(rhs) < 0;
          case ASTNodeType.LESS_THAN_OR_EQUAL:
            return lhs.CompareTo(rhs) <= 0;
          case ASTNodeType.GREATER_THAN:
            return lhs.CompareTo(rhs) > 0;
          case ASTNodeType.GREATER_THAN_OR_EQUAL:
            return lhs.CompareTo(rhs) >= 0;
          case ASTNodeType.IN:
            return ((string)rhs).Contains((string)lhs);
          case ASTNodeType.AS:

            if ((uint)lhs > 0)
            {
              Interpreter.SetVariable(rhs.ToString(), lhs.ToString());
            }

            return CompareOperands(ASTNodeType.EQUAL, lhs, true);
        }
      }
      catch (ArgumentException e)
      {
        throw new RunTimeError(e.Message);
      }

      throw new RunTimeError("Unknown operator in expression");

    }

    private bool EvaluateUnaryExpression(ref ASTNode node)
    {
      node = EvaluateModifiers(node, out bool quiet, out bool force, out bool not);

      var handler = Interpreter.GetExpressionHandler(node.Lexeme);

      if (handler == null)
        throw new RunTimeError("Unknown expression, check is not a command");

      var result = handler(node.Lexeme, ConstructArguments(ref node), quiet, force);

      if (not)
        return CompareOperands(ASTNodeType.EQUAL, result, false);
      else
        return CompareOperands(ASTNodeType.EQUAL, result, true);
    }

    private bool EvaluateBinaryExpression(ref ASTNode node)
    {
      // Evaluate the left hand side
      var lhs = EvaluateBinaryOperand(ref node);

      // Capture the operator
      var op = node.Type;
      node = node.Next();

      // Evaluate the right hand side
      var rhs = EvaluateBinaryOperand(ref node);

      return CompareOperands(op, lhs, rhs);
    }

    private IComparable EvaluateBinaryOperand(ref ASTNode node)
    {
      IComparable val;

      node = EvaluateModifiers(node, out bool quiet, out bool force, out _);
      switch (node.Type)
      {
        case ASTNodeType.INTEGER:
          val = TypeConverter.ToInt(node.Lexeme);
          node = node.Next();
          break;
        case ASTNodeType.SERIAL:
          val = TypeConverter.ToUInt(node.Lexeme);
          node = node.Next();
          break;
        case ASTNodeType.STRING:
          val = node.Lexeme;
          node = node.Next();
          break;
        case ASTNodeType.DOUBLE:
          val = TypeConverter.ToDouble(node.Lexeme);
          node = node.Next();
          break;
        case ASTNodeType.OPERAND:
          {
            // This might be a registered keyword, so do a lookup
            var handler = Interpreter.GetExpressionHandler(node.Lexeme);

            if (handler != null)
            {
              val = handler(node.Lexeme, ConstructArguments(ref node), quiet, force);
              break;
            }

            // It could be a variable
            var arg = Interpreter.GetVariable(node.Lexeme);
            if (arg != null)
            {
              // TODO: Should really look at the type of arg here
              val = arg.AsString();
              node = node.Next();
              break;
            }

            // It's just a string
            val = node.Lexeme;
            node = node.Next();
            break;
          }
        default:
          throw new RunTimeError("Invalid type found in expression");
      }

      return val;
    }
  }
}
