#!/bin/zsh

function createKeychain() {
    local KEYCHAIN_PASSWORD=""
    while getopts "p:" opt; do
        case $opt in
            p)
                KEYCHAIN_PASSWORD=$OPTARG
                ;;
            \?)
                echo "Invalid option: -$OPTARG" >&2
                exit 1
                ;;
        esac
    done

    # set the keychain name to the last argument
    local KEYCHAIN_NAME=${@: -1}
    
    if [ -z $KEYCHAIN_NAME ]; then
        echo "failed creating keychain, KEYCHAIN_NAME is empty"
        exit 1
    fi

    if [ -z $KEYCHAIN_PASSWORD ]; then
        echo "failed creating keychain, KEYCHAIN_PASSWORD is empty"
        exit 1
    fi

    # does KEYCHAIN_NAME end with .keychain?
    if [[ $KEYCHAIN_NAME != *.keychain ]]; then
        KEYCHAIN_NAME="$KEYCHAIN_NAME.keychain"
    fi
    
    # list keychains and check if KEYCHAIN_NAME is already in the list
    local KEYCHAIN_LIST=$(security list-keychains)
    if [[ $KEYCHAIN_LIST == *"$KEYCHAIN_NAME-db"* ]]; then
        echo "skipping creating keychain, $KEYCHAIN_NAME already exists"    
    else 
        #create the keychain
        security create-keychain -p $KEYCHAIN_PASSWORD $KEYCHAIN_NAME
        # add the new keychain to the keychain list
        security list-keychains -d user | xargs -I {} security list-keychains -d user -s {} $KEYCHAIN_NAME
        # set the keychain timeout to 2 hours
        security set-keychain-settings -lut 7200 $KEYCHAIN_NAME        
    fi
}

function installSigningCert() {
    # usage installSigningCert -k <keychain_name> -p <keychain_password> -c <cert_path> -a <cert_password>
    local KEYCHAIN_NAME=""
    local KEYCHAIN_PASSWORD=""
    local CERT_PATH=""
    local CERT_PASSWORD=""
    local FORCE_INSTALL=0

    while getopts "k:p:c:a:f" opt; do
        case $opt in
            k)
                KEYCHAIN_NAME=$OPTARG
                ;;
            p)
                KEYCHAIN_PASSWORD=$OPTARG
                ;;
            c)
                CERT_PATH=$OPTARG
                ;;
            a)
                CERT_PASSWORD=$OPTARG
                ;;
            f)
                FORCE_INSTALL=1
                ;;
            \?)
                echo "Invalid option: -$OPTARG" >&2
                exit 1
                ;;
            :)
                echo "Option -$OPTARG requires an argument." >&2
                exit 1
                ;;
        esac
    done

    local SHOW_USAGE=0
    if [ -z $KEYCHAIN_NAME ]; then
        echo "Keychain name is required"
        SHOW_USAGE=1
    fi

    if [ -z $KEYCHAIN_PASSWORD ]; then
        echo "Keychain password is required"
        SHOW_USAGE=1
    fi

    if [ -z $CERT_PATH ]; then
        echo "Certificate path is required"
        SHOW_USAGE=1
    fi

    if [ $SHOW_USAGE -eq 1 ]; then
        echo "Usage: installSigningCert [-f] -k <keychain_name> -p <keychain_password> -c <cert_path> -a <cert_password>"
        echo "  -f: OPTIONAL force install the certificate"
        echo "  -a: OPTIONAL if the certificate is a .p12 file, the password is required"
        exit 1
    fi

    # does KEYCHAIN_NAME end with .keychain?
    if [[ $KEYCHAIN_NAME != *.keychain ]]; then
        KEYCHAIN_NAME="$KEYCHAIN_NAME.keychain"
    fi

    echo "----------------------------------------"
    echo "Checking if certificate $CERT_PATH needs to be installed to keychain $KEYCHAIN_NAME"

    # unlock the keychain
    security unlock-keychain -p $KEYCHAIN_PASSWORD $KEYCHAIN_NAME

    # load the 509 cert based on the extension
    local X509_CERT
    CERT_EXTENSION="${CERT_PATH##*.}"
    if [[ "$CERT_EXTENSION" == "p12" ]]; then
        X509_CERT=$(openssl pkcs12 -in $CERT_PATH -nodes -passin pass:$CERT_PASSWORD -legacy)
    elif [[ "$CERT_EXTENSION" == "cer" ]]; then
        X509_CERT=$(openssl x509 -in $CERT_PATH -inform DER)
    else
        echo "Certificate extension $CERT_EXTENSION is not supported"
        exit 1
    fi
    
    local CERT_NAME
    CERT_NAME=$(echo $X509_CERT | openssl x509 -noout -subject | sed -n 's/^.*CN=//p' | sed -n 's/\/.*//p') || { echo "failed to get certificate name"; exit 1;}

    local CERT_SHA1
    CERT_SHA1=$(echo $X509_CERT | openssl x509 -noout -fingerprint -sha1 | sed -n 's/^.*=//p' | sed 's/://g' ) || { echo "failed to get certificate sha1"; exit 1;}

    # does the certificate already exist in the keychain?
    local CERT_EXISTS
    local CERT_EXISTS_SHA1
    CERT_EXISTS_SHA1=$(security find-certificate -c $CERT_NAME -a -Z $KEYCHAIN_NAME | grep "SHA-1 hash:" | sed 's/SHA-1 hash: //g' || :)
    if [ ! -z $CERT_EXISTS_SHA1 ]; then
        if [[ "$CERT_SHA1" == "$CERT_EXISTS_SHA1" ]]; then
            CERT_EXISTS=1
        fi
    fi

    if [ -z $CERT_EXISTS ] || [ $FORCE_INSTALL -eq 1 ]; then
        # import the certificate
        echo "Installing certificate $CERT_NAME to keychain $KEYCHAIN_NAME"
        if [ -z $CERT_PASSWORD ]; then
            # this .cer file does not have a password and is not used for signing
            security import $CERT_PATH -k $KEYCHAIN_NAME -A
        else 
            # this .p12 file has a password and is used for signing
            security import $CERT_PATH -k $KEYCHAIN_NAME -P $CERT_PASSWORD -T /usr/bin/codesign
            security set-key-partition-list -S "apple-tool:,apple:" -s -k $KEYCHAIN_PASSWORD $KEYCHAIN_NAME
        fi
        echo "Certificate $CERT_NAME imported to keychain $KEYCHAIN_NAME"
    else
        echo "Certificate $CERT_NAME already exists in keychain $KEYCHAIN_NAME"
    fi
    echo "----------------------------------------"
}