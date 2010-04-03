// GraphData.cs created with MonoDevelop
// User: paul at 11:28 AM 2/27/2009
//
// To change standard headers go to Edit->Preferences->Coding->Standard Headers
//

using System;
using System.Collections.Generic;
using System.Collections;
using NPlot;
using System.Windows.Forms;
using System.Drawing;

namespace DRAMVis
{
	
	public class LineGrapher : IPlottable
	{
		public NPlot.Windows.PlotSurface2D canvas = null;
		public List<VisFileContainer> containers;
		public DrawMode drawMode = DrawMode.TOTAL;
		private bool isClear = true;
		
		public string y_key, x_label, y_label, title;
		
		public void SetState(MouseState mouseState) 
		{
			Console.WriteLine("setting state to "+mouseState);
			Grapher.SetState(this.canvas, mouseState);
		}
		
		public void ResetZoom() {
			
		}
		
		private static void SetGraphDefaults(NPlot.Windows.PlotSurface2D canvas, string title) 
		{
			canvas.Padding = 15;
			canvas.ShowCoordinates = false;
			canvas.Dock = DockStyle.Fill;
			canvas.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
			canvas.Title = title;
		}
		
		public LineGrapher(NPlot.Windows.PlotSurface2D canvas, string y_key, string title, string x_label, string y_label)
		{
			containers = new List<VisFileContainer>();

			this.y_key = y_key;
			this.y_label = y_label;
			this.x_label = x_label;
			this.title = title;
			
			this.canvas = canvas;
			canvas.Title = title;
			SetGraphDefaults(this.canvas, title);
			
			
			//FUCK YOU, GRID
			/*
			Grid grid = new Grid();
			grid.VerticalGridType = Grid.GridType.Coarse;
            grid.HorizontalGridType = Grid.GridType.Coarse;
            grid.MajorGridPen = new Pen(Color.LightGray, 1.0f);	
            
			this.canvas.Add(grid);
			*/
		}
		
		public void SetDrawMode(DrawMode d) 
		{
			this.Clear();
			this.drawMode = d;
			List<VisFileContainer> vc = new List<VisFileContainer>(containers);
			SetGraphDefaults(this.canvas, title);
			foreach (VisFileContainer v in vc) 
			{
				AddToCanvas(v);
			}	
		}

		private void getRangeForDrawMode(VisFileContainer v, DrawMode d, out int start, out int end) 
		{
			int rankOffset = (int)v.systemParameters.NUM_RANKS * (int)v.deviceParameters.NUM_BANKS;
			int totalOffset = rankOffset + (int)v.systemParameters.NUM_RANKS;
			switch(d) 
			{
				case DrawMode.BANK:
					start = 0;
					end = rankOffset;
					break;
				case DrawMode.RANK:
					start = rankOffset;
					end = totalOffset;
					break;
				case DrawMode.TOTAL:
					start = totalOffset;
					end = totalOffset+1;
					break;
				case DrawMode.AVERAGE:
					start = totalOffset+1;
					end = totalOffset+2;
					break;
				default:
					start=0;
					end=totalOffset+2;
					break;
			}
		}
		
		public void AddToCanvas(VisFileContainer v) 
		{
			Console.WriteLine("ADD CALLED");
			int start, end;
			
			getRangeForDrawMode(v, this.drawMode, out start, out end);
			for (int i=start; i<end; i++)
			{
//				Console.WriteLine("Adding "+i);
				MyLinePlot linePlot = v.plotTable[y_key][i];
				{
					linePlot.Name = v.filepath+i;
					canvas.Add(linePlot);
				}

				if (!containers.Contains(v))
				{
					containers.Add(v);
				}
			}
		//	p("NumDrawn="+this.itemsDrawn);
		
			if (true || isClear)
			{
				canvas.XAxis1.Label = x_label;
				canvas.YAxis1.Label = y_label;
				
				canvas.YAxis1.LabelOffsetAbsolute = true;
				canvas.YAxis1.LabelOffset = 40;
	            canvas.XAxis1.HideTickText = false;
				isClear = false;
			}
			canvas.Refresh();
		}
		
		public void RemoveFromCanvas(VisFileContainer v) 
		{
			int start;
			int end;
			bool needUnhighlight=false;
			getRangeForDrawMode(v, this.drawMode, out start, out end);
			
			ArrayList DrawablesCopy = new ArrayList(canvas.Drawables);
			
			foreach (IDrawable d in DrawablesCopy) 
			{
				if (d is MyLinePlot) 
				{
					for (int i=start; i<end; i++)
					{
						MyLinePlot l = (MyLinePlot)d;
						string keyName = v.filepath+i;
						if (keyName.Equals(l.Name)) 
						{
							if (l.isHighlighted) 
							{
								needUnhighlight=true;
							}
//							Console.WriteLine("REMOVING="+l.Name);
							canvas.Remove(l,true);
						}
					}
				}
			}
			if (needUnhighlight) 
			{
				this.HighlightGraph(null);
			}
			canvas.Refresh();
			containers.Remove(v);
		}
		
		public void Clear() 
		{
			isClear = true;
			canvas.Clear();
			canvas.Refresh();
		 	SetGraphDefaults(canvas,title);
			
		}
		
		public void HighlightGraph(VisFileContainer v) 
		{	
			//unhighlight everyone
			foreach (IDrawable d in canvas.Drawables) 
			{
				if (d is MyLinePlot) 
				{
					MyLinePlot l = (MyLinePlot)d;
					l.Pen.Width=1.0f;
					l.Pen.Color = l.OriginalColor;
					l.isHighlighted = false;
				}
			}
			
			// if we wanted to unhighlight, that's all we need to do
			if (v == null) {
				canvas.Refresh();
				return;
			}
			int start;
			int end; 
			getRangeForDrawMode(v, this.drawMode, out start, out end);
			
			//iterate over a copy since we can't change the collection while its being iterated
			foreach (IDrawable d in new ArrayList(canvas.Drawables))
			{
				if (d is MyLinePlot) 
				{
					for (int i=start; i<end; i++) 
					{
						MyLinePlot l = (MyLinePlot)d;
						string keyName = v.filepath+i;
						if (l.Name.Equals(keyName)) 
						{
							l.Pen.Width=2.0f;
							l.Pen.Color = l.OriginalColor;
							l.isHighlighted = true;
							// remove/add to force the line graph to be on top
							canvas.Remove(l,false);
							canvas.Add(l);
							
						}
						else 
						{
							// only unhiglight a plot that we haven't previously highlighted
							if (!l.isHighlighted) 
							{
								l.Pen.Width=1.0f;
								l.Pen.Color = Color.DarkGray;
							}
						}
					}	
					// highlight a matching plot
				
				}
			}
	
			
			canvas.Refresh();
	}
		
		public static MyLinePlot MakeLinePlot(List<decimal> x_values, List<decimal> y_values, Color color) 
		{
			MyLinePlot returnPlot = new MyLinePlot(x_values, y_values, color);
			returnPlot.Pen = new Pen(color, 1.0f);
			return returnPlot;
		}

	}
}
