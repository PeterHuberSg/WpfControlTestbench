/********************************************************************************************************

WpfTestbench.TestBench
======================

To be placed into a test Window, holds the TestControl and controls to change the TestControl's 
properties. Can be used like this:
    xmlns:TBLib="clr-namespace:WpfTestbench;assembly=WpfControlTestbenchLib"

    <TBLib:TestBench>
      <TBLib:TestBench.TestProperties>
        place a Grid or ... here which holds controls for changing the values of the TestControl
        it will appear at the top of the TestBench
      </TBLib:TestBench.TestProperties>

      <TBLib:TestBench.TestControl>
        Place the control he which you want to test. It appears at the bottom left corner of Testbench.
      </TBLib:TestBench.TestControl>
    </TBLib:TestBench>

    +-----------------------+ 
    |TestProperties         |
    +-----------------------+
    |StandardPropertyViewer |
    +-----------+-----------+ 
    |TestControl|Event Trace|
    +-----------+-----------+


License
-------

To the extent possible under law, the author(s) have dedicated all copyright and related and 
neighboring rights to this software to the public domain worldwide under the Creative Commons 0 license 
(relevant legal text see License CC0.html file, also 
<http://creativecommons.org/publicdomain/zero/1.0/>). 

You might use it freely for any purpose, commercial or non-commercial. It is provided "as-is." The 
author gives no warranty of any kind whatsoever. It is up to you to ensure that there are no defects, 
that the code is fit for your purpose and does not infringe on other copyrights. Use this code only if 
you agree with these conditions. The entire risk of using the code lays with you :-)

Written 2014-2022 in Switzerland & Singapore by Jürgpeter Huber 

Contact: https://github.com/PeterHuberSg/WpfControlTestbench
********************************************************************************************************/
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;


namespace WpfTestbench {

  /// <summary>
  /// WPF Control used in a test window to display a control to be tested 
  /// </summary>
  public class TestBench: Control {

    #region Properties
    //      ----------

    /// <summary>
    /// A control inheriting from FrameworkElement to be inspected in TestBench. This property is mainly
    /// used by XAML and gets set exactly once after the TestBench constructor has executed. Changing its value after
    /// the initial setting throws an exception.
    /// </summary>
    public FrameworkElement? TestControl {
      get { return (FrameworkElement)GetValue(TestControlProperty); }
      set { SetValue(TestControlProperty, value); }
    }

    /// <summary>
    /// Dependency Property definition for TestControl
    /// </summary>
    public static readonly DependencyProperty TestControlProperty =
    DependencyProperty.Register("TestControl", typeof(FrameworkElement), typeof(TestBench),
      new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender,
      TestControlChanged));

    private static void TestControlChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
      //unfortunately, XAML cannot pass arguments in a constructor, but the attribute values defined in XAML will
      //be assigned to properties, once TestBench is constructed. 
      if (e.OldValue is not null) {
        if (DesignerProperties.GetIsInDesignMode(d)) {
          throw new NotSupportedException("TestBench: Press the WPF Designer Refresh button to see changed TestControl.");
        } else {
          throw new NotSupportedException("TestBench allows to add a TestControl only once.");
        }
      }

