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

using System;
using System.Collections.Generic;
using System.Globalization;

namespace Assistant.Scripts.Engine
{
  public static class Interpreter
  {
    public static Action OnStop;

    // The "global" scope
    private readonly static Scope _scope = new Scope(null, null);

    // The current scope
    private static Scope _currentScope = _scope;

    // Lists
    private static Dictionary<string, List<Variable>> _lists = new Dictionary<string, List<Variable>>();

    // Timers
    private static Dictionary<string, DateTime> _timers = new Dictionary<string, DateTime>();

    // Expressions
    public delegate IComparable ExpressionHandler(string expression, Variable[] args, bool quiet, bool force);
    public delegate T ExpressionHandler<T>(string expression, Variable[] args, bool quiet, bool force) where T : IComparable;

    private static Dictionary<string, ExpressionHandler> _exprHandlers = new Dictionary<string, ExpressionHandler>();

    public delegate bool CommandHandler(string command, Variable[] args, bool quiet, bool force);

    private static Dictionary<string, CommandHandler> _commandHandlers = new Dictionary<string, CommandHandler>();

    public delegate uint AliasHandler(string alias);

    private static Dictionary<string, AliasHandler> _aliasHandlers = new Dictionary<string, AliasHandler>();

    private static Script _activeScript = null;
    public static int CurrentLine
    {
      get
      {
        return _activeScript == null ? 0 : _activeScript.CurrentLine;
      }
    }
    private enum ExecutionState
    {
      RUNNING,
      PAUSED,
      TIMING_OUT
    };

    public delegate bool TimeoutCallback();

    private static ExecutionState _executionState = ExecutionState.RUNNING;
    private static long _pauseTimeout = long.MaxValue;
    private static TimeoutCallback _timeoutCallback = null;

    internal static CultureInfo Culture;

    static Interpreter()
    {
      Culture = new CultureInfo(CultureInfo.CurrentCulture.LCID, false);
      Culture.NumberFormat.NumberDecimalSeparator = ".";
      Culture.NumberFormat.NumberGroupSeparator = ",";
    }

    public static void RegisterExpressionHandler<T>(string keyword, ExpressionHandler<T> handler) where T : IComparable
    {
      _exprHandlers[keyword] = (expression, args, quiet, force) => handler(expression, args, quiet, force);
    }

    public static ExpressionHandler GetExpressionHandler(string keyword)
    {
      _exprHandlers.TryGetValue(keyword, out var expression);

      return expression;
    }

    public static void RegisterCommandHandler(string keyword, CommandHandler handler)
    {
      _commandHandlers[keyword] = handler;
    }

    public static CommandHandler GetCommandHandler(string keyword)
    {
      _commandHandlers.TryGetValue(keyword, out CommandHandler handler);

      return handler;
    }

    public static void RegisterAliasHandler(string keyword, AliasHandler handler)
    {
      _aliasHandlers[keyword] = handler;
    }

    public static void UnregisterAliasHandler(string keyword)
    {
      _aliasHandlers.Remove(keyword);
    }

    public static bool AliasHandlerExist(string alias)
    {
      return _aliasHandlers.TryGetValue(alias, out _);
    }

    public static void PushScope(ASTNode node)
    {
      _currentScope = new Scope(_currentScope, node);
    }

    public static void PopScope()
    {
      if (_currentScope == _scope)
        throw new RunTimeError("Attempted to remove global scope");

      _currentScope = _currentScope.Parent;
    }

    internal static Scope CurrentScope => _currentScope;

    public static void SetVariable(string name, string value, bool global = false)
    {
      Scope scope = global ? _scope : _currentScope;

      scope.SetVariable(name, new Variable(value));
    }

    public static void AddIgnore(Serial serial, bool global = true)
    {
      Scope scope = global ? _scope : _currentScope;
      scope.AddIgnore(serial);
    }

    public static void ClearIgnore(bool global = true)
    {
      Scope scope = global ? _scope : _currentScope;
      scope.ClearIgnore();
    }

    public static void AddIgnoreRange(List<Serial> serials, bool global = true)
    {
      Scope scope = global ? _scope : _currentScope;
      scope.AddIgnoreRange(serials);
    }

