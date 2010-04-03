# MARSS uses scons, DRAMSim uses make so this file just a hack to 
# build both

all: 
	make -C DRAMSim libdramsim.so
# the c parameter is the number of cores to compile QEMU with 
	scons debug=0 pretty=0 c=4

clean:
	make -C DRAMSim clean
	scons -c
	
