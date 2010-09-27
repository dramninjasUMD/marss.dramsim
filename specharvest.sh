#!/bin/bash 

# This script "harvests" the commands from specinvoke and generates little shell
# scripts that will run each command of the specinvoke inside of a MARSS virtual machine. 
#
# You can use this script along with vmutils.sh to produce a whole bunch of VM disk images
# that will auto start up marss simulations all in parallel on a many core machine. 
#
# If this sounds interesting to you and you want more details about how to use these files
# send an email to dramninjas (at) gmail (dot) com and I'll try to explain it

SPEC_DIR="$HOME/SPEC06"
OUTPUT_DIR="$HOME/marss.dramsim"
COUNTER=0


function generate_sim_desc_from_run_dir () {
	SIM_DESC=`echo "$1" | sed -n -e 's:^.*/CPU2006/[0-9]*.\([^/]*\)/.*:\1:p'`_$2
	echo $SIM_DESC
}

function generate_simulate_sh() {
if [ "$#" != "4" ] ; then 
	echo "usage: $0 command_string run_dir sim_number run_number" 
	return
fi 

local COMMAND_STR="$1"
local RUN_DIR=`echo "$2" | sed s:$SPEC_DIR/::`
local NUMBER="$3"
local RUN_NUMBER=$4
local OUTPUT_PATH="$OUTPUT_DIR/simulate$NUMBER.sh"
local SIM_DESC_STR=`generate_sim_desc_from_run_dir "$RUN_DIR" $RUN_NUMBER`
echo "Writing $NUMBER to $OUTPUT_PATH (rundir=$RUN_DIR, desc=$SIM_DESC_STR)" 
cat > $OUTPUT_PATH <<EOF
export PATH=\$PATH:/root
export OMP_NUM_THREADS=4

function simulate() {
	cd /root/mnt/SPEC06
	source shrc
	cd $RUN_DIR
	start_sim
	$COMMAND_STR
	stop_sim
}
simulate
#SIM_DESC="$SIM_DESC_STR"
EOF
echo "$NUMBER: $SIM_DESC_STR = '$COMMAND_STR'" >> $OUTPUT_DIR/spec.cmds
}

# so apparently specinvoke clobbers all kinds of local variables -- except
# running it in a subshell is not as easy as it seems
function specinvoke_to_cmd_strings() {
	local OLD_IFS=$IFS
#change the separator to iterate newlines instead of white spaces
	IFS=$'\n'
	aa=( `specinvoke -n` )
	for line in "${aa[@]}"
	do
		#echo back all non-commented lines
		if [ ! ${line:0:1} == "#" ] ; then
			# bashfu note: this grabs the first "word" from the line 
			filename=${line%% *}
			if [ -f "$filename" ] ; then
				echo $line
			fi 
		fi
	done
	#change back the separator so as not to screw up the shell 
	IFS=$OLD_IFS
}

pushd "$SPEC_DIR" > /dev/null
rm $OUTPUT_DIR/spec.cmds
source shrc
popd
COUNTER=0

OLD_IFS=$IFS

for run_dir in `find "$SPEC_DIR" -path '*run_base_ref*speccmds.cmd'`
do
	RUN_DIR=`dirname $run_dir`
	pushd "$RUN_DIR" > /dev/null
	returned_arr=$(specinvoke_to_cmd_strings)
	RUN_NUMBER=0
	IFS=$'\n'
	for cmd in $returned_arr
	do
		generate_simulate_sh $cmd $RUN_DIR $COUNTER $RUN_NUMBER
		COUNTER=$((COUNTER+1))
		RUN_NUMBER=$((RUN_NUMBER+1))
	done
	IFS=$OLD_IFS
	popd > /dev/null
done
echo "Will need to set up $COUNTER simulations"
