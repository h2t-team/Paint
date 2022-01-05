using Contract;
using System;
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

        public void HandleStart(double x, double y)
        {
            _start = new Point2D() { X = x, Y = y };
        }

        public void HandleEnd(double x, double y)
        {
            _end = new Point2D() { X = x, Y = y };
        }

        public UIElement Draw()
        {
            Line l = new Line()
            {
                X1 = _start.X,
                Y1 = _start.Y,
                X2 = _end.X,
                Y2 = _end.Y,
                StrokeThickness = 1,
                Stroke = new SolidColorBrush(Colors.Black)
            };

            return l;
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
    }
}
