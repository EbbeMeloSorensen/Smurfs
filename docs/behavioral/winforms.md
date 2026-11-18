# WinForms browsing

WinForms migrates and seeds its local PostgreSQL database `Smurfs_WinForms` before opening the main form.

The form opens with no page loaded. **Load smurfs** opens page 1. **Previous** and **Next** navigate to adjacent pages when available. The form displays the current page, Smurf names, and load errors. The shared view model controls paging and disables commands while a request is in progress.

WinForms writes startup and application diagnostics through [shared file logging](../structural/index.md#application-logging).
