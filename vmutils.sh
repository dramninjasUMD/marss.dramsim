#!/bin/bash -x
QEMU_IMG_BIN="$HOME/marss.dramsim/qemu/qemu-img"
IMAGE_DIR="$HOME/disks"
MOUNT_POINT="$IMAGE_DIR/mnt"
IMAGE_NAME="parsec.base.raw"
IMAGE_PATH="$IMAGE_DIR/$IMAGE_NAME"
MARSS_DIR=$HOME/marss.dramsim

function __mount_image () {
	local PARTITION_NUM=1
	if [ -e "$1" ] ; then # a real filename given 
		local FILENAME="$1"
		local FILE_DIR="$2"
	else
		local FILENAME="$IMAGE_FILE"
		local FILE_DIR="$IMAGE_DIR"
		return
	fi

	local PARTITION_TYPE="ext2"
	local OFFSET=`sfdisk -l -uS "$FILE_DIR/$FILENAME" 2> /dev/null | grep "$FILENAME$PARTITION_NUM" | awk '{print $2}'`
	local SECTOR_OFFSET=$((OFFSET*512))
	sudo mount -t $PARTITION_TYPE -o loop,offset=$SECTOR_OFFSET "$FILE_DIR/$FILENAME" "$MOUNT_POINT"
}

function vmumount() {
	echo "unmounting image from $MOUNT_POINT"
	pushd $HOME > /dev/null
	sudo umount "$MOUNT_POINT";
	popd > /dev/null
	sync;
}

function vmmount () {
	__mount_image $1
	__vmpushd $MOUNT_POINT 
	if [ -z "$1" ] ; then 
		sudo su
	else
		bash
	fi
	vmumount
	__vmpopd 
}

function vmclone() {
	if [ "$#" == "2" ] ; then 
		if [ -e "$1" ] ; then 
			local INPUT_FILENAME="$1"
		elif [ -e "$IMAGE_DIR/$1" ] ; then
			local INPUT_FILENAME="$IMAGE_DIR/$1"
		else 
			echo "Image $1 not found"
			return
		fi

		local OUTPUT_FILENAME="$2"
	elif [ "$#" == "1" ] ; then 
		local INPUT_FILENAME="$IMAGE_PATH"
		local OUTPUT_FILENAME="$1"
	else 
		echo "Must at least specify output file ";
		return 
	fi 

	echo "cloning $INPUT_FILENAME to $OUTPUT_FILENAME..."
	$QEMU_IMG_BIN convert -f raw "$INPUT_FILENAME" -O qcow2 "$OUTPUT_FILENAME"
}

function __vmpushd() {
	pushd "$1" > /dev/null
}
function __vmpopd() {
	popd > /dev/null
}

function vmrun() {
	if [ -z "$1" ] ; then
		echo "Specify a number"
		return
	fi 

	__vmpushd "$MARSS_DIR"
	screen -d -m -S "sim$1" qemu/qemu-system-x86_64 -m 2GB -simconfig "simconfig$1.cfg" -hda "hda$1.qcow2" -hdb "hdb$1.qcow2" -hdc "hdc$1.raw" -curses 
	__vmpopd

}
function vmsetup() {
	if [ -z "$1" ] ; then
		echo "Specify a number"
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
	__mount_image "hdc$1.raw" `pwd`
	echo "Copying $SIMULATE_FILE_SRC to $SIMULATE_FILE_DEST"
	sudo cp "$SIMULATE_FILE_SRC" "$SIMULATE_FILE_DEST"
	vmumount
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


