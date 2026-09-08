# 🎮 Game Save Editor

<p align="center">
  <img src="https://img.shields.io/badge/.NET-10.0-512BD4?style=for-the-badge&logo=dotnet&logoColor=white" alt=".NET 10">
  <img src="https://img.shields.io/badge/.NET%20MAUI-Windows-512BD4?style=for-the-badge&logo=dotnet&logoColor=white" alt=".NET MAUI">
  <img src="https://img.shields.io/badge/SQLite-SQLCipher-003B57?style=for-the-badge&logo=sqlite&logoColor=white" alt="SQLite / SQLCipher">
  <img src="https://img.shields.io/badge/Platform-Windows-0078D4?style=for-the-badge&logo=windows&logoColor=white" alt="Windows">
</p>

<p align="center">
  <strong>A modern, extensible desktop database editor for game save files.</strong>
</p>

<p align="center">
  Inspect, search, edit, compare, and safely modify SQLite and SQLCipher-backed game saves through a modern Windows interface.
</p>

<p align="center">
  <a href="#-features">Features</a> •
  <a href="#-screenshots">Screenshots</a> •
  <a href="#-getting-started">Getting Started</a> •
  <a href="#-editing-data">Editing</a> •
  <a href="#-encryption">Encryption</a> •
  <a href="#-building">Building</a> •
  <a href="#-architecture">Architecture</a> •
  <a href="#-roadmap">Roadmap</a>
</p>

---

## 📖 Overview

**Game Save Editor** is a Windows desktop application designed to make inspecting and modifying game save databases easier and safer.

Many games store save data in SQLite databases, while others use encrypted SQLite databases through technologies such as **SQLCipher**.

Instead of requiring users to work directly with SQLite command-line tools or manually manipulate database files, Game Save Editor provides a graphical workflow for:

* 🔍 Discovering database tables
* 📊 Browsing database records
* ✏️ Editing values
* 🧩 Editing multiple cells simultaneously
* 🔎 Searching records
* ↕️ Sorting data
* 📄 Paginating large datasets
* 🗑️ Adding and deleting rows
* ↩️ Undoing and redoing staged changes
* 🔐 Opening SQLCipher databases
* 💾 Creating automatic backups
* 📝 Reviewing changes before saving
* 🔄 Applying changes transactionally
* 🧪 Running read-only SQL queries
* 🔗 Inspecting foreign-key relationships
* 👤 Securely remembering encryption keys per Windows user

The application is designed with **save-data safety** as a priority.

> **Changes are staged first. Nothing is permanently written until you explicitly choose `Save Changes`.**

---

## ✨ Features

### 🗄️ Database Support

| Feature                        | Supported |
| ------------------------------ | :-------: |
| SQLite databases               |     ✅     |
| SQLCipher 4                    |     ✅     |
| SQLCipher 3 configuration      |     ⚙️    |
| Custom SQLCipher configuration |     ⚙️    |
| Automatic database probing     |     ✅     |
| Encrypted database detection   |     ✅     |
| Schema discovery               |     ✅     |
| Table discovery                |     ✅     |
| Column metadata                |     ✅     |
| Primary-key detection          |     ✅     |
| Foreign-key detection          |     ✅     |
| WITHOUT ROWID detection        |     ✅     |

---

### 📊 Modern Table Editor

Browse database tables through a structured, spreadsheet-style interface.

The editor supports:

* Column headers
* Row numbers
* Primary-key indicators
* Foreign-key indicators
* Data-type information
* Search
* Sorting
* Pagination
* Cell selection
* Multi-cell selection
* Bulk editing
* Row insertion
* Row deletion
* Staged modifications

---

## 🖱️ Multi-Cell Selection & Bulk Editing

Phase 7 introduces spreadsheet-style selection.

You can select multiple cells and modify them simultaneously.

### Selection methods

#### Click + Drag

Click a cell and drag across the table to create a rectangular selection.

```text
┌─────────┬─────────┬─────────┬─────────┐
│ ID      │ Name    │ Level   │ Gold    │
├─────────┼─────────┼─────────┼─────────┤
│ 001     │ Knight  │ 10      │ 500     │
│ 002     │ Mage    │ 12      │ 750     │
│ 003     │ Rogue   │ 8       │ 300     │
│ 004     │ Archer  │ 15      │ 900     │
└─────────┴─────────┴─────────┴─────────┘
              ▲──────────────▲
                 selection
```

