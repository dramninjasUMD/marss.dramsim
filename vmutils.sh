#!/bin/bash -x
QEMU_IMG_BIN="$HOME/marss.dramsim/qemu/qemu-img"
IMAGE_DIR="$HOME/disks"
MOUNT_POINT="$IMAGE_DIR/mnt"
IMAGE_NAME="parsec.base.raw"
IMAGE_PATH="$IMAGE_DIR/$IMAGE_NAME"
MARSS_DIR=$HOME/marss.dramsim

# so I'm not sure why something like dirname $HOME returns /home when I'd
# expect it to return /home/USER, but I need to make a function that is sane --
# i.e. if the argument is a path to a file, return the directory the file is in,
# but if the directory is already a directory, just return the absolute path to it
function __absdirname() {
	if [ -d "$1" ] ; then
		echo $(__abspath "$1")
	else
		echo $(__abspath $(dirname "$1"))
	fi
}

function __abspath() {
	if [ -d "$1" ] ; then 
		local DIR=$1
	else 
		local DIR=$(dirname "$1")
	fi
	__vmpushd $DIR
	echo $PWD
	__vmpopd
}

function __mount_image () {
	local PARTITION_NUM=1
	
	if [ -e "$1" ] ; then # a real filename given 
		local FILENAME=$( basename "$1" )
		local FILE_DIR=$( __absdirname "$1" )
	else
		local FILENAME="$IMAGE_FILE"
		local FILE_DIR="$IMAGE_DIR"
	fi

	local PARTITION_TYPE="ext2"
	echo "Mounting $PARTITION_TYPE partition: image=$FILE_DIR/$FILENAME to $MOUNT_POINT ... "

	local OFFSET=`sfdisk -l -uS "$FILE_DIR/$FILENAME" 2> /dev/null | grep "$FILENAME$PARTITION_NUM" | awk '{print $2}'`
	local SECTOR_OFFSET=$((OFFSET*512))
	sudo mount -t $PARTITION_TYPE -o loop,offset=$SECTOR_OFFSET "$FILE_DIR/$FILENAME" "$MOUNT_POINT"
}

function __umount_image () {
	echo -n "unmounting image from $MOUNT_POINT ... "
	__vmpushd $HOME 
	sudo umount "$MOUNT_POINT";
	__vmpopd 
	sync
	echo "OK"
}

#for symmetry's sake
function vmumount() {
	__umount_image
}

function vmmount () {
	__mount_image $1
	__vmpushd $MOUNT_POINT 
#TODO: this doesn't always have to be done as root for things like the hdc image ... 
	sudo su
	vmumount
	__vmpopd 
}

# get rid of the annoying output of these commands
function __vmpushd() {
	pushd "$1" > /dev/null
}
function __vmpopd() {
	popd > /dev/null
}

function vmclone() {
#TODO: maybe use getopt to add a -force flag to override the output file check
	if [ "$#" == "2" ] ; then 
		# try both the filename and to prefix the IMAGE_DIR directory to see if the file exists
		if [ -e "$1" ] ; then 
			local INPUT_FILENAME="$1"
		elif [ -e "$IMAGE_DIR/$1" ] ; then
			local INPUT_FILENAME="$IMAGE_DIR/$1"
		else 
			echo "Input image $1 not found"
			return
		fi

		if [ -e "$2" ] ; then 
			echo "Output file $2 already exists" 
			return
		fi

		local OUTPUT_FILENAME="$2"
	elif [ "$#" == "1" ] ; then 
		if [ -e "$1" ] ; then 
			echo "Output file $2 already exists" 
			return
		fi

		local INPUT_FILENAME="$IMAGE_PATH"
		local OUTPUT_FILENAME="$1"
	else 
		echo "usage: $0 output_filename [input_filename] -- Must at least specify output file ";
		return 
	fi 

	echo "cloning $INPUT_FILENAME to $OUTPUT_FILENAME..."
	$QEMU_IMG_BIN convert -f raw "$INPUT_FILENAME" -O qcow2 "$OUTPUT_FILENAME"
}


