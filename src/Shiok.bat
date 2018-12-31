REM /* Copyright (C) TIN WIN AUNG - All Rights Reserved
REM * Unauthorized copying of this file, via any medium is strictly prohibited
REM * Proprietary and confidential
REM * Written by TIN WIN AUNG <tinwinaung@sayargyi.com>, Dec 2018
REM */


@ECHO OFF
REM *** Setup Secript for Service ***
REM INIT CONSTANTS
CLS

SETLOCAL ENABLEEXTENSIONS
SET me=%~n0
SET parent=%~dp0
SET service_name=Shiok

REM ------------------------------------------------ Show Banner
 SET _APPNAME=%service_name% (v.0.5.6)
 TITLE %_APPNAME%

 ECHO.
 ECHO  No warranties
 ECHO.
 ECHO  I distribute software in the hope that it will be useful, 
 ECHO  but without any warranty. 
 ECHO  No author or distributor of this software accepts responsibility 
 ECHO  to anyone for the consequences of using it or for whether it serves 
 ECHO  any particular purpose or works at all, unless he says so in writing. 
 ECHO  This is exactly the same warranty that the proprietary software companies offer: None. 
 ECHO.
 REM PAUSE & REM This will wait until user press Enter 
 ECHO ...............................................
 ECHO  %_APPNAME%
 ECHO ...............................................
 ECHO  Computer Name : %COMPUTERNAME%
 ECHO  Current Login : %USERNAME%@%USERDOMAIN%
 ECHO  OS Arc        : %OS% (Not OS Name)
 ECHO  Processor Arc :
 ECHO  %PROCESSOR_ARCHITECTURE%/%PROCESSOR_IDENTIFIER%/%PROCESSOR_LEVEL%/%PROCESSOR_REVISION%
 ECHO  # of Processors X %NUMBER_OF_PROCESSORS%
 ECHO.

 TIMEOUT /T 20 & REM Wait for reading


REM ------------------------------------------------ Check Permission
 IF "%PROCESSOR_ARCHITECTURE%" EQU "amd64" (
	>nul 2>&1 "%SYSTEMROOT%\SysWOW64\cacls.exe" "%SYSTEMROOT%\SysWOW64\config\system"
 ) ELSE (
	>nul 2>&1 "%SYSTEMROOT%\system32\cacls.exe" "%SYSTEMROOT%\system32\config\system"
 )
 if '%errorlevel%' NEQ '0' (
    ECHO Requesting administrative privileges...
    goto UACPrompt
 ) else ( goto gotAdmin )
 :UACPrompt
    echo Set UAC = CreateObject^("Shell.Application"^) > "%temp%\getadmin.vbs"
    set params= %*
    echo UAC.ShellExecute "cmd.exe", "/c ""%~s0"" %params:"=""%", "", "runas", 1 >> "%temp%\getadmin.vbs"

    "%temp%\getadmin.vbs"
    del "%temp%\getadmin.vbs"
    exit /B

 :gotAdmin
  REM pushd "%CD%"
  REM CD /D "%~dp0"

REM ------------------------------------------------ Check whether microsoft .net 4.5 is installed
 CLS
 SET NET4_INSTALLUTILDIR=
 for /F "tokens=1,2*" %%i in ('reg query "HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full" /v "InstallPath"') DO (
    if "%%i"=="InstallPath" (
        SET "NET4_INSTALLUTILDIR=%%k"
    )
 )

 IF "%NET4_INSTALLUTILDIR%"=="" (
   CLS
   ECHO.
   ECHO  You DO NOT have Microsoft.NET v4.* or later
   ECHO  Do you want to install now?
   ECHO  OR You can install Microsoft.NET v4.0 and comeback later.
   ECHO  Press Enter to install or type no to exit
   ECHO.
   SET /P M=  :
   IF "%M%"=="" GOTO INSTALL_NET_45
    CLS
    ECHO  Sad to see you go.
    ECHO  I am going to close in 10 seconds.
    TIMEOUT /T 10 & REM Wait before exit
    GOTO EOF
 ) ELSE (
  REM We found dot net version 4.*
  REM No action to be taken
 )

