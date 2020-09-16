Examples:
-s c:\temp\GOES\goes17\fd\**\*CH02*.jpg -o out -v -D Resources\Goes17AllChannels.json
-s c:\temp\GOES\goes17\fd\**\*CH02*.jpg -o out1 -v -D Resources\Goes17AllChannels.json -f -h 0 -r 2 -u Resources\world.lights.3x10848x5424.jpg
reproject -o foo.jpg -s Resources -T 2020-08-30T03:50:20 -fva 