using Contract;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Text2D 
{
    public class Text2D : IShape
    {
        private Point2D _start;
        private Point2D _end;

        public string Name => "Text";
        public Color OutlineColor { get; set; }
        public int PenWidth { get; set; }
        public DoubleCollection StrokeType { get; set; }
        public Color FillColor { get; set; }

        public string Text { get; set; }

        public IShape Clone()
        {
            return new Text2D();
        }

        public UIElement Draw()
        {
            var textBlock =  new TextBlock()
            {
                Width = Math.Abs(_end.X - _start.X),
                Height = Math.Abs(_end.Y - _start.Y),
                Foreground = new SolidColorBrush(OutlineColor),
                Background = new SolidColorBrush(Colors.Transparent),
                Text = Text
            };
            var border = new Border() {
                BorderThickness = new Thickness(1)
            };
            var visualBrush = new VisualBrush();
            visualBrush.Visual = new Rectangle()
            {
                Width = Math.Abs(_end.X - _start.X),
                Height = Math.Abs(_end.Y - _start.Y),
                Stroke = new SolidColorBrush(Colors.Black),
                StrokeThickness = 0,
                Fill = new SolidColorBrush(Colors.Transparent),
                StrokeDashArray = new DoubleCollection(new List<double>() { 4, 2 }),
            };
            border.BorderBrush = visualBrush;
            border.Child = textBlock;
            if (_end.X - _start.X >= 0)
            {
                Canvas.SetLeft(border, _start.X);
            }
            else
            {
                Canvas.SetLeft(border, _end.X);
            }
            if (_end.Y - _start.Y >= 0)
            {
                Canvas.SetTop(border, _start.Y);
            }
            else
            {
                Canvas.SetTop(border, _end.Y);
            }
            return border;
        }

        public Point2D GetEnd()
        {
            return _end;
        }

        public Point2D GetStart()
        {
            return _start;
        }

        public void HandleEnd(double x, double y)
        {
            _end = new Point2D() { X = x, Y = y };
        }

        public void HandleStart(double x, double y)
        {
            _start = new Point2D() { X = x, Y = y };
        }
    }
}
