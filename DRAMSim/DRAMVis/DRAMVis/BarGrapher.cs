
using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Drawing;
using NPlot;

namespace DRAMVis
{
	
	public class BarGrapher : IPlottable
	{
		
		public Control control;

		private TableLayoutPanel t;
		private string keyName;
		
		public BarGrapher(Control c, string keyName)
		{
			this.control = c;
			t = new TableLayoutPanel();
			t.RowCount = 1;
			t.ColumnCount = 0;
			t.CellBorderStyle = TableLayoutPanelCellBorderStyle.Inset;
			c.Controls.Add(t);
			t.Dock = DockStyle.Fill;
			this.keyName = keyName;
		}

		public Bitmap GetBitmap() 
		{
			//for whatever reason, trying to just call drawtobitmap on the tablelayoutpanel doesn't work 
			Bitmap b = new Bitmap(control.Width, control.Height);
			Grapher.SetBitmapBackground(b, Color.White);
			
			foreach (Control c in t.Controls) {
				c.DrawToBitmap(b, c.Bounds);
	//			p("Drawing "+c.Name+"("+control.Width+","+control.Height+") "+c.Bounds);
			}
			return b;
		}
		
		public void HighlightGraph(VisFileContainer v)
		{
			
		}
		public void SetState(MouseState mouseState) 
		{
			foreach (Object obj in t.Controls) 
			{
				if (obj is NPlot.Windows.PlotSurface2D) 
				{
					Grapher.SetState((NPlot.Windows.PlotSurface2D)obj, mouseState);
				}
			}
		}
		public void ResetZoom() 
		{
			
		}
		// set's all the Y axes of the bar plots displayed to be the same -- for comparison reasons
		public void SetCommonYAxis() 
		{
			double min=double.MaxValue, max=0.0;
			foreach (Object obj in t.Controls) 
			{
				if (obj is NPlot.Windows.PlotSurface2D) 
				{
					NPlot.Windows.PlotSurface2D plot = (NPlot.Windows.PlotSurface2D) obj;
					max = Math.Max(plot.YAxis1.WorldMax, max);
					min = Math.Min(plot.YAxis1.WorldMin, min);
				}
			}
			foreach (Object obj in t.Controls) 
			{
				if (obj is NPlot.Windows.PlotSurface2D) 
				{
					NPlot.Windows.PlotSurface2D plot = (NPlot.Windows.PlotSurface2D) obj;
					plot.YAxis1.WorldMax = max;
					plot.YAxis1.WorldMin = min;
				}
			}
			
		}

	 public static void SetPowerLabels(NPlot.Windows.PlotSurface2D plot) 
		{
			plot.YAxis1.LabelOffsetAbsolute = true;
			plot.YAxis1.LabelOffset = 40;
            plot.XAxis1.HideTickText = false;
			plot.Title = "Power Dissipation (watts)";
			plot.XAxis1.Label = "Time (milliseconds)";
			plot.YAxis1.Label = "Average Power (watts)";
		}

		public void AddGraph(VisFileContainer v) 
		{
			t.ColumnCount++;
			NPlot.Windows.PlotSurface2D plot = new NPlot.Windows.PlotSurface2D();
			plot.Dock = DockStyle.Fill;

			foreach (BarPlot bp in v.barPlotTable[keyName]) 
			{
				plot.Add(bp);
			}
			plot.Add(ComputeAveragePower(v));
			plot.Legend = new Legend();
			
			plot.Legend.HorizontalEdgePlacement = NPlot.Legend.Placement.Outside;
			plot.Legend.VerticalEdgePlacement = NPlot.Legend.Placement.Inside;
			plot.Legend.NumberItemsVertically = 4;
			plot.Legend.AttachTo(NPlot.PlotSurface2D.XAxisPosition.Bottom,NPlot.PlotSurface2D.YAxisPosition.Left); 
			plot.Legend.YOffset = 40;
			
			plot.Title = v.deviceName+"\n"+v.configurationName;
			plot.Name = v.filepath;
			t.Controls.Add(plot,t.ColumnCount-1,0);
			
			
			t.ColumnStyles.Add(new ColumnStyle(SizeType.Percent));
			RescaleBarGraphs();
			
		}
		
