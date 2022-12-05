using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AcadApplication = Autodesk.AutoCAD.ApplicationServices.Application;

namespace AcadTestRunner
{
  [DebuggerStepThrough]
  internal class Notification
  {
    private const string Passed = "PASSED";
    private const string Failed = "FAILED";
    private readonly string prefix;

    public Notification(string testName)
      => prefix = $"AcadTestRunner - {testName} - ";

    internal void WriteMessage(string message)
      => AcadApplication.DocumentManager.MdiActiveDocument.Editor.WriteMessage($"{Environment.NewLine}{prefix}{message}");

    internal void TestPassed()
      => WriteMessage(Passed);

    public void TestFailed(string message)
      => TestFailed(message, null);

    public void TestFailed(string message, string stackTrace)
      => WriteMessage($"{Failed} - {message}{Environment.NewLine}{stackTrace}");

    public void TestFailed(Exception e)
    {
      var message = new StringBuilder()
                    .Append(e.Message);

      while (e.InnerException != null)
      {
        e = e.InnerException;
        message.Append(" -> ")
               .Append(e.Message);
      }

      WriteMessage($"{Failed} - {message}");
    }

    public static string GetPassedMessage(string testName)
      => GetMessage(testName, Passed);

    public static string GetFailedMessage(string testName)
      => GetMessage(testName, Failed);

    public static string GetMessage(string testName, string message)
    {
      var notification = new Notification(testName);
      return notification.prefix + message;
    }
  }
}
