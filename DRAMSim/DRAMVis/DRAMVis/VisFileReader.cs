// IniReader.cs created with MonoDevelop
// User: elliott at 11:36 AM 2/7/2009
//
// To change standard headers go to Edit->Preferences->Coding->Standard Headers
//

using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using DictionaryExtensions;

namespace DRAMVis
{
	public class VisFileReader
	{
		private StreamReader inputFile;
		private Dictionary<string, float> lastValue = new Dictionary<string, float>();
		
		public VisFileReader(string filename)
		{
			inputFile = File.OpenText(filename);
		}
		
		public DeviceParameters ReadDeviceData(string deviceFile)
		{		
			string line;
			DeviceParameters deviceParameters = new DeviceParameters();
			
			if(deviceFile != null)
			{
				inputFile = File.OpenText(deviceFile);
			}
			
			while((line = inputFile.ReadLine())!=null)
			{
				line = line.Trim();
				
				if(line.Equals("!!EPOCH_DATA")) break;
				   
				if(line.Equals("!!DEVICE_INI") ||
				   line.StartsWith(";") || 
				   line.Length==0) 
					continue;
				
				if(line.IndexOf(";")>0)
				{
					//trim off comments
					line = line.Substring(0,line.IndexOf(";"));
				}
				//Console.WriteLine(line);
				string param = line.Substring(0,line.IndexOf("="));
				param = param.Trim();
				string val = line.Substring(line.IndexOf("=")+1);
				val = val.Trim();

				switch(param)
				{
				case "NUM_BANKS":
					deviceParameters.NUM_BANKS = uint.Parse(val);
					break;
				case "NUM_ROWS":
					deviceParameters.NUM_ROWS = uint.Parse(val);
					break;
				case "NUM_COLS":
					deviceParameters.NUM_COLS = uint.Parse(val);
					break;
				case "DEVICE_WIDTH":
					deviceParameters.DEVICE_WIDTH = uint.Parse(val);
					break;
				case "REFRESH_PERIOD":
					deviceParameters.REFRESH_PERIOD = uint.Parse(val);
					break;
				case "tCK":
					deviceParameters.tCK = float.Parse(val);
					break;
				case "CL":
					deviceParameters.CL = uint.Parse(val);
					break;
				case "AL":
					deviceParameters.AL = uint.Parse(val);
					break;
				case "BL": 
					deviceParameters.BL = uint.Parse(val);
					break;
				case "tRAS":
					deviceParameters.tRAS = uint.Parse(val);
					break;
				case "tRCD":
					deviceParameters.tRCD = uint.Parse(val);
					break;
				case "tRRD":
					deviceParameters.tRRD = uint.Parse(val);
					break;
				case "tRC":
					deviceParameters.tRC = uint.Parse(val);
					break;
				case "tRP":
					deviceParameters.tRP = uint.Parse(val);
					break;
				case "tCCD":
					deviceParameters.tCCD = uint.Parse(val);
					break;
				case "tRTP":
					deviceParameters.tRTP = uint.Parse(val);
					break;
				case "tWTR":
					deviceParameters.tWTR = uint.Parse(val);
					break;
				case "tWR":
					deviceParameters.tWR = uint.Parse(val);
					break;
				case "tRTRS":
					deviceParameters.tRTRS = uint.Parse(val);
					break;
				case "tRFC":
					deviceParameters.tRFC = uint.Parse(val);
					break;
				case "tFAW":
					deviceParameters.tFAW = uint.Parse(val);
					break;
				case "tCKE":
					deviceParameters.tCKE = uint.Parse(val);
					break;
				case "tXP":
					deviceParameters.tXP = uint.Parse(val);
					break;
				case "tCMD":
					deviceParameters.tCMD = uint.Parse(val);
					break;
				case "IDD0":
					deviceParameters.IDD0 = uint.Parse(val);
					break;
				case "IDD1":
					deviceParameters.IDD1 = uint.Parse(val);
					break;
				case "IDD2P":
					deviceParameters.IDD2P = uint.Parse(val);
					break;
				case "IDD2Q":
					deviceParameters.IDD2Q = uint.Parse(val);
					break;
				case "IDD2N":
					deviceParameters.IDD2N = uint.Parse(val);
					break;
				case "IDD3Pf":
					deviceParameters.IDD3Pf = uint.Parse(val);
					break;
				case "IDD3Ps":
					deviceParameters.IDD3Ps = uint.Parse(val);
					break;
				case "IDD3N":
					deviceParameters.IDD3N = uint.Parse(val);
					break;
				case "IDD4W":
					deviceParameters.IDD4W = uint.Parse(val);
					break;
				case "IDD4R":
					deviceParameters.IDD4R = uint.Parse(val);
					break;
				case "IDD5":
					deviceParameters.IDD5 = uint.Parse(val);
					break;
				case "IDD6":
					deviceParameters.IDD6 = uint.Parse(val);
					break;
				case "IDD6L":
					deviceParameters.IDD6L = uint.Parse(val);
					break;
				case "IDD7":
					deviceParameters.IDD7 = uint.Parse(val);
					break;
				default:
					return null;
				}
			}
			return deviceParameters;
		}

