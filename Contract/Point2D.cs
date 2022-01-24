using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Contract
{
    public class Point2D : IShape
    {
        public double X { get; set; }
        public double Y { get; set; }
        public string Name => "Point";
        public Color OutlineColor { get; set; }
        public int PenWidth { get; set; }
        public DoubleCollection StrokeType { get; set; }
        public Color FillColor { get; set; }

        public IShape Clone()
        {
            return new Point2D();
        }

        public void HandleEnd(double x, double y)
        {
            X = x;
            Y = y;
        }

        public void HandleStart(double x, double y)
        {
            X = x;
            Y = y;
        }

        public Point2D GetStart()
        {
            return this;
        }
        public Point2D GetEnd()
        {
            return this;
        }

        public UIElement Draw()
        {
            Line l = new Line()
            {
                X1 = X,
                Y1 = Y,
                X2 = X,
                Y2 = Y,
                StrokeThickness = PenWidth,
                Stroke = new SolidColorBrush(OutlineColor),
                StrokeDashArray = StrokeType,
                Fill = new SolidColorBrush(FillColor)
            };

            return l;
        }
    }
}
