dotnet publish MelodyPlus\MelodyPlus.csproj /p:PublishProfile=Windows-x64
dotnet publish MelodyPlus\MelodyPlus.csproj /p:PublishProfile=Linux-x64

$compress = @{
  Path = ".\Release\MelodyPlus"
  CompressionLevel = "Optimal"
  DestinationPath = "MelodyPlus.zip"
}
Compress-Archive @compress -Force