#### Shift + Click

Select a rectangular range between two cells.

#### Ctrl + Click

Add or remove individual cells from the current selection.

---

### Bulk Editing

After selecting compatible cells, use:

> **Edit Selected**

A type-aware editor allows one value to be applied to every selected cell.

For example:

```text
Selected cells: 37

Column:
    Gold

Type:
    INTEGER

New value:
    999999

[ Cancel ]       [ Apply to 37 Cells ]
```

The operation is staged and can be undone as a single operation.

### Supported bulk-edit types

* `INTEGER`
* `REAL`
* `NUMERIC`
* `DECIMAL`
* `TEXT`
* `BOOLEAN`
* `DATE/TIME`
* `JSON`
* `BLOB`
* `NULL`

Selections containing incompatible types are prevented from being bulk-edited to avoid accidental data conversion.

---

## ✏️ Data Editing

The editor provides type-aware editing rather than treating every database value as plain text.

### INTEGER

```text
42
100
999999
```

### REAL / NUMERIC

```text
1.5
99.95
0.001
```

### BOOLEAN

Boolean values are presented through a dedicated control rather than requiring users to remember SQLite's integer representation.

### TEXT

Regular string editing is supported.

### JSON

JSON values can be:

* Edited
* Formatted
* Validated
* Pretty-printed

### BLOB

Binary data can be represented using Base64.

```text
SGVsbG8gV29ybGQ=
```

### NULL

Database `NULL` values are explicitly supported.

---

# ↩️ Undo & Redo

Editing is staged before being committed.

This allows the user to experiment without immediately modifying the original database.

Supported operations include:

* Cell edits
* Bulk edits
* Row insertion
* Row deletion

For example:

```text
Edit Cell
    ↓
Bulk Edit
    ↓
Delete Row
    ↓
Undo
    ↓
Undo
    ↓
Redo
```

Bulk operations are treated as a single logical operation, making large edits significantly easier to reverse.

---

# 💾 Safe Save System

Game save files are valuable and can be difficult to recover if corrupted.

Game Save Editor therefore uses a staged save workflow.

```text
Open Database
      │
      ▼
Edit Data
      │
      ▼
Changes Staged
      │
      ├───────────────┐
      │               │
      ▼               ▼
Discard           Save Changes
                      │
                      ▼
                  Backup
                      │
                      ▼
                  Transaction
                      │
                 ┌────┴────┐
                 ▼         ▼
               Success   Failure
                 │         │
                 ▼         ▼
              Commit    Rollback
```

### Save process

When `Save Changes` is pressed:

1. A backup is created.
2. A database transaction begins.
3. All staged changes are applied.
4. The transaction is committed if successful.
5. The transaction is rolled back if an error occurs.

This significantly reduces the risk of leaving the database in a partially modified state.

---

# 🗂️ Automatic Backups

Backups are created before an explicit save operation.

Example:

```text
SaveDatabaseExplorer/
└── Backups/
    ├── Save_2026-09-08_120001.db
    ├── Save_2026-09-08_121542.db
    └── Save_2026-09-08_123010.db
```

The application includes a **Backup Manager** for managing previous backups.

### Backup features

* Automatic pre-save backups
* Timestamped backup names
* Configurable backup directory
* Backup retention
* Backup restoration
* WAL/SHM cleanup during restoration

---

# 🔐 Encryption & SQLCipher

Game Save Editor supports encrypted SQLite databases through **SQLCipher**.

The current implementation uses:

```text
Microsoft.Data.Sqlite.Core
        +
SQLitePCLRaw.bundle_e_sqlcipher
```

The primary supported configuration is **SQLCipher 4**.

Typical SQLCipher 4 parameters include:

```text
Cipher:
    AES-256-CBC

KDF:
    PBKDF2-HMAC-SHA512

KDF Iterations:
    256000

HMAC:
    HMAC-SHA512

Page Size:
    4096
```

The application does not assume that every `.db` file is encrypted.

Instead, it first probes the database and determines whether it appears to be:

