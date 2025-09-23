# WPF (.NET 9) – jednoduchá databázová aplikace **Studenti** (EF Core + LocalDB)

> **Cíl**: Vytvoříme GUI aplikaci ve Visual Studiu 2022 (EN UI), která se připojí k LocalDB (SQL Server Express LocalDB), zobrazí tabulku `Students`, umožní **přidat** záznam, **ověřit vstupy**, **upravit** přímo v tabulce a **smazat** vybraného studenta.  
> **Použité balíčky** (NuGet):  
> - `Microsoft.EntityFrameworkCore.SqlServer` (provider pro SQL Server)  
> - `Microsoft.EntityFrameworkCore.Tools` (design-time nástroje EF)  
> - `PropertyChanged.Fody` (+ automaticky nainstalovaný `Fody`) – pro pohodlné notifikace změn v entitách  
>
> **Budeme upravovat POUZE tyto soubory**:
> - `Data/Student.cs` (model/entita)
> - `Data/StudentContext.cs` (EF Core DbContext)
> - `MainWindow.xaml` (UI – XAML)
> - `MainWindow.xaml.cs` (code-behind logika okna)
>
> Vycházíme z materiálů k předmětu DBC (viz příloha/skripta), ale vše děláme **klikací cestou** ve VS 2022 a v **.NET 9**.  

---

## ZADÁNÍ (4 navazující úkoly)

1) **Zobrazit data z DB**  
   - Založte WPF projekt (.NET 9).  
   - Přidejte NuGet balíčky.  
   - Vytvořte složku `Data`, do ní soubory `Student.cs` a `StudentContext.cs`.  
   - Při startu aplikace vytvořte DB (pokud chybí) a **naplňte** ji 10 studenty.  
   - V okně zobrazte data v `DataGrid` (jen prohlížení).

2) **Přidat studenta přes formulář (bez validace)**  
   - Dole v okně bude jednoduchý formulář (jméno, příjmení, ročník, e‑mail) + tlačítko **Přidat**.  
   - Po kliknutí se záznam uloží do DB a hned zobrazí v tabulce.

3) **Základní kontrola vstupů**  
   - Zkontrolujte, že Jméno + Příjmení nejsou prázdné.  
   - Ročník musí být celé číslo 1–6.  
   - E‑mail pouze zkontrolujeme na neprázdnost (neřešíme formát).  
   - Při chybě ukažte `MessageBox` a záznam neukládejte.

4) **Úpravy přímo v tabulce + smazání + uložení**  
   - Umožněte **editovat** hodnoty přímo v `DataGrid`.  
   - Přidejte tlačítko **💾 Uložit data** – zapíše změny do DB.  
   - Přidejte **Smazat vybraného** – odstraní označený řádek z DB i z tabulky.

---

## PŘÍPRAVA PROJEKTU (klikací, VS 2022 – EN)

1) **Create project**  
   - **File ▸ New ▸ Project…**  
   - Šablona **WPF App** (.NET).  
   - Project name: `DatabazeVipis` (bez diakritiky a mezer).  
   - Framework: **.NET 9.0**.  
   - **Place solution and project in the same directory**: libovolně (doporučeno **ON** u malých projektů).

2) **Zapněte WPF a .NET 9 v projektu** (mají být defaultně):  
   - Ověřte v `DatabazeVipis.csproj`:
     ```xml
     <Project Sdk="Microsoft.NET.Sdk">
       <PropertyGroup>
         <OutputType>WinExe</OutputType>
         <TargetFramework>net9.0-windows</TargetFramework>
         <UseWPF>true</UseWPF>
         <Nullable>enable</Nullable>
         <ImplicitUsings>enable</ImplicitUsings>
       </PropertyGroup>
     </Project>
     ```

3) **Přidejte NuGety (klikací)**  
   - **Project ▸ Manage NuGet Packages…**  
   - Karta **Browse**: vyhledejte a nainstalujte do projektu:  
     - `Microsoft.EntityFrameworkCore.SqlServer`  
     - `Microsoft.EntityFrameworkCore.Tools`  
     - `PropertyChanged.Fody` → (při instalaci se automaticky přidá také `Fody`)  
   - Karta **Installed**: zkontrolujte, že jsou aktivní v **DatabazeVipis**.

4) **Vytvořte `FodyWeavers.xml`** (soubor v kořeni projektu)  
   - **Project ▸ Add ▸ New Item… ▸ XML File** → Name: `FodyWeavers.xml`  
   - Obsah:
     ```xml
     <?xml version="1.0" encoding="utf-8"?>
     <Weavers>
       <PropertyChanged/>
     </Weavers>
     ```
   - Co dělá: PropertyChanged.Fody při překladu automaticky doplní implementaci **INotifyPropertyChanged** do tříd označených atributem `[AddINotifyPropertyChangedInterface]` (méně kódu, hladší binding ve WPF).

5) **Přidejte složku `Data`** a v ní **dva soubory**  
   - **Solution Explorer** → pravým na projekt → **Add ▸ New Folder** → `Data`.  
   - Do `Data` přidejte **Class** `Student.cs` a **Class** `StudentContext.cs` (níže je kompletní kód).

---

# ÚKOL 1 – Zobrazit data z DB v tabulce

### 1A) `Data/Student.cs` – verze **bez komentářů**

```csharp
using PropertyChanged;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using PropertyChanged;

namespace DatabazeVipis.Data
{
    [AddINotifyPropertyChangedInterface]
    public class Student
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string LastName { get; set; } = string.Empty;

        [Range(1, 6)]
        public int Year { get; set; }

        [StringLength(200)]
        public string Email { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
```

### 1B) `Data/Student.cs` – verze **s komentáři** (řádek po řádku)

```csharp
// Přidáme using pro Fody-PropertyChanged – tento using zpřístupní atribut
// [AddINotifyPropertyChangedInterface], který Fody při překladu „vstřikuje“
using PropertyChanged;
using System;                                                // datum/čas (CreatedAt)
using System.ComponentModel.DataAnnotations;                 // datové anotace (Key, Required, Range...)
using System.ComponentModel.DataAnnotations.Schema;          // [DatabaseGenerated]
using PropertyChanged;                                       // atribut AddINotifyPropertyChangedInterface (Fody)

namespace DatabazeVipis.Data
{
    // Fody automaticky vygeneruje INotifyPropertyChanged – WPF pak lépe reaguje na změny v UI.
    [AddINotifyPropertyChangedInterface]
    public class Student
    {
        // Primární klíč; hodnota se generuje v DB (IDENTITY/auto‑increment).
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        // Povinné, max. 100 znaků. Výchozí hodnota = prázdný řetězec (nikdy null).
        [Required]
        [StringLength(100)]
        public string FirstName { get; set; } = string.Empty;

        // Povinné, max. 100 znaků.
        [Required]
        [StringLength(100)]
        public string LastName { get; set; } = string.Empty;

        // 1–6 (např. ročník studia)
        [Range(1, 6)]
        public int Year { get; set; }

        // Nepovinné, max. 200 znaků.
        [StringLength(200)]
        public string Email { get; set; } = string.Empty;

        // Datum a čas vložení záznamu (UTC, aby bylo jednotné napříč PC).
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
```

---

### 1C) `Data/StudentContext.cs` – verze **bez komentářů**

