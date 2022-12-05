using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AcadTestRunner
{
  [DebuggerStepThrough]
  [AttributeUsage(AttributeTargets.Method)]
  public class AcadTestAttribute : Attribute
  {
    public AcadTestAttribute()
      : this(null)
    {
    }

    public AcadTestAttribute(string dwgFilePath)
      => DwgFilePath = dwgFilePath;

    internal string DwgFilePath { get; }
  }
}