* Standard SQLite
* Encrypted / non-standard SQLite
* A database requiring a configured encryption profile

---

# 🔑 Secure Key Storage

Encryption keys can optionally be remembered for future sessions.

Keys are **not stored directly inside `profiles.json`**.

Windows DPAPI is used to protect remembered keys:

```text
Windows User Account
        │
        ▼
       DPAPI
        │
        ▼
Encrypted key storage
        │
        ▼
%LOCALAPPDATA%\SaveDatabaseExplorer\keys.dat
```

Keys are protected using:

```text
DataProtectionScope.CurrentUser
```

This means the stored key is tied to the current Windows user account.

The application provides controls to:

* Remember a database key
* Forget an individual key
* Forget all remembered keys

> ⚠️ The application intentionally does not expose or store remembered keys as plaintext configuration values.

---

# 👤 Database Profiles

Profiles allow different games and database configurations to be saved.

A profile can contain:

* Game name
* Database path
* Encryption family
* Page size
* KDF iterations
* KDF algorithm
* HMAC algorithm
* Whether a passphrase is required

Example:

```text
┌─────────────────────────────────────────┐
│ Database Profile                        │
├─────────────────────────────────────────┤
│ Game:        Example Game               │
│ Type:        SQLCipher 4                │
│ Page Size:   4096                       │
│ KDF:         PBKDF2-HMAC-SHA512         │
│ Iterations:  256000                     │
│ HMAC:        HMAC-SHA512                │
│                                         │
│              [ Open Database ]          │
└─────────────────────────────────────────┘
```

Built-in profiles include:

* Plain SQLite
* Solo Leveling
* SQLCipher 4
* SQLCipher 3
* SQLCipher Custom

Built-in profiles are protected from accidental modification and can be duplicated to create custom profiles.

---

# 🧙 Database Setup Wizard

Opening a database uses a three-step workflow.

```text
1. Database
      ↓
2. Profile
      ↓
3. Verify
      ↓
   Database
```

### Step 1 — Database

Select the `.db` file.

### Step 2 — Profile

Choose an existing profile or configure a new one.

### Step 3 — Verify

The application tests the selected configuration before opening the database.

This makes incorrect encryption settings easier to diagnose before entering the editor.

---

# 🔎 Search

The table editor supports searching through loaded records.

Search can be used to quickly locate:

* Player names
* IDs
* Item IDs
* Currency values
* Quest values
* Character statistics
* JSON content
* Other database fields

---

# ↕️ Sorting

Loaded data can be sorted from the table interface.

Sorting is performed on the currently loaded dataset.

> **Note:** Numeric sorting is currently based on the editor's loaded representation and may therefore behave differently from SQLite's native numeric ordering in some mixed-type datasets.

---

# 📄 Pagination

Large tables can contain thousands of records.

To keep the interface responsive, the editor limits the initial loaded dataset to:

```text
5,000 rows
```

The table can then be paginated using:

```text
25
50
100
250
```

rows per page.

Pagination is currently **UI-side pagination over the loaded dataset**, rather than database-side paging.

---

# 🔗 Foreign Keys

The editor detects foreign-key relationships using:

```sql
PRAGMA foreign_key_list(table);
```

Foreign-key metadata can be displayed alongside table information, helping users understand relationships between tables.

Example:

```text
Player
 ├── player_id
 ├── name
 └── inventory_id
          │
          ▼
      Inventory
```

---

# 🧪 SQL Query Console

The application includes a read-only SQL console for inspecting databases.

Supported query categories include:

* `SELECT`
* `PRAGMA`
* `EXPLAIN`

Example:

```sql
SELECT *
FROM Player
LIMIT 20;
```

Or:

```sql
PRAGMA table_info(Player);
```

The query console is intentionally **read-only**.

It does not provide arbitrary `INSERT`, `UPDATE`, `DELETE`, `DROP`, or other destructive SQL execution.

This keeps database modifications within the controlled editor workflow.

---

# 🛡️ Database Safety

Several safeguards are built into the editor.

### Primary keys

Primary-key information is detected and displayed.

### WITHOUT ROWID

Tables using:

```sql
WITHOUT ROWID
```

