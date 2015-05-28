using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookEditor
{
    public partial class Service : IServiceWarehouse
    {
        public Response ship(BookData book, long quantity)
        {
            callback.test(2);

            return new Response("success", "book successfully shipped");
        }
    }
}
