@if "%SCM_TRACE_LEVEL%" NEQ "4" @echo off

:: Prerequisites
:: -------------

:: Setup
:: -----

setlocal enabledelayedexpansion

IF NOT DEFINED SOLUTION_NAME (
	SET SOLUTION_NAME=Kwasant.sln
)

IF NOT DEFINED DEPLOYMENT_SOURCE (
  SET DEPLOYMENT_SOURCE=.
)

IF NOT DEFINED MSBUILD_PATH (
  SET MSBUILD_PATH=%WINDIR%\Microsoft.NET\Framework\v4.0.30319\msbuild.exe
)

IF NOT DEFINED NUNIT_RUNNERS (
  SET NUNIT_RUNNERS=packages\NUnit.Runners.2.6.3\tools\nunit-console.exe
)

IF NOT DEFINED NUGET_EXE (
	SET NUGET_EXE=.nuget/nuget.exe
)

::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
:: Building
:: ----------

:: 1. Clean workspace
echo Cleaning workspace...
call :ExecuteCmd "git" clean -fdx

echo Restoring nuget packages...
:: 1. Restore NuGet packages
IF /I "%SOLUTION_NAME%" NEQ "" (
  call :ExecuteCmd "%NUGET_EXE%" restore "%DEPLOYMENT_SOURCE%\%SOLUTION_NAME%"
  IF !ERRORLEVEL! NEQ 0 goto error
)

:: 3. Build to the temporary path
IF /I "%IN_PLACE_DEPLOYMENT%" NEQ "1" (
  echo Building application to temp folder
  call :ExecuteCmd "%MSBUILD_PATH%" "%DEPLOYMENT_SOURCE%\KwasantWeb.csproj" /nologo /verbosity:m /t:Build /p:WarningLevel=0 /t:pipelinePreDeployCopyAllFilesToOneFolder /p:_PackageTempDir="%DEPLOYMENT_TEMP%";AutoParameterizationWebConfigConnectionStrings=false;Configuration=Release /p:SolutionDir="%DEPLOYMENT_SOURCE%\.\\" %SCM_BUILD_ARGS%
) ELSE (
  echo Building application in place
  call :ExecuteCmd "%MSBUILD_PATH%" "%DEPLOYMENT_SOURCE%\KwasantWeb.csproj" /nologo /verbosity:m /t:Build /p:WarningLevel=0 /p:AutoParameterizationWebConfigConnectionStrings=false;Configuration=Release /p:SolutionDir="%DEPLOYMENT_SOURCE%\.\\" %SCM_BUILD_ARGS%
)

IF !ERRORLEVEL! NEQ 0 goto error

:: 4. Building test projects
echo Building test projects
"%MSBUILD_PATH%" "%DEPLOYMENT_SOURCE%\Tests\KwasantTest\KwasantTest.csproj"
IF !ERRORLEVEL! NEQ 0 goto error
"%MSBUILD_PATH%" "%DEPLOYMENT_SOURCE%\Tests\DDay\DDay.Collections.Test\DDay.Collections.Test.csproj"
IF !ERRORLEVEL! NEQ 0 goto error
"%MSBUILD_PATH%" "%DEPLOYMENT_SOURCE%\Tests\DDay\DDay.iCal.Test\DDay.iCal.Test.csproj"
IF !ERRORLEVEL! NEQ 0 goto error

:: 5.
echo Running tests
call :ExecuteCmd "%NUNIT_RUNNERS%" -labels "%DEPLOYMENT_SOURCE%\Tests\KwasantTest\bin\Debug\KwasantTest.dll"
IF !ERRORLEVEL! NEQ 0 goto error
:: vstest.console.exe "%DEPLOYMENT_SOURCE%\Tests\DDay\DDay.Collections.Test\bin\Debug\DDay.Collections.Test.dll"
:: IF !ERRORLEVEL! NEQ 0 goto error
:: vstest.console.exe "%DEPLOYMENT_SOURCE%\Tests\DDay\DDay.iCal.Test\bin\Debug\DDay.iCal.Test.dll"
:: IF !ERRORLEVEL! NEQ 0 goto error

goto end

:ExecuteCmd
setlocal
set _CMD_=%*
call %_CMD_%
if "%ERRORLEVEL%" NEQ "0" echo Failed exitCode=%ERRORLEVEL%, command=%_CMD_%
exit /b %ERRORLEVEL%

:end
echo Finished successfully.
