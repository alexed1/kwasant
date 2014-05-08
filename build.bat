@echo off

git clean -fdx
if not "%ERRORLEVEL%" == "0" goto :ERROR
".nuget/nuget.exe" restore Kwasant.sln
if not "%ERRORLEVEL%" == "0" goto :ERROR
"C:\Windows\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe" Kwasant.sln
if not "%ERRORLEVEL%" == "0" goto :ERROR
"packages\NUnit.Runners.2.6.3\tools\nunit-console.exe" -labels Tests/KwasantTest/KwasantTest.csproj
if not "%ERRORLEVEL%" == "0" goto :ERROR

exit /B 0

:ERROR
exit /B 1