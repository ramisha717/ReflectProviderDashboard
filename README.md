# Reflect XR Provider Dashboard

Standalone ASP.NET Core MVC demo of a clinician-facing Reflect XR Provider Dashboard. It implements patient review, session activity, clinical response, monitoring documentation, progress views, and report exports using synthetic data and replaceable integration stand-ins.

## Requirements and quick start

- .NET SDK **10**
- No database, external API, or credentials are required for the **Development** demo.

From the solution root (the directory containing `ReflectProviderDashboard.slnx`):

```bash
dotnet restore
dotnet build
dotnet run --project Reflect.Web
```

The included `Reflect.Web/Properties/launchSettings.json` sets `ASPNETCORE_ENVIRONMENT=Development` and the HTTP launch URL to `http://localhost:5079`. Open `http://localhost:5079/Provider/Overview`. If your console reports a different listening URL, use that address instead. Running outside Development is **not** a no-secrets signing configuration; see *Signed reports* below.

The demo supplies a simulated clinician identity with the `Clinician` role. **This is not production authentication.** Data is synthetic and held in memory; edits and generated records are not durable across application restarts.

## Implemented screens and URLs

All provider screens live in the `Provider` MVC Area and use the `ClinicianOnly` authorization policy. URLs below are relative to the application origin.

| Screen | URL | Purpose |
| --- | --- | --- |
| Overview | `/Provider/Overview` | Clinician dashboard and patient attention overview |
| Attention review | `/Provider/Overview/AttentionReview` | Review patients requiring attention |
| Patients | `/Provider/Patients/IndexPage` | Patient list and selection |
| Patient session details | `/Provider/Patients/SessionDetails?patientId={guid}&sessionId={guid}` | Individual session detail and review action |
| Patient session history | `/Provider/PatientSessionHistory/IndexPage` | Session history for a selected patient |
| Clinical response | `/Provider/ClinicalResponse/IndexPage` | Clinical response workflow |
| Monitoring documentation | `/Provider/MonitoringDocumentation/IndexPage?patientCode={code}` | Generate, edit, save, and review monitoring documentation |
| Progress | `/Provider/Progress/Index` | Patient progress and activity |
| Platform comparison | `/Provider/Progress/PlatformComparisonPage` | XR versus mobile activity comparison |
| Reports | `/Provider/Reports` | Report preview, PDF/CSV exports, signed ZIP exports, and signature verification |
| Settings | `/Provider/Settings/Index` | Provider settings screen |

Some screens require patient identifiers or query parameters and are normally reached through dashboard navigation. `IndexPage` routes are used where a controller's `Index` action also serves JSON to the frontend; for example, `/Provider/Patients?format=json` is a patient-data endpoint, not a separate screen. The Reports screen can preselect a patient using `/Provider/Reports?patientId={guid}`.

## Solution structure

- `Reflect.Domain/`: domain entities, repository interfaces, service interfaces, report data contracts, and enums; no MVC dependency.
- `Reflect.Web/Areas/Provider/Controllers/`: Provider Area endpoints and access checks.
- `Reflect.Web/Areas/Provider/Views/`: Razor screens and provider layout.
- `Reflect.Web/Models/ViewModels/`: screen-specific presentation and request models.
- `Reflect.Web/Repositories/`: synthetic in-memory repository implementations.
- `Reflect.Web/Services/`: report generation, PDF/CSV export, report signing, patient authorization, current-user, and summary services.
- `Reflect.Web/Standins/`: simulated clinician authentication, isolated from production integration.
- `Reflect.Web/wwwroot/`: locally served frontend scripts, styles, and assets.

Dependencies are registered in `Reflect.Web/Program.cs`. The `ClinicianOnly` policy requires an authenticated user with the `Clinician` role; patient-specific endpoints also use `IPatientAuthorizationService` to check access.

## Domain entities and interfaces

**Entities** in `Reflect.Domain/Entities/`:

- `BaseEntity`
- `Patient`
- `PatientSession`
- `ClinicalObservation`
- `MonitoringDraft`
- `ReportExport`
- `EntityNotFoundException` (domain exception)

**Repository interfaces** in `Reflect.Domain/Repositories/`:

- `IPatientRepository`
- `IPatientSessionRepository`
- `IPatientAccessRepository`
- `IClinicalObservationRepository`
- `IMonitoringDraftRepository`
- `IReportExportRepository`

**Service interfaces** in `Reflect.Domain/Services/`:

