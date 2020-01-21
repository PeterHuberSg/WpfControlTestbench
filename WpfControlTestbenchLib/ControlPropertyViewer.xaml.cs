using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml;
using System.ComponentModel;


namespace WpfTestbench {


  /// <summary>
  /// Interaction logic for ControlPropertyViewer.xaml
  /// </summary>
  public partial class ControlPropertyViewer: UserControl {


    /// <summary>
    /// FrameworkElement for which properties values like margin get displayed. If TestFrameworkElement inherits from a Control,
    /// also font related properties get displayed.
    /// </summary>
    public FrameworkElement? TestFrameworkElement {
      get { return (FrameworkElement)GetValue(TestFrameworkElementProperty); }
      set { SetValue(TestFrameworkElementProperty, value); }
    }

    // Using a DependencyProperty as the backing store for TestFrameworkElement.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty TestFrameworkElementProperty = 
    DependencyProperty.Register("TestFrameworkElement", typeof(FrameworkElement), typeof(ControlPropertyViewer), 
      new UIPropertyMetadata(null, testFrameworkElement_Changed));


    private static void testFrameworkElement_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e) {
      if (e.OldValue!=null) throw new NotSupportedException("It is not possible to initialise TestFrameworkElement twice.");

      ControlPropertyViewer controlPropertyViewer = (ControlPropertyViewer)d;
      //ControlPropertyViewer needs to add some lines to the host container of TestFrameworkElement, which is supposed to be a Grid.
      //When defined in XAML, the TestFrameworkElement property of ControlPropertyViewer gets set before TestFrameworkElement gets added
      //to the grid. For this reason, we have to delay the setup(), if ControlPropertyViewer is not loaded yet.
      if (controlPropertyViewer.IsLoaded) {
        FrameworkElement testFrameworkElement = (FrameworkElement)e.NewValue;
        controlPropertyViewer.setup(testFrameworkElement);
      }
    }


    public ControlPropertyViewer() {
      InitializeComponent();

      Loaded += controlPropertyViewer_Loaded;
      TemplateButton.Click += new RoutedEventHandler(templateButton_Click);
      DebugButton.Click += debugButton_Click;

      FontSize = 11;
    }


    void controlPropertyViewer_Loaded(object sender, RoutedEventArgs e) {
      if (TestFrameworkElement!=null) {
        setup(TestFrameworkElement);
      }
    }


    void templateButton_Click(object sender, RoutedEventArgs e) {
      if (testControl!=null) {
        var stringBuilder = new StringBuilder();
        var xmlSettings = new XmlWriterSettings { Indent = true };

        using (var xmlWriter = XmlWriter.Create(stringBuilder, xmlSettings)) {
          System.Windows.Markup.XamlWriter.Save(testControl.Template, xmlWriter);
        }
        MessageBox.Show(stringBuilder.ToString());
      }
    }


    void debugButton_Click(object sender, RoutedEventArgs e) {
      System.Diagnostics.Debugger.Break();
    }


    FrameworkElement? testFrameworkElement;
    Control? testControl; //gives access to TestFrameworkElement as a Control. This is needed for access to Fonts and Padding
    Grid? hostGrid;
    int? hostGridRow;
    int? hostGirdColumn;


//////private void setup(FrameworkElement testFrameworkElement, Grid hostGrid, int hostGridRow, int hostGirdColumn){
    private void setup(FrameworkElement testFrameworkElement){
      try { //improve how WPF handles exceptions in the constructor
        DependencyObject vparent = VisualTreeHelper.GetParent(testFrameworkElement);
        DependencyObject vgparent = VisualTreeHelper.GetParent(vparent);
        if (!(vgparent is Grid hostGrid)) {
          throw new NotSupportedException("TestFrameworkElement must be placed directly in a Grid.");
        }
        int hostGridRow = Grid.GetColumn(testFrameworkElement);
        int hostGirdColumn = Grid.GetRow(testFrameworkElement);
        this.testFrameworkElement = testFrameworkElement;
        testControl = (Control)testFrameworkElement;
        this.hostGrid = hostGrid;
        this.hostGridRow = hostGridRow;
        this.hostGirdColumn = hostGirdColumn;

        setupTextBoxes();
        setupAlignment();

        setupLeftMarginLine();
        setupRightMarginLine();
        setupTopMarginLine();
        setupBottomMarginLine();

        //bind width and height properties to TestFrameworkElement
        WpfBinding.Setup(testFrameworkElement, "Height", HeightTextBox, TextBox.TextProperty, BindingMode.TwoWay, new DoubleNanConverter());
        WpfBinding.Setup(testFrameworkElement, "MinHeight", MinHeightTextBox, TextBox.TextProperty, BindingMode.TwoWay, new DoublePositiveConverter());
        WpfBinding.Setup(testFrameworkElement, "MaxHeight", MaxHeightTextBox, TextBox.TextProperty, BindingMode.TwoWay, new DoublePositiveConverter());
        WpfBinding.Setup(testFrameworkElement, "ActualHeight", ActualHeightTextBox, TextBox.TextProperty, BindingMode.OneWay, null, ".0");
        WpfBinding.Setup(testFrameworkElement, "Width", WidthTextBox, TextBox.TextProperty, BindingMode.TwoWay, new DoubleNanConverter());
        WpfBinding.Setup(testFrameworkElement, "MinWidth", MinWidthTextBox, TextBox.TextProperty, BindingMode.TwoWay, new DoublePositiveConverter());
        WpfBinding.Setup(testFrameworkElement, "MaxWidth", MaxWidthTextBox, TextBox.TextProperty, BindingMode.TwoWay, new DoublePositiveConverter());
        WpfBinding.Setup(testFrameworkElement, "ActualWidth", ActualWidthTextBox, TextBox.TextProperty, BindingMode.OneWay, null, ".0");

        if (testControl==null) {
          //TestFrameworkElement does not support Fonts and Padding. Hide them
          ContentColumn.MaxWidth = 0;
          PaddingLeftColumn.MaxWidth = 0;
          PaddingRightColumn.MaxWidth = 0;
          ColorEmptyColumn.MaxWidth = 0;
          ColorColumn.MaxWidth = 0;
          FontColumn.MaxWidth = 0;
          FontSizeColumn.MaxWidth = 0;
          FontWeightColumn.MaxWidth = 0;
          TemplateButton.IsEnabled = false;
          //TextBlock textBlock = new TextBlock();
          //textBlock.Text =  "Template can only be displayed for FrameworkElement inheriting from Control";
          //TemplateButton.ToolTip = textBlock;
        } else {
          //TestFrameworkElement does support Fonts and Padding. Show them
          setupLeftPaddingLine();
          setupRightPaddingLine();
          setupTopPaddingLine();
          setupBottomPaddingLine();
          setupPaddingBorderBindings();
          setupFontComboBoxes();
        }

        setupMarginBindings();
        testFrameworkElement.LayoutUpdated += new EventHandler(testFrameworkElement_LayoutUpdated);

      } catch (Exception ex) {
        TracerLib.Tracer.Exception(ex, "");

        throw;
      }
    }


    const double strokeThickness = 4; //thickness of lines displayed to the user, who can change their position with the mouse.


