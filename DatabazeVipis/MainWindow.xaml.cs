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

using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;


namespace DatabazeVipis
{
    public partial class MainWindow : Window
    {
        private StudentContext _dbContext;
        private ObservableCollection<Student> _students;
        private ICollectionView _studentsView;

        public MainWindow()
        {
            InitializeComponent();

            _dbContext = new StudentContext();
            _dbContext.Database.EnsureCreated();
            _dbContext.SeedIfEmpty();

            LoadData();
            HookSelectionChanged();
        }

        private void LoadData()
        {
            // Načteme TRACKED entity (žádné AsNoTracking) → inline editace v DataGridu se uloží přes SaveChanges().
            var list = _dbContext.Students
                                 .OrderBy(s => s.Id)   // řazení načtených
                                 .ToList();

            _students = new ObservableCollection<Student>(list);
            StudentsGrid.ItemsSource = _students;

            // Primární řazení view podle Id (aby po přidání a uložení záznam "skočil" na správné místo).
            _studentsView = CollectionViewSource.GetDefaultView(StudentsGrid.ItemsSource);
            if (_studentsView != null)
            {
                _studentsView.SortDescriptions.Clear();
                _studentsView.SortDescriptions.Add(new SortDescription(nameof(Student.Id), ListSortDirection.Ascending));
            }
        }

        private void HookSelectionChanged()
        {
            StudentsGrid.SelectionChanged += (s, e) =>
            {
                Student selected = StudentsGrid.SelectedItem as Student;
                if (selected == null)
                {
                    TxtId.Text = string.Empty;
                    TxtFirstName.Text = string.Empty;
                    TxtLastName.Text = string.Empty;
                    TxtYear.Text = string.Empty;
                    TxtEmail.Text = string.Empty;
                    return;
                }

                TxtId.Text = selected.Id.ToString();
                TxtFirstName.Text = selected.FirstName;
                TxtLastName.Text = selected.LastName;
                TxtYear.Text = selected.Year.ToString();
                TxtEmail.Text = selected.Email;
            };
        }

        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            string firstName = (TxtFirstName.Text ?? string.Empty).Trim();
            string lastName = (TxtLastName.Text ?? string.Empty).Trim();
            string email = (TxtEmail.Text ?? string.Empty).Trim();
            string yearText = (TxtYear.Text ?? string.Empty).Trim();

            if (string.IsNullOrWhiteSpace(firstName) || string.IsNullOrWhiteSpace(lastName))
            {
                MessageBox.Show("Jméno i příjmení jsou povinné.", "Upozornění", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            int year;
            bool parsed = int.TryParse(yearText, out year);
            if (!parsed || year < 1 || year > 6)
            {
                MessageBox.Show("Ročník zadej jako celé číslo v intervalu 1–6.", "Upozornění", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            if (!string.IsNullOrWhiteSpace(email))
            {
                bool exists = _dbContext.Students.Any(s => s.Email == email);
                if (exists)
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
                CreatedAt = DateTime.UtcNow
            };

            // 1) do DB
            _dbContext.Students.Add(newStudent);
            _dbContext.SaveChanges();   // vygeneruje Id

            // 2) do kolekce pro UI
            _students.Add(newStudent);

            // Refresh řazení, vybrat a poscrollovat na nový záznam
            if (_studentsView != null)
            {
                _studentsView.Refresh();
            }
            StudentsGrid.SelectedItem = newStudent;
            StudentsGrid.ScrollIntoView(newStudent);

            // Reset formuláře
            TxtId.Text = string.Empty;
            TxtFirstName.Text = string.Empty;
            TxtLastName.Text = string.Empty;
            TxtYear.Text = string.Empty;
            TxtEmail.Text = string.Empty;

            MessageBox.Show("Student byl přidán.", "Hotovo", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            Student selected = StudentsGrid.SelectedItem as Student;
            if (selected == null)
            {
                MessageBox.Show("Nejprve vyber studenta v tabulce.", "Upozornění", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            MessageBoxResult result = MessageBox.Show(
                $"Opravdu smazat studenta {selected.FirstName} {selected.LastName} (Id {selected.Id})?",
                "Potvrzení",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes)
            {
                return;
            }

            _dbContext.Students.Remove(selected);
            _dbContext.SaveChanges();

            _students.Remove(selected);
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _dbContext.SaveChanges();
                if (_studentsView != null)
                {
                    _studentsView.Refresh();
                }
                MessageBox.Show("Změny byly uloženy.", "Uloženo", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ukládání selhalo.\n" + ex.ToString(), "Chyba", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            if (_dbContext != null)
            {
                _dbContext.Dispose();
            }
            base.OnClosed(e);
        }
    }
}
