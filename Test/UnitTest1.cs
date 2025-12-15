using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Reflection;
using Ultima;
using static System.Net.Mime.MediaTypeNames;

namespace Test
{
  [TestClass]
  public class UnitTest1
  {
    [TestMethod]
    public void TestMethod1()
    {   
      TileData.Initialize();
      var filePath = Path.Combine(Environment.CurrentDirectory, Guid.NewGuid().ToString(), ".csv");
      TileData.ExportLandDataToCSV(filePath);
    }
  }
}
