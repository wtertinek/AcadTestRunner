using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AcadTestRunner
{
  public class AcadTestSetupAttribute : Attribute
  {
  }

  public class AcadTestContext
  {
    private string dwgFilePath;

    internal AcadTestContext(string testMethodName, string dwgFilePath)
    {
      TestMethodName = testMethodName;
      this.dwgFilePath = dwgFilePath;
    }

    internal bool CustomDwgFilePath { get; private set; }

    public string TestMethodName { get; private set; }

    public string DwgFilePath
    {
      get { return dwgFilePath; }
      set
      {
        dwgFilePath = value;
        CustomDwgFilePath = !string.IsNullOrEmpty(dwgFilePath);
      }
    }
    
    public bool DeleteDwgFileAfterTest { get; set; }
  }
}
