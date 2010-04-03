// SystemParameters.cs created with MonoDevelop
// User: elliott at 6:07 PM 2/11/2009
//
// To change standard headers go to Edit->Preferences->Coding->Standard Headers
//

using System;
using System.ComponentModel;
using System.IO;

namespace DRAMVis
{
	public enum RowBufferPolicy {open_page,close_page};
	public enum SchedulingPolicy {rank_then_bank_round_robin,bank_then_rank_round_robin};
	public enum AddressMappingScheme {scheme1,scheme2,scheme3,scheme4,scheme5,scheme6};
	public enum QueuingStructure {per_rank, per_rank_per_bank};
	
	public class SystemParameters
	{
		private long _TOTAL_STORAGE=0;
		private uint _NUM_RANKS = 2;
		private uint _NUM_CHANS = 1;
		private uint _CACHE_LINE_SIZE = 8;
		private uint _JEDEC_DATA_BUS_WIDTH = 64;
		private uint _TRANS_QUEUE_DEPTH = 8;
		private uint _CMD_QUEUE_DEPTH = 8;
		private bool _USE_LOW_POWER = true;
		private uint _EPOCH_COUNT = 5000;
		private uint _TOTAL_ROW_ACCESSES = 5;
		private RowBufferPolicy _ROW_BUFFER_POLICY = RowBufferPolicy.open_page;
		private SchedulingPolicy _SCHEDULING_POLICY = SchedulingPolicy.bank_then_rank_round_robin;
		private AddressMappingScheme _ADDRESS_MAPPING_SCHEME = AddressMappingScheme.scheme1;
		private QueuingStructure _QUEUING_STRUCTURE = QueuingStructure.per_rank_per_bank;
		private bool _DEBUG_TRANS_Q = false;
		private bool _DEBUG_CMD_Q = false;
		private bool _DEBUG_ADDR_MAP = false;
		private bool _DEBUG_BANKSTATE = false;
		private bool _DEBUG_BUS = false;
		private bool _DEBUG_BANKS = false;
		private bool _DEBUG_POWER = false;
		private bool _VERIFICATION_OUTPUT = false;

		[CategoryAttribute("System Settings"), DescriptionAttribute("Total physical memory in test system"),
		 ReadOnly(true)]
		public long TOTAL_STORAGE
		{
			get { return _TOTAL_STORAGE; }
			set { _TOTAL_STORAGE = value; }
		}
		
		[CategoryAttribute("System Settings"), DescriptionAttribute("Total number of independent ranks in test system")]
		public uint NUM_RANKS
		{
			get { return _NUM_RANKS; }
			set 
			{ 
				_TOTAL_STORAGE = (_TOTAL_STORAGE / _NUM_RANKS) * value;
				_NUM_RANKS = value; 
			}
		}
		
		[CategoryAttribute("System Settings"), DescriptionAttribute("Total number of independent channels in test system")]
		public uint NUM_CHANS
		{
			get { return _NUM_CHANS; }
			set { _NUM_CHANS = value; }
		}
		
		[CategoryAttribute("System Settings"), DescriptionAttribute("Cache line size (in bytes) of highest level cache")]		
		public uint CACHE_LINE_SIZE
		{
			get { return _CACHE_LINE_SIZE; }
			set { _CACHE_LINE_SIZE = value; }
		}
		
		[CategoryAttribute("System Settings"), DescriptionAttribute("Data bus width (in bytes) of a JEDEC standard system")]
		public uint JEDEC_DATA_BUS_WIDTH
		{
			get { return _JEDEC_DATA_BUS_WIDTH; }
			set { _JEDEC_DATA_BUS_WIDTH = value; }
		}
		
		[CategoryAttribute("System Settings"), DescriptionAttribute("Time length of epoch (in cycles)")]
		public uint EPOCH_COUNT
		{
			get { return _EPOCH_COUNT; }
			set { _EPOCH_COUNT = value; }
		}
		
		[CategoryAttribute("Controller Settings"), DescriptionAttribute("Depth of transaction queue (requests from CPU)")]
		public uint TRANS_QUEUE_DEPTH
		{
			get { return _TRANS_QUEUE_DEPTH; }
			set { _TRANS_QUEUE_DEPTH = value; }
		}
		
