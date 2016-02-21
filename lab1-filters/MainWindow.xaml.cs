using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Resources;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using lab1_filters.Filters;
using lab1_filters.Filters.Convolution_Filters;
using lab1_filters.Filters.Function_Filters;

namespace lab1_filters 
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 
    public partial class MainWindow : Window {

        //window & animation ivars
        const int kWindowMinFullWidth = 1040;
        const int kWindowMinCollapsedWidth = 740;
        const int kLeftGridWidth = 250;

        static DispatcherTimer _timer = new DispatcherTimer();
        int _animationStop = 0;
        private double _Height;
        private double _Width;

        private double _RatioHeight;
        private double _RatioWidth;

        //drawing data
        UIElementCollection functionCoordinateLines;
        List<System.Drawing.Point> functionPoints = new List<System.Drawing.Point>();

        const double pointRadius = 3.5;

        #region Window lifecycle

        public MainWindow() {
            InitializeComponent();

            //resize animation
            _timer.Interval = new TimeSpan(2);
            _timer.Tick += new EventHandler(timer_Tick);

            //startup images
            this.filteredImage.Source = new BitmapImage(new Uri(@"../../Resources/Koala.jpg", UriKind.Relative));
            this.originalImage.Source = new BitmapImage(new Uri(@"../../Resources/Koala.jpg", UriKind.Relative));

            //combobox binding
            this.DataContext = new FiltersViewModel();
            this.filtersComboBox.SelectedIndex = 0;

            this.leftBorder.Visibility = System.Windows.Visibility.Collapsed;
            this.toggleButton.Content = "Show details";
            double width = this.ActualWidth - kLeftGridWidth;
            if (width < kWindowMinCollapsedWidth)
                width = kWindowMinCollapsedWidth;
            this.MinWidth = width;
            this.Width = width;
        }

        private bool _windowActivatedOnce = false;
        DispatcherTimer _temptimer = new DispatcherTimer();
        private void Window_Activated(object sender, EventArgs e) {

            bool animating = false;

            if (_windowActivatedOnce)
                return;

            if (animating) {
                _temptimer.Interval = new TimeSpan(3000000);
                _temptimer.Tick += new EventHandler(temptimer_Tick);
                _temptimer.Start();
            }
            
            this.functionCoordinateLines = this.functionCanvas.Children;
            this.addTwoInitialPoints();
            this.Render();

            this.functionCanvas.parentWindow = this;
            _windowActivatedOnce = true;
        }

        #endregion

        #region button handles
        private void applyButton_Click(object sender, RoutedEventArgs e) 
        {
            Filter filter = (Filter)this.filtersComboBox.SelectedItem;
            this.applyFilter(filter);
        }

        //apply filter

        private void applyFilter(Filter filter) 
        {
            BitmapSource source = (BitmapSource)this.originalImage.Source;

            if (filter is IImageProcessor) {
                IImageProcessor imgPrc = (IImageProcessor)filter;
                WriteableBitmap writeableBitmap = imgPrc.ProcessImage((BitmapImage)this.originalImage.Source);
                this.filteredImage.Source = writeableBitmap;

                return;
            }

            // **********  old labs  ************
            if (filter is ConvolutionFilter) {
                Bitmap originalBitmap = source.ConvertToBitmap();
                ConvolutionFilter convolutionFilter = (ConvolutionFilter)filter;

                bool parsed = false;
                if (this.offsetTextField.Text.Replace(" ", "").Length != 0) {
                    float offset = 0;
                    parsed = (float.TryParse(this.offsetTextField.Text.Replace(" ", ""), out offset));
                    if (parsed == false) {
                        MessageBox.Show("Enter valid offset / offset value", "Invalid parameters", MessageBoxButton.OK, MessageBoxImage.Information);
                        return;
                    }
                    convolutionFilter.Offset = offset;
                }

                if (this.factorTextField.Text.Replace(" ", "").Length != 0) {
                    float factor = 0;
                    parsed = parsed && (float.TryParse(this.factorTextField.Text.Replace(" ", ""), out factor));
                    if (parsed == false) {
                        MessageBox.Show("Enter valid offset / offset value", "Invalid parameters", MessageBoxButton.OK, MessageBoxImage.Information);
                        return;
                    }
                    convolutionFilter.Factor = factor;
                }

                Bitmap filteredBitmap = originalBitmap.ConvolutionFilter(convolutionFilter);
                BitmapImage bitmapImage = filteredBitmap.BitmapToImageSource();
                this.filteredImage.Source = bitmapImage;

            } else { //function filter
                FunctionFilter functionFilter = (FunctionFilter)filter;

                if (functionFilter is FunctionFilterOffset) {
                    FunctionFilterOffset factorFunctionFilter = (FunctionFilterOffset)functionFilter;
                    float coeff = 0;
                    bool parsed = (float.TryParse(this.coeffTextField.Text.Replace(" ", ""), out coeff));

                    if (parsed == false) {
                        MessageBox.Show("Enter valid coefficient value", "Invalid parameters", MessageBoxButton.OK, MessageBoxImage.Information);
                        return;
                    }

                    factorFunctionFilter.Offset = coeff;
                }

                WriteableBitmap writeableBitmap = functionFilter.ApplyFunctionFilter((BitmapImage)this.originalImage.Source);
                this.filteredImage.Source = writeableBitmap;
            }
        }

        private void newImageButton_Click(object sender, RoutedEventArgs e) {
            OpenFileDialog op = new OpenFileDialog();
            op.Title = "Select a picture";
            op.Filter = "All supported graphics|*.jpg;*.jpeg;*.png|" +
              "JPEG (*.jpg;*.jpeg)|*.jpg;*.jpeg|" +
              "Portable Network Graphic (*.png)|*.png";
            if (op.ShowDialog() == true) {
                this.originalImage.Source = new BitmapImage(new Uri(op.FileName));
                this.filteredImage.Source = null;
            }
        }

        private void toggleButton_Click(object sender, RoutedEventArgs e) 
        {
            if (this.leftBorder.Visibility == Visibility.Visible) {
                this.leftBorder.Visibility = System.Windows.Visibility.Collapsed;
                this.toggleButton.Content = "Show details";
                double width = this.ActualWidth -kLeftGridWidth;
                if (width < kWindowMinCollapsedWidth)
                    width = kWindowMinCollapsedWidth;
                this.resize(this.ActualHeight, width);
                this.MinWidth = width;

            } else if (this.leftBorder.Visibility == Visibility.Collapsed) {
                this.leftBorder.Visibility = System.Windows.Visibility.Visible;
                this.toggleButton.Content = "Hide details";
                if (this.ActualWidth < kWindowMinFullWidth)
                    this.resize(this.ActualHeight, kWindowMinFullWidth);
            }
        }
        #endregion

        #region mouse handles

        private void functionCanvas_MouseRightButtonUp(object sender, MouseButtonEventArgs e) {
            this.coeffTextField.IsEnabled = true;

            System.Windows.Point p = Mouse.GetPosition(this.functionCanvas);
            int numberOfElements = this.functionPoints.Count;
            for (int i = 0; i < numberOfElements; i++) {
                System.Drawing.Point point = this.functionPoints[i];
                if (this.pointsAreCloseEnough(point, new System.Drawing.Point((int)p.X, (int)p.Y))) {
                    this.removeCanvasElementAt(point);
                    this.functionPoints.RemoveAt(i);
                    this.Render();
                    return; //if point found, just remove and exit
                }
            }

            //otherwise (there was no point found) create a new one
            this.addFunctionPoint(p);
            this.Render();
        }
        #endregion


        #region canvas function and point methods

        private void clearFilterButton_Click(object sender, RoutedEventArgs e) 
        {
            this.functionPoints.Clear();
            this.addTwoInitialPoints();
            this.Render();

            this.filteredImage.Source = this.originalImage.Source;
        }

        private void saveFilterButton_Click(object sender, RoutedEventArgs e) 
        {
            CustomFilter customFilter = new CustomFilter();
            customFilter.Function = this.customFunctionArrayFromCanvas();
            this.applyFilter(customFilter);
        }

        private byte[] customFunctionArrayFromCanvas() 
        {
            this.sortFunctionPoints();
            byte[] function = new byte[256];

            double y = 0;
            for (int i = 0 ; i < this.functionPoints.Count; i++) {
                if (i == this.functionPoints.Count - 1)
                    break;

                System.Drawing.Point current = this.functionPoints[i].DeepClone();
                if (current.X < 0) current.X = 0;
                System.Drawing.Point next = this.functionPoints[i + 1].DeepClone();
                if (next.X < 0) next.X = 0;

                for (int x = current.X ; x <= next.X ; x++) {
                    y = (double)this.GetY(current, next, x);
                    function[x] = (byte)(y-1);
                }
            }

            return function;
        }

        //helper
        public double GetY(System.Drawing.Point point1, System.Drawing.Point point2, int x)
        {
            double m = (point2.X - point1.X) / (point2.Y + point1.Y);
            double b = point2.X - (m * point2.Y);

            return m * x + b;
        }
        
        //helper
        private void removeCanvasElementAt(System.Drawing.Point point) 
        {
            for (int i = 0 ; i < this.functionCanvas.Children.Count ; i++) {
                UIElement element = this.functionCanvas.Children[i];
                Vector vec = VisualTreeHelper.GetOffset(element);
                if (this.pointsAreCloseEnough(point, new System.Drawing.Point((int)vec.X, (int)vec.Y))) {
                    this.functionCanvas.Children.RemoveAt(i);
                    return;
                }
            }
        }

        //points
        private bool pointsAreCloseEnough(System.Drawing.Point p1, System.Drawing.Point p2) 
        {
            int offset = 15;
            if (Math.Abs(p1.X - p2.Y) < offset && Math.Abs(p1.Y - p2.X) < offset)
                return true;
            return false;
        }

        private void addTwoInitialPoints() 
        {
            this.addFunctionPoint(new System.Windows.Point(0, 255));
            this.addFunctionPoint(new System.Windows.Point(255, 0));
        }

        private void addFunctionPoint(System.Windows.Point p) 
        {
            System.Drawing.Point point = new System.Drawing.Point();
            point.Y = (int)p.X;
            point.X = (int)p.Y;
            this.functionPoints.Add(point);
        }

        private void sortFunctionPoints() 
        {
            List<System.Drawing.Point> sortedList = this.functionPoints.OrderBy(p => p.X).ToList();
            this.functionPoints = sortedList;
        }

        //workaround for Windows.Point and Drawing.Point mismatch
        private void sortInvertFunctionPoints() 
        {
            List<System.Drawing.Point> sortedList = this.functionPoints.OrderBy(p => p.Y).ToList();
            this.functionPoints = sortedList;
        }

        #endregion

        public void Render() 
        {
            for (int i = 0; i < this.functionCanvas.Children.Count; i++) {
                if(!_windowActivatedOnce)
                    break;

                if (this.functionCanvas.Children[i].GetType() == typeof(Line)) {
                    Line l = (Line)this.functionCanvas.Children[i];
                    if ((int)l.StrokeThickness != 2) {
                        this.functionCanvas.Children.RemoveAt(i);
                        i--;
                    }

                } else if (this.functionCanvas.Children[i] is Ellipse) {
                    this.functionCanvas.Children.RemoveAt(i);
                    i--;
                }
            }

            int numberOfElements = this.functionPoints.Count;

            for (int i = 0; i < numberOfElements; i++) {
                this.RenderValue(functionPoints[i]);
            }

            this.RenderLines();
        }


        private void RenderValue(System.Drawing.Point point) 
        {
            int x = point.X;
            int y = point.Y;
            int width = (int)pointRadius*2;
            var value = new Ellipse {
                Width = width,
                Height = width,
                Stroke = System.Windows.Media.Brushes.Black,
                StrokeThickness = 6,
            };

            Canvas.SetTop(value, x - (double)pointRadius);
            Canvas.SetLeft(value, y - (double)pointRadius);
            this.functionCanvas.Children.Add(value);
        }

        public void PointDragged() {
            this.functionPoints.Clear();
            foreach (UIElement element in this.functionCanvas.Children) {
                if (element is Ellipse) {
                    GeneralTransform generalTransform = element.TransformToVisual(functionCanvas);
                    System.Windows.Point childToParentCoordinates = generalTransform.Transform(new System.Windows.Point(0, 0));
                    this.functionPoints.Add(new System.Drawing.Point((int)(childToParentCoordinates.Y + pointRadius), (int)(childToParentCoordinates.X + pointRadius)));
                }
            }

            this.RenderLines();
        }

        private void RenderLines() 
        {
            this.sortInvertFunctionPoints();

            PointCollection pCollection = new PointCollection();
            foreach(System.Drawing.Point point in this.functionPoints) {
                pCollection.Add(new System.Windows.Point(point.Y , point.X ));
            }

            this.polyLine.Points = pCollection;
        }

        #region resize animation
        //resize animation
        public void resize(double height, double width) {
            _timer.Stop();
            _Height = height;
            _Width = width;
            _timer.Start();
        }

        private void timer_Tick(Object myObject, EventArgs myEventArgs) {
            if (_animationStop == 0) {
                _RatioHeight = ((this.Height - _Height) / 12) * -1;
                _RatioWidth = ((this.Width - _Width) / 12) * -1;
            }

            _animationStop++;

            this.Height += _RatioHeight;
            this.Width += _RatioWidth;

            if (_animationStop == 12) {
                _timer.Stop();
                _animationStop = 0;
                this.Height = _Height;
                this.Width = _Width;
                if(this.leftBorder.Visibility == System.Windows.Visibility.Visible)
                    this.MinWidth = kWindowMinFullWidth;
            }
        }

        private void filtersComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            if (sender != this.filtersComboBox)
                return;

            Filter filter;
            filter = (Filter)this.filtersComboBox.SelectedItem;

            if (filter is ConvolutionFilter) {
                this.factorTextField.IsEnabled = true;
                this.offsetTextField.IsEnabled = true;
                this.factorTextField.Text = "" + ((ConvolutionFilter)filter).Factor;
                this.offsetTextField.Text = "" + ((ConvolutionFilter)filter).Offset;
                this.coeffTextField.IsEnabled = true;

            } else if(filter is FunctionFilterOffset) {
                this.factorTextField.IsEnabled = false;
                this.offsetTextField.IsEnabled = false;
                this.coeffTextField.IsEnabled = true;
                this.factorTextField.Text = "";
                this.offsetTextField.Text = "";
                this.coeffTextField.Text = "" + ((FunctionFilterOffset)filter).Offset;

            } else {
                this.factorTextField.IsEnabled = false;
                this.offsetTextField.IsEnabled = false;
                this.coeffTextField.IsEnabled = true;
                this.factorTextField.Text = "";
                this.offsetTextField.Text = "";
            }
        }

        private void temptimer_Tick(Object myObject, EventArgs myEventArgs) {
            _temptimer.Stop();

            this.leftBorder.Visibility = System.Windows.Visibility.Visible;
            this.toggleButton.Content = "Hide details";
            if (this.ActualWidth < kWindowMinFullWidth)
                this.resize(this.ActualHeight, kWindowMinFullWidth);

            _temptimer = null;
        }

        #endregion

    }
}
