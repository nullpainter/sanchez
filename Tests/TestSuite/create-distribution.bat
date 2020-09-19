dotnet publish ..\..\Sanchez\Sanchez.csproj -r win-x64 -c release -o sanchez -p:PublishSingleFile=true -p:PublishTrimmed=true
dotnet publish ..\..\Sanchez\Sanchez.csproj -r linux-x64 -c release-linux -o sanchez-linux -p:PublishSingleFile=true -p:PublishTrimmed=true
