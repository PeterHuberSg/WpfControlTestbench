/**************************************************************************************

WpfTestbench.StackPanelTraced
=============================

StackPanel with event tracing for TestBench.

Written 2014 - 2020 by Jürgpeter Huber 
Contact: PeterCode at Peterbox dot com

To the extent possible under law, the author(s) have dedicated all copyright and 
related and neighboring rights to this software to the public domain worldwide under
the Creative Commons 0 license (details see COPYING.txt file, see also
<http://creativecommons.org/publicdomain/zero/1.0/>). 

This software is distributed without any warranty. 
**************************************************************************************/
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;


namespace WpfTestbench {


  /// <summary>
  /// Helper class to allow using Panel constructor with a parameter.
  /// </summary>
  public class StackPanelWithConstructor: StackPanel {
    public StackPanelWithConstructor(object _):base(){}
  }


  /// <summary>
  /// StackPanel with event tracing for TestBench.
  /// </summary>
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
