sanchez\sanchez reproject -o output\equirectangular\crop\nz.jpg -s sample-images\subset -T 2020-08-30T03:50:20 --lat -33.6:-48 --lon 165.1:179.3 -fv -r 2
sanchez\sanchez reproject -o output\equirectangular\crop\australia.jpg -s sample-images\subset -T 2020-08-30T03:50:20 --lat -7.8:-45 --lon 112:155 -fv -r 2
sanchez\sanchez reproject -o output\equirectangular\crop\nzandus.jpg -s sample-images\subset -T 2020-08-30T03:50:20 --lat 13.5:-58 --lon 165.1:-28 -fv -r 2


sanchez\sanchez reproject -o output\equirectangular\crop\nz-nounderlay.jpg -s sample-images\subset -T 2020-08-30T03:50:20 --lat -33.6:-48 --lon 165.1:179.3 -fv -r 2 -U
sanchez\sanchez reproject -o output\equirectangular\crop\australia-nounderlay.jpg -s sample-images\subset -T 2020-08-30T03:50:20 --lat -7.8:-45 --lon 112:155 -fv -r 2 -U
sanchez\sanchez reproject -o output\equirectangular\crop\nzandus-nounderlay.jpg -s sample-images\subset -T 2020-08-30T03:50:20 --lat 13.5:-58 --lon 165.1:-28 -fv -r 2 -U


rem Full earth coverage
sanchez\sanchez reproject -o output\equirectangular\crop\nz-full.jpg -s sample-images -T 2020-08-30T03:50:20  --lat -33.6:-48 --lon 165.1:179.3 -fv -r 2
sanchez\sanchez reproject -o output\equirectangular\crop\australia-full.jpg -s sample-images -T 2020-08-30T03:50:20 --lat -7.8:-45 --lon 112:155 -fv -r 2
sanchez\sanchez reproject -o output\equirectangular\crop\nzandus-full.jpg -s sample-images -T 2020-08-30T03:50:20 --lat 13.5:-58 --lon 165.1:-28 -fv -r 2

sanchez\sanchez reproject -o output\equirectangular\crop\nz-full-nounderlay.jpg -s sample-images -T 2020-08-30T03:50:20 --lat -33.6:-48 --lon 165.1:179.3 -fv -r 2 -U
sanchez\sanchez reproject -o output\equirectangular\crop\australia-full-nounderlay.jpg -s sample-images -T 2020-08-30T03:50:20 --lat -7.8:-45 --lon 112:155 -fv -r 2 -U
sanchez\sanchez reproject -o output\equirectangular\crop\nzandus-full-nounderlay.jpg -s sample-images -T 2020-08-30T03:50:20 --lat 13.5:-58 --lon 165.1:-28 -fv -r 2 -U