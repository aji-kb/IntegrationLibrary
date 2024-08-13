using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntegrationLibrary.Model
{
    public class DocusignCredentials
    {
        public string? ClientId { get; set; }
        public string? ImpersonatedUserId { get; set; }
        public string? PrivateRSAKey { get; set; }
        public List<string>? Scopes { get; set; }
        public string? RedirectUri { get; set; }
    }
}
