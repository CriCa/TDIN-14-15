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
using Xceed.Wpf.Toolkit;

namespace StoreApp
{
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class BooksUserControl : UserControlNotifiable
    {
        private App app = Application.Current as App;

        private ObservableCollection<BookData> books;

        public ObservableCollection<BookData> BooksList
        {
            get { return books; }
            set { SetField(ref books, value, "BooksList"); }
        }

        private BookData selectedBook;

        public BookData SelectedBook
        {
            get { return selectedBook; }
            set { SetField(ref selectedBook, value, "SelectedBook"); }
        }

        public BooksUserControl()
        {
            InitializeComponent();
            this.DataContext = this;

            init();
        }

        private void init()
        {
            books = new ObservableCollection<BookData>();

            refreshBookList();
        }

        private void refreshBookList()
        {
            Books bs = app.clientProxy.getBooks();
            books.Clear();
            
            foreach (BookData b in bs)
                books.Add(b);
        }

        private void AddBook(object sender, RoutedEventArgs e)
        {
            BookDialog dialog = new BookDialog(Window.GetWindow(this));

            if (dialog.ShowDialog() == true)
            {
                BookData b = new BookData();
                b.title = dialog.BookTitle;
                b.quantity = dialog.BookQuantity;
                b.price = dialog.BookPrice;

                Response rep = app.clientProxy.addBook(b);

                if(rep.State != "success")
                    System.Windows.MessageBox.Show(Window.GetWindow(this), "The book is already registered!", "Repeated book", MessageBoxButton.OK, MessageBoxImage.Error);

                refreshBookList();
            }
        }

        private void UpdateBook(object sender, RoutedEventArgs e)
        {
            BookDialog dialog = new BookDialog(Window.GetWindow(this), SelectedBook.title, SelectedBook.quantity, SelectedBook.price);

            if (dialog.ShowDialog() == true)
            {
                SelectedBook.title = dialog.BookTitle;
                SelectedBook.quantity = dialog.BookQuantity;
                SelectedBook.price = dialog.BookPrice;

                app.clientProxy.updateBook(SelectedBook);

                refreshBookList();
            }
        }

        private void SellBook(object sender, RoutedEventArgs e)
        {
            if (SelectedBook != null) {
                if (SelectedBook.quantity >= sellQuantity.Value)
                {
                    if (ClientName.Text != "")
                    {
                        long qtd = (long)sellQuantity.Value;
                        double price = SelectedBook.price;
                        string clientName = ClientName.Text;

                        ClientName.Text = "";

                        Console.WriteLine("contact server to do the sell");

                        new Printer.MainWindow(SelectedBook.title, qtd, price, (double)qtd * price, clientName).Show();

                        refreshBookList();
                    }
                    else System.Windows.MessageBox.Show(Window.GetWindow(this), "Specify the name of the client!", "No client name", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                else
                {
                    // TODO
                    Console.WriteLine("creating order");
                }
            }
            else System.Windows.MessageBox.Show(Window.GetWindow(this), "Select a book first!", "No book selected", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void BookSelectedChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SelectedBook != null)
            {
                UpdateButton.IsEnabled = true;

                if (SelectedBook.quantity == 0)
                {
                    SellButton.IsEnabled = false;
                    sellQuantity.IsEnabled = false;
                    ClientName.IsEnabled = false;
                }
                else
                {
                    SellButton.IsEnabled = true;
                    sellQuantity.IsEnabled = true;
                    ClientName.IsEnabled = true;
                    sellQuantity.Value = 1;
                }
            }
            else
            {
                UpdateButton.IsEnabled = false;
                sellQuantity.IsEnabled = false;
                ClientName.IsEnabled = false;
            }
        }
    }
}
