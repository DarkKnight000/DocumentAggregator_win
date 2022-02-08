using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DocAggregator.API.Core.Tests
{
    internal static class ResponseDebugPresenter
    {
        internal static string Handle(InteractorResponseBase response)
        {
            StringBuilder result = new StringBuilder();
            foreach (var err in response.Errors)
            {
                if (result.Length != 0)
                {
                    result.AppendLine(new string('-', 14));
                }
                result.AppendLine(err.GetType().ToString());
                result.AppendLine(err.Message);
                result.AppendLine(err.StackTrace);
            }
            return result.ToString();
        }
    }
}
