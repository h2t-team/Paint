using Contract;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Line2D
{
    public class Line2D : IShape
    {   
        private Point2D _start;
        private Point2D _end;

        public string Name => "Line";

        public Color OutlineColor { get; set; }
        public int PenWidth { get; set; }
        public DoubleCollection StrokeType { get; set; }
        public Color FillColor { get; set; }

        public void HandleStart(double x, double y)
        {
            _start = new Point2D() { X = x, Y = y };
        }

        public void HandleEnd(double x, double y)
        {
            _end = new Point2D() { X = x, Y = y };
        }

        public IShape Clone()
        {
            return new Line2D();
        }
        public Point2D GetStart()
        {
            return _start;
        }
        public Point2D GetEnd()
        {
            return _end;
        }

        public UIElement Draw()
        {
            Line l = new Line()
            {
                X1 = _start.X,
                Y1 = _start.Y,
                X2 = _end.X,
                Y2 = _end.Y,
                StrokeThickness = PenWidth,
                Stroke = new SolidColorBrush(OutlineColor),
                StrokeDashArray = StrokeType,
                Fill = new SolidColorBrush(FillColor)
        };
            return l;
        }
    }
}