function vmrun() {
	if [ -z "$1" ] ; then
		echo "Specify a number"
		return
	fi 

	# sed sure is hard to read ... this just grabs the argument of SIM_DESC= from the simulate file
	local SIM_DESCRIPTION=`cat simulate$1.sh | sed -n -e 's/^#SIM_DESC="\([^"]*\)"/\1/p '`
	if [ -z "$SIM_DESCRIPTION" ] ; then 
		echo "No sim description -- did you use vmsetup? "
		return
	else 
	fi 

	"Launching simulation #$1: $SIM_DESCRIPTION"
	__vmpushd "$MARSS_DIR"
	local CMD_TO_RUN="SIM_DESC=\"$SIM_DESCRIPTION\" gdb -x gdbrun -args qemu/qemu-system-x86_64 -m 2GB -simconfig \"simconfig$1.cfg\" -hda \"hda$1.qcow2\" -hdb \"hdb$1.qcow2\" -hdc \"hdc$1.raw\" -curses"
	screen -S "sim$1" $CMD_TO_RUN
	__vmpopd
}

function vmsetup() {
	if [ $# -lt 1 ] ; then 
		echo "usage: $0 sim_number [sim_description]"
		return
	fi 
	__vmpushd "$MARSS_DIR" 

#setup the simconfig file
	cat > simconfig$1.cfg <<EOF
-stats run$1.stats
-logfile run$1.log
-corefreq 2000000000
EOF

local SIMULATE_FILE_SRC="simulate$1.sh"
local SIMULATE_FILE_DEST="$MOUNT_POINT/simulate.sh"

cp "$IMAGE_DIR/hdc.raw" "hdc$1.raw"

if [ -e "$SIMULATE_FILE_SRC" ] ; then 

	if [ -z "`grep 'SIM_DESC' $SIMULATE_FILE_SRC`" ] ; then 
		if [ -z "$2" ] ; then
			echo "please provide a sim description"
			return 
		else
			echo "#SIM_DESC=\"$2\"" >> $SIMULATE_FILE_SRC
		fi 
	fi 

	__mount_image "hdc$1.raw" 
	echo "Copying $SIMULATE_FILE_SRC to $SIMULATE_FILE_DEST"
	sudo cp "$SIMULATE_FILE_SRC" "$SIMULATE_FILE_DEST"
	__umount_image
else 
	echo "WARNING: using default simulate.sh"
fi


for img in "hda" "hdb"
do
	local OUTPUT_IMG_NAME="${img}$1.qcow2"
	local BASE_IMG_NAME="$IMAGE_DIR/${img}0.qcow2"
	if [ -e "$BASE_IMG_NAME" ] ; then 
		echo "copying $BASE_IMG_NAME to $OUTPUT_IMG_NAME"
		cp "$BASE_IMG_NAME" "$OUTPUT_IMG_NAME"
	else
		vmclone "$IMAGE_DIR/$img.raw" "$OUTPUT_IMG_NAME"
	fi 
done

__vmpopd
}

function __mount_proc_sys_dev () {
	sudo mount -o bind /proc "$MOUNT_POINT/proc"
	sudo mount -o bind /dev "$MOUNT_POINT/dev"
	sudo mount -o bind /sys "$MOUNT_POINT/sys"
	sudo mount -o bind /dev/pts "$MOUNT_POINT/dev/pts"
	sudo cp /etc/resolv.conf "$MOUNT_POINT/etc/resolv.conf"
}

function __unmount_proc_sys_dev () {
	pushd $HOME > /dev/null
	sudo umount "$MOUNT_POINT/dev/pts"
	sudo umount "$MOUNT_POINT/dev"
	sudo umount "$MOUNT_POINT/sys"
	sudo umount "$MOUNT_POINT/proc"
	popd > /dev/null
}

function vmchroot() {
	__mount_image $IMAGE_PATH; 
	__mount_proc_sys_dev;
	sudo chroot $MOUNT_POINT
	__unmount_proc_sys_dev;
	vmumount;
}


