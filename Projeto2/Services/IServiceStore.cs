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
    [ServiceContract(CallbackContract = typeof(IServiceStoreCallback), SessionMode = SessionMode.Required)]
    public interface IServiceStore
    {
        [Description("Get books in store")]
        [OperationContract]
        Books getBooks();

        [Description("Add book to the store")]
        [OperationContract]
        Response addBook(BookData book);

        [Description("Update book info in the store")]
        [OperationContract]
        Response updateBook(BookData book);

        [Description("Sell book")]
        [OperationContract]
        Response sellBook(BookData book, int quantity);

        [Description("Order a book")]
        [OperationContract]
        Response orderBook(BookData book, long quantity, string clientName, string clientEmail, string clientPassowrd, string address);

        [Description("Get orders")]
        [OperationContract]
        Orders getOrders();

        [Description("Get requests")]
        [OperationContract]
        Requests getRequests();

        [Description("Received request from the warehouse")]
        [OperationContract]
        Response ReceivedRequest(RequestData request);
    }

    public interface IServiceStoreCallback
    {
        [Description("")]
        [OperationContract(IsOneWay = true)]
        void UpdateOrders();
    }
}
