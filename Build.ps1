$artifacts = ".\artifacts"

Write-Host "### Cleaning artifacts folder... ###"
if(Test-Path $artifacts) { Remove-Item $artifacts -Force -Recurse }

Write-Host "### dotnet clean... ###"
dotnet clean .\src\HeboTech.GsmApi\HeboTech.GsmApi.csproj -c Release

Write-Host "### dotnet build... ###"
dotnet build .\src\HeboTech.GsmApi\HeboTech.GsmApi.csproj -c Release

Write-Host "### dotnet test... ###"
dotnet test .\src\HeboTech.GsmApi.Tests\HeboTech.GsmApi.Tests.csproj -c Release

Write-Host "### dotnet pack... ###"
dotnet pack .\src\HeboTech.GsmApi\HeboTech.GsmApi.csproj -c Release -o $artifacts --no-build
