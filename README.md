# Business Management Web Application

A comprehensive web-based Business Management Application built with ASP.NET Core Razor Pages for managing Customers, Suppliers, Vendors, End Users, and Business Transactions with integrated Crystal Reports for analytics.

## Project Overview

This application is developed as part of a practical .NET Web Developer Assignment. It demonstrates key skills in ASP.NET MVC architecture, database design, reporting, and web development best practices.

### Key Features

- **User Authentication & Login** - Secure login page for application access
- **Dashboard** - Summary information on Customers, Suppliers, Vendors, End Users, and Transactions
- **Customer Management** - View, create, update, and delete customer information
- **Supplier Management** - Manage supplier information and relationships
- **Vendor Management** - Handle vendor data and interactions
- **End User Management** - Manage application users and permissions
- **Transaction/Order Management** - Track business activities and orders
- **Reports** - Crystal Reports for analytics and business intelligence
- **Navigation Menu** - Intuitive left-side navigation for easy access to all sections

## Technology Stack

- **Backend Framework**: ASP.NET Core (.NET 10) with Razor Pages
- **Language**: C#
- **Frontend**: HTML5, CSS3 (Bootstrap), JavaScript with jQuery
- **Database**: SQL Server
- **Reporting**: Crystal Reports
- **Validation**: Client-side (jQuery Validation) and Server-side

## Requirements

Before running the application, ensure you have the following installed:

- **Visual Studio 2026** (Community Edition or higher) with ASP.NET workload
- **.NET 10 SDK** or later
- **SQL Server** (SQL Server 2019 or later recommended)
- **SQL Server Management Studio (SSMS)** for database management
- **Crystal Reports** (if not included with Visual Studio, install separately)

## Project Structure

```
business_managment_system/
├── Pages/                      # Razor Pages (Views and Page Models)
│   ├── Index.cshtml           # Home page
│   ├── Error.cshtml
│   ├── Shared/                # Shared layouts and partials
│   └── [Feature Pages]/       # Feature-specific Razor Pages
├── Models/                     # Data models (to be created)
├── Data/                       # Database context and repositories (to be created)
├── Services/                   # Business logic services (to be created)
├── wwwroot/                    # Static files (CSS, JavaScript, images)
│   ├── css/                    # Stylesheets
│   ├── js/                     # Client-side JavaScript
│   └── lib/                    # Third-party libraries (Bootstrap, jQuery, etc.)
├── Properties/                 # Application properties and launch settings
├── appsettings.json           # Configuration settings
├── Program.cs                  # Application startup configuration
└── business_managment_system.csproj  # Project file
```

## Setup & Installation

### 1. Database Setup

1. Open **SQL Server Management Studio (SSMS)**
2. Create a new database named `BusinessManagementDB`
3. Execute the database creation and sample data script located in the `Database/` folder:
   ```sql
   -- Execute the provided SQL script to create tables, relationships, and sample data
   ```

### 2. Connection String Configuration

Update the connection string in `appsettings.json`:

```json
{
  "ConnectionStrings": {
	"DefaultConnection": "Server=YOUR_SERVER_NAME;Database=BusinessManagementDB;Trusted_Connection=true;Encrypt=false;"
  }
}
```

Replace `YOUR_SERVER_NAME` with your SQL Server instance name (e.g., `localhost` or `.\SQLEXPRESS`).

### 3. Install Dependencies

1. Open the solution in **Visual Studio 2026**
2. Right-click the project and select **Manage NuGet Packages**
3. Restore all NuGet packages (or use Package Manager Console: `Update-Package -Reinstall`)

### 4. Apply Database Migrations (if using Entity Framework)

If using Entity Framework Core, apply migrations:

```powershell
dotnet ef database update
```

### 5. Build & Run the Application

1. Press **F5** or click **Start** to run the application
2. The application will launch in your default browser at `https://localhost:7XXX` (port varies)
3. Login with the default credentials provided in the sample data script

## Running the Application

### Development

```powershell
dotnet run
```

### Production Build

```powershell
dotnet publish -c Release
```

## Default Login Credentials

Default credentials for testing (from sample data):

```
Username: admin
Password: Admin@123
```

**Note**: Change default credentials in production environments immediately.

## Features in Detail