		private void RescaleBarGraphs() 
		{
			//recompute the new cell percentages
			float newPercentage = 1.0f/(float)t.ColumnCount;
			for (int i=0; i<t.ColumnStyles.Count; i++) 
			{
				t.ColumnStyles[i].Width = newPercentage;
			}
			SetCommonYAxis();
			t.Refresh();
		}
		public void RemoveGraph(VisFileContainer v) 
		{	
			int removeIndex = -1;
			for (int i=0; i<t.ColumnCount; i++) 
			{
				Control obj = t.GetControlFromPosition(i,0);
				if (obj is NPlot.Windows.PlotSurface2D) 
				{
					NPlot.Windows.PlotSurface2D plot = (NPlot.Windows.PlotSurface2D) obj;
					
					if (plot.Name.Equals(v.filepath))
					{
						t.Controls.Remove(plot);
						t.ColumnStyles.RemoveAt(i);
						removeIndex=i;
						break;
					}
				}
			}
			if (removeIndex == -1) {
				Console.WriteLine("WARNING: did not find graph to remove");
				return;
			}

			if (removeIndex != t.ColumnCount-1) 
			{
				// we need to scoot the graphs down before removing the columns
				for (int i=removeIndex+1; i<t.ColumnCount; i++) 
				{
					Object nextObj = t.GetControlFromPosition(i,0);
					NPlot.Windows.PlotSurface2D nextPlot = (NPlot.Windows.PlotSurface2D) nextObj;
					
	//				p("Moving "+nextPlot.Name+" from "+(i)+" to "+(i-1));
					
					t.Controls.Remove(nextPlot);
					t.Controls.Add(nextPlot, i-1, 0);
				}
			}
			// then get rid of the last column and rescale everyone 
			t.ColumnCount--;
			RescaleBarGraphs();
			
			//Console.WriteLine("Moving "+plot.Title+" from "+i+1+" to "+i);
			return;
		}
		
		public void Clear() 
		{
			t.Controls.Clear();
			t.ColumnStyles.Clear();
			t.ColumnCount=0;
		}
		
		public static List<BarPlot> HistogramPlot(VisFileContainer v) {
			List<BarPlot> ret = new List<BarPlot>();
			BarPlot histogram = MakeBarPlot(v.valueTable["latency_values"],v.valueTable["latency_counts"],Color.SlateGray);
			ret.Add(histogram);
			return ret;
		}
		
		// used for power graphs to put the bars on top of one another
		private static void ComputeStackedBarValues(VisFileContainer v, int rank, List<decimal> offsets)
		{
			Dictionary<string, List<decimal>> valueTable = v.valueTable;
			
			valueTable.Add("background_top_"+rank, new List<decimal>());
			valueTable.Add("actpre_top_"+rank, new List<decimal>());
			valueTable.Add("burst_top_"+rank, new List<decimal>());
			valueTable.Add("refresh_top_"+rank, new List<decimal>());
			valueTable.Add("background_bottom_"+rank, new List<decimal>(offsets));
			int j=0;
            foreach (decimal val in valueTable["bgp_"+rank])
			{
				valueTable["background_top_"+rank].Add(offsets[j] + val);
				j++;
			}
			j=0;
			foreach (decimal val in valueTable["ap_"+rank])
			{
				valueTable["actpre_top_"+rank].Add(valueTable["background_top_"+rank][j] + val);
				j++;
			}
			j=0;
			foreach (decimal val in valueTable["bp_"+rank])
			{
				valueTable["burst_top_"+rank].Add(valueTable["actpre_top_"+rank][j] + val);
				j++;
			}
			j=0;
			foreach (decimal val in valueTable["rp_"+rank])
			{
				valueTable["refresh_top_"+rank].Add(valueTable["burst_top_"+rank][j] + val);
				j++;
			}
		}
		
