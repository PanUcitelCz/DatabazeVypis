# Praktický průvodce: Git & GitHub ve Visual Studio 2022 + .NET 9 WPF cvičení (Student DB)

> **Prostředí:** Visual Studio 2022 (ENG UI), .NET 9, WPF, EF Core 9, SQL Server LocalDB  
> **Repo názvy:** bez mezer a diakritiky (`StudentDbDemo`, nikoli `Studenti – cvičení`)

Inspirace a návaznost na původní kurz k databázovým aplikacím (Entity Framework + WPF). fileciteturn0file0

---

## 1) Základní pojmy Git/GitHub (stručný slovník na jednu stránku)

| Pojem | K čemu je | Jak to vypadá v praxi |
|---|---|---|
| **Repository (repo)** | Úložiště verzí kódu a souborů. | Vznikne na GitHubu nebo v **Git > Create Git Repository** ve VS. |
| **Commit** | „Snímek“ změn v čase. Ukládá, **co** se změnilo a **proč**. | V **Git Changes** napíšete zprávu (např. *Add Student model*), kliknete **Commit**. |
| **Commit message** | Stručný důvod změny. | Pravidla: 1) 50 znaků max v 1. řádku, 2) **rozkazovací způsob** (*Add*, *Fix*, *Refactor*), 3) případně další řádky s detaily. |
| **Remote** | Vzdálené repo (např. GitHub). | V **Git > Settings > Repository Settings** se jmenuje obvykle `origin`. |
| **Fetch** | Stáhne **jen informace** o nových commitech z remotu, kód **nepřepíše**. | **Git > Fetch** (nebo v **Git Changes** rozbalit **Fetch**). |
| **Pull** | = **Fetch + Merge** (stáhne změny a rovnou je aplikuje do vaší větve). | **Git > Pull**. Používejte před prací, ať pracujete na aktuální verzi. |
| **Push** | Nahraje vaše commity do remotu (GitHubu). | **Git > Push** nebo tlačítko **Push** v **Git Changes**. |
| **Branch** | „Větev“ vývoje pro změny odděleně od `main`. | **Git > New Branch...** (např. `feature/add-student-form`), **Checkout**. |
| **Merge** | Sloučení změn z jedné větve do druhé. | Přepněte na cílovou větev (např. `main`) → **Git > Merge...** → zvolte zdrojovou větev. |
| **Conflict** | Stejný řádek upraven v různých větvích. | VS otevře **Merge Editor** – vyberete „Current/Incoming/Both“, uložíte, commitnete. |

> **Přihlášení k GitHubu:** Pokud nejste přihlášení, VS vás samo vyzve u **Create Git Repository**, **Push**, **Clone** apod.

---

## 2) Git v praxi ve Visual Studio 2022 (ENG UI)

### A) Vytvoření repozitáře a první push
1. **File > New > Project…** → šablona **WPF App (.NET)** → **Framework:** `.NET 9.0` → **Create**.  
2. Otevřete **Git > Create Git Repository…**  
   - **Repository name:** bez diakritiky (např. `StudentDbDemo`).  
   - Provider **GitHub**, **Add a README** (volitelné), **Create and Push**.  
3. VS vás případně vyzve k přihlášení do GitHubu – potvrďte.

### B) Běžný rytmus práce
- **Než začnete:** **Git > Pull** (mít aktuální kód).
- **Práce na úkolu:** **Git > New Branch…** (např. `feature/validation`) → **Create and Checkout**.
- **Průběžné ukládání změn:** v **Git Changes** napište stručný **Commit message** → **Commit** (nebo **Commit All**).  
- **Sdílení do GitHubu:** **Push**.
- **Dokončeno?** Přepněte na `main` → **Git > Merge…** → vyberte svou větev → **Merge** → **Push**.
- **Aktualizace z GitHubu:** **Fetch** jen zkontroluje nové commity; **Pull** je stáhne a aplikuje.

### C) Načtení změn z jiného PC
- **Git > Pull** (stáhne a aplikuje změny), případně nejdřív **Fetch** (jen zkontroluje, co je nového).

### D) Otevření projektu a složky v Průzkumníku
- **Solution Explorer** → pravý klik na projekt/solution → **Open Folder in File Explorer**.

---

## 3) .NET 9 WPF cvičení – „Student DB“ (4 navazující úkoly)

