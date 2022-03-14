using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using TracerLib;

namespace WpfTestbench {
  /// <summary>
  /// Interaction logic for TextBoxWindow.xaml
  /// </summary>
  public partial class TextBoxWindow: TestbenchWindow {

    /// <summary>
    /// Creates and displays a Testbench Window to test TextBox
    /// </summary>
    public static void Show(Window ownerWindow) {
      ShowProtected( () => new TextBoxWindow(), ownerWindow);
    }
    

    /// <summary>
    /// Don't call default constructor directly. Use Show() instead
    /// </summary>
    public TextBoxWindow() {
      InitializeComponent();
      foreach (TextAlignment itemTextAlignment in Enum.GetValues(typeof(TextAlignment))) {
        TextAlignmentComboBox.Items.Add(itemTextAlignment);
      }

      foreach (TextWrapping itemTextWrapping in Enum.GetValues(typeof(TextWrapping))) {
        TextWrappingComboBox.Items.Add(itemTextWrapping);
      }

      foreach (ScrollBarVisibility itemScrollBarVisibility in Enum.GetValues(typeof(ScrollBarVisibility))) {
        HorizontalScrollBarVisibilityComboBox.Items.Add(itemScrollBarVisibility);
        VerticalScrollBarVisibilityComboBox.Items.Add(itemScrollBarVisibility);
      }

      ClearButton.Click += ClearButton_Click;

      TestTextBoxTraced.SizeChanged += new SizeChangedEventHandler(TestTextBoxTraced_SizeChanged);

      Loaded += new RoutedEventHandler(TextBoxWindow_Loaded);
    }


    void TextBoxWindow_Loaded(object sender, RoutedEventArgs e) {
      TestBench.Part_WpfTraceViewer.FilterFunc = filterFunction;
    }


    private bool filterFunction(TraceMessage traceMessage) {
      return traceMessage.FilterText==null;
    }


    void TestTextBoxTraced_SizeChanged(object sender, SizeChangedEventArgs e) {
      //Point point = new Point(0, 0);
      //Tracer.TraceLineFiltered("", "SizeChanged: W:{0}, H:{1}, {2}, {3}", e.NewSize.Width, e.NewSize.Height, TestTextBoxTraced.PointFromScreen(point), TestTextBoxTraced.PointToScreen(point));
    }


    void ClearButton_Click(object sender, RoutedEventArgs e) {
      TestTextBoxTraced.Clear();
    }
  }
}