REM ------------------------------------------------ Main Menu
:MENU
 CLS

 ECHO ...............................................
 ECHO  %service_name% SERVICE
 ECHO  Main Menu
 ECHO ...............................................
 ECHO  Option 1 and 2 will use embedded engine 
 ECHO  if you have custom setting for Shiok 
 ECHO ...............................................
 ECHO.
 ECHO  1 - Run Custom Scan
 ECHO  2 - Run Manual Signature DB Update
 ECHO  3 - Add/Remove Service ^>
 ECHO  4 - Start Service
 ECHO  5 - Stop Service
 ECHO  6 - EXIT
 ECHO.
 SET /P M= Select Menu press ENTER (6): || set M=6
 ECHO %M%
 IF %M%==1 GOTO SCAN
 IF %M%==2 GOTO UPDATE
 IF %M%==3 GOTO SERVICE_MENU
 IF %M%==4 GOTO SERVICE_CNT_START
 IF %M%==5 GOTO SERVICE_CNT_STOP
 IF %M%==6 GOTO EOF
REM ------------------------------------------------ Service Menu
:SERVICE_MENU
 CLS
 ECHO.
 ECHO ...............................................
 ECHO  %service_name% SERVICE
 ECHO  ^< Add/Remove Menu
 ECHO ...............................................
 ECHO.
 ECHO  1 - Service Status
 ECHO  2 - Add Service
 ECHO  3 - Remove Service
 ECHO  4 - ^< Back to Main Menu
 ECHO  5 - Exit
 ECHO.
 SET /P M= Select Menu press ENTER (4): || set M=4
 ECHO %M%
 IF %M%==1 GOTO STATUS
 IF %M%==2 GOTO ADD_SVC
 IF %M%==3 GOTO DEL_SVC
 IF %M%==4 GOTO MENU
 IF %M%==5 GOTO EOF
 GOTO EOF
REM ------------------------------------------------ Virus Scan
:SCAN
 CLS
 ECHO "Virus files will move to "
 ECHO "%parent%\virus if we found."
 SET /P M= Enter File or Folder path here: 
 %parent%\clamav\clamdscan.exe -m -v --move="%parent%\virus\" "%M%" 
 PAUSE
 GOTO MENU
REM ------------------------------------------------ Virus Signatur DB Update
:UPDATE
 CLS
 ECHO "We are updating signature database"
 %parent%\clamav\freshclam.exe --config-file="%parent%\clamav\freshclam.conf"
 PAUSE
 GOTO MENU
REM ------------------------------------------------ Start Service
:SERVICE_CNT_START
 sc start "%service_name%"
 PAUSE
 GOTO MENU
REM ------------------------------------------------ Stop Service
:SERVICE_CNT_STOP
 sc stop "%service_name%"
 PAUSE
 GOTO MENU
REM ------------------------------------------------ Check Service Status
:STATUS
 sc query "%service_name%"
 PAUSE
 GOTO MENU
REM ------------------------------------------------ Install Service
:ADD_SVC
 "%NET4_INSTALLUTILDIR%installutil.exe" -i "%parent%%service_name%.exe"
 IF ERRORLEVEL 1 ECHO There was a problem installing service.
 ECHO You can find install log under %parent%InstallUtil.InstallLog
 PAUSE
 GOTO MENU
REM ------------------------------------------------ Un install Service
:DEL_SVC
 "%NET4_INSTALLUTILDIR%installutil.exe" -u "%parent%%service_name%.exe"
 IF ERRORLEVEL 1 ECHO There was a problem removing service.
 ECHO You can find install log under %parent%InstallUtil.InstallLog
 PAUSE
 GOTO MENU
REM ------------------------------------------------ Install MS.NET 4.5
:INSTALL_NET_45
 Start /B /I /WAIT dotNetFx45_Full_setup.exe /norestart
 ECHO  Microsoft.NET 4.5 installation is executed.
 ECHO  Please make sure installation is successful.
 ECHO  Otherwise, Shiok will not work.
 PAUSE
 GOTO MENU
REM ------------------------------------------------ Exit
:EOF
 CLS