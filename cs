#!/bin/bash

PROGRAM_NAME=$(basename $0)

CURRENT_DIR="./"

usage() {
    echo ""
    echo "$PROGRAM_NAME [-f] dir1 dir2 ... "
    echo "  -f   with full path in cscope database "
    exit 1
}

#clean old database files
for file in cscope.files cscope.out cscope.in.out cscope.po.out fileametags tags
do
  rm -rf $file 
done

#parse options
while getopts "f" Option 
do
   case $Option in
      f ) CURRENT_DIR=$(pwd);;
      ? ) usage;;
   esac
done
shift $(($OPTIND - 1))

#at lest one dir is provided
if [ $# -lt 1 ]; then
   usage
   exit 1
fi

#for dir in "$*"
while [ $# -gt 0 ]
do
   #find . -wholename './*test' -prune -o -iname "*.h" -o -iname "*.cpp" -o -iname "*.c" -o -iname "*.sig" | grep -v 'test$' > cscope.files

   find $CURRENT_DIR/$1 -iname "*.h" -o -iname "*.c" -o -iname "*.cpp" -o -iname "*.cc" >> cscope.files
   shift
done

#TODO: add common files

cscope -b -q

ctags -L cscope.files --c++-kinds=+p --fields=+iaS --extra=+q

#for filelookup plugin
echo -e "!_TAG_FILE_SORTED\t2\t/2=foldcase/" > filenametags
find . \( -iname "*.h" -o -iname "*.cpp" -o -iname "*.c" \) -printf "%f\t%p\t1\n" | sort -f >> ./filenametags

#FIND -prune syntax:
#find [path] [tests for stuff you want to prune] -prune -o [the stuff you'd normally put after the path]
