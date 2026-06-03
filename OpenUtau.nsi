ManifestDPIAware true
SetCompressor zlib

!define PRODUCT_NAME "Stellar OpenUTAU Pro"
!define PRODUCT_VERSION "1.0.0"
!define PRODUCT_PUBLISHER "Stellarloom Limited"
!define PRODUCT_WEB_SITE "https://github.com/stellartraveler5162-SLCG/Stellar-OpenUTAU-Pro"
!define PRODUCT_UNINST_KEY "Software\Microsoft\Windows\CurrentVersion\Uninstall\${PRODUCT_NAME}"
!define PRODUCT_UNINST_ROOT_KEY "HKLM"

!include "MUI2.nsh"

!define MUI_ABORTWARNING
!define MUI_ICON "OpenUtau\Assets\open-utau.ico"
!define MUI_UNICON "${NSISDIR}\Contrib\Graphics\Icons\modern-uninstall.ico"

!define MUI_LANGDLL_REGISTRY_ROOT "${PRODUCT_UNINST_ROOT_KEY}"
!define MUI_LANGDLL_REGISTRY_KEY "${PRODUCT_UNINST_KEY}"
!define MUI_LANGDLL_REGISTRY_VALUENAME "NSIS:Language"

!insertmacro MUI_PAGE_WELCOME
!insertmacro MUI_PAGE_DIRECTORY
!insertmacro MUI_PAGE_INSTFILES
!define MUI_FINISHPAGE_RUN "$INSTDIR\OpenUtau.exe"
!insertmacro MUI_PAGE_FINISH

!insertmacro MUI_UNPAGE_INSTFILES

!insertmacro MUI_LANGUAGE "English"
!insertmacro MUI_LANGUAGE "French"
!insertmacro MUI_LANGUAGE "German"
!insertmacro MUI_LANGUAGE "Japanese"
!insertmacro MUI_LANGUAGE "Korean"
!insertmacro MUI_LANGUAGE "Russian"
!insertmacro MUI_LANGUAGE "SimpChinese"

Name "${PRODUCT_NAME} ${PRODUCT_VERSION}"
OutFile "Stellar-OpenUTAU-Pro-Windows-x64-Setup.exe"
InstallDir "$PROGRAMFILES64\Stellar OpenUTAU Pro"
ShowInstDetails show
ShowUnInstDetails show

Function .onInit
  !insertmacro MUI_LANGDLL_DISPLAY
FunctionEnd

Section "MainSection" SEC01
  SetOutPath "$INSTDIR"
  SetOverwrite ifnewer
  File /r "dist-win\*"
SectionEnd

Section -AdditionalIcons
  CreateShortCut "$SMPROGRAMS\Stellar OpenUTAU Pro.lnk" "$INSTDIR\OpenUtau.exe"
  CreateShortCut "$DESKTOP\Stellar OpenUTAU Pro.lnk" "$INSTDIR\OpenUtau.exe"
SectionEnd

Section -Post
  FileOpen $9 "$INSTDIR\installed.txt" w
  FileWrite $9 "yes"
  FileClose $9

  WriteUninstaller "$INSTDIR\uninst.exe"
  WriteRegStr ${PRODUCT_UNINST_ROOT_KEY} "${PRODUCT_UNINST_KEY}" "DisplayName" "$(^Name)"
  WriteRegStr ${PRODUCT_UNINST_ROOT_KEY} "${PRODUCT_UNINST_KEY}" "UninstallString" "$INSTDIR\uninst.exe"
  WriteRegStr ${PRODUCT_UNINST_ROOT_KEY} "${PRODUCT_UNINST_KEY}" "DisplayIcon" "$INSTDIR\OpenUtau.exe"
  WriteRegStr ${PRODUCT_UNINST_ROOT_KEY} "${PRODUCT_UNINST_KEY}" "DisplayVersion" "${PRODUCT_VERSION}"
  WriteRegStr ${PRODUCT_UNINST_ROOT_KEY} "${PRODUCT_UNINST_KEY}" "URLInfoAbout" "${PRODUCT_WEB_SITE}"
  WriteRegStr ${PRODUCT_UNINST_ROOT_KEY} "${PRODUCT_UNINST_KEY}" "Publisher" "${PRODUCT_PUBLISHER}"

  WriteRegStr HKCR ".ustx" "" "StellarOpenUtauProFile"
  WriteRegStr HKCR "StellarOpenUtauProFile" "" "Stellar OpenUTAU Pro Sequence File"
  WriteRegStr HKCR "StellarOpenUtauProFile\DefaultIcon" "" "$INSTDIR\OpenUtau.exe"
  WriteRegStr HKCR "StellarOpenUtauProFile\shell\open\command" "" '"$INSTDIR\OpenUtau.exe" "%1"'
SectionEnd

Function un.onUninstSuccess
  HideWindow
  MessageBox MB_ICONINFORMATION|MB_OK "$(^Name) was successfully removed from your computer."
FunctionEnd

Function un.onInit
!insertmacro MUI_UNGETLANGUAGE
  MessageBox MB_ICONQUESTION|MB_YESNO|MB_DEFBUTTON2 "Are you sure you want to completely remove $(^Name) and all of its components?" IDYES +2
  Abort
FunctionEnd

Section Uninstall
  Delete "$INSTDIR\uninst.exe"
  RMDir /r "$INSTDIR"

  Delete "$SMPROGRAMS\Stellar OpenUTAU Pro.lnk"
  Delete "$DESKTOP\Stellar OpenUTAU Pro.lnk"

  DeleteRegKey HKCR ".ustx"
  DeleteRegKey HKCR "StellarOpenUtauProFile"

  DeleteRegKey ${PRODUCT_UNINST_ROOT_KEY} "${PRODUCT_UNINST_KEY}"
  SetAutoClose true
SectionEnd
