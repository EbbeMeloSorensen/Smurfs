# WinForms browsing

WinForms migrates and seeds the database selected by its settings before opening the main form.

## Database settings

The app loads `appsettings.json` beside its executable, independent of the working directory. The project copies this file into build and publish output. Set `Database:Provider` to `Postgres` or `Sqlite` and configure `ConnectionStrings:Postgres` or `ConnectionStrings:Sqlite` respectively. The checked-in default is PostgreSQL `Smurfs_WinForms`. SQLite defaults to `Data Source=smurfs-winforms.db` relative to the process working directory.

Edit the deployed settings file and restart WinForms to switch providers without rebuilding. Environment variables override file settings, for example `Database__Provider=Sqlite` and `ConnectionStrings__Sqlite=Data Source=smurfs-winforms.db`. Provider names are case-insensitive. Missing or unsupported provider settings and missing selected connection strings use the existing startup-error dialog and exit code 1.

## Browsing

The form opens with no page loaded. **Load smurfs** opens page 1. **Previous** and **Next** navigate to adjacent pages when available. The form displays the current page, Smurf names, and load errors. The shared view model controls paging and disables commands while a request is in progress.

WinForms writes startup and application diagnostics through [shared file logging](../structural/index.md#application-logging).
