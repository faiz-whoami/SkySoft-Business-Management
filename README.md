# business_managment_system

ASP.NET MVC 5 (.NET Framework 4.8) web application. The on-screen product name is **SkySoft Business Management**.

Open this file in Visual Studio:

`business_managment_system.slnx`

## Run

1. Open `business_managment_system.slnx` in Visual Studio.
2. If Visual Studio asks to restore NuGet packages, choose Restore.
3. Press **F5**. You will be sent to the login page.

## Login

| Username | Password | Role |
|---|---|---|
| `sjenkins` | `Admin@123` | Admin |
| `mrodriguez` | `Sales@123` | Sales Staff |

Wrong passwords show a generic error. An inactive account shows a deactivated message. Sign out is in the top-right.

## Database (Module 2)

SQL Server Express instance: `localhost\SQLEXPRESS02`. You do **not** need SQL Server Management Studio (SSMS). Use `sqlcmd` from PowerShell.

Open PowerShell, then:

```powershell
& "C:\Program Files\Microsoft SQL Server\Client SDK\ODBC\180\Tools\Binn\SQLCMD.EXE" -S "localhost\SQLEXPRESS02" -E -C -I -d SkySoftDB
```

Meaning of the flags:
- `-S` server/instance
- `-E` Windows login (no SQL password)
- `-C` trust the local certificate
- `-I` quoted identifiers (needed for our indexes)
- `-d` database name

At the `1>` prompt:

```sql
SELECT Name FROM Customer;
GO
EXIT
```

To recreate the database from the project scripts (run from the `Database` folder, in this order):

```powershell
cd "d:\Projects\SkySoft\Business Management System\business_managment_system\Database"
$sqlcmd = "C:\Program Files\Microsoft SQL Server\Client SDK\ODBC\180\Tools\Binn\SQLCMD.EXE"
& $sqlcmd -S "localhost\SQLEXPRESS02" -E -C -I -i .\01_CreateSchema.sql
& $sqlcmd -S "localhost\SQLEXPRESS02" -E -C -I -i .\02_ViewsAndProcedures.sql
& $sqlcmd -S "localhost\SQLEXPRESS02" -E -C -I -i .\03_SeedData.sql
```

If your instance name is different, change the `Server=` value in `business_managment_system\Web.config` as well.

## Reports (PDF)

The Reports page streams PDFs from the same three stored procedures the assignment specified:

- `sp_Report_PartyDirectory` — Business Partner Directory
- `sp_Report_TransactionDetail` — Transaction Detail Statement
- `sp_Report_MonthlySummary` — Monthly Transaction Summary

The Reports page uses the same three stored procedures the assignment specified. On this development PC, SAP Crystal Reports for Visual Studio is not installed (the CRforVS package is several hundred megabytes), so PDFs come from `TablePdfWriter`. Empty result sets stay on the Reports page with a message.

On a machine that has **SAP Crystal Reports, version for Visual Studio** (v13 / `13.0.4000.0`, the **Install Package**), the first PDF export creates the three assignment `.rpt` files under `business_managment_system\Reports\`:

- `PartyDirectory.rpt`
- `TransactionDetail.rpt`
- `MonthlySummary.rpt`

Those files open in the Crystal Reports designer and are reused by the MVC app on later exports. Copy them back into the project after they are generated. Use **32-bit IIS Express** (already set) so the Crystal native libraries load.