		public SystemParameters ReadSystemData()
		{
			string line;
			SystemParameters systemParameters = new SystemParameters();
			
			while((line = inputFile.ReadLine())!=null)
			{
				line = line.Trim();
				
				if(line.Equals("!!DEVICE_INI")) break;
				
				if(line.Equals("!!SYSTEM_INI") || 
				   line.StartsWith(";") || 
				   line.Length==0) 
				   continue;
				
				if(line.IndexOf(";")>0)
				{
					//trim off comments
					line = line.Substring(0,line.IndexOf(";"));
				}

				string param = line.Substring(0,line.IndexOf("="));
				param = param.Trim();
				string val = line.Substring(line.IndexOf("=")+1);
				val = val.Trim();
				
				switch(param)
				{
					/*
				case "TOTAL_STORAGE":
					systemParameters.TOTAL_STORAGE = long.Parse(val);
					break;
					*/
				case "NUM_RANKS":
					systemParameters.NUM_RANKS = uint.Parse(val);
					break;
				case "NUM_CHANS":
					systemParameters.NUM_CHANS = uint.Parse(val);
					break;
				case "CACHE_LINE_SIZE":
					systemParameters.CACHE_LINE_SIZE = uint.Parse(val);
					break;
				case "JEDEC_DATA_BUS_WIDTH":
					systemParameters.JEDEC_DATA_BUS_WIDTH = uint.Parse(val);
					break;
				case "TRANS_QUEUE_DEPTH":
					systemParameters.TRANS_QUEUE_DEPTH = uint.Parse(val);
					break;
				case "CMD_QUEUE_DEPTH":
					systemParameters.CMD_QUEUE_DEPTH = uint.Parse(val);
					break;
				case "USE_LOW_POWER":
					systemParameters.USE_LOW_POWER = bool.Parse(val);
					break;
				case "EPOCH_COUNT":
					systemParameters.EPOCH_COUNT = uint.Parse(val);
					break;
				case "TOTAL_ROW_ACCESSES":
					systemParameters.TOTAL_ROW_ACCESSES = uint.Parse(val);
					break;
				case "ROW_BUFFER_POLICY":
					switch(val)
					{
					case "open_page":
						systemParameters.ROW_BUFFER_POLICY = RowBufferPolicy.open_page;
						break;
					case "close_page":
						systemParameters.ROW_BUFFER_POLICY = RowBufferPolicy.close_page;
						break;
					default:
						return null;
					}
					break;
				case "SCHEDULING_POLICY":
					switch (val)
					{
					case "rank_then_bank_round_robin":
						systemParameters.SCHEDULING_POLICY = SchedulingPolicy.rank_then_bank_round_robin;
						break;
					case "bank_then_rank_round_robin":
						systemParameters.SCHEDULING_POLICY = SchedulingPolicy.bank_then_rank_round_robin;
						break;
					default:
						return null;
					}
					break;
				case "ADDRESS_MAPPING_SCHEME":
					switch (val)
					{
					case "scheme1":
						systemParameters.ADDRESS_MAPPING_SCHEME = AddressMappingScheme.scheme1;
						break;
					case "scheme2":
						systemParameters.ADDRESS_MAPPING_SCHEME = AddressMappingScheme.scheme2;
						break;
					case "scheme3":
						systemParameters.ADDRESS_MAPPING_SCHEME = AddressMappingScheme.scheme3;
						break;
					case "scheme4":
						systemParameters.ADDRESS_MAPPING_SCHEME = AddressMappingScheme.scheme4;
						break;
					case "scheme5":
						systemParameters.ADDRESS_MAPPING_SCHEME = AddressMappingScheme.scheme5;
						break;
					case "scheme6":
						systemParameters.ADDRESS_MAPPING_SCHEME = AddressMappingScheme.scheme6;
						break;
					default:
						return null;
					}
					break;
				case "QUEUING_STRUCTURE":
					switch (val)
					{
					case "per_rank":
						systemParameters.QUEUING_STRUCTURE = QueuingStructure.per_rank;
						break;
					case "per_rank_per_bank":
						systemParameters.QUEUING_STRUCTURE = QueuingStructure.per_rank_per_bank;
						break;
					default:
						return null;
					}
					break;
				case "DEBUG_TRANS_Q":
					systemParameters.DEBUG_TRANS_Q = bool.Parse(val);
					break;
				case "DEBUG_CMD_Q":
					systemParameters.DEBUG_CMD_Q = bool.Parse(val);
					break;
				case "DEBUG_ADDR_MAP":
					systemParameters.DEBUG_ADDR_MAP = bool.Parse(val);
					break;
				case "DEBUG_BANKSTATE":
					systemParameters.DEBUG_BANKSTATE = bool.Parse(val);
					break;
				case "DEBUG_BUS":
					systemParameters.DEBUG_BUS = bool.Parse(val);
					break;
				case "DEBUG_BANKS":
					systemParameters.DEBUG_BANKS = bool.Parse(val);
					break;
				case "DEBUG_POWER":
					systemParameters.DEBUG_POWER = bool.Parse(val);
					break;
				case "VERIFICATION_OUTPUT":
					systemParameters.VERIFICATION_OUTPUT = bool.Parse(val);
					break;
				default:
					return null;
				}
			}
			return systemParameters;
		}
		
