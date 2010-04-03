
// Grapher.cs created with MonoDevelop
// User: paul at 7:00 PM 2/10/2009
//
// To change standard headers go to Edit->Preferences->Coding->Standard Headers
//

using System;
using System.Drawing;
using System.Collections.Generic;
using System.Windows.Forms;
using NPlot;

namespace DRAMVis
{
	public enum DrawMode {TOTAL,RANK,BANK,AVERAGE,ALL} ;
	
	public class Grapher
	{
		/*
		private Dictionary<string, GraphData> plots = new Dictionary<string,GraphData>();
		private Dictionary<NPlot.Windows.PlotSurface2D, List<GraphData>> canvases = new Dictionary<NPlot.Windows.PlotSurface2D,List<GraphData>>();
		*/
		
		//private Dictionary<NPlot.Windows.PlotSurface2D, List<BarPlot>> BarPlotsOnCanvas;
		//private static Grid grid = new Grid();
		private static NPlot.Windows.PlotSurface2D.Interactions.HorizontalRangeSelection zoomInteraction = new NPlot.Windows.PlotSurface2D.Interactions.HorizontalRangeSelection();
		private static NPlot.Windows.PlotSurface2D.Interactions.HorizontalDrag panInteraction = new NPlot.Windows.PlotSurface2D.Interactions.HorizontalDrag();
		
		private Dictionary<NPlot.Windows.PlotSurface2D, decimal> timeScale;
		private Dictionary<NPlot.Windows.PlotSurface2D, double> maxValue;
		
		public DrawMode drawMode = DrawMode.TOTAL;
		
		
		public Grapher(Visualizer v)
		{

		//	this.BarPlotsOnCanvas = new Dictionary<NPlot.Windows.PlotSurface2D,List<BarPlot>>();
			this.timeScale = new Dictionary<NPlot.Windows.PlotSurface2D,decimal>();
			this.maxValue = new Dictionary<NPlot.Windows.PlotSurface2D,double>();
			
		}
		

		public static void SetGraphDefaults(NPlot.Windows.PlotSurface2D plot) 
		{
		//	Console.WriteLine("Setting defaults");
		//	plot.Add(grid);

		//	plot.CacheAxes();				
	//		plot.InteractionOccured += new NPlot.Windows.PlotSurface2D.InteractionHandler(Grapher.zoomRefresh);
		}
		


		public static void SumLists(List<decimal> l1, List<decimal> l2) 
		{
			if (l1.Count != l2.Count) return;
			
			for (int i=0; i<l1.Count; i++) 
			{
				l1[i] += l2[i];
			}
		}
		
		public static void CountNonZero(List<decimal> l1, List<decimal> counts) 
		{
			for (int i=0; i<l1.Count; i++) 
			{
				if (l1[i] != 0)
					counts[i]++;
			}
		}
		
		// C# likes to make the canvas transparent, so just manually set all the pixels in a canvas to some color (usually before drawing into it)
		public static void SetBitmapBackground(Bitmap b, Color c)
		{
			for (int x=0; x<b.Width; x++) 
			{
				for (int y=0; y<b.Height; y++) 
				{
					b.SetPixel(x,y,c);
				}
			}
		}

		static uint [] ColorRanges = {
			0xffff9000,0xffffdaaa, // orange
			0xff15b000, 0xffa1ff90, // green
			0xff000090, 0xff8090ff, // blue
			0xff800080, 0xffffa0ff, // purple
			0xffff0000, 0xffffaaaa // red
		}; 
		
		public static Color GetColorRange(uint index, uint gradation, uint numGradations)
		{
			index = (index*2)%(uint)ColorRanges.Length; 
			uint start = ColorRanges[index]; 
			uint end = ColorRanges[index+1];			
			uint difference = end-start;
			uint r = ((difference>>16)&0x000000ff)/numGradations;
			uint g = ((difference>>8)&0x000000ff)/numGradations;
			uint b = ((difference)&0x000000ff)/numGradations;
			      
			if (gradation > numGradations) {
				Console.WriteLine("FAIL");
				return Color.Red;
			}
			
			return Color.FromArgb((int)(start+ ((r*gradation)<<16) + ((g*gradation)<<8) + (b*gradation)));
		}

		public void SetBarGraphWidths(NPlot.Windows.PlotSurface2D canvas) 
		{
			double numBars = (canvas.XAxis1.WorldLength)/(double)this.timeScale[canvas];
			foreach (IDrawable drawable in canvas.Drawables) 
			{
				if (drawable is BarPlot) 
				{
					BarPlot bp = (BarPlot)drawable;
					bp.BarWidth = (float)Math.Floor(canvas.Width/(numBars))-4.0f;
					if (bp.BarWidth < 0)
						bp.BarWidth = 1.0f;
				}
			}
			canvas.Refresh();
		}
		

	
		public void zoomOut(NPlot.Windows.PlotSurface2D plot)
		{
			double xLen = plot.XAxis1.WorldMax - plot.XAxis1.WorldMin;
			double newMinX = plot.XAxis1.WorldMin - xLen * 0.5;
			double newMaxX = plot.XAxis1.WorldMax + xLen * 0.5;
			if (newMinX < 0)
			{
				newMinX = 0;
			}
			
			if (newMaxX > this.maxValue[plot]) 
			{
				newMaxX = this.maxValue[plot];
			}
					
			plot.XAxis1.WorldMax = newMaxX ;
			plot.XAxis1.WorldMin = newMinX;
			
			if (!timeScale.ContainsKey(plot)) 
			{
				computeBarGraphTimeScales();
			}
			
			this.SetBarGraphWidths(plot);			
			
			plot.Refresh();
		}

				
		public static List<decimal> GenerateRunningAverage(List<decimal>inList)
		{
			List<decimal> avgList = new List<decimal>();
			avgList.Add(inList[0]);
			for (int i=0; i<inList.Count-1; i++)
			{
				avgList.Add(avgList[i] + ((inList[i+1] - avgList[i])/(i+1)));
			}
			return avgList;
		}
		
		// useful to use as the bottom value in a bar plot
		public static List<decimal> ZeroList(int len) 
		{
			List<decimal> l = new List<decimal>(len);
			for (int i=0;i<len;i++) 
			{
				l.Add(0);
			}
			return l;
		}		
		public static List<decimal> ZeroList(List<decimal> x) {
			List<decimal> l = new List<decimal>(x.Count);
			for (int i=0;i<x.Count;i++) {
				l.Add(0);
			}
			return l;
		}

		public static void SetState(NPlot.Windows.PlotSurface2D graph, MouseState mouseState)
		{
			switch(mouseState)
			{
			case MouseState.Pan:
				graph.RemoveInteraction(zoomInteraction);
				graph.AddInteraction(panInteraction);
				break;
			case MouseState.Zoom:
				graph.RemoveInteraction(panInteraction);
				graph.AddInteraction(zoomInteraction);
				break;
			case MouseState.Pointer:
				graph.RemoveInteraction(zoomInteraction);
				graph.RemoveInteraction(panInteraction);
				break;
			}
		}

		
		public void computeBarGraphTimeScales()
		{
			/*
			if (!timeScale.ContainsKey(vis.plotPower))
			    timeScale.Add(vis.plotPower, vis.valueTable["x_axis"][2] - vis.valueTable["x_axis"][1]);
			
			if (!timeScale.ContainsKey(vis.plotLatencyHistogram))
				timeScale.Add(vis.plotLatencyHistogram, vis.valueTable["latency_values"][2] - vis.valueTable["latency_values"][1]);
				*/
		}
			
		private void zoomRefresh(object sender)
		{
		}
	}
}

