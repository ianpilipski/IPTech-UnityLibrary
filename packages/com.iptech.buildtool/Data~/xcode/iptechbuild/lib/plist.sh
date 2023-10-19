#!/bin/zsh

function plistAddUrlScheme() {

    # set INFOPLIST_FILE to argument 1
    local INFOPLIST_FILE=$1

    # set SCHEMA_TO_ADD to argument 2
    local SCHEMA_TO_ADD=$2

    local SHOW_HELP=0
    # if INFOPLIST_FILE is empty, then exit with error
    if [ -z $INFOPLIST_FILE ]; then
        echo "INFOPLIST_FILE is empty"
        SHOW_HELP=1
    fi

    # if SCHEMA_TO_ADD is empty, then exit with error
    if [ -z $SCHEMA_TO_ADD ]; then
        echo "SCHEMA_TO_ADD is empty"
        SHOW_HELP=1
    fi

    if [ $SHOW_HELP -eq 1 ]; then
        echo "Usage: addUrlScheme <info_plist_file> <schema_to_add>"
        exit 1
    fi


    local plb=/usr/libexec/PlistBuddy

    # get the index of the last CFBundleURLTypes 
    local LAST_INDEX=$($plb -c "Print CFBundleURLTypes" $INFOPLIST_FILE 2>/dev/null | grep -cE "Dict \{")

    # if last index is empty then create the CFBundleURLTypes array
    if [ -z $LAST_INDEX ]; then
        $plb -c "Add CFBundleURLTypes: array" $INFOPLIST_FILE
        $plb -c "Add CFBundleURLTypes:0 dict" $INFOPLIST_FILE
        $plb -c "Add CFBundleURLTypes:0:CFBundleURLSchemes array" $INFOPLIST_FILE
        LAST_INDEX=0
    fi
    

    # loop through the found items and see if it already exists
    if [ $LAST_INDEX -ne 0 ]; then
        LAST_INDEX=$((LAST_INDEX - 1))
        
        # loop through the CFBundleURLTypes array
        for i in {0..$LAST_INDEX}; do
            # get the CFBundleURLSchemes array of the current CFBundleURLTypes array
            local SCHEMES=$($plb -c "Print CFBundleURLTypes:$i:CFBundleURLSchemes" $INFOPLIST_FILE 2>/dev/null)

            # loop through each line in SCHEMES
            while read -r line; do
                # trim whitespace from line
                line="$(echo $line | awk '{$1=$1};1')"

                # if line with whitespace removed is equal to SCHEMA_TO_ADD, then exit
                if [[ "$line" == "$SCHEMA_TO_ADD" ]]; then
                    echo "Skipped Adding CFBundleURLScheme: $SCHEMA_TO_ADD, it already exists in $INFOPLIST_FILE"
                    return
                fi
            done <<< "$SCHEMES"
        done
    fi


    $plb -c "Add CFBundleURLTypes:0:CFBundleURLSchemes:0 string $SCHEMA_TO_ADD" $INFOPLIST_FILE
    echo "Added CFBundleURLScheme: $SCHEMA_TO_ADD to $INFOPLIST_FILE"
}

