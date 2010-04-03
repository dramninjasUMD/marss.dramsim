#ifndef DRAMSIM_H
#define DRAMSIM_H
/* 
 * This is a public header for DRAMSim including this along with libdramsim.so should
 * provide all necessary functionality to talk to an external simulator 
 */
#include "Callback.h"
using std::string;

namespace DRAMSim 
{
	class MemorySystem
	{
		public:

			MemorySystem(uint id, string dev, string sys, string pwd, string trc);
			bool addTransaction(bool isWrite, uint64_t addr);
			bool WillAcceptTransaction();
			void update();
			void RegisterCallbacks( 
				TransactionCompleteCB *readDone,
				TransactionCompleteCB *writeDone,
				void (*reportPower)(double bgpower, double burstpower, double refreshpower, double actprepower));
	};
}


#endif
