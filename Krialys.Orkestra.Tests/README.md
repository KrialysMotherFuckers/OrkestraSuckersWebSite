
#  Krialys Test

## Summary


## Table of Contents

* [Test Project Structure]

----
----


# Test Project Structure

```shell
├── 4. Tests
│   └── Krialys.Orkestra.Tests

```

## Documentation

Playwright https://playwright.dev/dotnet/docs/intro

ASP.NET Community Standup - Blazor App Testing with Playwright https://www.youtube.com/watch?v=lJa3YlUliEs

## Requirements

1. Get-ExecutionPolicy -List

```shell
        Scope ExecutionPolicy
        ----- ---------------
MachinePolicy       Undefined
   UserPolicy       Undefined
      Process       Undefined
  CurrentUser    RemoteSigned
 LocalMachine       Undefined
 ```

 Set-ExecutionPolicy -ExecutionPolicy RemoteSigned -Scope CurrentUser

# Steps

1. Build project Krialys.Orkestra.Tests.csproj
```shell
Dotnet build
```
2. Install .\playwright.ps1 install with PowerShell

```shell
PS C:\[...]\Krialys.Orkestra.Tests\bin\Debug\net7.0> .\playwright.ps1 install
Downloading Chromium 117.0.5938.62 (playwright build v1080) from https://playwright.azureedge.net/builds/chromium/1080/chromium-win64.zip
118.6 Mb [====================] 100% 0.0s
Chromium 117.0.5938.62 (playwright build v1080) downloaded to C:\Users\[...]\AppData\Local\ms-playwright\chromium-1080
Downloading FFMPEG playwright build v1009 from https://playwright.azureedge.net/builds/ffmpeg/1009/ffmpeg-win64.zip
1.4 Mb [====================] 100% 0.0s
FFMPEG playwright build v1009 downloaded to C:\Users\[...]\AppData\Local\ms-playwright\ffmpeg-1009
Downloading Firefox 117.0 (playwright build v1424) from https://playwright.azureedge.net/builds/firefox/1424/firefox-win64.zip
78 Mb [====================] 100% 0.0s
Firefox 117.0 (playwright build v1424) downloaded to C:\Users\[...]\AppData\Local\ms-playwright\firefox-1424
Downloading Webkit 17.0 (playwright build v1908) from https://playwright.azureedge.net/builds/webkit/1908/webkit-win64.zip
45.9 Mb [====================] 100% 0.0s
Webkit 17.0 (playwright build v1908) downloaded to C:\Users\[...]\AppData\Local\ms-playwright\webkit-1908
 ```

 3. Run tests

```shell
dotnet test

Determining projects to restore...
  All projects are up-to-date for restore.
  Krialys.Orkestra.Tests -> C:\[...]\Krialys.Orkestra.Tests\bin\Debug\net7.0\Krial
  ys.Orkestra.Tests.dll
Test run for C:\[...]\Krialys.Orkestra.Tests\bin\Debug\net7.0\Krialys.Orkestra.Tests.dll (.NETCoreApp,Version=v7.0)
Microsoft (R) Test Execution Command Line Tool Version 17.7.1 (x64)
Copyright (c) Microsoft Corporation.  All rights reserved.

Starting test execution, please wait...
A total of 1 test files matched the specified pattern.

Passed!  - Failed:     0, Passed:     1, Skipped:     0, Total:     1, Duration: 4 s - Krialys.Orkestra.Tests.dll (net7.0)
```

## Playwright CodeGen Tool

1. On VS, Launch WebApi [Krialys.Orkestra.WebApi] 
2. On VS, Launch WebSite to test [Krialys.Orkestra.Web] => https://localhost:7192

3. Execute

```shell
PS C:\[...]\Krialys\Krialys.Orkestra.Tests\bin\Debug\net7.0> .\playwright.ps1 codegen https://localhost:7192
```