using DocAggregator.API.Core;
using DocAggregator.API.Infrastructure.OracleManaged;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Oracle.ManagedDataAccess.Client;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace DocAggregator.API.Pages
{
    /// <summary>
    /// Служебная страница для отладки сервиса.
    /// </summary>
    public class ServicePageModel : PageModel
    {
        private readonly ILogger _logger;
        private readonly SqlConnectionResource _sqlResource;
        private readonly TemplateMap _templateMap;
        private readonly ModelBind _modelBind;
        private readonly ClaimInteractor _claimInteractor;
        private readonly Infrastructure.OpenXMLProcessing.EditorService _editorService;

        public int? Queries { get; set; }
        public int? Bindings { get; set; }
        public bool Connection { get; set; }
        public IEnumerable<(string, int)> Templates { get; set; }
        public IEnumerable<(string, int)> Files { get; set; }
        public bool Editor { get; set; }
        public string EditorPath { get; set; }
        public bool Scripts { get; set; }
        public string ScriptsPath { get; set; }
        public bool Server { get; set; }
        public IEnumerable<Presentation.HistoryEntry> LogHistory { get; set; }
        public bool IsCorrect { get; set; } = true;
        public HtmlString Dump { get; set; }

        public ServicePageModel(ILoggerFactory loggerFactory,
            SqlConnectionResource sqlResource, TemplateMap templateMap, ModelBind modelBind,
            ClaimInteractor claimInteractor, IEditorService editorService)
        {
            _logger = loggerFactory.GetLoggerFor<ServicePageModel>();
            _sqlResource = sqlResource;
            _templateMap = templateMap;
            _modelBind = modelBind;
            _claimInteractor = claimInteractor;
            _editorService = editorService as Infrastructure.OpenXMLProcessing.EditorService;
        }

        public void OnGet()
        {
            Queries = _sqlResource.Count;
            Bindings = _modelBind.DataBindings.Count;
            OracleConnection _connection = null;
            try
            {
                _connection = new OracleConnection(new OracleConnectionStringBuilder()
                {
                    DataSource = _sqlResource.Server,
                    UserID = _sqlResource.Username,
                    Password = _sqlResource.Password,
                }.ToString());
                _connection.Open();
                if (_connection.State == System.Data.ConnectionState.Open)
                {
                    Connection = true;
                }
            }
            catch (OracleException ex)
            {
                _logger.Error(ex, "An error occured testing a db connection.");
                Connection = false;
            }
            finally
            {
                _connection.Close();
            }
            Templates = _templateMap.BindsMap.Select(pair => (pair.Key, pair.Value.Count()));
            Files = from dir in Directory.GetDirectories(_editorService.TemplatesDirectory)
                    select
                    (
                        dir.Substring(dir.LastIndexOf(Path.DirectorySeparatorChar) + 1).ToLower(),
                        Directory.GetFiles(dir, "*.doc?", SearchOption.AllDirectories).Count()
                    );
            EditorPath = _editorService.LibreOfficeExecutable;
            if (System.IO.File.Exists(EditorPath + ".bin") && System.IO.File.Exists(EditorPath + ".exe"))
            {
                Editor = true;
            }
            ScriptsPath = _editorService.Scripts;
            if (System.IO.File.Exists(Path.Combine(ScriptsPath, "server.py")) &&
                System.IO.File.Exists(Path.Combine(ScriptsPath, "converter.py")))
            {
                Scripts = true;
            }
            if (Process.GetProcessesByName("soffice").Length != 0)
            {
                Server = true;
            }
            LogHistory = Presentation.Logger.GlobalHistory;
            return;
        }

        public void OnGetClaim(int id)
        {
            OnGet();
            var dump = new StringBuilder();
            DocumentRequest request = new DocumentRequest()
            {
                Type = "claim",
                Args = new()
                {
                    ["id"] = id.ToString(),
                    ["userip"] = "0.0.0.0"
                }
            };
            DocumentResponse response = _claimInteractor.Handle(request);
            if (response.Success)
            {
                dump.AppendLine("Processed succesful.");
                dump.AppendLine("Result:");
                dump.AppendLine();
                dump.AppendLine(System.Web.HttpUtility.HtmlEncode(new StreamReader(response.ResultStream).ReadToEnd()));
            }
            else
            {
                dump.AppendLine("Processed not succesful.");
                dump.AppendLine("Errors:");
                dump.AppendLine();
                foreach (var err in response.Errors)
                {
                    if (dump.Length != 0)
                    {
                        dump.AppendLine("<hr/>");
                    }
                    dump.AppendLine(err.GetType().ToString());
                    dump.AppendLine(err.Message);
                    dump.AppendLine(err.StackTrace);
                }
            }
            Dump = new HtmlString(dump.Replace(System.Environment.NewLine, "<br/>").ToString());
        }

        public string CountableDeclension(int num, string root, string first, string fifth, string many)
        {
            if (num < 20 && num > 10)
            {
                return string.Concat(root, many);
            }
            switch ((num % 10).ToString())
            {
                case "1":
                    return string.Concat(root, first);
                case "2":
                case "3":
                case "4":
                    return string.Concat(root, fifth);
                default:
                    return string.Concat(root, many);
            }
        }
    }
}