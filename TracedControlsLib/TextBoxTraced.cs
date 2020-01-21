using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;


namespace WpfTestbench {


  /// <summary>
  /// Helper class to allow using Panel constructor with a paramter.
  /// </summary>
  public class TextBoxWithConstructor: TextBox {
    public TextBoxWithConstructor(object dummy):base(){}
  }
  

  public class TextBoxTraced: TextBoxWithConstructor, ITraceName {

    /// <summary>
    /// Name to be used for tracing
    /// </summary>
    public string TraceName { get; private set; }

    
    #region Constructor
    //      -----------

    /// <summary>
    /// Default Constructor
    /// </summary>
    public TextBoxTraced(): this("TextBox") {}

    /// <summary>
    /// Constructor supporting tracing of multiple Rectangles with different names
    /// </summary>
    public TextBoxTraced(string traceName): base(TraceWPFEvents.TraceCreateStart(traceName)) {
      TraceName = traceName;
      TraceWPFEvents.TraceCreateEnd(traceName);
    }
    #endregion


    #region Event Tracing
    //      -------------

    protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e) {
      TraceWPFEvents.OnPropertyChanged(this, e, base.OnPropertyChanged);
    }


    protected override Size MeasureOverride(Size constraint) {
      return TraceWPFEvents.MeasureOverride(this, constraint, base.MeasureOverride);
    }

    
    protected override Size ArrangeOverride(Size finalSize) {
      return TraceWPFEvents.ArrangeOverride(this, finalSize, base.ArrangeOverride);
    }

    
    protected override void OnRender(DrawingContext drawingContext) {
      TraceWPFEvents.OnRender(this, drawingContext, base.OnRender);
    }
    #endregion
  }
}