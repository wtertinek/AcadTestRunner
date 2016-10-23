using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AcadTestRunner
{
  internal class FileManager : IDisposable
  {
    private bool deleteDwgFile;

    public FileManager(string dwgFilePath, bool deleteDwgFile)
    {
      ScriptFilePath = Path.GetTempFileName()
                           .Replace(".tmp", ".scr");

      if (string.IsNullOrEmpty(dwgFilePath))
      {
        TmpDwgFilePath = ScriptFilePath.Replace(".scr", ".dwg");
      }

      this.deleteDwgFile = deleteDwgFile;
    }

    public void SaveScript(string script)
    {
      File.WriteAllText(ScriptFilePath, script, Encoding.Default);
    }

    public string ScriptFilePath { get; private set; }

    public string TmpDwgFilePath { get; private set; }

    public void Dispose()
    {
      File.Delete(ScriptFilePath);

      if (deleteDwgFile)
      {
        File.Delete(TmpDwgFilePath);
      }
    }
  }
}