receive additional protection because they cannot be edited using the same assumptions as ordinary SQLite rowid tables.

### Transactions

Database modifications are applied transactionally.

### Backups

A backup is created before explicit saves.

### Staged changes

Editing does not immediately modify the database on disk.

### Read-only SQL

The SQL console cannot directly execute destructive statements.

---

# 🖥️ Screenshots

> 📸 Add screenshots of the application here.

Recommended screenshots:

### Database Setup

```text
docs/screenshots/database-setup.png
```

### Table Editor

```text
docs/screenshots/table-editor.png
```

### Multi-Cell Editing

```text
docs/screenshots/bulk-edit.png
```

### Database Profiles

```text
docs/screenshots/profiles.png
```

### Backup Manager

```text
docs/screenshots/backups.png
```

### SQL Console

```text
docs/screenshots/sql-console.png
```

Once the screenshots are added to the repository, use:

```markdown
<p align="center">
  <img src="docs/screenshots/table-editor.png" width="900">
</p>
```

---

# 🚀 Getting Started

## Requirements

### Operating System

* Windows 10 version 1809 or later
* Windows 11 recommended
* x64 system

### Development

* Visual Studio 2026
* .NET 10 SDK
* .NET MAUI workload
* Windows SDK
* Windows App SDK

Verify your SDK:

```powershell
dotnet --info
```

Verify installed SDKs:

```powershell
dotnet --list-sdks
```

Verify MAUI:

```powershell
dotnet workload list
```

If necessary:

```powershell
dotnet workload install maui
```

---

# 📥 Running From Source

Clone the repository:

```powershell
git clone https://github.com/YOUR_USERNAME/GameSaveEditor.git
cd GameSaveEditor
```

Restore dependencies:

```powershell
dotnet restore
```

Build:

```powershell
dotnet build -c Release
```

Run the Windows target from Visual Studio or the command line.

---

# 📦 Publishing a Windows Build

The application can be published as a self-contained Windows application.

```powershell
dotnet publish GameSaveEditor.csproj `
    -f net10.0-windows10.0.19041.0 `
    -c Release `
    -p:RuntimeIdentifierOverride=win-x64 `
    -p:WindowsPackageType=None `
    -p:WindowsAppSDKSelfContained=true
```

The resulting files will be located under:

```text
bin/
└── Release/
    └── net10.0-windows10.0.19041.0/
        └── win-x64/
            └── publish/
```

The directory should contain:

```text
GameSaveEditor.exe
...
```

### Important

The **entire `publish` directory** is required.

Do not distribute only:

```text
GameSaveEditor.exe
```

unless the application has specifically been configured for a single-file deployment.

---

# 📦 Creating the Installer

The project uses **Inno Setup** to create a conventional Windows installer.

Install Inno Setup 6 and open:

```text
Installer/
└── GameSaveEditor.iss
```

Then select:

```text
Build
    ↓
Compile
```

The installer will package the published application into:

```text
Build/
└── Installer/
    └── GameSaveEditor-1.0.0-Setup.exe
