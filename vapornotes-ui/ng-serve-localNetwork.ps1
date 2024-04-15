#export const environment = { <...> }
$envPath = '.\src\environments\environment.localNetwork.ts'
$envContent = Get-Content $envPath -Raw

#-> <...>
$firstBraceIndex = $envContent.IndexOf('{')
$lastBraceIndex = $envContent.LastIndexOf('}')
$jsonLength = $lastBraceIndex - $firstBraceIndex + 1
$jsonEnvContent = $envContent.Substring($firstBraceIndex, $jsonLength)

#-> { apiBaseUrl: "http://<ip>:<port>" }
$environment = $jsonEnvContent | ConvertFrom-Json

# -> <ip>
$apiBaseUrlIp = ([System.Uri]$environment.apiBaseUrl).Host

$ngServeCommand = "ng serve --open --host $apiBaseUrlIp --disable-host-check --configuration localNetwork"

Invoke-Expression $ngServeCommand
