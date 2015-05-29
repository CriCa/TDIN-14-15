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
    [ServiceContract(SessionMode = SessionMode.Required)]
    public interface IServiceWarehouse
    {
        [Description("Ship book from warehouse to store")]
        [OperationContract]
        Response ship(RequestData request);
    }
}
