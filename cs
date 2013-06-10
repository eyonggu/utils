#!/bin/bash -x

PROGRAM_NAME=$(basename $0)

ROOT_PATH="."
EXCLUDED_PATH=

usage() {
    echo ""
    echo "$PROGRAM_NAME [-f] [-e pattern] dir1 dir2 ... "
    echo "  -f             with full path in cscope database "
    echo "  -e pattern     exclude directories whose name matches pattern "
    exit 1
}

#clean old database files
for file in cscope.files cscope.out cscope.in.out cscope.po.out fileametags tags
do
  rm -rf $file 
done

#parse options
while getopts "fe:" OPTION 
do
   case $OPTION in
      f ) ROOT_PATH=$(pwd);;
      e ) EXCLUDED_PATH="*/$OPTARG";;
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
   find $ROOT_PATH/$1 -wholename "$EXCLUDED_PATH" -prune -o \( -iname "*.h" -o -iname "*.c" -o -iname "*.cpp" -o -iname "*.cc" \) -print >> cscope.files
   shift
done

#TODO: add common files

cscope -b -q

ctags -L cscope.files --c++-kinds=+p --fields=+iaS --extra=+q

#for filelookup plugin
echo -e "!_TAG_FILE_SORTED\t2\t/2=foldcase/" > filenametags
find . \( -iname "*.h" -o -iname "*.c" -o -iname "*.cpp" -o -iname "*.cc" \) -printf "%f\t%p\t1\n" | sort -f >> ./filenametags

