# WinForms browsing

WinForms initializes the app-data catalog in `babuska.db` before opening the main form. It uses the same working-directory database filename as WPF and Avalonia.

The form opens with no page loaded. **Load smurfs** opens page 1. **Previous** and **Next** navigate to adjacent pages when available. The form displays the current page, Smurf names, and load errors. The shared view model controls paging and disables commands while a request is in progress.

WinForms writes startup and application diagnostics through [shared file logging](../structural/index.md#application-logging).
