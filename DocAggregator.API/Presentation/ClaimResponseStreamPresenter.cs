using DocAggregator.API.Core;
using Microsoft.AspNetCore.Mvc;

namespace DocAggregator.API.Presentation
{
    public class ClaimResponseStreamPresenter
    {
        public FileStreamResult Handle(ClaimResponse response)
        {
            var stream = System.IO.File.OpenRead(response.File);
            return new FileStreamResult(stream, "application/octet-stream");
        }
    }
}
