using DocAggregator.API.Core;
using DocAggregator.API.Core.Models;
using DocAggregator.API.Infrastructure.OracleManaged;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Oracle.ManagedDataAccess.Client;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DocAggregator.API.Pages
{
    public class ServicePageModel : PageModel
    {
        private readonly ILogger _logger;
        private readonly SqlConnectionResource _sqlResource;
        private readonly TemplateMap _templateMap;
        private readonly Infrastructure.OpenXMLProcessing.EditorService _editorService;
        private readonly IClaimRepository _claimRepository;
        private readonly IClaimFieldRepository _fieldRepository;

        public int? Queries { get; set; }
        public int? Bindings { get; set; }
        public bool Connection { get; set; }
        public int? Templates { get; set; }
        public bool Editor { get; set; }
        public string EditorPath { get; set; }
        public bool Scripts { get; set; }
        public string ScriptsPath { get; set; }
        public bool IsCorrect { get; set; } = true;

        public ServicePageModel(ILoggerFactory loggerFactory,
            SqlConnectionResource sqlResource, TemplateMap templateMap,
            IEditorService editorService, IClaimRepository claimRepository, IClaimFieldRepository fieldRepository)
        {
            _logger = loggerFactory.GetLoggerFor<ClaimInfoModel>();
            _sqlResource = sqlResource;
            _templateMap = templateMap;
            _editorService = editorService as Infrastructure.OpenXMLProcessing.EditorService;
            _claimRepository = claimRepository;
            _fieldRepository = fieldRepository;
        }

        public void OnGet()
        {
            Queries = _sqlResource.Count;
            Bindings = _templateMap.Count;
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
            Templates = Directory.GetFiles(_editorService.TemplatesDirectory, "*.doc?", SearchOption.AllDirectories).Count();
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
            return;
        }
    }
}