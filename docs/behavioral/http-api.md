# HTTP API workflows

## Startup

The API migrates Identity and AppData in the same database using the configured SQLite or PostgreSQL provider, then seeds the catalog. In Development it also creates missing Bob, Jane and Tom sample accounts. Swagger UI is served at `/swagger` in all environments.

The API selects both context types from `Database:Provider` at startup. `Sqlite` uses `ConnectionStrings:Sqlite`. `Postgres` uses `DATABASE_URL` when supplied, requiring TLS, or `ConnectionStrings:Postgres` otherwise. Selection is independent of the host environment. Change the deployed setting and restart the API to switch providers without rebuilding. On Heroku, set `Database__Provider=Postgres`. In Development, an absent `TokenKey` generates a key for the current process. Sign in again after a restart if the API generated the key. A configured key of at least 64 UTF-8 bytes is required outside Development.

The API serves the production WebClient from `wwwroot`. Browser routes such as `/people` load `index.html`, and unmatched `/api` routes return 404.

## Sign in

Send email and password to `POST /api/account/login`. Identity checks the credentials and returns a user DTO with a `token` on success. The API signs and validates the JWT with HS512. Registration at `POST /api/account/register` also returns a token on success. Use the token from the Smurfs login response for requests to that API.

On startup, the WebClient uses its saved token to call `GET /api/account`. The API looks up the Identity account by the user ID in the token and returns 401 if the account is missing. If session restoration returns 401, the WebClient clears the saved token and continues signed out.

## List Smurfs

For [UC-003](../requirements/use-cases.md#uc-003-list-smurfs), send `GET /api/smurfs?pageNumber=1&pageSize=2` with the `Authorization: Bearer <token>` header. The API authenticates the request before dispatching the shared list query through MediatR. The read service projects the requested page into Smurf summaries.

- A valid request returns 200 with a JSON array and a `Pagination` header containing `currentPage`, `itemsPerPage`, `totalCount` and `totalPages`.
- A missing or invalid token returns 401.
- Nonpositive paging inputs return 400 for an authenticated caller. A page beyond the available rows returns an empty array with pagination metadata.

![Protected paging sequence](../_generated/diagrams/behavioral/diagrams/api-list.svg)

[PlantUML source](diagrams/api-list.puml)
