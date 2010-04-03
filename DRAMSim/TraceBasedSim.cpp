//TraceBasedSim.cpp
//
//File to run a trace-based simulation
//

#include <iostream>
#include <fstream>
#include <sstream>
#include <getopt.h>

#include "SystemConfiguration.h"
#include "Transaction.h"
#include "MemorySystem.h"
#include "Rank.h"

//number of hex characters in a uint64_t 
#define CHARS_PER_CHUNK 16

using namespace DRAMSim;
using namespace std;

#ifndef _SIM_
int SHOW_SIM_OUTPUT = 1; 
ofstream visDataOut; //mostly used in MemoryController

void usage() 
{
	cout << "DRAMSim [-t TRACEFILE] [-i INIFILE] [-c #] -k -q" <<endl;
	cout << "\t-t, --tracefile=FILENAME \tspecify a tracefile to run [default=traces/trace.trc] "<<endl;
	cout << "\t-s, --systemini=FILENAME \tspecify an ini file that describes the memory system parameters [default=ini/system.ini] "<<endl;
	cout << "\t-d, --deviceini=FILENAME \tspecify an ini file that describes the device-level parameters [default=ini/DDR2/default.ini] "<<endl;
	cout << "\t-c, --numcycles=# \t\tspecify number of cycles to run the simulation for [default=30] "<<endl;
	cout << "\t-q, --quiet \t\t\tflag to suppress simulation output (except final stats) [default=no]"<<endl;
	cout << "\t-o, --option=OPTION_A=234,OPTION_B=1234 \t\t\toverwrite any ini file option from the command line"<<endl;
	cout << "\t-p, --pwd=DIRECTORY\t\tSet the working directory (i.e. usually DRAMSim directory where ini/ and results/ are)"<<endl;
	cout << "\t-v \t\t\t\tnot implemented yet" <<endl;
}
#endif

