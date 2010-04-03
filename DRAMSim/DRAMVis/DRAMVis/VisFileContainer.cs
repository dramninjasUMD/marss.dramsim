// VisFileContainer.cs created with MonoDevelop
// User: elliott at 12:27 PM 2/26/2009
//
// To change standard headers go to Edit->Preferences->Coding->Standard Headers
//

using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using NPlot;

namespace DRAMVis
{
	
	public class VisFileContainer 
	{
		private SystemParameters _systemParameters;
		private DeviceParameters _deviceParameters;
		private string _filepath;
		private Dictionary<string, List<decimal>> _valueTable;
		
		public Dictionary<string, List<MyLinePlot>> plotTable;
		public Dictionary<string, List<BarPlot>> barPlotTable;
		
		public string deviceName;
		public string configurationName; 
		public SystemParameters systemParameters
		{
			get { return _systemParameters; }
			set { _systemParameters = value; }
		}
		
		public DeviceParameters deviceParameters
		{
			get { return _deviceParameters; }
			set { _deviceParameters = value; }
		}
		
		public Dictionary<string, List<decimal>> valueTable
		{
			get { return _valueTable; }
		}
		
		public string filepath
		{
			get { return _filepath; }
		}
		
		public VisFileContainer(string filename) 
		{
			_filepath = filename;
			VisFileReader vfreader = new VisFileReader(_filepath);
			
			systemParameters = vfreader.ReadSystemData();	
			if(systemParameters==null)
			{
				throw new NullReferenceException("Error reading vis file");
			}
			
			deviceParameters = vfreader.ReadDeviceData(null);
			if(deviceParameters==null)
			{
				throw new NullReferenceException("Error reading vis file");
			}
			
			_valueTable = new Dictionary<string, List<decimal>>();
			plotTable = new Dictionary<string, List<MyLinePlot>>();
			barPlotTable = new Dictionary<string, List<BarPlot>>();
				
			vfreader.ReadEpochData(_valueTable);
			vfreader.ReadHistogramData(_valueTable);
			this.MakeGraphsFromValueTable();
		}
		
		private void MakeGraphsFromValueTable() 
		{
			int numValues = valueTable["x_axis"].Count;
			string keyName;
			//yes, this is a cheapo way to get unique colors, but who cares
			
			Color lineColor = Grapher.GetColorRange((uint)filepath.GetHashCode(), 0, 1) ;	;
			
			// Per Bank Data is in the list from 0 ... NUM_BANKS*NUM_RANKS
			for (int r=0; r<systemParameters.NUM_RANKS; r++) 
			{                          
				for (int b=0; b<deviceParameters.NUM_BANKS; b++)
				{	
					if (r == 0 && b == 0) 
					{
						plotTable.Add("latency", new List<MyLinePlot>());
						plotTable.Add("bandwidth", new List<MyLinePlot>());
					}
					lineColor = Grapher.GetColorRange((uint)filepath.GetHashCode(), (uint)r, systemParameters.NUM_RANKS) ;		
					keyName = "l_"+r+"_"+b;
					MyLinePlot linePlot = LineGrapher.MakeLinePlot(valueTable["x_axis"], valueTable[keyName], lineColor);
					plotTable["latency"].Add(linePlot);
					keyName = "b_"+r+"_"+b;
					linePlot = LineGrapher.MakeLinePlot(valueTable["x_axis"], valueTable[keyName], lineColor);
					plotTable["bandwidth"].Add(linePlot);
				}
			}
			
			// Per Rank data is in BANKS*RANKS ... BANKS*RANKS+RANKS
			
			List<decimal> bandwidthGrandTotal = Grapher.ZeroList(numValues);
			List<decimal> latencyGrandTotal = Grapher.ZeroList(numValues);
			List<decimal> totalCounts = Grapher.ZeroList(numValues);
			// generate per rank and total graphs
			for (int r=0; r<systemParameters.NUM_RANKS; r++) 
			{   
				List<decimal> latencyRankTotal = Grapher.ZeroList(numValues);
				List<decimal> bandwidthRankTotal = Grapher.ZeroList(numValues);
				List<decimal> counts = Grapher.ZeroList(numValues);
				// sum the latencies and bandwidths into grand totals and per rank totals
				for (int b=0; b<deviceParameters.NUM_BANKS; b++)
				{	
					keyName = "b_"+r+"_"+b;	
					Grapher.SumLists(bandwidthRankTotal,valueTable[keyName]);
					Grapher.SumLists(bandwidthGrandTotal,valueTable[keyName]);
					
					keyName = "l_"+r+"_"+b;	
					Grapher.CountNonZero(valueTable[keyName], counts);
					Grapher.CountNonZero(valueTable[keyName], totalCounts);
					Grapher.SumLists(latencyRankTotal,valueTable[keyName]);
					Grapher.SumLists(latencyGrandTotal,valueTable[keyName]);
				}
				// take the totals of all the latencies and generate a per-rank average since it doesn't really make sense
				// to sum latencies across banks
				for (int i=0; i<latencyRankTotal.Count; i++) 
				{
/*					
 * 					if (counts[i] > deviceParameters.NUM_BANKS) 
					{
						Console.WriteLine("COUNTS = "+counts[i]);
					}
					if (counts[i] == 0)
					{	
						Console.WriteLine("ZERO COUNT");
						latencyRankTotal[i] = latencyRankTotal[i];
					}
					else 
					{
						latencyRankTotal[i] = latencyRankTotal[i]/counts[i];
					}
					*/
						latencyRankTotal[i] = latencyRankTotal[i]/deviceParameters.NUM_BANKS;
				}
				
				plotTable["latency"].Add(LineGrapher.MakeLinePlot(valueTable["x_axis"], new List<decimal>(latencyRankTotal), lineColor));
				plotTable["bandwidth"].Add(LineGrapher.MakeLinePlot(valueTable["x_axis"], new List<decimal>(bandwidthRankTotal), lineColor));
				
				// clear these lists before the next iteration (to keep from accumulating)
				bandwidthRankTotal.Clear();
				latencyRankTotal.Clear();
				counts.Clear();
			}
			
			
			for (int i=0; i<latencyGrandTotal.Count; i++) 
			{
				latencyGrandTotal[i] = latencyGrandTotal[i]/(deviceParameters.NUM_BANKS*systemParameters.NUM_RANKS);
			}
			
			// last 2 indices are the total/avg
			plotTable["latency"].Add(LineGrapher.MakeLinePlot(valueTable["x_axis"], latencyGrandTotal, Color.Green));
//			plotTable["latency"].Add(LineGrapher.MakeLinePlot(valueTable["x_axis"], Grapher.GenerateRunningAverage(latencyGrandTotal), lineColor));
			
			plotTable["bandwidth"].Add(LineGrapher.MakeLinePlot(valueTable["x_axis"], bandwidthGrandTotal, Color.Green));
//			plotTable["bandwidth"].Add(LineGrapher.MakeLinePlot(valueTable["x_axis"], Grapher.GenerateRunningAverage(bandwidthGrandTotal), lineColor));

			//Console.WriteLine("finished recomputing");
			
			//Draw the Power and Histogram plots
			barPlotTable.Add("power", BarGrapher.PowerGraph(this));
			barPlotTable.Add("histogram", BarGrapher.HistogramPlot(this));
			
		}
	}
}
