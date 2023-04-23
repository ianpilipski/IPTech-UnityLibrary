#!/bin/zsh

set -eu

function resignArchive() {
    local ARCHIVE_PATH=$1
    local EXPORTOPTIONS_PLIST=$2
    [ $# -eq 3 ] && local EXPORTOPTIONS_ENTITLEMENTS=$3

    local SHOW_HELP=0
    if [ -z $ARCHIVE_PATH ]; then
        echo "failed resigning archive, ARCHIVE_PATH is empty"
        SHOW_HELP=1
    fi

    if [ -z $EXPORTOPTIONS_PLIST ]; then
        echo "failed resigning archive, EXPORTOPTIONS_PLIST is empty"
        SHOW_HELP=1
    fi

    if [ $SHOW_HELP -eq 1 ]; then
        echo "Usage: resignArchive <archive_path> <export_options_plist> [entitlements_plist]"
        exit 1
    fi

    if [ ! -f "$ARCHIVE_PATH/Info.plist" ]; then
        echo "failed resigning archive, ARCHIVE_PATH does not contain Info.plist"
        exit 1
    fi

    if [ ! -f "$EXPORTOPTIONS_PLIST" ]; then
        echo "failed resigning archive, EXPORTOPTIONS_PLIST does not exist"
        exit 1
    fi

    # get the bundle identifier from the archive plist
    local BUNDLE_IDENTIFIER=$(/usr/libexec/PlistBuddy -c "Print :ApplicationProperties:CFBundleIdentifier" "$ARCHIVE_PATH/Info.plist")

    #get the app path from the archive plist
    local APP_PATH=Products/$(/usr/libexec/PlistBuddy -c "Print :ApplicationProperties:ApplicationPath" "$ARCHIVE_PATH/Info.plist")

    # get the export options plist bundleidentifier
    local EXPORTOPTIONS_BUNDLE_IDENTIFIER=$(/usr/libexec/PlistBuddy -c "Print :provisioningProfiles" "$EXPORTOPTIONS_PLIST" | head -n 2 | tail -n 1 | awk '{print $1}')
    local EXPORTOPTIONS_SIGNINGCERT=$(/usr/libexec/PlistBuddy -c "Print :signingCertificate" "$EXPORTOPTIONS_PLIST")

    echo "resigning\n  archive: $ARCHIVE_PATH/$APP_PATH\n  bundleId: $EXPORTOPTIONS_BUNDLE_IDENTIFIER\n  cert: $EXPORTOPTIONS_SIGNINGCERT"

    # if the bundle identifier from the archive plist is not equal to the bundle identifier from the export options plist, then assign it
    if [[ "$BUNDLE_IDENTIFIER" != "$EXPORTOPTIONS_BUNDLE_IDENTIFIER" ]]; then
        echo "setting bundle identifier in $Archive_PATH/$APP_PATH/Info.plist to $EXPORTOPTIONS_BUNDLE_IDENTIFIER"
        /usr/libexec/PlistBuddy -c "Set :CFBundleIdentifier $EXPORTOPTIONS_BUNDLE_IDENTIFIER" "$ARCHIVE_PATH/$APP_PATH/Info.plist"
    fi

    if [[ -v EXPORTOPTIONS_ENTITLEMENTS ]]; then
        /usr/bin/codesign -f -s "$EXPORTOPTIONS_SIGNINGCERT" --entitlements "$EXPORTOPTIONS_ENTITLEMENTS" "$ARCHIVE_PATH/$APP_PATH"
    else
        /usr/bin/codesign -f -s "$EXPORTOPTIONS_SIGNINGCERT" "$ARCHIVE_PATH/$APP_PATH"
    fi

    echo "resigned archive: $ARCHIVE_PATH/$APP_PATH"
}

function installProvisioningProfile() {
    local FILE=$1

    echo "Checking if the provisionging profile $FILE should be installed"

    #get the uuid from the movileprovision file
    local UUID=$(/usr/libexec/PlistBuddy -c "Print UUID" /dev/stdin <<< $(security cms -D -i "$FILE"))

    local DESTFILE=~/Library/MobileDevice/Provisioning\ Profiles/$UUID.mobileprovision

    if [ -f "$DESTFILE" ]; then
        echo "Skipped installing provisioning profile $DESTFILE already exists, skipping"
        return
    fi

    cp "$FILE" "$DESTFILE"

    echo "Installed provisioning profile to $DESTFILE"
}