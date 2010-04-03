
using System;
using NPlot;
using System.Collections.Generic;
using System.Drawing;

namespace DRAMVis
{
	
	
	public class MyLinePlot : NPlot.LinePlot
	{
		private string name_;
		private Color originalColor_; 
		public string Name 
		{
			get { return name_;	}
			set { name_ = value; }
		}
		
		public Color OriginalColor { 
			get { return originalColor_; }
			set { originalColor_ = value; }
		}
		
		public bool isHighlighted = false;
		
		public MyLinePlot() : base()
		{
		}
		public MyLinePlot(List<decimal> x, List<decimal> y) : base(y,x)
		{
			this.OriginalColor = Color.Blue;
		}
		public MyLinePlot(List<decimal> x, List<decimal> y, Color c) : base(y,x)
		{
			this.OriginalColor = c;
		}
		
	}
}
