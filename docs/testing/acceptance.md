# Manual acceptance cases

These procedures cover proposed requirements and their current console, desktop, API, and WebClient realizations. Run them for review and record observations separately. See the [test plan](index.md) and [execution record](execution-record.md).

## Solution-wide acceptance criterion

Each NuGet package ID used as a direct dependency must have one declared version across all production and test projects in `Smurfs.sln`. Before accepting a baseline, compare the effective direct `PackageReference` versions, including centrally declared versions if used. Compare package IDs case-insensitively. Record any conflicting versions and the projects that declare them in the execution record. Acceptance requires zero conflicts.

## Isolated setup

Use PowerShell 7. For the console cases, build the app from the repository root and capture its absolute path. Stop if the build fails. The WPF case has its own build steps. Run these cases against a disposable local PostgreSQL instance. For these PostgreSQL cases, set `Database__Provider=Postgres` explicitly (the console default is SQLite). The console and WPF use their configured credentials at localhost:5432 and use `Smurfs_ConsoleApp` and `Smurfs_WPF` respectively. Start each data-dependent case with its target database absent on that test instance. The applications create and seed it.

```powershell
dotnet build src/Smurfs.UI.Console/Smurfs.UI.Console.csproj
if ($LASTEXITCODE -ne 0) { throw 'Console build failed.' }
$app = (Resolve-Path 'src/Smurfs.UI.Console/bin/Debug/net10.0/Smurfs.UI.Console.dll').Path
```

Before **each** case, run this from the same PowerShell session. Record the directory and test database name. The directory isolates file artifacts. PostgreSQL data is isolated by the test instance.

```powershell
$caseDir = Join-Path ([IO.Path]::GetTempPath()) ("smurfs-acceptance-" + [guid]::NewGuid())
New-Item -ItemType Directory -Path $caseDir -ErrorAction Stop | Out-Null
Push-Location $caseDir
```

Each `dotnet $app` call starts a new process. Read `$LASTEXITCODE` immediately after each call. After each case run `Pop-Location`. Keep the directory if evidence is needed. To reset, use another disposable instance or reset the target database on the disposable instance. Use a fresh directory for file artifacts.

## AT-001 Initialize and preserve a catalog

**Objective** — Verify explicit initialization and preservation of an operator-created record.

