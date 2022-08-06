dotnet restore

dotnet build --configuration Debug
dotnet build --configuration Release

dotnet test -c Debug .\test\TauCode.Db.FluentMigrations.Tests\TauCode.Db.FluentMigrations.Tests.csproj
dotnet test -c Release .\test\TauCode.Db.FluentMigrations.Tests\TauCode.Db.FluentMigrations.Tests.csproj

nuget pack nuget\TauCode.Db.FluentMigrations.nuspec