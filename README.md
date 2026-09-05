# SkySoft Business Management

ASP.NET MVC 5 (.NET Framework 4.8) web app for customers, suppliers, vendors, products, transactions, end users, and three PDF reports.

The Visual Studio project folder is spelled `business_managment_system` on purpose. The name on screen is **SkySoft Business Management**.

This README is the setup guide. Follow it in order whether you **cloned the Git repo** or **opened a Zip**.

---

## 1. What you need on the PC

| Software | Why |
|---|---|
| Windows 10 or later | The project is .NET Framework, not .NET Core. |
| [Visual Studio 2022](https://visualstudio.microsoft.com/) (Community is fine) | Open and run the site. |
| **ASP.NET and web development** workload in Visual Studio | MVC 5 project type. |
| **.NET Framework 4.8 targeting pack** | Build target. Install it from the Visual Studio Installer if the project will not build. |
| SQL Server 2019+ **or SQL Server Express** | Database. SSMS is optional. |
| `sqlcmd` | Runs the three SQL scripts. It is installed with SQL Server. |

Optional, only for Crystal `.rpt` files:

| Software | Why |
|---|---|
| **SAP Crystal Reports, developer version for Microsoft Visual Studio** (v13 / `13.0.4000.0`) | Assignment report templates. Install the **CRforVS Install Package**, not only the runtime. |

Reports still download as PDF if Crystal is not installed. See [section 7](#7-crystal-reports-optional-but-required-for-rpt-files).

---

## 2. Get the project onto the PC

### Option A — Git clone

```powershell
git clone <repo-url>
cd business_managment_system
```

You are in the right folder when you can see all of these:

- `README.md` (this file)
- `business_managment_system.slnx`
- `Database\`
- `business_managment_system\` (the web project)

### Option B — Zip file

1. Extract the Zip.
2. Open the extracted folder until you see `README.md` and `business_managment_system.slnx`.
3. In PowerShell:

```powershell
cd "<path-to-that-folder>"
```

Example after extract:

```powershell
cd "$env:USERPROFILE\Downloads\business_managment_system"
```

Do **not** open the inner `business_managment_system\business_managment_system` folder as the solution root. Open the folder that contains the `.slnx` file.

---

## 3. Confirm SQL Server is running

1. Press **Win**, type **Services**, open it.
2. Find a service named like **SQL Server (SQLEXPRESS02)** or **SQL Server (SQLEXPRESS)**.
3. Status must be **Running**. If it is Stopped, right-click → **Start**.

The name in parentheses is the **instance name**.

| Service name | Use this server value |
|---|---|
| SQL Server (SQLEXPRESS02) | `localhost\SQLEXPRESS02` |
| SQL Server (SQLEXPRESS) | `localhost\SQLEXPRESS` |
| SQL Server (MSSQLSERVER) | `localhost` |

This project was built against **`localhost\SQLEXPRESS02`**. If your instance is different, use yours in **every** command below and in `Web.config`.

To list instances from PowerShell:

```powershell
sqlcmd -L
```

If `sqlcmd` is not on your PATH, use the full path (usual install):

```powershell
& "C:\Program Files\Microsoft SQL Server\Client SDK\ODBC\180\Tools\Binn\SQLCMD.EXE" -L
```

---

## 4. Create the database and load sample data

The three scripts live in `Database\`. Run them **in this order**. Script `01` drops `SkySoftDB` if it already exists and creates it again.

### 4.1 Set the instance and sqlcmd path

In PowerShell, from the folder that contains `README.md`:

```powershell
$instance = "localhost\SQLEXPRESS02"
$sqlcmd = "C:\Program Files\Microsoft SQL Server\Client SDK\ODBC\180\Tools\Binn\SQLCMD.EXE"
```

Change `$instance` if your SQL Server name is different. If `sqlcmd` is already on PATH you can use:

```powershell
$sqlcmd = "sqlcmd"
```

### 4.2 Run the three scripts

```powershell
& $sqlcmd -S $instance -E -C -I -i .\Database\01_CreateSchema.sql
& $sqlcmd -S $instance -E -C -I -i .\Database\02_ViewsAndProcedures.sql
& $sqlcmd -S $instance -E -C -I -i .\Database\03_SeedData.sql
```

What the flags mean:

| Flag | Meaning |
|---|---|
| `-S` | Server / instance |
| `-E` | Windows Authentication (no SQL password) |
| `-C` | Trust the local certificate |
| `-I` | Quoted identifiers (required for the Product SKU index) |
| `-i` | Input script file |

Each script should finish without error. `01` prints nothing useful when it succeeds. If `02` or `03` fail, fix the error and run **all three again from `01`**.

### 4.3 Check that data is there

```powershell
& $sqlcmd -S $instance -E -C -I -d SkySoftDB -Q "SELECT EndUserId, Username, FullName, RoleId FROM dbo.EndUser; SELECT COUNT(*) AS Customers FROM dbo.Customer; SELECT COUNT(*) AS Transactions FROM dbo.[Transaction];"
```

You should see:

- `imran` / Imran Khan
- `khalid` / Khalid Mehmood
- several customers and 8 sample transactions

### 4.4 Using SSMS instead of sqlcmd

1. Connect to the same instance with Windows Authentication.
2. Open and **Execute** `Database\01_CreateSchema.sql`.
3. Then execute `02_ViewsAndProcedures.sql`.
4. Then execute `03_SeedData.sql`.

---

## 5. Point the website at that database

Open:

`business_managment_system\Web.config`

Find:

```xml
<connectionStrings>
  <add name="SkySoftDb"
       connectionString="Server=localhost\SQLEXPRESS02;Database=SkySoftDB;Trusted_Connection=True;TrustServerCertificate=True;"
       providerName="System.Data.SqlClient" />
</connectionStrings>
```

Change only the `Server=` value if your instance is not `localhost\SQLEXPRESS02`. Leave `Database=SkySoftDB`.

`Trusted_Connection=True` uses your Windows login. You do not type a SQL password.

---

## 6. Open and run the website

1. Double-click `business_managment_system.slnx`, or in Visual Studio use **File → Open → Project/Solution**.
2. If Visual Studio asks to **restore NuGet packages**, click Restore. Packages are `Microsoft.AspNet.Mvc` 5.2.9 and `Newtonsoft.Json` 13.0.3.
3. In Solution Explorer, right-click the `business_managment_system` web project → **Set as Startup Project**.
4. Confirm the project is using **IIS Express, 32-bit** (already set in the project file as `Use64BitIISExpress=false`). Crystal native libraries need 32-bit.
5. Press **F5** (or **IIS Express**).
6. The browser should open the **Sign in** page.

If the browser shows a certificate warning on `https://localhost:44355`, continue to the site. That is normal for IIS Express.

---

## 7. Crystal Reports (optional, but required for `.rpt` files)

The assignment asks for three Crystal report files:

| File | Report |
|---|---|
| `business_managment_system\Reports\PartyDirectory.rpt` | Business Partner Directory |
| `business_managment_system\Reports\TransactionDetail.rpt` | Transaction Detail Statement |
| `business_managment_system\Reports\MonthlySummary.rpt` | Monthly Transaction Summary |

Those `.rpt` files are binary Crystal templates. They are **created automatically on the first PDF export** on a PC that has Crystal installed. They cannot be invented by hand.

### If Crystal is not installed

The Reports page still works. PDFs are built from the same three stored procedures by `TablePdfWriter`. You can test every report without Crystal.

### If you install Crystal (to produce the `.rpt` files)

1. Close Visual Studio.
2. Install **SAP Crystal Reports, developer version for Microsoft Visual Studio** (v13 / `13.0.4000.0`), **Install Package**.
3. Re-open the solution and press **F5**.
4. Sign in as Admin.
5. Open **Reports**.
6. Click **Open PDF** on each of the three cards (for Transaction Detail use a seed id such as `1`).
7. After the first successful Crystal export, these files appear under `business_managment_system\Reports\`:
   - `PartyDirectory.rpt`
   - `TransactionDetail.rpt`
   - `MonthlySummary.rpt`
8. Copy those three files back into the project (and into Git / the Zip) so the next machine can open them in the Crystal designer.

How to tell which engine ran:

- Crystal installed: the app loads the `.rpt` (or creates it) and exports PDF through Crystal.
- Crystal missing: you still get a PDF table. No `.rpt` file is created.

---

## 8. Test logins

| Full name | Username | Password | Role |
|---|---|---|---|
| Imran Khan | `imran` | `Admin@123` | Admin |
| Khalid Mehmood | `khalid` | `Sales@123` | Sales Staff |

Usernames are lowercase. Passwords are case-sensitive.

Wrong username or password shows a generic error. A deactivated account shows a deactivated message. **Sign out** is in the top-right.

---

## 9. What to click through (submission test)

Sign in as **Imran Khan** (`imran` / `Admin@123`) first.

1. **Dashboard** — four summary cards and the five most recent transactions.
2. **Customers** — search, Active/Inactive/All, New, Edit, View (contact + sales history), Deactivate / Reactivate.
3. **Suppliers** and **Vendors** — same pattern. Vendors also have a service type.
4. **Products** — catalogue with SKU and price. Soft deactivate hides a product from new transactions.
5. **End Users** — only Admin sees this in the sidebar. You cannot deactivate your own account. The last Admin cannot be deactivated.
6. **Transactions → New transaction**
   - Type **Sale** → customer list.
   - Type **Purchase** → supplier list.
   - Type **Service** → vendor list.
   - Add lines. Pick a product to fill description and price, or leave **Custom line**.
   - Save. The total is recalculated in `sp_CreateTransactionWithItems`, not trusted from the browser.
7. Open a **Pending** transaction → **Complete** or **Cancel**.
8. **Reports**
   - Business Partner Directory (All / Customers / Suppliers / Vendors).
   - Transaction Detail Statement (enter `1`).
   - Monthly Transaction Summary (pick a year or All years).
   - Empty results stay on the page with a message. They do not download a blank PDF.

Then sign out and sign in as **Khalid Mehmood** (`khalid` / `Sales@123`):

9. Customers — full create / edit / deactivate.
10. Suppliers, Vendors, Products — list and View only. Create / Edit URLs send you back to the Dashboard with an access message.
11. End Users — hidden in the sidebar. Opening `/User` redirects to the Dashboard.
12. Transactions and Reports — same access as Admin.

---

## 10. SQL objects the assignment asked for

| Script | What it creates |
|---|---|
| `Database\01_CreateSchema.sql` | `SkySoftDB`, tables, keys, check constraint, indexes |
| `Database\02_ViewsAndProcedures.sql` | `vw_PartyDirectory`, `vw_MonthlyTransactionSummary`, `sp_CreateTransactionWithItems`, `sp_Report_PartyDirectory`, `sp_Report_TransactionDetail`, `sp_Report_MonthlySummary` |
| `Database\03_SeedData.sql` | Roles, types, statuses, the two test users, sample master data and transactions |

---

## 11. If something fails

| Symptom | What to do |
|---|---|
| `sqlcmd` cannot connect | SQL Server service is not running, or `$instance` is wrong. Check Services and `sqlcmd -L`. |
| Website dashboard error about SQL Server | `Web.config` `Server=` does not match the instance you used for the scripts. |
| Login rejected for `imran` / `khalid` | The database still has the old users. Run the three scripts again from `01`. |
| NuGet restore failed | In Visual Studio: **Project → Restore NuGet Packages**. Need internet once. |
| Project will not build / targeting pack | Visual Studio Installer → Modify → Individual components → **.NET Framework 4.8 targeting pack**. |
| Crystal PDF fails after you installed Crystal | Confirm IIS Express is **32-bit** (`Use64BitIISExpress` is `false`). Restart Visual Studio. |
| Port already in use | Visual Studio will offer another IIS Express port. Accept it. |

---

## 12. Project layout

```
business_managment_system/          ← open this folder (clone / Zip root)
  README.md
  business_managment_system.slnx
  Database/
    01_CreateSchema.sql
    02_ViewsAndProcedures.sql
    03_SeedData.sql
  business_managment_system/        ← ASP.NET MVC 5 web project
    Web.config                      ← connection string
    Controllers / Services / Data / Views
    Scripts/transactions.js
    Reports/                        ← .rpt files appear here after Crystal export
```