**References** — [Use cases and requirements](../_generated/traceability.md#at-001).

**Preconditions and data** — Fresh isolated directory. Use the name `Acceptance Init`.

1. Run `dotnet $app initdb`. Expect a readiness confirmation and exit code 0.
2. Run `dotnet $app createsmurf --name 'Acceptance Init'`. Expect creation confirmation and exit code 0.
3. Run `dotnet $app initdb` again. Expect readiness confirmation and exit code 0.
4. Run `dotnet $app listsmurfs --page 1 --page-size 50` and advance through the pages as needed. Expect exit code 0 and one row with the exact test name.

**Acceptance criteria** — Both initialization requests succeed and the previously saved record is still present once.

## AT-002 Create and read a Smurf in a later session

**Objective** — Verify successful creation at the proposed name-length boundary.

**References** — [Use cases and requirements](../_generated/traceability.md#at-002).

**Preconditions and data** — Fresh isolated directory initialized with `dotnet $app initdb` (exit code 0). Use `abcdefghijklmnopqrst` (20 ASCII characters).

1. Run `dotnet $app createsmurf --name abcdefghijklmnopqrst`. Expect exit code 0 and a creation confirmation naming the Smurf.
2. Run `dotnet $app listsmurfs --page 1 --page-size 50` and advance through the pages as needed. Expect exit code 0 and exactly one row with the submitted name.

**Acceptance criteria** — The boundary name is accepted and the exact record is visible in a later process.

## AT-003 Reject invalid names without adding rows

**Objective** — Verify both proposed invalid-name conditions and absence of a saved record.

**References** — [Use cases and requirements](../_generated/traceability.md#at-003).

**Preconditions and data** — Fresh isolated directory initialized with `dotnet $app initdb` (exit code 0). Use three spaces and `abcdefghijklmnopqrstu` (21 ASCII characters).

1. Run `dotnet $app listsmurfs --page 1 --page-size 50`. Expect exit code 0 and record the displayed total.
2. Run `dotnet $app createsmurf --name '   '`. Expect exit code 1 and a message that a name is required.
3. Run `dotnet $app createsmurf --name abcdefghijklmnopqrstu`. Expect exit code 1 and a message indicating the 20-character limit.
4. Repeat step 1. Expect the same total and no row for either rejected name.

**Acceptance criteria** — Both requests report the corresponding rejection and leave the catalog unchanged.

## AT-004 Page results and recover from invalid paging

**Objective** — Verify page selection, the empty result beyond the end, and recovery after invalid input.

**References** — [Use cases and requirements](../_generated/traceability.md#at-004).

**Preconditions and data** — Fresh isolated directory initialized with `dotnet $app initdb` (exit code 0). Create `Acceptance Page A` and `Acceptance Page B` using `createsmurf --name` and confirm exit code 0 for each.

1. Run `dotnet $app listsmurfs --page 1 --page-size 50`. Expect exit code 0, both test names and a total of at least two. Record the total as `$total` in PowerShell.
2. Run `dotnet $app listsmurfs --page 0 --page-size 1`, then `dotnet $app listsmurfs --page 1 --page-size 0`. Each should return exit code 1 and explain that paging values must be positive.
3. Run `dotnet $app listsmurfs --page 1 --page-size 1`, then `dotnet $app listsmurfs --page 2 --page-size 1`. Expect exit code 0 each time, the same overall total, and one distinct row on each page.
4. Run `dotnet $app listsmurfs --page ($total + 1) --page-size 1`. Expect exit code 0, the same overall total and no rows.

**Acceptance criteria** — Both invalid values are rejected, corrected requests succeed, and a positive page beyond the last row returns an empty page.

## AT-005 Browse adjacent pages in WPF

**Objective** — Verify the WPF page indicator and adjacent-page controls for the portion of FR-003 realized by WPF.

**References** — [Use case and requirement](../_generated/traceability.md#at-005).

**Preconditions and data** — Use Windows, PowerShell 7 and the selected .NET SDK. From the repository root, build WPF and capture its path before creating a fresh directory with the isolated setup above.

```powershell
dotnet build src/Smurfs.UI.WPF/Smurfs.UI.WPF.csproj
if ($LASTEXITCODE -ne 0) { throw 'WPF build failed.' }
$wpf = (Resolve-Path 'src/Smurfs.UI.WPF/bin/Debug/net10.0-windows/Smurfs.UI.WPF.exe').Path
```

1. On the disposable PostgreSQL instance with `Smurfs_WPF` absent, launch WPF with `& $wpf`. Startup creates and seeds the catalog. Expect the main window to show **No page loaded** with both navigation buttons disabled.
2. Select **Load smurfs**. With the current eight seed rows, expect **Page 1 of 1**, eight rows, and both navigation buttons disabled. For the remaining steps, use a console configured to the same disposable WPF catalog to add 20 rows named Acceptance WPF 01 through Acceptance WPF 20. Confirm each create command succeeds. This gives 28 rows; select **Load smurfs** again and expect **Page 1 of 2**, 25 rows, **Previous** disabled and **Next** enabled.
3. Select **Next**. Expect **Page 2 of 2**, three rows, **Next** disabled and **Previous** enabled.
4. Select **Previous** and expect **Page 1 of 2**. Select **Load smurfs** and expect **Page 1 of 2** again.
5. Close the WPF window and run `Pop-Location` in the setup shell.

**Acceptance criteria** — Page labels and navigation boundaries follow the displayed result. Restarting the list returns to page 1. The separate console case covers direct paging input, the total row count and nonpositive input handling.

## AT-006 Create a Smurf in WPF

**Objective** — Verify catalog feedback, cancel, and successful persistence through the WPF dialog.

**References** — [Use case and requirements](../_generated/traceability.md#at-006).

**Preconditions and data** — Use the WPF build and fresh directory setup in AT-005. Launch WPF with `& $wpf` with a fresh `Smurfs_WPF` catalog on the disposable instance. Use a blank or whitespace-only name, `abcdefghijklmnopqrstu` (21 characters), and `Acceptance WPF`.

1. Select **New Smurf**. Enter three spaces and confirm that no error appears and **Save** is enabled. Select **Save** and expect a required-name message while the dialog stays open. Replace the draft with the 21-character name. The previous message should clear and no new error should appear until **Save** is selected again. On that attempt, expect a length message and the dialog to stay open.
2. Enter `Acceptance WPF` and select **Cancel**. Select **Load smurfs** and verify the name is absent.
3. Open **New Smurf** again, enter `Acceptance WPF`, and rapidly double-click **Save**. Expect the dialog to close and exactly one matching name to appear on the refreshed page.
4. Close and reopen WPF in the same case directory. Select **Load smurfs** and verify that `Acceptance WPF` is present once. Close WPF and run `Pop-Location`.

**Acceptance criteria** — Validation messages appear only after a save attempt. Invalid drafts and cancellation add no rows. Saving a valid name adds one row visible after restarting WPF.


## AT-007 Diagnose frontend failures after exit

**Objective** — Verify that startup failures and unexpected list request failures remain diagnosable after each frontend exits.

**References** — [Diagnosability requirement](../requirements/index.md#sr-001-diagnose-failures-after-exit) and [logging design](../structural/index.md#application-logging).

**Preconditions and data** — Build both frontends, use absolute paths to their binaries, and record each scenario's start time. Use the disposable local PostgreSQL instance and a PostgreSQL client for the request-failure scenarios.

1. **Console startup failure.** Stop the disposable PostgreSQL instance. Run the console with `initdb`. Expect an error and exit code 1.
2. **WPF startup failure.** Keep the disposable PostgreSQL instance stopped. Launch WPF and dismiss the startup error. Expect exit code 1.
3. **Console request failure.** Restart the instance and run `initdb` successfully. In the disposable `Smurfs_ConsoleApp` database, temporarily rename the table with `ALTER TABLE "Smurfs" RENAME TO "Smurfs_TestHidden";`. Run `listsmurfs --page 1 --page-size 50` and expect an error and exit code 1. Restore the table with `ALTER TABLE "Smurfs_TestHidden" RENAME TO "Smurfs";`.
4. **WPF request failure.** Launch WPF and wait for the main window. Temporarily rename the table in the disposable `Smurfs_WPF` database using the same SQL. Select **Load smurfs**, observe the error, close WPF, and restore the table.
5. After each process exits, inspect its `console-*.log` or `wpf-*.log` under `%LOCALAPPDATA%\Smurfs\Logs`. Find the new entry for that run. Expect a startup failure for steps 1 and 2 and a failed list query for steps 3 and 4. Record the filename, timestamp, operation, and exception type in the [execution record](execution-record.md).

**Acceptance criteria** — All four scenarios leave a readable entry after exit. Each entry identifies the frontend by its log file, gives the failure time and failed operation, and includes the exception details.

## AT-008 List Smurfs through the HTTP API

Set `Database__Provider=Postgres` and `ConnectionStrings__Postgres` to a connection string for a fresh disposable PostgreSQL database. Ensure `DATABASE_URL` is absent so this case uses the supplied connection string. Start the API with the Development HTTPS profile. Use the request sequence in `src/Smurfs.API/Smurfs.API.http` or Postman.

1. Confirm startup prepares one PostgreSQL database with Identity and catalog tables.
2. Request `GET /api/smurfs?pageNumber=1&pageSize=2` without credentials. Expect 401.
3. Sign in as a seeded development user. Send the returned bearer token with the same GET. Expect 200, up to two Smurf summaries, and a `Pagination` header with the page number, page size, row count, and page count.
4. Send `pageNumber=0` with the bearer token. Expect 400. Correct the page number and expect 200.

For the hosted WebClient, reload `/smurfs` and confirm it loads the client. Request an unknown path such as `/api/typo` and expect 404. Request `GET /api/account` without a bearer token and expect 401.


## AT-009 Create, inspect, edit, and delete in the WebClient

**Scope** — Review [UC-002](../requirements/use-cases.md#uc-002-create-a-smurf), [UC-004](../requirements/use-cases.md#uc-004-inspect-a-smurf), [UC-005](../requirements/use-cases.md#uc-005-edit-a-smurf), and [UC-006](../requirements/use-cases.md#uc-006-delete-a-smurf). This is a planned manual case; record results separately.

**Setup** — Use a disposable API database, Development configuration, and the current production client bundle. Follow AT-008 for startup and authentication, then sign into the browser client.

1. Select Create Smurf. Enter Acceptance Web, health 42, birthday 2000-01-02, infection No, and gender Female. Submit. Expect one new row and return to /smurfs.
2. Open the row. Verify all values and reload its direct detail URL. Record the id and createdUtc from the authenticated GET response.
3. Open Edit. Change health to 12, clear birthday, and choose Unknown for infection and gender. Save. Verify updated detail values, explicit API nulls, and the unchanged id and createdUtc.
4. Attempt to save a blank name and then a 21-character name. Expect validation and unchanged stored values. Cancel a valid draft and confirm it was not saved.
5. Select Delete and decline confirmation. The record remains. Repeat and confirm deletion. Expect return to the overview, the record absent, and its direct GET returning 404.
6. Sign out and confirm protected requests return 401.

**Acceptance criteria** — Create and edit persist only valid values; nulls round-trip, cancellation preserves data, successful deletion removes the selected row, and authentication protects API operations.

## AT-010 Filter and sort before paging

**Scope** — Review [UC-003](../requirements/use-cases.md#uc-003-list-smurfs), including FR-007 and FR-008. Record results separately.

**Setup** — Use the disposable authenticated API/WebClient from AT-009. Create Alpha Older (infection Yes), Alpha Newer (No), and Beta Unknown (Unknown) in that order, recording identifiers and createdUtc values. Wait between creates so times differ.

1. Search for alpha with all infection boxes clear. Expect both Alpha rows, with page totals counting only these matches.
2. Select No and Search. Expect Alpha Newer only. Change to Yes and Search without changing the name field. Expect Alpha Older only.
3. Clear the name and select only Unspecified. Expect rows whose infection value is null, including Beta Unknown.
4. Use name=alpha, sorting=created, pageSize=1 in authenticated GET requests. Page 1 must contain Alpha Newer, page 2 Alpha Older, with totalCount 2. Switching sorting=name reverses that ordering for these names. A page beyond the results is empty with totalCount 2.
5. In the browser, select Newest first and Search. Verify descending creation timestamps. Switch to Name and Search, verifying the server's alphabetical order. Every Search starts a new result set at page 1.
6. Send infected=invalid and sorting=invalid separately. Expect 400. Correct them and expect 200.

**Acceptance criteria** — Filtering precedes counting and paging; sorting is global across pages, changed filter values take effect, and unknown infection is distinct from false.

## AT-011 API console login and paging

**Scope** — UC-003 and UC-007, FR-003, FR-010, and SR-002. Proposed human acceptance; record execution separately.

**Setup** — Run the API in Development against a disposable SQLite database with eight sample rows/accounts. Configure HTTPS with a trusted development certificate. Build the API console and capture its DLL as `$app`. Each `dotnet $app ...` below starts a separate process. Unset SMURFS_API_TOKEN initially and use a test API address so real saved logins are untouched.

1. Run `dotnet $app logout`, then `dotnet $app listsmurfs`. Expect logout exit 0, listing exit 1 with a login instruction and no prompt.
2. Run login with invalid credentials. Expect one error and exit 1, without another prompt. Login again as `bob@test.com` with Development password `Pa$$w0rd`; expect hidden password entry, confirmation, and exit 0.
3. Run `dotnet $app listsmurfs --page 1 --page-size 5`, then page 2 in another process. Expect five then three names, correct totals, and no credential/navigation prompts. Default page size is 25, matching the original console.
4. Change Api__BaseUrl to another isolated address; expect missing login. Restore the original and verify its saved login still works.
5. Restart the Development API without a stable TokenKey. Listing should exit 1 with a login instruction, without automatic login or retry. Re-login and verify listing works.
6. Use `login --email bob@test.com --password-stdin` with a protected password source. Expect no prompts and reuse by later processes. Redirected login without the explicit flag must fail promptly. Missing verb options/unknown commands exit 2; help exits 0.
7. Supply SMURFS_API_TOKEN from a protected source. Verify commands bypass saved credentials. Invalid overrides must not fall back to another account. Logout removes the saved entry and explains that the override remains active until unset; it does not revoke the server JWT.
8. Unset the override, logout, and verify later listing fails. Stop the API: logout still works locally; an authenticated list fails on transport. No password/token should appear in output or a plaintext token file.

**Evidence** — Record command options, endpoint, outcomes, and exit codes without secrets. Confirm test Credential Manager entries are removed. Saved login is Windows-specific; environment-token commands also work on other platforms. Use the execution-record template for human acceptance.

## AT-012 API console name-only creation

**Scope** — UC-002 and UC-003, FR-002, FR-004, and SR-002. Proposed human acceptance.

**Setup** — Use the isolated API from AT-011 and log in once. Record the initial count. Each operation runs in a separate process.

1. Run createsmurf without --name; expect syntax error/exit 2. Supply 21 name characters; expect server validation and exit 1 without a prompt or new row.
2. Run `createsmurf --name "Console New Smurf"`. Expect one confirmation and exit 0, without a list reload. List separately: count increases by one and another process finds the name alphabetically. API details show health zero, unknown optional fields, and server-generated ID/UTC timestamp.
3. Reject/expire the token before creation. Expect exit 1 with a login instruction, without automatic sign-in or resubmission. Logging in later must not replay the old name.
4. Using a controlled stub/proxy, drop the response after a completed create. Expect uncertainty feedback, exit 1, no false success, and exactly one POST. List separately to check the outcome before trying again.

**Evidence** — Record commands, counts, failure setup, and exit codes without credentials. Stopping the API before a request does not establish a lost response after commit.
