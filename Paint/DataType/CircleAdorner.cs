using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;

namespace Paint.DataType
{
    internal class CircleAdorner : Adorner
    {
        public CircleAdorner(UIElement adornedElement) : base(adornedElement)
        {
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            Rect adornedElementRect;
            if(AdornedElement.GetType() == typeof(System.Windows.Shapes.Line)){
                double x1 = ((System.Windows.Shapes.Line)AdornedElement).X1;
                double y1 = ((System.Windows.Shapes.Line)AdornedElement).Y1;
                double x2 = ((System.Windows.Shapes.Line)AdornedElement).X2;
                double y2 = ((System.Windows.Shapes.Line)AdornedElement).Y2;
                adornedElementRect = new Rect(new Point(x1,y1), new Point(x2,y2));
            }
            else
                adornedElementRect = new Rect(this.AdornedElement.RenderSize);

            // circle drawing.
            Pen circlePen = new Pen(new SolidColorBrush(Colors.Gray), 1.5);
            double renderRadius = 2;

            //rect drawing
            Pen rectPen = new Pen(new SolidColorBrush(Colors.Black), 0.1);

            // Draw a circle at each corner.
            double center = (adornedElementRect.Left + adornedElementRect.Right) / 2;
            double middle = (adornedElementRect.Top + adornedElementRect.Bottom) / 2;
            drawingContext.DrawRectangle(null, rectPen, adornedElementRect);
            drawingContext.DrawEllipse(null, circlePen, adornedElementRect.TopLeft, renderRadius, renderRadius);
            drawingContext.DrawEllipse(null, circlePen, new Point(center, adornedElementRect.Top), renderRadius, renderRadius);
            drawingContext.DrawEllipse(null, circlePen, adornedElementRect.TopRight, renderRadius, renderRadius);
            drawingContext.DrawEllipse(null, circlePen, new Point(adornedElementRect.Right, middle), renderRadius, renderRadius);
            drawingContext.DrawEllipse(null, circlePen, adornedElementRect.BottomLeft, renderRadius, renderRadius);
            drawingContext.DrawEllipse(null, circlePen, new Point(center, adornedElementRect.Bottom), renderRadius, renderRadius);
            drawingContext.DrawEllipse(null, circlePen, adornedElementRect.BottomRight, renderRadius, renderRadius);
            drawingContext.DrawEllipse(null, circlePen, new Point(adornedElementRect.Left, middle), renderRadius, renderRadius);
        }
    }
}