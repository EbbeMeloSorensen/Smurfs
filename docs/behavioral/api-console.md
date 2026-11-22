# API console workflows

`Smurfs.UI.Console.API` realizes [UC-007 authentication](../requirements/use-cases.md#uc-007-authenticate-api-access), [UC-002 creation](../requirements/use-cases.md#uc-002-create-a-smurf), and [UC-003 listing](../requirements/use-cases.md#uc-003-list-smurfs). Like the direct-persistence console, it executes one command and exits. Login is retained between separate processes, rather than by a menu. Catalog commands never request credentials or other input. The server owns Identity, validation, and persistence.

## Setup and lifetime

Start `dotnet run --project src/Smurfs.API --launch-profile https` with a trusted development certificate, then `dotnet run --project src/Smurfs.UI.Console.API -- login` in another terminal. Development accounts include `bob@test.com` / `Pa$$w0rd`. Only the server initializes databases.

The client reads appsettings.json beside its executable. Api:BaseUrl defaults to `https://localhost:7231/`; Api__BaseUrl overrides it. An absolute HTTP(S) base URL must end in `/`, without embedded credentials, query, or fragment. Routes append api/account/login and api/smurfs; normally configure the server root. Use HTTPS remotely. Normal certificate checks remain enabled; redirects and cookies are disabled. Requests time out after 30 seconds.

Parsing precedes configuration/credential access. Help/version exit 0, invalid syntax exits 2, successes exit 0, and operation/authentication/credential-store failures exit 1. Login with --password-stdin but no --email is an option error (2). Results go to stdout; diagnostics and login prompts go to stderr. A process owns one HttpClient. There is no banner, menu, implicit login, automatic list reload, or retry loop. No rolling file logger is registered.

## Saved login and script authentication

Windows Credential Manager stores a generic credential under Smurfs.API/ plus the SHA-256 hash of the canonical API base URL. Scheme, host, port, and base path isolate entries; Uri normalizes equivalent host casing and default ports. Credentials belong to the current Windows user on this machine and persist across processes/logon sessions. The UTF-8 token is the secret blob. Passwords are never saved; Windows uses no plaintext token file. One account is saved per endpoint; successful login replaces it. Native-store failures are reported without plaintext fallback.

Linux uses `LinuxTokenStore` under an absolute `$XDG_CONFIG_HOME/smurfs-api`, or `~/.config/smurfs-api` when XDG_CONFIG_HOME is missing/relative. It uses the same canonical-address hash as the Windows key, with one `.token` file per endpoint. Files contain **unencrypted bearer tokens**, protected by owner-only Unix permissions: the directory is created as `0700` and files as `0600`. This is an intentional headless-compatible storage policy, not an encrypted keyring or a fallback after Windows credential failure. No desktop service or extra package is needed. The credential directory must be on a filesystem that honors Unix permissions. The application user and root can read these files; keep them out of shared directories and backups that should not contain credentials.

Reads, writes, and deletion reject symbolic links at the credential directory or entry, and reject group/other-accessible modes rather than silently repairing them. If modes were changed, restore `700` on the directory and `600` on the affected file before retrying. New files are created privately from the outset, flushed, and atomically renamed over the old entry; failed writes do not truncate the saved token. Missing reads/logout do not create a directory. Concurrent login/logout is last-writer-wins, not a server transaction. Windows credential names and existing saved logins are preserved.

SMURFS_API_TOKEN takes precedence over saved credentials for list/create. A blank override is ignored; an override is never persisted. This is the portable/unattended path: saved login/logout support Windows and Linux. Ordinary override-authenticated commands bypass credential storage. Obtain secrets from an existing protected source, avoid logging them, and unset the override when finished. A rejected override never falls back to another saved account.

Rejected/expired tokens exit 1 without prompting, refreshing, or replaying. The adapter clears its in-memory rejected token. The CLI leaves the saved entry untouched, avoiding interference with a concurrent login; login replaces it and logout explicitly deletes it. Development API restarts can invalidate tokens without a stable TokenKey. Local logout does not revoke the server JWT.

For repeated calls, build/publish once and run the executable or DLL directly:

```powershell
$app = (Resolve-Path 'src/Smurfs.UI.Console.API/bin/Debug/net10.0/Smurfs.UI.Console.API.dll').Path
dotnet $app login
dotnet $app createsmurf --name "Kylling"
if ($LASTEXITCODE -ne 0) { throw 'Creation failed or was not confirmed.' }
dotnet $app listsmurfs --page 1 --page-size 25
dotnet $app logout
```

These are separate processes sharing a saved token. List/create each perform one operation request, without an extra login or session-validation round trip. No execution-time threshold is claimed. See [SR-002](../requirements/index.md#sr-002-run-cli-operations-from-scripts).

## Linux usage

Install the .NET 10 SDK to build, or publish a self-contained executable for the target Linux architecture. From the solution root:

```bash
dotnet publish src/Smurfs.UI.Console.API -c Release -r linux-x64 --self-contained true -o ./artifacts/api-console-linux
export Api__BaseUrl='https://your-server/'
./artifacts/api-console-linux/Smurfs.UI.Console.API login
./artifacts/api-console-linux/Smurfs.UI.Console.API listsmurfs --page 1 --page-size 5
./artifacts/api-console-linux/Smurfs.UI.Console.API createsmurf --name Kylling
./artifacts/api-console-linux/Smurfs.UI.Console.API logout
```

Use `linux-arm64` for an ARM64 Linux machine. When copying a publish made on Windows, preserve/set executable permission on Linux with `chmod +x Smurfs.UI.Console.API`. A framework-dependent DLL can instead run with `dotnet Smurfs.UI.Console.API.dll` and the .NET 10 runtime. Publishing targets a Linux build; successful cross-publishing does not establish execution on Linux. Use HTTPS with a trusted certificate and a reachable API address; localhost refers to the Linux machine itself.

Login and `--email ... --password-stdin` work identically on both supported systems. For isolated Linux-native verification without a running API or keyring, use the .NET 10 SDK:

```bash
bash scripts/test-api-console-linux.sh
```

This runs the Linux file-permission/symlink tests plus shared HTTP/command tests. Linux-native cases are explicitly skipped on Windows; Windows-native cases are skipped on Linux. [AT-013](../testing/acceptance.md#at-013-linux-saved-login-and-permissions) describes cross-process acceptance. Never claim native Linux execution merely from a Windows build.

## Login

`login` prompts once for email and a hidden password, POSTs to api/account/login, saves the returned token, prints an account confirmation, and exits. `login --email bob@test.com` prompts only for password. Invalid credentials exit 1 without reprompting. Failed authentication preserves an earlier saved login; token-storage failure does not print success.

`login --email bob@test.com --password-stdin` reads one password line without prompting. There is no --password argument. Login without this flag fails immediately on redirected stdin. The adapter exports a token explicitly for secure storage; ApiUser retains display information only. Passwords/tokens are never printed. If the override is set, login still saves the new token and explains that commands currently prefer the override.

![API console login](../_generated/diagrams/behavioral/diagrams/api-console-login.svg)

Source: [api-console-login.puml](diagrams/api-console-login.puml).

## Logout

`logout` removes only the configured endpoint's saved token, without contacting the server. It succeeds when no entry exists. If SMURFS_API_TOKEN is set, it explains that the override remains active until unset in the caller's shell/script. It cannot alter the parent process environment or revoke a JWT remotely.

![API console logout](../_generated/diagrams/behavioral/diagrams/api-console-logout.svg)

Source: [api-console-logout.puml](diagrams/api-console-logout.puml).

## Browse pages

`listsmurfs --page 2 --page-size 5` prints names and page/totals, then exits. Defaults match the original console: page 1, size 25, capped at 50 by the application. Select another page through another invocation. Missing login fails without a request or prompt.

The existing List.Handler uses the HTTP ISmurfReadService. The adapter reads JSON items and the Pagination header, preserving server ordering and effective page size. Invalid metadata/JSON fails instead of inventing totals. The read contract supports filters/sorting, but the CLI does not expose those controls. HTTP status failures do not expose raw error bodies.

![API console listing](../_generated/diagrams/behavioral/diagrams/api-console-list.svg)

Source: [api-console-list.puml](diagrams/api-console-list.puml).

## Create a Smurf

`createsmurf --name "Kylling"` sends one authenticated POST containing only name, prints the original console's confirmation, and exits. There is no form, list reload, or client-generated ID. The server Create handler validates/commits: health defaults to zero, optional values to null, and ID/UTC timestamp are generated on the server.

A 400 prints a validation string or first structured problem-details error and exits 1. Correct the arguments and explicitly invoke again. Unexpected validation bodies use generic feedback. Authentication/forbidden errors also exit 1. Timeouts, interrupted responses, or unexpected HTTP failures may occur after commit: the CLI reports uncertainty, advises checking the list, and never retries automatically. No false success is printed. There are no idempotency keys or exactly-once guarantees.

![API console creation](../_generated/diagrams/behavioral/diagrams/api-console-create.svg)

Source: [api-console-create.puml](diagrams/api-console-create.puml). See [AT-012](../testing/acceptance.md#at-012-api-console-name-only-creation).

## Milestone boundary

No client migrations, edit/delete, registration, refresh tokens, or dataset upload is implemented. UnitOfWorkFactory, UnitOfWork, and SmurfRepository remain unimplemented/unregistered skeletons. Creation is a direct application operation with a server-owned commit; multiple HTTP requests cannot share an assumed transaction. Dataset restore/import needs a separate server contract for IDs, duplicates, replacement, and transaction semantics. See [architecture](../structural/index.md#http-api-client) and [AT-011](../testing/acceptance.md#at-011-api-console-login-and-paging).
