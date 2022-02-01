using DocAggregator.API.Core;
using Microsoft.AspNetCore.Mvc;

namespace DocAggregator.API.Presentation
{
    public class ClaimResponseStreamPresenter
    {
        public FileStreamResult Handle(ClaimResponse response)
        {
            var stream = new System.IO.MemoryStream(new System.Text.ASCIIEncoding().GetBytes("davay, chitay!"));
            return new FileStreamResult(stream, "application/octet-stream");
        }
    }
}