```csharp
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;

namespace DatabazeVipis.Data
{
    public class StudentContext : DbContext
    {
        public DbSet<Student> Students => Set<Student>();

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer(
                    "Data Source=(localdb)\\MSSQLLocalDB;" +
                    "Initial Catalog=StudentDbDemo;" +
                    "Integrated Security=True;" +
                    "TrustServerCertificate=True");
            }
        }

        public void EnsureCreatedAndSeed()
        {
            Database.EnsureCreated();
            SeedIfEmpty();
        }

        public void SeedIfEmpty()
        {
            if (!Students.Any())
            {
                var initial = new List<Student>
                {
                    new Student { FirstName="Jan",   LastName="Novák",    Year=1, Email="jan.novak@example.com" },
                    new Student { FirstName="Petr",  LastName="Svoboda",  Year=2, Email="petr.svoboda@example.com" },
                    new Student { FirstName="Karel", LastName="Černý",    Year=3, Email="karel.cerny@example.com" },
                    new Student { FirstName="Lucie", LastName="Malá",     Year=1, Email="lucie.mala@example.com" },
                    new Student { FirstName="Eva",   LastName="Bílá",     Year=2, Email="eva.bila@example.com" },
                    new Student { FirstName="Adam",  LastName="Zelený",   Year=3, Email="adam.zeleny@example.com" },
                    new Student { FirstName="Tomáš", LastName="Dvořák",   Year=1, Email="tomas.dvorak@example.com" },
                    new Student { FirstName="Marie", LastName="Veselá",   Year=2, Email="marie.vesela@example.com" },
                    new Student { FirstName="Jana",  LastName="Horáková", Year=3, Email="jana.horakova@example.com" },
                    new Student { FirstName="Filip", LastName="Král",     Year=1, Email="filip.kral@example.com" }
                };
                Students.AddRange(initial);
                SaveChanges();
            }
        }
    }
}
```

### 1D) `Data/StudentContext.cs` – verze **s komentáři**

```csharp
using Microsoft.EntityFrameworkCore;     // EF Core – DbContext, DbSet, UseSqlServer
using System.Collections.Generic;        // List<T>
using System.Linq;                       // Any()

namespace DatabazeVipis.Data
{
    // Hlavní třída EF Core – drží připojení a sadu tabulek (DbSet).
    public class StudentContext : DbContext
    {
        // Virtuální tabulka "Students" – nad ní děláme LINQ dotazy a SaveChanges().
        public DbSet<Student> Students => Set<Student>();

        // Nastavení připojení: LocalDB (součást VS). DB jméno: StudentDbDemo.
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer(
                    "Data Source=(localdb)\\MSSQLLocalDB;" +  // lokální vývojový SQL Server
                    "Initial Catalog=StudentDbDemo;" +        // název databáze
                    "Integrated Security=True;" +             // přihlášení přes Windows účet
                    "TrustServerCertificate=True");           // potlačí kontrolu certifikátu (lokální DB)
            }
        }

        // Při startu: vytvoř DB (pokud neexistuje) a naplň ji ukázkovými daty.
        public void EnsureCreatedAndSeed()
        {
            Database.EnsureCreated(); // vytvoří DB a tabulky podle entit
            SeedIfEmpty();            // přidá 10 řádků, pokud je tabulka prázdná
        }

        // Naplnění tabulky Students 10 ukázkovými řádky.
        public void SeedIfEmpty()
        {
            if (!Students.Any()) // jestli je tabulka prázdná
            {
                var initial = new List<Student>
                {
                    new Student { FirstName="Jan",   LastName="Novák",    Year=1, Email="jan.novak@example.com" },
                    new Student { FirstName="Petr",  LastName="Svoboda",  Year=2, Email="petr.svoda@example.com".Replace("svoda","svoboda") },
                    new Student { FirstName="Karel", LastName="Černý",    Year=3, Email="karel.cerny@example.com" },
                    new Student { FirstName="Lucie", LastName="Malá",     Year=1, Email="lucie.mala@example.com" },
                    new Student { FirstName="Eva",   LastName="Bílá",     Year=2, Email="eva.bila@example.com" },
                    new Student { FirstName="Adam",  LastName="Zelený",   Year=3, Email="adam.zeleny@example.com" },
                    new Student { FirstName="Tomáš", LastName="Dvořák",   Year=1, Email="tomas.dvorak@example.com" },
                    new Student { FirstName="Marie", LastName="Veselá",   Year=2, Email="marie.vesela@example.com" },
                    new Student { FirstName="Jana",  LastName="Horáková", Year=3, Email="jana.horakova@example.com" },
                    new Student { FirstName="Filip", LastName="Král",     Year=1, Email="filip.kral@example.com" }
                };
                Students.AddRange(initial); // dávkové vložení
                SaveChanges();              // zápis do DB
            }
        }
    }
}
```

> **Poznámka**: Connection string je „natvrdo“ v `OnConfiguring` = jednoduché pro výuku. Pro produkční aplikace se používá konfigurační soubor, uživatelské secret-y apod.

---

### 1E) `MainWindow.xaml` – verze **bez komentářů** (pro ÚKOL 1: jen zobrazení)

```xml
<Window x:Class="DatabazeVipis.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        Title="Studenti" Height="600" Width="900">
    <Grid Margin="12">
        <Grid.RowDefinitions>
            <RowDefinition Height="2*"/>
            <RowDefinition Height="Auto"/>
        </Grid.RowDefinitions>

        <DataGrid x:Name="StudentsGrid"
                  Grid.Row="0"
                  AutoGenerateColumns="False"
                  CanUserAddRows="False"
                  IsReadOnly="True">
            <DataGrid.Columns>
                <DataGridTextColumn Header="ID"        Binding="{Binding Id}"        Width="70"/>
                <DataGridTextColumn Header="Jméno"     Binding="{Binding FirstName}" Width="*"/>
                <DataGridTextColumn Header="Příjmení"  Binding="{Binding LastName}"  Width="*"/>
                <DataGridTextColumn Header="Ročník"    Binding="{Binding Year}"      Width="90"/>
                <DataGridTextColumn Header="E‑mail"    Binding="{Binding Email}"     Width="2*"/>
                <DataGridTextColumn Header="Vytvořeno" Binding="{Binding CreatedAt, StringFormat={}{0:yyyy-MM-dd HH:mm:ss}}" Width="180"/>
            </DataGrid.Columns>
        </DataGrid>

        <TextBlock Grid.Row="1" Margin="0,8,0,0" Text="ÚKOL 1: pouze zobrazení dat."/>
    </Grid>
</Window>
```

### 1F) `MainWindow.xaml` – verze **s komentáři**

