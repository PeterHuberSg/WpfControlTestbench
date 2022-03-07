using CustomControlBaseLib;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using TracerLib;

namespace WpfTestbench {
  
  
  public class TestBench: CustomControlBase {

    #region Properties
    //      ----------

    /// <summary>
    /// A control inheriting from FrameworkElement to be inspected in WpfControlTestbench. This property is mainly
    /// used by XAML and gets set exactly once after the TestBench contructor has executed. Changing its value after
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

      if (e.NewValue is FrameworkElement newControl) {
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
      if (e.OldValue is FrameworkElement oldControl) {
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
    public List<(string Name, Func<Action?> Function)> TestFunctions;
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

    public TestBench() {
      Background = Brushes.Gainsboro;

      outerGrid = new Grid();
      outerGrid.RowDefinitions.Add(new RowDefinition { Height =  GridLength.Auto });//testPropertiesContainer
      outerGrid.RowDefinitions.Add(new RowDefinition { Height =  GridLength.Auto });//Standard Properties
      outerGrid.RowDefinitions.Add(new RowDefinition { Height =  GridLength.Auto });//hSpliter
      outerGrid.RowDefinitions.Add(new RowDefinition { /*1Star*/ }); //InnerGrid
      AddChild(outerGrid);

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
      innerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });//vSpliter
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

      //Loaded += TestBench_Loaded;
    }