- `ICurrentUserService`
- `IPatientAuthorizationService`
- `ISummaryGenerationService`
- `IReportGenerationService`
- `IReportFileExportService`
- `IReportSigningService`
- `ISignedReportService`

Other domain contracts include `ReportContent`, `SignedReportManifest`, and the `ReportType` enum. These inventories describe the current solution; they are not a claim that every listed type was added in one commit.

## Reports and signed exports

The Reports screen generates a preview for a selected patient and report template, then supports standard PDF/CSV downloads or signed ZIP downloads. A signed package contains the report, `signature.json`, and `public-key.pem`.

Signing uses an application-level **RSA-PSS signature with SHA-256** over the report's exact bytes. The ZIP container itself is **not** signed: unpacking and repacking without changing the report bytes should not invalidate the signature, provided the verifier can locate the expected package files. Verification uses the server's trusted signing key, **not** a public key supplied by the uploaded ZIP. A changed report or invalid signature should fail verification.

In Development, when `ReportSigning:PrivateKeyPem` is absent, `ReportSigningService` creates a temporary 3072-bit RSA signing key. **No signing secret needs to be supplied to run the demo.** That key is regenerated when the app restarts, so packages created before a restart will not verify against the new key. In non-Development environments, the service requires `ReportSigning:PrivateKeyPem` when instantiated. Production integration must provide secure persistent key storage, key rotation/version retention, and an appropriate trust policy; the demo key must not be treated as a production identity.

## Data, security, and integration handoff

The demo is deliberately self-contained:

- In-memory repositories replace persistent application storage.
- `Reflect.Web/Standins/DemoClinicianAuthenticationExtensions.cs` injects a synthetic clinician identity. Replace this middleware with the host application's authentication and claims mapping.
- `CurrentUserService` and `PatientAuthorizationService` are integration points for the host's real clinician identity and patient access rules.
- Synthetic patient/session/observation records and template-generated summaries are demonstrations, not live clinical data or clinical decision support.
- Report export records are stored in memory. Production needs durable export/audit storage and production-appropriate retention and access controls.
- `Program.cs` defines a local Content Security Policy and cookie authentication setup; reconcile these with the host application's security configuration on merge.

Preserve the Provider Area routes and the Domain contracts as appropriate, but replace the stand-ins and in-memory implementations when integrating with Reflect XR's production infrastructure.


## Demo authentication and authorization

This standalone extension uses simulated clinician authentication, as permitted by the technical specification. `DemoClinicianAuthenticationExtensions` supplies a synthetic clinician identity with the `Clinician` role so the application can run independently of Reflect XR's production authentication infrastructure.

`CurrentUserService.ClinicianId` falls back to a predefined demo clinician ID when the current user identity is missing or invalid. This behavior is intentional for the standalone demo and is **not suitable for production authentication**.

Provider endpoints use the `ClinicianOnly` authorization policy, and patient-specific operations use `IPatientAuthorizationService` to enforce patient access-control checks. However, these checks operate within the demo identity model and should not be interpreted as production authentication guarantees.

**Production integration requirements:**
- Replace simulated authentication with the host application's authentication and clinician claims.
- Remove the demo clinician ID fallback and reject missing or invalid clinician identities.
- Connect patient authorization to the host application's actual clinician-patient access rules.
- Validate authorization behavior against the production identity and access-control system.

No changes to the demo authentication implementation are required to run the standalone demonstration.


## Additional packages

`Reflect.Web/Reflect.Web.csproj` declares **QuestPDF 2026.9.1** for PDF generation. `Reflect.Domain` has no explicit third-party NuGet package references. Frontend assets are served locally; check the repository's vendored assets before replacing or updating them. No remote CDN or external service is required for the Development demo.


## Unit tests

The `Reflect.Tests/` project contains automated unit tests covering:

- Patient access-control grants and denials.
- Patient-level authorization checks on selected controller endpoints.
- Report signing and digital signature verification.
- Detection of modified report contents.
- Rejection of invalid signatures, signing manifests, and untrusted signing keys.

The tests use synthetic data and do not require a database, external API, or production credentials.

Run the tests from the solution root:

```bash
dotnet restore
dotnet build
dotnet test
```

The test project is included in `ReflectProviderDashboard.slnx`.


## Development notes

Run `dotnet build` after changes. The supplied ZIP also contains local development artifacts and environment files; those are **not** needed as handoff source files. Avoid committing generated build outputs, local caches, cookies, test exports, private keys, or machine-specific directories. See `CHANGELOG.md` for the implementation and cleanup history.
