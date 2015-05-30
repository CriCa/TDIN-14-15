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
    public interface IServiceUsers
    {

        [WebGet(UriTemplate = "/{id}/orders", ResponseFormat = WebMessageFormat.Json)]
        [Description("Get all the tickets from user with {id}")]
        [OperationContract]
        Orders getOrdersFrom(string id);

    }
}
