# Supplementary tools

## `geostationary.py`

Creates underlay images.

This script projects a plate carrée image image of the Earth - such as those obtained NASA's from [Blue Marble](https://visibleearth.nasa.gov/collection/1484/blue-marble) collection - into a geostationary projection.

The script is a quick hack as it's not designed for day-to-day use. Uncomment the satellite you want to generate an underlay for and update the source paths as required.

(caveat: I am not a Python developer. This script was mostly cobbled from sample Cartophy code.)

### Dependencies

This script uses [Cartophy](https://scitools.org.uk/cartopy/docs/latest/) to do the projection mapping. Installation instructions are on the site.

### Post-processing
Fine tune scale and positioning adjustments need to be done manually. I used Photoshop, screening the underlay with a sample image. Enabling country boundary rendering for IR images is useful, however it's hard to get the underlay perfectly aligned.


