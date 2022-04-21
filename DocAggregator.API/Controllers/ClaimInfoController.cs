using DocAggregator.API.Core;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Text;

namespace DocAggregator.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ClaimInfoController : ControllerBase
    {
        private readonly ILogger _logger;
        private readonly IClaimFieldRepository _fieldRepository;

        public ClaimInfoController(ILoggerFactory loggerFactory, IClaimFieldRepository fieldRepository)
        {
            _logger = loggerFactory.GetLoggerFor<ClaimInfoController>();
            _fieldRepository = fieldRepository;
        }

        [HttpPost]
        [Consumes("application/json")]
        public IActionResult Post([FromBody] ClaimRequest request)
        {
            StringBuilder output = new StringBuilder();
            output.Append("<!DOCTYPE html>");
            output.Append("<html>");

            output.Append("<head>");
            output.Append("<title>");
            output.Append("Claim info page.");
            output.Append("</title>");
            output.Append("</head>");

            output.Append("<body>");

            output.Append("<h1>");
            output.Append($"Инспектируемая заявка <{request.ClaimID}>.");
            output.Append("</h1>");

            output.Append("<table cellspacing=\"0\" border=\"1\">");

            output.Append("<caption>");
            output.Append("Перечень всех доступных полей для шаблона.");
            output.Append("</caption>");

            output.Append("<tr>");
            output.Append("<th>");
            output.Append("Имя");
            output.Append("</th>");
            output.Append("<th>");
            output.Append("Код");
            output.Append("</th>");
            output.Append("<th>");
            output.Append("Значение");
            output.Append("</th>");
            output.Append("</tr>");
            foreach (var a in _fieldRepository.GetFiledListByClaimId(request.ClaimID))
            {
                output.Append("<tr>");
                output.Append("<td>");
                output.Append(a.Item1.Replace("\r\n", "<br/>"));
                output.Append("</td>");
                output.Append("<td>");
                output.Append(a.Item2);
                output.Append("</td>");
                output.Append("<td>");
                output.Append(a.Item3);
                output.Append("</td>");
                output.Append("</tr>");
            }
            output.Append("</table>");

            output.Append("<table cellspacing=\"0\" border=\"1\">");

            output.Append("<caption>");
            output.Append("Перечень всех доступных полей полномочий для шаблона.");
            output.Append("</caption>");

            output.Append("<tr>");
            output.Append("<th>");
            output.Append("Имя");
            output.Append("</th>");
            output.Append("<th>");
            output.Append("Код");
            output.Append("</th>");
            output.Append("<th>");
            output.Append("Значение");
            output.Append("</th>");
            output.Append("</tr>");
            foreach (var a in _fieldRepository.GetFilledAccessListByClaimId(request.ClaimID))
            {
                output.Append("<tr>");
                output.Append("<td>");
                output.Append(a.Item1);
                output.Append("</td>");
                output.Append("<td>");
                output.Append(a.Item2);
                output.Append("</td>");
                output.Append("<td align=\"center\">");
                output.Append(a.Item3);
                output.Append("</td>");
                output.Append("</tr>");
            }
            output.Append("</table>");

            output.Append("</body>");

            output.Append("</html>");
            return new ContentResult
            {
                Content = output.ToString(),
                ContentType = "text/html",
                StatusCode = (int) HttpStatusCode.OK,
            };
        }
    }
}
