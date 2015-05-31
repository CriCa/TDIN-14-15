using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.ComponentModel;

namespace BookEditor
{
    [ServiceContract]
    public interface IServiceBooks
    {
        [WebGet(UriTemplate = "/", ResponseFormat = WebMessageFormat.Json)]
        [Description("Get all books in store")]
        [OperationContract]
        Books getAllBooks();
    }
}
