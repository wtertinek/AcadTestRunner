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
                                 .Select(m => (Method: m,
                                               AcadTestAttribute: m.GetCustomAttributes(typeof(AcadTestAttribute), false).FirstOrDefault()))
                                 .FirstOrDefault(m => m.Method.Name == methodName &&
                                                      m.AcadTestAttribute != null);

        if (testMethodInfo != default)
        {
          TestMethod = testMethodInfo.Method;
          AcadTestAttribute = testMethodInfo.AcadTestAttribute as AcadTestAttribute;

          var expectedExceptionAttribute = TestMethod.GetCustomAttribute(typeof(AcadExpectedExceptionAttribute), false);

          if (expectedExceptionAttribute != null)
          {
            ExpectedException = (expectedExceptionAttribute as AcadExpectedExceptionAttribute).ExpectedException;
          }

          var testSetupMethodInfo = Type.GetMethods(BindingFlags.Public | BindingFlags.Instance)
                                        .Select(m => (Method: m,
                                                      AcadTestSetupAttribute: m.GetCustomAttributes(typeof(AcadTestSetupAttribute), false).FirstOrDefault()))
                                        .FirstOrDefault(m => m.AcadTestSetupAttribute != null);

          if (testSetupMethodInfo != default)
          {
            TestSetupMethod = testSetupMethodInfo.Method;
          }
        }
      }
    }

    public string ClassName { get; }

    public string MethodName { get; }

    public Type Type { get; }

    public bool HasPublicConstructor
      => Type.GetConstructors()
             .Any(c => c.IsPublic &&
                       !c.GetParameters()
                         .Any());

    public MethodInfo TestMethod { get; }

    public MethodInfo TestSetupMethod { get; }

    public AcadTestAttribute AcadTestAttribute { get; }

    public Type ExpectedException { get; }
  }
}
