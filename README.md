# WPF + EF Core (.NET 9) – **Student** demo krok‑za‑krokem (VS 2022, EN UI)

> Tento návod navazuje na styl práce používaný ve cvičných materiálech k předmětu DBC (Entity Framework, WPF, MVVM) a je zjednodušený pro úplné začátky. fileciteturn0file0

---

## Co postavíme
Jednoduchou **WPF** aplikaci na .NET 9, která se připojí na lokální **SQL Server LocalDB**, vytvoří tabulku **Students**, naplní ji daty a umožní:
1) **Zobrazit** záznamy v tabulce (read‑only).  
2) **Přidat** nový záznam přes formulář (bez validací).  
3) **Přidat základní validace** vstupů.  
4) **Upravovat přímo v tabulce, mazat a ukládat** změny tlačítkem **💾 Save**.

Vše je **klikací** z prostředí Visual Studio 2022 (EN UI). Studenti budou upravovat jen **4 soubory**:
- `Data/Student.cs` (entita/řádka tabulky)
- `Data/StudentContext.cs` (připojení k DB)
- `MainWindow.xaml` (UI)
- `MainWindow.xaml.cs` (logika okna)

> Pozn.: Ukázkový název projektu je **DatabazeVipis**. Pokud používáte jiný název/namespace, upravte jej v uvedených fragmentech.

---

## Předpoklady (na školních PC obvykle připraveno)
- **Visual Studio 2022** (EN UI), workload *“.NET Desktop Development“*.
- **.NET SDK 9**.
- **SQL Server Express LocalDB** (součást VS workloadu; v nápovědě níže ukážeme, jak data i tabulku vizuálně otevřít).
- Internet pro instalaci NuGet balíčků.

---

## NuGet balíčky, které nainstalujeme
Budeme používat **3 balíčky** (instalace **jen do WPF projektu**):
1. `Microsoft.EntityFrameworkCore.SqlServer` (EF Core provider pro SQL Server)
2. `Microsoft.EntityFrameworkCore.Tools` (design‑time nástroje – zde kvůli verzi a analyzérům)
3. `PropertyChanged.Fody` (automaticky přidá INotifyPropertyChanged – pohodlnější binding ve WPF)

> Při instalaci `PropertyChanged.Fody` se automaticky přidá závislost `Fody`. Navíc **musíme přidat soubor** `FodyWeavers.xml` (viz krok níže).

### Jak NuGet **klikací** nainstalovat
1. **Solution Explorer** → **pravé tlačítko** na projekt (např. *DatabazeVipis*) → **Manage NuGet Packages…**
2. Záložka **Browse** → postupně vyhledejte a nainstalujte:
   - *Microsoft.EntityFrameworkCore.SqlServer* (verze **9.x**)
   - *Microsoft.EntityFrameworkCore.Tools* (verze **9.x**)
   - *PropertyChanged.Fody* (aktuální)
3. Po instalaci `PropertyChanged.Fody` **přidejte nový soubor**:  
   **Project** → **Add** → **New Item…** → **XML File** → **Name**: `FodyWeavers.xml` → vložte obsah:
   ```xml
   <?xml version="1.0" encoding="utf-8"?>
   <Weavers>
     <PropertyChanged/>
   </Weavers>
   ```

---

# Úkol 1 — Založení projektu, DB a **read‑only** výpis

### 1) Založte WPF projekt (.NET 9)
- VS: **File** → **New** → **Project** → vyberte **WPF App** (.NET) → **Next**
- **Project name**: `DatabazeVipis` (bez diakritiky a mezer)
- **Framework**: **.NET 9 (net9.0)** → **Create**

### 2) Ověřte csproj (.NET 9 + WPF)
`DatabazeVipis.csproj` by měl obsahovat:
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

### 3) Přidejte složku **Data** a do ní dvě třídy
- **Solution Explorer** → pravým na projekt → **Add** → **New Folder** → `Data`
- Do složky **Data** přidejte **Class** → `Student.cs`
- Do složky **Data** přidejte **Class** → `StudentContext.cs`