    void testFrameworkElement_LayoutUpdated(object? sender, EventArgs e) {
      DesiredHeightTextBox.Text = testFrameworkElement!.DesiredSize.Height.ToString(".0");
      DesiredWidthTextBox.Text = testFrameworkElement.DesiredSize.Width.ToString(".0");
      RenderHeightTextBox.Text = testFrameworkElement.RenderSize.Height.ToString(".0");
      RenderWidthTextBox.Text = testFrameworkElement.RenderSize.Width.ToString(".0");
    }


    #region TextBox
    //      -------

    private void setupTextBoxes() {
      //adds behavior that when user clicks on TextBox, all text gets selected first
      setupTextBox(HeightTextBox);
      setupTextBox(MinHeightTextBox);
      setupTextBox(MaxHeightTextBox);
      setupTextBox(WidthTextBox);
      setupTextBox(MinWidthTextBox);
      setupTextBox(MaxWidthTextBox);
      setupTextBox(MarginLeftTextBox);
      setupTextBox(MarginTopTextBox);
      setupTextBox(MarginRightTextBox);
      setupTextBox(MarginBottomTextBox);
      if (testControl!=null) {
        setupTextBox(PaddingLeftTextBox);
        setupTextBox(PaddingTopTextBox);
        setupTextBox(PaddingRightTextBox);
        setupTextBox(PaddingBottomTextBox);
        setupTextBox(BorderThicknessTextBox);
        BorderThicknessTextBox.ToolTip = new TextBlock{Text = "BorderThickness takes 4 values, but for simplycity only 1 is supported here."}; 
      }
    }


    /// <summary>
    /// When a user clicks on a textbox, all content gets selected
    /// </summary>
    private void setupTextBox(TextBox textBox) {
      textBox.PreviewMouseDown += new MouseButtonEventHandler(textBox_PreviewMouseDown);
      textBox.GotFocus += new RoutedEventHandler(textBox_GotFocus);
      textBox.SelectionChanged += new RoutedEventHandler(textBox_SelectionChanged);
    }


    bool isMouseDown;


    void textBox_PreviewMouseDown(object sender, MouseButtonEventArgs e) {
      isMouseDown = true;
    }


    bool hasGotFocused;


    void textBox_GotFocus(object sender, RoutedEventArgs e) {
      if (isMouseDown) {
        //user clicked with mouse on TextBox. Wait for the SelectionChanged event to select all the text
        isMouseDown = false;
        hasGotFocused = true;
      } else {
        //user used Tab key, which does not change the selection and the SelectionChanged event will not get fired.
        TextBox textBox = (TextBox)sender;
        textBox.SelectAll();
      }
    }

    void textBox_SelectionChanged(object sender, RoutedEventArgs e) {
      if (hasGotFocused) {
        hasGotFocused = false;
        TextBox textBox = (TextBox)sender;
        textBox.SelectAll();
      }        
    }
    #endregion


    #region Alignment Setup
    //      ---------------

    private void setupAlignment() {
      HorizontalAlignmentComboBox.Items.Add(new ComboBoxItem {Content=((HorizontalAlignment)0).ToString()});
      HorizontalAlignmentComboBox.Items.Add(new ComboBoxItem {Content=((HorizontalAlignment)1).ToString()});
      HorizontalAlignmentComboBox.Items.Add(new ComboBoxItem {Content=((HorizontalAlignment)2).ToString()});
      HorizontalAlignmentComboBox.Items.Add(new ComboBoxItem {Content=((HorizontalAlignment)3).ToString()});
      HorizontalAlignmentComboBox.SelectedIndex = (int)testFrameworkElement!.HorizontalAlignment;

      DependencyPropertyDescriptor horizontalDescriptor = DependencyPropertyDescriptor.FromProperty(FrameworkElement.HorizontalAlignmentProperty, typeof(FrameworkElement));
      horizontalDescriptor.AddValueChanged(testFrameworkElement, onHorizontalAlignment);

      HorizontalAlignmentComboBox.SelectionChanged += new SelectionChangedEventHandler(horizontalAlignmentComboBox_SelectionChanged);

      VerticalAlignmentComboBox.Items.Add(new ComboBoxItem {Content=((VerticalAlignment)0).ToString()});
      VerticalAlignmentComboBox.Items.Add(new ComboBoxItem {Content=((VerticalAlignment)1).ToString()});
      VerticalAlignmentComboBox.Items.Add(new ComboBoxItem {Content=((VerticalAlignment)2).ToString()});
      VerticalAlignmentComboBox.Items.Add(new ComboBoxItem {Content=((VerticalAlignment)3).ToString()});
      VerticalAlignmentComboBox.SelectedIndex = (int)testFrameworkElement.VerticalAlignment;

      DependencyPropertyDescriptor verticalDescriptor = DependencyPropertyDescriptor.FromProperty(FrameworkElement.VerticalAlignmentProperty, typeof(FrameworkElement));
      verticalDescriptor.AddValueChanged(testFrameworkElement, onVerticalAlignment);

      VerticalAlignmentComboBox.SelectionChanged += new SelectionChangedEventHandler(verticalAlignmentComboBox_SelectionChanged);

      if (testControl!=null) {
        HorizontalContentAlignmentComboBox.Items.Add(new ComboBoxItem { Content=((HorizontalAlignment)0).ToString() });
        HorizontalContentAlignmentComboBox.Items.Add(new ComboBoxItem { Content=((HorizontalAlignment)1).ToString() });
        HorizontalContentAlignmentComboBox.Items.Add(new ComboBoxItem { Content=((HorizontalAlignment)2).ToString() });
        HorizontalContentAlignmentComboBox.Items.Add(new ComboBoxItem { Content=((HorizontalAlignment)3).ToString() });
        HorizontalContentAlignmentComboBox.SelectedIndex = (int)testControl.HorizontalContentAlignment;

        DependencyPropertyDescriptor horizontalContentDescriptor = DependencyPropertyDescriptor.FromProperty(Control.HorizontalContentAlignmentProperty, typeof(Control));
        horizontalContentDescriptor.AddValueChanged(testFrameworkElement, onHorizontalContentAlignment);

        HorizontalContentAlignmentComboBox.SelectionChanged += new SelectionChangedEventHandler(horizontalContentAlignmentComboBox_SelectionChanged);

        VerticalContentAlignmentComboBox.Items.Add(new ComboBoxItem { Content=((VerticalAlignment)0).ToString() });
        VerticalContentAlignmentComboBox.Items.Add(new ComboBoxItem { Content=((VerticalAlignment)1).ToString() });
        VerticalContentAlignmentComboBox.Items.Add(new ComboBoxItem { Content=((VerticalAlignment)2).ToString() });
        VerticalContentAlignmentComboBox.Items.Add(new ComboBoxItem { Content=((VerticalAlignment)3).ToString() });
        VerticalContentAlignmentComboBox.SelectedIndex = (int)testControl.VerticalContentAlignment;

        DependencyPropertyDescriptor verticalContentDescriptor = DependencyPropertyDescriptor.FromProperty(Control.VerticalContentAlignmentProperty, typeof(Control));
        verticalContentDescriptor.AddValueChanged(testFrameworkElement, onVerticalContentAlignment);

        VerticalContentAlignmentComboBox.SelectionChanged += new SelectionChangedEventHandler(verticalContentAlignmentComboBox_SelectionChanged);
      }
    }


    void onHorizontalAlignment(object? sender, EventArgs args) {
      FrameworkElement frameworkElement = (FrameworkElement)sender!;
      HorizontalAlignmentComboBox.SelectedIndex = (int)frameworkElement.HorizontalAlignment;
    }


