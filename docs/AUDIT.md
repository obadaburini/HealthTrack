# HealthTrack maintenance audit

Audit date: 19 October 2026. Work is isolated on `codex/healthtrack-improvements`.

## Original architecture and functionality

The repository is a single ASP.NET Core MVC application targeting `net8.0`. Controllers use an EF Core SQL Server context directly; Razor renders the UI. Login stores the username and role in server-side session. Bootstrap/jQuery are bundled for shared template pages; most clinical pages have their own styling and JavaScript. The original EF Core SqlServer/Tools packages were 9.0.0.

The database context exposes User, PatientUser, Patient, BloodSugarRecords, GeneralCheckupRecords, HeartRecords, and AppointmentRecords. Twelve existing migrations describe the SQL Server schema. Models/Clinic.cs and Models/Appointment.cs remain legacy/unmapped classes. Account and patient-profile tables duplicate username/password data, and clinical records refer to usernames rather than account IDs. That structure was retained to avoid an unrelated data-model rewrite.

Implemented features are patient registration; patient and doctor login; patient/doctor dashboards; blood sugar, heart vitals, and general checkup records with edit/delete; personal appointments; nutrition estimates and tips; a doctor patient directory, classification, detailed histories, and reports. Email delivery, account recovery, external appointment booking, and per-doctor patient assignment are not implemented.

## Findings and resolutions

| Finding | Resolution |
| --- | --- |
| Solution referenced nonexistent `HelthTrack/HelthTrack.csproj` | Corrected to the root project; solution now also contains the security test project. |
| No .gitignore; 222 tracked Visual Studio/build files | Added ASP.NET/Visual Studio ignore rules and removed only generated/IDE files from the Git index. Source, bundled browser libraries, and all migrations remain. |
| Plaintext passwords stored twice and queried directly at login | ASP.NET Core PasswordHasher with salted hashes; new registrations hash both copies; successful legacy login upgrades both. Explicit transactional bulk upgrade provided. |
| Missing role checks on doctor deletion and some patient pages | Central controller-wide session role filters cover every doctor/patient action, including GETs and POSTs. Existing patient record ownership checks are retained and smoke-tested. |
| Inconsistent CSRF validation; GET logout | Global automatic antiforgery validation; each POST form has a token; logout is POST-only. Role selection uses buttons instead of clickable divs. |
| Registration bound the persistence entity and bypassed model validation | Dedicated validated registration/login view models; password confirmation, email, lengths, DOB, username and allowed selections checked server-side. Patient registration is transactional. |
| Health-reading POSTs accepted malformed/nonpositive values and dates | Entry ranges, valid dates, blood pressure ordering, description length and allowed clinic/patient selections validated on the server; rejected readings show feedback. Clinical feedback thresholds/formulas are unchanged. |
| No account request limit or safe DB error screen | Per-IP account rate limit, HTTP 429 with retry guidance, safe HTTP 503 database view, private-page no-store responses, explicit memory session backing. |
| Doctor delete view did not load the linked profile | Includes PatientUser so the confirmation shows the correct synthetic/tested patient name. |
| Duplicate patient-directory entries possible through the form | Check for an existing patient link before adding and report a validation error. No unique-index migration was introduced. |
| Nutrition stylesheet URL used wrong case | All references match `Nutrition.css`; verified in a real browser. |
| Repeated inline style blocks, inconsistent mobile metadata and labels | Extracted 22 page style blocks to versioned stylesheets; shared responsive/accessibility styles, associated form labels, keyboard role buttons, mobile viewport metadata and reduced-motion handling. Existing screen structure is preserved. |
| Generated template home/privacy pages and inconsistent product spelling | Useful product/data pages and consistent displayed HealthTrack branding; original project filenames/namespaces retained. |
| Old EF patch and no pinned local CLI/test suite | EF remains major version 9, updated to 9.0.21 from official NuGet metadata; pinned local EF tool, seven security/migration regression tests. |
| No README/screenshots | Added setup, configuration, migration, account-maintenance, run/test instructions and 14 reviewed screenshots. |

## Necessary migration compatibility repair

Fresh SQL Server migration testing exposed a pre-existing failure: `20260105065419_RenameDiseaseToPatientType` renames Disease to PatientType, then `20260105132022_AddPatientTypeToPatient` tries to add PatientType again. SQL Server rejects the duplicate column.

