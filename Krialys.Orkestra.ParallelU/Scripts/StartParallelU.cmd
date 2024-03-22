@ECHO OFF

REM Get the directory of the batch script file, then push it to the stack
PUSHD "%~dp0"

REM Get current folder name
FOR %%I in (.) do SET CurrDirName=%%~nxI

REM Set Destination Folder
SET DEST=%CD%
REM Set service name (It will take the folder name automatically)
SET NAME="%CurrDirName%"
REM Set Working Directory
SET WORK_DIR=..\..\KRepertoireTravail

REM Working Directory Creation
ECHO Working Directory Creation
IF EXIST %WORK_DIR% ECHO %WORK_DIR% Already exists
IF NOT EXIST %WORK_DIR% ECHO %WORK_DIR% Created
IF NOT EXIST %WORK_DIR% MKDIR %WORK_DIR% 
ECHO.

REM Check if Service installed or not
SC query %NAME% > nul
IF ERRORLEVEL 1060 (
	ECHO "Service is not installed. Nothing to remove"
) ELSE (
	ECHO "Service is installed...Gonna be removed"
	NET STOP  %NAME%
	SC delete %NAME%
)

REM Create, set parameters then start service
CALL SC.EXE create %NAME% displayname=%NAME% binPath=%DEST%\Krialys.Orkestra.ParallelU.exe start=delayed-auto
CALL SC.EXE description %NAME% %NAME%
CALL SC.EXE failure %NAME% reset=86400 actions=restart/60000/restart/60000//
CALL NET.EXE START %NAME%

REM Return to the previous working directory, which we pushed onto the stack
POPD

PAUSE