    public static void RemoveIgnore(Serial serial, bool global = true)
    {
      Scope scope = global ? _scope : _currentScope;
      scope.RemoveIgnore(serial);
    }

    public static void RemoveIgnoreRange(List<Serial> serials, bool global = true)
    {
      Scope scope = global ? _scope : _currentScope;
      scope.RemoveIgnoreRange(serials);
    }

    public static bool CheckIgnored(Serial serial, bool global = true)
    {
      Scope scope = global ? _scope : _currentScope;
      return scope.CheckIgnored(serial);
    }

    public static Variable GetVariable(string name)
    {
      var scope = _currentScope;
      Variable result = null;

      while (scope != null)
      {
        result = scope.GetVariable(name);
        if (result != null)
          return result;

        scope = scope.Parent;
      }

      return result;
    }

    public static void ClearVariable(string name)
    {
      _currentScope.ClearVariable(name);
    }

    public static bool ExistVariable(string name)
    {
      return _currentScope.ExistVariable(name);
    }

    public static uint GetAlias(string alias)
    {
      // If a handler is explicitly registered, call that.
      if (_aliasHandlers.TryGetValue(alias, out AliasHandler handler))
        return handler(alias);

      var arg = GetVariable(alias);

      return arg?.AsUInt() ?? uint.MaxValue;
    }

    public static void SetAlias(string alias, uint serial)
    {
      SetVariable(alias, serial.ToString(), true);
    }

    public static void ClearAlias(string alias)
    {
      _scope.ClearVariable(alias);
    }

    public static bool ExistAlias(string alias)
    {
      return _scope.ExistVariable(alias);
    }

    public static void CreateList(string name)
    {
      if (_lists.ContainsKey(name))
        return;

      _lists[name] = new List<Variable>();
    }

    public static void DestroyList(string name)
    {
      _lists.Remove(name);
    }

    public static void ClearList(string name)
    {
      if (!_lists.ContainsKey(name))
        return;

      _lists[name].Clear();
    }

    public static bool ListExists(string name)
    {
      return _lists.ContainsKey(name);
    }

    public static List<Variable> GetList(string name)
    {
      return _lists.ContainsKey(name) ? _lists[name] : null;
    }
    public static bool ListContains(string name, Variable arg)
    {
      if (!_lists.ContainsKey(name))
        throw new RunTimeError("List does not exist");

      var list = _lists[name];
      var a = arg.AsString();

      foreach (var v in list)
      {
        if (v.AsString().Equals(a, StringComparison.OrdinalIgnoreCase))
        {
          return true;
        }
      }

      return false;
    }

    public static int ListLength(string name)
    {
      if (!_lists.ContainsKey(name))
        throw new RunTimeError("List does not exist");

      return _lists[name].Count;
    }

    public static void PushList(string name, Variable arg, bool front, bool unique)
    {
      if (!_lists.ContainsKey(name))
        throw new RunTimeError("List does not exist");

      if (unique && _lists[name].Contains(arg))
        return;

      if (front)
        _lists[name].Insert(0, arg);
      else
        _lists[name].Add(arg);
    }

    public static bool PopList(string name, Variable arg)
    {
      if (!_lists.ContainsKey(name))
        throw new RunTimeError("List does not exist");

      return _lists[name].Remove(arg);
    }

    public static bool PopList(string name, bool front, out Variable removedVar)
    {
      if (!_lists.ContainsKey(name))
        throw new RunTimeError("List does not exist");

      var list = _lists[name];
      var idx = front ? 0 : _lists[name].Count - 1;

      removedVar = list[idx];
      list.RemoveAt(idx);

      return list.Count > 0;
    }

    public static Variable GetListValue(string name, int idx)
    {
      if (!_lists.ContainsKey(name))
        throw new RunTimeError("List does not exist");

      var list = _lists[name];

      if (idx < list.Count)
        return list[idx];

      return null;
    }

    public static void CreateTimer(string name)
    {
      _timers[name] = DateTime.UtcNow;
    }

    public static TimeSpan GetTimer(string name)
    {
      if (!_timers.TryGetValue(name, out DateTime timestamp))
        throw new RunTimeError("Timer does not exist");

      TimeSpan elapsed = DateTime.UtcNow - timestamp;

      return elapsed;
    }

