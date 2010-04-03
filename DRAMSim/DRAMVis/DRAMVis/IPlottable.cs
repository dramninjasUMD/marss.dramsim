
using System;

namespace DRAMVis
{
	
	
	public interface IPlottable
	{
		void Clear();
		void SetState(MouseState mouseState);
		void ResetZoom();
		
		//void SetZoomCallBack(NPlot.Windows.PlotSurface2D.Interactions.Interaction x);
		
		
	}
}
