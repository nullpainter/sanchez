DEL /F/Q/S output
RMDIR /s/q output
DEL /F/Q/S sanchez
RMDIR /s/q sanchez

call .\create-distribution.bat
call .\test-equirectangular.bat
call .\test-geostationary.bat