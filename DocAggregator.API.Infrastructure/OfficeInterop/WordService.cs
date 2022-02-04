using DocAggregator.API.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DocAggregator.API.Infrastructure.OfficeInterop
{
    public class WordService : IEditorService
    {
        private string _templatesDirectory;
        private string _temporaryOutputDirectory;

        public string TemplatesDirectory
        {
            get => _templatesDirectory;
            set
            {
                _templatesDirectory = System.IO.Path.GetFullPath(value);
            }
        }
        public string TemporaryOutputDirectory
        {
            get => _temporaryOutputDirectory;
            set
            {
                _temporaryOutputDirectory = System.IO.Path.GetFullPath(value);
            }
        }

        public IEnumerable<Insert> GetInserts()
        {
            return Array.Empty<Insert>();
        }
        public void SetInserts(IEnumerable<Insert> inserts)
        {
            ;
        }
    }
}