		[CategoryAttribute("Controller Settings"), DescriptionAttribute("Depth of command queue (DRAM level commands)")]
		public uint CMD_QUEUE_DEPTH
		{
			get { return _CMD_QUEUE_DEPTH; }
			set { _CMD_QUEUE_DEPTH = value; }
		}
		
		[CategoryAttribute("Controller Settings"), DescriptionAttribute("Flag that determines whether to use low-power mode")]	
		public bool USE_LOW_POWER
		{
			get { return _USE_LOW_POWER; }
			set { _USE_LOW_POWER = value; }
		}
		
		[CategoryAttribute("Controller Settings"), DescriptionAttribute("Numer of open-page row accesses before closing")]
		public uint TOTAL_ROW_ACCESSES
		{
			get { return _TOTAL_ROW_ACCESSES; }
			set { _TOTAL_ROW_ACCESSES = value; }
		}
		
		[CategoryAttribute("Controller Settings"), DescriptionAttribute("Determines whether or not a row is kept open after a column access")]
		public RowBufferPolicy ROW_BUFFER_POLICY
		{
			get { return _ROW_BUFFER_POLICY; }
			set { _ROW_BUFFER_POLICY = value; }
		}
		
		[CategoryAttribute("Controller Settings"), DescriptionAttribute("Determines which queue to pull from next")]
		public SchedulingPolicy SCHEDULING_POLICY
		{
			get { return _SCHEDULING_POLICY; }
			set { _SCHEDULING_POLICY = value; }
		}
		
		[CategoryAttribute("Controller Settings"), DescriptionAttribute("Scheme used to map a physical address to row/col/bank/etc")]
		public AddressMappingScheme ADDRESS_MAPPING_SCHEME
		{
			get { return _ADDRESS_MAPPING_SCHEME; }
			set { _ADDRESS_MAPPING_SCHEME = value; }
		}
		
		[CategoryAttribute("Controller Settings"), DescriptionAttribute("Determines structure of queues in controller")]
		public QueuingStructure QUEUING_STRUCTURE
		{
			get { return _QUEUING_STRUCTURE; }
			set { _QUEUING_STRUCTURE = value; } 
		}
		
		//[CategoryAttribute("Simulation Settings"), DescriptionAttribute("Print transaction queue contents (every cycle)")]
		[BrowsableAttribute(false)]
		public bool DEBUG_TRANS_Q
		{
			get { return _DEBUG_TRANS_Q; }
			set { _DEBUG_TRANS_Q = value; }
		}
		
		//[CategoryAttribute("Simulation Settings"), DescriptionAttribute("Print command queue contents (every cycle)")]
		[BrowsableAttribute(false)]
		public bool DEBUG_CMD_Q
		{
			get { return _DEBUG_CMD_Q; }
			set { _DEBUG_CMD_Q = value; }
		}
		
		//[CategoryAttribute("Simulation Settings"), DescriptionAttribute("Print the mapping of a physical address (every cycle)")]
		[BrowsableAttribute(false)]
		public bool DEBUG_ADDR_MAP
		{
			get { return _DEBUG_ADDR_MAP; }
			set { _DEBUG_ADDR_MAP = value; }
		}
		
		//[CategoryAttribute("Simulation Settings"), DescriptionAttribute("Print bank states (every cycle)")]
		[BrowsableAttribute(false)]		
		public bool DEBUG_BANKSTATE
		{
			get { return _DEBUG_BANKSTATE; }
			set { _DEBUG_BANKSTATE = value; }
		}
		
		//[CategoryAttribute("Simulation Settings"), DescriptionAttribute("Print bus transactions")]
		[BrowsableAttribute(false)]
		public bool DEBUG_BUS
		{
			get { return _DEBUG_BUS; }
			set { _DEBUG_BUS = value; }
		}
		
		//[CategoryAttribute("Simulation Settings"), DescriptionAttribute("Print reading and writing to bank object")]
		[BrowsableAttribute(false)]
		public bool DEBUG_BANKS
		{
			get { return _DEBUG_BANKS; }
			set { _DEBUG_BANKS = value; }
		}
		
