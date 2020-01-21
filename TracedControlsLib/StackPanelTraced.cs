using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Media;


namespace WpfTestbench {


  /// <summary>
  /// Helper class to allow using Panel constructor with a paramter.
  /// </summary>
  public class StackPanelWithConstructor: StackPanel {
    public StackPanelWithConstructor(object dummy):base(){}
  }
  

  public class StackPanelTraced: StackPanelWithConstructor, ITraceName  {

    /// <summary>
    /// Name to be used for tracing
    /// </summary>
    public string TraceName { get; private set; }


    #region Constructor
    //      -----------

    /// <summary>
    /// Default Constructor
    /// </summary>
    public StackPanelTraced(): this("StackPanel") {}


    /// <summary>
    /// Constructor supporting tracing of multiple Rectangles with different names
    /// </summary>
    public StackPanelTraced(string traceName): base(TraceWPFEvents.TraceCreateStart(traceName)) {
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
