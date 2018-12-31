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
CLS
GOTO INS_AV

REM ------------------------------------------------ Extract Zip File
 :UnZipFile <ExtractTo> <newzipfile>
 set vbs="%temp%\_.vbs"
 if exist %vbs% del /f /q %vbs%
 >%vbs%  echo Set fso = CreateObject("Scripting.FileSystemObject")
 >>%vbs% echo If NOT fso.FolderExists(%1) Then
 >>%vbs% echo fso.CreateFolder(%1)
 >>%vbs% echo End If
 >>%vbs% echo set objShell = CreateObject("Shell.Application")
 >>%vbs% echo set FilesInZip=objShell.NameSpace(%2).items
 >>%vbs% echo objShell.NameSpace(%1).CopyHere(FilesInZip)
 >>%vbs% echo Set fso = Nothing
 >>%vbs% echo Set objShell = Nothing
 cscript //nologo %vbs%
 if exist %vbs% del /f /q %vbs%
 GOTO CHK_NET4

:INS_AV
 IF exist %parent%ClamAV\ (
  REM Try to clean AV Folder
  DEL /q "%parent%ClamAV\*"
  FOR /D %%p IN ("%parent%ClamAV\*.*") DO rmdir "%%p" /s /q
  RMDIR "%parent%ClamAV" /s /q
  Call :UnZipFile "%parent%" "%parent%ClamAV.zip"
  GOTO CHK_NET4
) ELSE (
 Call :UnZipFile "%parent%" "%parent%ClamAV.zip"
 REM EXIT /b
 GOTO CHK_NET4
)

REM ------------------------------------------------ Check whether microsoft .net 4.5 is installed
:CHK_NET4
 SET NET4_INSTALLUTILDIR=
 for /F "tokens=1,2*" %%i in ('reg query "HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full" /v "InstallPath"') DO (
    if "%%i"=="InstallPath" (
        SET "NET4_INSTALLUTILDIR=%%k"
    )
 )

 IF "%NET4_INSTALLUTILDIR%"=="" (
  GOTO INSTALL_NET_45
 ) ELSE (
  REM We found dot net version 4.*
  REM No action to be taken
  GOTO CHK_SVC
 )
 
 :INSTALL_NET_45
 Start /B /I /WAIT dotNetFx45_Full_setup.exe /q /norestart
 GOTO CHK_SVC

:CHK_SVC
 SC QUERY %service_name% > NUL
 IF ERRORLEVEL 1060 GOTO ADD_SVC
 GOTO END

:ADD_SVC
 "%NET4_INSTALLUTILDIR%installutil.exe" -i "%parent%%service_name%.exe"
 IF ERRORLEVEL 1 ECHO There was a problem installing service.
 ECHO You can find install log under %parent%InstallUtil.InstallLog
 PAUSE
 GOTO END

:END
 REM net start %service_name%