    void horizontalAlignmentComboBox_SelectionChanged(object? sender, SelectionChangedEventArgs e) {
      testFrameworkElement!.HorizontalAlignment = (HorizontalAlignment)HorizontalAlignmentComboBox.SelectedIndex;
    }


    void onVerticalAlignment(object? sender, EventArgs args) {
      FrameworkElement frameworkElement = (FrameworkElement)sender!;
      VerticalAlignmentComboBox.SelectedIndex = (int)frameworkElement.VerticalAlignment;
    }


    void verticalAlignmentComboBox_SelectionChanged(object? sender, SelectionChangedEventArgs e) {
      testFrameworkElement!.VerticalAlignment = (VerticalAlignment)VerticalAlignmentComboBox.SelectedIndex;
    }


    void onHorizontalContentAlignment(object? sender, EventArgs args) {
      Control control = (Control)sender!;
      HorizontalContentAlignmentComboBox.SelectedIndex = (int)control.HorizontalContentAlignment;
    }


    void horizontalContentAlignmentComboBox_SelectionChanged(object? sender, SelectionChangedEventArgs e) {
      testControl!.HorizontalContentAlignment = (HorizontalAlignment)HorizontalContentAlignmentComboBox.SelectedIndex;
    }


    void onVerticalContentAlignment(object? sender, EventArgs args) {
      var control = (Control)sender!;
      VerticalContentAlignmentComboBox.SelectedIndex = (int)control.VerticalContentAlignment;
    }


    void verticalContentAlignmentComboBox_SelectionChanged(object? sender, SelectionChangedEventArgs e) {
      testControl!.VerticalContentAlignment = (VerticalAlignment)VerticalContentAlignmentComboBox.SelectedIndex;
    }
    #endregion


    #region Margin Setup
    //      ------------

    private void setupMarginBindings() {
      DependencyPropertyDescriptor descriptor = DependencyPropertyDescriptor.FromProperty( FrameworkElement.MarginProperty, typeof(FrameworkElement));
      descriptor.AddValueChanged(testFrameworkElement, onMarginChanged);
      onMarginChanged(testFrameworkElement, null);

      MarginLeftTextBox.LostFocus += marginLeftTextBox_LostFocus;
      MarginTopTextBox.LostFocus += marginTopTextBox_LostFocus;
      MarginRightTextBox.LostFocus += marginRightTextBox_LostFocus;
      MarginBottomTextBox.LostFocus += marginBottomTextBox_LostFocus;
    }


    void onMarginChanged(object? sender, EventArgs? args) {
      FrameworkElement frameworkElement = (FrameworkElement)sender!;
      MarginLeftTextBox.Text = frameworkElement.Margin.Left.ToString();
      MarginTopTextBox.Text = frameworkElement.Margin.Top.ToString();
      MarginRightTextBox.Text = frameworkElement.Margin.Right.ToString();
      MarginBottomTextBox.Text = frameworkElement.Margin.Bottom.ToString();

      leftMarginLine!.Margin = new Thickness(testFrameworkElement!.Margin.Left, 0, 0, 0);
      topMarginLine!.Margin = new Thickness(0, testFrameworkElement.Margin.Top, 0, 0);
      rightMarginLine!.Margin =  new Thickness(0, 0, testFrameworkElement.Margin.Right, 0);
      bottomMarginLine!.Margin =  new Thickness(0, 0, 0, testFrameworkElement.Margin.Bottom);

      if (testControl!=null) {
        leftPaddingLine!.Margin = new Thickness(testControl.Margin.Left + testControl.BorderThickness.Left + testControl.Padding.Left, 0, 0, 0);
        topPaddingLine!.Margin = new Thickness(0, testControl.Margin.Top + testControl.BorderThickness.Top + testControl.Padding.Top, 0, 0);
        rightPaddingLine!.Margin = new Thickness(0, 0, testControl.Margin.Right + testControl.BorderThickness.Right + testControl.Padding.Right, 0);
        bottomPaddingLine!.Margin = new Thickness(0, 0, 0, testControl.Margin.Bottom + testControl.BorderThickness.Bottom + testControl.Padding.Bottom);
      }
    }


    void marginLeftTextBox_LostFocus(object sender, RoutedEventArgs e) {
      if (int.TryParse(MarginLeftTextBox.Text, out var newMargin)) {
        testFrameworkElement!.Margin = new Thickness(newMargin, testFrameworkElement.Margin.Top, testFrameworkElement.Margin.Right, testFrameworkElement.Margin.Bottom);
      } else {
        testFrameworkElement!.Margin = new Thickness(0, testFrameworkElement.Margin.Top, testFrameworkElement.Margin.Right, testFrameworkElement.Margin.Bottom);
      }
    }


    void marginTopTextBox_LostFocus(object sender, RoutedEventArgs e) {
      if (int.TryParse(MarginTopTextBox.Text, out var newMargin)) {
        testFrameworkElement!.Margin = new Thickness(testFrameworkElement.Margin.Left, newMargin, testFrameworkElement.Margin.Right, testFrameworkElement.Margin.Bottom);
      } else {
        testFrameworkElement!.Margin = new Thickness(testFrameworkElement.Margin.Left, 0, testFrameworkElement.Margin.Right, testFrameworkElement.Margin.Bottom);
      }
    }


    void marginRightTextBox_LostFocus(object sender, RoutedEventArgs e) {
      if (int.TryParse(MarginRightTextBox.Text, out var newMargin)) {
        testFrameworkElement!.Margin = new Thickness(testFrameworkElement.Margin.Left, testFrameworkElement.Margin.Top, newMargin, testFrameworkElement.Margin.Bottom);
      } else {
        testFrameworkElement!.Margin = new Thickness(testFrameworkElement.Margin.Left, testFrameworkElement.Margin.Top, 0, testFrameworkElement.Margin.Bottom);
      }
    }


    void marginBottomTextBox_LostFocus(object sender, RoutedEventArgs e) {
      if (int.TryParse(MarginBottomTextBox.Text, out var newMargin)) {
        testFrameworkElement!.Margin = new Thickness(testFrameworkElement.Margin.Left, testFrameworkElement.Margin.Top, testFrameworkElement.Margin.Right, newMargin);
      } else {
        testFrameworkElement!.Margin = new Thickness(testFrameworkElement.Margin.Left, testFrameworkElement.Margin.Top, testFrameworkElement.Margin.Right, 0);
      }
    }


    #region Left Margin Line
    //      ................

    Line? leftMarginLine;
    

