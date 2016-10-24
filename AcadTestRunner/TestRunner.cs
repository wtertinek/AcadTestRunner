using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace AcadTestRunner
{
  [DebuggerStepThrough]
  public static class TestRunner
  {
    private const string AppSettingAcadRootDir = "AcadRootDir";
    private const string AppSettingAddinRootDir = "AddinRootDir";
    private static string coreConsolePath;
    private static string addinPath;

    public static void Init(string acadRootDir, string addinRootDir)
    {
      if (!Directory.Exists(acadRootDir))
      {
        throw new FileNotFoundException("Directory " + acadRootDir + " not found");
      }
      else if (!File.Exists(Path.Combine(acadRootDir, "AcCoreConsole.exe")))
      {
        throw new FileNotFoundException("File " + Path.Combine(acadRootDir, "AcCoreConsole.exe") + " not found");
      }
      else if (!Directory.Exists(addinRootDir))
      {
        throw new FileNotFoundException("Directory " + addinRootDir + " not found");
      }
      else if (!File.Exists(Path.Combine(addinRootDir, Path.GetFileName(typeof(TestRunner).Assembly.Location))))
      {
        throw new FileNotFoundException("File " + Path.Combine(addinRootDir, Path.GetFileName(typeof(TestRunner).Assembly.Location)) + " not found");
      }

      TestRunner.coreConsolePath = Path.Combine(acadRootDir, "AcCoreConsole.exe");
      TestRunner.addinPath = Path.Combine(addinRootDir, Path.GetFileName(typeof(TestRunner).Assembly.Location));
    }

    public static TestResult RunTest(Type testClassType, string acadTestName)
    {
      return RunTest(new TestMetadata(testClassType, testClassType.Name, acadTestName));
    }

    public static TestResult RunTest(string testAssemblyPath, string testClassName, string acadTestName)
    {
      if (!File.Exists(testAssemblyPath))
      {
        throw new FileNotFoundException("Path " + testAssemblyPath + " not found");
      }

      return RunTest(new TestMetadata(testAssemblyPath, testClassName, acadTestName));
    }

    private static TestResult RunTest(TestMetadata metadata)
    {
      #region Parameter checks

      if (string.IsNullOrEmpty(coreConsolePath) ||
          string.IsNullOrEmpty(addinPath))
      {
        var assemblyPath = typeof(TestRunner).Assembly.Location;
        var configuration = ConfigurationManager.OpenExeConfiguration(assemblyPath);

        if (configuration.AppSettings.Settings.AllKeys.Any(key => key == AppSettingAcadRootDir))
        {
          Init(configuration.AppSettings.Settings[AppSettingAcadRootDir].Value,
               configuration.AppSettings.Settings[AppSettingAddinRootDir].Value);
        }
        else
        {
          throw new FileNotFoundException("AppSetting '" + AppSettingAcadRootDir + "' not found");
        }
      }

      #endregion

      if (metadata.Type == null)
      {
        return TestResult.TestFailed("Class " + metadata.ClassName + " not found");
      }
      else if (!metadata.HasPublicConstructor)
      {
        return TestResult.TestFailed("No public default constructor found in class " + metadata.ClassName);
      }

      var dwgFilePath = metadata.AcadTestAttribute != null ? metadata.AcadTestAttribute.DwgFilePath : null;
      AcadTestContext context = null;

      if (metadata.TestSetupMethod != null)
      {
        var instance = Activator.CreateInstance(metadata.Type);
        context = new AcadTestContext(metadata.MethodName, dwgFilePath);
        metadata.Type.InvokeMember(metadata.TestSetupMethod.Name, BindingFlags.InvokeMethod, null, instance, new object[] { context });

        if (context.CustomDwgFilePath)
        {
          if (!File.Exists(context.DwgFilePath))
          {
            return TestResult.TestFailed("File " + context.DwgFilePath + " not found");
          }
          else if (dwgFilePath != null)
          {
            return TestResult.TestFailed("DWG file path provided via AcadTestAttribute and AcadTestContext");
          }
        }

        dwgFilePath = context.DwgFilePath;
      }
      else if (dwgFilePath != null &&
               !File.Exists(dwgFilePath))
      {
        return TestResult.TestFailed("File " + dwgFilePath + " not found");
      }

      var deleteDwgFileAfterTest = string.IsNullOrEmpty(dwgFilePath);

      if (context != null)
      {
        deleteDwgFileAfterTest = context.DeleteDwgFileAfterTest;
      }

      var coreConsole = new CoreConsole(coreConsolePath, addinPath);
      var result = coreConsole.LoadAndExecuteTest(metadata.Type.Assembly.Location, metadata.Type.Name, metadata.MethodName, dwgFilePath, deleteDwgFileAfterTest);

      if (context != null &&
          context.CustomDwgFilePath &&
          context.DeleteDwgFileAfterTest)
      {
        File.Delete(context.DwgFilePath);
      }

      if (result.ExitCode == 0)
      {
        var idx = FindIndex(result.Output, Notification.GetPassedMessage(metadata.MethodName));

        if (idx >= 0)
        {
          return TestResult.TestPassed(result.Output);
        }
        else
        {
          var failedMessage = Notification.GetFailedMessage(metadata.MethodName);
          idx = FindIndex(result.Output, failedMessage);

          if (idx >= 0)
          {
            var msg = result.Output
                            .ElementAt(idx)
                            .Trim()
                            .Replace(failedMessage + " - ", "");

            return TestResult.TestFailed(msg, result.Output);
          }
          else
          {
            return TestResult.TestFailed("A general error occured", result.Output);
          }
        }
      }
      else
      {
        return TestResult.TestFailed("A general error occured", result.Output);
      }
    }

    private static int FindIndex(IReadOnlyCollection<string> output, string searchString)
    {
      return output.ToList()
                   .FindIndex(l => l.TrimStart()
                                    .StartsWith(searchString));
    }
  }
}
