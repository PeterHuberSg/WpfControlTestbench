/********************************************************************************************************

WpfTestbench.ControlWindow
==========================

Test window for ControlTraced.

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
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;


namespace WpfTestbench {

  /// <summary>
  /// Interaction logic for ControlWindow.xaml
  /// </summary>
  /// 
  public partial class ControlWindow: Window {

    /// <summary>
    /// Creates and opens a new ControlWindow
    /// </summary>
    public static void Show(Window ownerWindow) {
      var newWindow = new ControlWindow {
        Owner = ownerWindow
      };
      newWindow.Show();
    }


    readonly Brush? resetFill;


    public ControlWindow() {
      InitializeComponent();

      Width = (int)System.Windows.SystemParameters.PrimaryScreenWidth*4/5;
      Height = (int)System.Windows.SystemParameters.PrimaryScreenHeight*4/5;

      WpfBinding.Setup(TextTextBox, "Text", TestControlTraced,
        ControlTraced.TextProperty, BindingMode.TwoWay);

      FillStandardColorComboBox.SetSelectedBrush(TestControlTraced.Fill??Brushes.Transparent);
      WpfBinding.Setup(FillStandardColorComboBox, "SelectedColorBrush", TestControlTraced,
        ControlTraced.FillProperty, BindingMode.TwoWay);

      TestBench.TestFunctions.Add(("Green Fill", fillGreen));
      TestBench.TestFunctions.Add(("Red Fill", ()=>{ TestControlTraced.Fill = Brushes.Red; return null;}));
      TestBench.TestFunctions.Add(("Ratio", testRatio));
      TestBench.TestFunctions.Add(("Reset Properties", resetProperties));

      resetFill = TestControlTraced.Fill;
      TestBench.ResetAction = () => TestControlTraced.Fill = resetFill;
    }


    Brush? oldFill;
    double oldWidth;


    private Action? fillGreen() {
      oldFill = TestControlTraced.Fill;
      TestControlTraced.Fill = Brushes.LightGreen; 
      return null; ;
    }


    private Action? testRatio() {
      oldWidth = TestControlTraced.Width;
      TestControlTraced.Width = 200;
      return verifyRation;
    }


    private void verifyRation() {
      if (double.IsNaN(TestControlTraced.Width)) return;

      if (TestControlTraced.ActualWidth==TestControlTraced.Width) {
        throw new InvalidOperationException($"Actual width should be {TestControlTraced.Width} " +
          $"but was {TestControlTraced.ActualWidth}.");
      }
    }

    private Action? resetProperties() {
      TestControlTraced.Fill = oldFill;
      TestControlTraced.Width = oldWidth;
      return null; ;
    }
  }
}