    private void setupLeftMarginLine() {
      leftMarginLine = new Line();
      GradientStopCollection leftMarginGradientStopCollection = new GradientStopCollection(4) {
        new GradientStop(Colors.Blue, 0),
        new GradientStop(Colors.Blue, 0.5),
        new GradientStop(Colors.Transparent, .5),
        new GradientStop(Colors.Transparent, 1)
      };
      LinearGradientBrush leftMarginBrush = new LinearGradientBrush(leftMarginGradientStopCollection, 0) {
        Opacity = 0.4
      };
      leftMarginBrush.Freeze();
      leftMarginLine.Stroke = leftMarginBrush;

      leftMarginLine.StrokeThickness = strokeThickness;
      leftMarginLine.StrokeDashArray = new DoubleCollection {4, 8};

      leftMarginLine.HorizontalAlignment = HorizontalAlignment.Left;
      leftMarginLine.Margin = new Thickness(testFrameworkElement!.Margin.Left, 0, 0, 0);
      leftMarginLine.X1 = strokeThickness/2;
      leftMarginLine.Y1 = 0;
      leftMarginLine.X2 = leftMarginLine.X1;
      leftMarginLine.Y2 = SystemParameters.VirtualScreenHeight;//VerticalAlign=stretch doesn't work
      leftMarginLine.VerticalAlignment = VerticalAlignment.Stretch;
      leftMarginLine.Cursor =Cursors.SizeWE;
      hostGrid!.Children.Add(leftMarginLine);
      Grid.SetRow(leftMarginLine, hostGridRow!.Value);
      Grid.SetColumn(leftMarginLine, hostGirdColumn!.Value);
      leftMarginLine.MouseDown += leftMarginLine_MouseDown;
      leftMarginLine.MouseMove += leftMarginLine_MouseMove;
      leftMarginLine.MouseUp += leftMarginLine_MouseUp;
    }


    void leftMarginLine_MouseDown(object sender, MouseButtonEventArgs e) {
      startMouseCapture(sender, e, testFrameworkElement!.Margin.Left);
    }


    Point startMousePosition;
    double startValue;


    private void startMouseCapture(object sender, MouseButtonEventArgs e, double startValue) {
      e.MouseDevice.Capture((FrameworkElement)sender, CaptureMode.Element);
      startMousePosition = e.MouseDevice.GetPosition(this);
      this.startValue = startValue;
    }


    void leftMarginLine_MouseMove(object sender, MouseEventArgs e) {
      if (getValueBasedOnMouseXMovement(sender, e, out var newLeftMargin)) {

        if (newLeftMargin<0) {
          //prevent negative margins created with the mouse.
          newLeftMargin = 0;
        }
        leftMarginLine!.Margin = new Thickness(newLeftMargin, 0, 0, 0); ;
        testFrameworkElement!.Margin = new Thickness(newLeftMargin, testFrameworkElement.Margin.Top, testFrameworkElement.Margin.Right, testFrameworkElement.Margin.Bottom);
      }
    }


    private bool getValueBasedOnMouseXMovement(object sender, MouseEventArgs e, out double newValue) {
      if (e.MouseDevice.Captured!=sender) {
        newValue = double.NaN;
        return false;
      }

      Point newMouseDownPosition = e.MouseDevice.GetPosition(this);
      newValue = startValue + newMouseDownPosition.X - startMousePosition.X;
      return true;
    }


    private bool getValueBasedOnMouseXMinusMovement(object sender, MouseEventArgs e, out double newValue) {
      if (e.MouseDevice.Captured!=sender) {
        newValue = double.NaN;
        return false;
      }

      Point newMouseDownPosition = e.MouseDevice.GetPosition(this);
      newValue = startValue - newMouseDownPosition.X + startMousePosition.X;
      return true;
    }


    private bool getValueBasedOnMouseYMovement(object sender, MouseEventArgs e, out double newValue) {
      if (e.MouseDevice.Captured!=sender) {
        newValue = double.NaN;
        return false;
      }

      Point newMouseDownPosition = e.MouseDevice.GetPosition(this);
      newValue = startValue + newMouseDownPosition.Y - startMousePosition.Y;
      return true;
    }


    private bool getValueBasedOnMouseYMinusMovement(object sender, MouseEventArgs e, out double newValue) {
      if (e.MouseDevice.Captured!=sender) {
        newValue = double.NaN;
        return false;
      }

      Point newMouseDownPosition = e.MouseDevice.GetPosition(this);
      newValue = startValue - newMouseDownPosition.Y + startMousePosition.Y;
      return true;
    }


    void leftMarginLine_MouseUp(object sender, MouseButtonEventArgs e) {
      e.MouseDevice.Capture((IInputElement)sender, CaptureMode.None);
    }
    #endregion


    #region Right Margin Line
    //      .................

    Line? rightMarginLine;


    private void setupRightMarginLine() {
      rightMarginLine = new Line();
      GradientStopCollection rightMarginGradientStopCollection = new GradientStopCollection(4) {
        new GradientStop(Colors.Transparent, 0),
        new GradientStop(Colors.Transparent, 0.5),
        new GradientStop(Colors.Green, .5),
        new GradientStop(Colors.Green, 1)
      };
      LinearGradientBrush rightMarginBrush = new LinearGradientBrush(rightMarginGradientStopCollection, 0) {
        Opacity = 0.4
      };
      rightMarginBrush.Freeze();
      rightMarginLine.Stroke = rightMarginBrush;

      rightMarginLine.StrokeThickness = strokeThickness;
      rightMarginLine.StrokeDashArray = new DoubleCollection {4, 8};

      rightMarginLine.HorizontalAlignment = HorizontalAlignment.Right;
      rightMarginLine.Margin = new Thickness(0,0,testFrameworkElement!.Margin.Left, 0); ;
      rightMarginLine.X1 = strokeThickness/2;
      rightMarginLine.Y1 = 0;
      rightMarginLine.X2 = rightMarginLine.X1;
      rightMarginLine.Y2 = SystemParameters.VirtualScreenHeight;//VerticalAlign=stretch doesn't work;
      rightMarginLine.VerticalAlignment = VerticalAlignment.Stretch;
      rightMarginLine.Cursor =Cursors.SizeWE;
      hostGrid!.Children.Add(rightMarginLine);
      Grid.SetRow(leftMarginLine, hostGridRow!.Value);
      Grid.SetColumn(leftMarginLine, hostGirdColumn!.Value);
      rightMarginLine.MouseDown += rightMarginLine_MouseDown;
      rightMarginLine.MouseMove += rightMarginLine_MouseMove;
      rightMarginLine.MouseUp += rightMarginLine_MouseUp;
    }


    void rightMarginLine_MouseDown(object sender, MouseButtonEventArgs e) {
      startMouseCapture(sender, e, testFrameworkElement!.Margin.Right);
    }


    void rightMarginLine_MouseMove(object sender, MouseEventArgs e) {
      if (getValueBasedOnMouseXMinusMovement(sender, e, out var newRightMargin)) {

        if (newRightMargin<0) {
          //prevent negative margins created with the mouse.
          newRightMargin = 0;
        }
        rightMarginLine!.Margin = new Thickness(0, 0, newRightMargin, 0);
        testFrameworkElement!.Margin = new Thickness(testFrameworkElement.Margin.Left, testFrameworkElement.Margin.Top, newRightMargin, testFrameworkElement.Margin.Bottom);
      }
    }


    void rightMarginLine_MouseUp(object sender, MouseButtonEventArgs e) {
      e.MouseDevice.Capture(rightMarginLine, CaptureMode.None);
    }
    #endregion


    #region Top Margin Line
    //      ...............

    Line? topMarginLine;
    

