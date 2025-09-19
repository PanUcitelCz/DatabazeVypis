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
            using var db = new StudentContext();

            // 1) Vytvoří databázi a tabulky, pokud chybí
            db.Database.EnsureCreated();

            // 2) Při prázdné tabulce doplní 10 záznamů
            db.SeedIfEmpty();

            // 3) Načte data (jen pro čtení) a zobrazí v DataGridu
            List<Student> data = db.Students
                                   .AsNoTracking()
                                   .OrderBy(s => s.LastName)
                                   .ThenBy(s => s.FirstName)
                                   .ToList();

            dgStudents.ItemsSource = data;
        }
    }
}