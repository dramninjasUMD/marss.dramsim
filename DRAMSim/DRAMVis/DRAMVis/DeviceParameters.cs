// DeviceParameters.cs created with MonoDevelop
// User: elliott at 9:56 PM 2/11/2009
//
// To change standard headers go to Edit->Preferences->Coding->Standard Headers
//

using System;
using System.ComponentModel;
namespace DRAMVis
{
	public class DeviceParameters
	{
		private uint _NUM_BANKS;
		private uint _NUM_ROWS;
		private uint _NUM_COLS;
		private uint _DEVICE_WIDTH;
		private uint _REFRESH_PERIOD;
		
		private float _tCK;
		private uint _CL;
		private uint _AL;
		private uint _BL;
		private uint _tRAS;
		private uint _tRCD;
		private uint _tRRD;
		private uint _tRC;
		private uint _tRP;
		private uint _tCCD;
		private uint _tRTP;
		private uint _tWTR;
		private uint _tWR;
		private uint _tRTRS;
		private uint _tRFC;
		private uint _tFAW;
		private uint _tCKE;
		private uint _tXP;
		private uint _tCMD;
		
		private uint _IDD0;
		private uint _IDD1;
		private uint _IDD2P;
		private uint _IDD2Q;
		private uint _IDD2N;
		private uint _IDD3Pf;
		private uint _IDD3Ps;
		private uint _IDD3N;
		private uint _IDD4W;
		private uint _IDD4R;
		private uint _IDD5;
		private uint _IDD6;
		private uint _IDD6L;
		private uint _IDD7;
		
		[CategoryAttribute("Physical"), DescriptionAttribute("Number of internal and independent arrays of DRAM cells"),
		 ReadOnly(true)]
		public uint NUM_BANKS
		{
			get { return _NUM_BANKS; }
			set { _NUM_BANKS = value; }
		}
		
		[CategoryAttribute("Physical"), DescriptionAttribute("Number of rows within a bank"),
		 ReadOnly(true)]
		public uint NUM_ROWS
		{
			get { return _NUM_ROWS; }
			set { _NUM_ROWS = value; }
		}
		
		[CategoryAttribute("Physical"), DescriptionAttribute("Number of columns within a bank"),
		 ReadOnly(true)]
		public uint NUM_COLS
		{
			get { return _NUM_COLS; }
			set { _NUM_COLS = value; }
		}
		
		[CategoryAttribute("Physical"), DescriptionAttribute("Output data width (also - column size)"),
		 ReadOnly(true)]
		public uint DEVICE_WIDTH
		{
			get { return _DEVICE_WIDTH; }
			set { _DEVICE_WIDTH = value; }
		}
		
		[CategoryAttribute("Physical"), DescriptionAttribute("Time between successive REFRESH commands (in ns)"),
		 ReadOnly(true)]
		public uint REFRESH_PERIOD
		{
			get { return _REFRESH_PERIOD; }
			set { _REFRESH_PERIOD = value; }
		}
		
		[CategoryAttribute("Timing"), DescriptionAttribute("Period of clock (in ns)"),
		 ReadOnly(true)]
		public float tCK
		{
			get { return _tCK; }
			set { _tCK = value; }
		}
		
		[CategoryAttribute("Timing"), DescriptionAttribute("Column access latency (in clock cycles)"),
		 ReadOnly(true)]
		public uint CL
		{
			get { return _CL; }
			set { _CL = value; }
		}
		
		[CategoryAttribute("Timing"), DescriptionAttribute("Additive latency value (in clock cycles) - Used for posted-CAS, typically with a value of tRCD-1"),
		 ReadOnly(true)]
		public uint AL
		{
			get { return _AL; }
			set { _AL = value; }
		}
		
		[CategoryAttribute("Timing"), DescriptionAttribute("Burst length (in DDR spec 'beats')"),
		 ReadOnly(true)]
		public uint BL
		{
			get { return _BL; }
			set { _BL = value; }
		}
		
		[CategoryAttribute("Timing"), DescriptionAttribute("ACTIVATE-to-PRECHARGE delay (in clock cycles)"),
		 ReadOnly(true)]
		public uint tRAS
		{
			get { return _tRAS; }
			set { _tRAS = value; }
		}
		
		[CategoryAttribute("Timing"), DescriptionAttribute("ACTIVATE-to-READ or WRITE delay (in clock cycles)"),
		 ReadOnly(true)]
		public uint tRCD
		{
			get { return _tRCD; }
			set { _tRCD = value; }
		}
		
		[CategoryAttribute("Timing"), DescriptionAttribute("ACTIVATE-to-ACTIVATE delay to different banks (in clock cycles)"),
		 ReadOnly(true)]
		public uint tRRD
		{
			get { return _tRRD; }
			set { _tRRD = value; }
		}
		
		[CategoryAttribute("Timing"), DescriptionAttribute("ACTIVATE-to-ACTIVATE delay to same bank (in clock cycles)"),
		 ReadOnly(true)]
		public uint tRC
		{
			get { return _tRC; }
			set { _tRC = value; }
		}
		
		[CategoryAttribute("Timing"), DescriptionAttribute("PRECHARGE period (in clock cycles)"),
		 ReadOnly(true)]
		public uint tRP
		{
			get { return _tRP; }
			set { _tRP = value; }
		}
		
		[CategoryAttribute("Timing"), DescriptionAttribute("Delay between column accesses to same open row (in clock cycles)"),
		 ReadOnly(true)]
		public uint tCCD
		{
			get { return _tCCD; }
			set { _tCCD = value; }
		}
		
		[CategoryAttribute("Timing"), DescriptionAttribute("READ-to-PRECHARGE delay (in clock cycles)"),
		 ReadOnly(true)]
		public uint tRTP
		{
			get { return _tRTP; }
			set { _tRTP = value; }
		}
		
