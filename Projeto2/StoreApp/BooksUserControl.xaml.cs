using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using StoreApp.BookEditorServices;

namespace StoreApp
{
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class BooksUserControl : UserControlNotifiable
    {
        App app = Application.Current as App;

        private ObservableCollection<string> books;

        public ObservableCollection<string> BooksList
        {
            get
            {
                return books;
            }

            set
            {
                SetField(ref books, value, "BooksList");
            }
        }


        public BooksUserControl()
        {
            InitializeComponent();
            this.DataContext = this;

            init();
            
        }

        private void init()
        {
            books = new ObservableCollection<string>();

            refreshBookList();
        }

        private void refreshBookList()
        {
            Books bs = app.clientProxy.getBooks();
            books.Clear();
            
            foreach (BookData b in bs)
                books.Add(b.title);
        }

        private void AddBook(object sender, RoutedEventArgs e)
        {

            BookData b = new BookData();
            b.title = "cenas";
            b.quantity = 2;
            b.price = 3.3;

            app.clientProxy.addBook(b);

            refreshBookList();
        }

        private void UpdateBook(object sender, RoutedEventArgs e)
        {

            refreshBookList();
        }

        private void SellBook(object sender, RoutedEventArgs e)
        {

            
            refreshBookList();

            new Printer.MainWindow("title", 2, 3.0, 1.1, "cenas").Show();
        }
    }
}
