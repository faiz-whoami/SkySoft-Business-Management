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

After F5 you should be sent to the login page.

## Login

| Username | Password | Role |
|---|---|---|
| `sjenkins` | `Admin@123` | Admin |
| `mrodriguez` | `Sales@123` | Sales Staff |

Wrong password shows a generic error. An inactive account shows a deactivated message. Sales Staff do not see **End Users** in the menu.
