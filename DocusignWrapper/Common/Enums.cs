using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntegrationLibrary.Common
{
    public enum DocumentType
    {
        Docx,
        PDF,
        HTML
    }

    public enum RecipientType
    {
        To,
        CC
    }

    public enum EnvStatus
    {
        Sent,
        Created
    }

}