      if (e.NewValue is FrameworkElement) {
        //XAML is assigning a TextControl to TestBench
        TestBench testBench = (TestBench)d;
        testBench.setupStandardPropertyViewer();

      } else {
        throw new NotSupportedException("TestBench allows only to add a FrameworkElement.");
      }
    }


    /// <summary>
    /// A container for WPF controls used to test properties of TestControl
    /// </summary>
    public FrameworkElement? TestProperties {
      get { return (FrameworkElement)GetValue(TestPropertiesProperty); }
      set { SetValue(TestPropertiesProperty, value); }
    }

    /// <summary>
    /// Dependency Property definition for TestProperties
    /// </summary>
    public static readonly DependencyProperty TestPropertiesProperty =
    DependencyProperty.Register("TestProperties", typeof(FrameworkElement), typeof(TestBench),
      new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender,
      TestPropertiesChanged));

    private static void TestPropertiesChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
      TestBench testBench = (TestBench)d;
      if (e.OldValue is FrameworkElement) {
        if (DesignerProperties.GetIsInDesignMode(d)) {
          throw new NotSupportedException("TestBench: Press the WPF Designer Refresh button to see changed TestProperties.");
        } else {
          throw new NotSupportedException("TestBench allows to add TestProperties only once.");
        }
      }
      if (e.NewValue is FrameworkElement newControl) {
        testBench.outerGrid.Children.Add(newControl);
        //Row = 0, Column = 0
      }
    }


    /// <summary>
    /// Defines which test functions should get executed when user clicks on Next Test button. TestbenchWindow fills 
    /// it with some standard test like for sizing, layout, Background, Font, etc. <para/>
    /// A test function sets some properties on the test object. The test function returns a test action which 
    /// verifies that the test object has responded properly to the changed inputs. Since it is not easy to find out 
    /// when the WPF has processed the changes caused by the test function (layout happens after WPF returns from 
    /// the test function), the test action gets executed before the user initiates the next test.
    /// </summary>
    /// <example><![CDATA[
    /// Action testFunc() {
    ///   TestFrameworkElement.Property1 = testInput1;
    ///   TestFrameworkElement.Property2 = testInput2;
    ///
    ///   //test to be executed before next test gets executed
    ///   return () => { 
    ///     if (TestFrameworkElement.Property3!=expectedResult) throw new Exception("some Error message");
    ///   };
    /// }
    /// ]]></example>
    public readonly List<(string Name, Func<Action?> Function)> TestFunctions;


    /// <summary>
    /// Action to execute when Reset button is pressed.
    /// This can be helpful to go into a well defined stated after testing or user has changed some properties.
    /// TestBench does this already for FrameworkElement and Control properties by reading their values in
    /// the TestBench_Loaded event. 
    /// </summary>
    public Action? ResetAction;
    #endregion


    #region Constructor
    //      -----------

    /*
    +---------------------------------------+ Outer Grid
    |TestProperties                         |
    +---------------------------------------+
    |standardPropertyViewer                 |
    +===============hSplitter===============+
    |+---------------------+---------------+| Inner Grid
    ||testControlContainer|||wpfTraceViewer||
    |+---------------------+---------------+|
    +---------------------------------------+
    */
    readonly Grid outerGrid;
    //TestProperties, defined in properties section
    StandardPropertyViewer standardPropertyViewer;
    readonly GridSplitter hSplitter;
    readonly Grid innerGrid;
    FrameworkElement? testControlContainer;//contains TestControl, can be Grid, Panel, ScrollViewer, ...
    readonly GridSplitter vSplitter;
    readonly WpfTraceViewer wpfTraceViewer;

    readonly List<Visual> children; //use LogicalChildren to get the children
    readonly VisualCollection visualCollection;


    #pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor.
    public TestBench() {
    #pragma warning restore CS8618 // 
      Background = Brushes.Gainsboro;

      outerGrid = new Grid();
      outerGrid.RowDefinitions.Add(new RowDefinition { Height =  GridLength.Auto });//testPropertiesContainer
      outerGrid.RowDefinitions.Add(new RowDefinition { Height =  GridLength.Auto });//Standard Properties
      outerGrid.RowDefinitions.Add(new RowDefinition { Height =  GridLength.Auto });//hSplitter
      outerGrid.RowDefinitions.Add(new RowDefinition { /*1Star*/ }); //InnerGrid
      children = new List<Visual> {
        outerGrid
      };
      AddLogicalChild(outerGrid); //equivalent to outerGrid.Parent = this;
      //add outerGrid to visual tree
      visualCollection = new VisualCollection(this) {
        outerGrid
      };

      //TestProperties gets added in TestPropertiesChanged() to outerGrid
      //row = 0; column = 0

      //standardPropertyViewer added in TestControlChanged() to outerGrid
      //row = 1; column = 0

      var hSplitterBrush = new LinearGradientBrush { StartPoint = new Point(0.5, 1), EndPoint = new Point(0.5, 0) };
      hSplitterBrush.GradientStops.Add(new GradientStop { Offset=0, Color=Colors.AntiqueWhite });
      hSplitterBrush.GradientStops.Add(new GradientStop { Offset=1, Color=Colors.DarkGray });
      hSplitter = new GridSplitter {
        Height = 5,
        HorizontalAlignment = HorizontalAlignment.Stretch,
        ResizeDirection = GridResizeDirection.Rows,
        ResizeBehavior = GridResizeBehavior.PreviousAndNext,
        Background = hSplitterBrush
      };
      outerGrid.Children.Add(hSplitter);
      Grid.SetRow(hSplitter, 2);//column = 0

      innerGrid = new Grid();
      innerGrid.ColumnDefinitions.Add(new ColumnDefinition { /*1Star*/ });//testControlContainer
      innerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });//vSplitter
      innerGrid.ColumnDefinitions.Add(new ColumnDefinition { /*1Star*/ MinWidth=300 });//wpfTraceViewer
      outerGrid.Children.Add(innerGrid);
      Grid.SetRow(innerGrid, 3);//column = 0

      //testControlContainer gets added in TestControlChanged() to innerGrid
      //row = 0; column = 0


      var vSplitterBrush = new LinearGradientBrush { StartPoint = new Point(0, 0.5), EndPoint = new Point(1, 0.5) };
      vSplitterBrush.GradientStops.Add(new GradientStop { Offset=0, Color=Colors.AntiqueWhite });
      vSplitterBrush.GradientStops.Add(new GradientStop { Offset=1, Color=Colors.DarkGray });
      vSplitter = new GridSplitter {
        Width = 5,
        VerticalAlignment = VerticalAlignment.Stretch,
        ResizeDirection = GridResizeDirection.Columns,
        ResizeBehavior = GridResizeBehavior.PreviousAndNext,
        Background = vSplitterBrush
      };
      innerGrid.Children.Add(vSplitter);
      Grid.SetColumn(vSplitter, 1);//row = 0

      wpfTraceViewer = new WpfTraceViewer {MinWidth=300};
      innerGrid.Children.Add(wpfTraceViewer);
      Grid.SetColumn(wpfTraceViewer, 2);//row = 0

      TestFunctions = new List<(string Name, Func<Action?> Function)>();

      Loaded += TestBench_Loaded;
    }


    private void setupStandardPropertyViewer() {
      //called once XAML sets TestBench.TestControl
      standardPropertyViewer = new StandardPropertyViewer(TestControl!, innerGrid);
      Grid.SetRow(standardPropertyViewer, 1);//column = 0
      outerGrid.Children.Add(standardPropertyViewer);
      setupContainers(standardPropertyViewer.ContainerComboBox);//executes innerGrid.Children.Add(testControlContainer);
      setContainer(containerTypeEnum.GridStar);//adds TestControl to testContainer
      createAxisLines();
      createOriginShadow();
    }


    private void TestBench_Loaded(object sender, RoutedEventArgs e) {
      savePreviousParameters();
      initTestFunctions();
    }
    #endregion


    #region TestControl Container
    //      ---------------------

    enum containerTypeEnum {
      GridStar,
      GridAuto,
      ScrollViewer,
      Canvas,
      HStackPanel,
      VStackPanel,
      DockPanel
    }


    GridTraced gridStarContainer;
    GridTraced gridAutoContainer;
    ScrollViewerTraced scrollViewerContainer;
    CanvasTraced canvasContainer;
    StackPanelTraced hStackPanelContainer;
    StackPanelTraced vStackPanelContainer;
    DockPanelTraced dockPanelContainer;


    private void setupContainers(ComboBox containerComboBox) {
      containerComboBox.Items.Add(new ComboBoxItem { Content = "Grid Star", Tag=containerTypeEnum.GridStar });
      containerComboBox.Items.Add(new ComboBoxItem { Content = "Grid Auto", Tag=containerTypeEnum.GridAuto });
      containerComboBox.Items.Add(new ComboBoxItem { Content = "ScrollViewer", Tag=containerTypeEnum.ScrollViewer });
      containerComboBox.Items.Add(new ComboBoxItem { Content = "Canvas", Tag=containerTypeEnum.Canvas });
      containerComboBox.Items.Add(new ComboBoxItem { Content = "Hor. StackPanel", Tag=containerTypeEnum.HStackPanel });
      containerComboBox.Items.Add(new ComboBoxItem { Content = "Vert. StackPanel", Tag=containerTypeEnum.VStackPanel });
      containerComboBox.Items.Add(new ComboBoxItem { Content = "DockPanel", Tag=containerTypeEnum.DockPanel });
      containerComboBox.SelectedIndex = 0;
      containerComboBox.SelectionChanged += ContainerComboBox_SelectionChanged;

      standardPropertyViewer!.ContainerTraceCheckBox.Click += ContainerTraceCheckBox_Click;

      SolidColorBrush containerBackground = Brushes.Gray;
      gridStarContainer = new GridTraced("GridStarContainer", isTracing: true) { Background = containerBackground};
      gridStarContainer.RowDefinitions.Add(new RowDefinition { /*1star*/ });
      gridStarContainer.ColumnDefinitions.Add(new ColumnDefinition { /*1star*/ });

      gridAutoContainer = new GridTraced("GridAutoContainer", isTracing: true) { Background = containerBackground};
      gridAutoContainer.RowDefinitions.Add(new RowDefinition { /*1star*/ });
      gridAutoContainer.RowDefinitions.Add(new RowDefinition { Height =  GridLength.Auto });
      gridAutoContainer.RowDefinitions.Add(new RowDefinition { /*1star*/ });
      gridAutoContainer.ColumnDefinitions.Add(new ColumnDefinition { /*1star*/ });
      gridAutoContainer.ColumnDefinitions.Add(new ColumnDefinition { Width =  GridLength.Auto });
      gridAutoContainer.ColumnDefinitions.Add(new ColumnDefinition { /*1star*/ });

      scrollViewerContainer = new ScrollViewerTraced("ScrollViewerContainer", isTracing: true) {
        Background = containerBackground,
        HorizontalScrollBarVisibility = ScrollBarVisibility.Visible,
        VerticalScrollBarVisibility = ScrollBarVisibility.Visible
      };

      canvasContainer = new CanvasTraced("CanvasContainer", isTracing: true) { Background = containerBackground};
      
      hStackPanelContainer = new StackPanelTraced("HStackPanelContainer", isTracing: true) 
        { Background = containerBackground, Orientation = Orientation.Horizontal};
      
      vStackPanelContainer = new StackPanelTraced("VStackPanelContainer", isTracing: true) 
        { Background = containerBackground, Orientation = Orientation.Vertical};
      
      dockPanelContainer = new DockPanelTraced("DockPanelContainer", isTracing: true) { Background = containerBackground};
    }


    private void ContainerComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e) {
      setContainer((containerTypeEnum)((ComboBoxItem)e.AddedItems[0]!).Tag);
    }


    private void ContainerTraceCheckBox_Click(object sender, RoutedEventArgs e) {
      if (testControlContainer is IIsTracing iIsTracing) {
        iIsTracing.IsTracing = ((CheckBox)sender).IsChecked??false;
      }
      var isTracing = ((CheckBox)sender).IsChecked??false;
      gridStarContainer.IsTracing = isTracing;
      gridAutoContainer.IsTracing = isTracing;
      scrollViewerContainer.IsTracing = isTracing;
      canvasContainer.IsTracing = isTracing;
      hStackPanelContainer.IsTracing = isTracing;
      vStackPanelContainer.IsTracing = isTracing;
      dockPanelContainer.IsTracing = isTracing;
    }


    containerTypeEnum containerType;


    private void setContainer(containerTypeEnum newContainerType) {
      if (testControlContainer is not null) {
        //remove TestControl from testControlContainer and testControlContainer from innerGrid
        switch (containerType) {
        case containerTypeEnum.GridStar:
          gridStarContainer.Children.Remove(TestControl);
          break;
        case containerTypeEnum.GridAuto:
          gridAutoContainer.Children.Remove(TestControl);
          break;
        case containerTypeEnum.ScrollViewer:
          scrollViewerContainer.Content = null;
          break;
        case containerTypeEnum.Canvas:
          canvasContainer.Children.Remove(TestControl);
          break;
        case containerTypeEnum.HStackPanel:
          hStackPanelContainer.Children.Remove(TestControl);
          break;
        case containerTypeEnum.VStackPanel:
          vStackPanelContainer.Children.Remove(TestControl);
          break;
        case containerTypeEnum.DockPanel:
          dockPanelContainer.Children.Remove(TestControl);
          break;
        default:
          throw new NotSupportedException();
        }
        innerGrid.Children.Remove(testControlContainer);
      }

      //select new testControlContainer, add TestControl to it and add testControlContainer to innerGrid
      switch (newContainerType) {
      case containerTypeEnum.GridStar:
        testControlContainer = gridStarContainer;
        gridStarContainer.Children.Add(TestControl);
        Grid.SetRow(TestControl, 0);
        Grid.SetColumn(TestControl, 0);
        break;
      case containerTypeEnum.GridAuto:
        testControlContainer = gridAutoContainer;
        gridAutoContainer.Children.Add(TestControl);
        Grid.SetRow(TestControl, 1);
        Grid.SetColumn(TestControl, 1);
        break;
      case containerTypeEnum.ScrollViewer:
        testControlContainer = scrollViewerContainer;
        scrollViewerContainer.Content = TestControl;
        break;
      case containerTypeEnum.Canvas:
        testControlContainer = canvasContainer;
        canvasContainer.Children.Add(TestControl);
        break;
      case containerTypeEnum.HStackPanel:
        testControlContainer = hStackPanelContainer;
        hStackPanelContainer.Children.Add(TestControl);
        break;
      case containerTypeEnum.VStackPanel:
        testControlContainer = vStackPanelContainer;
        vStackPanelContainer.Children.Add(TestControl);
        break;
      case containerTypeEnum.DockPanel:
        testControlContainer = dockPanelContainer;
        dockPanelContainer.Children.Add(TestControl);
        break;
      default:
        throw new NotSupportedException();
      }

      innerGrid.Children.Insert(0, testControlContainer);//row = 0; column = 0
      containerType = newContainerType;
    }
    #endregion


    #region Axis Lines
    //      ----------

    const double strokeThickness = 2; //thickness of visible lines displayed to the user, who can change their position with the mouse.
    readonly AxisLine[/*type*/,/*dimension*/,/*order*/] axisLines = new AxisLine[3, 2, 2];
    //referenceLine is in the same innerGrid cell like testControlContainer. It is used to get the cell related
    //transformation to testFrameworkElement, which then is used to calculate the position of the axis.
    Line? referenceLine;

    const LineTypeEnum isMargin = LineTypeEnum.margin;
    const LineTypeEnum isBorder = LineTypeEnum.border;
    const LineTypeEnum isPadding = LineTypeEnum.padding;
    const DimensionEnum isWidth_ = DimensionEnum.width;
    const DimensionEnum isHeight = DimensionEnum.height;
    const LineOrderEnum isFirst_ = LineOrderEnum.first;
    const LineOrderEnum isSecond = LineOrderEnum.second;


    private void createAxisLines() {
      referenceLine = new Line();
      innerGrid.Children.Add(referenceLine);//row: 0, column: 0
      //Grid.SetRow(referenceLine, testFrameworkElementGridRow);
      //Grid.SetColumn(referenceLine, testFrameworkElementGirdColumn);
      var axisLineContext = new AxisLineContext(strokeThickness, TestControl!, innerGrid);
      createAxisLine(isMargin, isWidth_, isFirst_, axisLineContext);
      createAxisLine(isMargin, isWidth_, isSecond, axisLineContext);
      createAxisLine(isMargin, isHeight, isFirst_, axisLineContext);
      createAxisLine(isMargin, isHeight, isSecond, axisLineContext);
      if (TestControl is Control) {
        createAxisLine(isBorder, isWidth_, isFirst_, axisLineContext);
        createAxisLine(isBorder, isWidth_, isSecond, axisLineContext);
        createAxisLine(isBorder, isHeight, isFirst_, axisLineContext);
        createAxisLine(isBorder, isHeight, isSecond, axisLineContext);

        createAxisLine(isPadding, isWidth_, isFirst_, axisLineContext);
        createAxisLine(isPadding, isWidth_, isSecond, axisLineContext);
        createAxisLine(isPadding, isHeight, isFirst_, axisLineContext);
        createAxisLine(isPadding, isHeight, isSecond, axisLineContext);
      }

      TestControl!.LayoutUpdated += TestControl_LayoutUpdated;
    }


    private void TestControl_LayoutUpdated(object? sender, EventArgs e) {
      updateAxisPositions();
    }


    private void createAxisLine(LineTypeEnum lineType, DimensionEnum dimension, LineOrderEnum lineOrder,
      AxisLineContext axisLineContext) 
    {
      axisLines[(int)lineType, (int)dimension, (int)lineOrder] =
        new AxisLine(lineType, dimension, lineOrder, axisLineContext);
    }


    Point offsetPointUsed = new(double.NaN, double.NaN);
    Size renderSizeUsed = new(double.NaN, double.NaN);
    Thickness marginUsed;
    Thickness borderUsed;
    Thickness paddingUsed;


    private void updateAxisPositions() {
      bool hasHeightChanged = false;
      bool hasWidthChanged = false;

      GeneralTransform generalTransform1 = TestControl!.TransformToVisual(referenceLine);
      Point newOffsetPoint = generalTransform1.Transform(new Point(0, 0));

      if (offsetPointUsed!=newOffsetPoint) {
        hasHeightChanged |= (offsetPointUsed.Y!=newOffsetPoint.Y);
        hasWidthChanged |= (offsetPointUsed.X!=newOffsetPoint.X);
        offsetPointUsed = newOffsetPoint;
      }

      if (renderSizeUsed!=TestControl.RenderSize) {
        hasHeightChanged |= (renderSizeUsed.Height!=TestControl.RenderSize.Height);
        hasWidthChanged |= (renderSizeUsed.Width!=TestControl.RenderSize.Width);
        renderSizeUsed = TestControl.RenderSize;
      }

      updateThickness(ref marginUsed, TestControl.Margin, ref hasHeightChanged, ref hasWidthChanged);

      if (TestControl is Control testControl) {
        updateThickness(ref borderUsed, testControl.BorderThickness, ref hasHeightChanged, ref hasWidthChanged);
        updateThickness(ref paddingUsed, testControl.Padding, ref hasHeightChanged, ref hasWidthChanged);
      }


      if (!hasHeightChanged&&!hasWidthChanged) return;

      foreach (AxisLine axisLine in axisLines) {
        if (axisLine!=null) {
          if ((axisLine.Dimension==DimensionEnum.width && hasWidthChanged) ||
            (/*axisLine.Dimension==DimensionEnum.height && */ hasHeightChanged))
            axisLine.UpdateLinePosition(offsetPointUsed);
        }
      }
    }


    private static void updateThickness(ref Thickness thickness, Thickness newThickness, ref bool isHorizontalChange, ref bool isVerticalChange) {
      if (thickness==newThickness) return;

      isHorizontalChange = isHorizontalChange | (thickness.Top!=newThickness.Top) | (thickness.Bottom!=newThickness.Bottom);
      isVerticalChange = isVerticalChange | (thickness.Left!=newThickness.Left) | (thickness.Right!=newThickness.Right);
      thickness = newThickness;
      return;
    }
    #endregion


    #region Shadow of original position
    //      ---------------------------

    Rectangle originShadow;


    private void createOriginShadow() {
      originShadow = new Rectangle {
        Fill = new SolidColorBrush(Color.FromArgb(0x80, 0xA0, 0xA0, 0xA0))
      };
      originShadow.Fill.Freeze();
      innerGrid.Children.Add(originShadow);//row: 0, column: 0
      //Grid.SetRow(originShadow, testFrameworkElementGridRow);
      //Grid.SetColumn(originShadow, testFrameworkElementGirdColumn);
      Grid.SetZIndex(originShadow, Grid.GetZIndex(TestControl) - 1);
      TestControl!.SizeChanged += TestControl_SizeChanged;
      var horizontalAlignmentDescriptor = 
        DependencyPropertyDescriptor.FromProperty(FrameworkElement.HorizontalAlignmentProperty, typeof(FrameworkElement));
      horizontalAlignmentDescriptor.AddValueChanged(TestControl, TestControl_HorizontalAlignmentChanged);
      var verticalAlignmentDescriptor = 
        DependencyPropertyDescriptor.FromProperty(FrameworkElement.VerticalAlignmentProperty, typeof(FrameworkElement));
      verticalAlignmentDescriptor.AddValueChanged(TestControl, TestControl_VerticalAlignmentChanged);
    }


    private void TestControl_SizeChanged(object sender, SizeChangedEventArgs e) {
      updateOriginShadowPosition();
    }


    void TestControl_HorizontalAlignmentChanged(object? sender, EventArgs args) {
      updateOriginShadowPosition();
    }


    void TestControl_VerticalAlignmentChanged(object? sender, EventArgs args) {
      updateOriginShadowPosition();
    }


    private void updateOriginShadowPosition() {
      originShadow.HorizontalAlignment = TestControl!.HorizontalAlignment;
      #pragma warning disable IDE0045 // Convert to conditional expression
      if (originShadow.HorizontalAlignment==HorizontalAlignment.Stretch && double.IsNaN(TestControl.Width)) {
        originShadow.Width = double.NaN;
      } else {
        originShadow.Width = Math.Max(strokeThickness, TestControl.ActualWidth);
      }
      originShadow.VerticalAlignment = TestControl.VerticalAlignment;
      if (originShadow.VerticalAlignment==VerticalAlignment.Stretch && double.IsNaN(TestControl.Height)) {
        originShadow.Height = double.NaN;
      } else {
        originShadow.Height = Math.Max(strokeThickness, TestControl.ActualHeight);
      }
      #pragma warning restore IDE0045
    }
    #endregion


    #region Testing
    //      -------

    int testFuncsIndex = -1;
    Action? testResultAction;//tests if the last testFunction was successful before next test gets executed


    const double nan = double.NaN;
    readonly static FontFamily arial = new("Ariel");
    readonly static FontFamily couri = new("Courier New");
    const HorizontalAlignment hstr = HorizontalAlignment.Stretch;
    const HorizontalAlignment left = HorizontalAlignment.Left;
    const HorizontalAlignment hcen = HorizontalAlignment.Center;
    const HorizontalAlignment rigt = HorizontalAlignment.Right;
    const VerticalAlignment vst = VerticalAlignment.Stretch;
    const VerticalAlignment top = VerticalAlignment.Top;
    const VerticalAlignment vce = VerticalAlignment.Center;
    const VerticalAlignment bot = VerticalAlignment.Bottom;
    const bool CoT = true; //is test only for Control ?
    const bool FeT = true; //is test for FrameworkElement and Control ?


    private void initTestFunctions() {
      bool isCo = TestControl is Control;
      var tf = TestFunctions;
      var control = TestControl as Control;
      if (control is not null) {
        tf.Add(("Colors", () => {
          control.Background = Brushes.White;
          control.Foreground = Brushes.Black;
          control.BorderBrush = Brushes.Black;
          return tFunc(nan, nan, hstr, hstr, vst, vst, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, arial, 12);
        }));
        tf.Add(("Background", () => { 
          control.Background = Brushes.LightGoldenrodYellow;
          control.Foreground = Brushes.Black;
          control.BorderBrush = Brushes.Black;
          return null; 
        }));
        tf.Add(("Border Color", () => {
          control.Background = Brushes.LightGoldenrodYellow;
          control.Foreground = Brushes.Black;
          control.BorderBrush = Brushes.Goldenrod;
          return null; 
        }));
        tf.Add(("Foreground", () => {
          control.Background = Brushes.LightGoldenrodYellow;
          control.Foreground = Brushes.DarkGoldenrod;
          control.BorderBrush = Brushes.Goldenrod;
          return null; 
        }));
        tf.Add(("Reset Colors", () => {
          control.Background = previousBackground;
          control.Foreground = previousForeground;
          control.BorderBrush = previousBorderBrush;
          return null; 
        }));
      }
      //     Width     Horizontal   Vertical  Left        Right       Top         Bottom      Text         Font
      //         Height Contr Conte Cntr Cnte Mar Bor Pad Pad Bor Mar Mar Bor Pad Pad Bor Mar 
      addTest(nan, nan, hstr, hstr, vst, vst,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0, arial, 12, isCo, FeT, "Layouting");
      addTest(nan, nan, hstr, hstr, vst, vst, 10,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0, arial, 12, isCo, FeT, "Left Margin");
      addTest(nan, nan, hstr, hstr, vst, vst, 10, 10,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0, arial, 12, isCo, CoT, "Left Border");
      addTest(nan, nan, hstr, hstr, vst, vst, 10, 10, 10,  0,  0,  0,  0,  0,  0,  0,  0,  0, arial, 12, isCo, CoT, "Left Padding");
      addTest(nan, nan, hstr, hstr, vst, vst, 10, 10, 10, 10,  0,  0,  0,  0,  0,  0,  0,  0, arial, 12, isCo, CoT, "Right Padding");
      addTest(nan, nan, hstr, hstr, vst, vst, 10, 10, 10, 10, 10,  0,  0,  0,  0,  0,  0,  0, arial, 12, isCo, CoT, "Right Border");
      addTest(nan, nan, hstr, hstr, vst, vst, 10, 10, 10, 10, 10, 10,  0,  0,  0,  0,  0,  0, arial, 12, isCo, FeT, "Right Margin");
      addTest(nan, nan, hstr, hstr, vst, vst, 10, 10, 10, 10, 10, 10, 10,  0,  0,  0,  0,  0, arial, 12, isCo, FeT, "Top Margin");
      addTest(nan, nan, hstr, hstr, vst, vst, 10, 10, 10, 10, 10, 10, 10, 10,  0,  0,  0,  0, arial, 12, isCo, CoT, "Top Border");
      addTest(nan, nan, hstr, hstr, vst, vst, 10, 10, 10, 10, 10, 10, 10, 10, 10,  0,  0,  0, arial, 12, isCo, CoT, "Top Padding");
      addTest(nan, nan, hstr, hstr, vst, vst, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10,  0,  0, arial, 12, isCo, CoT, "Bottom Padding");
      addTest(nan, nan, hstr, hstr, vst, vst, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10,  0, arial, 12, isCo, CoT, "Bottom Border");
      addTest(nan, nan, hstr, hstr, vst, vst, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, arial, 12, isCo, FeT, "Bottom Margin");
      addTest(nan, nan, left, hstr, vst, vst, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, arial, 12, isCo, FeT, "Horizontal Alignment Left");
      addTest(nan, nan, hcen, hstr, vst, vst, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, arial, 12, isCo, FeT, "Horizontal Alignment Center");
      addTest(nan, nan, rigt, hstr, vst, vst, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, arial, 12, isCo, FeT, "Horizontal Alignment Right");
      addTest(nan, nan, hstr, hstr, top, vst, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, arial, 12, isCo, FeT, "Vertical Alignment Top");
      addTest(nan, nan, hstr, hstr, vce, vst, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, arial, 12, isCo, FeT, "Vertical Alignment Center");
      addTest(nan, nan, hstr, hstr, bot, vst, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, arial, 12, isCo, FeT, "Vertical Alignment Bottom");
      addTest(nan, nan, hstr, left, vst, vst, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, arial, 12, isCo, CoT, "Horizontal Content Left");
      addTest(nan, nan, hstr, hcen, vst, vst, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, arial, 12, isCo, CoT, "Horizontal Content Center");
      addTest(nan, nan, hstr, rigt, vst, vst, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, arial, 12, isCo, CoT, "Horizontal Content Right");
      addTest(nan, nan, hstr, hstr, vst, top, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, arial, 12, isCo, CoT, "Vertical Content Top");
      addTest(nan, nan, hstr, hstr, vst, vce, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, arial, 12, isCo, CoT, "Vertical Content Center");
      addTest(nan, nan, hstr, hstr, vst, bot, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, arial, 12, isCo, CoT, "Vertical Content Bottom");
      addTest(nan, nan, hstr, hstr, vst, vst, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, arial, 14, isCo, CoT, "Hor Content Stretched, Arial 14");
      addTest(nan, nan, hstr, hstr, vst, vst, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, arial, 16, isCo, CoT, "Hor Content Stretched, Arial 16");
      addTest(nan, nan, hstr, hstr, vst, vst, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, arial, 18, isCo, CoT, "Hor Content Stretched, Arial 18");
      addTest(nan, nan, hstr, hstr, vst, vst, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, arial, 22, isCo, CoT, "Hor Content Stretched, Arial 22");
      addTest(nan, nan, hstr, hstr, vst, vst, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, arial, 26, isCo, CoT, "Hor Content Stretched, Arial 26");
      addTest(nan, nan, hstr, hstr, vst, vst, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, arial, 32, isCo, CoT, "Hor Content Stretched, Arial 32");
      addTest(nan, nan, hstr, hstr, vst, vst, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, arial, 40, isCo, CoT, "Hor Content Stretched, Arial 40");
      addTest(nan, nan, hstr, hstr, vst, vst, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, couri, 60, isCo, CoT, "Hor Content Stretched, Courier 60");
      addTest(nan, nan, hstr, rigt, vst, vst, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, arial, 12, isCo, CoT, "Hor Content Right, Arial 12");
      addTest(nan, nan, hstr, rigt, vst, vst, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, arial, 14, isCo, CoT, "Hor Content Right, Arial 14");
      addTest(nan, nan, hstr, rigt, vst, vst, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, arial, 16, isCo, CoT, "Hor Content Right, Arial 16");
      addTest(nan, nan, hstr, rigt, vst, vst, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, arial, 18, isCo, CoT, "Hor Content Right, Arial 18");
      addTest(nan, nan, hstr, rigt, vst, vst, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, arial, 22, isCo, CoT, "Hor Content Right, Arial 22");
      addTest(nan, nan, hstr, rigt, vst, vst, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, arial, 26, isCo, CoT, "Hor Content Right, Arial 26");
      addTest(nan, nan, hstr, rigt, vst, vst, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, arial, 32, isCo, CoT, "Hor Content Right, Arial 32");
      addTest(nan, nan, hstr, rigt, vst, vst, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, arial, 40, isCo, CoT, "Hor Content Right, Arial 40");
      addTest(nan, nan, hstr, rigt, vst, vst, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, couri, 60, isCo, CoT, "Hor Content Right, Courier 60");
      
      addTest(tWi, nan, hstr, hstr, vst, vst, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, arial, 12, isCo, CoT, "Width 50%, Horizontal Stretched, Hor Content Stretched");
      addTest(tWi, nan, hstr, left, vst, vst, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, arial, 12, isCo, CoT, "Width 50%, Horizontal Stretched, Hor Content Left");
      addTest(tWi, nan, hstr, hcen, vst, vst, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, arial, 12, isCo, CoT, "Width 50%, Horizontal Stretched, Hor Content Center");
      addTest(tWi, nan, hstr, rigt, vst, vst, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, arial, 12, isCo, CoT, "Width 50%, Horizontal Stretched, Hor Content Right");
      addTest(tWi, nan, left, hstr, vst, vst, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, arial, 12, isCo, CoT, "Width 50%, Horizontal Left, Hor Content Stretched");
      addTest(tWi, nan, left, left, vst, vst, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, arial, 12, isCo, CoT, "Width 50%, Horizontal Left, Hor Content Left");
      addTest(tWi, nan, left, hcen, vst, vst, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, arial, 12, isCo, CoT, "Width 50%, Horizontal Left, Hor Content Center");
      addTest(tWi, nan, left, rigt, vst, vst, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, arial, 12, isCo, CoT, "Width 50%, Horizontal Left, Hor Content Right");
      addTest(tWi, nan, hcen, hstr, vst, vst, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, arial, 12, isCo, CoT, "Width 50%, Horizontal Center, Hor Content Stretched");
      addTest(tWi, nan, hcen, left, vst, vst, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, arial, 12, isCo, CoT, "Width 50%, Horizontal Center, Hor Content Left");
      addTest(tWi, nan, hcen, hcen, vst, vst, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, arial, 12, isCo, CoT, "Width 50%, Horizontal Center, Hor Content Center");
      addTest(tWi, nan, hcen, rigt, vst, vst, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, arial, 12, isCo, CoT, "Width 50%, Horizontal Center, Hor Content Right");
      addTest(tWi, nan, rigt, hstr, vst, vst, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, arial, 12, isCo, CoT, "Width 50%, Horizontal Right, Hor Content Stretched");
      addTest(tWi, nan, rigt, left, vst, vst, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, arial, 12, isCo, CoT, "Width 50%, Horizontal Right, Hor Content Left");
      addTest(tWi, nan, rigt, hcen, vst, vst, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, arial, 12, isCo, CoT, "Width 50%, Horizontal Right, Hor Content Center");
      addTest(tWi, nan, rigt, rigt, vst, vst, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, arial, 12, isCo, CoT, "Width 50%, Horizontal Right, Hor Content Right");
      addTest(nan, tHi, hstr, hstr, vst, vst, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, arial, 12, isCo, CoT, "Height 50%, Vertical Stretched, Ver Content Stretched");
      addTest(nan, tHi, hstr, hstr, vst, top, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, arial, 12, isCo, CoT, "Height 50%, Vertical Stretched, Ver Content Top");
      addTest(nan, tHi, hstr, hstr, vst, vce, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, arial, 12, isCo, CoT, "Height 50%, Vertical Stretched, Ver Content Center");
      addTest(nan, tHi, hstr, hstr, vst, bot, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, arial, 12, isCo, CoT, "Height 50%, Vertical Stretched, Ver Content Bottom");
      addTest(nan, tHi, hstr, hstr, top, vst, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, arial, 12, isCo, CoT, "Height 50%, Vertical Top, Ver Content Stretched");
      addTest(nan, tHi, hstr, hstr, top, top, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, arial, 12, isCo, CoT, "Height 50%, Vertical Top, Ver Content Top");
      addTest(nan, tHi, hstr, hstr, top, vce, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, arial, 12, isCo, CoT, "Height 50%, Vertical Top, Ver Content Center");
      addTest(nan, tHi, hstr, hstr, top, bot, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, arial, 12, isCo, CoT, "Height 50%, Vertical Top, Ver Content Bottom");
      addTest(nan, tHi, hstr, hstr, vce, vst, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, arial, 12, isCo, CoT, "Height 50%, Vertical Center, Ver Content Stretched");
      addTest(nan, tHi, hstr, hstr, vce, top, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, arial, 12, isCo, CoT, "Height 50%, Vertical Center, Ver Content Top");
      addTest(nan, tHi, hstr, hstr, vce, vce, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, arial, 12, isCo, CoT, "Height 50%, Vertical Center, Ver Content Center");
      addTest(nan, tHi, hstr, hstr, vce, bot, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, arial, 12, isCo, CoT, "Height 50%, Vertical Center, Ver Content Bottom");
      addTest(nan, tHi, hstr, hstr, bot, vst, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, arial, 12, isCo, CoT, "Height 50%, Vertical Bottom, Ver Content Stretched");
      addTest(nan, tHi, hstr, hstr, bot, top, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, arial, 12, isCo, CoT, "Height 50%, Vertical Bottom, Ver Content Top");
      addTest(nan, tHi, hstr, hstr, bot, vce, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, arial, 12, isCo, CoT, "Height 50%, Vertical Bottom, Ver Content Center");
      addTest(nan, tHi, hstr, hstr, bot, bot, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, arial, 12, isCo, CoT, "Height 50%, Vertical Bottom, Ver Content Bottom");
      addTest(tWi, tHi, hstr, hstr, vst, vst, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, arial, 12, isCo, CoT, "Width&Height 50%, Horizontal Stretched, Hor Content Stretched");
      addTest(tWi, tHi, hstr, left, vst, vst, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, arial, 12, isCo, CoT, "Width&Height 50%, Horizontal Stretched, Hor Content Left");
      addTest(tWi, tHi, hstr, hcen, vst, vst, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, arial, 12, isCo, CoT, "Width&Height 50%, Horizontal Stretched, Hor Content Center");
      addTest(tWi, tHi, hstr, rigt, vst, vst, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, arial, 12, isCo, CoT, "Width&Height 50%, Horizontal Stretched, Hor Content Right");
      addTest(tWi, tHi, left, hstr, vst, vst, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, arial, 12, isCo, CoT, "Width&Height 50%, Horizontal Left, Hor Content Stretched");
      addTest(tWi, tHi, left, left, vst, vst, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, arial, 12, isCo, CoT, "Width&Height 50%, Horizontal Left, Hor Content Left");
      addTest(tWi, tHi, left, hcen, vst, vst, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, arial, 12, isCo, CoT, "Width&Height 50%, Horizontal Left, Hor Content Center");
      addTest(tWi, tHi, left, rigt, vst, vst, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, arial, 12, isCo, CoT, "Width&Height 50%, Horizontal Left, Hor Content Right");
      addTest(tWi, tHi, hcen, hstr, vst, vst, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, arial, 12, isCo, CoT, "Width&Height 50%, Horizontal Center, Hor Content Stretched");
      addTest(tWi, tHi, hcen, left, vst, vst, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, arial, 12, isCo, CoT, "Width&Height 50%, Horizontal Center, Hor Content Left");
      addTest(tWi, tHi, hcen, hcen, vst, vst, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, arial, 12, isCo, CoT, "Width&Height 50%, Horizontal Center, Hor Content Center");
      addTest(tWi, tHi, hcen, rigt, vst, vst, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, arial, 12, isCo, CoT, "Width&Height 50%, Horizontal Center, Hor Content Right");
      addTest(tWi, tHi, rigt, hstr, vst, vst, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, arial, 12, isCo, CoT, "Width&Height 50%, Horizontal Right, Hor Content Stretched");
      addTest(tWi, tHi, rigt, left, vst, vst, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, arial, 12, isCo, CoT, "Width&Height 50%, Horizontal Right, Hor Content Left");
      addTest(tWi, tHi, rigt, hcen, vst, vst, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, arial, 12, isCo, CoT, "Width&Height 50%, Horizontal Right, Hor Content Center");
      addTest(tWi, tHi, rigt, rigt, vst, vst, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, arial, 12, isCo, CoT, "Width%Height 50%, Horizontal Right, Hor Content Right");
      addTest(tWi, tHi, hstr, hstr, vst, vst, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, arial, 12, isCo, CoT, "Width&Height 50%, Vertical Stretched, Ver Content Stretched");
      addTest(tWi, tHi, hstr, hstr, vst, top, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, arial, 12, isCo, CoT, "Width&Height 50%, Vertical Stretched, Ver Content Top");
      addTest(tWi, tHi, hstr, hstr, vst, vce, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, arial, 12, isCo, CoT, "Width&Height 50%, Vertical Stretched, Ver Content Center");
      addTest(tWi, tHi, hstr, hstr, vst, bot, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, arial, 12, isCo, CoT, "Width&Height 50%, Vertical Stretched, Ver Content Bottom");
      addTest(tWi, tHi, hstr, hstr, top, vst, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, arial, 12, isCo, CoT, "Width&Height 50%, Vertical Top, Ver Content Stretched");
      addTest(tWi, tHi, hstr, hstr, top, top, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, arial, 12, isCo, CoT, "Width&Height 50%, Vertical Top, Ver Content Top");
      addTest(tWi, tHi, hstr, hstr, top, vce, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, arial, 12, isCo, CoT, "Width&Height 50%, Vertical Top, Ver Content Center");
      addTest(tWi, tHi, hstr, hstr, top, bot, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, arial, 12, isCo, CoT, "Width&Height 50%, Vertical Top, Ver Content Bottom");
      addTest(tWi, tHi, hstr, hstr, vce, vst, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, arial, 12, isCo, CoT, "Width&Height 50%, Vertical Center, Ver Content Stretched");
      addTest(tWi, tHi, hstr, hstr, vce, top, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, arial, 12, isCo, CoT, "Width&Height 50%, Vertical Center, Ver Content Top");
      addTest(tWi, tHi, hstr, hstr, vce, vce, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, arial, 12, isCo, CoT, "Width&Height 50%, Vertical Center, Ver Content Center");
      addTest(tWi, tHi, hstr, hstr, vce, bot, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, arial, 12, isCo, CoT, "Width&Height 50%, Vertical Center, Ver Content Bottom");
      addTest(tWi, tHi, hstr, hstr, bot, vst, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, arial, 12, isCo, CoT, "Width&Height 50%, Vertical Bottom, Ver Content Stretched");
      addTest(tWi, tHi, hstr, hstr, bot, top, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, arial, 12, isCo, CoT, "Width&Height 50%, Vertical Bottom, Ver Content Top");
      addTest(tWi, tHi, hstr, hstr, bot, vce, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, arial, 12, isCo, CoT, "Width&Height 50%, Vertical Bottom, Ver Content Center");
      addTest(tWi, tHi, hstr, hstr, bot, bot, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, arial, 12, isCo, CoT, "Width&Height 50%, Vertical Bottom, Ver Content Bottom");
      tf.Add(("Standard Properties", () => {restorePreviousParameters(); return null;}));

      standardPropertyViewer.NextTestButton.Click += new RoutedEventHandler(nextTestButton_Click);
      standardPropertyViewer.PreviousTestButton.Click += new RoutedEventHandler(previousTestButton_Click);
      standardPropertyViewer.ResetButton.Click += new RoutedEventHandler(resetButton_Click);
    }


    private void addTest(
      double width,
      double height,
      HorizontalAlignment horizontalControlAlignment,
      HorizontalAlignment horizontalContentAlignment,
      VerticalAlignment verticalControlAlignment,
      VerticalAlignment verticalContentAlignment,
      double marginLeft,
      double borderLeft,
      double paddingLeft,
      double paddingRight,
      double borderRight,
      double marginRight,
      double marginTop,
      double borderTop,
      double paddingTop,
      double paddingBottom,
      double borderBottom,
      double marginBottom,
      FontFamily fontFamily,
      double fontSize,
      bool isControl,
      bool isControlTest,
      string testName) 
    {
      if (isControlTest && !isControl) return;

      TestFunctions.Add((testName, () => tFunc(
        width,
        height,
        horizontalControlAlignment,
        horizontalContentAlignment,
        verticalControlAlignment,
        verticalContentAlignment,
        marginLeft,
        borderLeft,
        paddingLeft,
        paddingRight,
        borderRight,
        marginRight,
        marginTop,
        borderTop,
        paddingTop,
        paddingBottom,
        borderBottom,
        marginBottom,
        fontFamily,
        fontSize)));
    }


    private Action? tFunc(
      double width,
      double height,
      HorizontalAlignment horizontalControlAlignment,
      HorizontalAlignment horizontalContentAlignment,
      VerticalAlignment verticalControlAlignment,
      VerticalAlignment verticalContentAlignment,
      double marginLeft,
      double borderLeft,
      double paddingLeft,
      double paddingRight,
      double borderRight,
      double marginRight,
      double marginTop,
      double borderTop,
      double paddingTop,
      double paddingBottom,
      double borderBottom,
      double marginBottom,
      FontFamily fontFamily,
      double fontSize) 
    {
      TestControl!.Width = width;
      TestControl.Height = height;
      TestControl.HorizontalAlignment = horizontalControlAlignment;
      TestControl.VerticalAlignment = verticalControlAlignment;
      TestControl.Margin = new Thickness(marginLeft, marginTop, marginRight, marginBottom);
      if (TestControl is Control control) {
        control.HorizontalContentAlignment = horizontalContentAlignment;
        control.VerticalContentAlignment = verticalContentAlignment;
        control.BorderThickness = new Thickness(borderLeft, borderTop, borderRight, borderBottom);
        control.Padding = new Thickness(paddingLeft, paddingTop, paddingRight, paddingBottom);
        control.FontFamily = fontFamily;
        control.FontSize = fontSize;
      }
      return null;
    }


    void nextTestButton_Click(object sender, RoutedEventArgs e) {
      if (testResultAction!=null) {
        //verify result of previous test
        try {
          testResultAction();
        } catch (Exception ex) {
          Tracer.Error("Test Error: " + ex.Message + Environment.NewLine + ex.ToDetailString());
        }
        testResultAction = null;
      }

      //execute next test
      if (++testFuncsIndex>=TestFunctions.Count) {
        testFuncsIndex = 0;
      }
      var testInfo = TestFunctions[testFuncsIndex];
      if (!string.IsNullOrEmpty(testInfo.Name)) {
        try {//if testResultAction reports an error, it will only be detected when writing next trace.
          Tracer.IsBreakOnError = false;
          Tracer.TraceLine("Test: " + testInfo.Name);
        } finally {
          Tracer.IsBreakOnError = true;
        }
      }
      testResultAction = TestFunctions[testFuncsIndex].Function();
      standardPropertyViewer.TestTextBox.Visibility = Visibility.Visible;
      standardPropertyViewer.TestTextBox.Text = $"{testFuncsIndex} {testInfo.Name}";
    }


    void previousTestButton_Click(object sender, RoutedEventArgs e) {
      if (testResultAction!=null) {
        //verify result of previous test
        try {
          testResultAction();
        } catch (Exception ex) {
          Tracer.Error("Test Error: " + ex.Message + Environment.NewLine + ex.ToDetailString());
        }
        testResultAction = null;
      }

      //execute previous test
      if (--testFuncsIndex<0) {
        testFuncsIndex = TestFunctions.Count-1;
      }
      var testInfo = TestFunctions[testFuncsIndex];
      if (!string.IsNullOrEmpty(testInfo.Name)) {
        Tracer.TraceLine("Test: " + testInfo.Name);
      }
      testResultAction = TestFunctions[testFuncsIndex].Function();
      standardPropertyViewer.TestTextBox.Visibility = Visibility.Visible;
      standardPropertyViewer.TestTextBox.Text = $"{testFuncsIndex} {testInfo.Name}";
    }


    void resetButton_Click(object sender, RoutedEventArgs e) {
      if (testResultAction!=null) {
        //verify result of previous test
        try {
          testResultAction();
        } catch (Exception ex) {
          Tracer.Error("Test Error: " + ex.Message + Environment.NewLine + ex.ToDetailString());
        }
        testResultAction = null;
      }

      standardPropertyViewer.TestTextBox.Visibility = Visibility.Hidden;
      testFuncsIndex = -1;
      restorePreviousParameters();
      ResetAction?.Invoke();
    }


    double previousWidth;
    double previousHeight;
    double previousMinWidth;
    double previousMinHeight;
    double previousMaxWidth;
    double previousMaxHeight;
    double tWi;//testWidth
    double tHi;//testHeight
    HorizontalAlignment previousHorizontalControlAlignment;
    HorizontalAlignment previousHorizontalContentAlignment;
    VerticalAlignment previousVerticalControlAlignment;
    VerticalAlignment previousVerticalContentAlignment;
    Thickness previousMargin;
    Thickness previousBorderThickness;
    Thickness previousPadding;
    FontFamily previousFontFamily;
    double previousFontSize;
    Brush previousBackground;
    Brush previousForeground;
    Brush previousBorderBrush;


    private void savePreviousParameters() {
      previousWidth = TestControl!.Width;
      previousHeight = TestControl.Height;
      previousMinWidth = TestControl!.MinWidth;
      previousMinHeight = TestControl.MinHeight;
      previousMaxWidth = TestControl!.MaxWidth;
      previousMaxHeight = TestControl.MaxHeight;
      tWi = TestControl.ActualWidth/2;
      tHi = TestControl.ActualHeight/2;
      previousHorizontalControlAlignment = TestControl.HorizontalAlignment;
      previousVerticalControlAlignment = TestControl.VerticalAlignment;
      previousMargin = TestControl.Margin;
      if (TestControl is Control control) {
        previousHorizontalContentAlignment = control.HorizontalContentAlignment;
        previousVerticalContentAlignment = control.VerticalContentAlignment;
        previousBorderThickness = control.BorderThickness;
        previousPadding = control.Padding;
        previousFontFamily = control.FontFamily;
        previousFontSize = control.FontSize;
        previousBackground = control.Background;
        previousForeground = control.Foreground;
        previousBorderBrush = control.BorderBrush;
      }
    }


    private void restorePreviousParameters() {
      TestControl!.Width = previousWidth;
      TestControl.Height = previousHeight;
      TestControl.MinWidth = previousMinWidth;
      TestControl.MinHeight = previousMinHeight;
      TestControl.MaxWidth = previousMaxWidth;
      TestControl.MaxHeight = previousMaxHeight;
      TestControl.HorizontalAlignment = previousHorizontalControlAlignment;
      TestControl.VerticalAlignment = previousVerticalControlAlignment;
      TestControl.Margin = previousMargin;
      if (TestControl is Control control) {
        control.HorizontalContentAlignment = previousHorizontalContentAlignment;
        control.VerticalContentAlignment = previousVerticalContentAlignment;
        control.BorderThickness = previousBorderThickness;
        control.Padding = previousPadding;
        control.FontFamily = previousFontFamily;
        control.FontSize = previousFontSize;
        control.Background = previousBackground;
        control.Foreground = previousForeground;
        control.BorderBrush = previousBorderBrush;
      }
    }
    #endregion


    #region Visual and Logical Tree
    //      -----------------------

    /// <summary>
    /// Provides access to the logical children(Logical tree)
    /// </summary>
    protected override IEnumerator LogicalChildren {
      get {
        return children.GetEnumerator();
      }
    }


    /// <summary>
    /// Number of Visuals added as children to this control by inheriting class.
    /// </summary>
    protected override sealed int VisualChildrenCount {
      get {
        return visualCollection.Count;
      }
    }


    /// <summary>
    /// Returns a Visual added by inheriting class.
    /// </summary>
    protected override sealed Visual GetVisualChild(int index) {
      return visualCollection[index];
    }
    #endregion


    #region Layouting
    //      ---------

    protected override Size MeasureOverride(Size constraint) {
      outerGrid.Measure(constraint);
      var width = double.IsInfinity(constraint.Width) ? outerGrid.DesiredSize.Width : constraint.Width;
      var height = double.IsInfinity(constraint.Height) ? outerGrid.DesiredSize.Height : constraint.Height;
      return new Size(width, height);
    }


    protected override Size ArrangeOverride(Size arrangeBounds) {
      outerGrid.Arrange(new Rect(arrangeBounds));
      return arrangeBounds;
    }
    #endregion
  }
}
