using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;


namespace WpfTestbench {

  /// <summary>
  /// Helper class to allow using ScrollViewerTraced constructor with a parameter, which is used to call 
  /// TraceCreateStart().
  /// </summary>
  public class ScrollViewerWithConstructor: ScrollViewer {
    public ScrollViewerWithConstructor(object? _) : base() { }
  }


  /// <summary>
  /// ScrollViewer with event tracing for TestBench.
  /// </summary>
  public class ScrollViewerTraced: ScrollViewerWithConstructor, IIsTracing {
    #region Property
    //      --------

    /// <summary>
    /// Name to be used for tracing
    /// </summary>
    public string TraceName { get; private set; }


    /// <summary>
    /// Controls if trace should get written
    /// </summary>
    public bool IsTracing { get; set; } = true;
    #endregion


    #region Constructor
    //      -----------

    /// <summary>
    /// Default Constructor
    /// </summary>
    public ScrollViewerTraced() : this("ScrollViewer", true) { }


    /// <summary>
    /// Constructor supporting tracing of multiple ScrollViewers with different names
    /// </summary>
    public ScrollViewerTraced(string traceName, bool isTracing) : base(TraceWPFEvents.TraceCreateStart(traceName, isTracing)) {
      TraceName = traceName;
      IsTracing = isTracing;
      TraceWPFEvents.TraceCreateEnd(traceName, isTracing);
    }
    #endregion


    #region Event Tracing
    //      -------------

    protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e) {
      TraceWPFEvents.OnPropertyChanged(this, e, base.OnPropertyChanged, IsTracing);
    }


    protected override Size MeasureOverride(Size constraint) {
      return TraceWPFEvents.MeasureOverride(this, constraint, base.MeasureOverride, IsTracing);
    }


    protected override Size ArrangeOverride(Size finalSize) {
      return TraceWPFEvents.ArrangeOverride(this, finalSize, base.ArrangeOverride, IsTracing);
    }


    protected override void OnRender(DrawingContext drawingContext) {
      TraceWPFEvents.OnRender(this, drawingContext, base.OnRender, IsTracing);
    }
    #endregion
  }
}