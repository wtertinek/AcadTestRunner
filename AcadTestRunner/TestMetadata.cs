using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace AcadTestRunner
{
  [DebuggerStepThrough]
  public class TestMetadata
  {
    public TestMetadata(string assemblyPath, string className, string methodName)
      : this(Assembly.LoadFrom(assemblyPath)
                     .GetTypes()
                     .FirstOrDefault(t => t.Name == className), className, methodName)
    {
    }

    public TestMetadata(Type type, string className, string methodName)
    {
      ClassName = className;
      MethodName = methodName;

      if (type != null)
      {
        Type = type;

        var testMethodInfo = Type.GetMethods(BindingFlags.Public | BindingFlags.Instance)
                                 .Select(m => new { Method = m, AcadTestAttribute = m.GetCustomAttributes(typeof(AcadTestAttribute), false).FirstOrDefault() })
                                 .FirstOrDefault(m => m.Method.Name == methodName &&
                                                      m.AcadTestAttribute != null);

        if (testMethodInfo != null)
        {
          TestMethod = testMethodInfo.Method;
          AcadTestAttribute = testMethodInfo.AcadTestAttribute as AcadTestAttribute;

          var expectedExceptionAttribute = TestMethod.GetCustomAttribute(typeof(AcadExpectedExceptionAttribute), false);

          if (expectedExceptionAttribute != null)
          {
            ExpectedException = (expectedExceptionAttribute as AcadExpectedExceptionAttribute).ExpectedException;
          }

          var testSetupMethodInfo = Type.GetMethods(BindingFlags.Public | BindingFlags.Instance)
                                        .Select(m => new { Method = m, AcadTestSetupAttribute = m.GetCustomAttributes(typeof(AcadTestSetupAttribute), false).FirstOrDefault() })
                                        .FirstOrDefault(m => m.AcadTestSetupAttribute != null);

          if (testSetupMethodInfo != null)
          {
            TestSetupMethod = testSetupMethodInfo.Method;
          }
        }
      }
    }

    public string ClassName { get; private set; }

    public string MethodName { get; private set; }

    public Type Type { get; private set; }

    public bool HasPublicConstructor
    {
      get
      {
        return Type.GetConstructors()
                   .Any(c => c.IsPublic &&
                             c.GetParameters().Count() == 0);
      }
    }

    public MethodInfo TestMethod { get; private set; }

    public MethodInfo TestSetupMethod { get; private set; }

    public AcadTestAttribute AcadTestAttribute { get; private set; }

    public Type ExpectedException { get; private set; }
  }
}
