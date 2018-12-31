; /* Copyright (C) TIN WIN AUNG - All Rights Reserved
; * Unauthorized copying of this file, via any medium is strictly prohibited
; * Proprietary and confidential
; * Written by TIN WIN AUNG <tinwinaung@sayargyi.com>, Dec 2018
; */

; HM NIS Edit Wizard helper defines
!define PRODUCT_NAME "Shiok"
!define PRODUCT_VERSION "0.5.6."
!define PRODUCT_PUBLISHER "TIN WIN AUNG"
!define PRODUCT_WEB_SITE "https://www.sayargyi.com"
!define PRODUCT_DIR_REGKEY "Software\Microsoft\Windows\CurrentVersion\App Paths\${PRODUCT_NAME}.exe"
!define PRODUCT_UNINST_KEY "Software\Microsoft\Windows\CurrentVersion\Uninstall\${PRODUCT_NAME}"
!define PRODUCT_UNINST_ROOT_KEY "HKLM"

RequestExecutionLevel admin ;Require admin rights on NT6+ (When UAC is turned on)

; MUI 1.67 compatible ------
!include "MUI2.nsh"

; MUI Settings
!define MUI_ABORTWARNING
!define MUI_ICON "..\res\Shiok.ico"
!define MUI_UNICON "..\res\Uninstall.ico"

; Welcome page
!insertmacro MUI_PAGE_WELCOME
; License page
!insertmacro MUI_PAGE_LICENSE "..\src\license.txt"

; Directory page
!insertmacro MUI_PAGE_DIRECTORY
; Instfiles page
!insertmacro MUI_PAGE_INSTFILES
; Finish page
;!define MUI_FINISHPAGE_RUN "$INSTDIR\Shiok.bat"
!define MUI_FINISHPAGE_SHOWREADME "$INSTDIR\ReadMe.txt"
!insertmacro MUI_PAGE_FINISH

; Uninstaller pages
!insertmacro MUI_UNPAGE_INSTFILES

; Language files
!insertmacro MUI_LANGUAGE "English"

;Check if Admin
!macro VerifyUserIsAdmin
UserInfo::GetAccountType
pop $0
${If} $0 != "admin" ;Require admin rights on NT4+
        messageBox mb_iconstop "Administrator rights required!"
        setErrorLevel 740 ;ERROR_ELEVATION_REQUIRED
        quit
${EndIf}
!macroend


; MUI end ------

Name "${PRODUCT_NAME} ${PRODUCT_VERSION}"
OutFile "Shiok_Setup.exe"
InstallDir "$PROGRAMFILES\${PRODUCT_NAME}"
InstallDirRegKey HKLM "${PRODUCT_DIR_REGKEY}" ""
ShowInstDetails show
ShowUnInstDetails show

Section "MainSection" SEC01
  SetOutPath "$INSTDIR"
  SetOverwrite try
  File "..\src\bin\Release\clamav.zip"
  File "..\src\bin\Release\dotNetFx45_Full_setup.exe"
  File "..\src\bin\Release\clamd.conf.ref"
  File "..\src\bin\Release\freshclam.conf.ref"
  File "..\src\bin\Release\NLog.config"
  File "..\src\bin\Release\Shiok.exe.config"
  File "..\src\bin\Release\NLog.dll"
  File "..\src\bin\Release\Newtonsoft.Json.dll"
  File "..\src\bin\Release\NLog.xml"
  File "..\src\bin\Release\Shiok.bat"
  File "..\src\bin\Release\Shiok.exe"
  File "..\src\bin\Release\Shiok.pdb"
  File "..\src\bin\Release\Shiok.xml"
  File "..\src\bin\Release\PostInstall.bat"
  File "..\src\bin\Release\PreUnInstall.bat"
  ;SetOverwrite ifnewer
  File "..\src\bin\Release\ReadMe.txt"
  CreateDirectory "$INSTDIR\etc"
  SetOutPath "$INSTDIR\etc"
  File "..\src\bin\Release\etc\Shiok.txt" 
  File "..\src\bin\Release\etc\ShiokWatchFolders.txt" 
  File "..\src\bin\Release\etc\Application.txt"
  SetOutPath "$INSTDIR" 
SectionEnd

Section -AdditionalIcons
  SetShellVarContext all
  CreateDirectory "$SMPROGRAMS\${PRODUCT_NAME}"
	CreateShortCut "$SMPROGRAMS\${PRODUCT_NAME}\${PRODUCT_NAME} Control.lnk" "$INSTDIR\Shiok.bat"
  WriteIniStr "$INSTDIR\${PRODUCT_NAME}.url" "InternetShortcut" "URL" "${PRODUCT_WEB_SITE}"
  CreateShortCut "$SMPROGRAMS\${PRODUCT_NAME}\Website.lnk" "$INSTDIR\${PRODUCT_NAME}.url"
  CreateShortCut "$SMPROGRAMS\${PRODUCT_NAME}\Uninstall.lnk" "$INSTDIR\uninst.exe"
  CreateShortCut "$SMPROGRAMS\${PRODUCT_NAME}\${PRODUCT_NAME} Config.lnk" "$INSTDIR\etc\Shiok.txt"
  CreateShortCut "$SMPROGRAMS\${PRODUCT_NAME}\Watch Folder Config.lnk" "$INSTDIR\etc\ShiokWatchFolders.txt"
  CreateShortCut "$SMPROGRAMS\${PRODUCT_NAME}\Open Log Folder.lnk" "$INSTDIR\log"
  CreateShortCut "$SMPROGRAMS\${PRODUCT_NAME}\Open Config Folder.lnk" "$INSTDIR\etc"
  CreateShortCut "$SMPROGRAMS\${PRODUCT_NAME}\Open Virus Folder.lnk" "$INSTDIR\Virus"
