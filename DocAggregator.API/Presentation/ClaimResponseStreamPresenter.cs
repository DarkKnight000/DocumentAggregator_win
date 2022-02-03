using DocAggregator.API.Core;
using Microsoft.AspNetCore.Mvc;

namespace DocAggregator.API.Presentation
{
    public class ClaimResponseStreamPresenter
    {
        public FileStreamResult Handle(ClaimResponse response)
        {
            var result = "Success: " + response.Success.ToString();
            var stream = new System.IO.MemoryStream(new System.Text.ASCIIEncoding().GetBytes(result));
            return new FileStreamResult(stream, "application/octet-stream");
        }
    }
}
