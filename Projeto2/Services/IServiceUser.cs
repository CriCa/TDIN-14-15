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
    public interface IServiceUser
    {
        [WebInvoke(UriTemplate = "/login", Method = "POST", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        [Description("Login in the system")]
        [OperationContract]
        LoginResponse login(LoginData data);

        [WebInvoke(UriTemplate = "/logout", Method = "POST", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        [Description("Logout of the system")]
        [OperationContract]
        Response logout(LogoutData data);

        [WebInvoke(UriTemplate = "/register", Method = "POST", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        [Description("Register in the system")]
        [OperationContract]
        Response register(RegisterData data);
    }
}
