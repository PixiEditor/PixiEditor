$assemblyInfoPath = "PixiEditor\Properties\AssemblyInfo.cs"

$contents = [System.IO.File]::ReadAllText($assemblyInfoPath)

$versionString = [RegEx]::Match($contents,"(?:\d+\.\d+\.\d+\.\d+)")
$env:TagVersion = $versionString