void *parseTraceFileLine(string &line, uint64_t &addr, enum TransactionType &transType, uint64_t &clockCycle, TraceType type)
{
	size_t previousIndex=0;
	size_t spaceIndex=0;
	uint64_t *dataBuffer = NULL;
	string addressStr="", cmdStr="", dataStr="", ccStr="";
#ifndef _SIM_
	bool useClockCycle = false;
#else
	bool useClockCycle = true;
#endif

	switch(type)
		{
		case k6:				
			{
				spaceIndex = line.find_first_of(" ", 0);

				addressStr = line.substr(0, spaceIndex);
				previousIndex = spaceIndex;

				spaceIndex = line.find_first_not_of(" ", previousIndex);
				cmdStr = line.substr(spaceIndex, line.find_first_of(" ", spaceIndex) - spaceIndex);
				previousIndex = line.find_first_of(" ", spaceIndex);

				spaceIndex = line.find_first_not_of(" ", previousIndex);
				ccStr = line.substr(spaceIndex, line.find_first_of(" ", spaceIndex) - spaceIndex);

				if(cmdStr.compare("P_MEM_WR")==0 ||
					 cmdStr.compare("BOFF")==0)
					{
						transType = DATA_WRITE;
					}
				else if(cmdStr.compare("P_FETCH")==0 || 
								cmdStr.compare("P_MEM_RD")==0 ||
								cmdStr.compare("P_LOCK_RD")==0 ||
								cmdStr.compare("P_LOCK_WR")==0)
					{
						transType = DATA_READ;
					}
				else
					{
						ERROR("== WTF Command : "<<cmdStr);
						exit(0);
					}

				istringstream a(addressStr.substr(2));//gets rid of 0x
				a>>hex>>addr;

				//if this is set to false, clockCycle will remain at 0, and every line read from the trace
				//  will be allowed to be issued
				if(useClockCycle)
					{
						istringstream b(ccStr);
						b>>clockCycle;
					}
				break;
			}
		case mase:
			{
				spaceIndex = line.find_first_of(" ", 0);
				
				addressStr = line.substr(0, spaceIndex);
				previousIndex = spaceIndex;
				
				spaceIndex = line.find_first_not_of(" ", previousIndex);
				cmdStr = line.substr(spaceIndex, line.find_first_of(" ", spaceIndex) - spaceIndex);
				previousIndex = line.find_first_of(" ", spaceIndex);
				
				spaceIndex = line.find_first_not_of(" ", previousIndex);
				ccStr = line.substr(spaceIndex, line.find_first_of(" ", spaceIndex) - spaceIndex);
				
				if(cmdStr.compare("IFETCH")==0||
				   cmdStr.compare("READ")==0)
					{
						transType = DATA_READ;
					}
				else if(cmdStr.compare("WRITE")==0)
					{
						transType = DATA_WRITE;
					}
				else
					{
						ERROR("== Unknown command in tracefile : "<<cmdStr);
					}

				istringstream a(addressStr.substr(2));//gets rid of 0x
				a>>hex>>addr;

				//if this is set to false, clockCycle will remain at 0, and every line read from the trace
				//  will be allowed to be issued
				if(useClockCycle)
					{
						istringstream b(ccStr);
						b>>clockCycle;
					}
				
				break;
			}
		case misc:
			spaceIndex = line.find_first_of(" ", spaceIndex+1);
			if (spaceIndex == string::npos) 
				{
					ERROR("Malformed line: '"<< line <<"'");
				}

			addressStr = line.substr(previousIndex,spaceIndex);
			previousIndex=spaceIndex;

			spaceIndex = line.find_first_of(" ", spaceIndex+1);
			if (spaceIndex == string::npos) 
				{
					cmdStr = line.substr(previousIndex+1);
				}
			else
				{
					cmdStr = line.substr(previousIndex+1,spaceIndex-previousIndex-1);
					dataStr = line.substr(spaceIndex+1);
				}
	
			//convert address string -> number
			istringstream b(addressStr.substr(2)); //substr(2) chops off 0x characters
			b >>hex>> addr;

			// parse command
			if (cmdStr.compare("read") == 0) 
				{
					transType=DATA_READ;
				}
			else if (cmdStr.compare("write") == 0) 
				{
					transType=DATA_WRITE;
				}
			else
				{
					ERROR("INVALID COMMAND '"<<cmdStr<<"'");
					exit(-1);
				}
			if (SHOW_SIM_OUTPUT)
				{
					DEBUGN("ADDR='"<<hex<<addr<<dec<<"',CMD='"<<transType<<"'");//',DATA='"<<dataBuffer[0]<<"'");
				}

			//parse data
			//if we are running in a no storage mode, don't allocate space, just return NULL
#ifndef NO_STORAGE
			if (dataStr.size() > 0 && transType == DATA_WRITE) 
				{
					// 32 bytes of data per transaction 
					dataBuffer = (uint64_t *)calloc(sizeof(uint64_t),4);
					size_t strlen = dataStr.size();
					for (int i=0; i < 4; i++) 
						{
							size_t startIndex = i*CHARS_PER_CHUNK; 
							if (startIndex > strlen)
								{
									break;
								}
							size_t charsLeft = min(((size_t)CHARS_PER_CHUNK), strlen - startIndex + 1);
							string piece = dataStr.substr(i*CHARS_PER_CHUNK,charsLeft);
							istringstream iss(piece);
							iss >> hex >> dataBuffer[i];
						}
					PRINTN("\tDATA=");
					BusPacket::printData(dataBuffer);
				}
	
			PRINT("");
#endif
			break;
		}
	return dataBuffer;
}

#ifndef _SIM_

