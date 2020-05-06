using OxyPlot;
using OxyPlot.Series;
using System;
using System.Collections.Generic;
using System.Text;

namespace SWIChallenge.Components
{
	public class MyLineSeries : LineSeries
	{

		List<ScreenPoint> outputBuffer = null;

		public bool Aliased { get; set; } = true;

		protected override void RenderLine(IRenderContext rc, OxyRect clippingRect, IList<ScreenPoint> pointsToRender)
		{
			var dashArray = this.ActualDashArray;

			if (this.outputBuffer == null)
			{
				this.outputBuffer = new List<ScreenPoint>(pointsToRender.Count);
			}

			rc.DrawClippedLine(clippingRect, pointsToRender, this.MinimumSegmentLength * this.MinimumSegmentLength, this.GetSelectableColor(this.ActualColor), this.StrokeThickness, dashArray, this.LineJoin, Aliased, this.outputBuffer);

		}
	}

}
