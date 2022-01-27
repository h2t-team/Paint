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
using Paint.DataType;
using System.Windows.Documents;
using System.Windows.Shapes;

namespace Paint
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : RibbonWindow
    {
        bool _isDrawing = false;
        bool _isDragging = false;
        bool _isEditing = false;
        bool _isZooming = false;
        bool dragStarted = false;
        List<IShape> _shapes = new List<IShape>();
        IShape _preview;
        IShape _zoomRectangle;
        UIElement backgroundElement = null;
        string _selectedShapeName = "";
        int _selectedPenWidth = 1;
        string _selectedStrokeType = "";
        string _selection = "shape";
        Dictionary<string, IShape> _prototypes = new Dictionary<string, IShape>();
        Dictionary<string, DoubleCollection> _strokeTypes = new Dictionary<string, DoubleCollection>();
        List<UIElement> _elements = new(); //Contain shape and image element.
        Stack<State> _undoStates = new();
        Stack<State> _redoStates = new();
        CircleAdorner adoner;
        private enum HitType
        {
            None, Body, UL, UR, LR, LL, T, B, L, R
        };
        HitType MouseHitType = HitType.None;
        // The drag's last point.
        private Point LastPoint;
        private void GetLRTB(IShape shape, out double left, out double right, out double top, out double bottom)
        {
            left = shape.GetStart().X < shape.GetEnd().X ? shape.GetStart().X : shape.GetEnd().X;
            top = shape.GetStart().Y < shape.GetEnd().Y ? shape.GetStart().Y : shape.GetEnd().Y;
            right = shape.GetStart().X > shape.GetEnd().X ? shape.GetStart().X : shape.GetEnd().X;
            bottom = shape.GetStart().Y > shape.GetEnd().Y ? shape.GetStart().Y : shape.GetEnd().Y;
        }
        private HitType SetHitType(IShape shape, Point point)
        {
            double left, top, right, bottom;
            GetLRTB(shape, out left, out right, out top, out bottom);
            const double GAP = 10;
            if (point.X < left - GAP) return HitType.None;
            if (point.X > right + GAP) return HitType.None;
            if (point.Y < top - GAP) return HitType.None;
            if (point.Y > bottom + GAP) return HitType.None;

            if (Math.Abs(point.X - left) < GAP)
            {
                // Left edge.
                if (Math.Abs(point.Y - top) < GAP) return HitType.UL;
                if (Math.Abs(bottom - point.Y) < GAP) return HitType.LL;
                return HitType.L;
            }
            else if (Math.Abs(right - point.X) < GAP)
            {
                // Right edge.
                if (Math.Abs(point.Y - top) < GAP) return HitType.UR;
                if (Math.Abs(bottom - point.Y) < GAP) return HitType.LR;
                return HitType.R;
            }
            if (point.Y - top < GAP) return HitType.T;
            if (bottom - point.Y < GAP) return HitType.B;
            return HitType.Body;
        }
        private void SetMouseCursor()
        {
            // See what cursor we should display.
            Cursor desired_cursor = Cursors.Cross;
            switch (MouseHitType)
            {
                case HitType.None:
                    desired_cursor = Cursors.Cross;
                    break;
                case HitType.Body:
                    desired_cursor = Cursors.SizeAll;
                    break;
                case HitType.UL:
                case HitType.LR:
                    desired_cursor = Cursors.SizeNWSE;
                    break;
                case HitType.LL:
                case HitType.UR:
                    desired_cursor = Cursors.SizeNESW;
                    break;
                case HitType.T:
                case HitType.B:
                    desired_cursor = Cursors.SizeNS;
                    break;
                case HitType.L:
                case HitType.R:
                    desired_cursor = Cursors.SizeWE;
                    break;
            }
            // Display the desired cursor.
            if (Cursor != desired_cursor) Cursor = desired_cursor;
        }
        public MainWindow()
        {
            InitializeComponent();
        }
        private void EndEdit()
        {
            if (_isEditing)
            {
                _redoStates.Clear();
                SaveUndoState();
                _isEditing = false;
                _isDragging = false;
                _shapes.Add(_preview);
                _elements.Add(_preview.Draw());
                DrawAll();
                MouseHitType = HitType.None;
                SetMouseCursor();
                return;
            }
        }
        private void canvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (_selection == "shape")
            {
                if (MouseHitType == HitType.None)
                    EndEdit();
                if (_isEditing)
                {
                    if (!_isDragging)
                    {
                        MouseHitType = SetHitType(_preview, Mouse.GetPosition(CanvasArea));
                        SetMouseCursor();
                        if (MouseHitType == HitType.None) return;
                        LastPoint = Mouse.GetPosition(CanvasArea);
                        _isDragging = true;
                    }
                }
                else
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
                    _preview.FillColor = Colors.Transparent;
                }
            }
            else if (_selection == "fill")
            {
                IShape cur = null;
                Point pt = e.GetPosition(canvas);
                HitTestResult result = VisualTreeHelper.HitTest(canvas, pt);
                if (result != null)
                {
                    if (result.VisualHit is Shape)
                    {
                        _redoStates.Clear();
                        SaveUndoState();
                        foreach (var item in _shapes)
                        {
                            MouseHitType = SetHitType(item, pt);
                            if (MouseHitType != HitType.None)
                            {
                                cur = item;
                                cur.FillColor = (Color)ColorGalleryStandard.SelectedColor;
                            }
                        }
                        Shape shape = result.VisualHit as Shape;
                        foreach (var item in _elements)
                        {
                            if (item.Equals(shape) == true)
                            {
                                (item as Shape).Fill = new SolidColorBrush((Color)ColorGalleryStandard.SelectedColor);
                                DrawAll();
                            }
                        }
                    }
                    else if (result.VisualHit is Canvas)
                    {
                        //canvas.Color
                        _redoStates.Clear();
                        SaveUndoState();
                        canvas.Background = new SolidColorBrush((Color)ColorGalleryStandard.SelectedColor);
                    }
                }
            }
            else if (_selection == "text")
            {
                _isDrawing = true;
                Point position = e.GetPosition(canvas);
                _elements.Add(new TextBlock());
            }
            else if (_selection=="zoom")
            {
                Point position = e.GetPosition(canvas);
                Console.WriteLine("abc");
                zoomSlider.Value = zoomSlider.Value * 2;
                TransformGroup g = new TransformGroup();
                g.Children.Add(new ScaleTransform(zoomSlider.Value, zoomSlider.Value, position.X, position.Y));
                g.Children.Add(new TranslateTransform(-50, -50));
                canvas.LayoutTransform = g;
               

                scrollViewer.ScrollToVerticalOffset(position.Y);
                scrollViewer.ScrollToHorizontalOffset(position.X) ;

                //   canvas.RenderTransform = g;

            }
        }
        void scrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {

            ScrollViewer scrollViewer = sender as ScrollViewer;

            if (e.ExtentHeightChange != 0 || e.ExtentWidthChange != 0)
            {

                double xMousePositionOnScrollViewer = Mouse.GetPosition(scrollViewer).X;
                double yMousePositionOnScrollViewer = Mouse.GetPosition(scrollViewer).Y;
                double offsetX = e.HorizontalOffset + xMousePositionOnScrollViewer;
                double offsetY = e.VerticalOffset + yMousePositionOnScrollViewer;

                double oldExtentWidth = e.ExtentWidth - e.ExtentWidthChange;
                double oldExtentHeight = e.ExtentHeight - e.ExtentHeightChange;

                double relx = offsetX / oldExtentWidth;
                double rely = offsetY / oldExtentHeight;

                offsetX = Math.Max(relx * e.ExtentWidth - xMousePositionOnScrollViewer, 0);
                offsetY = Math.Max(rely * e.ExtentHeight - yMousePositionOnScrollViewer, 0);


                ScrollViewer scrollViewerTemp = sender as ScrollViewer;
                scrollViewerTemp.ScrollToHorizontalOffset(offsetX);
                scrollViewerTemp.ScrollToVerticalOffset(offsetY);
            }
        }

        private void canvas_MouseMove(object sender, MouseEventArgs e)
        {
            //If drawing
            if (_isDrawing)
            {
                Point position = e.GetPosition(canvas);
                //get current position
                _preview.HandleEnd(position.X, position.Y);
                DrawAll();
                //Draw preview
                canvas.Children.Add(_preview.Draw());
            }
            else if (_isEditing)
            {
                if (_isDragging)
                {
                    // See how much the mouse has moved.
                    Point point = Mouse.GetPosition(CanvasArea);
                    double offset_x = point.X - LastPoint.X;
                    double offset_y = point.Y - LastPoint.Y;

                    // Get the shape's current position.
                    double left, top, right, bottom;
                    GetLRTB(_preview, out left, out right, out top, out bottom);
                    Point2D new_start = new Point2D();
                    Point2D new_end = new Point2D();
                    // Update the shape.
                    switch (MouseHitType)
                    {
                        case HitType.Body:
                            left += offset_x;
                            right += offset_x;
                            top += offset_y;
                            bottom += offset_y;
                            break;
                        case HitType.UL:
                            left += offset_x;
                            top += offset_y;
                            break;
                        case HitType.UR:
                            top += offset_y;
                            right += offset_x;
                            break;
                        case HitType.LR:
                            bottom += offset_y;
                            right += offset_x;
                            break;
                        case HitType.LL:
                            bottom += offset_y;
                            left += offset_x;
                            break;
                        case HitType.L:
                            left += offset_x;
                            break;
                        case HitType.R:
                            right += offset_x;
                            break;
                        case HitType.B:
                            bottom += offset_y;
                            break;
                        case HitType.T:
                            top += offset_y;
                            break;
                    }
                    if (left < right && top < bottom)
                    {
                        new_start.X = _preview.GetStart().X < _preview.GetEnd().X ? left : right;
                        new_start.Y = _preview.GetStart().Y < _preview.GetEnd().Y ? top : bottom;
                        new_end.X = _preview.GetStart().X > _preview.GetEnd().X ? left : right;
                        new_end.Y = _preview.GetStart().Y > _preview.GetEnd().Y ? top : bottom;
                        _preview.HandleStart(new_start.X, new_start.Y);
                        _preview.HandleEnd(new_end.X, new_end.Y);
                        DrawAll();
                        canvas.Children.Add(_preview.Draw());
                        adoner = new CircleAdorner(canvas.Children[canvas.Children.Count - 1]);
                        AdornerLayer.GetAdornerLayer(canvas.Children[canvas.Children.Count - 1]).Add(new CircleAdorner(canvas.Children[canvas.Children.Count - 1]));
                        // Save the mouse's new location.
                        LastPoint = point;
                    }
                }
                else
                {
                    MouseHitType = SetHitType(_preview, Mouse.GetPosition(CanvasArea));
                    SetMouseCursor();
                }
            }
            else if (_isZooming)
            {
                Point position = e.GetPosition(canvas);
                ////clone shape of preview with selected shape
                ////IShape rec = new Rectangle();
                _zoomRectangle = _prototypes["Rectangle"].Clone();
                ////get current position
                _zoomRectangle.HandleStart(position.X - 10, position.Y - 10);
                _zoomRectangle.HandleEnd(position.X + 10, position.Y + 10);
                DrawAll();
                //Draw preview
                _zoomRectangle.PenWidth = _selectedPenWidth;
                _zoomRectangle.StrokeType = _strokeTypes[_selectedStrokeType];
                _zoomRectangle.OutlineColor = (Color)ColorGalleryStandard.SelectedColor;
                canvas.Children.Add(_zoomRectangle.Draw());
            }
        }
        private void canvas_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (_isDrawing == true)
            {
                _isDrawing = false;
                //get end position and save in HandleEnd of preview
                Point postion = e.GetPosition(canvas);
                _preview.HandleEnd(postion.X, postion.Y);
                if (_preview.GetStart().X == _preview.GetEnd().X && _preview.GetStart().Y == _preview.GetEnd().Y)
                    return;
                _isEditing = true;
                adoner = new CircleAdorner(canvas.Children[canvas.Children.Count - 1]);
                AdornerLayer.GetAdornerLayer(canvas.Children[canvas.Children.Count - 1]).Add(adoner);
            }
            if (_isDragging)
            {
                _isDragging = false;
            }
            if (_isZooming)
            {
                Point position = e.GetPosition(canvas);
                ////clone shape of preview with selected shape
                ////IShape rec = new Rectangle();
                _zoomRectangle = _prototypes["Rectangle"].Clone();


                ////get current position
                _zoomRectangle.HandleStart(position.X - 10 * 2, position.Y - 10 * 2);
                _zoomRectangle.HandleEnd(position.X + 10 / 2, position.Y + 10 / 2);
                //Clear all drawings
                canvas.Children.Clear();
                //Redraw all shapes that was saved before
                foreach (var element in _elements)
                {
                    canvas.Children.Add(element);
                }
                //Draw preview
                canvas.Children.Add(_zoomRectangle.Draw());
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
            EndEdit();
            var Btn = sender as System.Windows.Controls.Button;
            _selectedShapeName = Btn.Tag as string;
            _preview = _prototypes[_selectedShapeName];
            _selection = "shape";
            _isZooming = false;
            //_isDrawing = true;
        }
        private void Export_Click(object sender, RoutedEventArgs e)
        {
            EndEdit();
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
            if (ext == ".png")
                img.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
            if (ext == ".jpg")
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
                    IShape shape;
                    for (int i = 1; i < lines.Length; i++)
                    {
                        string[] split = lines[i].Split(" ");
                        shape = _prototypes[split[0]].Clone();
                        shape.HandleStart(Double.Parse(split[1]), Double.Parse(split[2]));
                        shape.HandleEnd(Double.Parse(split[3]), Double.Parse(split[4]));
                        shape.StrokeType = _strokeTypes[split[5]];
                        shape.PenWidth = int.Parse(split[6]);
                        shape.OutlineColor = (Color)ColorConverter.ConvertFromString(split[7]);
                        shape.FillColor = (Color)ColorConverter.ConvertFromString(split[8]);
                        _shapes.Add(shape);
                        _elements.Add(shape.Draw());
                        DrawAll();
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
                    backgroundElement = wpfImage;
                    canvas.Children.Add(wpfImage);
                }
            }
        }
        private string FindStrokeType(DoubleCollection stroke)
        {
            foreach (var item in _strokeTypes)
            {
                if (item.Value.SequenceEqual(stroke))
                {
                    return item.Key;
                }
            }
            return "";
        }
        private void Save_Click(object sender, RoutedEventArgs e)
        {
            EndEdit();
            SaveFileDialog dialog = new SaveFileDialog();
            dialog.Title = "Save";
            dialog.Filter = "Binary File (*.bin)|*.bin";
            if (dialog.ShowDialog() == true)
            {
                string textout = "PaintSaveFile" + Environment.NewLine;
                foreach (var item in _shapes)
                {
                    //Name startX startY endX endY stroketype width strokecolor fillcolor
                    textout += $"{item.Name} {item.GetStart().X} {item.GetStart().Y} " +
                        $"{item.GetEnd().X} {item.GetEnd().Y} {FindStrokeType(item.StrokeType)} " +
                        $"{item.PenWidth} {item.OutlineColor.ToString()} {item.FillColor.ToString()}" + Environment.NewLine;
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
            if (backgroundElement != null)
                canvas.Children.Add(backgroundElement);
            foreach (var element in _elements)
                canvas.Children.Add(element);
        }
        private void SaveUndoState()
        {
            State state = new(_shapes, canvas.Background.ToString());
            if (_undoStates.Count == 0 || !_undoStates.Peek().Equals(state))
            {
                _undoStates.Push(state);
            }
        }
        private void LoadState(State state)
        {
            _elements.Clear();
            _shapes.Clear();
            _shapes = new(state.Shapes);
            foreach (var item in _shapes)
            {
                _elements.Add(item.Draw());
            }
            canvas.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(state.Background));
            DrawAll();
        }
        private void HandleUndo()
        {
            EndEdit();
            if (_undoStates.Count != 0)
            {
                State redoState = new State(_shapes, canvas.Background.ToString());
                _redoStates.Push(redoState);
                State undoState = _undoStates.Pop();
                LoadState(undoState);
            }
        }
        private void Undo_Click(object sender, RoutedEventArgs e)
        {
            HandleUndo();
        }
        private void HandleRedo()
        {
            EndEdit();
            if (_redoStates.Count != 0)
            {
                State undoState = new State(_shapes, canvas.Background.ToString());
                _undoStates.Push(undoState);
                State redoState = _redoStates.Pop();
                LoadState(redoState);
            }
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
            if (_isEditing)
            {
                _preview.PenWidth = _selectedPenWidth;
                DrawAll();
                canvas.Children.Add(_preview.Draw());
                adoner = new CircleAdorner(canvas.Children[canvas.Children.Count - 1]);
                AdornerLayer.GetAdornerLayer(canvas.Children[canvas.Children.Count - 1]).Add(adoner);
            }
        }
        private void SetStrokeType(object sender, RoutedEventArgs e)
        {
            var Btn = sender as Button;
            _selectedStrokeType = (string)Btn.Tag;
            if (_isEditing)
            {
                _preview.StrokeType = _strokeTypes[_selectedStrokeType];
                DrawAll();
                canvas.Children.Add(_preview.Draw());
                adoner = new CircleAdorner(canvas.Children[canvas.Children.Count - 1]);
                AdornerLayer.GetAdornerLayer(canvas.Children[canvas.Children.Count - 1]).Add(adoner);
            }
        }
        private void SetFill(object sender, RoutedEventArgs e)
        {
            EndEdit();
            _selection = "fill";
        }
        private void canvas_MouseLeave(object sender, MouseEventArgs e)
        {
            Cursor = Cursors.Arrow;
        }
        private void canvas_MouseEnter(object sender, MouseEventArgs e)
        {
            if (_selection == "shape")
                Cursor = Cursors.Cross;
            else if (_selection == "fill")
                Cursor = new Cursor("format-color-fill.cur");
            else if (_selection == "text")
                Cursor = Cursors.IBeam;
        }
        private void ColorGalleryStandard_SelectedColorChanged(object sender, RoutedEventArgs e)
        {
            if (_isEditing)
            {
                _preview.OutlineColor = (Color)ColorGalleryStandard.SelectedColor;
                DrawAll();
                canvas.Children.Add(_preview.Draw());
                adoner = new CircleAdorner(canvas.Children[canvas.Children.Count - 1]);
                AdornerLayer.GetAdornerLayer(canvas.Children[canvas.Children.Count - 1]).Add(adoner);
            }
        }

        private void SetText(object sender, RoutedEventArgs e)
        {
            EndEdit();
            _selection = "text";
        }

        private void zoom_Click(object sender, RoutedEventArgs e)
        {
            EndEdit();
            _selection = "zoom";
            _isZooming = true;
            //_isDragging = false;
            _isDrawing = false;
        }

        private void zoomSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (dragStarted)
            {
                TransformGroup g = new TransformGroup();
                g.Children.Add(new ScaleTransform(zoomSlider.Value, zoomSlider.Value));
                g.Children.Add(new TranslateTransform(0,0));
                canvas.LayoutTransform = g;
            }

        }

        private void Slider_DragStarted(object sender, System.Windows.Controls.Primitives.DragStartedEventArgs e)
        {
            this.dragStarted = true;
        }

        private void Slider_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            this.dragStarted = false;
        }
    }
}