		//[CategoryAttribute("Simulation Settings"), DescriptionAttribute("Print power statistics gathering (every cycle)")]
		[BrowsableAttribute(false)]
		public bool DEBUG_POWER
		{
			get { return _DEBUG_POWER; }
			set { _DEBUG_POWER = value; }
		}
		
		//[CategoryAttribute("Simulation Settings"), DescriptionAttribute("Print output for ModelSIM verification")]
		[BrowsableAttribute(false)]
		public bool VERIFICATION_OUTPUT
		{
			get { return _VERIFICATION_OUTPUT; }
			set { _VERIFICATION_OUTPUT = value; }
		}

		public SystemParameters(){}
		
		public string GetVisFilename() {
			string sched = "RtB";
			string queue = "pRank";
			if (this._SCHEDULING_POLICY == SchedulingPolicy.bank_then_rank_round_robin) {
				sched = "BtR";
			}
			if (this._QUEUING_STRUCTURE == QueuingStructure.per_rank_per_bank) {
				queue = "pRankpBank";
			}
			Console.WriteLine("TOTAL_STORAGE = "+_TOTAL_STORAGE);
			string s = _TOTAL_STORAGE/(1024*1024*1024)+"GB."+_NUM_CHANS+"Ch."+_NUM_RANKS+"R."+ADDRESS_MAPPING_SCHEME+"."+_ROW_BUFFER_POLICY+"."+_TRANS_QUEUE_DEPTH+"TQ."+_CMD_QUEUE_DEPTH+"CQ."+sched+"."+queue+".vis";
			Console.WriteLine("FILENAME="+s);
			return s;

		}
		public void PrintToFile(string filename)
		{
			StreamWriter sw = new StreamWriter(filename);
			sw.WriteLine("NUM_RANKS="+this._NUM_RANKS);
			sw.WriteLine("NUM_CHANS="+this._NUM_CHANS);
			//sw.WriteLine("TOTAL_STORAGE="+this._TOTAL_STORAGE);
			sw.WriteLine("JEDEC_DATA_BUS_WIDTH="+this._JEDEC_DATA_BUS_WIDTH);
			sw.WriteLine("CACHE_LINE_SIZE="+this._CACHE_LINE_SIZE);
			sw.WriteLine("TRANS_QUEUE_DEPTH="+this._TRANS_QUEUE_DEPTH);
			sw.WriteLine("CMD_QUEUE_DEPTH="+this._CMD_QUEUE_DEPTH);
			sw.WriteLine("EPOCH_COUNT="+this._EPOCH_COUNT);
			sw.WriteLine("ROW_BUFFER_POLICY="+this._ROW_BUFFER_POLICY.ToString());
			sw.WriteLine("ADDRESS_MAPPING_SCHEME="+this._ADDRESS_MAPPING_SCHEME.ToString());
			sw.WriteLine("SCHEDULING_POLICY="+this._SCHEDULING_POLICY.ToString());
			sw.WriteLine("QUEUING_STRUCTURE="+this._QUEUING_STRUCTURE.ToString());
			sw.WriteLine("DEBUG_TRANS_Q="+this._DEBUG_TRANS_Q);
			sw.WriteLine("DEBUG_CMD_Q="+this._DEBUG_CMD_Q);
			sw.WriteLine("DEBUG_ADDR_MAP="+this._DEBUG_ADDR_MAP);
			sw.WriteLine("DEBUG_BUS="+this._DEBUG_BUS);
			sw.WriteLine("DEBUG_BANKSTATE="+this._DEBUG_BANKSTATE);
			sw.WriteLine("DEBUG_BANKS="+this._DEBUG_BANKS);
			sw.WriteLine("DEBUG_POWER="+this._DEBUG_POWER);
			sw.WriteLine("USE_LOW_POWER="+this._USE_LOW_POWER);
			sw.WriteLine("VERIFICATION_OUTPUT="+this._VERIFICATION_OUTPUT);
			sw.WriteLine("TOTAL_ROW_ACCESSES="+this._TOTAL_ROW_ACCESSES);
			
			sw.Close();
		}
	}
}
