Write-Host "### dotnet clean... ###"
dotnet clean .\src\HeboTech.GsmApi\HeboTech.GsmApi.csproj -c Release

Write-Host "### dotnet build... ###"
dotnet build .\src\HeboTech.GsmApi\HeboTech.GsmApi.csproj -c Release
