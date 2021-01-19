setlocal

set WIX_PATH=C:\Program Files (x86)\WiX Toolset v3.11\bin
set OUTPUT=fooeditor_setup.msi
set TYPE=%1
set IDE_PATH=C:\Program Files (x86)\Microsoft Visual Studio\2017\Community\Common7\IDE

if "%1"=="" set TYPE=Release

md ..\FooEditor\bin\%TYPE%\Plugin

"%IDE_PATH%\devenv" /build %TYPE% ..\FooEditor.sln
if errorlevel 1 goto end
"%IDE_PATH%\devenv" /build %TYPE% ..\FooGrep.sln
if errorlevel 1 goto end

md dist
xcopy ..\Definitions\Keywords .\dist\Keywords /E /I
xcopy ..\Definitions\Sinppets .\dist\Sinppets /E /I
copy ..\Definitions\*.xsd .\dist
copy ..\FooEditor\bin\%TYPE%\*.* .\dist
copy ..\FooGrep\bin\%TYPE%\*.* .\dist
copy ..\FooEditorHelp\*.chm .\dist
copy ..\AdminOperation\bin\%TYPE%\*.exe .\dist

md dist\Plugin
copy ..\FooEditor\bin\%TYPE%\Plugin\*.* .\dist\Plugin

"%WIX_PATH%\heat.exe" dir .\dist -scom -sreg -sfrag -srd -gg -cg MainGroup -dr INSTALLDIR -out files.wxs
"%WIX_PATH%\candle.exe" installer.xml
"%WIX_PATH%\candle.exe" files.wxs
"%WIX_PATH%\light.exe" -cultures:ja-jp -ext WiXNetFxExtension -ext WixUIExtension installer.wixobj files.wixobj -b dist -out "%OUTPUT%"

:end
endlocal
pause

