<#
.SYNOPSIS
Adds randomly named Smurfs to the local SQLite database for paging tests.
.DESCRIPTION
Uses smurfs.db in the current directory. Initializes a missing database through
the console app, then inserts the requested rows in one transaction using Python 3.
.PARAMETER Count
Number of rows to append. Zero leaves the database alone.
.EXAMPLE
./scripts/add-random-smurfs.ps1 -Count 1000
#>
[CmdletBinding()]
param(
    [Parameter(Mandatory, Position = 0)]
    [ValidateRange(0, [int]::MaxValue)]
    [int]$Count
)

$ErrorActionPreference = 'Stop'
if ($Count -eq 0) {
    return
}

$python = Get-Command python -CommandType Application -ErrorAction Stop | Select-Object -First 1
if (-not (Test-Path -LiteralPath 'smurfs.db' -PathType Leaf)) {
    $project = Join-Path $PSScriptRoot '../src/Smurfs.UI.Console/Smurfs.UI.Console.csproj'
    dotnet run --project $project --no-launch-profile -- initdb
    if ($LASTEXITCODE -ne 0) {
        throw 'Database initialization failed.'
    }
}

@'
from contextlib import closing
from random import choice
from uuid import uuid4
import sqlite3
import sys

count = int(sys.argv[1])
adjectives = ("Bashful", "Bright", "Cheerful", "Curious", "Drowsy", "Jolly", "Merry", "Quiet")
hobbies = ("Baker", "Dancer", "Gardener", "Painter", "Poet", "Singer", "Tailor", "Writer")

# Reuse names so pages can exercise the identifier tie-breaker.
rows = (
    (str(uuid4()).upper(), f"{choice(adjectives)} {choice(hobbies)} Smurf", 0)
    for _ in range(count)
)
with closing(sqlite3.connect("file:smurfs.db?mode=rw", uri=True)) as database:
    with database:
        database.executemany("INSERT INTO Smurfs (Id, Name, Age) VALUES (?, ?, ?)", rows)

print(f"Added {count} Smurfs to smurfs.db.")
'@ | & $python.Source - $Count
if ($LASTEXITCODE -ne 0) {
    throw 'Could not add Smurfs. Run initdb if the database schema needs updating.'
}
