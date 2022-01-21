using System;
using System.Collections.Generic;
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
            Rect adornedElementRect = new Rect(this.AdornedElement.DesiredSize);

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