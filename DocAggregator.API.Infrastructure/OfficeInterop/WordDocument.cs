using DocAggregator.API.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Word = Microsoft.Office.Interop.Word;

namespace DocAggregator.API.Infrastructure.OfficeInterop
{
    public class WordDocument : IDocument
    {
        public Word.Document Body { get; set; }
    }
}
