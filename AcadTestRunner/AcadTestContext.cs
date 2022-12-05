using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AcadTestRunner
{
  public class AcadTestContext
  {
    private string dwgFilePath;

    internal AcadTestContext(string testMethodName, string dwgFilePath)
    {
      TestMethodName = testMethodName;
      this.dwgFilePath = dwgFilePath;
    }

    internal bool UsesCustomDwg { get; private set; }

    public string TestMethodName { get; }

    public string DwgFilePath
    {
      get => dwgFilePath;
      set
      {
        dwgFilePath = value;
        UsesCustomDwg = !string.IsNullOrEmpty(dwgFilePath);
      }
    }

    public bool DeleteDwgFileAfterTest { get; set; }
  }
}
