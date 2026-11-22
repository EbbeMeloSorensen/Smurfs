# API console workflows

`Smurfs.UI.Console.API` realizes [UC-007 authentication](../requirements/use-cases.md#uc-007-authenticate-api-access), [UC-002 creation](../requirements/use-cases.md#uc-002-create-a-smurf), and [UC-003 listing](../requirements/use-cases.md#uc-003-list-smurfs) through a running Smurfs.API. It is separate from the direct-persistence console. The HTTP adapter implements `ISmurfReadService`; the console constructs the existing `List.Handler` with it. The server performs authorization, filtering, ordering, paging, and persistence.

## Setup and lifetime

Start `dotnet run --project src/Smurfs.API --launch-profile https` with a trusted development certificate, then run `dotnet run --project src/Smurfs.UI.Console.API` in another terminal. Development sample accounts include `bob@test.com` with password `Pa$$w0rd`; existing registered accounts also work. Only the server initializes/seeds databases.

The client reads `appsettings.json` beside its executable. `Api:BaseUrl` defaults to `https://localhost:7231/`; environment variable `Api__BaseUrl` overrides it. The absolute HTTP(S) address must end in `/` and contain no embedded credentials, query, or fragment. Routes append `api/account/login` and `api/smurfs`; normally configure the server root. HTTP is suitable for local development; use HTTPS remotely. The client does not bypass certificate validation, follow redirects, or use authentication cookies. Requests time out after 30 seconds.

One HttpClient and one SmurfsApiClient live for the console session. The adapter is not intended for concurrent calls or sharing between users. The token exists only in private memory; no token file, password setting, or command-line credential option is used. Interactive password entry is hidden. Redirected input uses one line per prompt, which is useful for automated checks but must be handled as sensitive input. The console has no rolling file logger in this milestone and prints concise errors to stderr. EOF or blank email exits with code 0; unrecovered configuration, transport, protocol, or non-authentication HTTP errors during login/listing exit with code 1. Create failures are handled as described below. Ordinary quit exits with code 0.

## Login

Enter email and password. The adapter clears any previous token before POSTing credentials to `/api/account/login`. A valid response supplies account display information and the new private token. A 401 reports a sign-in failure and prompts again. Malformed/incomplete responses fail without establishing a session. Raw sign-in error response bodies and tokens are never printed. An API restart can invalidate Development tokens; sign in again when needed.

![API console login](../_generated/diagrams/behavioral/diagrams/api-console-login.svg)

Source: [api-console-login.puml](diagrams/api-console-login.puml).

## Browse pages

Successful sign-in immediately requests page 1 with page size 5. The console displays names, current page, total pages, and matching row count. Use `n`/`next`, `p`/`previous`, `r`/`reload`, `c`/`create`, `l`/`login`, and `q`/`quit`. Next/previous stay within known page bounds. Reload requests the current page. Signing in again resets browsing to page 1. Invalid commands display guidance and reload the current page.

Each read sends a bearer header on that request only. The adapter reads the item array and the server's `Pagination` header, honoring the server's effective page size (the API caps it at 50). Missing/malformed metadata or invalid JSON is reported instead of inventing totals. The read contract also forwards encoded name/infection filters and ordering, although the console has no filter/sort controls. The API performs filtering/sorting before paging; the adapter does not reorder results or fetch the whole catalog. A 401 clears the token and returns to the login prompt. Other HTTP errors retain their status in ApiException without exposing raw server details.

![API console browsing](../_generated/diagrams/behavioral/diagrams/api-console-list.svg)

Source: [api-console-list.puml](diagrams/api-console-list.puml).

## Create a Smurf

Choose `c`/`create` and enter a name. This realizes [UC-002](../requirements/use-cases.md#uc-002-create-a-smurf). Blank/whitespace name cancels and returns to browsing; EOF exits. The adapter's CreateSmurfAsync sends one authenticated `POST /api/smurfs` containing only `name`, requesting a JSON response. The existing server Create handler validates and commits it. The server generates the ID and UTC timestamp; health defaults to zero, and Birthday, Infected, Gender, and PetId remain null. The client does not construct a domain entity or invoke its local write-handler/repository skeletons.

A successful response produces a confirmation and reloads the current page. The list is alphabetical, so a newly created Smurf may appear on a different page. A 400 displays the API's validation string or the first structured problem-details error and asks for a corrected name. Unexpected validation bodies use generic feedback. A 401 clears the token and returns to login; the name is not automatically resubmitted after sign-in. A 403 displays a denial and returns to browsing.

Timeouts, connection errors, and interrupted responses can occur after the API has saved the Smurf. The console reports that creation is unconfirmed, advises checking the list before trying again, and never retries the POST automatically. Unexpected HTTP failures also display an unconfirmed outcome with the status, without raw server error bodies. The console attempts to reload the current page afterward; if the API remains unavailable, the normal listing error exits with code 1. Quitting after a handled create failure exits with code 0. Cancelling a draft sends no POST. This baseline does not provide idempotency keys or exactly-once delivery.

![API console creation](../_generated/diagrams/behavioral/diagrams/api-console-create.svg)

Source: [api-console-create.puml](diagrams/api-console-create.puml). See [AT-012](../testing/acceptance.md#at-012-api-console-name-only-creation) for manual verification.

## Milestone boundary

There are no client-side migrations, local repositories, edit/delete commands, registration flow, refresh-token mechanism, or stored sessions. The original UnitOfWorkFactory, UnitOfWork, and SmurfRepository classes in Smurfs.Persistence.API remain explicitly documented, unimplemented skeletons and are not constructed by this frontend. Name-only creation is a direct HTTP application operation with a server-owned commit. A later repository/unit-of-work milestone would need an explicit commit policy; multiple requests cannot be assumed to share a transaction. See the [HTTP adapter architecture](../structural/index.md#http-api-client) and [manual acceptance case](../testing/acceptance.md#at-011-api-console-login-and-paging).
