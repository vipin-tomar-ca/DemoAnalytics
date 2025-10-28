# Payroll Analytics Mobile — Vue 3 + Apache ECharts + .NET 8 + SQLite

Mobile-first payroll analytics (Visier-style) with advanced charts, map⇄bar morphing, filters, multi-tenant header, **JWT login**, and **SQLite DB** seeded with demo data.

## Demo Login
- **Username:** `admin`  **Password:** `admin123`
- (Alt) `analyst` / `analyst123`

## Run

### Backend
```bash
cd backend/Api
dotnet restore
dotnet run
# API http://localhost:5188  Swagger at /swagger
# SQLite DB file: backend/Api/app.db (auto-created & seeded on first run)
```

### Frontend
```bash
cd frontend
npm i
npm run dev
# http://localhost:5173
```

### Schema Overview
- `Users`: login control (username, password hash, role, tenant)
- `OrgUnits`: organization units
- `Employees`: core employee master (province, org, job family, hire/termination)
- `EmployeeStarts`: hire/start events
- `EmployeeExits`: exit events (voluntary/involuntary)
- `EmployeeDataChanges`: audit of field changes
- `Absences`: sick/PTO/LOA and **overtime** records (hours, cost)
- `Compensations`: comp history (base, bonus, benefits, payroll taxes)

### Frontend
- Login view stores JWT in localStorage; Axios adds `Authorization: Bearer ...`
- Dashboard with KPI tiles + advanced charts; filter bar passes query params

### Notes
- For production: rotate JWT key, add refresh tokens, migrations, and RBAC.
