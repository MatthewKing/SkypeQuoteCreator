param (
  [string]$publisher = "MKing",
  [string]$product = "Skype Quote Creator",
  [string]$assemblyName = "SkypeQuoteCreator"
)

Set-Alias Mage 'C:\Program Files (x86)\Microsoft SDKs\Windows\v10.0A\bin\NETFX 4.6 Tools\mage.exe'
Set-Alias MSBuild (Join-Path -Path (Get-ItemProperty "HKLM:\SOFTWARE\Microsoft\MSBuild\ToolsVersions\14.0").MSBuildToolsPath -ChildPath "MSBuild.exe")
Set-Alias SignTool 'C:\Program Files (x86)\Windows Kits\8.1\bin\x64\signtool.exe'

$url = "http://mking.s3.amazonaws.com/$assemblyName.application"

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
$versionUnderscore = $version -Replace "\.", "_"

# Build from source
MSBuild "/nologo" "/p:Configuration=Release,OutputPath=$temp,AssemblyVersion=$version,AssemblyFileVersion=$version,AssemblyInformationalVersion=$version" "..\source\SkypeQuoteCreator\SkypeQuoteCreator.csproj"

# Bring the logo across, too; we need it for ClickOnce
Copy-Item "..\source\SkypeQuoteCreator\Icon.ico" "$temp\Icon.ico"

# Get rid of unwanted output files
Remove-Item $temp\* -include *.pdb,*.xml
Remove-Item $temp\* -include *.application,*.manifest

# Create the relevant deploy directories, and copy the files there.
$applicationFiles = Join-Path -Path $publish -ChildPath "Application Files"
if ((Test-Path -path $applicationFiles) -eq $false) {
  New-Item $applicationFiles -Type Directory
}
$deploy = Join-Path -Path $applicationFiles -ChildPath ("{0}_{1}" -f $assemblyName, $versionUnderscore)
if ((Test-Path -path $deploy)) {
  Remove-Item $deploy -Recurse
}
Copy-Item $temp $deploy -Recurse

# Generate the application manifest
$manifest = "$deploy\$assemblyName.exe.manifest"
mage -New Application -FromDirectory $deploy -ToFile $manifest -Version $version -Name $assemblyName -IconFile "Icon.ico" -Algorithm SHA256RSA

# Janitor the application manifest XML
[xml]$manifestXml = Get-Content $manifest
$manifestNs = New-Object System.Xml.XmlNamespaceManager($manifestXml.NameTable)
$manifestNs.AddNamespace("asmv1", "urn:schemas-microsoft-com:asm.v1")
$manifestNs.AddNamespace("asmv2", "urn:schemas-microsoft-com:asm.v2")
$manifestAssemblyIdentityNode =  $manifestXml.SelectSingleNode("/asmv1:assembly/asmv2:entryPoint/asmv2:assemblyIdentity", $manifestNs)
$manifestAssemblyIdentityNode.SetAttribute("name", $assemblyName)
$manifestAssemblyIdentityNode.SetAttribute("version", $version)
$manifestCommandLineNode =  $manifestXml.SelectSingleNode("/asmv1:assembly/asmv2:entryPoint/asmv2:commandLine", $manifestNs)
$manifestCommandLineNode.SetAttribute("file", "$assemblyName.exe")
$manifestXml.Save($manifest)

# Generate the deployment manifest
$application = "$deploy\$assemblyName.application"
$codeBase = "Application Files\{0}_{1}\{2}" -f $assemblyName, $versionUnderscore, "$assemblyName.exe.manifest"
mage -New Deployment -Version $version -Install true -ProviderUrl $url -AppManifest $manifest -AppCodeBase $codeBase -ToFile $application -Algorithm SHA256RSA

# Rename the files to have the .deploy extension
# (excluding the application manifest and the deployment manifest)
Get-ChildItem $deploy\*.* -Recurse -Exclude "$assemblyName.application","$assemblyName.exe.manifest" | Rename-Item -NewName { $_.Name + '.deploy' }

# Janitor the XML
# Trying to get it as close as possible to what VS used to output for this project
[xml]$xml = Get-Content $application
$ns = New-Object System.Xml.XmlNamespaceManager($xml.NameTable)
$ns.AddNamespace("asmv1", "urn:schemas-microsoft-com:asm.v1")
$ns.AddNamespace("asmv2", "urn:schemas-microsoft-com:asm.v2")
$ns.AddNamespace("co.v1", "urn:schemas-microsoft-com:clickonce.v1")
$ns.AddNamespace("co.v2", "urn:schemas-microsoft-com:clickonce.v2")
$assemblyIdentityNode = $xml.SelectSingleNode("/asmv1:assembly/asmv1:assemblyIdentity", $ns)
$assemblyIdentityNode.SetAttribute("name", "$assemblyName.application")
$descriptionNode = $xml.SelectSingleNode("/asmv1:assembly/asmv1:description", $ns)
$descriptionNode.SetAttribute("asmv2:publisher", "$publisher")
$descriptionNode.SetAttribute("asmv2:product", "$product")
$deploymentNode = $xml.SelectSingleNode("/asmv1:assembly/asmv2:deployment", $ns)
$deploymentNode.SetAttribute("mapFileExtensions", "true")
$subscriptionNode = $xml.SelectSingleNode("/asmv1:assembly/asmv2:deployment/asmv2:subscription", $ns)
$subscriptionNode.ParentNode.RemoveChild($subscriptionNode)
$frameworksNode = $xml.SelectSingleNode("/asmv1:assembly/co.v2:compatibleFrameworks", $ns)
$frameworkElementClient = $xml.CreateElement("framework", $frameworksNode.NamespaceURI)
$frameworkElementClient.SetAttribute("targetVersion", "4.0")
$frameworkElementClient.SetAttribute("profile", "Client")
$frameworkElementClient.SetAttribute("supportedRuntime", "4.0.30319")
$frameworkElementFull = $xml.CreateElement("framework", $frameworksNode.NamespaceURI)
$frameworkElementFull.SetAttribute("targetVersion", "4.0")
$frameworkElementFull.SetAttribute("profile", "Full")
$frameworkElementFull.SetAttribute("supportedRuntime", "4.0.30319")
$frameworksNode.RemoveAll()
$frameworksNode.AppendChild($frameworkElementClient)
$frameworksNode.AppendChild($frameworkElementFull);
$xml.Save($application)

# Copy the deployment manifest to the root deployment directory
Copy-Item "$deploy\$assemblyName.application" "$publish\$assemblyName.application"

# Clean up the temporary directory
Remove-Item $temp -Recurse
