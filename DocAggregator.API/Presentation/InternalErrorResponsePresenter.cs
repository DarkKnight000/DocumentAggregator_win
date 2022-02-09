using DocAggregator.API.Core;
using Microsoft.AspNetCore.Mvc;
using System.Text;

namespace DocAggregator.API.Presentation
{
    public class InternalErrorResponsePresenter
    {
        public ObjectResult Handle(ClaimResponse response)
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
            return new ObjectResult(result.ToString())
            {
                StatusCode = 500,
            };
        }
    }
}