    private void setupTopMarginLine() {
      topMarginLine = new Line();
      GradientStopCollection topMarginGradientStopCollection = new GradientStopCollection(4) {
        new GradientStop(Colors.Blue, 0),
        new GradientStop(Colors.Blue, 0.5),
        new GradientStop(Colors.Transparent, .5),
        new GradientStop(Colors.Transparent, 1)
      };
      LinearGradientBrush topMarginBrush = new LinearGradientBrush(topMarginGradientStopCollection, 90) {
        Opacity = 0.4
      };
      topMarginBrush.Freeze();
      topMarginLine.Stroke = topMarginBrush;

      topMarginLine.StrokeThickness = strokeThickness;
      topMarginLine.StrokeDashArray = new DoubleCollection {4, 8};

      topMarginLine.VerticalAlignment= VerticalAlignment.Top;
      topMarginLine.Margin = new Thickness(0, testFrameworkElement!.Margin.Top, 0, 0); ;
      topMarginLine.X1 = 0;
      topMarginLine.Y1 = strokeThickness/2;
      topMarginLine.X2 = SystemParameters.VirtualScreenWidth;//HorizontalAlign=stretch doesn't work
      topMarginLine.Y2 = topMarginLine.Y1;
      topMarginLine.HorizontalAlignment = HorizontalAlignment.Stretch;
      topMarginLine.Cursor =Cursors.SizeNS;
      hostGrid!.Children.Add(topMarginLine);
      Grid.SetRow(leftMarginLine, hostGridRow!.Value);
      Grid.SetColumn(leftMarginLine, hostGirdColumn!.Value);
      topMarginLine.MouseDown += topMarginLine_MouseDown;
      topMarginLine.MouseMove += topMarginLine_MouseMove;
      topMarginLine.MouseUp += topMarginLine_MouseUp;
    }


    void topMarginLine_MouseDown(object sender, MouseButtonEventArgs e) {
      startMouseCapture(sender, e, testFrameworkElement!.Margin.Top);
    }


    void topMarginLine_MouseMove(object sender, MouseEventArgs e) {
      if (getValueBasedOnMouseYMovement(sender, e, out var newTopMargin)) {

        if (newTopMargin<0) {
          //prevent negative margins created with the mouse.
          newTopMargin = 0;
        }
        topMarginLine!.Margin = new Thickness(0, newTopMargin, 0, 0); ;
        testFrameworkElement!.Margin = new Thickness(testFrameworkElement.Margin.Left, newTopMargin, testFrameworkElement.Margin.Right, testFrameworkElement.Margin.Bottom);
      }
    }


    void topMarginLine_MouseUp(object sender, MouseButtonEventArgs e) {
      e.MouseDevice.Capture((IInputElement)sender, CaptureMode.None);
    }
    #endregion


    #region Bottom Margin Line
    //      ..................

    Line? bottomMarginLine;


    private void setupBottomMarginLine() {
      bottomMarginLine = new Line();
      GradientStopCollection bottomMarginGradientStopCollection = new GradientStopCollection(4) {
        new GradientStop(Colors.Transparent, 0),
        new GradientStop(Colors.Transparent, 0.5),
        new GradientStop(Colors.Green, .5),
        new GradientStop(Colors.Green, 1)
      };
      LinearGradientBrush bottomMarginBrush = new LinearGradientBrush(bottomMarginGradientStopCollection, 90) {
        Opacity = 0.4
      };
      bottomMarginBrush.Freeze();
      bottomMarginLine.Stroke = bottomMarginBrush;

      bottomMarginLine.StrokeThickness = strokeThickness;
      bottomMarginLine.StrokeDashArray = new DoubleCollection {4, 8};

      bottomMarginLine.VerticalAlignment= VerticalAlignment.Bottom;
      bottomMarginLine.Margin = new Thickness(0, 0, 0, testFrameworkElement!.Margin.Bottom);
      bottomMarginLine.X1 = 0;
      bottomMarginLine.Y1 = strokeThickness/2;
      bottomMarginLine.X2 = SystemParameters.VirtualScreenWidth;//HorizontalAlign=stretch doesn't work;
      bottomMarginLine.Y2 = bottomMarginLine.Y1;
      bottomMarginLine.HorizontalAlignment = HorizontalAlignment.Stretch;
      bottomMarginLine.Cursor =Cursors.SizeNS;
      hostGrid!.Children.Add(bottomMarginLine);
      Grid.SetRow(leftMarginLine, hostGridRow!.Value);
      Grid.SetColumn(leftMarginLine, hostGirdColumn!.Value);
      bottomMarginLine.MouseDown += bottomMarginLine_MouseDown;
      bottomMarginLine.MouseMove += bottomMarginLine_MouseMove;
      bottomMarginLine.MouseUp += bottomMarginLine_MouseUp;
    }


    void bottomMarginLine_MouseDown(object sender, MouseButtonEventArgs e) {
      startMouseCapture(sender, e, testFrameworkElement!.Margin.Bottom);
    }


    void bottomMarginLine_MouseMove(object sender, MouseEventArgs e) {
      if (getValueBasedOnMouseYMinusMovement(sender, e, out var newBottomMargin)) {

        if (newBottomMargin<0) {
          //prevent negative margins created with the mouse.
          newBottomMargin = 0;
        }
        bottomMarginLine!.Margin = new Thickness(0, 0, 0, newBottomMargin);
        testFrameworkElement!.Margin = new Thickness(testFrameworkElement.Margin.Left, testFrameworkElement.Margin.Top, testFrameworkElement.Margin.Right, newBottomMargin);
      }
    }


    void bottomMarginLine_MouseUp(object sender, MouseButtonEventArgs e) {
      e.MouseDevice.Capture(bottomMarginLine, CaptureMode.None);
    }
    #endregion
    #endregion


    #region Padding Setup
    //      -------------

    private void setupPaddingBorderBindings() {
      if (testControl==null) return;//TestFrameworkElement does not support padding

      DependencyPropertyDescriptor paddingDescriptor = DependencyPropertyDescriptor.FromProperty( Control.PaddingProperty, typeof(FrameworkElement));
      paddingDescriptor.AddValueChanged(testFrameworkElement, onPaddingChanged);
      onPaddingChanged(testFrameworkElement, null);

      PaddingLeftTextBox.LostFocus += paddingLeftTextBox_LostFocus;
      PaddingTopTextBox.LostFocus += paddingTopTextBox_LostFocus;
      PaddingRightTextBox.LostFocus += paddingRightTextBox_LostFocus;
      PaddingBottomTextBox.LostFocus += paddingBottomTextBox_LostFocus;

      DependencyPropertyDescriptor borderThicknessDescriptor = DependencyPropertyDescriptor.FromProperty( Control.BorderThicknessProperty, typeof(Control));
      borderThicknessDescriptor.AddValueChanged(testFrameworkElement, onBorderThicknessChanged);
      onBorderThicknessChanged(testFrameworkElement, null);
      BorderThicknessTextBox.LostFocus += borderThicknessTextBox_LostFocus;
    }


    void onPaddingChanged(object? sender, EventArgs? args) {
      PaddingLeftTextBox.Text = testControl!.Padding.Left.ToString();
      PaddingTopTextBox.Text = testControl.Padding.Top.ToString();
      PaddingRightTextBox.Text = testControl.Padding.Right.ToString();
      PaddingBottomTextBox.Text = testControl.Padding.Bottom.ToString();

      leftPaddingLine!.Margin = new Thickness(testControl.Margin.Left + testControl.BorderThickness.Left + testControl.Padding.Left, 0, 0, 0);
      topPaddingLine!.Margin = new Thickness(0, testControl.Margin.Top + testControl.BorderThickness.Top + testControl.Padding.Top, 0, 0);
      rightPaddingLine!.Margin = new Thickness(0, 0, testControl.Margin.Right + testControl.BorderThickness.Right + testControl.Padding.Right, 0);
      bottomPaddingLine!.Margin = new Thickness(0, 0, 0, testControl.Margin.Bottom + testControl.BorderThickness.Bottom + testControl.Padding.Bottom);
    }


