using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IntegrationLibrary.Model;

namespace IntegrationLibrary.Abstractions
{
    public interface IDocusignWrapper
    {
        Token Authenticate(DocusignCredentials docusignCredentials);

        SendEnvelopeResponse SendEnvelopeByEmail(string accountId, string accessToken, EnvelopeConfiguration envelopeConfiguration, List<DocusignDocument> docusignDocuments, List<DocusignAnchor> docusignAnchors);
    }
}
