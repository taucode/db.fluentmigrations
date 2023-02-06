dotnet restore

dotnet build TauCode.Db.FluentMigrations.sln -c Debug
dotnet build TauCode.Db.FluentMigrations.sln -c Release

dotnet test TauCode.Db.FluentMigrations.sln -c Debug
dotnet test TauCode.Db.FluentMigrations.sln -c Release

nuget pack nuget\TauCode.Db.FluentMigrations.nuspec