Budeme pracovat **jen ve 4 souborech** (a jedné složce):  
`Data/Student.cs`, `Data/StudentContext.cs`, `MainWindow.xaml`, `MainWindow.xaml.cs`

> #### NuGet balíčky (nainstalujte přes *Project > Manage NuGet Packages…*):
> - `Microsoft.EntityFrameworkCore.SqlServer` (verze 9.x)
> - `Microsoft.EntityFrameworkCore.Tools` (verze 9.x)
> - `PropertyChanged.Fody` (doplní INotifyPropertyChanged bez psaní kódu)  
>   + přidejte soubor **FodyWeavers.xml** do kořene projektu s obsahem:
>   ```xml
>   <?xml version="1.0" encoding="utf-8"?>
>   <Weavers>
>     <PropertyChanged />
>   </Weavers>
>   ```

> #### Target framework a WPF v projektu (`.csproj` by mělo obsahovat alespoň):
> ```xml
> <Project Sdk="Microsoft.NET.Sdk">
>   <PropertyGroup>
>     <OutputType>WinExe</OutputType>
>     <TargetFramework>net9.0-windows</TargetFramework>
>     <UseWPF>true</UseWPF>
>     <Nullable>enable</Nullable>
>     <ImplicitUsings>enable</ImplicitUsings>
>   </PropertyGroup>
> </Project>
> ```

### Struktura projektu (doporučeno)
```
YourSolution/
└─ YourProject/
   ├─ Data/
   │  ├─ Student.cs
   │  └─ StudentContext.cs
   ├─ MainWindow.xaml
   └─ MainWindow.xaml.cs
```

---

### ÚKOL 1 — Založení DB, seed 10 záznamů a **pouze zobrazení** v tabulce

#### 1.1 Vytvořte složku `Data` a soubor `Student.cs`

**Bez komentářů:**
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

**S komentáři:**
```csharp
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using PropertyChanged; // NuGet PropertyChanged.Fody – vygeneruje INotifyPropertyChanged

namespace DatabazeVipis.Data
{
    // Díky této anotaci nemusíme ručně implementovat INotifyPropertyChanged.
    [AddINotifyPropertyChangedInterface]
    public class Student
    {
        // Primární klíč (PK) – auto-increment (IDENTITY)
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        // Pár jednoduchých sloupců s validacemi přes DataAnnotations
        [Required, StringLength(100)]
        public string FirstName { get; set; } = string.Empty;

        [Required, StringLength(100)]
        public string LastName { get; set; } = string.Empty;

        [Range(1, 6)] // 1..6 (ročník)
        public int Year { get; set; }

        [StringLength(200)]
        public string Email { get; set; } = string.Empty;

        // Výchozí hodnota v UTC – praktické pro srovnání napříč počítači
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
```

#### 1.2 Soubor `StudentContext.cs` (EF Core context + seed 10 záznamů)

**Bez komentářů:**
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

**S komentáři:**
```csharp
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;

namespace DatabazeVipis.Data
{
    // EF Core DbContext – přístup k DB a tabulkám přes DbSet<T>
    public class StudentContext : DbContext
    {
        public DbSet<Student> Students => Set<Student>();

        // Připojení na LocalDB (součást VS). DB se vytvoří automaticky při prvním běhu.
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

        // Pomocné metody pro první běh
        public void EnsureCreatedAndSeed()
        {
            Database.EnsureCreated(); // vytvoří databázi, pokud neexistuje (bez migrací)
            SeedIfEmpty();            // naplní tabulku ukázkovými daty
        }

        public void SeedIfEmpty()
        {
            if (!Students.Any())
            {
                var initial = new List<Student>
                {
                    // 10 záznamů pro rychlý start – vše čistě pro demo
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

#### 1.3 `MainWindow.xaml` – jednoduchý DataGrid (jen zobrazení)

**Bez komentářů:**
```xml
<Window x:Class="DatabazeVipis.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        Title="Studenti" Height="600" Width="900">
    <Grid Margin="12">
        <DataGrid x:Name="StudentsGrid"
                  AutoGenerateColumns="False"
                  IsReadOnly="True"
                  CanUserAddRows="False"
                  CanUserDeleteRows="False">
            <DataGrid.Columns>
                <DataGridTextColumn Header="ID"        Binding="{Binding Id}"        Width="80"/>
                <DataGridTextColumn Header="Jméno"     Binding="{Binding FirstName}" Width="*"/>
                <DataGridTextColumn Header="Příjmení"  Binding="{Binding LastName}"  Width="*"/>
                <DataGridTextColumn Header="Ročník"    Binding="{Binding Year}"      Width="100"/>
                <DataGridTextColumn Header="E-mail"    Binding="{Binding Email}"     Width="2*"/>
                <DataGridTextColumn Header="Vytvořeno" Binding="{Binding CreatedAt}" Width="180"/>
            </DataGrid.Columns>
        </DataGrid>
    </Grid>