```xml
<Window x:Class="DatabazeVipis.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"   <!-- XAML jmenné prostory -->
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        Title="Studenti" Height="600" Width="900">                           <!-- Titulek a velikost okna -->
    <Grid Margin="12">                                                       <!-- Základní mřížka s okrajem -->
        <Grid.RowDefinitions>                                                <!-- Dva řádky: tabulka + poznámka -->
            <RowDefinition Height="2*"/>
            <RowDefinition Height="Auto"/>
        </Grid.RowDefinitions>

        <DataGrid x:Name="StudentsGrid"                                      <!-- Mřížka pro zobrazení studentů -->
                  Grid.Row="0"
                  AutoGenerateColumns="False"                                 <!-- Sloupce definujeme ručně -->
                  CanUserAddRows="False"                                      <!-- Žádný prázdný řádek na konci -->
                  IsReadOnly="True">                                          <!-- ÚKOL 1: jen pro čtení -->
            <DataGrid.Columns>
                <DataGridTextColumn Header="ID"        Binding="{Binding Id}"        Width="70"/>
                <DataGridTextColumn Header="Jméno"     Binding="{Binding FirstName}" Width="*"/>
                <DataGridTextColumn Header="Příjmení"  Binding="{Binding LastName}"  Width="*"/>
                <DataGridTextColumn Header="Ročník"    Binding="{Binding Year}"      Width="90"/>
                <DataGridTextColumn Header="E‑mail"    Binding="{Binding Email}"     Width="2*"/>
                <DataGridTextColumn Header="Vytvořeno" Binding="{Binding CreatedAt, StringFormat={}{0:yyyy-MM-dd HH:mm:ss}}" Width="180"/>
            </DataGrid.Columns>
        </DataGrid>

        <TextBlock Grid.Row="1" Margin="0,8,0,0" Text="ÚKOL 1: pouze zobrazení dat."/>   <!-- Popisek -->
    </Grid>
</Window>
```

---

### 1G) `MainWindow.xaml.cs` – verze **bez komentářů** (pro ÚKOL 1)

```csharp
using System.Collections.ObjectModel;
using System.ComponentModel;
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
using WpfApp1.Data;

namespace WpfApp1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
        public partial class MainWindow : Window
        {
            private readonly StudentContext _db = new StudentContext();
            private readonly ObservableCollection<Student> _students = new ObservableCollection<Student>();
            private ICollectionView _studentsView;

            public MainWindow()
            {
                InitializeComponent();

                _db.EnsureCreatedAndSeed();

                foreach (var s in _db.Students.OrderBy(x => x.Id).ToList())
                    _students.Add(s);

                _studentsView = CollectionViewSource.GetDefaultView(_students);
                _studentsView.SortDescriptions.Clear();
                _studentsView.SortDescriptions.Add(new SortDescription(nameof(Student.Id), ListSortDirection.Ascending));

                StudentsGrid.ItemsSource = _studentsView;
            }

            protected override void OnClosed(System.EventArgs e)
            {
                _db.Dispose();
                base.OnClosed(e);
            }
        }
    
}
```

### 1H) `MainWindow.xaml.cs` – verze **s komentáři**

```csharp
using System.Collections.ObjectModel;          // ObservableCollection = kolekce, která umí UI oznámit, že se přidala/odebrala položka
using System.ComponentModel;                   // ICollectionView, SortDescription – pro řazení/filtraci nad kolekcí
using System.Linq;                             // LINQ rozšíření: OrderBy, ToList
using System.Windows;                          // Základní WPF typy (Window, MessageBox, ...)
using System.Windows.Data;                     // CollectionViewSource.GetDefaultView(...)
using WpfApp1.Data;                            // Naše datová vrstva: Student a StudentContext (EF Core)

namespace DatabazeVipis
{
    // partial = část třídy vygenerovaná z XAML (InitializeComponent) + tato „code-behind“ část
    // Window   = základní WPF okno
    public partial class MainWindow : Window
    {
        // Dlouho-žijící EF Core kontext.
        // Drží spojení na LocalDB, sleduje změny entit a umožňuje SaveChanges/Dispose.
        private readonly StudentContext _db = new StudentContext();

        // Datová kolekce navázaná na DataGrid v XAMLu (StudentsGrid).
        // ObservableCollection zajistí, že přidání/odebrání položky se okamžitě projeví v UI.
        private readonly ObservableCollection<Student> _students = new ObservableCollection<Student>();

        // Pohled nad kolekcí (view) – umožní setřídit (případně filtrovat) data pro DataGrid.
        private ICollectionView _studentsView;

        // Konstruktor okna – proběhne při vytvoření instance MainWindow.
        public MainWindow()
        {
            // Vytvoří vizuální prvky definované v MainWindow.xaml a přiřadí jim jména/události.
            InitializeComponent();

            // 1) Zajisti, že DB existuje a je (při prázdné tabulce) naplněná 10 vzorovými záznamy.
            _db.EnsureCreatedAndSeed();

            // 2) Načti data ze StudentContextu do ObservableCollection.
            //    - OrderBy(x => x.Id) ... řadíme vzestupně podle ID
            //    - ToList()          ... zhmotní výsledek query (enumerable -> list)
            //    - foreach           ... položku po položce přidáme do ObservableCollection,
            //                           aby UI dostalo notifikace a vykreslilo řádky.
            foreach (var s in _db.Students.OrderBy(x => x.Id).ToList())
                _students.Add(s);

            // 3) Vytvoř "view" nad kolekcí, abychom mohli řídit řazení (případně filtrování).
            _studentsView = CollectionViewSource.GetDefaultView(_students);

            // Pro jistotu smažeme případná předchozí řazení (když by se kód volal opakovaně).
            _studentsView.SortDescriptions.Clear();

            // Nastavíme řazení podle vlastnosti "Id" vzestupně (Ascending).
            _studentsView.SortDescriptions.Add(
                new SortDescription(nameof(Student.Id), ListSortDirection.Ascending)
            );

            // 4) Napojíme DataGrid (StudentsGrid v XAMLu) na náš "view" jako zdroj dat.
            //    StudentsGrid je element v XAMLu s x:Name="StudentsGrid".
            StudentsGrid.ItemsSource = _studentsView;
        }

        // Správné uvolnění prostředků při zavření okna (např. uvolnit DB spojení).
        protected override void OnClosed(System.EventArgs e)
        {
            // Uvolní EF Core kontext (Dispose uzavře spojení a vyčistí unmanaged prostředky).
            _db.Dispose();

            // Zavoláme implementaci předka (Window) – standardní dočistění v rámci WPF.
            base.OnClosed(e);
        }
    }
}

```

> **Stav po ÚKOLU 1:** Aplikace zobrazí 10 studentů z LocalDB v tabulce (jen pro čtení).

---

# ÚKOL 2 – Přidání studenta formulářem (bez validace)

V `MainWindow.xaml` přidáme dole formulář a tlačítko **Přidat**, v code‑behind doplníme obsluhu.

### 2A) `MainWindow.xaml` – rozšíření (část dole)

