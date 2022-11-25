$assemblyInfoPath = "src\PixiEditor\Properties\AssemblyInfo.cs"

$contents = [System.IO.File]::ReadAllText($assemblyInfoPath)

$versionString = [RegEx]::Match($contents,"(?:\d+\.\d+\.\d+\.\d+)")
Write-Host "##vso[task.setvariable variable=TagVersion;]$versionString"