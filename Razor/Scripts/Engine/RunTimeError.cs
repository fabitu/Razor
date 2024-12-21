using System;

namespace Assistant.Scripts.Engine
{
  public class RunTimeError : Exception
  {
    public RunTimeError(string error) : base(error)
    {
    }
  }
}