```

---

# ⚙️ Automated Release Build

The repository includes:

```text
Build/
├── Publish-Windows.ps1
└── Publish-Windows.bat
```

Run:

```powershell
.\Build\Publish-Windows.ps1
```

Or:

```text
Build/Publish-Windows.bat
```

### Skip installer creation

```powershell
.\Build\Publish-Windows.ps1 -SkipInstaller
```

### Clean build

```powershell
.\Build\Publish-Windows.ps1 -Clean
```

### Clean build without installer

```powershell
.\Build\Publish-Windows.ps1 -Clean -SkipInstaller
```

---

# 📁 Project Structure

```text
GameSaveEditor/
│
├── App.xaml
├── App.xaml.cs
├── MauiProgram.cs
├── GameSaveEditor.csproj
│
├── Components/
│   ├── Layout/
│   └── Pages/
│
├── Models/
│   ├── DatabaseColumn.cs
│   ├── DatabaseForeignKey.cs
│   ├── DatabaseProfile.cs
│   ├── DatabaseRow.cs
│   └── DatabaseTable.cs
│
├── Services/
│   ├── AppSettingsService.cs
│   ├── BackupService.cs
│   ├── DatabaseProbeService.cs
│   ├── DatabaseService.cs
│   ├── KeyVaultService.cs
│   └── ProfileService.cs
│
├── Properties/
│   └── PublishProfiles/
│       └── Windows-x64-Unpackaged.pubxml
│
├── Installer/
│   ├── GameSaveEditor.iss
│   └── GameSaveEditor.ico
│
├── Build/
│   ├── Publish-Windows.ps1
│   └── Publish-Windows.bat
│
├── docs/
│   └── screenshots/
│
├── README.md
└── .gitignore
```

---

# 🏗️ Architecture

The application follows a service-oriented architecture.

```text
                    ┌──────────────────────┐
                    │      Blazor UI       │
                    │                      │
                    │  Table Viewer        │
                    │  Database Wizard     │
                    │  Profiles            │
                    │  Backup Manager      │
                    │  Query Console       │
                    └──────────┬───────────┘
                               │
                               ▼
                    ┌──────────────────────┐
                    │      Services        │
                    ├──────────────────────┤
                    │ DatabaseService      │
                    │ BackupService        │
                    │ ProfileService       │
                    │ KeyVaultService      │
                    │ DatabaseProbeService │
                    └──────────┬───────────┘
                               │
                               ▼
                    ┌──────────────────────┐
                    │ SQLite / SQLCipher   │
                    │                      │
                    │ Microsoft.Data.Sqlite│
                    │ SQLCipher Native     │
                    └──────────────────────┘
```

---

# 🔧 Technology Stack

| Technology                     | Purpose                          |
| ------------------------------ | -------------------------------- |
| **C#**                         | Application logic                |
| **.NET 10**                    | Runtime/framework                |
| **.NET MAUI**                  | Native Windows application shell |
| **Blazor Hybrid**              | UI layer                         |
| **Razor**                      | UI components                    |
| **Microsoft.Data.Sqlite.Core** | SQLite access                    |
| **SQLitePCLRaw**               | Native SQLite integration        |
| **SQLCipher**                  | Encrypted SQLite support         |
| **Windows DPAPI**              | Secure key storage               |
| **Inno Setup**                 | Windows installer                |
| **Git**                        | Version control                  |

---

# 🔐 Security Considerations

Game Save Editor is designed primarily as a **local desktop utility**.

The application does not require a cloud service for database editing.

Remembered encryption keys are protected using Windows DPAPI and stored separately from normal profile configuration.

However:

> **This application should not be considered a secure credential manager.**

Users should only use the key-memory feature on trusted Windows accounts and machines.

The application also does not attempt to bypass server-side authentication, online validation, DRM, or other game security systems.

It operates on database files provided by the user.

---

# 💡 Typical Use Cases

Game Save Editor can be useful for:

### 🎮 Game developers

Inspect local save data during development.

### 🧪 QA

Investigate unusual save states and reproduce game-state bugs.

### 🔍 Modding

Inspect locally stored game data where modification is permitted.

### 🛠️ Tools development

Experiment with SQLite-backed game data structures.

### 📊 Data inspection

Quickly inspect relationships and values without opening a database CLI.

---

# 🧭 Roadmap

The project is actively evolving.

Potential future features include:

* [ ] Advanced database-side pagination
* [ ] Native SQLite expression sorting
* [ ] Column visibility controls
* [ ] Column resizing and reordering
* [ ] Freeze columns
* [ ] Advanced filtering
* [ ] Saved table filters
* [ ] Find & Replace
* [ ] Bulk increment/decrement operations
* [ ] Bulk mathematical transformations
* [ ] Copy/paste spreadsheet-style ranges
* [ ] Import/export CSV
* [ ] Import/export JSON
* [ ] Database comparison
* [ ] Diff between two save files
* [ ] Schema visualization
* [ ] Database relationship graph
* [ ] More SQLCipher versions
* [ ] Additional cipher configuration support
* [ ] Plugin architecture for game-specific formats
* [ ] Game-specific editor profiles
* [ ] Custom field editors
* [ ] Automated save validation
* [ ] Advanced backup/version history
* [ ] Portable mode
* [ ] Single-file publishing
* [ ] Automated GitHub Releases
* [ ] CI/CD build pipeline

---

# 🧩 Planned Bulk Editing Improvements

The Phase 2 selection system provides the foundation for more powerful batch operations.

Future operations could include:

```text
Set Value
Set NULL
Increment
Decrement
Multiply
Divide
Add Prefix
Add Suffix
Find & Replace
Convert Type
```

For example:

```text
Gold

