#!/bin/bash 

# Source this file (it won't do anything if you just run it) and then use the
# commands: vmsetup, vmclone, vmmount, vmumount, vmchroot
#
# This file contains a set of commands that make it easier to deal with marss disk images 
# I tried to briefly describe what the functions do, but if you need help
# you can send an email to dramninjas (at) gmail (dot) com and I can try
# to explain how to use these.
#
# The gist is that I have 3 raw disk images hda.raw (/root in the VM), hdb.raw (some
# benchmarks like SPEC06 mounted at /root/mnt in the VM), and hdc.raw (containing
# some autorun script at /root/sim/simulate.sh which will be auto run when the
# VM starts). These disk images are copied and the appropriate simulate.sh file is
# loaded into each one when vmsetup is called. vmrun then launches the virtual machine
# and the simulation within the VM begins automatically, allowing one to start up many
# instances at once -- only limited by processors and memory.
#
# The root image needs a bit of work to make this happen (such as autologin as
# root with mingetty) and modifications to /root/.bashrc, but the vmchroot
# command makes it easy to add these without having to actually run the image in QEMU


#adjust these paths to taste
QEMU_IMG_BIN="$HOME/marss.dramsim/qemu/qemu-img"
IMAGE_DIR="$HOME/disks"
MOUNT_POINT="$IMAGE_DIR/mnt"
# referred to from here on out as the "base image" 
IMAGE_NAME="parsec.base.raw"
IMAGE_PATH="$IMAGE_DIR/$IMAGE_NAME"

MARSS_DIR=$HOME/marss.dramsim


######################## The actual "public" functions ######################


function vmmount () {
	__mount_image $1
	__vmpushd $MOUNT_POINT 
#TODO: this doesn't always have to be done as root for things like the hdc image ... 
	sudo su
	vmumount
	__vmpopd 
}

#for symmetry's sake
function vmumount() {
	__umount_image
}

# vmclone -- Takes a raw disk image and creates a qcow2 copy of it
# run  'vmclone output.qcow2'  to make a copy of the "base image" 
# or run 'vmclone someimage.raw output.qcow2' if you want to make a clone of some other image
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

# vmrun x -- launch VM #x with the proper parameters for that number
# use 'vmsetup x' before using the vmrun command. Note: the VM will launch 
# in a detached screen. Please read up on how to use screen if you are 
# not familiar with it, or just remove the call to screen to run as a foreground
# task 

function vmrun() {
	if [ -z "$1" ] ; then
		echo "Specify a number"
		return
	fi 

	__vmpushd "$MARSS_DIR"
	# sed sure is hard to read ... this just grabs the argument of SIM_DESC= from the simulate file
	local SIM_DESCRIPTION=`cat simulate$1.sh | sed -n -e 's/^#SIM_DESC="\([^"]*\)"/\1/p '`
	if [ -z "$SIM_DESCRIPTION" ] ; then 
		echo "No sim description in simulate$1.sh -- did you use vmsetup? (not launching sim)"
		__vmpopd
		return
	fi 
	if [ ! -f "hda$1.qcow2" ] ; then 
		echo "ERROR: hda$1.qcow2 not found; can't start VM"
		return
	fi
	local hdb_string=""
	if [ -f "hdb$1.qcow2" ] ; then 
		hdb_string="-hdb hdb$1.qcow2"
	fi 
	local hdc_string=""
	if [ -f "hdc$1.raw" ] ; then 
		hdb_string="-hdc hdc$1.raw"
	fi 

		
	echo "Launching simulation #$1: $SIM_DESCRIPTION"
	local CMD_TO_RUN="SIM_DESC=\"$SIM_DESCRIPTION\" gdb -x gdbrun -args qemu/qemu-system-x86_64 -m 2GB -net nic,model=ne2k_pci -net user -simconfig \"simconfig$1.cfg\" -hda \"hda$1.qcow2\" $hdb_string $hdc_string -curses"
	screen -d -m -S "sim$1" bash -c "$CMD_TO_RUN"
	__vmpopd
}

