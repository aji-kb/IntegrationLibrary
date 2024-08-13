using DocuSign.eSign.Client;
using Microsoft.Extensions.Configuration;
using IntegrationLibrary.Model;
using IntegrationLibrary.Abstractions;
using DocuSign.eSign.Model;
using DocuSign.eSign.Api;
using Microsoft.IdentityModel.Abstractions;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace IntegrationLibrary
{
    public class DocusignWrapper : IDocusignWrapper
    {

        private readonly IConfiguration _configuration;
        private readonly ILogger<DocusignWrapper> _logger;

        public DocusignWrapper(IConfiguration configuration, ILogger<DocusignWrapper> logger) 
        { 
            _configuration = configuration;
            _logger = logger;
        }

        /// <summary>
        /// Authenticate the given credentials against Docusign and return access token and refresh token
        /// </summary>
        /// <param name="docusignCredentials">Credentials for authentication <see cref="DocusignCredentials"/></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public Token Authenticate(DocusignCredentials docusignCredentials)
        {
            var docusignClient = new DocuSignClient();

            if (docusignCredentials != null && !string.IsNullOrEmpty(docusignCredentials.ClientId) && !string.IsNullOrEmpty(docusignCredentials.ImpersonatedUserId) && !string.IsNullOrEmpty(docusignCredentials.PrivateRSAKey))
            {

                var oauthBasePath = _configuration.GetSection("Docusign:AuthorizationEndpoint").Value;
                var expiresInHours = Convert.ToInt32(_configuration.GetSection("Docusign:TokenExpiryInHours").Value);

                try
                {

                    var token = docusignClient.RequestJWTUserToken(docusignCredentials.ClientId, docusignCredentials.ImpersonatedUserId, oauthBasePath, System.Text.Encoding.UTF8.GetBytes(docusignCredentials.PrivateRSAKey), expiresInHours, docusignCredentials.Scopes);

                    return new Token
                    {
                        AccessToken = token.access_token,
                        ExpiresIn = token.expires_in,
                        RefreshToken = token.refresh_token,
                        TokenType = token.token_type
                    };
                }
                catch (Exception ex)
                {
                    if (ex.Message.Contains("consent_required"))
                    {
                        return new Token
                        {
                            ErrorMessage = BuildConsentUrl(docusignCredentials.ClientId, docusignCredentials.RedirectUri ?? "")
                        };
                    }
                    else
                    {
                        _logger.LogError(ex, $"Error in Docusign Authentication for ClientId : {docusignCredentials.ClientId}");

                        return new Token
                        {
                            ErrorMessage = "Error in Docusign Authentication"
                        };
                    }
                }

            }
            else
            {
                throw new ArgumentException("Docusign Credentials missing (ClientId, ImpersonatedUserId and PrivateRSAKey are mandatory)");
            }
        }


        /// <summary>
        /// Send a Document for signing to users by email 
        /// </summary>
        /// <param name="envelopeConfiguration">Details of the users to whom the document is to be sent</param>
        /// <param name="docusignDocuments">List of documents to be sent</param>
        public SendEnvelopeResponse SendEnvelopeByEmail(string accountId, string accessToken, EnvelopeConfiguration envelopeConfiguration, List<DocusignDocument> docusignDocuments, List<DocusignAnchor> docusignAnchors)
        {
            _logger.LogTrace("Start - SendEnvelopeByEmail. AccountId: {accountId}", accountId);

            //Create the envelope definition using the supplied information
            var sendEnvelopeResponse = new SendEnvelopeResponse();
            var envelopeDefinition = MakeEnvelope(envelopeConfiguration, docusignDocuments, docusignAnchors);

            var docusignClient = new DocuSignClient();
            docusignClient.Configuration.DefaultHeader.Add("Authorization", "Bearer " + accessToken);

            //Create Envelope to send the email
            EnvelopesApi envelopesApi = new EnvelopesApi(docusignClient);
            var result = envelopesApi.CreateEnvelope(accountId, envelopeDefinition);

            sendEnvelopeResponse.EnvelopeId = result.EnvelopeId;

            _logger.LogTrace("End - SendEnvelopeByEmail. AccountId: {accountId}, {EnvelopeId}", accountId, result.EnvelopeId);
            
            return sendEnvelopeResponse;
        }

        private EnvelopeDefinition MakeEnvelope(EnvelopeConfiguration envelopeConfiguration, List<DocusignDocument> docusignDocuments, List<DocusignAnchor> docusignAnchors)
        {
            EnvelopeDefinition envelopeDefinition = new EnvelopeDefinition();
            envelopeDefinition.EmailSubject = envelopeConfiguration.EmailSubject;

            var i = 1;
            foreach(DocusignDocument docusignDocument in docusignDocuments)
            {
                if (docusignDocument.Content != null)
                {
                    Document doc = new Document();
                    doc.DocumentId = (i++).ToString();
                    doc.Name = docusignDocument.Name;
                    doc.DocumentBase64 = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(docusignDocument.Content));
                    doc.FileExtension = docusignDocument.DocumentType.ToString();

                    if (envelopeDefinition.Documents == null) envelopeDefinition.Documents = new List<Document>();
                    envelopeDefinition.Documents.Add(doc);
                }
                else
                {
                    _logger.LogError($"Document not having content. So ignoring the document. Document name: {docusignDocument.Name}");
                }
            }

            Recipients recipients = new Recipients();

            if (envelopeConfiguration != null && envelopeConfiguration.EmailTo != null)
            {
                int j = 1;
                foreach (var docusignRecipient in envelopeConfiguration.EmailTo)
                {
                    //Add the signers to the document
                    Signer signer = new Signer();
                    signer.RecipientId = (j++).ToString();
                    signer.Name = docusignRecipient.Name;
                    signer.Email = docusignRecipient.Email;

                    foreach (var docusignAnchor in docusignAnchors)
                    {
                        Tabs tabs = new Tabs();
                        if (!string.IsNullOrEmpty(docusignAnchor.AnchorString))
                        {
                            //Add anchor information where the sign here should be placed
                            SignHere signHere = new SignHere
                            {
                                AnchorString = docusignAnchor.AnchorString,
                                AnchorUnits = docusignAnchor.AnchorUnits,
                                AnchorXOffset = docusignAnchor.AnchorXOffset,
                                AnchorYOffset = docusignAnchor.AnchorYOffset,
                            };

                            if (tabs.SignHereTabs == null) tabs.SignHereTabs = new List<SignHere>();
                            tabs.SignHereTabs.Add(signHere);
                        }

                        signer.Tabs = tabs;

                    }

                    if (recipients.Signers == null) recipients.Signers = new List<Signer>();
                    recipients.Signers.Add(signer);

                }


                if (envelopeConfiguration.EmailCC != null)
                {
                    foreach (var docusignRecipient in envelopeConfiguration.EmailCC)
                    {
                        CarbonCopy carbonCopy = new CarbonCopy();

                        carbonCopy.Email = docusignRecipient.Email;
                        carbonCopy.Name = docusignRecipient.Name;
                        carbonCopy.RecipientId = j++.ToString();

                        if (recipients.CarbonCopies == null) recipients.CarbonCopies = new List<CarbonCopy>();
                        recipients.CarbonCopies.Add(carbonCopy);

                    }
                }

                envelopeDefinition.Recipients = recipients;

                envelopeDefinition.Status = envelopeConfiguration.EnvStatus.ToString();

                return envelopeDefinition;
            }
            else
                throw new ArgumentNullException("EnvelopeConfiguration and/or Email To recipient information are missing");
        }

        private string BuildConsentUrl(string clientId,string redirectUri)
        {
            var scopes = "signature impersonation";

            return _configuration.GetSection("DocuSign:AuthorizationEndpoint")?.Value + "?response_type=code" +
                            "&scope=" + scopes +  "&client_id=" + clientId  +
                            "&redirect_uri=" + redirectUri;

        }
    }
}