    private void setupStandardPropertyViewer() {
      //called once XAML sets TestBench.TestControl
      standardPropertyViewer = new StandardPropertyViewer(TestControl!, innerGrid);
      Grid.SetRow(standardPropertyViewer, 1);//column = 0
      outerGrid.Children.Add(standardPropertyViewer);
      setupContainers(standardPropertyViewer.ContainerComboBox);//executes innerGrid.Children.Add(testControlContainer);
      setContainer(containerTypeEnum.GridStar);//adds TestControl to testContainer
      createAxisLines();
      createOrigineShadow();
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
      containerComboBox.Items.Add(new ComboBoxItem { Content = "Vert. StackPannel", Tag=containerTypeEnum.VStackPanel });
      containerComboBox.Items.Add(new ComboBoxItem { Content = "DockPannel", Tag=containerTypeEnum.DockPanel });
      containerComboBox.SelectedIndex = 0;
      containerComboBox.SelectionChanged += ContainerComboBox_SelectionChanged;

      standardPropertyViewer!.ContainerTraceCheckBox.Click += ContainerTraceCheckBox_Click;

      SolidColorBrush cotainerBackground = Brushes.Gray;
      gridStarContainer = new GridTraced("GridStarContainer", isTracing: true) { Background = cotainerBackground};
      gridStarContainer.RowDefinitions.Add(new RowDefinition { /*1star*/ });
      gridStarContainer.ColumnDefinitions.Add(new ColumnDefinition { /*1star*/ });

      gridAutoContainer = new GridTraced("GridAutoContainer", isTracing: true) { Background = cotainerBackground};
      gridAutoContainer.RowDefinitions.Add(new RowDefinition { /*1star*/ });
      gridAutoContainer.RowDefinitions.Add(new RowDefinition { Height =  GridLength.Auto });
      gridAutoContainer.RowDefinitions.Add(new RowDefinition { /*1star*/ });
      gridAutoContainer.ColumnDefinitions.Add(new ColumnDefinition { /*1star*/ });
      gridAutoContainer.ColumnDefinitions.Add(new ColumnDefinition { Width =  GridLength.Auto });
      gridAutoContainer.ColumnDefinitions.Add(new ColumnDefinition { /*1star*/ });

      scrollViewerContainer = new ScrollViewerTraced("ScrollViewerContainer", isTracing: true) 
        { Background = cotainerBackground};
      scrollViewerContainer.HorizontalScrollBarVisibility = ScrollBarVisibility.Visible;
      scrollViewerContainer.VerticalScrollBarVisibility = ScrollBarVisibility.Visible;
      
      canvasContainer = new CanvasTraced("CanvasContainer", isTracing: true) { Background = cotainerBackground};
      
      hStackPanelContainer = new StackPanelTraced("HStackPanelContainer", isTracing: true) 
        { Background = cotainerBackground, Orientation = Orientation.Horizontal};
      
      vStackPanelContainer = new StackPanelTraced("VStackPanelContainer", isTracing: true) 
        { Background = cotainerBackground, Orientation = Orientation.Vertical};
      
      dockPanelContainer = new DockPanelTraced("DockPanelContainer", isTracing: true) { Background = cotainerBackground};
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

      innerGrid.Children.Add(testControlContainer);//row = 0; column = 0
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

      //if (testControl==null) {
      //  if (hasHeightChanged) {
      //    TracerLib.Tracer.TraceLineFiltered("Height:{0}, Alignment:{1}, Margin:{2}, {3} Offset:{4}, AHeight: {5}", 
      //      testFrameworkElement.Height, testFrameworkElement.VerticalAlignment, testFrameworkElement.Margin.Top, testFrameworkElement.Margin.Bottom, 
      //      offsetPointUsed.Y, testFrameworkElement.ActualHeight);
      //  }
      //  if (hasWidthChanged) {
      //    TracerLib.Tracer.TraceLineFiltered("Width:{0}, Alignment:{1}, Margin:{2}, {3} Offset:{4}, AWidth: {5}", 
      //      testFrameworkElement.Width, testFrameworkElement.HorizontalAlignment, testFrameworkElement.Margin.Left, testFrameworkElement.Margin.Right, 
      //      offsetPointUsed.X, testFrameworkElement.ActualWidth);
      //  }
      //} else {
      //  if (hasHeightChanged) {
      //    TracerLib.Tracer.TraceLineFiltered("Height:{0}, Alignment:{1}, Margin:{2}, {3} Offset:{4}, AHeight: {5}", 
      //      testFrameworkElement.Height, testFrameworkElement.VerticalAlignment, testFrameworkElement.Margin.Top, testFrameworkElement.Margin.Bottom, 
      //      offsetPointUsed.Y, testFrameworkElement.ActualHeight);
      //  }
      //  if (hasWidthChanged) {
      //    TracerLib.Tracer.TraceLineFiltered("Width:{0}, Alignment:{1}, Margin:{2}, {3} Offset:{4}, AWidth: {5}", 
      //      testFrameworkElement.Width, testFrameworkElement.HorizontalAlignment, testFrameworkElement.Margin.Left, testFrameworkElement.Margin.Right, 
      //      offsetPointUsed.X, testFrameworkElement.ActualWidth);
      //  }
      //}

      foreach (AxisLine axisLine in axisLines) {
        if (axisLine!=null) {
          if ((axisLine.Dimension==DimensionEnum.width && hasWidthChanged) ||
            (/*axisLine.Dimension==DimensionEnum.height && */ hasHeightChanged))
            axisLine.UpdateLinePosition(offsetPointUsed);
        }
      }
    }


    private void updateThickness(ref Thickness thickness, Thickness newThickness, ref bool isHorizontalChange, ref bool isVerticalChange) {
      if (thickness==newThickness) return;

      isHorizontalChange = isHorizontalChange | (thickness.Top!=newThickness.Top) | (thickness.Bottom!=newThickness.Bottom);
      isVerticalChange = isVerticalChange | (thickness.Left!=newThickness.Left) | (thickness.Right!=newThickness.Right);
      thickness = newThickness;
      return;
    }
    #endregion


    #region Shadow of original position
    //      ---------------------------

    Rectangle origineShadow;


    private void createOrigineShadow() {
      origineShadow = new Rectangle {
        Fill = new SolidColorBrush(Color.FromArgb(0x80, 0xA0, 0xA0, 0xA0))
      };
      origineShadow.Fill.Freeze();
      innerGrid.Children.Add(origineShadow);//row: 0, column: 0
      //Grid.SetRow(origineShadow, testFrameworkElementGridRow);
      //Grid.SetColumn(origineShadow, testFrameworkElementGirdColumn);
      Grid.SetZIndex(origineShadow, Grid.GetZIndex(TestControl) - 1);
      TestControl!.SizeChanged += TestControl_SizeChanged;
      var horizontalAlignmentDescriptor = 
        DependencyPropertyDescriptor.FromProperty(FrameworkElement.HorizontalAlignmentProperty, typeof(FrameworkElement));
      horizontalAlignmentDescriptor.AddValueChanged(TestControl, TestControl_HorizontalAlignmentChanged);
      var verticalAlignmentDescriptor = 
        DependencyPropertyDescriptor.FromProperty(FrameworkElement.VerticalAlignmentProperty, typeof(FrameworkElement));
      verticalAlignmentDescriptor.AddValueChanged(TestControl, TestControl_VerticalAlignmentChanged);
    }