```xml
<Grid Margin="12">
  <Grid.RowDefinitions>
    <RowDefinition Height="2*"/>
    <RowDefinition Height="6"/>
    <RowDefinition Height="Auto"/>
  </Grid.RowDefinitions>

  <!-- ... (DataGrid z Úkolu 1 beze změny) ... -->

  <GridSplitter Grid.Row="1" Height="6" HorizontalAlignment="Stretch" VerticalAlignment="Center" Background="#DDD"/>

  <Grid Grid.Row="2" Margin="0,10,0,0">
    <Grid.ColumnDefinitions>
      <ColumnDefinition Width="Auto"/>
      <ColumnDefinition Width="2*"/>
      <ColumnDefinition Width="16"/>
      <ColumnDefinition Width="Auto"/>
      <ColumnDefinition Width="2*"/>
      <ColumnDefinition Width="*"/>
    </Grid.ColumnDefinitions>
    <Grid.RowDefinitions>
      <RowDefinition Height="Auto"/>
      <RowDefinition Height="Auto"/>
      <RowDefinition Height="Auto"/>
    </Grid.RowDefinitions>

    <TextBlock Grid.Row="0" Grid.Column="0" Margin="0,0,6,8" VerticalAlignment="Center" Text="Jméno:"/>
    <TextBox  x:Name="TxtFirstName" Grid.Row="0" Grid.Column="1" Margin="0,0,0,8"/>

    <TextBlock Grid.Row="0" Grid.Column="3" Margin="0,0,6,8" VerticalAlignment="Center" Text="Příjmení:"/>
    <TextBox  x:Name="TxtLastName" Grid.Row="0" Grid.Column="4" Margin="0,0,0,8"/>

    <TextBlock Grid.Row="1" Grid.Column="0" Margin="0,0,6,8" VerticalAlignment="Center" Text="Ročník (1–6):"/>
    <TextBox  x:Name="TxtYear" Grid.Row="1" Grid.Column="1" Margin="0,0,0,8"/>

    <TextBlock Grid.Row="1" Grid.Column="3" Margin="0,0,6,8" VerticalAlignment="Center" Text="E‑mail:"/>
    <TextBox  x:Name="TxtEmail" Grid.Row="1" Grid.Column="4" Margin="0,0,0,8"/>

    <StackPanel Grid.Row="0" Grid.RowSpan="2" Grid.Column="5" Orientation="Vertical" HorizontalAlignment="Right">
      <Button x:Name="BtnAddStudent"
              Content="Přidat studenta"
              Padding="16,6"
              Click="BtnAddStudent_Click"/>
    </StackPanel>
  </Grid>
</Grid>
```

### 2B) `MainWindow.xaml.cs` – obsluha tlačítka **Přidat** (bez validace)

```csharp
private void BtnAddStudent_Click(object sender, RoutedEventArgs e)
{
    string firstName = (TxtFirstName.Text ?? string.Empty).Trim();
    string lastName  = (TxtLastName.Text  ?? string.Empty).Trim();
    string email     = (TxtEmail.Text     ?? string.Empty).Trim();
    int year = 1;
    int.TryParse((TxtYear.Text ?? string.Empty).Trim(), out year);

    var s = new Student
    {
        FirstName = firstName,
        LastName  = lastName,
        Year      = year,
        Email     = email,
        CreatedAt = System.DateTime.UtcNow
    };

    _db.Students.Add(s);
    _db.SaveChanges();          // vygeneruje Id

    _students.Add(s);           // přidat do UI kolekce
    StudentsGrid.SelectedItem = s;
    StudentsGrid.ScrollIntoView(s);

    TxtFirstName.Text = TxtLastName.Text = TxtEmail.Text = TxtYear.Text = string.Empty;
}
```

> **Stav po ÚKOLU 2:** Lze přidat studenta bez validace (i chybné ročníky projdou).

---

# ÚKOL 3 – Základní validace vstupů (MessageBox)

Rozšíříme obsluhu tlačítka **Přidat** o jednoduché kontroly.

### 3A) `MainWindow.xaml.cs` – **úplná** verze metody s validací

```csharp
private void BtnAddStudent_Click(object sender, RoutedEventArgs e)
{
    string firstName = (TxtFirstName.Text ?? string.Empty).Trim();
    string lastName  = (TxtLastName.Text  ?? string.Empty).Trim();
    string email     = (TxtEmail.Text     ?? string.Empty).Trim();
    string yearText  = (TxtYear.Text      ?? string.Empty).Trim();

    if (string.IsNullOrWhiteSpace(firstName) || string.IsNullOrWhiteSpace(lastName))
    {
        MessageBox.Show("Jméno i příjmení jsou povinné.", "Upozornění",
            MessageBoxButton.OK, MessageBoxImage.Information);
        return;
    }

    if (!int.TryParse(yearText, out int year) || year < 1 || year > 6)
    {
        MessageBox.Show("Ročník zadej jako celé číslo v intervalu 1–6.", "Upozornění",
            MessageBoxButton.OK, MessageBoxImage.Information);
        return;
    }

    // e‑mail volitelný – jen kontrola na neprázdnost (formát neřešíme)
    // if (string.IsNullOrWhiteSpace(email)) { ... }  // případně přísnější pravidla

    var s = new Student
    {
        FirstName = firstName,
        LastName  = lastName,
        Year      = year,
        Email     = email,
        CreatedAt = System.DateTime.UtcNow
    };

    // volitelná kontrola duplicitního e‑mailu (jen ukázka)
    if (!string.IsNullOrWhiteSpace(email) && _db.Students.Any(x => x.Email == email))
    {
        MessageBox.Show("Zadaný e‑mail už v databázi existuje.", "Upozornění",
            MessageBoxButton.OK, MessageBoxImage.Warning);
        return;
    }

    _db.Students.Add(s);
    _db.SaveChanges();

    _students.Add(s);
    StudentsGrid.SelectedItem = s;
    StudentsGrid.ScrollIntoView(s);

    TxtFirstName.Text = TxtLastName.Text = TxtEmail.Text = TxtYear.Text = string.Empty;
}
```

> **Stav po ÚKOLU 3:** Přidání odmítne prázdné jméno/příjmení a špatný ročník.

---

# ÚKOL 4 – Úpravy přímo v tabulce + Uložit + Smazat

- Umožníme **editaci** přímo v `DataGrid`.  
- Přidáme tlačítka **💾 Uložit data** a **Smazat vybraného**.

### 4A) `MainWindow.xaml` – DataGrid povolíme k editaci a přidáme tlačítka

```xml
<DataGrid x:Name="StudentsGrid"
          Grid.Row="0"
          AutoGenerateColumns="False"
          CanUserAddRows="False"
          IsReadOnly="False"
          SelectionMode="Single">
  <DataGrid.Columns>
    <DataGridTextColumn Header="ID"        Binding="{Binding Id}"        IsReadOnly="True" Width="70"/>
    <DataGridTextColumn Header="Jméno"     Binding="{Binding FirstName, Mode=TwoWay, UpdateSourceTrigger=PropertyChanged}" Width="*"/>
    <DataGridTextColumn Header="Příjmení"  Binding="{Binding LastName,  Mode=TwoWay, UpdateSourceTrigger=PropertyChanged}" Width="*"/>
    <DataGridTextColumn Header="Ročník"    Binding="{Binding Year,      Mode=TwoWay, UpdateSourceTrigger=PropertyChanged}" Width="90"/>
    <DataGridTextColumn Header="E‑mail"    Binding="{Binding Email,     Mode=TwoWay, UpdateSourceTrigger=PropertyChanged}" Width="2*"/>
    <DataGridTextColumn Header="Vytvořeno" Binding="{Binding CreatedAt, Mode=OneWay,  StringFormat={}{0:yyyy-MM-dd HH:mm:ss}}" IsReadOnly="True" Width="180"/>
  </DataGrid.Columns>
</DataGrid>

<!-- Tlačítka vpravo ve formuláři -->
<StackPanel Grid.Row="2" Grid.Column="5" Orientation="Vertical" HorizontalAlignment="Right">
  <Button x:Name="BtnAddStudent"    Content="Přidat studenta" Padding="16,6" Margin="0,0,0,8" Click="BtnAddStudent_Click"/>
  <Button x:Name="BtnDeleteSelected" Content="Smazat vybraného" Padding="16,6" Margin="0,0,0,8" Click="BtnDeleteSelected_Click"/>
  <Button x:Name="BtnSave"          Content="💾 Uložit data"    Padding="16,6" Click="BtnSave_Click"/>
</StackPanel>
```