		[CategoryAttribute("Timing"), DescriptionAttribute("Internal WRITE-to-READ delay (in clock cycles)"),
		 ReadOnly(true)]
		public uint tWTR
		{
			get { return _tWTR; }
			set { _tWTR = value; }
		}
		
		[CategoryAttribute("Timing"), DescriptionAttribute("WRITE recovery time (in clock cycles)"),
		 ReadOnly(true)]
		public uint tWR
		{
			get { return _tWR; }
			set { _tWR = value; }
		}
		
		[CategoryAttribute("Timing"), DescriptionAttribute("Rank-to-Rank switching time (in clock cycles)"),
		 ReadOnly(true)]
		public uint tRTRS
		{
			get { return _tRTRS; }
			set { _tRTRS = value; }
		}
		
		[CategoryAttribute("Timing"), DescriptionAttribute("REFRESH-to-ACTIVATE delay (in clock cycles)"),
		 ReadOnly(true)]
		public uint tRFC
		{
			get { return _tRFC; }
			set { _tRFC = value; }
		}
		
		[CategoryAttribute("Timing"), DescriptionAttribute("Four bank activation window (in clock cycles)"),
		 ReadOnly(true)]
		public uint tFAW
		{
			get { return _tFAW; }
			set { _tFAW = value; }
		}
		
		[CategoryAttribute("Timing"), DescriptionAttribute("Minimum CKE high/low transition time (in clock cycles)"),
		 ReadOnly(true)]
		public uint tCKE
		{
			get { return _tCKE; }
			set { _tCKE = value; }
		}
		
		[CategoryAttribute("Timing"), DescriptionAttribute("Exit power-down mode delay (in clock cycles)"),
		 ReadOnly(true)]
		public uint tXP
		{
			get { return _tXP; }
			set { _tXP = value; }
		}
		
		[CategoryAttribute("Timing"), DescriptionAttribute("Command time on bus (in clock cycles)"),
		 ReadOnly(true)]
		public uint tCMD
		{
			get { return _tCMD; }
			set { _tCMD = value; }
		}
		
		[CategoryAttribute("Power"), DescriptionAttribute("ACTIVATE and PRECHARGE with background current combined (mA)"),
		 ReadOnly(true)]
		public uint IDD0
		{
			get { return _IDD0; }
			set { _IDD0 = value; }
		}
		
		[CategoryAttribute("Power"), DescriptionAttribute("Current draw from ACT-READ-PRE from one bank only (mA)"),
		 ReadOnly(true)]
		public uint IDD1
		{
			get { return _IDD1; }
			set { _IDD1 = value; }
		}
		
		[CategoryAttribute("Power"), DescriptionAttribute("Current draw with all banks precharged and in low-power mode (mA)"),
		 ReadOnly(true)]
		public uint IDD2P
		{
			get { return _IDD2P; }
			set { _IDD2P = value; }
		}
		
		[CategoryAttribute("Power"), DescriptionAttribute("Quiet stand-by background current (mA)"),
		 ReadOnly(true)]
		public uint IDD2Q
		{
			get { return _IDD2Q; }
			set { _IDD2Q = value; }
		}
		
		[CategoryAttribute("Power"), DescriptionAttribute("All-banks precharged background current (mA)"),
		 ReadOnly(true)]
		public uint IDD2N
		{
			get { return _IDD2N; }
			set { _IDD2N = value; }
		}
			
		[CategoryAttribute("Power"), DescriptionAttribute("All-banks open in power-down mode - fast (mA)"),
		 ReadOnly(true)]
		public uint IDD3Pf
		{
			get { return _IDD3Pf; }
			set { _IDD3Pf = value; }
		}
		
		[CategoryAttribute("Power"), DescriptionAttribute("All-banks open in power-down mode - slow (mA)"),
		 ReadOnly(true)]
		public uint IDD3Ps
		{
			get { return _IDD3Ps; }
			set { _IDD3Ps = value; }
		}
		
		[CategoryAttribute("Power"), DescriptionAttribute("Background current with 1 or more banks open (mA)"),
		 ReadOnly(true)]
		public uint IDD3N
		{
			get { return _IDD3N; }
			set { _IDD3N = value; }
		}
		
		[CategoryAttribute("Power"), DescriptionAttribute("Write burst current (mA)"),
		 ReadOnly(true)]
		public uint IDD4W
		{
			get { return _IDD4W; }
			set { _IDD4W = value; }
		}
		
		[CategoryAttribute("Power"), DescriptionAttribute("Read burst current (mA)"),
		 ReadOnly(true)]
		public uint IDD4R
		{
			get { return _IDD4R; }
			set { _IDD4R = value; }
		}
		
		[CategoryAttribute("Power"), DescriptionAttribute("Refresh current (mA)"),
		 ReadOnly(true)]
		public uint IDD5
		{
			get { return _IDD5; }
			set { _IDD5 = value; }
		}
		
		[CategoryAttribute("Power"), DescriptionAttribute("Self refresh in sleep-mode (mA)"),
		 ReadOnly(true)]
		public uint IDD6
		{
			get { return _IDD6; }
			set { _IDD6 = value; }
		}
		
		[CategoryAttribute("Power"), DescriptionAttribute("Self refresh in sleep-mode (mA)"),
		 ReadOnly(true)]
		public uint IDD6L
		{
			get { return _IDD6L; }
			set { _IDD6L = value; }
		}
		
		[CategoryAttribute("Power"), DescriptionAttribute("All-banks open and interleaving reads from all banks (mA)"),
		 ReadOnly(true)]
		public uint IDD7
		{
			get { return _IDD7; }
			set { _IDD7 = value; }
		}
		
		public DeviceParameters(){}
	}
}
