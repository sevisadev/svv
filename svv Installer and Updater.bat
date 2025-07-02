@echo off
chcp 65001 >nul
title svv Installer And Updater

for /f %%i in ('echo prompt $E^| cmd') do set "ESC=%%i"
echo %ESC%[35m     _________  _____  __
echo %ESC%[35m    /  ___/\  \/ /\  \/ /
echo %ESC%[35m    \___ \  \   /  \   /
echo %ESC%[35m   /____  >  \_/    \_/
echo %ESC%[35m        \/
echo.

set "ZIP_FILE=svv_latest.zip"
set "EXTRACT_DIR=%~dp0svv"
set "DOWNLOAD_URL=https://github.com/sevisadev/svv/releases/latest/download/svv.zip"

echo Downloading latest ZIP...
curl -L -o "%ZIP_FILE%" "%DOWNLOAD_URL%"
if not exist "%ZIP_FILE%" (
    echo %ESC%[31m ERROR: Failed to download ZIP. Check your internet or GitHub link.
    pause
    exit /b
)

echo Extracting to %EXTRACT_DIR%...
powershell -nologo -noprofile -command "Expand-Archive -Path '%ZIP_FILE%' -DestinationPath '%EXTRACT_DIR%' -Force"

if not exist "%EXTRACT_DIR%\svv.exe" (
    echo %ESC%[31m ERROR: svv.exe not found in extracted files!
    pause
    exit /b
)

echo Deleting ZIP file...
del "%ZIP_FILE%"

echo Launching svv...
start "" "%EXTRACT_DIR%\svv.exe"

echo Creating desktop shortcut...
powershell -nologo -noprofile -command "$w=New-Object -ComObject WScript.Shell; $desktop=\"$env:USERPROFILE\Desktop\svv.lnk\"; $folderShortcut=\"%~dp0svv.lnk\"; $target=\"%EXTRACT_DIR%\svv.exe\"; $shortcutDesktop=$w.CreateShortcut($desktop); $shortcutDesktop.TargetPath=$target; $shortcutDesktop.WorkingDirectory=\"%EXTRACT_DIR%\"; $shortcutDesktop.Save(); $shortcutFolder=$w.CreateShortcut($folderShortcut); $shortcutFolder.TargetPath=$target; $shortcutFolder.WorkingDirectory=\"%EXTRACT_DIR%\"; $shortcutFolder.Save()"
