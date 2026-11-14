# Smurfs API local baseline

In Development, the API generates a signing key at startup if `TokenKey` is absent. Sign in again after a restart. To keep local tokens valid across restarts, set `TokenKey` to a randomly generated signing secret of at least 64 UTF-8 bytes. A configured key is required outside Development. The default SQLite files are `Smurfs_API_Identity.db` and `Smurfs_API_AppData.db`. Override their paths with `ConnectionStrings__Identity` and `ConnectionStrings__AppData` if needed.

Run `dotnet run --project src/Smurfs.API --launch-profile https` with a trusted local development certificate. Startup migrates both databases and seeds the catalog before accepting requests.

The requests in `src/Smurfs.API/Smurfs.API.http` can also be sent from Postman. Register once, sign in, and send the returned `token` as a bearer token. Verify that `GET /api/smurfs?pageNumber=1&pageSize=2` returns 401 without the token and 200 with it. The authorized response includes the page as JSON and its metadata in the `Pagination` header.