    private void TestControl_SizeChanged(object sender, SizeChangedEventArgs e) {
      updateOrigineShadowPosition();
    }

    void TestControl_HorizontalAlignmentChanged(object? sender, EventArgs args) {
      updateOrigineShadowPosition();
    }


    void TestControl_VerticalAlignmentChanged(object? sender, EventArgs args) {
      updateOrigineShadowPosition();
    }


    private void updateOrigineShadowPosition() {
      origineShadow.HorizontalAlignment = TestControl!.HorizontalAlignment;
      if (origineShadow.HorizontalAlignment==HorizontalAlignment.Stretch && double.IsNaN(TestControl.Width)) {
        origineShadow.Width = double.NaN;
      } else {
        origineShadow.Width = Math.Max(strokeThickness, TestControl.ActualWidth);
      }
      origineShadow.VerticalAlignment = TestControl.VerticalAlignment;
      if (origineShadow.VerticalAlignment==VerticalAlignment.Stretch && double.IsNaN(TestControl.Height)) {
        origineShadow.Height = double.NaN;
      } else {
        origineShadow.Height = Math.Max(strokeThickness, TestControl.ActualHeight);
      }
    }
    #endregion


    #region Testing
    //      -------

    int testFuncsIndex = 0;
    Action? testResultAction;//tests if the last testFunctionwas succesfull before next test gets executed


    const double nan = double.NaN;
    readonly static FontFamily arial = new FontFamily("Ariel");
    readonly static FontFamily couri = new FontFamily("Courier New");
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

    Brush oldBorderBrush;
    Brush oldBackground;
    Brush oldForeground;