Only the latter migration's implementation was repaired: its Up now checks COL_LENGTH before adding; its Down leaves the earlier migration's column intact. Adding a later migration could not resolve an earlier migration failure. All original migration IDs, designer files, snapshots, persisted entities, and database provider remain intact. No new migration is added and no database reset is performed.

All 12 migrations applied successfully to a new isolated SQL Server LocalDB audit database, and EF reports no pending model changes. The operator's existing HealthTrackDb was not used or modified during validation. Databases that have already recorded the final migration do not reapply it. Back up an existing database and review the selected connection before deployment.

## Files changed

- Build/configuration: `.gitignore`, `.config/dotnet-tools.json`, `HelthTrack.sln`, `HelthTrack.csproj`, `Program.cs`.
- Backend: account, doctor and patient controllers; new Filters/SessionRoleAttribute.cs and DatabaseExceptionFilter.cs; new Services/PasswordService.cs and AccountMaintenance.cs.
- Validation: new LoginVM.cs and RegisterPatientVM.cs; allowed-value validation on EditPatientVM.cs and NutritionInput.cs.
- Database: conditional compatibility fix in the final existing migration only. Models/ and Data/ are unchanged.
- UI: account/doctor/patient views, Home/Index, Home/Privacy, shared layout and service-unavailable view; new shared healthtrack.css and 22 page CSS files.
- Documentation/tests: README.md, this audit, docs/screenshots/*.png and tests/HealthTrack.Tests/.
- Git removals: `.vs/`, `bin/`, `obj/`, `HelthTrack.csproj.user` (222 files). Removal was from tracking only, not a recursive source deletion.

## Validation results

| Check | Result |
| --- | --- |
| dotnet restore HelthTrack.sln | Passed |
| dotnet build HelthTrack.sln --no-restore | Passed: 0 warnings, 0 errors |
| dotnet test HelthTrack.sln --no-build | Passed: 7 tests |
| NuGet vulnerability audit including transitive dependencies | No known vulnerable packages reported for app or test project by configured NuGet sources |
| Pinned dotnet-ef restore | Passed |
| EF database update against isolated SQL Server LocalDB | All 12 existing migrations passed |
| EF has-pending-model-changes | No changes |
| HTTPS runtime startup | Passed at localhost:7105; HTTP companion at localhost:5122 |
| Browser patient workflows | Registration, hash storage, login, create/edit/invalid readings, appointments and nutrition passed |
| Browser doctor workflows | Legacy password upgrade, directory add, details, reports, populated delete confirmation and deletion passed |
| Browser security checks | Missing CSRF token rejected; unauthenticated/wrong-role requests blocked; cross-patient read/delete rejected |
| Operator maintenance commands | Doctor provisioning and bulk password upgrade ran successfully against isolated audit DB only |
| Mobile rendering | Patient dashboard, nutrition and general checkup fit 390px viewport without document overflow |
| Browser runtime/stylesheets | Final smoke pass had no JavaScript errors, server failures or failed stylesheet requests |
| Screenshots/README references | 14 synthetic-data PNGs reviewed; all README paths resolve; password fields empty in captures |
| Credential scan | No credential signatures or connection-string passwords found in candidate source/config files; secrets/generated paths ignored |

Browser testing used installed Chrome via Playwright with a fresh browser context, a new LocalDB audit database, synthetic users, and random in-memory test passwords. Temporary synthetic account/record rows were removed afterward. No real patient database or account was used. The temporary capture harness and browser dependencies are not committed to the application.

## Configuration/security boundaries

The checked-in connection uses Windows integrated authentication and contains no password. Deployment-specific credentials belong in user secrets or a secret-backed environment variable, not Git. HTTPS/certificate validation, AllowedHosts and proxy configuration need deployment-specific setup. The local development connection and TrustServerCertificate option are not a production database security configuration.

Password upgrades are data operations, not schema changes. Legacy rows remain in their prior format until successful login or explicit bulk upgrade; operators should run the bulk command on an existing installation before reopening it. Keep backups protected because older backups may contain legacy credentials. No hash values or passwords are printed by the maintenance commands.

The existing session-based architecture and shared doctor directory are retained. Sessions and rate limiting are process-local; multi-instance hosting needs shared storage. Persistent account/profile duplication and username-based clinical relations are documented follow-up design work, not silently changed here. Older default/missing DOB values need correction by an administrator. Medical/nutrition guidance remains the existing application logic and was not clinically validated by this maintenance work.

Removing generated files from the branch does not purge historical Git objects. Historical contents were not rewritten. No merge to main is performed.
