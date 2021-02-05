sanchez\sanchez reproject -o output\equirectangular\equirectangular-stitched-0.5.jpg -s sample-images\subset -T 2020-08-30T03:50:20 -f -r 0.5 %1 %2 %3 %4 %5
sanchez\sanchez reproject -o output\equirectangular\equirectangular-stitched.jpg -s sample-images\subset -T 2020-08-30T03:50:20 -f %1 %2 %3 %4 %5
sanchez\sanchez reproject -o output\equirectangular\equirectangular-stitched-clut.jpg -s sample-images\subset -T 2020-08-30T03:50:20 -f -c0-1
sanchez\sanchez reproject -o output\equirectangular\equirectangular-stitched-nocrop.jpg -s sample-images\subset -T 2020-08-30T03:50:20 -f --nocrop
sanchez\sanchez reproject -o output\equirectangular\equirectangular-stitched-clut-nocrop.jpg -s sample-images\subset -T 2020-08-30T03:50:20 -f --nocrop -c0-1