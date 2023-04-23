#!/bin/zsh

function encrypt() {
    # echo "\nCommandline was: $@ \n"
    local PASSWORD
    local DECRYPTARG=""
    local INPUTVALUE=""

    if [ -v IPTECH_BUILDTOOL_PASSWORD ]; then
        PASSWORD=$IPTECH_BUILDTOOL_PASSWORD
    fi

    while getopts "p:o:i:v:d" opt; do
        case $opt in
            p)
                PASSWORD=$(echo $OPTARG | awk '{print $1}')
                ;;
            d)
                DECRYPTARG="-d"
                ;;
            o)
                local OUTPUTFILEARG=$OPTARG
                ;;
            i)  
                local INFILEARG=TRUE
                INPUTVALUE=$OPTARG
                ;;
            v)
                INPUTVALUE=$OPTARG
                ;;
            \?)
                echo "Invalid option: -$OPTARG" >&2
                ;;
        esac
    done

    local SHOW_USAGE=0
    if [ -z $INPUTVALUE ]; then
        echo "Input value is required either -in or -val"
        SHOW_USAGE=1
    fi

    if [ -z $PASSWORD ]; then
        echo "Password is required"
        SHOW_USAGE=1
    fi

    if [ $SHOW_USAGE -eq 1 ]; then
        echo "Usage: encrypt.sh [-p <password>] [-d] [-o <output_file>] < -i <input_value> | -v <input_value> >"
        echo "  -p You can also assign the password to the environment variable IPTECH_BUILDTOOL_PASSWORD"
        echo "  -d will decrypt instead of encrypting it"
        echo "  -o will output the encrypted string to the specified file"
        echo "  -i will treat the input value as a file path and encrypt the contents of the file"
        echo "  -v will treat the input value as a string"
        exit 1
    fi

    if [ -v INFILEARG ]; then
        if [ -v OUTPUTFILEARG ]; then
            openssl enc -aes-256-cbc $DECRYPTARG -base64 -md sha256 -pass pass:$PASSWORD -in $INPUTVALUE -out $OUTPUTFILEARG  
        else 
            openssl enc -aes-256-cbc $DECRYPTARG -base64 -md sha256 -pass pass:$PASSWORD -in $INPUTVALUE
        fi
    else
        if [ -v OUTPUTFILEARG ]; then
            echo $INPUTVALUE | openssl enc -aes-256-cbc $DECRYPTARG -base64 -md sha256 -pass pass:$PASSWORD -out $OUTPUTFILEARG    
        else
            echo $INPUTVALUE | openssl enc -aes-256-cbc $DECRYPTARG -base64 -md sha256 -pass pass:$PASSWORD
        fi
    fi
}

function encryptString() {
    local LASTARG=${@:$#}
    local PREVARGS=${@:1:-1}

    
    encrypt $PREVARGS -v $LASTARG
}

function test() {
    #zshell argument count


}

function decryptString() {
    local LASTARG=${@:$#}
    local PREVARGS=${@:1:-1}

    encrypt $PREVARGS -d -v $LASTARG
}

function encryptFile() {
    local FILENAME=${@:$#}
    local PREVARGS=${@:1:-1}

    encrypt $PREVARGS -i $FILENAME -o $FILENAME.encrypted
}

function decryptFile() {
    local FILENAME=${@:$#}
    local PREVARGS=${@:1:-1}
    local FILENAME_NO_EXTENSION="${FILENAME%.*}"
    
    encrypt $PREVARGS -d -i $FILENAME -o $FILENAME_NO_EXTENSION
}

function decryptDir() {
    local LASTARG=${@:$#}
    local PREVARGS=${@:1:-1}
    local DIRECTORY_TO_DECRYPT=$LASTARG

    local SHOW_HELP=0
    # if directory to decrypt is empty, then exit with error
    if [ -z $DIRECTORY_TO_DECRYPT ]; then
        echo "Directory to decrypt is empty"
        SHOW_HELP=1
    fi

    if [ $SHOW_HELP -eq 1 ]; then
        echo "Usage: decryptDir [-p <password>] <directory_to_decrypt>"
        echo "  You can also assign the password to the environment variable IPTECH_BUILDTOOL_PASSWORD"
        exit 1
    fi

    # loop over all files that end with .encrypted in the directory to decrypt and call decrypt.sh for that file
    for file in $DIRECTORY_TO_DECRYPT/*.encrypted; do
        decryptFile $PREVARGS "$file"
    done
}

function encryptDir() {
    local LASTARG=${@:$#}
    local PREVARGS=${@:1:-1}
    local DIRECTORY_TO_ENC=$LASTARG

    local SHOW_HELP=0
    # if directory to decrypt is empty, then exit with error
    if [ -z $DIRECTORY_TO_ENC ]; then
        echo "Directory to encrypt is empty"
        SHOW_HELP=1
    fi

    if [ $SHOW_HELP -eq 1 ]; then
        echo "Usage: encryptDir [-p <password>] <directory_to_encrypt>"
        echo "  You can also assign the password to the environment variable IPTECH_BUILDTOOL_PASSWORD"
        exit 1
    fi

    # loop over all files that end with .encrypted in the directory to decrypt and call decrypt.sh for that file
    for file in $DIRECTORY_TO_ENC/*; do
        FILE_EXTENSION="${file##*.}"
        if [[ $FILE_EXTENSION == "encrypted" ]]; then
            continue
        fi
        
        encryptFile $PREVARGS "$file"
    done
}

function readValueFromDecryptedFile() {
    while getopts "k:f:o:d" opt; do
        case $opt in
            k)
                local KEY=$OPTARG
                ;;
            f)
                local FILENAME=$OPTARG
                ;;
            d)
                local DECODE
                ;;
            o)
                local OUTPUTFILENAME=$OPTARG
                ;;
            \?)
                echo "Invalid option: -$OPTARG" >&2
                ;;
        esac
    done

    local SHOW_USAGE=0

    [ -v KEY ] || SHOW_USAGE=1
    [ -v FILENAME ] || SHOW_USAGE=1

    if [ $SHOW_USAGE -eq 1 ]; then
        echo "Usage: readKeyFromDecryptedFile -k key -f file"
        exit 1
    fi

    while IFS=: read -r keyVar lineVar; do
        if [[ "$KEY" == "$keyVar" ]]; then
            local FOUNDVALUE="$lineVar"
            break;
        fi
    done < $FILENAME

    if [ -v FOUNDVALUE ];then
        if [ -v OUTPUTFILENAME ];then
            if [ -v DECODE ];then
                echo "$FOUNDVALUE" | base64 -d > "$OUTPUTFILENAME"
            else 
                echo "$FOUNDVALUE" > "$OUTPUTFILENAME"
            fi
        else
            if [ -v DECODE ];then
                echo "$FOUNDVALUE" | base64 -d 
            else 
                echo "$FOUNDVALUE"
            fi
        fi
    else
        >&2 echo "key $KEY not found in file $FILENAME"
        exit 1;
    fi
}