### 4B) `MainWindow.xaml.cs` – metody **Uložit** a **Smazat**

```csharp
private void BtnSave_Click(object sender, RoutedEventArgs e)
{
    try
    {
        // commit rozeditovaných buněk/řádků (DataGrid drží editaci v paměti)
        StudentsGrid.CommitEdit(DataGridEditingUnit.Cell, true);
        StudentsGrid.CommitEdit(DataGridEditingUnit.Row,  true);

        if (!_db.ChangeTracker.HasChanges())
        {
            MessageBox.Show("Žádné změny k uložení.", "Informace",
                MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        _db.SaveChanges();
        MessageBox.Show("Změny byly uloženy.", "Hotovo",
            MessageBoxButton.OK, MessageBoxImage.Information);
    }
    catch (System.Exception ex)
    {
        MessageBox.Show("Nepodařilo se uložit změny.\n\n" + ex, "Chyba",
            MessageBoxButton.OK, MessageBoxImage.Error);
    }
}

private void BtnDeleteSelected_Click(object sender, RoutedEventArgs e)
{
    var selected = StudentsGrid.SelectedItem as Student;
    if (selected == null)
    {
        MessageBox.Show("Nejprve vyberte studenta v tabulce.", "Upozornění",
            MessageBoxButton.OK, MessageBoxImage.Information);
        return;
    }

    var answer = MessageBox.Show("Opravdu smazat vybraného studenta?",
        "Potvrzení", MessageBoxButton.YesNo, MessageBoxImage.Question);

    if (answer != MessageBoxResult.Yes)
        return;

    _db.Students.Remove(selected);
    try
    {
        _db.SaveChanges();
        _students.Remove(selected);
    }
    catch (System.Exception ex)
    {
        MessageBox.Show("Smazání se nepodařilo.\n\n" + ex, "Chyba",
            MessageBoxButton.OK, MessageBoxImage.Error);
    }
}
```

> **Stav po ÚKOLU 4 (finální)**:  
> - Přidání přes formulář (s jednoduchou validací).  
> - Přímé úpravy v tabulce + uložení.  
> - Smazání vybraného řádku.  

---

# PLNÉ VERZE SOUBORŮ – nejprve bez komentářů, pak s komentáři

Níže je **kompletní finální kód** pro všechny 4 soubory.

## `Data/Student.cs` (bez komentářů)

```csharp
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using PropertyChanged;

namespace DatabazeVipis.Data
{
    [AddINotifyPropertyChangedInterface]
    public class Student
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string LastName { get; set; } = string.Empty;

        [Range(1, 6)]
        public int Year { get; set; }

        [StringLength(200)]
        public string Email { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
```

## `Data/Student.cs` (s komentáři)

```csharp
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using PropertyChanged;

namespace DatabazeVipis.Data
{
    [AddINotifyPropertyChangedInterface]                     // Fody vygeneruje INotifyPropertyChanged
    public class Student
    {
        [Key]                                                // primární klíč
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]// auto-increment (IDENTITY)
        public int Id { get; set; }

        [Required] [StringLength(100)]
        public string FirstName { get; set; } = string.Empty;

        [Required] [StringLength(100)]
        public string LastName  { get; set; } = string.Empty;

        [Range(1, 6)]
        public int Year { get; set; }

        [StringLength(200)]
        public string Email { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow; // čas vložení
    }
}
```

---

## `Data/StudentContext.cs` (bez komentářů)

```csharp
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;

namespace DatabazeVipis.Data
{
    public class StudentContext : DbContext
    {
        public DbSet<Student> Students => Set<Student>();

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer(
                    "Data Source=(localdb)\\MSSQLLocalDB;" +
                    "Initial Catalog=StudentDbDemo;" +
                    "Integrated Security=True;" +
                    "TrustServerCertificate=True");
            }
        }

        public void EnsureCreatedAndSeed()
        {
            Database.EnsureCreated();
            SeedIfEmpty();
        }

        public void SeedIfEmpty()
        {
            if (!Students.Any())
            {
                var initial = new List<Student>
                {
                    new Student { FirstName="Jan",   LastName="Novák",    Year=1, Email="jan.novak@example.com" },
                    new Student { FirstName="Petr",  LastName="Svoboda",  Year=2, Email="petr.svoboda@example.com" },
                    new Student { FirstName="Karel", LastName="Černý",    Year=3, Email="karel.cerny@example.com" },
                    new Student { FirstName="Lucie", LastName="Malá",     Year=1, Email="lucie.mala@example.com" },
                    new Student { FirstName="Eva",   LastName="Bílá",     Year=2, Email="eva.bila@example.com" },
                    new Student { FirstName="Adam",  LastName="Zelený",   Year=3, Email="adam.zeleny@example.com" },
                    new Student { FirstName="Tomáš", LastName="Dvořák",   Year=1, Email="tomas.dvorak@example.com" },
                    new Student { FirstName="Marie", LastName="Veselá",   Year=2, Email="marie.vesela@example.com" },
                    new Student { FirstName="Jana",  LastName="Horáková", Year=3, Email="jana.horakova@example.com" },
                    new Student { FirstName="Filip", LastName="Král",     Year=1, Email="filip.kral@example.com" }
                };
                Students.AddRange(initial);
                SaveChanges();
            }
        }
    }
}
```

## `Data/StudentContext.cs` (s komentáři)

```csharp
using Microsoft.EntityFrameworkCore;     // DbContext, UseSqlServer, DbSet
using System.Collections.Generic;
using System.Linq;

namespace DatabazeVipis.Data
{
    public class StudentContext : DbContext
    {
        public DbSet<Student> Students => Set<Student>();   // tabulka Students

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)               // nastav připojení, pokud není dané zvenku
            {
                optionsBuilder.UseSqlServer(
                    "Data Source=(localdb)\\MSSQLLocalDB;" +// LocalDB instance
                    "Initial Catalog=StudentDbDemo;" +      // název databáze
                    "Integrated Security=True;" +           // Windows auth
                    "TrustServerCertificate=True");         // OK pro lokální vývoj
            }
        }

        public void EnsureCreatedAndSeed()
        {
            Database.EnsureCreated();                       // vytvoř DB a tabulky podle entit (Code First)
            SeedIfEmpty();                                  // naplň tabulku ukázkovými daty
        }

        public void SeedIfEmpty()
        {
            if (!Students.Any())
            {
                var initial = new List<Student>
                {
                    new Student { FirstName="Jan",   LastName="Novák",    Year=1, Email="jan.novak@example.com" },
                    new Student { FirstName="Petr",  LastName="Svoboda",  Year=2, Email="petr.svoboda@example.com" },
                    new Student { FirstName="Karel", LastName="Černý",    Year=3, Email="karel.cerny@example.com" },
                    new Student { FirstName="Lucie", LastName="Malá",     Year=1, Email="lucie.mala@example.com" },
                    new Student { FirstName="Eva",   LastName="Bílá",     Year=2, Email="eva.bila@example.com" },
                    new Student { FirstName="Adam",  LastName="Zelený",   Year=3, Email="adam.zeleny@example.com" },
                    new Student { FirstName="Tomáš", LastName="Dvořák",   Year=1, Email="tomas.dvorak@example.com" },
                    new Student { FirstName="Marie", LastName="Veselá",   Year=2, Email="marie.vesela@example.com" },
                    new Student { FirstName="Jana",  LastName="Horáková", Year=3, Email="jana.horakova@example.com" },
                    new Student { FirstName="Filip", LastName="Král",     Year=1, Email="filip.kral@example.com" }
                };
                Students.AddRange(initial);                 // vlož 10 řádků
                SaveChanges();                              // ulož do DB
            }
        }
    }
}
```