</Window>
```

**S komentáři (co je co):**
```xml
<Window x:Class="DatabazeVipis.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        Title="Studenti" Height="600" Width="900">
    <!-- Grid = základní „mřížka“ pro layout -->
    <Grid Margin="12">
        <!-- DataGrid = tabulka s daty -->
        <DataGrid x:Name="StudentsGrid"
                  AutoGenerateColumns="False"  <!-- sloupce definujeme ručně -->
                  IsReadOnly="True"            <!-- jen čtení (Úkol 1: pouze zobrazit) -->
                  CanUserAddRows="False"
                  CanUserDeleteRows="False">
            <!-- Sloupce a jejich propojení na property ve Student -->
            <DataGrid.Columns>
                <DataGridTextColumn Header="ID"        Binding="{Binding Id}"        Width="80"/>
                <DataGridTextColumn Header="Jméno"     Binding="{Binding FirstName}" Width="*"/>
                <DataGridTextColumn Header="Příjmení"  Binding="{Binding LastName}"  Width="*"/>
                <DataGridTextColumn Header="Ročník"    Binding="{Binding Year}"      Width="100"/>
                <DataGridTextColumn Header="E-mail"    Binding="{Binding Email}"     Width="2*"/>
                <DataGridTextColumn Header="Vytvořeno" Binding="{Binding CreatedAt}" Width="180"/>
            </DataGrid.Columns>
        </DataGrid>
    </Grid>
</Window>
```

#### 1.4 `MainWindow.xaml.cs` – načtení a zobrazení dat
**Bez komentářů:**
```csharp
using DatabazeVipis.Data;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;

namespace DatabazeVipis
{
    public partial class MainWindow : Window
    {
        private readonly StudentContext _db = new StudentContext();
        private readonly ObservableCollection<Student> _students = new ObservableCollection<Student>();

        public MainWindow()
        {
            InitializeComponent();

            _db.EnsureCreatedAndSeed();

            foreach (var s in _db.Students.AsNoTracking().OrderBy(x => x.Id).ToList())
            {
                _students.Add(s);
            }
            StudentsGrid.ItemsSource = _students;
        }
    }
}
```

**S komentáři:**
```csharp
using DatabazeVipis.Data;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;

namespace DatabazeVipis
{
    public partial class MainWindow : Window
    {
        // Dlouhožijící EF kontext a kolekce pro DataGrid
        private readonly StudentContext _db = new StudentContext();
        private readonly ObservableCollection<Student> _students = new ObservableCollection<Student>();