#### `Data/Student.cs` — verze **bez komentářů**
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

        [Required, StringLength(100)]
        public string FirstName { get; set; } = string.Empty;

        [Required, StringLength(100)]
        public string LastName { get; set; } = string.Empty;

        [Range(1, 6)]
        public int Year { get; set; }

        [StringLength(200)]
        public string Email { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
```

#### `Data/Student.cs` — verze **s komentáři (řádek po řádku)**
```csharp
using System;                                           // Běžné typy (DateTime apod.)
using System.ComponentModel.DataAnnotations;            // Datové anotace: [Key], [Required], [Range]...
using System.ComponentModel.DataAnnotations.Schema;     // [DatabaseGenerated] – auto-increment
using PropertyChanged;                                   // Atribut pro Fody – INotifyPropertyChanged „magicky“

// Namespace projektu (u vás může být jiný název projektu)
namespace DatabazeVipis.Data
{
    // Fody při překladu doplní implementaci INotifyPropertyChanged pro všechny public vlastnosti
    [AddINotifyPropertyChangedInterface]
    public class Student
    {
        // Primární klíč tabulky, auto-increment (IDENTITY)
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        // Povinné textové pole, max 100 znaků
        [Required, StringLength(100)]
        public string FirstName { get; set; } = string.Empty;

        // Povinné textové pole, max 100 znaků
        [Required, StringLength(100)]
        public string LastName { get; set; } = string.Empty;

        // Celé číslo 1–6 (ročník)
        [Range(1, 6)]
        public int Year { get; set; }

        // Nepovinný text, max 200 znaků (validaci formátu e-mailu řešit nebudeme)
        [StringLength(200)]
        public string Email { get; set; } = string.Empty;

        // Vytvořeno – defaultně UTC při vytvoření objektu
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
```

#### `Data/StudentContext.cs` — verze **bez komentářů**
```csharp
using Microsoft.EntityFrameworkCore;
using System;
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

#### `Data/StudentContext.cs` — verze **s komentáři**
```csharp
using Microsoft.EntityFrameworkCore;        // EF Core – DbContext, DbSet, UseSqlServer
using System;                               // Obecné typy
using System.Collections.Generic;           // List<T>
using System.Linq;                          // Any(), OrderBy(), ToList()

namespace DatabazeVipis.Data
{
    // Hlavní třída pro práci s databází (Unit of Work / kontext)
    public class StudentContext : DbContext
    {
        // Reprezentuje tabulku Students (každý Student = jeden řádek)
        public DbSet<Student> Students => Set<Student>();

        // Nastavení připojení – zde přímo v kódu (pro výuku nejjednodušší varianta)
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // Nastavíme provider jen pokud ještě není nastaven (např. z testů)
            if (!optionsBuilder.IsConfigured)
            {
                // LocalDB – lokální vývojářský SQL Server (součást VS)
                // Databáze StudentDbDemo se vytvoří automaticky při EnsureCreated()
                optionsBuilder.UseSqlServer(
                    "Data Source=(localdb)\\MSSQLLocalDB;" +
                    "Initial Catalog=StudentDbDemo;" +
                    "Integrated Security=True;" +
                    "TrustServerCertificate=True");
            }
        }

        // Vytvoření DB a naplnění ukázkami (spustíme na startu aplikace)
        public void EnsureCreatedAndSeed()
        {
            Database.EnsureCreated(); // založí DB a tabulku dle modelu
            SeedIfEmpty();            // doplní 10 řádků, pokud je tabulka prázdná
        }

        // Jednoduchý seed – jen při prázdné tabulce
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
                Students.AddRange(initial); // vložíme do kontextu
                SaveChanges();              // zapíšeme do DB (vygenerují se Id)
            }
        }
    }
}
```

### 4) UI pro **read‑only** výpis
Otevřete `MainWindow.xaml` a nahraďte obsah:

#### `MainWindow.xaml` — verze **bez komentářů**
```xml
<Window x:Class="DatabazeVipis.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        Title="Studenti" Height="500" Width="900">
    <Grid Margin="12">
        <DataGrid x:Name="StudentsGrid"
                  AutoGenerateColumns="False"
                  IsReadOnly="True"
                  CanUserAddRows="False"
                  CanUserDeleteRows="False">
            <DataGrid.Columns>
                <DataGridTextColumn Header="ID"        Binding="{Binding Id}"        Width="70"/>
                <DataGridTextColumn Header="Jméno"     Binding="{Binding FirstName}" Width="*"/>
                <DataGridTextColumn Header="Příjmení"  Binding="{Binding LastName}"  Width="*"/>
                <DataGridTextColumn Header="Ročník"    Binding="{Binding Year}"      Width="90"/>
                <DataGridTextColumn Header="E‑mail"    Binding="{Binding Email}"     Width="2*"/>
                <DataGridTextColumn Header="Vytvořeno" Binding="{Binding CreatedAt, StringFormat={}{0:yyyy-MM-dd HH:mm:ss}}" Width="180"/>
            </DataGrid.Columns>
        </DataGrid>
    </Grid>
</Window>
```

#### `MainWindow.xaml` — verze **s komentáři**
```xml
<Window x:Class="DatabazeVipis.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"  <!-- WPF tagy -->
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"             <!-- x:Name apod. -->
        Title="Studenti" Height="500" Width="900">
    <Grid Margin="12">                                                     <!-- 12 px okraje -->
        <DataGrid x:Name="StudentsGrid"                                   <!-- hlavní tabulka -->
                  AutoGenerateColumns="False"                             <!-- sloupce definujeme ručně -->
                  IsReadOnly="True"                                       <!-- jen pro čtení (Úkol 1) -->
                  CanUserAddRows="False"                                  <!-- bez prázdného řádku -->
                  CanUserDeleteRows="False">
            <DataGrid.Columns>                                            <!-- ruční definice sloupců -->
                <DataGridTextColumn Header="ID"        Binding="{Binding Id}"        Width="70"/>
                <DataGridTextColumn Header="Jméno"     Binding="{Binding FirstName}" Width="*"/>
                <DataGridTextColumn Header="Příjmení"  Binding="{Binding LastName}"  Width="*"/>
                <DataGridTextColumn Header="Ročník"    Binding="{Binding Year}"      Width="90"/>
                <DataGridTextColumn Header="E‑mail"    Binding="{Binding Email}"     Width="2*"/>
                <DataGridTextColumn Header="Vytvořeno" Binding="{Binding CreatedAt, StringFormat={}{0:yyyy-MM-dd HH:mm:ss}}" Width="180"/>
            </DataGrid.Columns>
        </DataGrid>
    </Grid>
</Window>
```

### 5) Kód okna – načtení a zobrazení dat
Otevřete `MainWindow.xaml.cs` a nahraďte:

#### `MainWindow.xaml.cs` — verze **bez komentářů**
```csharp
using DatabazeVipis.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Windows;

namespace DatabazeVipis
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            using var db = new StudentContext();
            db.EnsureCreatedAndSeed();

            var data = db.Students
                         .AsNoTracking()
                         .OrderBy(s => s.Id)
                         .ToList();
            StudentsGrid.ItemsSource = data;
        }
    }
}
```

#### `MainWindow.xaml.cs` — verze **s komentáři**
```csharp
using DatabazeVipis.Data;           // Student, StudentContext
using Microsoft.EntityFrameworkCore; // AsNoTracking()
using System.Linq;                   // OrderBy(), ToList()
using System.Windows;                // Window, MessageBox

namespace DatabazeVipis
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();           // vytvoří UI dle XAML

            // Krátce žijící kontext jen pro načtení dat (Úkol 1 = read-only)
            using var db = new StudentContext();

            // Založí DB + tabulku a naplní 10 záznamy (pokud je prázdná)
            db.EnsureCreatedAndSeed();

            // Načti data bez „tracking“ (rychlejší pro čtení) a seřaď podle Id
            var data = db.Students
                         .AsNoTracking()
                         .OrderBy(s => s.Id)
                         .ToList();

            // Přiřaď seznam jako zdroj pro DataGrid
            StudentsGrid.ItemsSource = data;
        }
    }
}
```

### ✅ Stav po Úkolu 1
- Aplikace se spustí → DB se vytvoří → v tabulce se **zobrazí 10 záznamů** (jen čtení).

---

# Úkol 2 — **Přidání** nového studenta (bez validací)

> Cíl: pod tabulku přidáme jednoduchý formulář a tlačítko **Přidat**. **Žádné kontroly** – jde o první, nejjednodušší verzi.

### 1) Rozšiřte XAML o formulář
Otevřete `MainWindow.xaml` a nahraďte celý obsah tímto (přidali jsme 2. řádek s formulářem):

#### `MainWindow.xaml` (Úkol 2) — **bez komentářů**
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
                  IsReadOnly="True"
                  CanUserAddRows="False"
                  CanUserDeleteRows="False">
            <DataGrid.Columns>
                <DataGridTextColumn Header="ID"        Binding="{Binding Id}"        Width="70"/>
                <DataGridTextColumn Header="Jméno"     Binding="{Binding FirstName}" Width="*"/>
                <DataGridTextColumn Header="Příjmení"  Binding="{Binding LastName}"  Width="*"/>
                <DataGridTextColumn Header="Ročník"    Binding="{Binding Year}"      Width="90"/>
                <DataGridTextColumn Header="E‑mail"    Binding="{Binding Email}"     Width="2*"/>
                <DataGridTextColumn Header="Vytvořeno" Binding="{Binding CreatedAt, StringFormat={}{0:yyyy-MM-dd HH:mm:ss}}" Width="180"/>
            </DataGrid.Columns>
        </DataGrid>

        <GroupBox Grid.Row="1" Header="Přidat studenta" Margin="0,10,0,0">
            <Grid Margin="10">
                <Grid.ColumnDefinitions>
                    <ColumnDefinition Width="Auto"/>
                    <ColumnDefinition Width="2*"/>
                    <ColumnDefinition Width="Auto"/>
                    <ColumnDefinition Width="2*"/>
                    <ColumnDefinition Width="Auto"/>
                </Grid.ColumnDefinitions>
                <Grid.RowDefinitions>
                    <RowDefinition Height="Auto"/>
                    <RowDefinition Height="Auto"/>
                    <RowDefinition Height="Auto"/>
                </Grid.RowDefinitions>

                <TextBlock Grid.Row="0" Grid.Column="0" Text="Jméno:"    Margin="0,0,8,6" VerticalAlignment="Center"/>
                <TextBox   Grid.Row="0" Grid.Column="1" x:Name="TxtFirstName" Margin="0,0,16,6"/>

                <TextBlock Grid.Row="0" Grid.Column="2" Text="Příjmení:" Margin="0,0,8,6" VerticalAlignment="Center"/>
                <TextBox   Grid.Row="0" Grid.Column="3" x:Name="TxtLastName"  Margin="0,0,16,6"/>

                <TextBlock Grid.Row="1" Grid.Column="0" Text="Ročník:"   Margin="0,0,8,6" VerticalAlignment="Center"/>
                <TextBox   Grid.Row="1" Grid.Column="1" x:Name="TxtYear"      Margin="0,0,16,6"/>

                <TextBlock Grid.Row="1" Grid.Column="2" Text="E‑mail:"   Margin="0,0,8,6" VerticalAlignment="Center"/>
                <TextBox   Grid.Row="1" Grid.Column="3" x:Name="TxtEmail"     Margin="0,0,16,6"/>

                <Button Grid.Row="0" Grid.RowSpan="2" Grid.Column="4"
                        x:Name="BtnAdd"
                        Content="Přidat"
                        Padding="16,6"
                        Click="BtnAdd_Click"/>
            </Grid>
        </GroupBox>
    </Grid>
</Window>
```

### 2) Přidejte obsluhu tlačítka (bez kontrol)
Otevřete `MainWindow.xaml.cs` a doplňte **metodu pro načtení** (reuse z Úkolu 1) a klik na **Přidat**:

#### `MainWindow.xaml.cs` (Úkol 2) — **bez komentářů**
```csharp
using DatabazeVipis.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Windows;

namespace DatabazeVipis
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            using var db = new StudentContext();
            db.EnsureCreatedAndSeed();
            LoadData();
        }

        private void LoadData()
        {
            using var db = new StudentContext();
            StudentsGrid.ItemsSource = db.Students
                                         .AsNoTracking()
                                         .OrderBy(s => s.Id)
                                         .ToList();
        }

        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            string first = (TxtFirstName.Text ?? string.Empty).Trim();
            string last  = (TxtLastName.Text  ?? string.Empty).Trim();
            string email = (TxtEmail.Text     ?? string.Empty).Trim();
            int year = 1;
            int.TryParse((TxtYear.Text ?? string.Empty).Trim(), out year);

            using var db = new StudentContext();
            var s = new Student
            {
                FirstName = first,
                LastName  = last,
                Year      = year,
                Email     = email,
                CreatedAt = DateTime.UtcNow
            };
            db.Students.Add(s);
            db.SaveChanges();

            LoadData();

            TxtFirstName.Clear();
            TxtLastName.Clear();
            TxtYear.Clear();
            TxtEmail.Clear();
        }
    }
}
```

### ✅ Stav po Úkolu 2
- Lze **přidat řádek** tlačítkem **Přidat** a znovu se načte tabulka. (Zatím **bez validací**.)

---

# Úkol 3 — **Základní validace** vstupů (před uložením)

> Cíl: před uložením ověřit, že **Jméno**, **Příjmení** a **E‑mail** nejsou prázdné a **Ročník** je celé číslo **1–6**. (Formát e‑mailu **neřešíme**.)

Otevřete `MainWindow.xaml.cs` a **nahraďte** metodu `BtnAdd_Click`:

#### `BtnAdd_Click` (Úkol 3) — **bez komentářů**
```csharp
private void BtnAdd_Click(object sender, RoutedEventArgs e)
{
    string first = (TxtFirstName.Text ?? string.Empty).Trim();
    string last  = (TxtLastName.Text  ?? string.Empty).Trim();
    string email = (TxtEmail.Text     ?? string.Empty).Trim();
    string yearText = (TxtYear.Text ?? string.Empty).Trim();

    if (string.IsNullOrWhiteSpace(first) ||
        string.IsNullOrWhiteSpace(last)  ||
        string.IsNullOrWhiteSpace(email))
    {
        MessageBox.Show("Vyplň Jméno, Příjmení i E‑mail.", "Upozornění",
            MessageBoxButton.OK, MessageBoxImage.Information);
        return;
    }

    if (!int.TryParse(yearText, out int year) || year < 1 || year > 6)
    {
        MessageBox.Show("Ročník zadej jako celé číslo v intervalu 1–6.", "Upozornění",
            MessageBoxButton.OK, MessageBoxImage.Information);
        return;
    }

    using var db = new StudentContext();
    var s = new Student
    {
        FirstName = first,
        LastName  = last,
        Year      = year,
        Email     = email,
        CreatedAt = DateTime.UtcNow
    };
    db.Students.Add(s);
    db.SaveChanges();

    LoadData();

    TxtFirstName.Clear();
    TxtLastName.Clear();
    TxtYear.Clear();
    TxtEmail.Clear();
}
```

#### `BtnAdd_Click` (Úkol 3) — **s komentáři**
```csharp
private void BtnAdd_Click(object sender, RoutedEventArgs e)
{
    // 1) Přečti vstupy z TextBoxů
    string first = (TxtFirstName.Text ?? string.Empty).Trim();
    string last  = (TxtLastName.Text  ?? string.Empty).Trim();
    string email = (TxtEmail.Text     ?? string.Empty).Trim();
    string yearText = (TxtYear.Text ?? string.Empty).Trim();

    // 2) Povinná pole
    if (string.IsNullOrWhiteSpace(first) ||
        string.IsNullOrWhiteSpace(last)  ||
        string.IsNullOrWhiteSpace(email))
    {
        MessageBox.Show("Vyplň Jméno, Příjmení i E‑mail.", "Upozornění",
            MessageBoxButton.OK, MessageBoxImage.Information);
        return;
    }

    // 3) Ročník = celé číslo 1–6
    if (!int.TryParse(yearText, out int year) || year < 1 || year > 6)
    {
        MessageBox.Show("Ročník zadej jako celé číslo v intervalu 1–6.", "Upozornění",
            MessageBoxButton.OK, MessageBoxImage.Information);
        return;
    }

    // 4) Ulož do DB
    using var db = new StudentContext();
    var s = new Student
    {
        FirstName = first,
        LastName  = last,
        Year      = year,
        Email     = email,
        CreatedAt = DateTime.UtcNow // timestamp vytvoření záznamu
    };
    db.Students.Add(s);
    db.SaveChanges(); // teď se vygeneruje nové Id

    // 5) Načti tabulku znovu (jednoduché řešení – v Úkolu 4 zlepšíme)
    LoadData();

    // 6) Vyčisti formulář
    TxtFirstName.Clear();
    TxtLastName.Clear();
    TxtYear.Clear();
    TxtEmail.Clear();
}
```

### ✅ Stav po Úkolu 3
- Před uložením **kontrolujeme vstupy**. Chyby se zobrazí v **MessageBoxu**.

---

# Úkol 4 — **Editace v tabulce, Mazání a Uložení (💾)**

> Cíl: přepneme se na **dlouho‑žijící** `StudentContext`, použijeme **ObservableCollection** (živá kolekce pro WPF), povolíme **TwoWay** editaci v tabulce, přidáme **Smazat vybraného** a **💾 Save** – ukládá všechny rozeditované změny najednou.

### 1) Upravte XAML – povolte editaci + přidejte tlačítka
Otevřete `MainWindow.xaml` a nahraďte **celý soubor**:

#### `MainWindow.xaml` (Úkol 4) — **bez komentářů**
```xml
<Window x:Class="DatabazeVipis.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        Title="Studenti" Height="600" Width="900">
    <Grid Margin="12">
        <Grid.RowDefinitions>
            <RowDefinition Height="*"/>
            <RowDefinition Height="6"/>
            <RowDefinition Height="Auto"/>
        </Grid.RowDefinitions>

        <!-- EDITOVATELNÁ TABULKA -->
        <DataGrid x:Name="StudentsGrid"
                  Grid.Row="0"
                  AutoGenerateColumns="False"
                  IsReadOnly="False"
                  CanUserAddRows="False"
                  SelectionMode="Single">
            <DataGrid.Columns>
                <DataGridTextColumn Header="ID"        Binding="{Binding Id, Mode=OneWay}" IsReadOnly="True" Width="70"/>
                <DataGridTextColumn Header="Jméno"     Binding="{Binding FirstName, Mode=TwoWay, UpdateSourceTrigger=PropertyChanged}" Width="*"/>
                <DataGridTextColumn Header="Příjmení"  Binding="{Binding LastName,  Mode=TwoWay, UpdateSourceTrigger=PropertyChanged}" Width="*"/>
                <DataGridTextColumn Header="Ročník"    Binding="{Binding Year,      Mode=TwoWay, UpdateSourceTrigger=PropertyChanged}" Width="90"/>
                <DataGridTextColumn Header="E‑mail"    Binding="{Binding Email,     Mode=TwoWay, UpdateSourceTrigger=PropertyChanged}" Width="2*"/>
                <DataGridTextColumn Header="Vytvořeno" Binding="{Binding CreatedAt, Mode=OneWay, StringFormat={}{0:yyyy-MM-dd HH:mm:ss}}" IsReadOnly="True" Width="180"/>
            </DataGrid.Columns>
        </DataGrid>

        <!-- DĚLIČ -->
        <GridSplitter Grid.Row="1" Height="6" HorizontalAlignment="Stretch" VerticalAlignment="Center" Background="#DDD"/>

        <!-- FORMULÁŘ + OVLÁDACÍ TLAČÍTKA -->
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

            <TextBlock Grid.Row="0" Grid.Column="0" Margin="0,0,6,8" VerticalAlignment="Center" Text="ID:"/>
            <TextBox  x:Name="TxtId" Grid.Row="0" Grid.Column="1" Margin="0,0,0,8" IsReadOnly="True"/>

            <TextBlock Grid.Row="1" Grid.Column="0" Margin="0,0,6,8" VerticalAlignment="Center" Text="Jméno:"/>
            <TextBox  x:Name="TxtFirstName" Grid.Row="1" Grid.Column="1" Margin="0,0,0,8"/>

            <TextBlock Grid.Row="1" Grid.Column="3" Margin="0,0,6,8" VerticalAlignment="Center" Text="Příjmení:"/>
            <TextBox  x:Name="TxtLastName" Grid.Row="1" Grid.Column="4" Margin="0,0,0,8"/>

            <TextBlock Grid.Row="2" Grid.Column="0" Margin="0,0,6,8" VerticalAlignment="Center" Text="Ročník (1–6):"/>
            <TextBox  x:Name="TxtYear" Grid.Row="2" Grid.Column="1" Margin="0,0,0,8"/>

            <TextBlock Grid.Row="2" Grid.Column="3" Margin="0,0,6,8" VerticalAlignment="Center" Text="E‑mail:"/>
            <TextBox  x:Name="TxtEmail" Grid.Row="2" Grid.Column="4" Margin="0,0,0,8"/>

            <StackPanel Grid.Row="0" Grid.RowSpan="3" Grid.Column="5" Orientation="Vertical" HorizontalAlignment="Right">
                <Button x:Name="BtnAdd"            Content="Přidat"       Padding="16,6" Margin="0,0,0,8" Click="BtnAdd_Click"/>
                <Button x:Name="BtnDeleteSelected" Content="Smazat vybraného" Padding="16,6" Margin="0,0,0,8" Click="BtnDeleteSelected_Click"/>
                <Button x:Name="BtnSave"           Content="💾 Uložit data" Padding="16,6" Click="BtnSave_Click"/>
            </StackPanel>
        </Grid>
    </Grid>
