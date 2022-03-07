/**************************************************************************************

WpfTestbench.TestbenchWindow
============================

 Base class for WPF windows containing WpfControlTestbench.

Written 2014 - 2020 by Jürgpeter Huber 
Contact: PeterCode at Peterbox dot com

To the extent possible under law, the author(s) have dedicated all copyright and 
related and neighboring rights to this software to the public domain worldwide under
the Creative Commons 0 license (details see COPYING.txt file, see also
<http://creativecommons.org/publicdomain/zero/1.0/>). 

This software is distributed without any warranty. 
**************************************************************************************/
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using CustomControlBaseLib;
using TracerLib;


namespace WpfTestbench {

  /// <summary>
  /// Base class for WPF windows containing WpfControlTestbench. The goal is that a new testbench window can be created for 
  /// a FrameworkElement with as little effort as possible. The TestbenchWindow provides the following functions:<para/>
  /// ShowProtected, a static method, which allows easy creation of 
  /// </summary>
  public class TestbenchWindow: Window {

    /// <summary>
    /// Creates a new window inherited from TestbenchWindow and sets its owner. Closing a TestbenchWindow
    /// will activate its owner window. Func&lt;Window&gt; is a delegate creating the child window. Code sample:<para/>
    /// <example>
    /// public static void Show(Window ownerWindow) {<para/>
    ///    ShowProtected( () => new TestbenchWindowInherited(), MainWindow);<para/>
    /// }<para/>
    /// </example>
    /// </summary>
    protected static void ShowProtected(Func<Window> createWindow, Window ownerWindow) {
      Window newWindow = createWindow();
      newWindow.Owner = ownerWindow;
      
      newWindow.Show();
    }


    /// <summary>
    /// Default constructor
    /// </summary>
    public TestbenchWindow() {
      Loaded +=new RoutedEventHandler(testbenchWindow_Loaded);
      Closed += new EventHandler(TestbenchWindow_Closed);
    }


    /// <summary>
    /// Defines which test functions should get executed when user clicks on Next Test button. TestbenchWindow calls it 
    /// in its load event. Inheritors of TestbenchWindow should overwrite it if they want to define tests.<para/>
    /// InitTestFuncs returns an array with the test functions. A test function sets some properties on the test object. The 
    /// test function returns a test action which verifies that the test object has responded properly to the changed 
    /// inputs. Since it is not easy to find out when the WPF has processed the changes caused by the test function (layout 
    /// happens after WPF returns from the test function), the test action gets executed before the user initiates the next test.<para/>
    /// See code sample in TestbenchWindow.InitTestFuncs()
    /// </summary>
    /// <example><![CDATA[
    /// protected override Func<Action>[] InitTestFuncs() {
    ///    return new Func<Action>[] {
    ///    () => testFunc(1, 2, 3),
    ///    () => testFunc(4, 5, 9),
    ///   };
    /// }
    /// Action testFunc(int testInput1, int testInput2, int result) {
    ///   TestFrameworkElement.Property1 = testInput1;
    ///   TestFrameworkElement.Property2 = testInput2;
    ///
    ///   //test to be executed before next values get applies
    ///   return () => { 
    ///     if (TestFrameworkElement.Property3!=result) throw new Exception(" +  should be .");
    ///   };
    /// }
    /// ]]></example>
    protected virtual Func<Action?>[]? InitTestFuncs() {
      return null;
    }


    Button nextTestButton;
    CustomControlBase? customControl;


    void testbenchWindow_Loaded(object sender, RoutedEventArgs e) {
      Background = Brushes.Gainsboro;

      //setup test functions
      testFuncs = InitTestFuncs();
      WpfControlTestbench wpfControlTestbench = (WpfControlTestbench)Content!;
      customControl = wpfControlTestbench.TestFrameworkElement as CustomControlBase;
      if (customControl!=null) {
        Func<Action?>[] customControlTests = initCustomControlFuncs();
        if (testFuncs==null) {
          firstcustomControlTestIndex = 0;
          testFuncs = customControlTests;
        } else {
          firstcustomControlTestIndex = testFuncs.Length;
          testFuncs = testFuncs.Concat(customControlTests).ToArray();
        }
      }

      var controlPropertyViewer = wpfControlTestbench.Template.FindName("PART_StandardPropertyViewer", wpfControlTestbench) as StandardPropertyViewer;
      nextTestButton = controlPropertyViewer!.NextTestButton;
      if (testFuncs==null) {
        nextTestButton.IsEnabled = false;
      } else {
        nextTestButton.Click += new RoutedEventHandler(nextTestButton_Click);
      }
    }


    Func<Action?>[]? testFuncs;
    int testFuncsIndex = 0;
    Action? testAction;
    int firstcustomControlTestIndex;