        public MainWindow()
        {
            InitializeComponent();

            // 1) Založ DB (pokud není) a naplň demo daty
            _db.EnsureCreatedAndSeed();

            // 2) Načti data pouze pro čtení (Úkol 1 – jen zobrazit)
            foreach (var s in _db.Students.AsNoTracking().OrderBy(x => x.Id).ToList())
            {
                _students.Add(s);
            }

            // 3) Napoj DataGrid
            StudentsGrid.ItemsSource = _students;
        }
    }
}
```

> **Spuštění:** **Debug > Start Without Debugging** (Ctrl+F5) – měla by se zobrazit tabulka s 10 studenty.

---

### ÚKOL 2 — Přidání záznamu přes formulář (bez validací)

Rozšíříme okno o jednoduchý formulář (TextBoxy + tlačítko **Add student**). **ID zadávat nebudeme** – generuje se samo.

#### 2.1 `MainWindow.xaml` – přidáme formulář
**Bez komentářů:**
```xml
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
      <DataGridTextColumn Header="ID"        Binding="{Binding Id}"        Width="80"/>
      <DataGridTextColumn Header="Jméno"     Binding="{Binding FirstName}" Width="*"/>
      <DataGridTextColumn Header="Příjmení"  Binding="{Binding LastName}"  Width="*"/>
      <DataGridTextColumn Header="Ročník"    Binding="{Binding Year}"      Width="100"/>
      <DataGridTextColumn Header="E-mail"    Binding="{Binding Email}"     Width="2*"/>
      <DataGridTextColumn Header="Vytvořeno" Binding="{Binding CreatedAt}" Width="180"/>
    </DataGrid.Columns>
  </DataGrid>

  <GroupBox Grid.Row="1" Header="Add new student" Margin="0,10,0,0">
    <Grid Margin="10">
      <Grid.ColumnDefinitions>
        <ColumnDefinition Width="Auto"/>
        <ColumnDefinition Width="2*"/>
        <ColumnDefinition Width="Auto"/>
        <ColumnDefinition Width="2*"/>
        <ColumnDefinition Width="Auto"/>
      </Grid.ColumnDefinitions>

      <TextBlock Grid.Column="0" Text="First name:" VerticalAlignment="Center" Margin="0,0,8,0"/>
      <TextBox  x:Name="TxtFirstName" Grid.Column="1"/>

      <TextBlock Grid.Column="2" Text="Last name:" VerticalAlignment="Center" Margin="16,0,8,0"/>
      <TextBox  x:Name="TxtLastName" Grid.Column="3"/>

      <TextBlock Grid.Column="0" Margin="0,8,8,0" Grid.Row="1" Text="Year (1-6):" VerticalAlignment="Center"/>
      <TextBox  x:Name="TxtYear" Grid.Column="1" Grid.Row="1" Margin="0,8,0,0"/>

      <TextBlock Grid.Column="2" Margin="16,8,8,0" Grid.Row="1" Text="Email:" VerticalAlignment="Center"/>
      <TextBox  x:Name="TxtEmail" Grid.Column="3" Grid.Row="1" Margin="0,8,0,0"/>

      <Button x:Name="BtnAddStudent"
              Grid.Column="4"
              Grid.RowSpan="2"
              Content="Add student"
              Padding="16,6"
              Click="BtnAddStudent_Click"/>
    </Grid>
  </GroupBox>
</Grid>
```

**S komentáři:**
```xml
<Grid Margin="12">
  <!-- 2 řádky: nahoře grid s daty, dole formulář -->
  <Grid.RowDefinitions>
    <RowDefinition Height="2*"/>
    <RowDefinition Height="Auto"/>
  </Grid.RowDefinitions>

  <!-- Tabulka dat -->
  <DataGrid x:Name="StudentsGrid"
            Grid.Row="0"
            AutoGenerateColumns="False"
            IsReadOnly="True"            <!-- Úkol 2: stále jen čtení -->
            CanUserAddRows="False"
            CanUserDeleteRows="False">

    <!-- (stejné sloupce jako v Úkolu 1) -->
    <DataGrid.Columns>
      <DataGridTextColumn Header="ID"        Binding="{Binding Id}"        Width="80"/>
      <DataGridTextColumn Header="Jméno"     Binding="{Binding FirstName}" Width="*"/>
      <DataGridTextColumn Header="Příjmení"  Binding="{Binding LastName}"  Width="*"/>
      <DataGridTextColumn Header="Ročník"    Binding="{Binding Year}"      Width="100"/>
      <DataGridTextColumn Header="E-mail"    Binding="{Binding Email}"     Width="2*"/>
      <DataGridTextColumn Header="Vytvořeno" Binding="{Binding CreatedAt}" Width="180"/>
    </DataGrid.Columns>
  </DataGrid>

  <!-- Jednoduchý formulář bez validací -->
  <GroupBox Grid.Row="1" Header="Add new student" Margin="0,10,0,0">
    <Grid Margin="10">
      <Grid.ColumnDefinitions>
        <ColumnDefinition Width="Auto"/>
        <ColumnDefinition Width="2*"/>
        <ColumnDefinition Width="Auto"/>
        <ColumnDefinition Width="2*"/>
        <ColumnDefinition Width="Auto"/>
      </Grid.ColumnDefinitions>

      <TextBlock Grid.Column="0" Text="First name:" VerticalAlignment="Center" Margin="0,0,8,0"/>
      <TextBox  x:Name="TxtFirstName" Grid.Column="1"/>

      <TextBlock Grid.Column="2" Text="Last name:" VerticalAlignment="Center" Margin="16,0,8,0"/>
      <TextBox  x:Name="TxtLastName" Grid.Column="3"/>

      <TextBlock Grid.Column="0" Margin="0,8,8,0" Grid.Row="1" Text="Year (1-6):" VerticalAlignment="Center"/>
      <TextBox  x:Name="TxtYear" Grid.Column="1" Grid.Row="1" Margin="0,8,0,0"/>

      <TextBlock Grid.Column="2" Margin="16,8,8,0" Grid.Row="1" Text="Email:" VerticalAlignment="Center"/>
      <TextBox  x:Name="TxtEmail" Grid.Column="3" Grid.Row="1" Margin="0,8,0,0"/>

      <!-- Tlačítko, které obsloužíme v code-behind -->
      <Button x:Name="BtnAddStudent"
              Grid.Column="4"
              Grid.RowSpan="2"
              Content="Add student"
              Padding="16,6"
              Click="BtnAddStudent_Click"/>
    </Grid>
  </GroupBox>
