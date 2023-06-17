echo mucomMD2vgm

del /Q .\output\*.*
xcopy .\mucomMD2vgm\bin\x64\Release\net6.0-windows\*.* .\output /E /R /Y /I /K
xcopy .\mdvc\bin\x64\Release\net6.0-windows\*.* .\output /E /R /Y /I /K

copy /Y .\CHANGE.txt .\output
copy /Y .\LICENSE.txt .\output
copy /Y .\mucomMD2vgm_MMLCommandMemo.txt .\output
copy /Y .\README.md .\output
copy /Y .\removeZoneIdent.bat .\output
del /Q .\output\*.pdb
del /Q .\output\*.config
del /Q .\output\*.wav
del /Q .\output\*.config
del /Q .\output\bin.zip

pause
