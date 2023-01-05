echo off

echo.
echo Rebuilding your application, please wait...
echo.

cd KioskScreensIdsInLogsTranslator

dotnet build

echo.
echo Rebuilding your application is successfully completed
echo.

echo.
echo Running your app
echo.

cd bin/Debug/net5.0/

KioskScreensIdsInLogsTranslator.exe