</Grid>
```

#### 2.2 `MainWindow.xaml.cs` – obsluha přidání (bez validací)
**Bez komentářů (přidejte do class MainWindow):**
```csharp
private void BtnAddStudent_Click(object sender, RoutedEventArgs e)
{
    string firstName = (TxtFirstName.Text ?? string.Empty).Trim();
    string lastName  = (TxtLastName.Text  ?? string.Empty).Trim();
    string email     = (TxtEmail.Text     ?? string.Empty).Trim();
    int year = 0;
    int.TryParse((TxtYear.Text ?? string.Empty).Trim(), out year);

    var s = new Student
    {
        FirstName = firstName,
        LastName  = lastName,
        Year      = year,
        Email     = email,
        CreatedAt = DateTime.UtcNow
    };

    _db.Students.Add(s);
    _db.SaveChanges();           // vygeneruje nové Id
    _students.Add(s);            // přidej i do kolekce pro UI

    StudentsGrid.SelectedItem = s;
    StudentsGrid.ScrollIntoView(s);

    TxtFirstName.Text = string.Empty;
    TxtLastName.Text  = string.Empty;
    TxtYear.Text      = string.Empty;
    TxtEmail.Text     = string.Empty;
}
```

**S komentáři:**
```csharp
private void BtnAddStudent_Click(object sender, RoutedEventArgs e)
{
    // 1) Seber hodnoty z textboxů (zatím bez validací)
    string firstName = (TxtFirstName.Text ?? string.Empty).Trim();
    string lastName  = (TxtLastName.Text  ?? string.Empty).Trim();
    string email     = (TxtEmail.Text     ?? string.Empty).Trim();
    int year = 0;
    int.TryParse((TxtYear.Text ?? string.Empty).Trim(), out year);

    // 2) Sestav entitu a ulož do DB
    var s = new Student
    {
        FirstName = firstName,
        LastName  = lastName,
        Year      = year,       // zatím bez kontrol
        Email     = email,
        CreatedAt = DateTime.UtcNow
    };

    _db.Students.Add(s);
    _db.SaveChanges();           // zde se vygeneruje nové Id (IDENTITY)

    // 3) Přidej i do kolekce pro UI (aby se hned zobrazil)
    _students.Add(s);

    // 4) QoL – vyber a sjeď na nového studenta + vyčisti formulář
    StudentsGrid.SelectedItem = s;
    StudentsGrid.ScrollIntoView(s);

    TxtFirstName.Text = string.Empty;
    TxtLastName.Text  = string.Empty;
    TxtYear.Text      = string.Empty;
    TxtEmail.Text     = string.Empty;
}
```

---

### ÚKOL 3 — Základní validace vstupu (před uložením)

Doplníme kontroly: *Jméno* a *Příjmení* povinné, *Year* musí být číslo 1–6. (E-mail neřešíme formálně.)

**Nahraďte handler z Úkolu 2:**
```csharp
private void BtnAddStudent_Click(object sender, RoutedEventArgs e)
{
    string firstName = (TxtFirstName.Text ?? string.Empty).Trim();
    string lastName  = (TxtLastName.Text  ?? string.Empty).Trim();
    string email     = (TxtEmail.Text     ?? string.Empty).Trim();
    string yearText  = (TxtYear.Text      ?? string.Empty).Trim();

    if (string.IsNullOrWhiteSpace(firstName) || string.IsNullOrWhiteSpace(lastName))
    {
        MessageBox.Show("First name and Last name are required.", "Info",
            MessageBoxButton.OK, MessageBoxImage.Information);
        return;
    }

    int year;
    bool parsed = int.TryParse(yearText, out year);
    if (parsed == false || year < 1 || year > 6)
    {
        MessageBox.Show("Year must be an integer in range 1–6.", "Info",
            MessageBoxButton.OK, MessageBoxImage.Information);
        return;
    }

    var s = new Student
    {
        FirstName = firstName,
        LastName  = lastName,
        Year      = year,
        Email     = email,
        CreatedAt = DateTime.UtcNow
    };

    _db.Students.Add(s);
    _db.SaveChanges();
    _students.Add(s);

    StudentsGrid.SelectedItem = s;
    StudentsGrid.ScrollIntoView(s);

    TxtFirstName.Text = string.Empty;
    TxtLastName.Text  = string.Empty;
    TxtYear.Text      = string.Empty;
    TxtEmail.Text     = string.Empty;
}
```

---

### ÚKOL 4 — Úpravy přímo v tabulce + Uložit (💾) + Smazat vybraného

- Povolit úpravy v **DataGridu** (TwoWay binding uvnitř DataGridu dělá WPF za nás).  
- Přidat **Save** a **Delete** tlačítka. Uložení použije `_db.SaveChanges()`; smazání smaže v DB i v `_students`.

#### 4.1 `MainWindow.xaml` – povolíme editaci a přidáme tlačítka
**Bez komentářů:**
```xml
<Grid Margin="12">
  <Grid.RowDefinitions>
    <RowDefinition Height="Auto"/>
    <RowDefinition Height="*"/>
    <RowDefinition Height="Auto"/>
  </Grid.RowDefinitions>

  <StackPanel Orientation="Horizontal" Grid.Row="0" Margin="0,0,0,8">
    <Button x:Name="BtnSave"   Content="💾 Save changes" Padding="12,6" Click="BtnSave_Click"  Margin="0,0,8,0"/>
    <Button x:Name="BtnDelete" Content="Delete selected" Padding="12,6" Click="BtnDelete_Click"/>
  </StackPanel>

  <DataGrid x:Name="StudentsGrid"
            Grid.Row="1"
            AutoGenerateColumns="False"
            IsReadOnly="False"
            CanUserAddRows="False"
            CanUserDeleteRows="False">
    <DataGrid.Columns>
      <DataGridTextColumn Header="ID"        Binding="{Binding Id}"        IsReadOnly="True" Width="80"/>
      <DataGridTextColumn Header="Jméno"     Binding="{Binding FirstName, Mode=TwoWay, UpdateSourceTrigger=PropertyChanged}" Width="*"/>
      <DataGridTextColumn Header="Příjmení"  Binding="{Binding LastName,  Mode=TwoWay, UpdateSourceTrigger=PropertyChanged}" Width="*"/>
      <DataGridTextColumn Header="Ročník"    Binding="{Binding Year,      Mode=TwoWay, UpdateSourceTrigger=PropertyChanged}" Width="100"/>
      <DataGridTextColumn Header="E-mail"    Binding="{Binding Email,     Mode=TwoWay, UpdateSourceTrigger=PropertyChanged}" Width="2*"/>
      <DataGridTextColumn Header="Vytvořeno" Binding="{Binding CreatedAt}" IsReadOnly="True" Width="180"/>
    </DataGrid.Columns>
  </DataGrid>

  <!-- Ponecháme spodní formulář z Úkolu 2/3 – volitelné -->
  <GroupBox Grid.Row="2" Header="Add new student" Margin="0,10,0,0">
    <!-- ... (stejný obsah jako dřív) ... -->
  </GroupBox>
