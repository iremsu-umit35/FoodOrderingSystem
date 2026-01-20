FoodOrderingSystem – Database Project


This project is a Food Ordering System application developed using
ASP.NET Core MVC and Microsoft SQL Server.
The system is designed as a web application that works with different user roles.

--------------------------------------------------
Technologies Used
--------------------------------------------------
- ASP.NET Core MVC
- Microsoft SQL Server
- SQL Server Management Studio (SSMS)
- Visual Studio
- Entity Framework Core

--------------------------------------------------
Database Information
--------------------------------------------------
Database Name:
FoodOrderingSystem

The database must be restored using the .bak file included
in the Database folder of the project.

--------------------------------------------------
Database Setup (Restore)
--------------------------------------------------
1. Open SQL Server Management Studio (SSMS).
2. Right-click on Databases and select "Restore Database".
3. Select the Device option and choose the provided .bak file.
4. Set the database name as "FoodOrderingSystem".
5. Complete the restore process.

--------------------------------------------------
Running the Project
--------------------------------------------------
1. Open the FoodOrderingSystem.sln file using Visual Studio.
2. Open the appsettings.json file.
3. Check and update the connection string if necessary.
4. Run the project from Visual Studio.
5. The application will start in the browser.

--------------------------------------------------
Database Connection Settings
--------------------------------------------------
The database connection is defined in the following file:

- appsettings.json

"ConnectionStrings": {
  "DefaultConnection": "Server=YOUR_SERVER_NAME;Database=FoodOrderingSystem;Trusted_Connection=True;"

}


The server and database information in the ConnectionStrings
section should be updated according to your SQL Server settings.

--------------------------------------------------
User Roles
--------------------------------------------------
The system includes three different user roles:

- Admin
- Restaurant Owner
- Customer

--------------------------------------------------
Admin Account 
--------------------------------------------------
admin account for the login:

Username: admin@gmail.com
Password: 123456

(The admin account can be used to manage the system.)

--------------------------------------------------