    const double nan = double.NaN;
    readonly static FontFamily arial = new FontFamily("Ariel");
    readonly static FontFamily couri = new FontFamily("Courier New");
    const HorizontalAlignment hstr = HorizontalAlignment.Stretch;
    const HorizontalAlignment left = HorizontalAlignment.Left;
    const HorizontalAlignment hcen = HorizontalAlignment.Center;
    const HorizontalAlignment rigt = HorizontalAlignment.Right;
    const VerticalAlignment vst = VerticalAlignment.Stretch;
    const VerticalAlignment top = VerticalAlignment.Top;
    const VerticalAlignment vce = VerticalAlignment.Center;
    const VerticalAlignment bot = VerticalAlignment.Bottom;


    Brush oldBorderBrush;
    Brush oldBackground;
    Brush oldForeground;


    private Func<Action?>[] initCustomControlFuncs() {
      return new Func<Action?>[] {
        () => testFunc(200, 100, hstr, hstr, vst, vst, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, arial, 12),
        () =>{oldBorderBrush = customControl!.BorderBrush; customControl.BorderBrush = Brushes.DarkBlue; return null;},
        () =>{oldBackground = customControl!.Background; customControl.Background = Brushes.Lavender; return null;},
        () =>{oldForeground = customControl!.Foreground; customControl.Foreground = Brushes.Firebrick; return null;},
        () =>{customControl!.BorderBrush = oldBorderBrush; customControl.Background = oldBackground; customControl.Foreground = oldForeground; return null;},

        //            Height     Horizontal  Vertical  Left        Right       Top         Bottom      Text         Font
        //                Width  Contr Conte Cntr Cnte Mar Bor Pad Pad Bor Mar Mar Bor Pad Pad Bor Mar 
        () => testFunc(nan, nan, hstr, hstr, vst, vst,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0, arial, 12),
        () => testFunc(nan, nan, hstr, hstr, vst, vst, 10,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0, arial, 12),
        () => testFunc(nan, nan, hstr, hstr, vst, vst, 10, 10,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0, arial, 12),
        () => testFunc(nan, nan, hstr, hstr, vst, vst, 10, 10, 10,  0,  0,  0,  0,  0,  0,  0,  0,  0, arial, 12),
        () => testFunc(nan, nan, hstr, hstr, vst, vst, 10, 10, 10, 10,  0,  0,  0,  0,  0,  0,  0,  0, arial, 12),
        () => testFunc(nan, nan, hstr, hstr, vst, vst, 10, 10, 10, 10, 10,  0,  0,  0,  0,  0,  0,  0, arial, 12),
        () => testFunc(nan, nan, hstr, hstr, vst, vst, 10, 10, 10, 10, 10, 10,  0,  0,  0,  0,  0,  0, arial, 12),
        () => testFunc(nan, nan, hstr, hstr, vst, vst, 10, 10, 10, 10, 10, 10, 10,  0,  0,  0,  0,  0, arial, 12),
        () => testFunc(nan, nan, hstr, hstr, vst, vst, 10, 10, 10, 10, 10, 10, 10, 10,  0,  0,  0,  0, arial, 12),
        () => testFunc(nan, nan, hstr, hstr, vst, vst, 10, 10, 10, 10, 10, 10, 10, 10, 10,  0,  0,  0, arial, 12),
        () => testFunc(nan, nan, hstr, hstr, vst, vst, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10,  0,  0, arial, 12),
        () => testFunc(nan, nan, hstr, hstr, vst, vst, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10,  0, arial, 12),
        () => testFunc(nan, nan, hstr, hstr, vst, vst, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, arial, 12),
        () => testFunc(nan, nan, left, hstr, vst, vst, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, arial, 12),
        () => testFunc(nan, nan, hcen, hstr, vst, vst, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, arial, 12),
        () => testFunc(nan, nan, rigt, hstr, vst, vst, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, arial, 12),
        () => testFunc(nan, nan, hstr, hstr, top, vst, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, arial, 12),
        () => testFunc(nan, nan, hstr, hstr, vce, vst, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, arial, 12),
        () => testFunc(nan, nan, hstr, hstr, bot, vst, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, arial, 12),
        () => testFunc(nan, nan, hstr, left, vst, vst, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, arial, 12),
        () => testFunc(nan, nan, hstr, hcen, vst, vst, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, arial, 12),
        () => testFunc(nan, nan, hstr, rigt, vst, vst, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, arial, 12),
        () => testFunc(nan, nan, hstr, hstr, vst, top, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, arial, 12),
        () => testFunc(nan, nan, hstr, hstr, vst, vce, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, arial, 12),
        () => testFunc(nan, nan, hstr, hstr, vst, bot, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, arial, 12),
        () => testFunc(nan, nan, hstr, hstr, vst, vst, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, arial, 14),
        () => testFunc(nan, nan, hstr, hstr, vst, vst, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, arial, 16),
        () => testFunc(nan, nan, hstr, hstr, vst, vst, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, arial, 18),
        () => testFunc(nan, nan, hstr, hstr, vst, vst, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, arial, 22),
        () => testFunc(nan, nan, hstr, hstr, vst, vst, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, arial, 26),
        () => testFunc(nan, nan, hstr, hstr, vst, vst, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, arial, 32),
        () => testFunc(nan, nan, hstr, hstr, vst, vst, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, arial, 40),
        () => testFunc(nan, nan, hstr, hstr, vst, vst, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, couri, 60),
        () => testFunc(nan, nan, hstr, rigt, vst, vst, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, arial, 12),
        () => testFunc(nan, nan, hstr, rigt, vst, vst, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, arial, 14),
        () => testFunc(nan, nan, hstr, rigt, vst, vst, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, arial, 16),
        () => testFunc(nan, nan, hstr, rigt, vst, vst, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, arial, 18),
        () => testFunc(nan, nan, hstr, rigt, vst, vst, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, arial, 22),
        () => testFunc(nan, nan, hstr, rigt, vst, vst, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, arial, 26),
        () => testFunc(nan, nan, hstr, rigt, vst, vst, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, arial, 32),
        () => testFunc(nan, nan, hstr, rigt, vst, vst, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, arial, 40),
        () => testFunc(nan, nan, hstr, rigt, vst, vst, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, couri, 60),
        () => testFunc(200, nan, hstr, hstr, vst, vst, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, arial, 12),
        () => testFunc(200, nan, hstr, left, vst, vst, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, arial, 12),
        () => testFunc(200, nan, hstr, hcen, vst, vst, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, arial, 12),
        () => testFunc(200, nan, hstr, rigt, vst, vst, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, arial, 12),
        () => testFunc(200, nan, left, hstr, vst, vst, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, arial, 12),
        () => testFunc(200, nan, left, left, vst, vst, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, arial, 12),
        () => testFunc(200, nan, left, hcen, vst, vst, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, arial, 12),
        () => testFunc(200, nan, left, rigt, vst, vst, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, arial, 12),
        () => testFunc(200, nan, hcen, hstr, vst, vst, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, arial, 12),
        () => testFunc(200, nan, hcen, left, vst, vst, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, arial, 12),
        () => testFunc(200, nan, hcen, hcen, vst, vst, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, arial, 12),
        () => testFunc(200, nan, hcen, rigt, vst, vst, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, arial, 12),
        () => testFunc(200, nan, rigt, hstr, vst, vst, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, arial, 12),
        () => testFunc(200, nan, rigt, left, vst, vst, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, arial, 12),
        () => testFunc(200, nan, rigt, hcen, vst, vst, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, arial, 12),
        () => testFunc(200, nan, rigt, rigt, vst, vst, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, arial, 12),
        () => testFunc(nan, 100, hstr, hstr, vst, vst, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, arial, 12),
        () => testFunc(nan, 100, hstr, hstr, vst, top, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, arial, 12),
        () => testFunc(nan, 100, hstr, hstr, vst, vce, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, arial, 12),
        () => testFunc(nan, 100, hstr, hstr, vst, bot, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, arial, 12),
        () => testFunc(nan, 100, hstr, hstr, top, vst, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, arial, 12),
        () => testFunc(nan, 100, hstr, hstr, top, top, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, arial, 12),
        () => testFunc(nan, 100, hstr, hstr, top, vce, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, arial, 12),
        () => testFunc(nan, 100, hstr, hstr, top, bot, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, arial, 12),
        () => testFunc(nan, 100, hstr, hstr, vce, vst, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, arial, 12),
        () => testFunc(nan, 100, hstr, hstr, vce, top, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, arial, 12),
        () => testFunc(nan, 100, hstr, hstr, vce, vce, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, arial, 12),
        () => testFunc(nan, 100, hstr, hstr, vce, bot, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, arial, 12),
        () => testFunc(nan, 100, hstr, hstr, bot, vst, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, arial, 12),
        () => testFunc(nan, 100, hstr, hstr, bot, top, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, arial, 12),
        () => testFunc(nan, 100, hstr, hstr, bot, vce, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, arial, 12),
        () => testFunc(nan, 100, hstr, hstr, bot, bot, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, arial, 12),
        () => testFunc(200, 100, hstr, hstr, vst, vst, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, arial, 12),
      };
    }


