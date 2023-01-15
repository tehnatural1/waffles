#!/usr/bin/python
import plistlib
import requests
import os
from packaging import version

MOBILE_ASSET_URL = "http://mesu.apple.com/assets/com_apple_MobileAsset_SoftwareUpdate/com_apple_MobileAsset_SoftwareUpdate.xml"
latest_version_location = "/Users/Dustin/last_known_iOS_version.txt"
latest_version = None

def get_ios_assets( mobile_asset_url ):
    global latest_version
    builds          = {}
    asset_request   = requests.get( mobile_asset_url )
    plist_dict      = plistlib.loads( asset_request.content )

    # Iterate over the assets
    for asset in plist_dict.get( "Assets" ):

        # Collect build information from asset dictionary
        su_documentation_id     = asset.get( "SUDocumentationID", "" )
        build                   = asset.get( "Build", None )
        os_version              = asset.get( "OSVersion", None )
        release_type            = asset.get( "ReleaseType", None )

        # Only iOS builds are interesting
        if( not su_documentation_id.startswith( "iOS" ) ):
            continue

        # Remove wierd numbers infront of iOS version
        if( os_version.startswith( "9.9." ) ):
            os_version = os_version[ 4: ]

        # Check if release is production
        isBeta  = ( ( "Beta"       == release_type        ) or
                    ( "PreRelease" == su_documentation_id ) )

        if( version.parse( latest_version ) < version.parse( os_version ) ):
            update_last_known_version( os_version )
            latest_version = os_version
            os.system( f"osascript -e 'display notification \"New Update\" with title \"New iOS Version\" subtitle \"{os_version}\" sound name \"Submarine\"'" )

        # Store build information
        builds.update( { build: { 'Version': os_version, 'Beta': isBeta } } )

    return builds

def update_last_known_version( new_version ):
    try:
        open( latest_version_location, "w" ).write( new_version )
    except:
        pass

def get_last_known_version():
    last_version="1.0"
    try:
        last_version = open( latest_version_location, "r" ).read()
        print("Last known version:", last_version)
    except:
        pass

    return last_version


def main():
    global latest_version
    latest_version = get_last_known_version()

    print( f"Fetching iOS Builds from { MOBILE_ASSET_URL }" )
    mobile_assets = get_ios_assets( MOBILE_ASSET_URL )

    # Print asset header
    print( f"\n{'#':>2} {'Build':>12} {'Version':>9} {'Beta':>8}" )

    # Display assets
    for idx, build in enumerate( sorted( mobile_assets ) ):
        asset = mobile_assets.get( build, {} )
        print( f"{idx+1:>2} {build:>12} {asset.get('Version'):>9} {asset.get('Beta'):>8}" )

if __name__ == '__main__':
    main()

