# WPF workflows

## Application lifetime

This sequence covers launching the application, leaving it idle and closing the main window. The splash remains on the UI thread while a worker uses the shared app-data initializer to migrate and seed `babuska.db` in the current working directory. Startup disposes the initialization scope before showing the main window. On exit, WPF disposes the host. The first list request is an operator action described under [Browse Smurfs](#browse-smurfs).

![WPF application lifetime sequence](../_generated/diagrams/behavioral/diagrams/wpf-lifetime.svg)

[PlantUML source](diagrams/wpf-lifetime.puml)

### Startup failure

If startup fails, WPF shows an error and shuts down with exit code 1. The host is disposed if it was built.

![WPF startup failure sequence](../_generated/diagrams/behavioral/diagrams/wpf-startup-failure.svg)

[PlantUML source](diagrams/wpf-startup-failure.puml)

## Create a Smurf

**New Smurf** opens an owned modal dialog in its own scope. Editing the draft clears any prior message. **Save** checks the draft through the catalog and displays any name error. A valid draft is submitted through the same create command as the console. Saving, editing, and dismissal are disabled while a request is in progress. A failure keeps the draft open and allows another attempt. A successful request disables further submissions from that dialog. On success, the dialog closes and the list refreshes the displayed page, or page 1 if none has been loaded. **Cancel** discards the draft. This realizes [UC-002 Create a Smurf](../requirements/use-cases.md#uc-002-create-a-smurf).

![WPF create sequence](../_generated/diagrams/behavioral/diagrams/wpf-create.svg)

[PlantUML source](diagrams/wpf-create.puml)

## Browse Smurfs

**Load smurfs** requests page 1. **Previous** and **Next** request adjacent pages of 25 rows through the same query as the console. The UI disables navigation at the corresponding boundary and while a request is in progress. Success shows the current page and total pages, or **No pages** for an empty catalog. If the request fails or the requested page disappears, WPF retains the displayed rows and page indicator and shows an error. This is the WPF realization of adjacent-page browsing in [UC-003 List Smurfs](../requirements/use-cases.md#uc-003-list-smurfs).

![WPF browse sequence](../_generated/diagrams/behavioral/diagrams/wpf-browse.svg)

[PlantUML source](diagrams/wpf-browse.puml)
