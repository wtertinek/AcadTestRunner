using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AcadTestRunner
{
  internal class ScriptBuilder
  {
    public readonly StringBuilder builder;

    public ScriptBuilder()
      => builder = new StringBuilder();

    public ScriptBuilder QSave(string dwgFilePath)
    {
      builder.Insert(0, GetFilePath(dwgFilePath) + Environment.NewLine)
             .Insert(0, "_qsave" + Environment.NewLine);

      return this;
    }

    public ScriptBuilder NetLoad(string path)
    {
      builder.AppendLine("_netload")
             .AppendLine(GetFilePath(path));

      return this;
    }

    public ScriptBuilder Command(string command, params string[] argumnets)
    {
      builder.AppendLine(command);

      foreach (var argument in argumnets)
      {
        builder.AppendLine(argument);
      }

      return this;
    }

    public ScriptBuilder Quit()
    {
      builder.AppendLine($"_quit{Environment.NewLine}");
      return this;
    }

    public override string ToString()
      => builder.ToString();

    private static string GetFilePath(string path)
      => $"\"{path.Replace("\\", "/")}\"";
  }
}