Selected:
    100
    250
    500
    750

Operation:
    Multiply

Value:
    2

Result:
    200
    500
    1000
    1500
```

These operations will continue to use the staged-change system so they can be reviewed and undone before saving.

---

# 🐛 Known Limitations

### Loaded row limit

The editor currently loads a maximum of approximately:

```text
5,000 rows
```

at once.

### Pagination

Pagination is currently performed over the loaded dataset rather than directly against SQLite.

### Numeric sorting

Some sorting operations currently operate on loaded string representations and may not perfectly reproduce SQLite's native numeric ordering for mixed-type data.

### SQLCipher versions

SQLCipher 4 is the primary supported encrypted database configuration.

SQLCipher 3 and custom configurations are represented by profiles but require additional provider support before they can be opened.

### Windows

The current release is primarily intended for:

```text
Windows x64
```

---

# 🧑‍💻 Development

Contributions are welcome.

A typical development workflow is:

```text
Create branch
     ↓
Implement feature
     ↓
Test database operations
     ↓
Test backup/rollback
     ↓
Test encrypted database
     ↓
Test UI
     ↓
Build Release
     ↓
Submit Pull Request
```

When working on database functionality, always test against a **copy** of the original save file.

---

# ⚠️ Important: Back Up Your Saves

Although the application includes automatic backups, users should still maintain their own backups of important game saves.

Before experimenting with an unfamiliar database:

```text
Original Save
     │
     ├──► Personal Backup
     │
     └──► Game Save Editor
```

Never assume that an unknown database format can be safely modified.

---


# 🙏 Acknowledgements

This project makes use of several excellent open-source technologies:

* .NET
* .NET MAUI
* Blazor
* SQLite
* SQLCipher
* Microsoft.Data.Sqlite
* SQLitePCLRaw
* Inno Setup

See the respective projects and licenses for additional information.

---

# ⭐ Support the Project

If you find Game Save Editor useful:

<p align="center">

<a href="https://github.com/YOUR_USERNAME/GameSaveEditor">
<img src="https://img.shields.io/badge/⭐%20Star%20on%20GitHub-181717?style=for-the-badge&logo=github&logoColor=white" alt="Star on GitHub">
</a>

<a href="https://github.com/YOUR_USERNAME/GameSaveEditor/issues">
<img src="https://img.shields.io/badge/🐛%20Report%20Issue-D73A49?style=for-the-badge&logo=github&logoColor=white" alt="Report Issue">
</a>

<a href="https://github.com/YOUR_USERNAME/GameSaveEditor/releases">
<img src="https://img.shields.io/badge/⬇️%20Releases-2EA44F?style=for-the-badge&logo=github&logoColor=white" alt="Releases">
</a>

</p>

---

# 📬 Issues & Feature Requests

Found a bug or have an idea?

Open an issue:

```text
GitHub
    ↓
Issues
    ↓
New Issue
```

When reporting a database-related issue, please include:

* Windows version
* Application version
* Database type
* SQLite / SQLCipher version if known
* Error message
* Relevant logs
* Steps to reproduce

**Do not upload or publicly share personal save files containing sensitive information.**

---

# 📌 Project Status

<p align="center">

<img src="https://img.shields.io/badge/Status-Active-success?style=for-the-badge" alt="Active">

<img src="https://img.shields.io/badge/Version-7.0.0-blue?style=for-the-badge" alt="Version 7.0.0">

<img src="https://img.shields.io/badge/Windows-x64-0078D4?style=for-the-badge&logo=windows&logoColor=white" alt="Windows x64">

</p>

**Current release:** `v7.0.0`

The project is actively being developed toward a more powerful, extensible game-save inspection and editing toolkit.

---

<p align="center">
  <strong>🎮 Inspect. Edit. Backup. Restore.</strong>
</p>

<p align="center">
  Built for developers, testers, modders, and anyone who needs a better way to work with game save databases.
</p>