    void paddingLeftTextBox_LostFocus(object sender, RoutedEventArgs e) {
      if (int.TryParse(PaddingLeftTextBox.Text, out var newPadding)) {
        testControl!.Padding = new Thickness(newPadding, testControl.Padding.Top, testControl.Padding.Right, testControl.Padding.Bottom);
      } else {
        testControl!.Padding = new Thickness(0, testControl.Padding.Top, testControl.Padding.Right, testControl.Padding.Bottom);
      }
    }


    void paddingTopTextBox_LostFocus(object sender, RoutedEventArgs e) {
      if (int.TryParse(PaddingTopTextBox.Text, out var newPadding)) {
        testControl!.Padding = new Thickness(testControl.Padding.Left, newPadding, testControl.Padding.Right, testControl.Padding.Bottom);
      } else {
        testControl!.Padding = new Thickness(testControl.Padding.Left, 0, testControl.Padding.Right, testControl.Padding.Bottom);
      }
    }


    void paddingRightTextBox_LostFocus(object sender, RoutedEventArgs e) {
      if (int.TryParse(PaddingRightTextBox.Text, out var newPadding)) {
        testControl!.Padding = new Thickness(testControl.Padding.Left, testControl.Padding.Top, newPadding, testControl.Padding.Bottom);
      } else {
        testControl!.Padding = new Thickness(testControl.Padding.Left, testControl.Padding.Top, 0, testControl.Padding.Bottom);
      }
    }


    void paddingBottomTextBox_LostFocus(object sender, RoutedEventArgs e) {
      if (int.TryParse(PaddingBottomTextBox.Text, out var newPadding)) {
        testControl!.Padding = new Thickness(testControl.Padding.Left, testControl.Padding.Top, testControl.Padding.Right, newPadding);
      } else {
        testControl!.Padding = new Thickness(testControl.Padding.Left, testControl.Padding.Top, testControl.Padding.Right, 0);
      }
    }


    void onBorderThicknessChanged(object? sender, EventArgs? args) {
      //too lazy to support 4 different border thicknesses. Use just the left one
      BorderThicknessTextBox.Text = testControl!.BorderThickness.Left.ToString();

      leftPaddingLine!.Margin = new Thickness(testControl.Margin.Left + testControl.BorderThickness.Left + testControl.Padding.Left, 0, 0, 0);
      topPaddingLine!.Margin = new Thickness(0, testControl.Margin.Top + testControl.BorderThickness.Top + testControl.Padding.Top, 0, 0);
      rightPaddingLine!.Margin = new Thickness(0, 0, testControl.Margin.Right + testControl.BorderThickness.Right + testControl.Padding.Right, 0);
      bottomPaddingLine!.Margin = new Thickness(0, 0, 0, testControl.Margin.Bottom + testControl.BorderThickness.Bottom + testControl.Padding.Bottom);
    }


    void borderThicknessTextBox_LostFocus(object sender, RoutedEventArgs e) {
      if (int.TryParse(BorderThicknessTextBox.Text, out var newBorderThickness)) {
        testControl!.BorderThickness = new Thickness(newBorderThickness);
      } else {
        testControl!.Padding = new Thickness(0);
        BorderThicknessTextBox.Text = "0";
      }
    }


    #region Left Padding Line
    //      ................

    Line? leftPaddingLine;
    

    private void setupLeftPaddingLine() {
      leftPaddingLine = new Line();
      GradientStopCollection leftPaddingGradientStopCollection = new GradientStopCollection(4) {
        new GradientStop(Colors.Black, 0),
        new GradientStop(Colors.Black, 0.5),
        new GradientStop(Colors.Transparent, .5),
        new GradientStop(Colors.Transparent, 1)
      };
      LinearGradientBrush leftPaddingBrush = new LinearGradientBrush(leftPaddingGradientStopCollection, 0) {
        Opacity = 0.4
      };
      leftPaddingBrush.Freeze();
      leftPaddingLine.Stroke = leftPaddingBrush;

      leftPaddingLine.StrokeThickness = strokeThickness;
      leftPaddingLine.StrokeDashArray = new DoubleCollection {0, 4, 4, 4};

      leftPaddingLine.HorizontalAlignment = HorizontalAlignment.Left;
      leftPaddingLine.Margin = new Thickness(testControl!.Margin.Left + testControl.BorderThickness.Left + testControl.Padding.Left,0,0,0);
      leftPaddingLine.X1 = strokeThickness/2;
      leftPaddingLine.Y1 = 0;
      leftPaddingLine.X2 = leftPaddingLine.X1;
      leftPaddingLine.Y2 = SystemParameters.VirtualScreenHeight;//VerticalAlign=stretch doesn't work
      leftPaddingLine.VerticalAlignment = VerticalAlignment.Stretch;
      leftPaddingLine.Cursor = Cursors.SizeWE;
      hostGrid!.Children.Add(leftPaddingLine);
      Grid.SetRow(leftMarginLine, hostGridRow!.Value);
      Grid.SetColumn(leftMarginLine, hostGirdColumn!.Value);
      leftPaddingLine.MouseDown += leftPaddingLine_MouseDown;
      leftPaddingLine.MouseMove += leftPaddingLine_MouseMove;
      leftPaddingLine.MouseUp += leftPaddingLine_MouseUp;
    }


    void leftPaddingLine_MouseDown(object sender, MouseButtonEventArgs e) {
      startMouseCapture(sender, e, testControl!.Padding.Left);
    }


    void leftPaddingLine_MouseMove(object sender, MouseEventArgs e) {
      if (getValueBasedOnMouseXMovement(sender, e, out var newLeftPadding)) {

        if (newLeftPadding<0) {
          //prevent negative margins created with the mouse.
          newLeftPadding = 0;
        }
        leftPaddingLine!.Margin = new Thickness(testControl!.Margin.Left + testControl.BorderThickness.Left + newLeftPadding, 0, 0, 0); ;
        testControl.Padding = new Thickness(newLeftPadding, testControl.Padding.Top, testControl.Padding.Right, testControl.Padding.Bottom);
      }
    }


    void leftPaddingLine_MouseUp(object sender, MouseButtonEventArgs e) {
      e.MouseDevice.Capture(leftPaddingLine, CaptureMode.None);
    }
    #endregion


    #region Right Padding Line
    //      .................

    Line? rightPaddingLine;


    private void setupRightPaddingLine() {
      rightPaddingLine = new Line();
      GradientStopCollection rightPaddingGradientStopCollection = new GradientStopCollection(4) {
        new GradientStop(Colors.Transparent, 0),
        new GradientStop(Colors.Transparent, 0.5),
        new GradientStop(Colors.DarkGreen, .5),
        new GradientStop(Colors.DarkGreen, 1)
      };
      LinearGradientBrush rightPaddingBrush = new LinearGradientBrush(rightPaddingGradientStopCollection, 0) {
        Opacity = 0.4
      };
      rightPaddingBrush.Freeze();
      rightPaddingLine.Stroke = rightPaddingBrush;

      rightPaddingLine.StrokeThickness = strokeThickness;
      rightPaddingLine.StrokeDashArray = new DoubleCollection {0, 4, 4, 4};

      rightPaddingLine.HorizontalAlignment = HorizontalAlignment.Right;
      rightPaddingLine.X1 = strokeThickness/2;
      rightPaddingLine.Y1 = 0;
      rightPaddingLine.X2 = rightPaddingLine.X1;
      rightPaddingLine.Y2 = SystemParameters.VirtualScreenHeight;//VerticalAlign=stretch doesn't work;
      leftPaddingLine!.VerticalAlignment = VerticalAlignment.Stretch;
      rightPaddingLine.Cursor =Cursors.SizeWE;
      hostGrid!.Children.Add(rightPaddingLine);
      Grid.SetRow(leftMarginLine, hostGridRow!.Value);
      Grid.SetColumn(leftMarginLine, hostGirdColumn!.Value);
      rightPaddingLine.MouseDown += rightPaddingLine_MouseDown;
      rightPaddingLine.MouseMove += rightPaddingLine_MouseMove;
      rightPaddingLine.MouseUp += rightPaddingLine_MouseUp;
    }