---

## `MainWindow.xaml` (bez komentářů – finální)

```xml
<Window x:Class="DatabazeVipis.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        Title="Studenti" Height="600" Width="900">
    <Grid Margin="12">
        <Grid.RowDefinitions>
            <RowDefinition Height="2*"/>
            <RowDefinition Height="6"/>
            <RowDefinition Height="Auto"/>
        </Grid.RowDefinitions>

        <DataGrid x:Name="StudentsGrid"
                  Grid.Row="0"
                  AutoGenerateColumns="False"
                  CanUserAddRows="False"
                  IsReadOnly="False"
                  SelectionMode="Single">
            <DataGrid.Columns>
                <DataGridTextColumn Header="ID"        Binding="{Binding Id}"        IsReadOnly="True" Width="70"/>
                <DataGridTextColumn Header="Jméno"     Binding="{Binding FirstName, Mode=TwoWay, UpdateSourceTrigger=PropertyChanged}" Width="*"/>
                <DataGridTextColumn Header="Příjmení"  Binding="{Binding LastName,  Mode=TwoWay, UpdateSourceTrigger=PropertyChanged}" Width="*"/>
                <DataGridTextColumn Header="Ročník"    Binding="{Binding Year,      Mode=TwoWay, UpdateSourceTrigger=PropertyChanged}" Width="90"/>
                <DataGridTextColumn Header="E‑mail"    Binding="{Binding Email,     Mode=TwoWay, UpdateSourceTrigger=PropertyChanged}" Width="2*"/>
                <DataGridTextColumn Header="Vytvořeno" Binding="{Binding CreatedAt, Mode=OneWay,  StringFormat={}{0:yyyy-MM-dd HH:mm:ss}}" IsReadOnly="True" Width="180"/>
            </DataGrid.Columns>
        </DataGrid>

        <GridSplitter Grid.Row="1" Height="6" HorizontalAlignment="Stretch" VerticalAlignment="Center" Background="#DDD"/>

        <Grid Grid.Row="2" Margin="0,10,0,0">
            <Grid.ColumnDefinitions>
                <ColumnDefinition Width="Auto"/>
                <ColumnDefinition Width="2*"/>
                <ColumnDefinition Width="16"/>
                <ColumnDefinition Width="Auto"/>
                <ColumnDefinition Width="2*"/>
                <ColumnDefinition Width="*"/>
            </Grid.ColumnDefinitions>
            <Grid.RowDefinitions>
                <RowDefinition Height="Auto"/>
                <RowDefinition Height="Auto"/>
                <RowDefinition Height="Auto"/>
            </Grid.RowDefinitions>

            <TextBlock Grid.Row="0" Grid.Column="0" Margin="0,0,6,8" VerticalAlignment="Center" Text="Jméno:"/>
            <TextBox  x:Name="TxtFirstName" Grid.Row="0" Grid.Column="1" Margin="0,0,0,8"/>

            <TextBlock Grid.Row="0" Grid.Column="3" Margin="0,0,6,8" VerticalAlignment="Center" Text="Příjmení:"/>
            <TextBox  x:Name="TxtLastName" Grid.Row="0" Grid.Column="4" Margin="0,0,0,8"/>

            <TextBlock Grid.Row="1" Grid.Column="0" Margin="0,0,6,8" VerticalAlignment="Center" Text="Ročník (1–6):"/>
            <TextBox  x:Name="TxtYear" Grid.Row="1" Grid.Column="1" Margin="0,0,0,8"/>

            <TextBlock Grid.Row="1" Grid.Column="3" Margin="0,0,6,8" VerticalAlignment="Center" Text="E‑mail:"/>
            <TextBox  x:Name="TxtEmail" Grid.Row="1" Grid.Column="4" Margin="0,0,0,8"/>

            <StackPanel Grid.Row="0" Grid.RowSpan="2" Grid.Column="5" Orientation="Vertical" HorizontalAlignment="Right">
                <Button x:Name="BtnAddStudent"     Content="Přidat studenta"  Padding="16,6" Margin="0,0,0,8" Click="BtnAddStudent_Click"/>
                <Button x:Name="BtnDeleteSelected" Content="Smazat vybraného" Padding="16,6" Margin="0,0,0,8" Click="BtnDeleteSelected_Click"/>
                <Button x:Name="BtnSave"           Content="💾 Uložit data"   Padding="16,6" Click="BtnSave_Click"/>
            </StackPanel>
        </Grid>
    </Grid>
</Window>
```

## `MainWindow.xaml` (s komentáři – finální)

