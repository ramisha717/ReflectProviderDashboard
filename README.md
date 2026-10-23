# Reflect Provider Dashboard

Backend and frontend implementation for the Reflect Provider Dashboard.

## Current Progress

The backend for the **Patients list page** is ready for frontend development.

Implemented so far:

- `Patient` and `PatientSession` domain entities
- `IPatientRepository` and `IPatientSessionRepository` interfaces
- In-memory repositories containing synthetic patient and session data
- `PatientsController` and `PatientSessionsController`
- `PatientIndexViewModel` and `PatientRowViewModel`

The Patients list ViewModel currently provides patient ID, patient code, last activity, session count, platform, trend, and review status.

Session counts, last activity, and platform are calculated from successfully recorded sessions. The trend calculation is not implemented yet.

### Patients List Frontend

The ViewModels are located in:

`Reflect.Web/Models/ViewModels/Patients/Index/`

The Patients endpoint is:

`/Provider/Patients/Index`

It currently returns JSON for testing. Once the Razor view is implemented, change the controller's `return Json(viewModel)` to `return View(viewModel)`.

The frontend can use the existing ViewModels as a starting point and add properties as needed. Please coordinate any changes that require additional backend data or modifications to shared entities and repositories.

## Next Steps

I'll continue implementing the backend for the remaining pages:

- Patient Profile
- Session History
- Clinical Response

For each page, I'll implement the initial ViewModel and its data-loading logic first, then push the changes so frontend work can begin. **Please wait until I've pushed the initial ViewModel for a page before starting its implementation**, so we can avoid duplicating work.

I'll also continue filling in the remaining backend functionality, including the Patients list trend calculation, while frontend development progresses.

## Running the Project

From the `ReflectProviderDashboard` directory:

```bash
dotnet build
dotnet run --project Reflect.Web
```

The Patients list endpoint is available at `/Provider/Patients/Index`.

All current patient data is synthetic. Persistent storage and authentication/authorization integration are not implemented yet.
