using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities;

namespace BookEditor
{
    public partial class Service : IServiceBooks
    {
        public Books getAllBooks()
        {
            Books list = new Books();
            
            List<Values> result = BookTable.Instance.all;

            foreach (Values v in result)
            {
                list.Add(new BookData(
                    (long)v.getValue(BookTable.KEY_ID),
                    (string)v.getValue(BookTable.KEY_TITLE),
                    (long)v.getValue(BookTable.KEY_QUANTITY),
                    (double)v.getValue(BookTable.KEY_PRICE)));
            }

            return list;
        }
    }
}