    private Action? testFunc(
      double width, 
      double height, 
      HorizontalAlignment horizontalControlAlignment, 
      HorizontalAlignment horizontaControllAlignment, 
      VerticalAlignment verticalControlAlignment, 
      VerticalAlignment verticalContentAlignment, 
      double marginLeft, 
      double borderLeft, 
      double paddingLeft, 
      double paddingRight, 
      double borderRight, 
      double marginRight, 
      double marginTop, 
      double borderTop, 
      double paddingTop, 
      double paddingBottom, 
      double borderBottom, 
      double marginBottom, 
      FontFamily fontFamily,
      double fontSize) 
    {
      customControl!.Width = width; 
      customControl.Height = height; 
      customControl.HorizontalAlignment = horizontalControlAlignment; 
      customControl.HorizontalContentAlignment = horizontaControllAlignment; 
      customControl.VerticalAlignment = verticalControlAlignment; 
      customControl.VerticalContentAlignment = verticalContentAlignment; 
      customControl.Margin = new Thickness(marginLeft, marginTop, marginRight, marginBottom); 
      customControl.BorderThickness = new Thickness(borderLeft, borderTop, borderRight, borderBottom); 
      customControl.Padding = new Thickness(paddingLeft, paddingTop, paddingRight, paddingBottom); 
      customControl.FontFamily = fontFamily;
      customControl.FontSize = fontSize;
      return null;
    }

    
    void nextTestButton_Click(object sender, RoutedEventArgs e) {
      if (testAction!=null){
        //verify result of previous test
        //Exception exception = null;
        try {
          testAction();
        } catch (Exception ex) {
          //          exception = ex;
          Tracer.Error("Test Error: " + ex.Message + Environment.NewLine + ex.ToDetailString());
        }
        //if (exception!=null) {
        //  MessageBox.Show(exception.Message, "Test Error", MessageBoxButton.OK, MessageBoxImage.Error);
        //}
        testAction = null;
      }

      if (testFuncsIndex==firstcustomControlTestIndex) {
        //save parameters before changing them with the first custom control test
        savePreviousParameters();
      } else if (testFuncsIndex==0 && previousWidth!=double.MinValue) {
        restorePreviousParameters();
      }

      //execute next test
      testAction = testFuncs![testFuncsIndex++]();
      if (testFuncsIndex>=testFuncs.Length) {
        testFuncsIndex = 0;
      }
      nextTestButton.Content = "_Next Test " + testFuncsIndex;
    }


