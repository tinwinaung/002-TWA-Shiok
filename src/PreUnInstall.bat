REM /* Copyright (C) TIN WIN AUNG - All Rights Reserved
REM * Unauthorized copying of this file, via any medium is strictly prohibited
REM * Proprietary and confidential
REM * Written by TIN WIN AUNG <tinwinaung@sayargyi.com>, Dec 2018
REM */

@ECHO OFF
SETLOCAL ENABLEEXTENSIONS
SET me=%~n0
SET parent=%~dp0
ECHO %parent%
SET service_name=Shiok

REM ------------------------------------------------ Get NET Framework Folder
 CLS
 SET NET4_INSTALLUTILDIR=
 for /F "tokens=1,2*" %%i in ('reg query "HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full" /v "InstallPath"') DO (
    if "%%i"=="InstallPath" (
        SET "NET4_INSTALLUTILDIR=%%k"
    )
 )

 IF "%NET4_INSTALLUTILDIR%"=="" (
  GOTO END
 ) ELSE (
  GOTO CHK_SVC
 )

:CHK_SVC
 SC QUERY %service_name% > NUL
 IF ERRORLEVEL 1060 (
	GOTO END
 ) ELSE (
	GOTO REMOVE_SVC
 )
 GOTO END

:REMOVE_SVC
 net stop %service_name%
 "%NET4_INSTALLUTILDIR%installutil.exe" -u "%parent%%service_name%.exe"
 IF ERRORLEVEL 1 ECHO There was a problem installing service.
 ECHO You can find install log under %parent%InstallUtil.InstallLog
 
 GOTO END

:END
 del /q "%parent%ClamAV\*"
 FOR /D %%p IN ("%parent%ClamAV\*.*") DO rmdir "%%p" /s /q
 rmdir "%parent%ClamAV" /s /q