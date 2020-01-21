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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WpfTestbench {
  /// <summary>
  /// Interaction logic for MainWindow.xaml
  /// </summary>
  public partial class MainWindow: Window {
    public MainWindow() {
TracerLib.Tracer.MessagesTraced += new Action<TracerLib.TraceMessage[]>(tracer_MessagesTraced);
      InitializeComponent();
      StackPanelButton.Click += stackPanelButton_Click;
      TextBoxButton.Click += textBoxButton_Click;
    }

    readonly StringBuilder s = new StringBuilder();
void tracer_MessagesTraced(TracerLib.TraceMessage[] traceMessages) {
  foreach (TracerLib.TraceMessage traceMessage in traceMessages) {
    s.Append(traceMessage.ToString());
  }
}


    void stackPanelButton_Click(object sender, RoutedEventArgs e) {
      StackPanelWindow.Show(this);
    }


    void textBoxButton_Click(object sender, RoutedEventArgs e) {
      TextBoxWindow.Show(this);
    }
  }
}