```xml
<Window x:Class="DatabazeVipis.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"   <!-- WPF prvky -->
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"              <!-- XAML rozšíření -->
        Title="Studenti" Height="600" Width="900">
    <Grid Margin="12">                                                       <!-- Hlavní mřížka, 3 řádky -->
        <Grid.RowDefinitions>
            <RowDefinition Height="2*"/>                                     <!-- horní – DataGrid -->
            <RowDefinition Height="6"/>                                      <!-- splitter -->
            <RowDefinition Height="Auto"/>                                   <!-- formulář -->
        </Grid.RowDefinitions>

        <!-- TABULKA STUDENTŮ -->
        <DataGrid x:Name="StudentsGrid"
                  Grid.Row="0"
                  AutoGenerateColumns="False"                                 <!-- definujeme ručně -->
                  CanUserAddRows="False"                                      <!-- žádný prázdný poslední řádek -->
                  IsReadOnly="False"                                          <!-- povolíme editaci v buňkách -->
                  SelectionMode="Single">                                     <!-- vyber vždy jeden řádek -->
            <DataGrid.Columns>
                <DataGridTextColumn Header="ID"        Binding="{Binding Id}" IsReadOnly="True" Width="70"/>
                <DataGridTextColumn Header="Jméno"     Binding="{Binding FirstName, Mode=TwoWay, UpdateSourceTrigger=PropertyChanged}" Width="*"/>
                <DataGridTextColumn Header="Příjmení"  Binding="{Binding LastName,  Mode=TwoWay, UpdateSourceTrigger=PropertyChanged}" Width="*"/>
                <DataGridTextColumn Header="Ročník"    Binding="{Binding Year,      Mode=TwoWay, UpdateSourceTrigger=PropertyChanged}" Width="90"/>
                <DataGridTextColumn Header="E‑mail"    Binding="{Binding Email,     Mode=TwoWay, UpdateSourceTrigger=PropertyChanged}" Width="2*"/>
                <DataGridTextColumn Header="Vytvořeno" Binding="{Binding CreatedAt, Mode=OneWay,  StringFormat={}{0:yyyy-MM-dd HH:mm:ss}}" IsReadOnly="True" Width="180"/>
            </DataGrid.Columns>
        </DataGrid>

        <!-- DĚLIČ mezi tabulkou a formulářem -->
        <GridSplitter Grid.Row="1" Height="6" HorizontalAlignment="Stretch" VerticalAlignment="Center" Background="#DDD"/>

        <!-- FORMULÁŘ PRO PŘIDÁNÍ A OVLÁDÁNÍ -->
        <Grid Grid.Row="2" Margin="0,10,0,0">
            <Grid.ColumnDefinitions>
                <ColumnDefinition Width="Auto"/>   <!-- popisky vlevo -->
                <ColumnDefinition Width="2*"/>    <!-- textboxy vlevo -->
                <ColumnDefinition Width="16"/>    <!-- mezera -->
                <ColumnDefinition Width="Auto"/>  <!-- popisky vpravo -->
                <ColumnDefinition Width="2*"/>    <!-- textboxy vpravo -->
                <ColumnDefinition Width="*"/>     <!-- sloupec pro tlačítka -->
            </Grid.ColumnDefinitions>
            <Grid.RowDefinitions>
                <RowDefinition Height="Auto"/>
                <RowDefinition Height="Auto"/>
                <RowDefinition Height="Auto"/>
            </Grid.RowDefinitions>

            <!-- Jméno/Příjmení -->
            <TextBlock Grid.Row="0" Grid.Column="0" Margin="0,0,6,8" VerticalAlignment="Center" Text="Jméno:"/>
            <TextBox  x:Name="TxtFirstName" Grid.Row="0" Grid.Column="1" Margin="0,0,0,8"/>
            <TextBlock Grid.Row="0" Grid.Column="3" Margin="0,0,6,8" VerticalAlignment="Center" Text="Příjmení:"/>
            <TextBox  x:Name="TxtLastName"  Grid.Row="0" Grid.Column="4" Margin="0,0,0,8"/>

            <!-- Ročník/E‑mail -->
            <TextBlock Grid.Row="1" Grid.Column="0" Margin="0,0,6,8" VerticalAlignment="Center" Text="Ročník (1–6):"/>
            <TextBox  x:Name="TxtYear"      Grid.Row="1" Grid.Column="1" Margin="0,0,0,8"/>
            <TextBlock Grid.Row="1" Grid.Column="3" Margin="0,0,6,8" VerticalAlignment="Center" Text="E‑mail:"/>
            <TextBox  x:Name="TxtEmail"     Grid.Row="1" Grid.Column="4" Margin="0,0,0,8"/>

            <!-- Ovládací tlačítka -->
            <StackPanel Grid.Row="0" Grid.RowSpan="2" Grid.Column="5" Orientation="Vertical" HorizontalAlignment="Right">
                <Button x:Name="BtnAddStudent"     Content="Přidat studenta"  Padding="16,6" Margin="0,0,0,8" Click="BtnAddStudent_Click"/>
                <Button x:Name="BtnDeleteSelected" Content="Smazat vybraného" Padding="16,6" Margin="0,0,0,8" Click="BtnDeleteSelected_Click"/>
                <Button x:Name="BtnSave"           Content="💾 Uložit data"   Padding="16,6" Click="BtnSave_Click"/>
            </StackPanel>
        </Grid>
    </Grid>
</Window>
```

---

## `MainWindow.xaml.cs` (bez komentářů – finální)

```csharp
using DatabazeVipis.Data;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace DatabazeVipis
{
    public partial class MainWindow : Window
    {
        private readonly StudentContext _db = new StudentContext();
        private readonly ObservableCollection<Student> _students = new ObservableCollection<Student>();
        private ICollectionView _studentsView;

        public MainWindow()
        {
            InitializeComponent();

            _db.EnsureCreatedAndSeed();

            foreach (var s in _db.Students.OrderBy(x => x.Id).ToList())
                _students.Add(s);

            _studentsView = CollectionViewSource.GetDefaultView(_students);
            _studentsView.SortDescriptions.Clear();
            _studentsView.SortDescriptions.Add(new SortDescription(nameof(Student.Id), ListSortDirection.Ascending));

            StudentsGrid.ItemsSource = _studentsView;
        }

        private void BtnAddStudent_Click(object sender, RoutedEventArgs e)
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

            if (!int.TryParse(yearText, out int year) || year < 1 || year > 6)
            {
                MessageBox.Show("Ročník zadej jako celé číslo v intervalu 1–6.", "Upozornění", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            if (!string.IsNullOrWhiteSpace(email) && _db.Students.Any(s => s.Email == email))
            {
                MessageBox.Show("Zadaný e‑mail už v databázi existuje.", "Upozornění", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var s = new Student
            {
                FirstName = firstName,
                LastName = lastName,
                Year = year,
                Email = email,
                CreatedAt = System.DateTime.UtcNow
            };

            _db.Students.Add(s);
            _db.SaveChanges();

            _students.Add(s);
            StudentsGrid.SelectedItem = s;
            StudentsGrid.ScrollIntoView(s);

            TxtFirstName.Text = TxtLastName.Text = TxtEmail.Text = TxtYear.Text = string.Empty;
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                StudentsGrid.CommitEdit(DataGridEditingUnit.Cell, true);
                StudentsGrid.CommitEdit(DataGridEditingUnit.Row, true);

                if (!_db.ChangeTracker.HasChanges())
                {
                    MessageBox.Show("Žádné změny k uložení.", "Informace", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                _db.SaveChanges();
                MessageBox.Show("Změny byly uloženy.", "Hotovo", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (System.Exception ex)
            {
                MessageBox.Show("Nepodařilo se uložit změny.\n\n" + ex, "Chyba", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnDeleteSelected_Click(object sender, RoutedEventArgs e)
        {
            var selected = StudentsGrid.SelectedItem as Student;
            if (selected == null)
            {
                MessageBox.Show("Nejprve vyberte studenta v tabulce.", "Upozornění", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var answer = MessageBox.Show("Opravdu smazat vybraného studenta?",
                "Potvrzení", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (answer != MessageBoxResult.Yes)
                return;

            _db.Students.Remove(selected);
            try
            {
                _db.SaveChanges();
                _students.Remove(selected);
            }
            catch (System.Exception ex)
            {
                MessageBox.Show("Smazání se nepodařilo.\n\n" + ex, "Chyba", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        protected override void OnClosed(System.EventArgs e)
        {
            _db.Dispose();
            base.OnClosed(e);
        }
    }
}
```

## `MainWindow.xaml.cs` (s komentáři – finální)

