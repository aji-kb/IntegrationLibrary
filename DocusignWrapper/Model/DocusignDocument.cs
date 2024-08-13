using IntegrationLibrary.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntegrationLibrary.Model
{
    public class DocusignDocument
    {
        public  DocumentType DocumentType { get; set; }
        public string? Name { get; set; }
        public string? Content { get; set; }
    }
}