    private void initTestFunctions() {
      bool isCo = TestControl is Control;
      TestFunctions = new List<(string Name, Func<Action?> Function)>();
      var tf = TestFunctions;
      tf.Add(("Start", () => tFunc(200, 100, hstr, hstr, vst, vst, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, arial, 12)));
      if (TestControl is Control control) {
        tf.Add(("Background", () => { oldBackground = control!.Background; control.Background = Brushes.Lavender; return null; }));
        tf.Add(("Border Color", () => { oldBorderBrush = control!.BorderBrush; control.BorderBrush = Brushes.DarkBlue; return null; }));
        tf.Add(("Foreground", () => { oldForeground = control!.Foreground; control.Foreground = Brushes.Firebrick; return null; }));
        tf.Add(("Reset Colors", () => { control!.BorderBrush = oldBorderBrush; control.Background = oldBackground; control.Foreground = oldForeground; return null; }));
      }
      //Todo: Correct spacing
      //     Width      Horizo  ntal  Verti  cal  Left              Righ  t       Top               Bott  om      Text         Font
      //         Height Contr   Conte Cntr   Cnte Mar   Bor   Pad   Pad   Bor Mar Mar   Bor   Pad   Pad   Bor Mar 
      addTest(nan, nan, hstr,   hstr, vst,   vst,  0,    0,    0,    0,    0,  0,  0,    0,    0,    0,    0,  0, arial, 12, isCo, FeT, "Start");
      addTest(nan, nan, hstr,   hstr, vst,   vst, 10,    0,    0,    0,    0,  0,  0,    0,    0,    0,    0,  0, arial, 12, isCo, FeT, "Left Margin");
      addTest(nan, nan, hstr,   hstr, vst,   vst, 10,   10,    0,    0,    0,  0,  0,    0,    0,    0,    0,  0, arial, 12, isCo, CoT, "Left Border");
      addTest(nan, nan, hstr,   hstr, vst,   vst, 10,   10,   10,    0,    0,  0,  0,    0,    0,    0,    0,  0, arial, 12, isCo, CoT, "Left Padding");
      addTest(nan, nan, hstr,   hstr, vst,   vst, 10,   10,   10,   10,    0,  0,  0,    0,    0,    0,    0,  0, arial, 12, isCo, CoT, "Right Padding");
      addTest(nan, nan, hstr,   hstr, vst,   vst, 10,   10,   10,   10,   10,  0,  0,    0,    0,    0,    0,  0, arial, 12, isCo, CoT, "Right Border");
      addTest(nan, nan, hstr,   hstr, vst,   vst, 10,   10,   10,   10,   10, 10,  0,    0,    0,    0,    0,  0, arial, 12, isCo, FeT, "Right Margin");
      addTest(nan, nan, hstr,   hstr, vst,   vst, 10,   10,   10,   10,   10, 10, 10,    0,    0,    0,    0,  0, arial, 12, isCo, FeT, "Top Margin");
      addTest(nan, nan, hstr,   hstr, vst,   vst, 10,   10,   10,   10,   10, 10, 10,   10,    0,    0,    0,  0, arial, 12, isCo, CoT, "Top Border");
      addTest(nan, nan, hstr,   hstr, vst,   vst, 10,   10,   10,   10,   10, 10, 10,   10,   10,    0,    0,  0, arial, 12, isCo, CoT, "Top Padding");
      addTest(nan, nan, hstr,   hstr, vst,   vst, 10,   10,   10,   10,   10, 10, 10,   10,   10,   10,    0,  0, arial, 12, isCo, CoT, "Bottom Padding");
      addTest(nan, nan, hstr,   hstr, vst,   vst, 10,   10,   10,   10,   10, 10, 10,   10,   10,   10,   10,  0, arial, 12, isCo, CoT, "Bottom Border");
      addTest(nan, nan, hstr,   hstr, vst,   vst, 10,   10,   10,   10,   10, 10, 10,   10,   10,   10,   10, 10, arial, 12, isCo, FeT, "Bottom Margin");
      addTest(nan, nan, left,   hstr, vst,   vst, 10,   10,   10,   10,   10, 10, 10,   10,   10,   10,   10, 10, arial, 12, isCo, FeT, "Horizontal Alignement Left");
      addTest(nan, nan, hcen,   hstr, vst,   vst, 10,   10,   10,   10,   10, 10, 10,   10,   10,   10,   10, 10, arial, 12, isCo, FeT, "Horizontal Alignement Center");
      addTest(nan, nan, rigt,   hstr, vst,   vst, 10,   10,   10,   10,   10, 10, 10,   10,   10,   10,   10, 10, arial, 12, isCo, FeT, "Horizontal Alignement Right");
      addTest(nan, nan, hstr,   hstr, top,   vst, 10,   10,   10,   10,   10, 10, 10,   10,   10,   10,   10, 10, arial, 12, isCo, FeT, "Vertical Alignement Top");
      addTest(nan, nan, hstr,   hstr, vce,   vst, 10,   10,   10,   10,   10, 10, 10,   10,   10,   10,   10, 10, arial, 12, isCo, FeT, "Vertical Alignement Center");
      addTest(nan, nan, hstr,   hstr, bot,   vst, 10,   10,   10,   10,   10, 10, 10,   10,   10,   10,   10, 10, arial, 12, isCo, FeT, "Vertical Alignement Bottom");
      addTest(nan, nan, hstr,   left, vst,   vst, 10,   10,   10,   10,   10, 10, 10,   10,   10,   10,   10, 10, arial, 12, isCo, CoT, "Horizontal Content Left");
      addTest(nan, nan, hstr,   hcen, vst,   vst, 10,   10,   10,   10,   10, 10, 10,   10,   10,   10,   10, 10, arial, 12, isCo, CoT, "Horizontal Content Center");
      addTest(nan, nan, hstr,   rigt, vst,   vst, 10,   10,   10,   10,   10, 10, 10,   10,   10,   10,   10, 10, arial, 12, isCo, CoT, "Horizontal Content Right");
      addTest(nan, nan, hstr,   hstr, vst,   top, 10,   10,   10,   10,   10, 10, 10,   10,   10,   10,   10, 10, arial, 12, isCo, CoT, "Vertical Content Top");
      addTest(nan, nan, hstr,   hstr, vst,   vce, 10,   10,   10,   10,   10, 10, 10,   10,   10,   10,   10, 10, arial, 12, isCo, CoT, "Vertical Content Center");
      addTest(nan, nan, hstr,   hstr, vst,   bot, 10,   10,   10,   10,   10, 10, 10,   10,   10,   10,   10, 10, arial, 12, isCo, CoT, "Vertical Content Bottom");
      addTest(nan, nan, hstr,   hstr, vst,   vst, 10,   10,   10,   10,   10, 10, 10,   10,   10,   10,   10, 10, arial, 14, isCo, CoT, "Hor Content Streched, Arial 14");
      addTest(nan, nan, hstr,   hstr, vst,   vst, 10,   10,   10,   10,   10, 10, 10,   10,   10,   10,   10, 10, arial, 16, isCo, CoT, "Hor Content Streched, Arial 16");
      addTest(nan, nan, hstr,   hstr, vst,   vst, 10,   10,   10,   10,   10, 10, 10,   10,   10,   10,   10, 10, arial, 18, isCo, CoT, "Hor Content Streched, Arial 18");
      addTest(nan, nan, hstr,   hstr, vst,   vst, 10,   10,   10,   10,   10, 10, 10,   10,   10,   10,   10, 10, arial, 22, isCo, CoT, "Hor Content Streched, Arial 22");
      addTest(nan, nan, hstr,   hstr, vst,   vst, 10,   10,   10,   10,   10, 10, 10,   10,   10,   10,   10, 10, arial, 26, isCo, CoT, "Hor Content Streched, Arial 26");
      addTest(nan, nan, hstr,   hstr, vst,   vst, 10,   10,   10,   10,   10, 10, 10,   10,   10,   10,   10, 10, arial, 32, isCo, CoT, "Hor Content Streched, Arial 32");
      addTest(nan, nan, hstr,   hstr, vst,   vst, 10,   10,   10,   10,   10, 10, 10,   10,   10,   10,   10, 10, arial, 40, isCo, CoT, "Hor Content Streched, Arial 40");
      addTest(nan, nan, hstr,   hstr, vst,   vst, 10,   10,   10,   10,   10, 10, 10,   10,   10,   10,   10, 10, couri, 60, isCo, CoT, "Hor Content Streched, Courier 60");
      addTest(nan, nan, hstr,   rigt, vst,   vst, 10,   10,   10,   10,   10, 10, 10,   10,   10,   10,   10, 10, arial, 12, isCo, CoT, "Hor Content Right, Arial 12");
      addTest(nan, nan, hstr,   rigt, vst,   vst, 10,   10,   10,   10,   10, 10, 10,   10,   10,   10,   10, 10, arial, 14, isCo, CoT, "Hor Content Right, Arial 14");
      addTest(nan, nan, hstr,   rigt, vst,   vst, 10,   10,   10,   10,   10, 10, 10,   10,   10,   10,   10, 10, arial, 16, isCo, CoT, "Hor Content Right, Arial 16");
      addTest(nan, nan, hstr,   rigt, vst,   vst, 10,   10,   10,   10,   10, 10, 10,   10,   10,   10,   10, 10, arial, 18, isCo, CoT, "Hor Content Right, Arial 18");
      addTest(nan, nan, hstr,   rigt, vst,   vst, 10,   10,   10,   10,   10, 10, 10,   10,   10,   10,   10, 10, arial, 22, isCo, CoT, "Hor Content Right, Arial 22");
      addTest(nan, nan, hstr,   rigt, vst,   vst, 10,   10,   10,   10,   10, 10, 10,   10,   10,   10,   10, 10, arial, 26, isCo, CoT, "Hor Content Right, Arial 26");
      addTest(nan, nan, hstr,   rigt, vst,   vst, 10,   10,   10,   10,   10, 10, 10,   10,   10,   10,   10, 10, arial, 32, isCo, CoT, "Hor Content Right, Arial 32");
      addTest(nan, nan, hstr,   rigt, vst,   vst, 10,   10,   10,   10,   10, 10, 10,   10,   10,   10,   10, 10, arial, 40, isCo, CoT, "Hor Content Right, Arial 40");
      addTest(nan, nan, hstr,   rigt, vst,   vst, 10,   10,   10,   10,   10, 10, 10,   10,   10,   10,   10, 10, couri, 60, isCo, CoT, "Hor Content Right, Courier 60");
      addTest(200, nan, hstr,   hstr, vst,   vst, 10,   10,   10,   10,   10, 10, 10,   10,   10,   10,   10, 10, arial, 12, isCo, CoT, "Width 200, Horizontal Stretched, Hor Content Streched");
      addTest(200, nan, hstr,   left, vst,   vst, 10,   10,   10,   10,   10, 10, 10,   10,   10,   10,   10, 10, arial, 12, isCo, CoT, "Width 200, Horizontal Stretched, Hor Content Left");
      addTest(200, nan, hstr,   hcen, vst,   vst, 10,   10,   10,   10,   10, 10, 10,   10,   10,   10,   10, 10, arial, 12, isCo, CoT, "Width 200, Horizontal Stretched, Hor Content Center");
      addTest(200, nan, hstr,   rigt, vst,   vst, 10,   10,   10,   10,   10, 10, 10,   10,   10,   10,   10, 10, arial, 12, isCo, CoT, "Width 200, Horizontal Stretched, Hor Content Right");
      addTest(200, nan, left,   hstr, vst,   vst, 10,   10,   10,   10,   10, 10, 10,   10,   10,   10,   10, 10, arial, 12, isCo, CoT, "Width 200, Horizontal Left, Hor Content Streched");
      addTest(200, nan, left,   left, vst,   vst, 10,   10,   10,   10,   10, 10, 10,   10,   10,   10,   10, 10, arial, 12, isCo, CoT, "Width 200, Horizontal Left, Hor Content Left");
      addTest(200, nan, left,   hcen, vst,   vst, 10,   10,   10,   10,   10, 10, 10,   10,   10,   10,   10, 10, arial, 12, isCo, CoT, "Width 200, Horizontal Left, Hor Content Center");
      addTest(200, nan, left,   rigt, vst,   vst, 10,   10,   10,   10,   10, 10, 10,   10,   10,   10,   10, 10, arial, 12, isCo, CoT, "Width 200, Horizontal Left, Hor Content Right");
      addTest(200, nan, hcen,   hstr, vst,   vst, 10,   10,   10,   10,   10, 10, 10,   10,   10,   10,   10, 10, arial, 12, isCo, CoT, "Width 200, Horizontal Center, Hor Content Streched");
      addTest(200, nan, hcen,   left, vst,   vst, 10,   10,   10,   10,   10, 10, 10,   10,   10,   10,   10, 10, arial, 12, isCo, CoT, "Width 200, Horizontal Center, Hor Content Left");
      addTest(200, nan, hcen,   hcen, vst,   vst, 10,   10,   10,   10,   10, 10, 10,   10,   10,   10,   10, 10, arial, 12, isCo, CoT, "Width 200, Horizontal Center, Hor Content Center");
      addTest(200, nan, hcen,   rigt, vst,   vst, 10,   10,   10,   10,   10, 10, 10,   10,   10,   10,   10, 10, arial, 12, isCo, CoT, "Width 200, Horizontal Center, Hor Content Right");
      addTest(200, nan, rigt,   hstr, vst,   vst, 10,   10,   10,   10,   10, 10, 10,   10,   10,   10,   10, 10, arial, 12, isCo, CoT, "Width 200, Horizontal Right, Hor Content Streched");
      addTest(200, nan, rigt,   left, vst,   vst, 10,   10,   10,   10,   10, 10, 10,   10,   10,   10,   10, 10, arial, 12, isCo, CoT, "Width 200, Horizontal Right, Hor Content Left");
      addTest(200, nan, rigt,   hcen, vst,   vst, 10,   10,   10,   10,   10, 10, 10,   10,   10,   10,   10, 10, arial, 12, isCo, CoT, "Width 200, Horizontal Right, Hor Content Center");
      addTest(200, nan, rigt,   rigt, vst,   vst, 10,   10,   10,   10,   10, 10, 10,   10,   10,   10,   10, 10, arial, 12, isCo, CoT, "Width 200, Horizontal Right, Hor Content Right");
      addTest(nan, 100, hstr,   hstr, vst,   vst, 10,   10,   10,   10,   10, 10, 10,   10,   10,   10,   10, 10, arial, 12, isCo, CoT, "Height 100, Vertical Stretched, Ver Content Stretched");
      addTest(nan, 100, hstr,   hstr, vst,   top, 10,   10,   10,   10,   10, 10, 10,   10,   10,   10,   10, 10, arial, 12, isCo, CoT, "Height 100, Vertical Stretched, Ver Content Left");
      addTest(nan, 100, hstr,   hstr, vst,   vce, 10,   10,   10,   10,   10, 10, 10,   10,   10,   10,   10, 10, arial, 12, isCo, CoT, "Height 100, Vertical Stretched, Ver Content Center");
      addTest(nan, 100, hstr,   hstr, vst,   bot, 10,   10,   10,   10,   10, 10, 10,   10,   10,   10,   10, 10, arial, 12, isCo, CoT, "Height 100, Vertical Stretched, Ver Content Right");
      addTest(nan, 100, hstr,   hstr, top,   vst, 10,   10,   10,   10,   10, 10, 10,   10,   10,   10,   10, 10, arial, 12, isCo, CoT, "Height 100, Vertical Top, Ver Content Stretched");
      addTest(nan, 100, hstr,   hstr, top,   top, 10,   10,   10,   10,   10, 10, 10,   10,   10,   10,   10, 10, arial, 12, isCo, CoT, "Height 100, Vertical Top, Ver Content Left");
      addTest(nan, 100, hstr,   hstr, top,   vce, 10,   10,   10,   10,   10, 10, 10,   10,   10,   10,   10, 10, arial, 12, isCo, CoT, "Height 100, Vertical Top, Ver Content Center");
      addTest(nan, 100, hstr,   hstr, top,   bot, 10,   10,   10,   10,   10, 10, 10,   10,   10,   10,   10, 10, arial, 12, isCo, CoT, "Height 100, Vertical Top, Ver Content Right");
      addTest(nan, 100, hstr,   hstr, vce,   vst, 10,   10,   10,   10,   10, 10, 10,   10,   10,   10,   10, 10, arial, 12, isCo, CoT, "Height 100, Vertical Center, Ver Content Stretched");
      addTest(nan, 100, hstr,   hstr, vce,   top, 10,   10,   10,   10,   10, 10, 10,   10,   10,   10,   10, 10, arial, 12, isCo, CoT, "Height 100, Vertical Center, Ver Content Left");
      addTest(nan, 100, hstr,   hstr, vce,   vce, 10,   10,   10,   10,   10, 10, 10,   10,   10,   10,   10, 10, arial, 12, isCo, CoT, "Height 100, Vertical Center, Ver Content Center");
      addTest(nan, 100, hstr,   hstr, vce,   bot, 10,   10,   10,   10,   10, 10, 10,   10,   10,   10,   10, 10, arial, 12, isCo, CoT, "Height 100, Vertical Center, Ver Content Right");
      addTest(nan, 100, hstr,   hstr, bot,   vst, 10,   10,   10,   10,   10, 10, 10,   10,   10,   10,   10, 10, arial, 12, isCo, CoT, "Height 100, Vertical Center, Ver Content Stretched");
      addTest(nan, 100, hstr,   hstr, bot,   top, 10,   10,   10,   10,   10, 10, 10,   10,   10,   10,   10, 10, arial, 12, isCo, CoT, "Height 100, Vertical Center, Ver Content Left");
      addTest(nan, 100, hstr,   hstr, bot,   vce, 10,   10,   10,   10,   10, 10, 10,   10,   10,   10,   10, 10, arial, 12, isCo, CoT, "Height 100, Vertical Center, Ver Content Center");
      addTest(nan, 100, hstr,   hstr, bot,   bot, 10,   10,   10,   10,   10, 10, 10,   10,   10,   10,   10, 10, arial, 12, isCo, CoT, "Height 100, Vertical Center, Ver Content Right");
      addTest(200, 100, hstr,   hstr, vst,   vst, 10,   10,   10,   10,   10, 10, 10,   10,   10,   10,   10, 10, arial, 12, isCo, CoT, "Width 200, Height 200, All Stretched");
      standardPropertyViewer.NextTestButton.Click += new RoutedEventHandler(nextTestButton_Click);
    }


