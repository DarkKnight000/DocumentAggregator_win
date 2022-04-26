using DocAggregator.API.Core;
using DocAggregator.API.Core.Models;
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
        private readonly IClaimRepository _claimRepository;
        private readonly IClaimFieldRepository _fieldRepository;

        public ClaimInfoController(ILoggerFactory loggerFactory, IClaimRepository claimRepository, IClaimFieldRepository fieldRepository)
        {
            _logger = loggerFactory.GetLoggerFor<ClaimInfoController>();
            _claimRepository = claimRepository;
            _fieldRepository = fieldRepository;
        }

        [HttpPost]
        [Consumes("application/json")]
        public IActionResult Post([FromBody] ClaimRequest request)
        {
            Claim claim = _claimRepository.GetClaim(request.ClaimID);
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
            output.Append($"Инспектируемая заявка <{claim.ID}[{claim.TypeID},{claim.SystemID}]>.");
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
            foreach (var field in _fieldRepository.GetFiledListByClaimId(claim))
            {
                output.Append("<tr>");
                output.Append("<td>");
                output.Append(field.Category);
                output.Append("<br/>");
                output.Append(field.Attribute);
                output.Append("</td>");
                output.Append("<td>");
                output.Append(field.NumeralID?.ToString() ?? field.VerbousID);
                output.Append("</td>");
                output.Append("<td>");
                output.Append(field.Value);
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
            foreach (var accessRight in _fieldRepository.GetFilledAccessListByClaimId(claim))
            {
                output.Append("<tr>");
                output.Append("<td rowspan=\"2\">");
                output.Append(accessRight.Name);
                output.Append("</td>");
                output.Append("<td>");
                output.Append($"*{accessRight.NumeralID}a");
                output.Append("</td>");
                output.Append("<td align=\"center\">");
                output.Append(accessRight.IsAllowed);
                output.Append("</td>");
                output.Append("</tr>");
                output.Append("<tr>");
                output.Append("</td>");
                output.Append("<td>");
                output.Append($"*{accessRight.NumeralID}d");
                output.Append("</td>");
                output.Append("<td align=\"center\">");
                output.Append(accessRight.IsDenied);
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
