# Manual acceptance cases

These procedures express expected results from proposed console requirements. Run them for review and record observations separately. See the [test plan](index.md) and [execution record](execution-record.md).

## Isolated setup

Use PowerShell 7. From the repository root, build the app and capture its absolute path. Stop if the build fails.

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
