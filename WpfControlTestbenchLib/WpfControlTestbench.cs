using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows;


namespace WpfTestbench {
  public class WpfControlTestbench: ContentControl {

    #region Properties
    //      ----------

    /// <summary>
    /// Control inheriting from FrameworkElement to be inspected
    /// </summary>
    public FrameworkElement TestFrameworkElement {
      get { return (FrameworkElement)GetValue(TestFrameworkElementProperty); }
      set { SetValue(TestFrameworkElementProperty, value); }
    }

    // Using a DependencyProperty as the backing store for TestFrameworkElement.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty TestFrameworkElementProperty = 
    DependencyProperty.Register("TestFrameworkElement", typeof(FrameworkElement), typeof(WpfControlTestbench), new UIPropertyMetadata(null));
    #endregion


    #region Constructor
    //      -----------

    static WpfControlTestbench() {
      DefaultStyleKeyProperty.OverrideMetadata(typeof(WpfControlTestbench), new FrameworkPropertyMetadata(typeof(WpfControlTestbench)));
    }


    public WpfControlTestbench() {
    }
    #endregion
  }
}
