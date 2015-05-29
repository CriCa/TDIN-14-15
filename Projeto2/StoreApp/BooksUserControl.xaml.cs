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
using System.Text.RegularExpressions;

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

            app.BooksEvent += new EventHandler(refreshBookList);

            refreshBookList(null, null);
        }

        private void refreshBookList(object sender, EventArgs e)
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

                if (rep.State != "success")
                    System.Windows.MessageBox.Show(Window.GetWindow(this), "The book is already registered!", "Repeated book", MessageBoxButton.OK, MessageBoxImage.Error);

                refreshBookList(null, null);
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

                refreshBookList(null, null);
            }
        }

        private void SellBook(object sender, RoutedEventArgs e)
        {
            if (SelectedBook != null)
            {
                if (SelectedBook.quantity >= sellQuantity.Value)
                {
                    if (ClientName.Text != "")
                    {
                        long qtd = (long)sellQuantity.Value;
                        double price = SelectedBook.price;
                        string clientName = ClientName.Text;

                        ClientName.Text = "";

                        app.clientProxy.sellBook(SelectedBook, (int)qtd);

                        new Printer.MainWindow(Window.GetWindow(this), SelectedBook.title, qtd, price, (double)qtd * price, clientName).Show();

                        refreshBookList(null, null);
                    }
                    else System.Windows.MessageBox.Show(Window.GetWindow(this), "Specify the name of the client!", "No client name", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                else
                {
                    if (ClientName.Text != "" && Regex.IsMatch(ClientName.Text,
                @"^(?("")("".+?(?<!\\)""@)|(([0-9a-z]((\.(?!\.))|[-!#\$%&'\*\+/=\?\^`\{\}\|~\w])*)(?<=[0-9a-z])@))" +
                @"(?(\[)(\[(\d{1,3}\.){3}\d{1,3}\])|(([0-9a-z][-\w]*[0-9a-z]*\.)+[a-z0-9][\-a-z0-9]{0,22}[a-z0-9]))$",
                RegexOptions.IgnoreCase))
                    {
                        long qtd = (long)sellQuantity.Value;
                        double price = SelectedBook.price;
                        string clientEmail = ClientName.Text;

                        ClientName.Text = "";

                        app.clientProxy.orderBook(SelectedBook, clientEmail, (int) qtd);

                        refreshBookList(null, null);
                        app.RefreshOrders();
                    }
                    else System.Windows.MessageBox.Show(Window.GetWindow(this), "Enter a valid client email!", "Client email error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else System.Windows.MessageBox.Show(Window.GetWindow(this), "Select a book first!", "No book selected", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void BookSelectedChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SelectedBook != null)
            {
                UpdateButton.IsEnabled = true;

                SellButton.IsEnabled = true;
                sellQuantity.IsEnabled = true;
                ClientName.IsEnabled = true;
                sellQuantity.Value = 1;

                if (SelectedBook.quantity >= sellQuantity.Value)
                {
                    SellButton.Content = "Sell";
                    ClientPlaceholder.Text = "Client Name";
                }
                else
                {
                    SellButton.Content = "Order";
                    ClientPlaceholder.Text = "Client Email";
                }
            }
            else
            {
                UpdateButton.IsEnabled = false;
                sellQuantity.IsEnabled = false;
                ClientName.IsEnabled = false;

                SellButton.Content = "Sell";
                ClientPlaceholder.Text = "Client Name";
                sellQuantity.Value = 1;
            }
        }

        private void QuantityChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (SelectedBook != null)
            {
                if (SelectedBook.quantity >= sellQuantity.Value)
                {
                    SellButton.Content = "Sell";
                    ClientPlaceholder.Text = "Client Name";
                }
                else
                {
                    SellButton.Content = "Order";
                    ClientPlaceholder.Text = "Client Email";
                }
            }
        }
    }
}
