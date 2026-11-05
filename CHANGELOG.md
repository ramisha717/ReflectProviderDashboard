# Changelog

Notable changes to the Reflect XR Provider Dashboard extension are documented here. This is a summary of delivered functionality and project-level changes, not a record of every intermediate edit.

## Unreleased — November 2026

### Added

- Built the standalone ASP.NET Core MVC Provider Dashboard on .NET 10, with Provider-area Razor views, shared layout, client-side JavaScript, and navigation.
- Implemented provider-facing workflows for overview and attention review; patient listing and patient-specific session views; session history and details; clinical response; monitoring documentation; progress and platform comparison; reports; and settings.
- Added domain entities for patients, patient sessions, clinical observations, monitoring drafts, and report exports, together with repository interfaces and in-memory implementations using synthetic demo data.
- Added application services and view models for patient access, report generation, report exports, and template-based summaries.
- Added PDF and CSV report generation and a recent-export history backed by the demo report-export repository.
- Added optional application-level signed report packages for PDF and CSV exports, using RSA-PSS with SHA-256. Each signed ZIP contains the report, `signature.json`, and a public-key PEM file.
- Added server-side verification of signed report packages using the application's trusted signing key, rather than trusting a public key supplied by the uploaded ZIP.
- Added demo clinician authentication, a clinician-only authorization policy, and patient-access checks for protected workflows.
- Added a Content Security Policy in the ASP.NET Core request pipeline.

### Changed

- Organized the implementation into `Reflect.Domain` (entities and contracts) and `Reflect.Web` (MVC application, services, repositories, and demo integrations).
- Updated the Reports interface to support patient selection, automatic selection from the `patientId` query parameter, preview generation, regular exports, signed exports, and ZIP verification.
- Updated signed-ZIP verification to allow a common enclosing folder when an archive is unpacked and repackaged, while continuing to verify the unchanged report bytes and reject invalid signatures.
- Configured the report-signing service to generate an ephemeral RSA key in Development when no private key is supplied, so the demo can run without external signing credentials.
- Added QuestPDF as the PDF-generation dependency.

### Fixed

- Restored the complete Reports page JavaScript controller after a partial replacement prevented patient loading and report preview interactions.
- Corrected the separation of report-signing and signed-package services, including use of the shared `SignedReportManifest` domain type.

### Removed / cleaned up

- Removed unused starter-template scaffolding and dead code during development, retaining the MVC infrastructure needed by the demo. Individual deleted files are not enumerated here because the final source snapshot alone does not establish their deletion history.

### Known demo limitations

- Demo data and export history are held in memory and are not persisted across application restarts.
- Demo clinician identity and authorization stand-ins must be replaced or integrated with production authentication and patient-access services.
- Unless a persistent private key is configured, the Development signing key changes on application restart; signatures from a previous run will then fail verification.
- The signing feature demonstrates application-level authenticity and integrity. Production deployment requires managed key storage, rotation, and a trusted verification-key distribution strategy.