### 1. Dashboard
- Summary cards showing counts of Customers, Suppliers, Vendors, and End Users
- Recent transactions overview
- Quick links to main sections

### 2. Customer Management
- List all customers with search and filtering
- View customer details and transaction history
- Create new customers
- Update customer information
- Delete customers
- View associated transactions

### 3. Supplier Management
- Manage supplier information
- Track supplier relationships
- View supplied products/services
- Contact information management

### 4. Vendor Management
- Manage vendor information
- Track vendor transactions
- Performance metrics

### 5. End User Management
- User registration and management
- Role-based access control
- User profile management
- Activity logging

### 6. Transactions/Orders
- Create and manage business transactions
- Track relationships between Customers, Suppliers, and Vendors
- Order status management
- Transaction history and reporting

### 7. Reports
The application includes Crystal Reports for:
- **Customer Listing Report** - Comprehensive list of all customers with details
- **Transaction Detail Report** - Detailed breakdown of transactions with customer and vendor information
- **Summary Report** - Grouped report with totals and subtotals by category

## Database Design

### Key Tables
- `Users` - Application users for login and access control
- `Customers` - Customer information
- `Suppliers` - Supplier information
- `Vendors` - Vendor information
- `Transactions` - Business transactions and orders
- `TransactionDetails` - Line items for transactions

### Database Features
- Primary and foreign key constraints
- Referential integrity
- Indexes on frequently queried columns
- Database views for reporting
- Stored procedures for complex operations

## Validation

### Client-Side Validation
- Real-time validation using jQuery Validation plugin
- Input masking for phone numbers and dates
- Custom validation rules

### Server-Side Validation
- Data annotation validation in Models
- Business logic validation in Services
- Error handling and user feedback

## Error Handling

- Structured exception handling throughout the application
- User-friendly error messages
- Logging of errors for debugging
- Graceful error pages (500, 404, etc.)

## Security Practices

- **Authentication**: Forms-based authentication with password hashing
- **Authorization**: Role-based access control
- **Input Validation**: Prevent SQL injection and XSS attacks
- **HTTPS**: All connections use HTTPS in production
- **CSRF Protection**: Built-in ASP.NET Core CSRF token validation
- **Secure Headers**: Security-related HTTP headers configured
- **Password Policy**: Strong password requirements enforced

## Important Assumptions

1. **Database Server**: Application assumes SQL Server is running and accessible locally or via configured connection string
2. **User Roles**: Three role types are assumed: Admin, Supervisor, and User
3. **Business Logic**: Transaction amounts are in the default currency (USD assumed)
4. **Date Format**: All dates use the application's configured culture format
5. **Email Notifications**: Optional; configure SMTP settings for email features
6. **Concurrent Users**: Application designed for small to medium-sized teams (< 100 concurrent users)
7. **Data Retention**: No automatic archival; data retention policy to be determined by organization

## Troubleshooting

### Common Issues

**Issue**: Connection string error when running
- **Solution**: Verify SQL Server is running and connection string in `appsettings.json` is correct

**Issue**: Crystal Reports not displaying
- **Solution**: Ensure Crystal Reports runtime is installed; may need to install via NuGet package

**Issue**: Static files (CSS/JS) not loading
- **Solution**: Rebuild solution and clear browser cache; ensure `wwwroot` folder exists

**Issue**: Login page shows but credentials don't work
- **Solution**: Verify database is created and sample data script was executed successfully

## Development Notes

- The application uses a Repository Pattern for data access
- Services contain business logic and are injected via Dependency Injection
- Razor Pages combine view and page model logic for simplified organization
- Bootstrap framework provides responsive UI
- jQuery provides client-side interactivity

## Future Enhancements

- Email notifications for transactions
- Advanced reporting with export to Excel/PDF
- Mobile application support
- API for third-party integrations
- Real-time dashboard updates using SignalR
- Multi-language support
- Advanced search and filtering capabilities
- Data encryption for sensitive fields

## Support & Documentation

For questions or issues:
1. Check the Troubleshooting section above
2. Review the database schema documentation
3. Examine Crystal Reports configuration files
4. Review ASP.NET Core documentation: https://docs.microsoft.com/aspnet/core

## License

This is an educational project for the practical .NET Web Developer Assignment.

## Author

Developed as part of practical ASP.NET skills assessment.

---

**Last Updated**: 2024
**Status**: In Development
