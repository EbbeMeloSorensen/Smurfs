# Smurfs API local baseline

In Development, the API generates a signing key at startup if `TokenKey` is absent. Sign in again after a restart. To keep local tokens valid across restarts, set `TokenKey` to a randomly generated signing secret of at least 64 UTF-8 bytes. A configured key is required outside Development. Identity and the catalog share a PostgreSQL database. Set `ConnectionStrings__DefaultConnection` to an Npgsql connection string for your local database.

Run `dotnet run --project src/Smurfs.API --launch-profile https` with a trusted local development certificate. Startup applies both sets of migrations and seeds the catalog before accepting requests.

The requests in `src/Smurfs.API/Smurfs.API.http` can also be sent from Postman. Register once, sign in, and send the returned `token` as a bearer token. Verify that `GET /api/smurfs?pageNumber=1&pageSize=2` returns 401 without the token and 200 with it. The authorized response includes the page as JSON and its metadata in the `Pagination` header.

In Development, startup also seeds Bob (`bob@test.com`), Jane (`jane@test.com`), and Tom (`tom@test.com`). Sign in with any of these email addresses and the sample password `Pa$$w0rd` to test the protected endpoint. Existing accounts with a matching user name or email are preserved.

## Deploy the API and WebClient to Heroku

The API serves the production React bundle from `wwwroot`. The bundle calls the API at the same origin under `/api`. Refresh the bundle after changing client code by running `npm ci` and `npm run build --ignore-scripts` in `src/Smurfs.WebClient`, then replacing the generated files in `src/Smurfs.API/wwwroot` with the contents of `build`. Commit the updated bundle.

1. Create a Heroku app and attach Heroku Postgres so the app receives `DATABASE_URL`. Set `ASPNETCORE_ENVIRONMENT` to `Production` and configure a stable `TokenKey` of at least 64 UTF-8 bytes as a Heroku config var. The API requires TLS for the database connection.
2. If using the Jincod .NET buildpack, configure it with `heroku buildpacks:set https://github.com/jincod/dotnetcore-buildpack --app YOUR_APP` and set `PROJECT_FILE=src/Smurfs.API/Smurfs.API.csproj` so the build selects the API project. Configure access to the Craft NuGet feed if the build environment needs it.
3. Add a Heroku remote from the existing checkout with `heroku git:remote --app YOUR_APP --remote heroku-smurfs`. Push the committed code with `git push heroku-smurfs HEAD:main`.
4. Open the app, register an account, and open Smurfs Overview. Refresh the page directly to verify the WebClient fallback and session restoration. Development sample accounts are only seeded in Development.