</Grid>
```

**S komentáři:**
```xml
<Grid Margin="12">
  <!-- 3 řádky: toolbar, grid s daty, formulář -->
  <Grid.RowDefinitions>
    <RowDefinition Height="Auto"/>
    <RowDefinition Height="*"/>
    <RowDefinition Height="Auto"/>
  </Grid.RowDefinitions>

  <!-- Horní tlačítka -->
  <StackPanel Orientation="Horizontal" Grid.Row="0" Margin="0,0,0,8">
    <Button x:Name="BtnSave"   Content="💾 Save changes" Padding="12,6" Click="BtnSave_Click"  Margin="0,0,8,0"/>
    <Button x:Name="BtnDelete" Content="Delete selected" Padding="12,6" Click="BtnDelete_Click"/>
  </StackPanel>

  <!-- DataGrid s povolenou editací (IsReadOnly=False) -->
  <DataGrid x:Name="StudentsGrid"
            Grid.Row="1"
            AutoGenerateColumns="False"
            IsReadOnly="False"               <!-- důležité -->
            CanUserAddRows="False"
            CanUserDeleteRows="False">
    <DataGrid.Columns>
      <DataGridTextColumn Header="ID"        Binding="{Binding Id}"        IsReadOnly="True" Width="80"/>
      <!-- TwoWay binding + UpdateSourceTrigger = průběžné ukládání do objektu Student -->
      <DataGridTextColumn Header="Jméno"     Binding="{Binding FirstName, Mode=TwoWay, UpdateSourceTrigger=PropertyChanged}" Width="*"/>
      <DataGridTextColumn Header="Příjmení"  Binding="{Binding LastName,  Mode=TwoWay, UpdateSourceTrigger=PropertyChanged}" Width="*"/>
      <DataGridTextColumn Header="Ročník"    Binding="{Binding Year,      Mode=TwoWay, UpdateSourceTrigger=PropertyChanged}" Width="100"/>
      <DataGridTextColumn Header="E-mail"    Binding="{Binding Email,     Mode=TwoWay, UpdateSourceTrigger=PropertyChanged}" Width="2*"/>
      <DataGridTextColumn Header="Vytvořeno" Binding="{Binding CreatedAt}" IsReadOnly="True" Width="180"/>
    </DataGrid.Columns>
  </DataGrid>

  <!-- Volitelně necháme spodní formulář z Úkolu 2/3 -->
  <GroupBox Grid.Row="2" Header="Add new student" Margin="0,10,0,0">
    <!-- ... -->
  </GroupBox>
