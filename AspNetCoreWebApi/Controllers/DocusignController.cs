using Microsoft.AspNetCore.Mvc;
using DocuSign.eSign.Client;
using IntegrationLibrary;
using IntegrationLibrary.Abstractions;
using IntegrationLibrary.Model;
using Examples.Models;

namespace Examples.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class DocusignController : ControllerBase
    {
        private readonly ILogger<DocusignController> _logger;
        private readonly IDocusignWrapper _docusignWrapper;

        public DocusignController(ILogger<DocusignController> logger, IDocusignWrapper docusignWrapper)
        {
            _logger = logger;
            _docusignWrapper = docusignWrapper;
        }


        /// <summary>
        /// Authenticate against Docusign and get the token
        /// </summary>
        /// <param name="clientId">ClientID for Docusign</param>
        /// <param name="userId">Impersonated UserId for Docusign</param>
        /// <param name="privateRSAKey">RSA Keys from Docusign Account</param>
        /// <returns>Access Token from Docusign</returns>
        [HttpPost]
        [Route("GetDocusignToken")]
        public IActionResult GetToken(string clientId, string userId, string privateRSAKey)
        {
            var docusignClient = new DocuSignClient();

            var scopes = new List<string>
                {
                    "signature",
                    "impersonation",
                };

            var docusignCredentials = new DocusignCredentials
            {
                ClientId = clientId,
                ImpersonatedUserId = userId,
                PrivateRSAKey = privateRSAKey,
            };
            var token = _docusignWrapper.Authenticate(docusignCredentials);

            return Ok(token);


        }



        /// <summary>
        /// Initiate a email to sign the document via email 
        /// </summary>
        /// <param name="sendEmailRequest">Request details <see cref="SendEmailRequest"/></param>
        /// <returns></returns>
        [HttpPost]
        [Route("SendEmail")]
        public IActionResult SendEmail([FromBody] SendEmailRequest sendEmailRequest)
        {

            try
            {
                if (sendEmailRequest == null || sendEmailRequest.Documents == null)
                {
                    return BadRequest("Document for signing is mandatory");
                }

                if (string.IsNullOrEmpty(sendEmailRequest.AccountId))
                    return BadRequest("AccountId is mandatory");

                if (string.IsNullOrEmpty(sendEmailRequest.AccessToken))
                    return BadRequest("AccessToken for Docusign is Mandatory");

                EnvelopeConfiguration envelopeConfiguration = new EnvelopeConfiguration
                {
                    EmailTo = sendEmailRequest.EmailTo,
                    EmailCC = sendEmailRequest.EmailCC,
                    EmailSubject = sendEmailRequest.EmailSubject,
                    EnvStatus = IntegrationLibrary.Common.EnvStatus.Created
                    
                };

                List<DocusignAnchor> anchors = new List<DocusignAnchor>() {
                    new DocusignAnchor{AnchorString = "**signature_1**", AnchorUnits = "pixels", AnchorXOffset = "20", AnchorYOffset = "10"}
                };

                var response = _docusignWrapper.SendEnvelopeByEmail(sendEmailRequest.AccountId ?? "", sendEmailRequest.AccessToken ?? "", envelopeConfiguration, sendEmailRequest.Documents, anchors);

                return Ok(response.EnvelopeId);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }

        }


    }
}
