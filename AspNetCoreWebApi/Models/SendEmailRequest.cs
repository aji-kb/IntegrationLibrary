using DocuSign.eSign.Model;
using IntegrationLibrary.Model;

namespace Examples.Models
{
    /// <summary>
    /// Information required to Send Email for Docusign
    /// </summary>
    public class SendEmailRequest
    {
        /// <summary>
        /// Docusign AccountId 
        /// </summary>
        public string? AccountId { get; set; }
        /// <summary>
        /// AccessToken to be sent in Docusign Request
        /// </summary>
        public string? AccessToken { get; set; }
        /// <summary>
        /// List of Documents to be signed <see cref="DocusignDocument"/>
        /// </summary>
        public List<DocusignDocument>? Documents { get; set; }

        /// <summary>
        /// List of Signers <see cref="DocusignRecipient"/>
        /// </summary>
        public List<DocusignRecipient>? EmailTo { get;set; }
        /// <summary>
        /// List of Recipients who are copied <see cref="DocusignRecipient"/>
        /// </summary>
        public List<DocusignRecipient>? EmailCC { get; set; }
        /// <summary>
        /// Subject of the Email sent to recipients
        /// </summary>
        public string? EmailSubject { get; set; }

    }
}
