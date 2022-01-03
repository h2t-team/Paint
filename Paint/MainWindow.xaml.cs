﻿using Contract;
using Fluent;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Paint
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : RibbonWindow
    {
        bool _isDrawing = false;
        List<IShape> _shapes = new List<IShape>();
        IShape _preview;
        string _selectedShapeName = "";
        Dictionary<string, IShape> _prototypes = new Dictionary<string, IShape>();

        public MainWindow()
        {
            InitializeComponent();
        }

        private void canvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            //clone shape of preview with selected shape
            _preview = _prototypes[_selectedShapeName].Clone();
            _isDrawing = true;
            //get start positon and save in HandleStart of preview
            Point position = e.GetPosition(canvas);
            _preview.HandleStart(position.X, position.Y);
        }

        private void canvas_MouseMove(object sender, MouseEventArgs e)
        {
            //If drawing
            if (_isDrawing)
            {
                Point position = e.GetPosition(canvas);
                //get current position
                _preview.HandleEnd(position.X, position.Y);
                //Clear all drawings
                canvas.Children.Clear();
                //Redraw all shapes that was saved before
                foreach (var shape in _shapes)
                {
                    UIElement element = shape.Draw();
                    canvas.Children.Add(element);
                }
                //Draw preview
                canvas.Children.Add(_preview.Draw());
            }
        }

        private void canvas_MouseUp(object sender, MouseButtonEventArgs e)
        {
            _isDrawing = false;
            //get end position and save in HandleEnd of preview
            Point postion = e.GetPosition(canvas);
            _preview.HandleEnd(postion.X, postion.Y);
            //add preview to shapes
            _shapes.Add(_preview);
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Khoan làm cái Line2D
            string exePath = Assembly.GetExecutingAssembly().Location;
            string folder = System.IO.Path.GetDirectoryName(exePath);
            var dlls = new DirectoryInfo(folder).GetFiles($"DLL/*.dll").Where(file =>  file.Name.Contains("2D"));

            foreach (var dll in dlls)
            {
                var assembly = Assembly.LoadFile(dll.FullName);
                var types = assembly.GetTypes();

                foreach (var type in types)
                {
                    if (type.IsClass)
                    {
                        if (typeof(IShape).IsAssignableFrom(type))
                        {
                            var shape = Activator.CreateInstance(type) as IShape;
                            _prototypes.Add(shape.Name, shape);
                        }
                    }
                }
            }
            foreach (var item in _prototypes)
            {
                var shape = item.Value;
                switch (shape.Name)
                {
                    case "Rectangle":
                        Rectangle2DBtn.Tag = shape.Name;
                        Rectangle2DBtn.Click += prototypeButton_Click;
                        break;
                    case "Ellipse":
                        Ellipse2DBtn.Tag = shape.Name;
                        Ellipse2DBtn.Click += prototypeButton_Click;
                        break;
                };
            }

            _selectedShapeName = _prototypes.First().Value.Name;
            _preview = _prototypes[_selectedShapeName].Clone();
        }
        private void prototypeButton_Click(object sender, RoutedEventArgs e)
        {
            _selectedShapeName = (sender as System.Windows.Controls.Button).Tag as string;
            _preview = _prototypes[_selectedShapeName];
        }
    }
}
