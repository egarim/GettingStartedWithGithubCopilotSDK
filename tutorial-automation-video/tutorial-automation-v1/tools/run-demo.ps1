[CmdletBinding()]
param(
    [Parameter(Mandatory=$true)][string]$Workspace
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$Workspace = (Resolve-Path $Workspace).Path
Push-Location $Workspace
try {
    dotnet run
}
finally {
    Pop-Location
}
