using DocAggregator.API.Core;
using Microsoft.AspNetCore.Mvc;

namespace DocAggregator.API.Presentation
{
    public class InternalErrorResponsePresenter
    {
        public ObjectResult Handle(ClaimResponse response)
        {
            return new ObjectResult(response.Errors)
            {
                StatusCode = 500,
            };
        }
    }
}
