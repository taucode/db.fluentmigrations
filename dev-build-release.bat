dotnet restore

dotnet build --configuration Debug
dotnet build --configuration Release

dotnet test -c Debug .\tests\TauCode.Db.FluentMigrations.Tests\TauCode.Db.FluentMigrations.Tests.csproj
dotnet test -c Release .\tests\TauCode.Db.FluentMigrations.Tests\TauCode.Db.FluentMigrations.Tests.csproj

nuget pack nuget\TauCode.Db.FluentMigrations.nuspec