int main(int argc, char **argv)
{
	int c;
	string traceFileName = "";
	TraceType traceType;
	string systemIniFilename = "ini/system.ini";
	string deviceIniFilename = "";
	string pwdString = "";

	bool overrideOpt = false;
	string overrideKey = "";
	string overrideVal = "";
	string tmp = "";
	size_t equalsign;

	uint numCycles=30;
	//getopt stuff
	while (1) 
		{
			static struct option long_options[] = 
				{
					{"deviceini", required_argument, 0, 'd'},
					{"tracefile", required_argument, 0, 't'},
					{"systemini", required_argument, 0, 's'},
					{"pwd", required_argument, 0, 'p'},
					{"numcycles",  required_argument,	0, 'c'},
					{"option",  required_argument,	0, 'o'},
					{"quiet",  no_argument, &SHOW_SIM_OUTPUT, 'q'},
					{"help", no_argument, 0, 'h'},
					{0, 0, 0, 0}
				};
			int option_index=0; //for getopt
			c = getopt_long (argc, argv, "t:s:c:d:o:p:bkq", long_options, &option_index);
			if (c == -1) 
				{
					break;
				}
			switch (c) 
				{
				case 0: //TODO: figure out what the hell this does, cuz it never seems to get called
					if (long_options[option_index].flag != 0) //do nothing on a flag
						{
							printf("setting flag\n");
							break; 
						}
					printf("option %s",long_options[option_index].name);
					if (optarg)
						{
							printf(" with arg %s", optarg);
						}
					printf("\n");
					break;
				case 'h':
					usage();
					exit(0);
					break;
				case 't':
					traceFileName = string(optarg);
					break;
				case 's':
					systemIniFilename = string(optarg);
					break;
				case 'd':
					deviceIniFilename = string(optarg);
					break;
				case 'c':
					numCycles = atoi(optarg);
					break;
				case 'p':
					pwdString = string(optarg);
					break;
				case 'q':
					SHOW_SIM_OUTPUT=false;
					break;
				case 'o':
					tmp = string(optarg);
					equalsign = tmp.find_first_of('=');
					overrideKey = tmp.substr(0,equalsign);
					overrideVal = tmp.substr(equalsign+1,tmp.size()-equalsign+1);
					overrideOpt = true;
					break;
				case '?':
					usage();
					exit(-1);
					break;
				}
		}

	// get the trace filename
	string temp = traceFileName.substr(traceFileName.find_last_of("/")+1);

	//get the prefix of the trace name
	temp = temp.substr(0,temp.find_first_of("_"));
	if(temp=="mase")
		{
			traceType = mase;
		}
	else if(temp=="k6")
		{
			traceType = k6;
		}
	else if(temp=="misc")
		{
			traceType = misc;
		}
	else
		{
			ERROR("== Unknown Tracefile Type : "<<temp);
			exit(0);
		}


	// no default value for the default model name
	if (deviceIniFilename.length() == 0) {
		ERROR("Please provide a device ini file");
		usage();
		exit(-1);
	}


	//ignore the pwd argument if the argument is an absolute path
	if (pwdString.length() > 0 && traceFileName[0] != '/')
	{
			traceFileName = pwdString + "/" +traceFileName;
	}

	DEBUG("== Loading trace file '"<<traceFileName<<"' == ");

	ifstream traceFile;
	string line;


#ifdef MBOB_SYSTEM
	MBoBController *memorySystem;
	memorySystem = new MBoBController(deviceIniFilename, systemIniFilename, pwdString, traceFileName);
#else
	MemorySystem *memorySystem;
	memorySystem = new MemorySystem(0, deviceIniFilename, systemIniFilename, pwdString, traceFileName);
#endif

	uint64_t addr;
	uint64_t clockCycle=0;
	enum TransactionType transType; 

	void *data = NULL;
	int lineNumber = 0;
	Transaction trans;
	bool pendingTrans = false;

	uint64_t addrtemp = 0;
	traceFile.open(traceFileName.c_str());

	if(!traceFile.is_open())
		{
			cout << "== Error - Could not open trace file"<<endl;
			exit(0);
		}

	for(size_t i=0;i<numCycles;i++)
		{
			if(!pendingTrans)
				{
					if(!traceFile.eof())
						{
							getline(traceFile, line);
							
							if (line.size() > 0)
								{
									data = parseTraceFileLine(line, addr, transType,clockCycle, traceType);
									trans = Transaction(transType, addr, data);
									
									if(i>=clockCycle)
										{
											if(!(*memorySystem).addTransaction(trans))
													{
														pendingTrans = true;
													}
										}
									else
										{
											pendingTrans = true;
										}
								}
							else
								{
									DEBUG("WARNING: Skipping line "<<lineNumber<< " ('" << line << "') in tracefile"); 
								}
							lineNumber++;
						}
					else 
						{
							// generate some "random" addresses after the end of a trace if wes still have clock cycles left
							// beyond the end of a trace-- this was a pretty silly idea, but we'll leave this here for now
							// other idea: dereference a garbage pointer and use that value as a memory address :)
#if 0
							addrtemp ^= 0x12345678;
							addrtemp = addrtemp << 5;
							trans = Transaction(transType, addrtemp, data);
							pendingTrans =true;
#endif
						}
				}

			else if(pendingTrans && i >= clockCycle)
				{
					pendingTrans = !(*memorySystem).addTransaction(trans);
				}

			(*memorySystem).update();
#ifdef MBOB_SYSTEM
			//mbob controller needs to step() too
			(*memorySystem).step();
#endif			
		}
	
	traceFile.close();
	(*memorySystem).printStats(true);
	// make valgrind happy
	delete(memorySystem);
}
#endif