SectionEnd

Section -Post
  WriteUninstaller "$INSTDIR\uninst.exe"
  WriteRegStr HKLM "${PRODUCT_DIR_REGKEY}" "" "$INSTDIR\Shiok.exe"
  WriteRegStr ${PRODUCT_UNINST_ROOT_KEY} "${PRODUCT_UNINST_KEY}" "DisplayName" "$(^Name)"
  WriteRegStr ${PRODUCT_UNINST_ROOT_KEY} "${PRODUCT_UNINST_KEY}" "UninstallString" "$INSTDIR\uninst.exe"
  WriteRegStr ${PRODUCT_UNINST_ROOT_KEY} "${PRODUCT_UNINST_KEY}" "DisplayIcon" "$INSTDIR\Shiok.exe"
  WriteRegStr ${PRODUCT_UNINST_ROOT_KEY} "${PRODUCT_UNINST_KEY}" "DisplayVersion" "${PRODUCT_VERSION}"
  WriteRegStr ${PRODUCT_UNINST_ROOT_KEY} "${PRODUCT_UNINST_KEY}" "URLInfoAbout" "${PRODUCT_WEB_SITE}"
  WriteRegStr ${PRODUCT_UNINST_ROOT_KEY} "${PRODUCT_UNINST_KEY}" "Publisher" "${PRODUCT_PUBLISHER}"
  ;Do some post installations
  ExecWait '"$INSTDIR\PostInstall.bat"'
SectionEnd

Function un.onUninstSuccess
  HideWindow
  MessageBox MB_ICONINFORMATION|MB_OK "$(^Name) was successfully removed from your computer."
FunctionEnd

Function un.onInit
  SetShellVarContext all
  ;MessageBox MB_ICONQUESTION|MB_YESNO|MB_DEFBUTTON2 "Are you sure you want to completely remove $(^Name) and all of its components?" IDYES +2
  ;Abort
  ;Verify the uninstaller - last chance to back out
	MessageBox MB_OKCANCEL "Are you sure you want to completely remove $(^Name) and all of its components?" IDOK next
		Abort
	next:
	!insertmacro VerifyUserIsAdmin
FunctionEnd

function .onInit
	setShellVarContext all
	!insertmacro VerifyUserIsAdmin
functionEnd

Section Uninstall
  ExecWait '"$INSTDIR\PreUnInstall.bat"'

  ;Remove Program Files
  Delete "$INSTDIR\${PRODUCT_NAME}.url"
  Delete "$INSTDIR\uninst.exe"
  Delete "$INSTDIR\clamav.zip"
  Delete "$INSTDIR\dotNetFx45_Full_setup.exe"
  Delete "$INSTDIR\clamd.conf.ref"
  Delete "$INSTDIR\freshclam.conf.ref"
  Delete "$INSTDIR\NLog.config"
  Delete "$INSTDIR\Shiok.exe.config"
  Delete "$INSTDIR\NLog.xml"
  Delete "$INSTDIR\Shiok.xml"
  Delete "$INSTDIR\NLog.dll"
  Delete "$INSTDIR\Newtonsoft.Json.dll"
  Delete "$INSTDIR\Shiok.pdb"
  Delete "$INSTDIR\Shiok.bat"
  Delete "$INSTDIR\Shiok.exe"
  Delete "$INSTDIR\ReadMe.txt"
  Delete "$INSTDIR\PostInstall.bat"
  Delete "$INSTDIR\PreUnInstall.bat"
  Delete "$INSTDIR\etc\Shiok.txt"
  Delete "$INSTDIR\etc\ShiokWatchFolders.txt"
  Delete "$INSTDIR\etc\Application.txt"
  ; Try to remove the Program folder - this will only happen if it is empty
  RMDir "$INSTDIR\etc\"
  RMDir "$INSTDIR"

  ; Remove Start Menu launcher
	Delete "$SMPROGRAMS\${PRODUCT_NAME}\${PRODUCT_NAME} Control.lnk"
  Delete "$SMPROGRAMS\${PRODUCT_NAME}\Website.lnk"
  Delete "$SMPROGRAMS\${PRODUCT_NAME}\Uninstall.lnk"
  Delete "$SMPROGRAMS\${PRODUCT_NAME}\${PRODUCT_NAME} Config.lnk"
  Delete "$SMPROGRAMS\${PRODUCT_NAME}\Watch Folder Config.lnk"
  Delete "$SMPROGRAMS\${PRODUCT_NAME}\Open Log Folder.lnk"
  Delete "$SMPROGRAMS\${PRODUCT_NAME}\Open Config Folder.lnk"
  Delete "$SMPROGRAMS\${PRODUCT_NAME}\Open Virus Folder.lnk"
	; Try to remove the Start Menu folder - this will only happen if it is empty
	RMDir "$SMPROGRAMS\${PRODUCT_NAME}"

  ; Remove Reg
  DeleteRegKey ${PRODUCT_UNINST_ROOT_KEY} "${PRODUCT_UNINST_KEY}"
  DeleteRegKey HKLM "${PRODUCT_DIR_REGKEY}"

  ; Open Folder for usre to backup and delete folder
  ExecShell "open" "$INSTDIR"

  SetAutoClose false
SectionEnd