    void rightPaddingLine_MouseDown(object sender, MouseButtonEventArgs e) {
      startMouseCapture(sender, e, testControl!.Padding.Right);
    }


    void rightPaddingLine_MouseMove(object sender, MouseEventArgs e) {
      if (getValueBasedOnMouseXMinusMovement(sender, e, out var newRightPadding)) {

        if (newRightPadding<0) {
          //prevent negative margins created with the mouse.
          newRightPadding = 0;
        }
        rightPaddingLine!.Margin = new Thickness(0, 0, testControl!.Margin.Right + testControl.BorderThickness.Right + newRightPadding, 0); ;
        testControl.Padding = new Thickness(testControl.Padding.Left, testControl.Padding.Top, newRightPadding, testControl.Padding.Bottom);
      }
    }


    void rightPaddingLine_MouseUp(object sender, MouseButtonEventArgs e) {
      e.MouseDevice.Capture(rightPaddingLine, CaptureMode.None);
    }
    #endregion


    #region Top Padding Line
    //      ...............

    Line? topPaddingLine;
    

    private void setupTopPaddingLine() {
      topPaddingLine = new Line();
      GradientStopCollection topPaddingGradientStopCollection = new GradientStopCollection(4) {
        new GradientStop(Colors.Black, 0),
        new GradientStop(Colors.Black, 0.5),
        new GradientStop(Colors.Transparent, .5),
        new GradientStop(Colors.Transparent, 1)
      };
      LinearGradientBrush topPaddingBrush = new LinearGradientBrush(topPaddingGradientStopCollection, 90) {
        Opacity = 0.4
      };
      topPaddingBrush.Freeze();
      topPaddingLine.Stroke = topPaddingBrush;

      topPaddingLine.StrokeThickness = strokeThickness;
      topPaddingLine.StrokeDashArray = new DoubleCollection {0, 4, 4, 4};

      topPaddingLine.VerticalAlignment = VerticalAlignment.Top;
      topPaddingLine.Margin = new Thickness(0,testControl!.Margin.Top + testControl.BorderThickness.Top + testControl.Padding.Top,0,0);
      topPaddingLine.X1 = 0;
      topPaddingLine.Y1 = strokeThickness/2;
      topPaddingLine.X2 = SystemParameters.VirtualScreenWidth;//HorizontalAlign=stretch doesn't work
      topPaddingLine.Y2 = topPaddingLine.Y1;
      topPaddingLine.HorizontalAlignment = HorizontalAlignment.Stretch;
      topPaddingLine.Cursor =Cursors.SizeNS;
      hostGrid!.Children.Add(topPaddingLine);
      Grid.SetRow(leftMarginLine, hostGridRow!.Value);
      Grid.SetColumn(leftMarginLine, hostGirdColumn!.Value);
      topPaddingLine.MouseDown += topPaddingLine_MouseDown;
      topPaddingLine.MouseMove += topPaddingLine_MouseMove;
      topPaddingLine.MouseUp += topPaddingLine_MouseUp;
    }


    void topPaddingLine_MouseDown(object sender, MouseButtonEventArgs e) {
      startMouseCapture(sender, e, testControl!.Padding.Top);
    }


    void topPaddingLine_MouseMove(object sender, MouseEventArgs e) {
      if (getValueBasedOnMouseYMovement(sender, e, out var newTopPadding)) {

        if (newTopPadding<0) {
          //prevent negative margins created with the mouse.
          newTopPadding = 0;
        }
        topPaddingLine!.Margin = new Thickness(0, testControl!.Margin.Top + testControl.BorderThickness.Top + newTopPadding, 0, 0); ;
        testControl.Padding = new Thickness(testControl.Padding.Left, newTopPadding, testControl.Padding.Right, testControl.Padding.Bottom);
      }
    }


    void topPaddingLine_MouseUp(object sender, MouseButtonEventArgs e) {
      e.MouseDevice.Capture(topPaddingLine, CaptureMode.None);
    }
    #endregion


    #region Bottom Padding Line
    //      ..................

    Line? bottomPaddingLine;


    private void setupBottomPaddingLine() {
      bottomPaddingLine = new Line();
      GradientStopCollection bottomPaddingGradientStopCollection = new GradientStopCollection(4) {
        new GradientStop(Colors.Transparent, 0),
        new GradientStop(Colors.Transparent, 0.5),
        new GradientStop(Colors.DarkGreen, .5),
        new GradientStop(Colors.DarkGreen, 1)
      };
      LinearGradientBrush bottomPaddingBrush = new LinearGradientBrush(bottomPaddingGradientStopCollection, 90) {
        Opacity = 0.4
      };
      bottomPaddingBrush.Freeze();
      bottomPaddingLine.Stroke = bottomPaddingBrush;

      bottomPaddingLine.StrokeThickness = strokeThickness;
      bottomPaddingLine.StrokeDashArray = new DoubleCollection {0, 4, 4, 4};

      bottomPaddingLine.VerticalAlignment = VerticalAlignment.Bottom;
      bottomPaddingLine.Margin = new Thickness(0, 0, 0, testControl!.Margin.Bottom + testControl.BorderThickness.Bottom + testControl.Padding.Bottom);;
      bottomPaddingLine.X1 = 0;
      bottomPaddingLine.Y1 = strokeThickness/2;
      bottomPaddingLine.X2 = SystemParameters.VirtualScreenWidth;//HorizontalAlign=stretch doesn't work;
      bottomPaddingLine.Y2 = bottomPaddingLine.Y1;
      topPaddingLine!.HorizontalAlignment = HorizontalAlignment.Stretch;
      bottomPaddingLine.Cursor =Cursors.SizeNS;
      hostGrid!.Children.Add(bottomPaddingLine);
      Grid.SetRow(leftMarginLine, hostGridRow!.Value);
      Grid.SetColumn(leftMarginLine, hostGirdColumn!.Value);
      bottomPaddingLine.MouseDown += bottomPaddingLine_MouseDown;
      bottomPaddingLine.MouseMove += bottomPaddingLine_MouseMove;
      bottomPaddingLine.MouseUp += bottomPaddingLine_MouseUp;
    }


    void bottomPaddingLine_MouseDown(object sender, MouseButtonEventArgs e) {
      startMouseCapture(sender, e, testControl!.Padding.Bottom);
    }


