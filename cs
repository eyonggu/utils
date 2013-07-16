#!/bin/bash

PROGRAM_NAME=$(basename $0)

ROOT_PATH="."
EXCLUDED_PATH=
APPEND_DIR=

usage() {
    echo ""
    echo "$PROGRAM_NAME [-f] [-e pattern] dir1 dir2 ... "
    echo "  -a dir         append dir to the database "
    echo "  -e pattern     exclude directories whose name matches pattern "
    echo "  -f             with full path in cscope database "
    exit 1
}

#parse options
while getopts "a:e:f" OPTION 
do
   case $OPTION in
      a ) APPEND_DIR=$OPTARG;;
      f ) ROOT_PATH=$(pwd);;
      e ) EXCLUDED_PATH="*/$OPTARG";;
      ? ) usage;;
   esac
done
shift $(($OPTIND - 1))

#remove old database files
for file in cscope.out cscope.in.out cscope.po.out fileametags tags
do
  rm -rf $file 
done

if [ -n "$APPEND_DIR" ]; then
    find $ROOT_PATH/$APPEND_DIR \( -iname "*.h" -o -iname "*.c" -o -iname "*.cpp" -o -iname "*.cc" \) -print >> cscope.files
else
    #at lest one dir is provided
    if [ $# -lt 1 ]; then
        usage
        exit 1
    fi

    #remove old file list
    rm -rf cscope.files 

    #for dir in "$*"
    while [ $# -gt 0 ]
    do
        find $ROOT_PATH/$1 -wholename "$EXCLUDED_PATH" -prune -o \( -iname "*.h" -o -iname "*.c" -o -iname "*.cpp" -o -iname "*.cc" \) -print >> cscope.files
        shift
    done
fi

#TODO: add common files

cscope -b -q

ctags -L cscope.files --c++-kinds=+p --fields=+iaS --extra=+q

#for filelookup plugin
echo -e "!_TAG_FILE_SORTED\t2\t/2=foldcase/" > filenametags
find . \( -iname "*.h" -o -iname "*.c" -o -iname "*.cpp" -o -iname "*.cc" \) -printf "%f\t%p\t1\n" | sort -f >> ./filenametags

