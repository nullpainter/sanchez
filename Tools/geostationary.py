from io import BytesIO
import PIL
import cartopy.crs as ccrs
import cartopy.feature as cfeature
import matplotlib.pyplot as plt

def get_satellite_details():
#    return 'himawari-8.png', 140.7
#    return 'gk-2a.png', 128.2
#    return 'goes-16.png', -75.2 
    return 'goes-17.png', -137.2 

    
def main():

    # Edge length in pixels
    out_size_px = 7500
    source_path = 'c:/temp/world.200411.3x21600x10800.jpg'

    # Get satellite details
    output_filename, longitude = get_satellite_details()

    # Allow large images to be loaded
    PIL.Image.MAX_IMAGE_PIXELS = 933120000

    # Perform geostationary projection of source image
    fig = plt.figure(figsize=(1, 1), dpi=out_size_px)
    ax = fig.add_subplot(projection=ccrs.Geostationary(longitude, satellite_height=35786000))

    # Disable full disc border
    ax.outline_patch.set_linewidth(0)

    print(f'Projecting and plotting image; longitude: {longitude}; output file: {output_filename}')

    file = open(source_path, "rb")
    img_handle = BytesIO(file.read())
    img = plt.imread(img_handle, "jpg")
    img_proj = ccrs.PlateCarree()

    ax.imshow(img, transform=img_proj, origin='upper',regrid_shape=out_size_px)
    plt.savefig(output_filename,dpi=out_size_px,facecolor='black', edgecolor='black', transparent=True)
  
if __name__ == '__main__':
    main()