```csharp
using DatabazeVipis.Data;                     // entita a DbContext
using Microsoft.EntityFrameworkCore;          // EF Core (tracking, změny)
using System.Collections.ObjectModel;         // ObservableCollection – kolekce pro UI
using System.ComponentModel;                  // ICollectionView (řazení)
using System.Linq;                             // LINQ: OrderBy, Any, ToList
using System.Windows;                         // Window, MessageBox
using System.Windows.Controls;                // DataGridEditingUnit
using System.Windows.Data;                    // CollectionViewSource

namespace DatabazeVipis
{
    public partial class MainWindow : Window
    {
        private readonly StudentContext _db = new StudentContext();             // dlouho-žijící kontext
        private readonly ObservableCollection<Student> _students = new();       // zdroj pro DataGrid
        private ICollectionView _studentsView;                                  // řazení, filtr (zde jen řazení)

        public MainWindow()
        {
            InitializeComponent();                                              // vytvoření UI z XAMLu

            _db.EnsureCreatedAndSeed();                                         // první běh: vytvoř + seed

            foreach (var s in _db.Students.OrderBy(x => x.Id).ToList())         // načti z DB a vlož do kolekce
                _students.Add(s);

            _studentsView = CollectionViewSource.GetDefaultView(_students);     // "obal" pro řazení
            _studentsView.SortDescriptions.Clear();
            _studentsView.SortDescriptions.Add(new SortDescription(nameof(Student.Id), ListSortDirection.Ascending));

            StudentsGrid.ItemsSource = _studentsView;                           // napoj DataGrid
        }

        // Přidat studenta (s jednoduchou validací vstupu).
        private void BtnAddStudent_Click(object sender, RoutedEventArgs e)
        {
            string firstName = (TxtFirstName.Text ?? string.Empty).Trim();
            string lastName  = (TxtLastName.Text  ?? string.Empty).Trim();
            string email     = (TxtEmail.Text     ?? string.Empty).Trim();
            string yearText  = (TxtYear.Text      ?? string.Empty).Trim();

            if (string.IsNullOrWhiteSpace(firstName) || string.IsNullOrWhiteSpace(lastName))
            {
                MessageBox.Show("Jméno i příjmení jsou povinné.", "Upozornění", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            if (!int.TryParse(yearText, out int year) || year < 1 || year > 6)
            {
                MessageBox.Show("Ročník zadej jako celé číslo v intervalu 1–6.", "Upozornění", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            if (!string.IsNullOrWhiteSpace(email) && _db.Students.Any(s => s.Email == email))
            {
                MessageBox.Show("Zadaný e‑mail už v databázi existuje.", "Upozornění", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var s = new Student
            {
                FirstName = firstName,
                LastName  = lastName,
                Year      = year,
                Email     = email,
                CreatedAt = System.DateTime.UtcNow
            };

            _db.Students.Add(s);                                                // přidej do kontextu
            _db.SaveChanges();                                                  // DB uloží a doplní nové Id

            _students.Add(s);                                                   // přidej do kolekce pro UI
            StudentsGrid.SelectedItem = s;                                      // označ nového
            StudentsGrid.ScrollIntoView(s);                                     // sroluj na něj

            // vyčisti formulář
            TxtFirstName.Text = TxtLastName.Text = TxtEmail.Text = TxtYear.Text = string.Empty;
        }

        // Ulož všechny změny (včetně editací v DataGridu).
        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // pokud je buňka/řádek v editačním režimu, potvrď editaci do objektu
                StudentsGrid.CommitEdit(DataGridEditingUnit.Cell, true);
                StudentsGrid.CommitEdit(DataGridEditingUnit.Row,  true);

                if (!_db.ChangeTracker.HasChanges())                            // není co ukládat
                {
                    MessageBox.Show("Žádné změny k uložení.", "Informace", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                _db.SaveChanges();                                              // zápis do DB
                MessageBox.Show("Změny byly uloženy.", "Hotovo", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (System.Exception ex)
            {
                MessageBox.Show("Nepodařilo se uložit změny.\n\n" + ex, "Chyba", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Smazání vybrané položky.
        private void BtnDeleteSelected_Click(object sender, RoutedEventArgs e)
        {
            var selected = StudentsGrid.SelectedItem as Student;
            if (selected == null)
            {
                MessageBox.Show("Nejprve vyberte studenta v tabulce.", "Upozornění", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var answer = MessageBox.Show("Opravdu smazat vybraného studenta?",
                "Potvrzení", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (answer != MessageBoxResult.Yes)
                return;

            _db.Students.Remove(selected);                                      // odeber z kontextu
            try
            {
                _db.SaveChanges();                                              // ulož do DB
                _students.Remove(selected);                                     // odeber i z UI kolekce
            }
            catch (System.Exception ex)
            {
                MessageBox.Show("Smazání se nepodařilo.\n\n" + ex, "Chyba", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Úklid – zavřít připojení k databázi.
        protected override void OnClosed(System.EventArgs e)
        {
            _db.Dispose();
            base.OnClosed(e);
        }
    }
}
```

---

## Poznámky k XAML a napojení na code‑behind
- `x:Name="StudentsGrid"`/`x:Name="TxtFirstName"`… → code‑behind může k prvkům přistupovat přes stejné názvy.  
- `Binding … Mode=TwoWay, UpdateSourceTrigger=PropertyChanged` → hodnota z buňky se zapisuje do objektu při každé změně (ne až při opuštění).  
- `ObservableCollection<T>` → když přidáme/odebereme položku v kolekci, DataGrid se sám překreslí.  
- `PropertyChanged.Fody` + atribut `[AddINotifyPropertyChangedInterface]` → při změnách vlastností entit je UI spolehlivě informováno bez ručního psaní `INotifyPropertyChanged`.

---

## Tipy k řešení typických problémů
- **„File is locked… apphost.exe … being used by another process“** → pravděpodobně běží předchozí instance aplikace na pozadí. Zavřete okno nebo v **Debug** ▸ **Stop Debugging**.  
- **Nevidím DB** v SQL Server Object Exploreru → VS **View ▸ SQL Server Object Explorer**. DB je `StudentDbDemo` pod `(localdb)\MSSQLLocalDB`.  
- **AsNoTracking()**: pro editace nepoužívejte u načítání – chceme tracking změn (pak stačí `SaveChanges()`).  
- **Auto‑increment Id** → `[Key]` + `[DatabaseGenerated(DatabaseGeneratedOption.Identity)]` v entitě.  

---

## Shrnutí klikacích kroků (rychlá taháková verze)
1. **New WPF App (.NET 9)** → název bez diakritiky.  
2. **Manage NuGet Packages** → nainstalujte:  
   `Microsoft.EntityFrameworkCore.SqlServer`, `Microsoft.EntityFrameworkCore.Tools`, `PropertyChanged.Fody` (Fody se přidá sám).  
3. Vytvořte **FodyWeavers.xml** v projektu (obsah viz výše).  
4. Přidejte složku **Data** a soubory `Student.cs`, `StudentContext.cs` (kódy výše).  
5. Upravte `MainWindow.xaml` a `MainWindow.xaml.cs` (kódy výše).  
6. Spusťte ▶ – při prvním běhu se DB vytvoří a naplní.  
7. Přidávejte, upravujte, mazejte a ukládejte data.  

---

## Pozn. k materialům kurzu
Materiál je kompatibilní s přístupem **Code First** a prací s EF Core popsanou v podkladech k DBC (základy XAML, MVVM, kolekce a validace jsou s tím plně v souladu).

---

**Hotovo.** Nyní můžete README sdílet se studenty.
