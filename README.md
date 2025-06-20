# 🎓 College Management System Backend

Welcome to the **College Management System Backend** repository! 🚀 This project provides a robust backend for managing college-related operations, built with a modular and scalable architecture. 📚💻

## 🌟 Features

- **User Management** 👥: Handle admin, user, and authentication workflows.
- **Course & Department Management** 📖: Manage courses, departments, and enrollments.
- **Quiz System** 🎯: Create and manage quizzes for educational assessments.
- **Enrollment Tracking** ✅: Monitor user enrollments efficiently.
- **Secure Authentication** 🔒: JWT-based authentication for secure access.
- **API-Driven** 🌐: RESTful APIs for seamless frontend integration.

## 🛠️ Project Structure

The repository is organized into the following key folders and files:

- **Controllers** 🕹️: Contains API controllers for managing various entities.
  - `AdminController.cs`: Handles admin-related operations.
  - `AuthController.cs`: Manages authentication workflows.
  - `courseController.cs`: Manages course-related endpoints.
  - `DepartmentController.cs`: Handles department operations.
  - `EnrollmentsController.cs`: Manages enrollment data.
  - `QuizController.cs`: Controls quiz-related APIs.
  - `UserController.cs`: Manages user operations.

- **Core** 🌐: Core logic and utilities.
  - `DTO` 📦: Data Transfer Objects for request and response handling.
    - **Requests**: Input models for API calls.
      - `course`, `Department`, `quiz`, `user`, `userEnrollments`.
    - **Responses**: Output models for API responses.
      - `Auth`, `CombienDtos`, `course`, `Department`, `quiz`, `user`, `userEnrollments`, `PagedResponseDto.cs`.
  - `Entities` 🗃️: Database entity models.
    - `course`, `department`, `Quizzes`, `user`, `userEnrollments`, `BaseEntity.cs`, `QuizCreator.cs`.

- **Interfaces** 🔗: Defines repository and service contracts.
  - `IAdminRepository.cs`, `ICourseRepository.cs`, `IDepRepository.cs`, `IPasswordHasher.cs`, `IQuizRepository.cs`, `IRepository.cs`, `ITokenService.cs`, `IUserEnrollments.cs`, `IUserService.cs`.

- **Infrastructure** 🏗️: Data access and middleware layers.
  - **Repositories**: Implementation of data access logic.
    - `AdminRepository.cs`, `CourseRepository.cs`, `DepRepository.cs`, `QuizRepository.cs`, `Repository.cs`, `UserEnrollmentsRepository.cs`, `ApplicationDbContext.cs`.
  - **Middlewares**: Custom middleware for request processing.
    - `AuthorizationMiddleware.cs`, `ExceptionMiddleware.cs`, `JwtAuthenticationMiddleware.cs`, `MiddlewareExtensions.cs`.

- **Services** ⚙️: Business logic and utilities.
  - **Auth**: Authentication services.
    - `TokenService.cs`.
  - **Validators**: Input validation logic.
    - `QuizValidator.cs`.

- **Migrations** 🛠️: Database migration scripts.

- **Root Files** 📄:
  - `launchSettings.json`: Configuration for development environment.
  - `.gitignore`: Specifies files to ignore in version control.
  - `AdminRepo_README.md`: Additional documentation for admin repository.
  - `appsettings.Development.json`, `appsettings.json`: Configuration settings.
  - `CollageManagementSystem.csproj`: Project file.
  - `CollageManagementSystem.http`: HTTP client configuration.
  - `CollageManagementSystem.sln`: Solution file.
  - `Dockerfile`: Docker configuration for containerization.
  - `Program.cs`: Entry point of the application.

## 🚀 Getting Started

### 📋 Prerequisites

- [.NET SDK](https://dotnet.microsoft.com/download) (latest version) 🛠️
- [SQL Server](https://www.microsoft.com/en-us/sql-server/) or compatible database 📊
- [Git](https://git-scm.com/) for cloning 📥
- [Docker](https://www.docker.com/) (optional for containerization) 🐳

### ⚙️ Installation

1. **Clone the Repository**:
   ```bash
   git clone https://github.com/Collage-Management-System/CMS-Backend.git
   cd CMS-Backend
   ```

2. **Restore Dependencies**:
   ```bash
   dotnet restore
   ```

3. **Configure Environment**:
   Update `appsettings.json` with your database connection string:
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "your-connection-string"
     }
   }
   ```

4. **Apply Migrations**:
   ```bash
   dotnet ef database update
   ```

5. **Run the Application**:
   ```bash
   dotnet run
   ```

6. **Test APIs**:
   Use tools like Postman to test endpoints (e.g., `/api/users`, `/api/courses`).

## 🧪 Testing

Run tests using:
```bash
dotnet test
```

## 🌐 Deployment

- Deploy using Docker:
  ```bash
  docker build -t cms-backend .
  docker run -p 5000:80 cms-backend
  ```
- Or deploy to a cloud provider like Azure or AWS ☁️.
