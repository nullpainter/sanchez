sanchez\sanchez reproject -o output\equirectangular\nostitch\4 -s sample-images\subset -f -r 4
sanchez\sanchez reproject -o output\equirectangular\nostitch\2 -s sample-images\subset -f -r 2
sanchez\sanchez reproject -o output\equirectangular\nostitch\1 -s sample-images\subset -f -r 1
sanchez\sanchez reproject -o output\equirectangular\nostitch\clut -s sample-images\subset -c 0-1 -g sanchez\Resources\Gradients\Purple-Yellow.json
sanchez\sanchez reproject -o output\equirectangular\nostitch\nocrop -s sample-images\subset --nocrop
