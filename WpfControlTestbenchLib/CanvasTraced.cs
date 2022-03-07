using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;


namespace WpfTestbench {


  /// <summary>
  /// Helper class to allow using CanvasTraced constructor with a parameter, which is used to call 
  /// TraceCreateStart().
  /// </summary>
  public class CanvasWithConstructor: Canvas {
    public CanvasWithConstructor(object? _) : base() { }
  }


  /// <summary>
  /// Canvas with event tracing for TestBench.
  /// </summary>
  public class CanvasTraced: CanvasWithConstructor, IIsTracing {
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
    public CanvasTraced() : this("Canvas", true) { }


    /// <summary>
    /// Constructor supporting tracing of multiple Canvases with different names
    /// </summary>
    public CanvasTraced(string traceName, bool isTracing) : base(TraceWPFEvents.TraceCreateStart(traceName, isTracing)) {
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