using Contract;
using Fluent;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Button = Fluent.Button;
using MaterialDesignThemes.Wpf;
using System.ComponentModel;
using System.Diagnostics;

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
        int _selectedPenWidth = 1;
        string _selectedStrokeType = "";
        string _selection = "shape";
        Dictionary<string, IShape> _prototypes = new Dictionary<string, IShape>();
        Dictionary<string, DoubleCollection> _strokeTypes = new Dictionary<string, DoubleCollection>();
        List<UIElement> _elements = new(); //Contain shape and image element.
        List<UIElement> _undoElements = new();
        List<UIElement> _redoElements = new();

        public MainWindow()
        {
            InitializeComponent();
        }

        private void canvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (_selection == "shape")
            {
                //clone shape of preview with selected shape
                _preview = _prototypes[_selectedShapeName].Clone();
                _isDrawing = true;
                //get start positon and save in HandleStart of preview
                Point position = e.GetPosition(canvas);
                _preview.HandleStart(position.X, position.Y);
                _preview.OutlineColor = (Color)ColorGalleryStandard.SelectedColor;
                _preview.PenWidth = _selectedPenWidth;
                _preview.StrokeType = _strokeTypes[_selectedStrokeType];
            }
            else if (_selection == "fill")
            {
                
            }
        }

        private void canvas_MouseMove(object sender, MouseEventArgs e)
        {
            //If drawing
            if (_selection == "shape")
            {
                if (_isDrawing)
                {
                    Point position = e.GetPosition(canvas);
                    //get current position
                    _preview.HandleEnd(position.X, position.Y);
                    //Clear all drawings
                    canvas.Children.Clear();
                    //Redraw all shapes that was saved before
                    foreach (var element in _elements)
                    {
                        canvas.Children.Add(element);
                    }
                    //Draw preview
                    canvas.Children.Add(_preview.Draw());
                }
            }
        }

        private void canvas_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (_selection == "shape")
            {
                if (_isDrawing == true)
                {
                    _isDrawing = false;
                    //get end position and save in HandleEnd of preview
                    Point postion = e.GetPosition(canvas);
                    _preview.HandleEnd(postion.X, postion.Y);
                    //add preview to shapes
                    _shapes.Add(_preview);
                    if (_elements.Count() != 0)
                        _undoElements.Add(_elements.Last());
                    _elements.Add(_preview.Draw());
                }
            }
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // load all the dlls
            string exePath = Assembly.GetExecutingAssembly().Location;
            string folder = System.IO.Path.GetDirectoryName(exePath);
            var dlls = new DirectoryInfo(folder).GetFiles($"DLL/*.dll").Where(file => file.Name.Contains("2D"));

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

            // add the shape buttons
            foreach (var item in _prototypes)
            {
                IShape shape = item.Value;

                Button button = new()
                {
                    Tag = shape.Name,
                    ToolTip = shape.Name
                };
                button.SizeDefinition = "Small";
                PackIcon icon = new();
                switch (shape.Name)
                {
                    case "Rectangle":
                        icon.Kind = PackIconKind.RectangleOutline;
                        break;
                    case "Square":
                        icon.Kind = PackIconKind.SquareOutline;
                        break;
                    case "Ellipse":
                        icon.Kind = PackIconKind.EllipseOutline;
                        break;
                    case "Circle":
                        icon.Kind = PackIconKind.CircleOutline;
                        break;
                    case "Triangle":
                        icon.Kind = PackIconKind.TriangleOutline;
                        break;
                    case "Right Triangle":
                        icon.Kind = PackIconKind.NetworkStrengthOutline;
                        break;
                    case "Diamond":
                        icon.Kind = PackIconKind.RhombusOutline;
                        break;
                    case "Hexagon":
                        icon.Kind = PackIconKind.HexagonOutline;
                        break;
                    default:
                        break;
                };
                button.Click += prototypeButton_Click;
                button.Icon = icon;
                ShapeGroupBox.Items.Add(button);
            }

            // add the line shape
            IShape lineShape = new Line2D.Line2D();
            _prototypes.Add(lineShape.Name, lineShape);

            // default selection
            _selectedShapeName = lineShape.Name;
            _preview = _prototypes[_selectedShapeName].Clone();
            CanvasArea.Cursor = Cursors.Cross;
            _selection = "shape";

            //add the stroke types
            _strokeTypes.Add("Solid", new DoubleCollection(new List<double>() { 1, 0 }));
            _strokeTypes.Add("Dash", new DoubleCollection(new List<double>() { 4, 4 }));
            _strokeTypes.Add("Dot", new DoubleCollection(new List<double>() { 1, 2 }));
            _strokeTypes.Add("Dash Dot", new DoubleCollection(new List<double>() { 4, 4, 1, 4 }));

            foreach (var item in _strokeTypes)
            {
                var Btn = new Button();
                Btn.Header = item.Key;
                Btn.Tag = item.Key;
                Btn.Click += SetStrokeType;
                StrokeCombobox.Items.Add(Btn);
            }
            
            _selectedStrokeType = _strokeTypes.First().Key;
        }
        private void prototypeButton_Click(object sender, RoutedEventArgs e)
        {
            var Btn = sender as System.Windows.Controls.Button;
            _selectedShapeName = Btn.Tag as string;
            _preview = _prototypes[_selectedShapeName];
            CanvasArea.Cursor = Cursors.Cross;
            _selection = "shape";
        }

        private void Export_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog dialog = new SaveFileDialog();
            dialog.Title = "Export";
            dialog.Filter = "PNG (*.png)|*.png|JPG (*.jpg)|*.jpg";
            if (dialog.ShowDialog() == true)
            {
                RenderTargetBitmap rtb = new RenderTargetBitmap((int)canvas.RenderSize.Width,
                (int)canvas.RenderSize.Height, 96d, 96d, System.Windows.Media.PixelFormats.Default);
                rtb.Render(canvas);
                //check file extension
                if (System.IO.Path.GetExtension(dialog.FileName) == ".png")
                {
                    BitmapEncoder pngEncoder = new PngBitmapEncoder();
                    pngEncoder.Frames.Add(BitmapFrame.Create(rtb));
                    using (var fs = System.IO.File.OpenWrite(dialog.FileName))
                    {
                        pngEncoder.Save(fs);
                    }
                }
                else if (System.IO.Path.GetExtension(dialog.FileName) == ".jpg")
                {
                    BitmapEncoder jpgEncoder = new JpegBitmapEncoder();
                    jpgEncoder.Frames.Add(BitmapFrame.Create(rtb));
                    using (var fs = System.IO.File.OpenWrite(dialog.FileName))
                    {
                        jpgEncoder.Save(fs);
                    }
                }
            }
        }
        private BitmapImage ConvertToBitmap(System.Drawing.Image img, string ext)
        {
            MemoryStream ms = new MemoryStream();
            if(ext == ".png")
                img.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
            if(ext ==".jpg")
                img.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);               

            BitmapImage ix = new BitmapImage();
            ix.BeginInit();
            ix.StreamSource = new MemoryStream(ms.ToArray());
            ix.EndInit();
            return ix;
        }
        private void Open_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Title = "Open";
            dialog.Filter = "Supported File(*.bin; *.png; *.jpg)|*.bin;*.png;*.jpg|Binary File (*.bin)|*.bin|Image File (*.png; *.jpg)|*.png;*.jpg";
            dialog.Multiselect = false;
            if (dialog.ShowDialog() == true)
            {
                if (System.IO.Path.GetExtension(dialog.FileName) == ".bin")
                {
                    string[] lines = File.ReadAllLines(dialog.FileName);
                    if (lines[0] != "PaintSaveFile")
                        return;
                    _shapes.Clear();
                    _elements.Clear();
                    canvas.Children.Clear();
                    IShape shape;
                    for (int i = 1; i < lines.Length; i++)
                    {
                        string[] split = lines[i].Split(" ");
                        shape = _prototypes[split[0]].Clone();
                        shape.HandleStart(Double.Parse(split[1]), Double.Parse(split[2]));
                        shape.HandleEnd(Double.Parse(split[3]), Double.Parse(split[4]));
                        shape.OutlineColor = (Color)ColorGalleryStandard.SelectedColor;
                        _shapes.Add(shape);
                        _elements.Add(shape.Draw());
                        canvas.Children.Add(shape.Draw());
                    }
                }
                else
                {
                    _shapes.Clear();
                    _elements.Clear();
                    canvas.Children.Clear();
                    System.Drawing.Image image = System.Drawing.Image.FromFile(dialog.FileName);
                    BitmapImage bitmap = ConvertToBitmap(image, System.IO.Path.GetExtension(dialog.FileName));
                    Image wpfImage = new Image
                    {
                        Source = bitmap,
                        Height = bitmap.Height,
                        Width = bitmap.Width,
                    };
                    if (canvas.Width < wpfImage.Width)
                        canvas.Width = wpfImage.Width;
                    if (canvas.Height < wpfImage.Height)
                        canvas.Height = wpfImage.Height;
                    _elements.Add(wpfImage);
                    canvas.Children.Add(wpfImage);
                }
            }
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog dialog = new SaveFileDialog();
            dialog.Title = "Save";
            dialog.Filter = "Binary File (*.bin)|*.bin";
            if (dialog.ShowDialog() == true)
            {
                string textout = "PaintSaveFile" + Environment.NewLine;
                foreach (var item in _shapes)
                {
                    //Name startX startY endX endY
                    textout += $"{item.Name} {item.GetStart().X} {item.GetStart().Y} " +
                        $"{item.GetEnd().X} {item.GetEnd().Y}" + Environment.NewLine;
                }
                File.WriteAllText(dialog.FileName, textout);
            }
        }

        private void New_Click(object sender, RoutedEventArgs e)
        {
            canvas.Children.Clear();
            _shapes.Clear();
            _preview = _prototypes["Line"].Clone();
        }

        private void Paint_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.V && Keyboard.IsKeyDown(Key.LeftCtrl))
            {
                if (Clipboard.ContainsImage())
                {
                    var clipboardImage = Clipboard.GetImage();
                    Image image = new Image
                    {
                        Source = clipboardImage,
                        Height = clipboardImage.Height,
                        Width = clipboardImage.Width,
                    };

                    if (canvas.Width < image.Width)
                        canvas.Width = image.Width;
                    if (canvas.Height < image.Height)
                        canvas.Height = image.Height;
                    _elements.Add(image);
                    canvas.Children.Add(image);
                }
            }
            if (e.Key == Key.Z && Keyboard.IsKeyDown(Key.LeftCtrl))
            {
                HandleUndo();
            }
            if (e.Key == Key.Y && Keyboard.IsKeyDown(Key.LeftCtrl))
            {
                HandleRedo();
            }

        }
        private void DrawAll()
        {            
            canvas.Children.Clear();
            foreach (var element in _elements)
                canvas.Children.Add(element);
        }
        private void HandleUndo()
        {
            if (_elements.Count() != 0)
            {
                _redoElements.Add(_elements.Last());
                _elements.RemoveAt(_elements.Count() - 1);
            }
            if (_undoElements.Count() != 0)
                _undoElements.RemoveAt(_undoElements.Count() - 1);
            DrawAll();
        }
        private void Undo_Click(object sender, RoutedEventArgs e)
        {
            HandleUndo();
        }
        private void HandleRedo()
        {
            if (_redoElements.Count() != 0)
            {
                if (_elements.Count() != 0)
                    _undoElements.Add(_elements.Last());
                _elements.Add(_redoElements.Last());
                _redoElements.RemoveAt(_redoElements.Count() - 1);
            }
            DrawAll();
        }
        private void Redo_Click(object sender, RoutedEventArgs e)
        {
            HandleRedo();
        }

        private void SetPenWidth(object sender, RoutedEventArgs e)
        {
            WidthCombobox.IsDropDownOpen = false;
            var Btn = sender as System.Windows.Controls.Button;
            _selectedPenWidth = int.Parse((string)Btn.Tag);
        }

        private void SetStrokeType(object sender, RoutedEventArgs e)
        {
            var Btn = sender as Button;
            _selectedStrokeType = (string)Btn.Tag;
        }

        private void SetFill(object sender, RoutedEventArgs e)
        {
            _selection = "fill";
            CanvasArea.Cursor = new Cursor("format-color-fill.cur");
        }
    }
}
