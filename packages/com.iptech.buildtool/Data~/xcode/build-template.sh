#!/bin/zsh

set -eu
set -o pipefail

if [[ ! -v IPTECH_BUILDTOOL_PASSWORD ]];then
    echo "You must supply a password by defining the environement variable IPTECH_BUILDTOOL_PASSWORD"
fi

[[ ! -v KEYCHAIN_NAME ]] && export KEYCHAIN_NAME=iptechbuildtool.keychain
[[ ! -v OUTPUT_PATH ]] && export OUTPUT_PATH=build

#ipilipski: I don't do this here because it should be build configurable
#export CP_HOME_DIR=$PWD/.cocoapods

source iptechbuild/lib/import-libs.sh

function cleanupDecryptedFiles() {
    setopt extendedglob
    setopt +o nullglob
    rm -f iptechbuild/data/^*.encrypted(N)
}

TRAPEXIT() {
    if [ $? -ne 0 ];then
        cleanupDecryptedFiles
        echoAsciiArtBuildFailed
    fi
}

decryptDir "iptechbuild/data"

# create the buildengine keychain AND install certs
createKeychain -p $IPTECH_BUILDTOOL_PASSWORD $KEYCHAIN_NAME
security unlock-keychain -p $IPTECH_BUILDTOOL_PASSWORD $KEYCHAIN_NAME

# install the signing certs
# IPTECH_SUB: SIGNING_CERTS

# install the provisioning profiles
# IPTECH_SUB: PROVISIONING_PROFILES

#if podfile exists, run pod install
if [ -f "Podfile" ]; then
   pod install --repo-update
fi

# IPTECH_SUB: BUILDS

cleanupDecryptedFiles
echoAsciiArtBuildSucceeded
