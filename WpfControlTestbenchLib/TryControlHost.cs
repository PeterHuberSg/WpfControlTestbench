using CustomControlBaseLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;

namespace WpfTestbench {

  //[ContentProperty("Children")]
  public class TryControlHost: CustomControlBase {
    #region Properties
    //      ----------

    /// <summary>
    /// A control inheriting from FrameworkElement to be inspected in WpfControlTestbench
    /// </summary>
    public FrameworkElement? TestControl {
      get { return (FrameworkElement)GetValue(TestControlProperty); }
      set { SetValue(TestControlProperty, value); }
    }
    /// <summary>
    /// Dependency Property definition for TestControl
    /// </summary>
    public static readonly DependencyProperty TestControlProperty =
    DependencyProperty.Register("TestControl", typeof(FrameworkElement), typeof(TryControlHost), 
      new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender,
      TestControlChanged));

    private static void TestControlChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
      if (d is TryControlHost host) {
        if (host.testControlContainer is Grid grid) {
          if (e.OldValue is FrameworkElement oldControl) {
            grid.Children.Remove(oldControl);
          }
          if (e.NewValue is FrameworkElement newControl) {
            grid.Children.Add(newControl);
          }
        }
      }
    }


    ///// <summary>
    ///// (Rich)Textbox displaying trace in WpfControlTestbench. Among others, accessing it through this property allows to add a 
    ///// special trace filter.
    ///// </summary>
    //public WpfTraceViewer Part_WpfTraceViewer { get; private set; }
    #endregion


    #region Constructor
    //      -----------

    readonly FrameworkElement testControlContainer;


    public TryControlHost() {
      Background = Brushes.LightYellow;
      testControlContainer = new Grid {Background=Brushes.Transparent};
      AddChild(testControlContainer);
    }
    #endregion


    protected override Size MeasureContentOverride(Size constraint) {
      testControlContainer.Measure(constraint);
      return constraint;
    }

    protected override Size ArrangeContentOverride(Rect arrangeRect) {
      testControlContainer.Arrange(arrangeRect);
      return testControlContainer.DesiredSize;
    }



  }
}