</Grid>
```

#### 4.2 `MainWindow.xaml.cs` – Save + Delete
**Bez komentářů:**
```csharp
private void BtnSave_Click(object sender, RoutedEventArgs e)
{
    try
    {
        StudentsGrid.CommitEdit(DataGridEditingUnit.Cell, true);
        StudentsGrid.CommitEdit(DataGridEditingUnit.Row,  true);

        if (_db.ChangeTracker.HasChanges())
        {
            _db.SaveChanges();
            MessageBox.Show("Changes saved.", "OK", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        else
        {
            MessageBox.Show("No changes to save.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
    catch (Exception ex)
    {
        MessageBox.Show("Save failed:\n\n" + ex, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
    }
}

private void BtnDelete_Click(object sender, RoutedEventArgs e)
{
    var selected = StudentsGrid.SelectedItem as Student;
    if (selected == null)
    {
        MessageBox.Show("Please select a student first.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
        return;
    }

    if (MessageBox.Show("Delete selected student?", "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
        return;

    _db.Students.Remove(selected);
    try
    {
        _db.SaveChanges();
        _students.Remove(selected);
    }
    catch (Exception ex)
    {
        MessageBox.Show("Delete failed:\n\n" + ex, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
    }
}
```

**S komentáři:**
```csharp
private void BtnSave_Click(object sender, RoutedEventArgs e)
{
    try
    {
        // Pro jistotu commit rozeditované buňky/řádku
        StudentsGrid.CommitEdit(DataGridEditingUnit.Cell, true);
        StudentsGrid.CommitEdit(DataGridEditingUnit.Row,  true);

        // Uložit jen když je co
        if (_db.ChangeTracker.HasChanges())
        {
            _db.SaveChanges();
            MessageBox.Show("Changes saved.", "OK", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        else
        {
            MessageBox.Show("No changes to save.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
    catch (Exception ex)
    {
        MessageBox.Show("Save failed:\n\n" + ex, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
    }
}

private void BtnDelete_Click(object sender, RoutedEventArgs e)
{
    // Vybraný řádek v gridu
    var selected = StudentsGrid.SelectedItem as Student;
    if (selected == null)
    {
        MessageBox.Show("Please select a student first.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
        return;
    }

    // Potvrzení smazání
    if (MessageBox.Show("Delete selected student?", "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
        return;

    // 1) Smazat v DB
    _db.Students.Remove(selected);
    try
    {
        _db.SaveChanges();

        // 2) Smazat i v ObservableCollection pro UI
        _students.Remove(selected);
    }
    catch (Exception ex)
    {
        MessageBox.Show("Delete failed:\n\n" + ex, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
    }
}
```

> **Pozn.:** V dalších cvičeních lze ukázat validace skrz DataAnnotations přímo v modelu a vizuální zvýraznění chyb ve WPF, nebo přechod na MVVM.

---

## 4) OOP – bleskové shrnutí (s ukázkami v C#)

**Objekt (instance)** je konkrétní „věc“ vytvořená z **třídy** (šablony).  
**Vlastnost (property)** popisuje stav objektu, **metoda** popisuje chování.  
**Konstruktor** nastaví počáteční stav objektu. **Dědičnost** umožní vytvořit třídu z jiné třídy.  
**Polymorfismus** dovolí volat „stejnou“ metodu různě podle skutečného typu.

```csharp
// Rodičovská třída
public class Vehicle
{
    public string Name { get; set; } = string.Empty;
    public int Year { get; set; }

    public virtual string Kind => "Vehicle";      // polymorfní vlastnost
    public virtual string Start() => $"{Kind} {Name} starts."; // polymorfní metoda
}

// Potomek 1
public class Car : Vehicle
{
    public double EngineLiters { get; set; }
    public override string Kind => "Car";
    public override string Start() => $"{Kind} {Name} starts engine ({EngineLiters} L).";
}

// Potomek 2
public class Bicycle : Vehicle
{
    public bool HasBell { get; set; }
    public override string Kind => "Bicycle";
    public override string Start() => $"{Kind} {Name} moves by pedaling.";
}
```

> **V praxi v našem cvičení:** Třída `Student` je naše „entita“ (model). `StudentContext` je „správce“ přístupu k databázi. V UI (WPF) pracujeme s objekty `Student` v tabulce a formuláři.

---

## 5) Rychlé „klikací“ checklisty (ENG UI)

**Instalace NuGetů:**  
`Project > Manage NuGet Packages… > Browse` → vyhledat **Microsoft.EntityFrameworkCore.SqlServer**, **Microsoft.EntityFrameworkCore.Tools**, **PropertyChanged.Fody** → **Install** do projektu.

**Soubor FodyWeavers.xml:**  
`Solution Explorer > Project (right click) > Add > New Item… > XML File` → název `FodyWeavers.xml` → vložit obsah a uložit.

**Založení složky a souborů:**  
`Solution Explorer > Project (right click) > Add > New Folder` (Data) → pravý klik na Data → **Add > Class…** (`Student.cs`, `StudentContext.cs`).

**Spuštění aplikace:**  
`Debug > Start Without Debugging` (Ctrl+F5).

**Git – vytvoření větve:**  
`Git > New Branch…` → jméno (např. `feature/add-form`) → **Create and Checkout**.

**Git – commit & push:**  
Panel **Git Changes** → popsat změny (commit message) → **Commit** → **Push**.

**Git – fetch vs pull:**  
`Git > Fetch` (jen zjistí nové commity), `Git > Pull` (stáhne a aplikuje).

**Git – merge do main:**  
`Git > Branches` → přepnout na `main` → `Git > Merge…` → vybrat větev → **Merge** → **Push**.

---

## 6) Celé řešení po Úkolu 4 – kde co hledat
- **Model & DB:** `Data/Student.cs`, `Data/StudentContext.cs`  
- **UI (XAML):** `MainWindow.xaml` (Grid, DataGrid, GroupBox, TextBox, Button)  
- **Logika okna:** `MainWindow.xaml.cs` (načtení, přidání, validace, uložení, smazání)

> Tip pro správu kódu ve výuce: Každý „ÚKOL“ dělejte na **samostatné větvi**. Student tak vidí historii kroků (commitů) i rozdíly (Git **Compare with previous**).

---

**Hotovo. Hodně zdaru při výuce!**

