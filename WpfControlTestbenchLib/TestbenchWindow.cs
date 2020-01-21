using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;
using TracerLib;


namespace WpfTestbench {
  public class TestbenchWindow: Window {

    protected static void ShowProtected(Func<Window> createWindow, Window ownerWindow) {
      Window newWindow = createWindow();
      newWindow.Owner = ownerWindow;
      //newWindow.Width = (System.Windows.SystemParameters.PrimaryScreenWidth-newWindow.Left)*0.95;
      newWindow.Loaded += new RoutedEventHandler(testbenchWindow_Loaded);
      newWindow.Closed += new EventHandler(newWindow_Closed);
      
      newWindow.Show();
    }


    //protected static void ShowProtected(Window newWindow, Window ownerWindow) {
    //  newWindow.Owner = ownerWindow;
    //  newWindow.Loaded += new RoutedEventHandler(testbenchWindow_Loaded);
    //  newWindow.Closed += new EventHandler(newWindow_Closed);

    //  newWindow.Show();
    //}

    
    static void testbenchWindow_Loaded(object sender, RoutedEventArgs e) {
      //Some properties get overwritten by templates after construction. Change them here instead in ShowProtected
      TestbenchWindow testbenchWindow = (TestbenchWindow) sender;
      testbenchWindow.Background = Brushes.Gainsboro;
    }


    bool isFirstTime = true;


    protected override Size MeasureOverride(Size availableSize) {
      if (isFirstTime) {
        isFirstTime = false;
        Width = (System.Windows.SystemParameters.PrimaryScreenWidth-Left)*0.95;
        return base.MeasureOverride(new Size(Width, availableSize.Height));
      } else {
        return base.MeasureOverride(availableSize);
      }
    }


    static void newWindow_Closed(object? sender, EventArgs e) {
      TestbenchWindow testbenchWindow = (TestbenchWindow)sender!;
      if (testbenchWindow.Owner!=null) {
        testbenchWindow.Owner.Activate();
      }
    }

  }
}