		private static MyLinePlot ComputeAveragePower(VisFileContainer v) 
		{
			Dictionary<string, List<decimal>> valueTable = v.valueTable;
			uint rank = v.systemParameters.NUM_RANKS-1;
			MyLinePlot line = LineGrapher.MakeLinePlot(valueTable["x_axis"], Grapher.GenerateRunningAverage(valueTable["refresh_top_"+rank]), Color.Red);
			line.Label = "Average Power";
			line.Pen = new Pen(Color.Red, 2.0f);
			return line;
			
		}
		
		//make a bar graph with an arbitrary bottom value
		public static BarPlot MakeBarPlot(List<decimal> xAxis, List<decimal> barTops, List<decimal>barBottoms, Color color)
		{
			BarPlot h = new BarPlot();
			h.AbscissaData = xAxis;
			h.OrdinateDataTop = barTops;
			h.OrdinateDataBottom = barBottoms;
			h.BarWidth = 3;
			h.BorderColor = color;
			h.FillBrush = new NPlot.RectangleBrushes.Solid(color);
			return h;
		}
		
		//make a bar graph with the bars starting at 0
		public static BarPlot MakeBarPlot(List<decimal> xAxis, List<decimal> barTops, Color color)
		{
			List<decimal> barBottoms = Grapher.ZeroList(barTops);
			return MakeBarPlot(xAxis, barTops,barBottoms,color);
		}


		public static List<BarPlot> PowerGraph(VisFileContainer v)
		{
			p("PowerGraph for "+v.filepath);
			List<BarPlot> plots = new List<BarPlot>();
			Dictionary<string, List<decimal>> valueTable = v.valueTable;
			int numValues = valueTable["x_axis"].Count;
			ComputeStackedBarValues(v, 0, Grapher.ZeroList(numValues));
			Color c;
			// run the loop from 1 since the 0th case is taken care of above
			for (uint r=1; r<=v.systemParameters.NUM_RANKS; r++) 
			{
				//when r==NUM_RANKS, we don't want to compute the stacked values
				if (r<v.systemParameters.NUM_RANKS) 
				{
					ComputeStackedBarValues(v, (int)r, valueTable["refresh_top_"+(r-1)]);
				}

				uint x=(r-1)*2;
				//adding these in reverse order of the stacks makes the legend work out well
				c = Grapher.GetColorRange(x,0,3);
				BarPlot bp = MakeBarPlot(valueTable["x_axis"],valueTable["refresh_top_"+(r-1)],valueTable["burst_top_"+(r-1)], c);
				bp.Label = "Refresh (Rank "+r+")";				
				plots.Add(bp);
			
				c = Grapher.GetColorRange(x,2,3);
				bp = MakeBarPlot(valueTable["x_axis"],valueTable["burst_top_"+(r-1)],valueTable["actpre_top_"+(r-1)], c);
				bp.Label = "Burst (Rank "+r+")";
				plots.Add(bp);
				
				c = Grapher.GetColorRange(x,1,3);				
				bp = MakeBarPlot(valueTable["x_axis"],valueTable["actpre_top_"+(r-1)],valueTable["background_top_"+(r-1)], c);
				bp.Label = "Activate/Precharge (Rank "+r+")";
				plots.Add(bp);
				
				c = Grapher.GetColorRange(x,3,3);
				bp = MakeBarPlot(valueTable["x_axis"],valueTable["background_top_"+(r-1)],valueTable["background_bottom_"+(r-1)], c);
				bp.Label = "Background Power (Rank "+r+")";				
				plots.Add(bp);

			}
			
			
			/*
		
			
	//		plot.Legend.NeverShiftAxes=true;
			
			*/
			
//			computeBarGraphTimeScales();	
//			SetBarGraphWidths(plot);
			return plots;
		}

		
		
		private static void p(string s) {
			Console.WriteLine(s);
		}

		
		
	}
}
