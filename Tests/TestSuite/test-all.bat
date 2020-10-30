DEL /F/Q/S output
RMDIR /s/q output
DEL /F/Q/S sanchez
RMDIR /s/q sanchez

call .\create-distribution.bat
call .\equirectangular-nostitch.bat
call .\equirectangular-nostitch-autocrop.bat
call .\equirectangular-nostitch-verbose.bat
call .\equirectangular-single-type.bat
call .\equirectangular-nounderlay.bat
call .\equirectangular-stitch.bat
call .\equirectangular-stitch-custom-underlay.bat
call .\equirectangular-stitch-goes-autocrop.bat
call .\equirectangular-stitch-goes-all-autocrop.bat
call .\equirectangular-stitch-autocrop.bat
call .\equirectangular-stitch-nounderlay.bat
call .\equirectangular-stitch-timelapse.bat
call .\equirectangular-verbose.bat
call .\geostationary.bat
call .\geostationary-single-.bat
call .\geostationary-custom-underlay.bat
call .\geostationary-longitude.bat
call .\geostationary-longitude-verbose.bat
call .\geostationary-longitude-timelapse.bat
call .\geostationary-longitude-rotation-timelapse.bat
call .\geostationary-verbose.bat