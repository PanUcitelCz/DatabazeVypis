using DatabazeVypis.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace DatabazeVypis
{
    public partial class MainWindow : Window
    {
        // Dlouho-žijící kontext pro sledování změn i během editace v gridu
        private readonly StudentContext _db = new StudentContext();

        // Kolekce napojená na DataGrid (živé aktualizace v UI)
        private readonly ObservableCollection<Student> _students = new ObservableCollection<Student>();

        // Pohled kvůli řazení (podle Id vzestupně)
        private ICollectionView _studentsView;

        public MainWindow()
        {
            InitializeComponent();

            // Vytvoření DB + seed na prvním běhu
            _db.EnsureCreatedAndSeed();

            // Naplnění kolekce z kontextu (bez AsNoTracking, chceme editovat a ukládat)
            foreach (Student s in _db.Students.OrderBy(x => x.Id).ToList())
            {
                _students.Add(s);
            }

            // Řazení v UI podle Id
            _studentsView = CollectionViewSource.GetDefaultView(_students);
            _studentsView.SortDescriptions.Clear();
            _studentsView.SortDescriptions.Add(new SortDescription(nameof(Student.Id), ListSortDirection.Ascending));

            StudentsGrid.ItemsSource = _studentsView;
        }

        private void BtnAddStudent_Click(object sender, RoutedEventArgs e)
        {
            // 1) Načti vstupy
            string firstName = (TxtFirstName.Text ?? string.Empty).Trim();
            string lastName = (TxtLastName.Text ?? string.Empty).Trim();
            string email = (TxtEmail.Text ?? string.Empty).Trim();
            string yearText = (TxtYear.Text ?? string.Empty).Trim();

            // 2) Validace vstupů (co nejčitelněji)
            if (string.IsNullOrWhiteSpace(firstName) || string.IsNullOrWhiteSpace(lastName))
            {
                MessageBox.Show("Jméno i příjmení jsou povinné.", "Upozornění", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            int year;
            bool parsed = int.TryParse(yearText, out year);
            if (parsed == false || year < 1 || year > 6)
            {
                MessageBox.Show("Ročník zadej jako celé číslo v intervalu 1–6.", "Upozornění", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            if (string.IsNullOrWhiteSpace(email) == false)
            {
                bool exists = _db.Students.Any(s => s.Email == email);
                if (exists == true)
                {
                    MessageBox.Show("Zadaný e‑mail už v databázi existuje.", "Upozornění", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
            }

            // 3) Vytvoř entitu
            Student newStudent = new Student();
            newStudent.FirstName = firstName;
            newStudent.LastName = lastName;
            newStudent.Year = year;
            newStudent.Email = email;
            newStudent.CreatedAt = DateTime.UtcNow;

            // 4) Přidej do kontextu (pro DB) i do kolekce (pro UI)
            _db.Students.Add(newStudent);

            try
            {
                _db.SaveChanges(); // vygeneruje nové Id
            }
            catch (Exception ex)
            {
                MessageBox.Show("Uložení do databáze se nezdařilo.\n\n" + ex, "Chyba", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            _students.Add(newStudent);

            // 5) Vyber nově přidaného
            StudentsGrid.SelectedItem = newStudent;
            StudentsGrid.ScrollIntoView(newStudent);

            // 6) Vyčisti formulář
            TxtId.Text = string.Empty;
            TxtFirstName.Text = string.Empty;
            TxtLastName.Text = string.Empty;
            TxtYear.Text = string.Empty;
            TxtEmail.Text = string.Empty;

            MessageBox.Show("Student byl přidán.", "Hotovo", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Pro jistotu commitni rozeditovanou buňku/řádek
                StudentsGrid.CommitEdit(DataGridEditingUnit.Cell, true);
                StudentsGrid.CommitEdit(DataGridEditingUnit.Row, true);

                bool hasChanges = _db.ChangeTracker.HasChanges();
                if (hasChanges == false)
                {
                    MessageBox.Show("Žádné změny k uložení.", "Informace", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                _db.SaveChanges();
                MessageBox.Show("Změny byly uloženy.", "Hotovo", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Nepodařilo se uložit změny.\n\n" + ex, "Chyba", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnDeleteSelected_Click(object sender, RoutedEventArgs e)
        {
            Student? selected = StudentsGrid.SelectedItem as Student;
            if (selected == null)
            {
                MessageBox.Show("Nejprve vyber studenta v tabulce.", "Upozornění", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            MessageBoxResult answer = MessageBox.Show(
                "Opravdu smazat vybraného studenta?",
                "Potvrzení",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (answer != MessageBoxResult.Yes)
            {
                return;
            }

            _db.Students.Remove(selected);
            try
            {
                _db.SaveChanges();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Smazání se nepodařilo.\n\n" + ex, "Chyba", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            _students.Remove(selected);
        }

        protected override void OnClosed(EventArgs e)
        {
            _db.Dispose();
            base.OnClosed(e);
        }
    }
}