    private void addTest(
      double width,
      double height,
      HorizontalAlignment horizontalControlAlignment,
      HorizontalAlignment horizontaControllAlignment,
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
        horizontaControllAlignment,
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
      HorizontalAlignment horizontaControllAlignment,
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
        control.HorizontalContentAlignment = horizontaControllAlignment;
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

      //if (previousWidth==double.MinValue) {
      //  //save parameters before changing them with the first test
      //  savePreviousParameters();
      //} else if (testFuncsIndex==0 && previousWidth!=double.MinValue) {
      //  restorePreviousParameters();
      //}

      //execute next test
      var testInfo = TestFunctions[testFuncsIndex++];
      if (!string.IsNullOrEmpty(testInfo.Name)) {
        Tracer.TraceLine("Test: " + testInfo.Name);
      }
      testResultAction = TestFunctions[testFuncsIndex++].Function();
      if (testFuncsIndex>=TestFunctions.Count) {
        testFuncsIndex = 0;
      }
      standardPropertyViewer.NextTestButton.Content = "_Next Test " + testFuncsIndex;
    }


    double previousWidth = double.MinValue;
    double previousHeight;
    HorizontalAlignment previousHorizontalControlAlignment;
    HorizontalAlignment previousHorizontaControllAlignment;
    VerticalAlignment previousVerticalControlAlignment;
    VerticalAlignment previousVerticalContentAlignment;
    Thickness previousMargin;
    Thickness previousBorderThickness;
    Thickness previousPadding;
    FontFamily previousFontFamily;
    double previousFontSize;


