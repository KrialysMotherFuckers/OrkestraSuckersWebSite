@ECHO OFF

ECHO Building ApiUnivers...
dotnet restore
dotnet build
ECHO Done!
ECHO Try using this commandline: CURL.EXE http://localhost:3615/api/products
dotnet run
PAUSE