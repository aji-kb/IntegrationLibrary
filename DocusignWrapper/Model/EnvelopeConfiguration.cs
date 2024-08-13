using IntegrationLibrary.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntegrationLibrary.Model
{
    public class EnvelopeConfiguration
    {
        public List<DocusignRecipient>? EmailTo { get; set; }
        public List<DocusignRecipient>? EmailCC { get; set; }
        public string? EmailSubject { get; set; }
        public EnvStatus EnvStatus { get; set; }

    }
}