		public void ReadEpochData(Dictionary<string, List<decimal>> valueTable)
		{
			String line;
			String [] linePieces;
			String key;
			float floatValue;
			char [] splitters = {':',',','='};
			valueTable["x_axis"] = new List<decimal>();			
			while((line = inputFile.ReadLine())!=null)
			{
				if(line.Equals("!!HISTOGRAM_DATA")) break;
				
				if(line.Equals("!!EPOCH_DATA") || 
				   line.StartsWith(";") || 
				   line.Length==0) 
				   continue;
				   
				linePieces = line.Split(splitters);
				decimal epoch;
				if (!decimal.TryParse(linePieces[0], out epoch)) 
				{
					Console.WriteLine("Bad Input file?");
				}
				
				valueTable["x_axis"].Add(epoch);
				
				for (int i=1; i<linePieces.Length; i+=2)
				{
					//due to the extra comma at the end of the line, make sure to prevent this case 
					if (i == linePieces.Length-1)
						break;
					
					key = linePieces[i];
					//value is at i+1
					if (!float.TryParse(linePieces[i+1], out floatValue))
					{
						// when we hit a NaN, just use the same value as the last epoch
						if (linePieces[i+1] == "nan") 
						{
							if (!lastValue.ContainsKey(key) || lastValue[key] == float.NaN) 
							{
								floatValue=0.0f;
							} 
							else 
							{
								floatValue = lastValue[key];
							}
						}
						else
						{
							Console.WriteLine("failed to parse float"+linePieces[i+1]);
						}
					}
					
					if (!valueTable.ContainsKey(key))
					{
						valueTable.Add(key, new List<decimal>());
					}
					decimal decimalValue = Convert.ToDecimal(floatValue);
					lastValue.AddReplace(key, floatValue);
					valueTable[key].Add(decimalValue);
					
				}
			}
		}
		
		public void ReadHistogramData(Dictionary<string, List<decimal>> valueTable)
		{
			String line;
			String [] linePieces;
			char [] splitters = {'='};
			valueTable["latency_values"] = new List<decimal>();
			valueTable["latency_counts"] = new List<decimal>();
			while((line = inputFile.ReadLine())!=null)
			{
				if(line.Equals("!!BUS_TRACE")) break;
				
				if(line.Equals("!!HISTOGRAM_DATA") || 
				   line.StartsWith(";") || 
				   line.Length==0) 
				   continue;
				linePieces = line.Split(splitters);
				int val;
				int count;
				
				if (!int.TryParse(linePieces[0], out val)) 
				{
					Console.WriteLine("Bad Input file?");
				}
				if (!int.TryParse(linePieces[1], out count)) 
				{
					Console.WriteLine("Bad Input file?");
				}
				// this keeps outlier latencies out of the graph
				if (count > 4)
				{ 
					valueTable["latency_values"].Add(val);
					valueTable["latency_counts"].Add(count);
				}	
			}
			
			inputFile.Close();
		}
		
	}
}
