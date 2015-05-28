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
    }

    public interface IServiceStoreCallback
    {
        [Description("")]
        [OperationContract(IsOneWay = true)]
        void test(int t);
    }
}
