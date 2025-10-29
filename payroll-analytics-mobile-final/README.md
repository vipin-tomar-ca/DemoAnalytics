# Payroll Analytics Mobile — Vue 3 + Apache ECharts + .NET 8 + SQLite

Mobile-first payroll analytics (Visier-style) with advanced charts, map⇄bar morphing, filters, multi-tenant header, and **SQLite DB** seeded with demo data.

## Demo Login
_Authentication is currently disabled._

## Run

### Backend
1.  Navigate to the backend directory:
    ```bash
    cd backend/Api
    ```
2.  Restore NuGet packages:
    ```bash
    dotnet restore
    ```
3.  Ensure the database is up-to-date and seeded with demo data:
    ```bash
    dotnet ef database drop --force
    dotnet ef migrations remove # if any exist from previous runs
    dotnet ef migrations add InitialSetup
    dotnet ef database update
    ```
    (Note: The `Seed.EnsureDbAsync` method in `backend/Api/Data/Seed.cs` will automatically populate initial data on first run after `dotnet ef database update` or `dotnet run` if the database is empty.)
4.  Run the API:
    ```bash
    dotnet run
    # API accessible at http://localhost:5000
    # Swagger UI at http://localhost:5000/swagger
    # SQLite DB file: backend/Api/payroll.db (auto-created & seeded on first run)
    ```
    If you encounter "address already in use" errors, you may need to kill the process occupying port 5000:
    ```bash
    kill $(lsof -t -i:5000)
    ```

### Frontend
1.  Navigate to the frontend directory:
    ```bash
    cd frontend
    ```
2.  Install npm dependencies:
    ```bash
    npm install
    ```
3.  Run the development server:
    ```bash
    npm run dev
    # Frontend accessible at http://localhost:5173
    ```
    If you encounter "address already in use" errors, you may need to kill the process occupying port 5173:
    ```bash
    kill $(lsof -t -i:5173)
    ```

## Developer Onboarding

### Project Structure
*   **`backend/Api`**: Contains the .NET 8 Web API project. This is where the API endpoints, data models, database context, and business logic reside.
*   **`frontend`**: Contains the Vue 3 (TypeScript) application. This is the user interface that consumes data from the backend API.

### Technologies Used
*   **Backend**:
    *   .NET 8 (C#)
    *   ASP.NET Core Web API
    *   Entity Framework Core (for database interaction)
    *   SQLite (lightweight, file-based database for development)
*   **Frontend**:
    *   Vue 3 (with Composition API and `<script setup>`)
    *   TypeScript
    *   Vite (build tool)
    *   Apache ECharts (for interactive data visualizations)
    *   Tailwind CSS (for styling)
    *   Axios (for API calls)
    *   Pinia (Vuex-like state management for filters and auth)

### Key Files and Components

**Backend (`backend/Api`)**:
*   **`Program.cs`**: The application's entry point, configuring services, middleware, and the HTTP request pipeline. Includes database context setup and initial data seeding.
*   **`Data/PayrollContext.cs`**: The Entity Framework Core `DbContext` defining the database schema and relationships between models.
*   **`Data/Seed.cs`**: Contains the logic for seeding the SQLite database with initial demo data. This is crucial for development and testing.
*   **`Models/`**: Directory containing all Entity Framework Core data models (e.g., `Employee.cs`, `Compensation.cs`, `Location.cs`).
*   **`DTOs/`**: Directory containing Data Transfer Objects (DTOs) used for API responses (e.g., `Kpis.cs`, `GeoRow.cs`).
*   **`Controllers/`**: Directory containing API controllers (e.g., `AnalyticsController.cs`, `CompensationController.cs`) that expose endpoints for data retrieval.
*   **`Services/`**: Directory containing service classes that encapsulate business logic and interact with the `DbContext`.

**Frontend (`frontend`)**:
*   **`src/main.ts`**: The main entry point for the Vue application.
*   **`src/App.vue`**: The root Vue component.
*   **`src/router.ts`**: Defines the Vue Router routes for navigation.
*   **`src/api.ts`**: Configures the `axios` instance for making API calls to the backend and defines TypeScript types for API responses.
*   **`src/stores/filters.ts`**: Pinia store for managing global filters used across the dashboard.
*   **`src/views/Dashboard.vue`**: The main dashboard component that orchestrates other components and displays overall analytics.
*   **`src/components/`**: Directory containing reusable Vue components (e.g., `KpiTiles.vue`, `GeoMorphChart.vue`, `HeadcountTrend.vue`).
*   **`src/components/visier/`**: Sub-directory for specific "Visier-style" analytics components.

### API Interaction (Frontend)
The frontend uses `axios` (configured in `src/api.ts`) to communicate with the backend.
*   **`baseURL`**: Set to `http://localhost:5000/api` in `src/api.ts` to point to the running backend.
*   **Interceptors**: `src/api.ts` includes an interceptor to automatically attach filters from the `useFilters` Pinia store as query parameters to API requests.
*   **Data Fetching**: Components like `KpiTiles.vue` use functions exported from `src/api.ts` (e.g., `fetchKpisAll`, `fetchKpis`) to retrieve data.

### Data Seeding for Development
The backend's `Seed.EnsureDbAsync` method in `backend/Api/Data/Seed.cs` is critical for populating the SQLite database with demo data. This method employs a two-pass strategy:
1.  **First Pass**: All `Employee` records are created and saved to the database to generate their unique IDs.
2.  **Second Pass**: Related entities (`EmployeeStart`, `EmployeeExit`, `Compensation`, `Absence`) are then created and linked to the existing `Employee` records using their generated IDs.

This ensures data integrity and prevents foreign key constraint errors during seeding.

### Troubleshooting Common Issues
*   **"Address already in use"**: If either the backend or frontend fails to start due to this error, use `kill $(lsof -t -i:PORT_NUMBER)` (replace `PORT_NUMBER` with 5000 for backend or 5173 for frontend) to terminate the conflicting process.
*   **Blank page on frontend**: Check the browser's developer console for JavaScript errors. Ensure the backend is running and the `baseURL` in `frontend/src/api.ts` is correct.
*   **No data on charts/KPIs (frontend)**:
    *   Verify the backend API is running (`http://localhost:5000/swagger`).
    *   Check backend console for any runtime errors during API calls.
    *   Confirm that the `Seed.EnsureDbAsync` method has run successfully and populated the `payroll.db` database (you can use `sqlite3 backend/Api/payroll.db "SELECT COUNT(*) FROM Employees;"` to check).
    *   Inspect browser network requests for failed API calls or incorrect responses.
*   **Backend compilation errors**:
    *   Run `dotnet build` in `backend/Api` to see detailed compilation errors.
    *   Pay attention to `CS0234` (missing namespace), `CS1061` (missing property/method), and `CS8618`/`CS8602`/`CS8603` (nullable reference type warnings/errors).
*   **`FOREIGN KEY constraint failed` errors during seeding**: This indicates a problem with the order or completeness of data insertion. Ensure `Seed.cs` is correctly implementing the two-pass seeding strategy. You may need to drop the database, delete migrations, and create a new migration (`dotnet ef database drop --force`, `dotnet ef migrations remove`, `dotnet ef migrations add NewMigration`, `dotnet ef database update`) after fixing `Seed.cs`.
