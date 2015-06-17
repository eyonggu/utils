#!/usr/bin/awk -f

#This script is to parse strongswan charon log to get useful information

BEGIN {
    first = 1

    spi_i = ""
    spi_r = ""
    SK_ai = ""
    SK_ar = ""
    SK_ei = ""
    SK_er = ""
}


#Common function
function timestamp(line) {
    split(line, f)
    return f[3]
}

function message_content(line) {
    content = ""
    nf = split(line, f)
    for (i = 9; i <= nf; i++) {
        content = content " " f[i]
    }
    return content
}

function direction(line) {
    if (line ~ /generating/) {
        return "---->"
    } else if (line ~ /parsed/) {
        return "<----"
    }
}

function substring(line, start, end) {
    content = ""
    nf = split(line, f)
    for (i = start; i <= end; i++) {
        content = content " " f[i]
    }
    return content
}

function print_key_mat(keyname, keylen) {
    key = keyname ": "
    while (keylen > 0) {
        getline
        for (i = 0; i < 16 && keylen > 0; i++) {
            key = key $(6+i)
            keylen--
        }
    }

    printf "\t%s\n", key
}

#A new connection up 
/initiating IKE_SA/ {
    if (first) {
        first = 0
    } else {
        printf "\n\n"
    }
}

#Connection Info
/parsing rule 0 IKE_SPI/ {
    getline
    getline
    spi_i = "Initiator's SPI: " $6 $7 $8 $9 $10 $11 $12 $13
}

/parsing rule 1 IKE_SPI/ {
    getline
    getline
    spi_r = "Responder's SPI: " $6 $7 $8 $9 $10 $11 $12 $13
}

/Sk_ai|Sk_ar|Sk_ei|Sk_er secret/ {
    print_key_mat($5, $8)
}

/selected proposal/ {
    printf "\t%s\n", $7
}

#Message Flow
/(generating|parsed) IKE_SA_INIT/ {
    print timestamp($0) " " direction($0) " IKE_SA_INIT " message_content($0)
    if (/parsed/) {
        printf "\t%s\n", spi_i
        printf "\t%s\n", spi_r
    }
}

/(generating|parsed) IKE_AUTH/ {
    print timestamp($0) " " direction($0) " IKE_AUTH " message_content($0)
}

/(generating|parsed) CREATE_CHILD_SA/ {
    print timestamp($0) " " direction($0) " CREATE_CHILD_SA " message_content($0)
}

/(generating|parsed) INFORMATIONAL/ {
    print timestamp($0) " " direction($0) " INFORMATIONAL " message_content($0)
}


#XFRM msg
function dump_xfrm_msg() {
    #use coprocess to avoid temp file
    parser = "/home/eyonggu/project/xfrmparser/bin/i686/xfrmpaser"
    while ($5 ~ /([[:digit:]])+:/) {
        print substring($0, 6, NF-1) |& parser
        getline
    }
    #close the write end, so that parser starts to work
    close(parser, "to")

    while ((parser |& getline line) > 0) {
        print "\t\t", line
    }
    close(parser)
}

/(XFRM_MSG_ALLOCSPI|XFRM_MSG_NEWSA|XFRM_MSG_UPDSA|XFRM_MSG_DELSA|XFRM_MSG_GETSA|XFRM_MSG_NEWPOLICY|XFRM_MSG_UPDPOLICY|XFRM_MSG_GETPOLICY|XFRM_MSG_DELPOLICY)/ {
    if (/XFRM_MSG_ALLOCSPI/) {
        printf "\tXFRM_MSG_ALLOCSPI\n"
    } else if (/XFRM_MSG_NEWSA/) {
        printf "\tXFRM_MSG_NEWSA\n"
    } else if (/XFRM_MSG_UPDSA/) {
        printf "\tXFRM_MSG_UPDSA\n"
    } else if (/XFRM_MSG_DELSA/) {
        printf "\tXFRM_MSG_DELSA\n"
    } else if (/XFRM_MSG_GETSA/) {
        printf "\tXFRM_MSG_GETSA\n"
    } else if (/XFRM_MSG_NEWPOLICY/) {
        printf "\tXFRM_MSG_NEWPOLICY\n"
    } else  if (/XFRM_MSG_UPDPOLICY/) {
        printf "\tXFRM_MSG_UPDPOLICY\n"
    } else  if (/XFRM_MSG_GETPOLICY/) {
        printf "\tXFRM_MSG_GETPOLICY\n"
    } else  if (/XFRM_MSG_DELPOLICY/) {
        printf "\tXFRM_MSG_DELPOLICY\n"
    }
    # } else  if (//) {
        # printf "\t\n"
    # }

    if (1) {
        #message starts from next line
        getline
        dump_xfrm_msg()
    }
}

END {
}
