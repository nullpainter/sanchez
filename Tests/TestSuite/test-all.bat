DEL /F/Q/S output
RMDIR /s/q output
DEL /F/Q/S sanchez
RMDIR /s/q sanchez

call .\create-distribution.bat
call .\equirectangular-nostitch.bat
call .\equirectangular-nostitch-autocrop.bat
call .\equirectangular-nostitch-verbose.bat
call .\equirectangular-single-typebat
call .\equirectangular-stitch.bat
call .\equirectangular-stitch-goes-autocrop.bat
call .\equirectangular-stitch-goes-all-autocrop.bat
call .\equirectangular-stitch-autocrop.bat
call .\equirectangular-verbose.bat
call .\geostationary.bat
call .\geostationary-latitude.bat
call .\geostationary-latitude-verbose.bat
call .\geostationary-verbose.bat