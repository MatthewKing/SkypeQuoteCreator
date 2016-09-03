Set-Alias Mage 'C:\Program Files (x86)\Microsoft SDKs\Windows\v10.0A\bin\NETFX 4.6 Tools\mage.exe'
Set-Alias MSBuild (Join-Path -Path (Get-ItemProperty "HKLM:\SOFTWARE\Microsoft\MSBuild\ToolsVersions\14.0").MSBuildToolsPath -ChildPath "MSBuild.exe")
Set-Alias SignTool 'C:\Program Files (x86)\Windows Kits\8.1\bin\x64\signtool.exe'

# Initialize the temporary directory
if ((Test-Path -path .\temporary)) {
  Remove-Item .\temporary\* -Recurse
} else {
  New-Item .\temporary -Type Directory
}
$temp = Resolve-Path .\temporary

# Initialize the publish directory
if ((Test-Path -path ..\publish) -eq $false) {
  New-Item ..\publish -Type Directory
}
$publish = Resolve-Path ..\publish

# Generate the version number using the git commit timestamp
$commitTimeUnix = git log -1 --format=%ct
$unixEpochUtc = New-Object -TypeName System.DateTime -ArgumentList 1970, 1, 1, 0, 0, 0, 1
$msbuildEpochUtc = New-Object -TypeName System.DateTime -ArgumentList 2000, 1, 1, 0, 0, 0, 1
$commitTimeUtc = $unixEpochUtc.AddSeconds($commitTimeUnix)
$daysPart = ($commitTimeUtc - $msbuildEpochUtc).Days
$secondsPart = [System.Convert]::ToInt64($commitTimeUtc.TimeOfDay.TotalSeconds / 2);
$version = "1.0.{0}.{1}" -f $daysPart, $secondsPart

# Build from source
MSBuild "/nologo" "/p:Configuration=Release,OutputPath=$temp,AssemblyVersion=$version,AssemblyFileVersion=$version,AssemblyInformationalVersion=$version" "..\source\SkypeQuoteCreator\SkypeQuoteCreator.csproj"

# Move the exe to the publish directory
Copy-Item "$temp\SkypeQuoteCreator.exe" "$publish\SkypeQuoteCreator.exe"

# Clean up the temporary directory
Remove-Item $temp -Recurse