    public static void SetTimer(string name, int elapsed)
    {
      // Setting a timer to start at a given value is equivalent to
      // starting the timer that number of milliseconds in the past.
      _timers[name] = DateTime.UtcNow.AddMilliseconds(-elapsed);
    }

    public static void RemoveTimer(string name)
    {
      _timers.Remove(name);
    }

    public static bool TimerExists(string name)
    {
      return _timers.ContainsKey(name);
    }

    public static bool StartScript(Script script)
    {
      if (_activeScript != null)
        return false;

      _currentScope = _scope;
      _activeScript = script;
      _activeScript.Initialize();
      _executionState = ExecutionState.RUNNING;

      ExecuteScript();

      return true;
    }

    public static void StopScript()
    {
      _activeScript = null;
      _currentScope = _scope;
      _executionState = ExecutionState.RUNNING;

      if (_timeoutCallback != null)
      {
        if (_timeoutCallback())
        {
          ClearTimeout();
        }

        _timeoutCallback = null;
      }

      OnStop?.Invoke();
    }
    public static void PauseScript()
    {
      _pauseTimeout = DateTime.MaxValue.Ticks;
      _executionState = ExecutionState.PAUSED;
    }
    public static void ResumeScript()
    {
      _executionState = ExecutionState.RUNNING;
    }
    public static bool ScriptIsRunning()
    {
      if (_activeScript == null)
        return false;
      return true;
    }
    public static bool ExecuteScript()
    {
      if (_activeScript == null)
        return false;

      if (_executionState == ExecutionState.PAUSED)
      {
        if (_pauseTimeout < DateTime.UtcNow.Ticks)
          _executionState = ExecutionState.RUNNING;
        else
          return true;
      }
      else if (_executionState == ExecutionState.TIMING_OUT)
      {
        if (_pauseTimeout < DateTime.UtcNow.Ticks)
        {
          if (_timeoutCallback != null)
          {
            if (_timeoutCallback())
            {
              _activeScript.Advance();
              ClearTimeout();
            }

            _timeoutCallback = null;
          }

          /* If the callback changed the state to running, continue
           * on. Otherwise, exit.
           */
          if (_executionState != ExecutionState.RUNNING)
          {
            _activeScript = null;
            return false;
          }
        }
      }

      if (!_activeScript.ExecuteNext())
      {
        _activeScript = null;
        return false;
      }

      return true;
    }

    // Pause execution for the given number of milliseconds
    public static void Pause(long duration)
    {
      // Already paused or timing out
      if (_executionState != ExecutionState.RUNNING)
        return;

      _pauseTimeout = DateTime.UtcNow.Ticks + (duration * 10000);
      _executionState = ExecutionState.PAUSED;
    }

    // Unpause execution
    public static void Unpause()
    {
      if (_executionState != ExecutionState.PAUSED)
        return;

      _pauseTimeout = 0;
      _executionState = ExecutionState.RUNNING;
    }

    // If forward progress on the script isn't made within this
    // amount of time (milliseconds), bail
    public static void Timeout(long duration, TimeoutCallback callback)
    {
      // Don't change an existing timeout
      if (_executionState != ExecutionState.RUNNING)
        return;

      _pauseTimeout = DateTime.UtcNow.Ticks + (duration * 10000);
      _executionState = ExecutionState.TIMING_OUT;
      _timeoutCallback = callback;
    }

    // If forward progress on the script isn't made within this
    // amount of time (milliseconds), bail
    public static void Timeout(long duration, long timeout, TimeoutCallback callback)
    {
      // Don't change an existing timeout
      if (_executionState != ExecutionState.RUNNING)
        return;

      _pauseTimeout = DateTime.UtcNow.Ticks + (duration * timeout);
      _executionState = ExecutionState.TIMING_OUT;
      _timeoutCallback = callback;
    }

    // Clears any previously set timeout. Automatically
    // called any time the script advances a statement.
    public static void ClearTimeout()
    {
      if (_executionState != ExecutionState.TIMING_OUT)
        return;

      _pauseTimeout = 0;
      _executionState = ExecutionState.RUNNING;
    }
  }
}