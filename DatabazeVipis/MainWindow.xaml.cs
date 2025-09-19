using DatabazeVipis.Data;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.EntityFrameworkCore; // Bez tohoto nám nepojede 

namespace DatabazeVipis
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            LoadData();
        }

        private void LoadData()
        {
            using StudentContext db = new StudentContext();

            db.Database.EnsureCreated(); // jen pro jistotu první běh

            List<Student> data = db.Students
                                   .AsNoTracking()
                                   .OrderBy(s => s.Id) // řazení podle ID (vzestupně)
                                   .ToList();

            dgStudents.ItemsSource = data;
        }

        private void btnAddStudent_Click(object sender, RoutedEventArgs e)
        {
            // 1) Načti vstupy a proveď základní validaci
            string firstName = (tbFirstName.Text ?? string.Empty).Trim();
            string lastName = (tbLastName.Text ?? string.Empty).Trim();
            string email = (tbEmail.Text ?? string.Empty).Trim();

            if (string.IsNullOrWhiteSpace(firstName) || string.IsNullOrWhiteSpace(lastName))
            {
                MessageBox.Show("Jméno i příjmení jsou povinné.", "Upozornění", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            if (!int.TryParse((tbYear.Text ?? string.Empty).Trim(), out int year) || year < 1 || year > 6)
            {
                MessageBox.Show("Ročník zadej jako celé číslo v intervalu 1–6.", "Upozornění", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            // 2) Ulož do databáze přes EF Core
            using StudentContext db = new StudentContext();

            // volitelná kontrola duplicitního e‑mailu (jen pro přívětivost)
            if (!string.IsNullOrWhiteSpace(email))
            {
                bool emailExists = db.Students.AsNoTracking().Any(s => s.Email == email);
                if (emailExists)
                {
                    MessageBox.Show("Zadaný e‑mail už v databázi existuje.", "Upozornění", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
            }

            Student newStudent = new Student
            {
                FirstName = firstName,
                LastName = lastName,
                Year = year,
                Email = email,
                CreatedAt = DateTime.UtcNow // necháme v UTC – konzistentní napříč stroji
            };

            db.Students.Add(newStudent);
            db.SaveChanges(); // TADY se vygeneruje nové Id (IDENTITY)

            int newId = newStudent.Id;

            // 3) Obnov DataGrid + vyber nově přidaného
            LoadData();

            if (dgStudents.ItemsSource is List<Student> current)
            {
                Student added = current.FirstOrDefault(x => x.Id == newId)!; // pozor: padneš, pokud tam fakt není
                if (added != null)
                {
                    dgStudents.SelectedItem = added;
                    dgStudents.ScrollIntoView(added);
                }
            }

            // 4) Vyčisti formulář
            tbFirstName.Clear();
            tbLastName.Clear();
            tbYear.Clear();
            tbEmail.Clear();

            MessageBox.Show("Student byl přidán.", "Hotovo", MessageBoxButton.OK, MessageBoxImage.Information);
        }


    }
}