    private void savePreviousParameters() {
      previousWidth = TestControl!.Width;
      previousHeight = TestControl.Height;
      previousHorizontalControlAlignment = TestControl.HorizontalAlignment;
      previousVerticalControlAlignment = TestControl.VerticalAlignment;
      previousMargin = TestControl.Margin;
      if (TestControl is Control control) {
        previousHorizontaControllAlignment = control.HorizontalContentAlignment;
        previousVerticalContentAlignment = control.VerticalContentAlignment;
        previousBorderThickness = control.BorderThickness;
        previousPadding = control.Padding;
        previousFontFamily = control.FontFamily;
        previousFontSize = control.FontSize;
      }
    }


    private void restorePreviousParameters() {
      TestControl!.Width = previousWidth;
      TestControl.Height = previousHeight;
      TestControl.HorizontalAlignment = previousHorizontalControlAlignment;
      TestControl.VerticalAlignment = previousVerticalControlAlignment;
      TestControl.Margin = previousMargin;
      if (TestControl is Control control) {
        control.HorizontalContentAlignment = previousHorizontaControllAlignment;
        control.VerticalContentAlignment = previousVerticalContentAlignment;
        control.BorderThickness = previousBorderThickness;
        control.Padding = previousPadding;
        control.FontFamily = previousFontFamily;
        control.FontSize = previousFontSize;
      }
    }

    #endregion


    #region Layouting
    //      ---------

    protected override Size MeasureContentOverride(Size constraint) {
      outerGrid.Measure(constraint);
      var width = double.IsInfinity(constraint.Width) ? outerGrid.DesiredSize.Width : constraint.Width;
      var height = double.IsInfinity(constraint.Height) ? outerGrid.DesiredSize.Height : constraint.Height;
      return new Size(width, height);
    }


    protected override Size ArrangeContentOverride(Rect arrangeRect) {
      outerGrid.Arrange(arrangeRect);
      return arrangeRect.Size;
    }
    #endregion
  }
}