    double previousWidth = double.MinValue;
    double previousHeight;
    HorizontalAlignment previousHorizontalControlAlignment;
    HorizontalAlignment previousHorizontaControllAlignment;
    VerticalAlignment previousVerticalControlAlignment;
    VerticalAlignment previousVerticalContentAlignment;
    Thickness previousMargin;
    Thickness previousBorderThickness;
    Thickness previousPadding;
    FontFamily previousFontFamily;
    double previousFontSize; 


    private void savePreviousParameters() {
      previousWidth = customControl!.Width;
      previousHeight = customControl.Height;
      previousHorizontalControlAlignment = customControl.HorizontalAlignment;
      previousHorizontaControllAlignment = customControl.HorizontalContentAlignment;
      previousVerticalControlAlignment = customControl.VerticalAlignment;
      previousVerticalContentAlignment = customControl.VerticalContentAlignment;
      previousMargin = customControl.Margin;
      previousBorderThickness = customControl.BorderThickness;
      previousPadding = customControl.Padding;
      previousFontFamily = customControl.FontFamily;
      previousFontSize = customControl.FontSize;
    }


    private void restorePreviousParameters() {
      customControl!.Width = previousWidth;
      customControl.Height = previousHeight;
      customControl.HorizontalAlignment = previousHorizontalControlAlignment;
      customControl.HorizontalContentAlignment = previousHorizontaControllAlignment;
      customControl.VerticalAlignment = previousVerticalControlAlignment;
      customControl.VerticalContentAlignment = previousVerticalContentAlignment;
      customControl.Margin = previousMargin;
      customControl.BorderThickness = previousBorderThickness;
      customControl.Padding = previousPadding;
      customControl.FontFamily = previousFontFamily;
      customControl.FontSize = previousFontSize;
    }

    
    bool isFirstTime = true;


    protected override Size MeasureOverride(Size availableSize) {
      if (isFirstTime) {
        isFirstTime = false;
        Width = System.Windows.SystemParameters.PrimaryScreenWidth*0.85;
        return base.MeasureOverride(new Size(Width, availableSize.Height));
      } else {
        return base.MeasureOverride(availableSize);
      }
    }


    void TestbenchWindow_Closed(object? sender, EventArgs e) {
      if (Owner!=null) {
        Owner.Activate();
      }
    }

  }
}
