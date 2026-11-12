# Manual acceptance cases

These procedures cover proposed requirements and their current console and WPF realizations. Run them for review and record observations separately. See the [test plan](index.md) and [execution record](execution-record.md).

## Solution-wide acceptance criterion

Each NuGet package ID used as a direct dependency must have one declared version across all production and test projects in `Smurfs.sln`. Before accepting a baseline, compare the effective direct `PackageReference` versions, including centrally declared versions if used. Compare package IDs case-insensitively. Record any conflicting versions and the projects that declare them in the execution record. Acceptance requires zero conflicts.

## Isolated setup

Use PowerShell 7. For the console cases, build the app from the repository root and capture its absolute path. Stop if the build fails. The WPF case has its own build and data preparation steps.

```powershell
dotnet build src/Smurfs.UI.Console/Smurfs.UI.Console.csproj
if ($LASTEXITCODE -ne 0) { throw 'Console build failed.' }
$app = (Resolve-Path 'src/Smurfs.UI.Console/bin/Debug/net10.0/Smurfs.UI.Console.dll').Path
```

Before **each** case, run this from the same PowerShell session. The fresh directory ensures the relative database path points to test data. Record the directory path.

```powershell
$caseDir = Join-Path ([IO.Path]::GetTempPath()) ("smurfs-acceptance-" + [guid]::NewGuid())
New-Item -ItemType Directory -Path $caseDir -ErrorAction Stop | Out-Null
Push-Location $caseDir
```

Each `dotnet $app` call starts a new process. Read `$LASTEXITCODE` immediately after each call. After each case run `Pop-Location`. Keep the directory if evidence is needed. To reset, create another fresh directory using the setup above. No database deletion is needed.

## AT-001 Initialize and preserve a catalog

**Objective** — Verify explicit initialization and preservation of an operator-created record.

**References** — [Use cases and requirements](../_generated/traceability.md#at-001).

**Preconditions and data** — Fresh isolated directory. Use the name `Acceptance Init`.

1. Run `dotnet $app initdb`. Expect a readiness confirmation and exit code 0.
2. Run `dotnet $app createsmurf --name 'Acceptance Init'`. Expect creation confirmation and exit code 0.
3. Run `dotnet $app initdb` again. Expect readiness confirmation and exit code 0.
4. Run `dotnet $app listsmurfs --page 1 --page-size 50`. Expect exit code 0 and one row with the exact test name.

**Acceptance criteria** — Both initialization requests succeed and the previously saved record is still present once.

## AT-002 Create and read a Smurf in a later session

**Objective** — Verify successful creation at the proposed name-length boundary.

**References** — [Use cases and requirements](../_generated/traceability.md#at-002).

**Preconditions and data** — Fresh isolated directory initialized with `dotnet $app initdb` (exit code 0). Use `abcdefghijklmnopqrst` (20 ASCII characters).

1. Run `dotnet $app createsmurf --name abcdefghijklmnopqrst`. Expect exit code 0 and a creation confirmation naming the Smurf.
2. Run `dotnet $app listsmurfs --page 1 --page-size 50`. Expect exit code 0 and exactly one row with the submitted name.

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

**Preconditions and data** — Use Windows, PowerShell 7, Python 3 and the selected .NET SDK. From the repository root, build WPF and capture the two paths before creating a fresh directory with the isolated setup above.

```powershell
dotnet build src/Smurfs.UI.WPF/Smurfs.UI.WPF.csproj
if ($LASTEXITCODE -ne 0) { throw 'WPF build failed.' }
$wpf = (Resolve-Path 'src/Smurfs.UI.WPF/bin/Debug/net10.0-windows/Smurfs.UI.WPF.exe').Path
$seed = (Resolve-Path 'scripts/add-random-smurfs.ps1').Path
```

1. In the fresh case directory, run `& $seed -Count 51`. Once it completes, run `Copy-Item -LiteralPath smurfs.db -Destination babuska.db` and launch WPF with `& $wpf`. Expect the splash to give way to one main window showing **No page loaded**. Both navigation buttons should be disabled.
2. Select **Load smurfs**. Expect **Page 1 of 3**, 25 rows, **Previous** disabled and **Next** enabled.
3. Select **Next** twice. Expect **Page 2 of 3**, then **Page 3 of 3** with at least one row. On the last page expect **Next** disabled and **Previous** enabled.
4. Select **Previous** and expect **Page 2 of 3**. Select **Load smurfs** and expect **Page 1 of 3** again.
5. Close the WPF window and run `Pop-Location` in the setup shell.

**Acceptance criteria** — Page labels and navigation boundaries follow the displayed result. Restarting the list returns to page 1. The separate console case covers direct paging input, the total row count and nonpositive input handling.

## AT-006 Create a Smurf in WPF

**Objective** — Verify catalog feedback, cancel, and successful persistence through the WPF dialog.

**References** — [Use case and requirements](../_generated/traceability.md#at-006).

**Preconditions and data** — Use the WPF build and fresh directory setup in AT-005. Launch WPF with `& $wpf` without copying a database. Use a blank or whitespace-only name, `abcdefghijklmnopqrstu` (21 characters), and `Acceptance WPF`.

1. Select **New Smurf**. Enter three spaces and confirm that no error appears and **Save** is enabled. Select **Save** and expect a required-name message while the dialog stays open. Replace the draft with the 21-character name. The previous message should clear and no new error should appear until **Save** is selected again. On that attempt, expect a length message and the dialog to stay open.
2. Enter `Acceptance WPF` and select **Cancel**. Select **Load smurfs** and verify the name is absent.
3. Open **New Smurf** again, enter `Acceptance WPF`, and select **Save**. Expect the dialog to close and the name to appear on the refreshed page.
4. Close and reopen WPF in the same case directory. Select **Load smurfs** and verify that `Acceptance WPF` is present once. Close WPF and run `Pop-Location`.

**Acceptance criteria** — Validation messages appear only after a save attempt. Invalid drafts and cancellation add no rows. Saving a valid name adds one row visible after restarting WPF.