</Window>
```

### 2) Upravte code‑behind – dlouhý kontext + ObservableCollection
Otevřete `MainWindow.xaml.cs` a nahraďte **celý soubor**:

#### `MainWindow.xaml.cs` (Úkol 4) — **bez komentářů**
```csharp
using DatabazeVipis.Data;
using Microsoft.EntityFrameworkCore;
using System;
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

        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            string first = (TxtFirstName.Text ?? string.Empty).Trim();
            string last  = (TxtLastName.Text  ?? string.Empty).Trim();
            string email = (TxtEmail.Text     ?? string.Empty).Trim();
            string yearText = (TxtYear.Text ?? string.Empty).Trim();

            if (string.IsNullOrWhiteSpace(first) ||
                string.IsNullOrWhiteSpace(last)  ||
                string.IsNullOrWhiteSpace(email))
            {
                MessageBox.Show("Vyplň Jméno, Příjmení i E‑mail.", "Upozornění",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            if (!int.TryParse(yearText, out int year) || year < 1 || year > 6)
            {
                MessageBox.Show("Ročník zadej jako celé číslo v intervalu 1–6.", "Upozornění",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var s = new Student
            {
                FirstName = first,
                LastName  = last,
                Year      = year,
                Email     = email,
                CreatedAt = DateTime.UtcNow
            };

            _db.Students.Add(s);
            try
            {
                _db.SaveChanges();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Uložení do databáze se nezdařilo.\n\n" + ex, "Chyba",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            _students.Add(s);
            StudentsGrid.SelectedItem = s;
            StudentsGrid.ScrollIntoView(s);

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
                StudentsGrid.CommitEdit(DataGridEditingUnit.Cell, true);
                StudentsGrid.CommitEdit(DataGridEditingUnit.Row, true);

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
            catch (Exception ex)
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
                MessageBox.Show("Nejprve vyber studenta v tabulce.", "Upozornění",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var answer = MessageBox.Show(
                "Opravdu smazat vybraného studenta?",
                "Potvrzení",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (answer != MessageBoxResult.Yes)
                return;

            _db.Students.Remove(selected);
            try
            {
                _db.SaveChanges();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Smazání se nepodařilo.\n\n" + ex, "Chyba",
                    MessageBoxButton.OK, MessageBoxImage.Error);
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
```

#### `MainWindow.xaml.cs` (Úkol 4) — **s komentáři**
```csharp
using DatabazeVipis.Data;                     // Entita Student + DbContext
using Microsoft.EntityFrameworkCore;           // EF Core (ChangeTracker)
using System;                                  // EventArgs, Exception
using System.Collections.ObjectModel;          // ObservableCollection<T>
using System.ComponentModel;                   // ListSortDirection
using System.Linq;                             
using System.Windows;                          // Window, MessageBox
using System.Windows.Controls;                 // DataGrid
using System.Windows.Data;                     // CollectionViewSource, ICollectionView

namespace DatabazeVipis
{
    public partial class MainWindow : Window
    {
        // 1) Dlouho-žijící kontext – zachytává změny (editace v tabulce)
        private readonly StudentContext _db = new StudentContext();

        // 2) Kolekce pro UI – DataGrid se připojí na tuto „živou“ kolekci
        private readonly ObservableCollection<Student> _students = new ObservableCollection<Student>();

        // 3) Pohled nad kolekcí – kvůli řazení
        private ICollectionView _studentsView;

        public MainWindow()
        {
            InitializeComponent();

            // a) Založ DB a prvních 10 záznamů (pokud je prázdná)
            _db.EnsureCreatedAndSeed();

            // b) Naplň ObservableCollection z databáze, seřaď podle Id
            foreach (var s in _db.Students.OrderBy(x => x.Id).ToList())
                _students.Add(s);

            // c) Pohled nad kolekcí – drží řazení a filtr (zde jen řazení)
            _studentsView = CollectionViewSource.GetDefaultView(_students);
            _studentsView.SortDescriptions.Clear();
            _studentsView.SortDescriptions.Add(new SortDescription(nameof(Student.Id), ListSortDirection.Ascending));

            // d) Připoj do DataGridu
            StudentsGrid.ItemsSource = _studentsView;
        }

        // Přidání studenta z formuláře (s jednoduchými validacemi z Úkolu 3)
        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            // 1) Načti vstupy
            string first = (TxtFirstName.Text ?? string.Empty).Trim();
            string last  = (TxtLastName.Text  ?? string.Empty).Trim();
            string email = (TxtEmail.Text     ?? string.Empty).Trim();
            string yearText = (TxtYear.Text ?? string.Empty).Trim();

            // 2) Povinná pole
            if (string.IsNullOrWhiteSpace(first) ||
                string.IsNullOrWhiteSpace(last)  ||
                string.IsNullOrWhiteSpace(email))
            {
                MessageBox.Show("Vyplň Jméno, Příjmení i E‑mail.", "Upozornění",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            // 3) Ročník 1–6
            if (!int.TryParse(yearText, out int year) || year < 1 || year > 6)
            {
                MessageBox.Show("Ročník zadej jako celé číslo v intervalu 1–6.", "Upozornění",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            // 4) Vytvoř entitu
            var s = new Student
            {
                FirstName = first,
                LastName  = last,
                Year      = year,
                Email     = email,
                CreatedAt = DateTime.UtcNow
            };

            // 5) Přidej do kontextu a ulož (vygeneruje Id)
            _db.Students.Add(s);
            try
            {
                _db.SaveChanges();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Uložení do databáze se nezdařilo.\n\n" + ex, "Chyba",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // 6) Přidej i do kolekce pro UI (bez zbytečného reloadu celé tabulky)
            _students.Add(s);

            // 7) Vyber nového a skoč na něj
            StudentsGrid.SelectedItem = s;
            StudentsGrid.ScrollIntoView(s);

            // 8) Vyčisti formulář
            TxtId.Text = string.Empty;
            TxtFirstName.Text = string.Empty;
            TxtLastName.Text = string.Empty;
            TxtYear.Text = string.Empty;
            TxtEmail.Text = string.Empty;

            MessageBox.Show("Student byl přidán.", "Hotovo", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        // Uložit rozeditované změny z tabulky (💾)
        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Ujisti se, že se commitly právě editované buňky/řádky
                StudentsGrid.CommitEdit(DataGridEditingUnit.Cell, true);
                StudentsGrid.CommitEdit(DataGridEditingUnit.Row, true);

                // Nic se nezměnilo? Není co ukládat.
                if (!_db.ChangeTracker.HasChanges())
                {
                    MessageBox.Show("Žádné změny k uložení.", "Informace",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                // Zapiš změny do DB
                _db.SaveChanges();
                MessageBox.Show("Změny byly uloženy.", "Hotovo",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Nepodařilo se uložit změny.\n\n" + ex, "Chyba",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Smazat vybraného studenta
        private void BtnDeleteSelected_Click(object sender, RoutedEventArgs e)
        {
            var selected = StudentsGrid.SelectedItem as Student;
            if (selected == null)
            {
                MessageBox.Show("Nejprve vyber studenta v tabulce.", "Upozornění",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var answer = MessageBox.Show(
                "Opravdu smazat vybraného studenta?",
                "Potvrzení",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (answer != MessageBoxResult.Yes)
                return;

            // 1) Odeber z kontextu, ulož do DB
            _db.Students.Remove(selected);
            try
            {
                _db.SaveChanges();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Smazání se nepodařilo.\n\n" + ex, "Chyba",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // 2) Odeber z kolekce UI
            _students.Remove(selected);
        }

        // Ukliď kontext při zavření okna
        protected override void OnClosed(EventArgs e)
        {
            _db.Dispose();
            base.OnClosed(e);
        }
    }
}
```

### ✅ Stav po Úkolu 4
- V tabulce lze **přímo upravovat** buňky (TwoWay binding).
- **Smazat vybraného** smaže řádek v DB i v UI.
- **💾 Uložit data** zapíše všechny rozeditované změny.

---

## Jak si **vizuálně** otevřít databázi a podívat se do tabulky
1. Menu **View** → **SQL Server Object Explorer**.
2. Rozbalte **(localdb)\MSSQLLocalDB → Databases → StudentDbDemo → Tables**.
3. Pravým na **dbo.Students** → **View Data** (nebo **New Query** a `SELECT * FROM dbo.Students`).

---

## Nejčastější zádrhely (rychlá pomoc)
- **Nelze přepsat .exe / file locked** při buildu: aplikace běží na pozadí. Zavřete okno, případně v **Task Manageru** ukončete proces s názvem vašeho projektu (např. *DatabazeVipis.exe*). Pak **Build → Rebuild**.
- **AsNoTracking neexistuje**: chybí `using Microsoft.EntityFrameworkCore;` v souboru, kde řetězíte LINQ nad `DbSet<T>`.
- **Fody „nic nedělá“**: zkontrolujte, že existuje soubor `FodyWeavers.xml` v kořeni projektu a obsahuje `<PropertyChanged/>`.

---

## Kde studenti **smí** upravovat kód
- `Data/Student.cs` – vlastnosti entity (sloupce tabulky)
- `Data/StudentContext.cs` – připojení na DB + seed
- `MainWindow.xaml` – UI (tabulka, formulář, tlačítka)
- `MainWindow.xaml.cs` – načítání dat, přidání, validace, mazání, uložení

> Vše ostatní (soubor projektu, App.xaml, atd.) nechte beze změn.

---

## Kompletní finální kód (stav po Úkolu 4)
> Pro jistotu na jednom místě – obsah jednotlivých souborů by měl odpovídat uvedenému.

- `Data/Student.cs` – viz výše (bez změn od Úkolu 1)
- `Data/StudentContext.cs` – viz výše (bez změn)
- `MainWindow.xaml` – celý soubor viz Úkol 4
- `MainWindow.xaml.cs` – celý soubor viz Úkol 4

---

*Hotovo. Ať se daří při práci s WPF a EF Core!*