    void bottomPaddingLine_MouseMove(object sender, MouseEventArgs e) {
      if (getValueBasedOnMouseYMinusMovement(sender, e, out var newBottomPadding)) {

        if (newBottomPadding<0) {
          //prevent negative margins created with the mouse.
          newBottomPadding = 0;
        }
        bottomPaddingLine!.Margin = new Thickness(0, 0, 0, testControl!.Margin.Bottom + testControl.BorderThickness.Bottom + newBottomPadding); ;
        testControl.Padding = new Thickness(testControl.Padding.Left, testControl.Padding.Top, testControl.Padding.Right, newBottomPadding);
      }
    }


    void bottomPaddingLine_MouseUp(object sender, MouseButtonEventArgs e) {
      e.MouseDevice.Capture(bottomPaddingLine, CaptureMode.None);
    }
    #endregion
    #endregion


    #region Font ComboBoxes
    //      ---------------
                //FontSize ="{Binding ElementName=FontSizeComboBox, Path=Text, Mode=OneWay, UpdateSourceTrigger=LostFocus}"
        //FontStyle ="{Binding ElementName=FontStyleComboBox, Path=Text, Mode=OneWay, UpdateSourceTrigger=LostFocus}"
        //FontStretch ="{Binding ElementName=FontStretchComboBox, Path=Text, Mode=OneWay, UpdateSourceTrigger=LostFocus}"
        //FontWeight ="{Binding ElementName=FontWeightComboBox, Path=Text, Mode=OneWay, UpdateSourceTrigger=LostFocus}"        

    void setupFontComboBoxes() {
      if (testControl==null) return; //TestFrameworkElement does not support fonts

      FontFamilyComboBox.SelectedItem = testControl.FontFamily;
      WpfBinding.Setup(FontFamilyComboBox, "SelectedItem", testFrameworkElement!, Control.FontFamilyProperty, BindingMode.OneWay);

      FontWeightComboBox.ItemsSource = new FontWeight[]{
        FontWeights.Black,
        FontWeights.Bold,
        FontWeights.DemiBold,
        FontWeights.ExtraBlack,
        FontWeights.ExtraBold,
        FontWeights.ExtraLight,
        FontWeights.Heavy,
        FontWeights.Light,
        FontWeights.Medium,
        FontWeights.Normal,
        FontWeights.Regular,
        FontWeights.SemiBold,
        FontWeights.Thin,
        FontWeights.UltraBlack,
        FontWeights.UltraBold,
        FontWeights.UltraLight
      };
      FontWeightComboBox.SelectedItem = testControl.FontWeight;
      WpfBinding.Setup(FontWeightComboBox, "SelectedItem", testFrameworkElement!, Control.FontWeightProperty, BindingMode.OneWay);

      FontSizeComboBox.Items.Add(new ComboBoxItem {Content = "8", HorizontalAlignment = HorizontalAlignment.Right});
      FontSizeComboBox.Items.Add(new ComboBoxItem {Content = "9", HorizontalAlignment = HorizontalAlignment.Right});
      FontSizeComboBox.Items.Add(new ComboBoxItem {Content = "10", HorizontalAlignment = HorizontalAlignment.Right});
      FontSizeComboBox.Items.Add(new ComboBoxItem {Content = "11", HorizontalAlignment = HorizontalAlignment.Right});
      FontSizeComboBox.Items.Add(new ComboBoxItem {Content = "12", HorizontalAlignment = HorizontalAlignment.Right});
      FontSizeComboBox.Items.Add(new ComboBoxItem {Content = "14", HorizontalAlignment = HorizontalAlignment.Right});
      FontSizeComboBox.Items.Add(new ComboBoxItem {Content = "16", HorizontalAlignment = HorizontalAlignment.Right});
      FontSizeComboBox.Items.Add(new ComboBoxItem {Content = "18", HorizontalAlignment = HorizontalAlignment.Right});
      FontSizeComboBox.Items.Add(new ComboBoxItem {Content = "20", HorizontalAlignment = HorizontalAlignment.Right});
      FontSizeComboBox.Items.Add(new ComboBoxItem {Content = "22", HorizontalAlignment = HorizontalAlignment.Right});
      FontSizeComboBox.Items.Add(new ComboBoxItem {Content = "24", HorizontalAlignment = HorizontalAlignment.Right});
      FontSizeComboBox.Items.Add(new ComboBoxItem {Content = "26", HorizontalAlignment = HorizontalAlignment.Right});
      FontSizeComboBox.Items.Add(new ComboBoxItem {Content = "28", HorizontalAlignment = HorizontalAlignment.Right});
      FontSizeComboBox.Items.Add(new ComboBoxItem {Content = "36", HorizontalAlignment = HorizontalAlignment.Right});
      FontSizeComboBox.Items.Add(new ComboBoxItem {Content = "48", HorizontalAlignment = HorizontalAlignment.Right});
      FontSizeComboBox.Items.Add(new ComboBoxItem {Content = "72", HorizontalAlignment = HorizontalAlignment.Right});
      FontSizeComboBox.SelectedIndex = 4;
      string searchIntString = testControl.FontSize.ToString();
      for (int itemIndex = 0; itemIndex < FontSizeComboBox.Items.Count; itemIndex++){
        ComboBoxItem comboBoxItem = (ComboBoxItem)FontSizeComboBox.Items[itemIndex];
        if (((string)comboBoxItem.Content)==searchIntString) {
          FontSizeComboBox.SelectedIndex = itemIndex;
          break;
        }
			}
      WpfBinding.Setup(FontSizeComboBox, "Text", testFrameworkElement!, Control.FontSizeProperty, BindingMode.OneWay);

      FontStretchComboBox.ItemsSource = new FontStretch[]{
        FontStretches.UltraCondensed, 
        FontStretches.ExtraCondensed, 
        FontStretches.Condensed, 
        FontStretches.SemiCondensed, 
        //FontStretches.Medium, same as Normal
        FontStretches.Normal, 
        FontStretches.Expanded, 
        FontStretches.SemiExpanded, 
        FontStretches.ExtraExpanded, 
        FontStretches.UltraExpanded};
      FontStretchComboBox.SelectedItem = testControl.FontStretch;
      WpfBinding.Setup(FontStretchComboBox, "SelectedItem", testFrameworkElement!, Control.FontStretchProperty, BindingMode.OneWay);

      FontStyleComboBox.ItemsSource = new FontStyle[]{
        FontStyles.Normal,
        FontStyles.Italic,
        FontStyles.Oblique
      };
      FontStyleComboBox.SelectedItem = testControl.FontStyle;
      WpfBinding.Setup(FontStyleComboBox, "SelectedItem", testFrameworkElement!, Control.FontStyleProperty, BindingMode.OneWay);

      ForegroundColorComboBox.SetSelectedBrush(testControl.Foreground);
      WpfBinding.Setup(ForegroundColorComboBox, "SelectedColorBrush", testFrameworkElement!, Control.ForegroundProperty, BindingMode.OneWay);
      BackgroundColorComboBox.SetSelectedBrush(testControl.Background);
      WpfBinding.Setup(BackgroundColorComboBox, "SelectedColorBrush", testFrameworkElement!, Control.BackgroundProperty, BindingMode.OneWay);
      BorderColorComboBox.SetSelectedBrush(testControl.BorderBrush);
      WpfBinding.Setup(BorderColorComboBox, "SelectedColorBrush", testFrameworkElement!, Control.BorderBrushProperty, BindingMode.OneWay);
    }
    #endregion


    #region Setup Bindings
    //      --------------

    #endregion
  }
}