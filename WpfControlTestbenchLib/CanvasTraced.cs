/********************************************************************************************************

WpfTestbench.CanvasTraced
=========================

Used to place the TestControl in a Canvas which supports event tracing

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