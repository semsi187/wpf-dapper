using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
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

namespace wpf_dapper
{
    public class Book
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public List<Author> Authors { get; set; } = new();
        public override string ToString() => $"{Name} - {Price}";
    }

    public class Author
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<Book> Books { get; set; } = new();
        public override string ToString() => Name;
    }

    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            string connectionString;


            var builder = new ConfigurationBuilder();
            builder.SetBasePath(Directory.GetCurrentDirectory());
            builder.AddJsonFile("AppConfig.json");
            var config = builder.Build();
            connectionString = config.GetConnectionString("DefaultConnection")!;

            using (var connection = new SqlConnection(connectionString))
            {

                var sql = @"SELECT a.[Id], a.[Name], b.[Name], b.[Price]
                        FROM Authors AS a
                        INNER JOIN AuthorBook AS ab 
                        ON a.Id = ab.AuthorId
                        INNER JOIN Books AS b
                        ON b.Id = ab.BookId";
                var authors = connection.Query<Author, Book, Author>(sql,
                    (author, book) =>
                    {
                        author.Books.Add(book);
                        return author;
                    },
                    splitOn: "Name");

                var result = authors.GroupBy(a => a.Id).Select(g =>
                {
                    var groupedBook = g.First();
                    groupedBook.Books = g.Select(b => b.Books.Single()).ToList();
                    return groupedBook;
                });


                DataTable dataTable = new DataTable();

                dataTable.Columns.Add("Author Id", typeof(int));
                dataTable.Columns.Add("Author Name", typeof(string));
                dataTable.Columns.Add("Book Name", typeof(string));
                dataTable.Columns.Add("Price", typeof(decimal));

                foreach (var author in authors)
                {
                    foreach (var book in author.Books)
                    {
                        dataTable.Rows.Add(author.Id, author.Name, book.Name, book.Price);
                    }
                }
                manytomany.ItemsSource = dataTable.DefaultView;
            }
        }
    }
}