function vmclean() {
	if [ -z "$1" ] ; then
		echo "specify a number"
		return
	fi 
	__vmpushd "$MARSS_DIR"
	rm hda$1.qcow2 hdb$1.qcow2 hdc$1.raw run$1.stats run$1.log simconfig$1.cfg simulate$1.sh
	__vmpopd
}

# vmsetup x [description] [shared|private] [nohdb] -- setup vm x with a sim description
# This command creates the necessary files, copies a simulate.sh file into the hdc disk image which can be auto launched if the VM is setup the right way.
# This function does the heavy lifting of this script -- if it sounds interesting, send dramninjas (at) gmail (dot) com an email and I can explain how
# the disk images are supposed to be set up for this to work right

function vmsetup() {
	if [ $# -lt 1 ] ; then 
		echo "usage: $0 sim_number [sim_description] [shared|private] [nohdb]"
		return
	fi 
	local usehdb=1
	local cache_config_string=""
	if [ "$3" == "shared" ] ; then
		cache_config_string="-cache-config-type shared_L2"	
	fi 
	if [ "$4" == "nohdb" ] ; then 
		usehdb=0
	fi 
	__vmpushd "$MARSS_DIR" 

#setup the simconfig file
	cat > simconfig$1.cfg <<EOF
-stats run$1.stats
-logfile run$1.log
-corefreq 2000000000
$cache_config_string 
EOF

local SIMULATE_FILE_SRC="simulate$1.sh"
local SIMULATE_FILE_DEST="$MOUNT_POINT/simulate.sh"


if [ -e "$SIMULATE_FILE_SRC" ] ; then 

	if [ -z "`grep 'SIM_DESC' $SIMULATE_FILE_SRC`" ] ; then 
		if [ -z "$2" ] ; then
			echo "please provide a sim description"
			return 
		else
			echo "#SIM_DESC=\"$2\"" >> $SIMULATE_FILE_SRC
		fi 
	fi 
	cp "$IMAGE_DIR/hdc.raw" "hdc$1.raw"

	__mount_image "hdc$1.raw" 
	echo "Copying $SIMULATE_FILE_SRC to $SIMULATE_FILE_DEST"
	sudo cp "$SIMULATE_FILE_SRC" "$SIMULATE_FILE_DEST"
	__umount_image
else 
	echo "WARNING: using default simulate.sh"
fi

	local images_to_clone="hda hdb"
	if [ $usehdb == 0 ] ; then 
		images_to_clone="hda"
	fi

for img in $images_to_clone
do
	local OUTPUT_IMG_NAME="${img}$1.qcow2"
#FIXME: need to do an lsof first to make sure the image isn't being used by a simulation before trying to do this
#	local BASE_IMG_NAME="$IMAGE_DIR/${img}0.qcow2"
#	if [ -e "$BASE_IMG_NAME" ] ; then 
#		echo "copying $BASE_IMG_NAME to $OUTPUT_IMG_NAME"
#		cp "$BASE_IMG_NAME" "$OUTPUT_IMG_NAME"
#	else
		vmclone "$IMAGE_DIR/$img.raw" "$OUTPUT_IMG_NAME"
#	fi 
done

__vmpopd
}

# vmmultirun x y -- setup and run VMs numbered x to y 
function vmmultirun () {
	if [ $# -lt 2 ] ; then
		echo "usage: $0 start end"
		return
	fi
	for i in `seq $1 $2` 
	do
		vmsetup $i
		vmrun $i
	done
}

# vmchroot -- mount the base image and chroot into it
# this is useful for mounting the root image and using it
# without having to actually start up QEMU. Things like 
# installing packages or compiling 
function vmchroot() {
	__mount_image $IMAGE_PATH; 
	__mount_proc_sys_dev;
	sudo chroot $MOUNT_POINT
	__unmount_proc_sys_dev;
	vmumount;
}


################## Utility functions #########################

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


# get rid of the annoying output of these commands
function __vmpushd() {
	pushd "$1" > /dev/null
}
function __vmpopd() {
	popd > /dev/null
}

