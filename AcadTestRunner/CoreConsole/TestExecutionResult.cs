using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AcadTestRunner
{
  internal class TestExecutionResult
  {
    public TestExecutionResult(int exitCode, params string[] output)
    {
      ExitCode = exitCode;
      Output = output;
    }

    public int ExitCode { get; }

    public IReadOnlyCollection<string> Output { get; }
  }
}
