# CRUD Application using ASP.NET Core MVC (.NET 8)

This project is a simple CRUD (Create, Read, Update, Delete) application built using ASP.NET Core MVC, Entity Framework Core, and SQL Server.

---

# 🚀 Technologies Used

- ASP.NET Core MVC (.NET 8)
- Entity Framework Core
- SQL Server / SQL Server Express
- Visual Studio 2022+

---

# 📌 1. Create ASP.NET MVC Project

## Install Required Tools:

- Visual Studio
- .NET SDK (.NET 8 or latest)
- SQL Server Express

## Create Project:

1. Open Visual Studio  
2. Click **Create a new project**  
3. Select **ASP.NET Core Web App (Model-View-Controller)**  
4. Project Name: `CrudApp`  
5. Choose `.NET 8 (or latest)`  
6. Click **Create**

---

# 📦 2. Install Entity Framework Packages

Open:
Tools → NuGet Package Manager → Package Manager Console


Run:

```powershell
Install-Package Microsoft.EntityFrameworkCore.SqlServer
Install-Package Microsoft.EntityFrameworkCore.Tools
```

# 🧩 3. Create Model

Create folder:
Models

Create file:
Student.cs
using System.ComponentModel.DataAnnotations;

```code
namespace CrudApp.Models
{
    public class Student
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Range(1,100)]
        public int Age { get; set; }

        [Required]
        public string Course { get; set; }
    }
}
```

#🗄️ 4. Create Database Context

Create folder:
>> Data

Create file:
>> ApplicationDbContext.cs

```code
using Microsoft.EntityFrameworkCore;
using CrudApp.Models;

namespace CrudApp.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Student> Students { get; set; }
    }
}
```

# 5. Configure Connection String

Open:
>> appsettings.json

```code
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=.;Database=CrudDb;Trusted_Connection=True;TrustServerCertificate=True"
  }
}
```

# 6. Register DbContext

Open:
Program.cs

Add:

```code
using CrudApp.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

app.UseStaticFiles();

app.UseRouting();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
```

# 7. Create Migration & Database
Open:
Package Manager Console

Run:
```powershell
Add-Migration InitialCreate
Update-Database
```

This will create:
Database: CrudDb
Table: Students

# 8. Create MVC Controller
Steps:

Right-click Controllers
Click Add → Controller

Select:
1.MVC Controller with views, using Entity Framework

Choose:
1.Model Class: Student
2.Data Context: ApplicationDbContext

Click Add

✔ This automatically generates:
Controller
Views
CRUD operations

Features

You will get:
Create Student
View Students
Edit Student
Delete Student
Details Page

** Note : Change Default route from program.cs 

