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
    public interface IServiceOrders
    {
        [WebInvoke(UriTemplate = "/create", Method = "POST", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        [Description("Create a new order coming from the webapp")]
        [OperationContract]
        Response CreateOrderFromWeb(CreateData data);


        [WebInvoke(UriTemplate = "/{id}", Method = "GET", ResponseFormat = WebMessageFormat.Json)]
        [Description("Get info of order with id = {id}")]
        [OperationContract]
        OrderData GetOrder(string id);
    }
}
