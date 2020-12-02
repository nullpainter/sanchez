sanchez\sanchez reproject -o output\equirectangular\nostitch\crop\nz -s sample-images\subset -f -r 2 --lat -33.6:-48 --lon 165.1:179.3
sanchez\sanchez reproject -o output\equirectangular\nostitch\crop\nz\nounderlay -s sample-images\subset -f -r 2 --lat -33.6:-48 --lon 165.1:179.3 -U
sanchez\sanchez reproject -o output\equirectangular\nostitch\clut\crop\nz -s sample-images\subset -c 0-1 -g sanchez\Resources\Gradients\Purple-Yellow.json  --lat -33.6:-48 --lon 165.1:179.3

sanchez\sanchez reproject -o output\equirectangular\nostitch\crop\au -s sample-images\subset -f -r 2  --lat -7.8:-45 --lon 112:155
sanchez\sanchez reproject -o output\equirectangular\nostitch\crop\au\nounderlay -s sample-images\subset -f -r 2  --lat -7.8:-45 --lon 112:155 -U
sanchez\sanchez reproject -o output\equirectangular\nostitch\clut\crop\au -s sample-images\subset -c 0-1 -g sanchez\Resources\Gradients\Purple-Yellow.json  --lat -7.8:-45 --lon 112:155

sanchez\sanchez reproject -o output\equirectangular\nostitch\crop\nzandus\nounderlay -s sample-images\subset -f -r 2  --lat 13.5:-58 --lon 165.1:-28 -U
sanchez\sanchez reproject -o output\equirectangular\nostitch\clut\crop\nzandus -s sample-images\subset -c 0-1 -g sanchez\Resources\Gradients\Purple-Yellow.json  --lat 13.5:-58 --lon 165.1:-28