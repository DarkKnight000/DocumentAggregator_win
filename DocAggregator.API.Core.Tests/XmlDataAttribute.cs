using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;
using Xunit.Sdk;

namespace DocAggregator.API.Core.Tests
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public class XmlDataAttribute : DataAttribute
    {
        private readonly string _fileName;
        public XmlDataAttribute(string fileName)
        {
            _fileName = fileName;
        }

        public override IEnumerable<object[]> GetData(MethodInfo testMethod)
        {
            var pars = testMethod.GetParameters();
            var parameterTypes = pars.Select(par => par.ParameterType).ToArray();
            if (parameterTypes.Length == 1)
            {
                if (parameterTypes[0].Equals(typeof(XDocument)))
                {
                    return new[] { new[] { XDocument.Load(_fileName) } };
                }
                else if (parameterTypes[0].Equals(typeof(string)))
                {
                    return new[] { new[] { File.ReadAllText(_fileName) } };
                }
            }
            return Enumerable.Empty<object[]>();
        }
    }
}
