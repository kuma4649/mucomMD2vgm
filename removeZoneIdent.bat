@echo off
echo zip,dll,exe�t�@�C����Zone���ʎq���폜���܂��B
pause

echo on
FOR %%a in (*.zip *.dll *.exe) do (echo . > %%a:Zone.Identifier)
FOR %%a in (lang\*.zip lang\*.dll lang\*.exe) do (echo . > %%a:Zone.Identifier)
@echo off

echo �������܂����B
pause
echo on
