using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AcadTestRunner
{
  [DebuggerStepThrough]
  public class TestResult
  {
    private TestResult(IReadOnlyCollection<string> fullOutput)
    {
      Passed = true;
      Message = "";
      BuildFullOutput(fullOutput);
    }

    private TestResult(string message, IReadOnlyCollection<string> fullOutput)
    {
      Passed = false;
      Message = message;
      BuildFullOutput(fullOutput);
    }

    private void BuildFullOutput(IReadOnlyCollection<string> fullOutput)
    {
      var skipEverySecondLine = true;

      for (int i = 1; i < fullOutput.Count - 1; i += 2)
      {
        if (!string.IsNullOrEmpty(fullOutput.ElementAt(i).Trim()))
        {
          skipEverySecondLine = false;
        }
      }

      if (skipEverySecondLine)
      {
        var builder = new StringBuilder();

        for (int i = 0; i < fullOutput.Count; i += 2)
        {
          builder.AppendLine(fullOutput.ElementAt(i));
        }

        FullOutput = builder.ToString();
      }
      else
      {
        FullOutput = string.Join(Environment.NewLine, fullOutput);
      }
    }

    public bool Passed { get; private set; }

    public string Message { get; private set; }

    public string FullOutput { get; private set; }

    public void DebugPrintFullOutput(string testName)
    {
      var header = "---------- " + testName + " ----------";
      Debug.WriteLine(header);
      Debug.WriteLine(FullOutput);
      Debug.WriteLine(new string('-', header.Length));
    }

    internal static TestResult TestPassed(IReadOnlyCollection<string> fullOutput)
    {
      return new TestResult(fullOutput);
    }

    internal static TestResult TestFailed(string message)
    {
      return TestFailed(message, new string[0]);
    }

    internal static TestResult TestFailed(string message, IReadOnlyCollection<string> fullOutput)
    {
      return new TestResult(message, fullOutput);
    }
  }
}
