using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.Runtime;
using System.Diagnostics;

namespace AcadTestRunner
{
  [DebuggerStepThrough]
  internal class TestExecution
  {
    [CommandMethod("LoadAndExecuteTest")]
    public static void LoadAndExecuteTest()
    {
      var loaderNotifier = new Notification("TestLoader");
      Notification testNotifier = null;
      Type expectedException = null;

      try
      {
        var editor = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument.Editor;
        var assemblyPath = editor.GetString(Notification.GetMessage("TestLoader", "Assembly path")).StringResult;
        var className = editor.GetString(Notification.GetMessage("TestLoader", "Class name")).StringResult;
        var methodName = editor.GetString(Notification.GetMessage("TestLoader", "Method name")).StringResult;
        var attachDebugger = bool.Parse(editor.GetString(Notification.GetMessage("TestLoader", "Attach debugger")).StringResult);

        if (attachDebugger)
        {
          dynamic dte = null;

          try
          {
            dte = System.Runtime.InteropServices.Marshal.GetActiveObject("VisualStudio.DTE.13.0");
          }
          catch { }

          if (dte == null)
          {
            try
            {
              dte = System.Runtime.InteropServices.Marshal.GetActiveObject("VisualStudio.DTE.12.0");
            }
            catch { }
          }

          if (dte == null)
          {
            try
            {
              dte = System.Runtime.InteropServices.Marshal.GetActiveObject("VisualStudio.DTE.11.0");
            }
            catch { }
          }


          if (dte == null)
          {
            loaderNotifier.WriteMessage("No running instance of VisualStudio found");
          }
          else
          {
            foreach (dynamic process in dte.Debugger.LocalProcesses)
            {
              if (process.ProcessID == Process.GetCurrentProcess().Id)
              {
                process.Attach();
                loaderNotifier.WriteMessage("Debugger attached to AcCoreConsole.exe");
              }
            }
          }
        }

        testNotifier = new Notification(methodName);

        var metadata = new TestMetadata(assemblyPath, className, methodName);

        if (metadata.Type == null)
        {
          testNotifier.TestFailed("Class " + className + " not found");
          loaderNotifier.WriteMessage("Test execution finished with errors");
        }
        else
        {
          loaderNotifier.WriteMessage("Class " + metadata.Type.Name + " loaded");

          if (metadata.Method == null)
          {
            testNotifier.TestFailed("No public instance method " + methodName + " found");
            loaderNotifier.WriteMessage("Test execution finished with errors");
          }
          else
          {
            loaderNotifier.WriteMessage("Method " + methodName + " found");

            if (!metadata.HasPublicConstructor)
            {
              testNotifier.TestFailed("No public default constructor found in class " + className);
              loaderNotifier.WriteMessage("Test execution finished with errors");
            }
            else
            {
              if (metadata.ExpectedException != null)
              {
                expectedException = metadata.ExpectedException;
                loaderNotifier.WriteMessage("ExcpectedException " + expectedException.Name + " found");
              }

              var instance = Activator.CreateInstance(metadata.Type);
              loaderNotifier.WriteMessage("Instance of " + metadata.Type.Name + " created");

              loaderNotifier.WriteMessage("Executing AcadTest " + methodName);
              metadata.Type.InvokeMember(metadata.Method.Name, BindingFlags.InvokeMethod, null, instance, new object[0]);

              if (expectedException != null)
              {
                testNotifier.TestFailed("Expected exception of type " + expectedException.FullName + " not thrown");
                loaderNotifier.WriteMessage("Test execution finished with errors");
              }
              else
              {
                testNotifier.TestPassed();
                loaderNotifier.WriteMessage("Test execution finished");
              }
            }
          }
        }
      }
      catch (TargetInvocationException tie)
      {
        var e = tie.InnerException;

        if (e != null)
        {
          if (expectedException != null &&
              e.GetType().Equals(expectedException))
          {
            testNotifier.TestPassed();
          }
          else
          {
            testNotifier.TestFailed(e.Message, e.StackTrace);
            loaderNotifier.WriteMessage("Test execution finished with errors");
          }
        }
        else
        {
          loaderNotifier.WriteMessage("No exception message available");
          loaderNotifier.WriteMessage("Test execution finished with errors");
        }
      }
      catch (System.Exception e)
      {
        loaderNotifier.WriteMessage(e.Message);
        loaderNotifier.WriteMessage("Test execution finished with errors");
      }
    }
  }
}
