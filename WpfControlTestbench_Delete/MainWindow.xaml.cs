/**************************************************************************************

WpfTestbench.TextBoxTraced
==========================

TextBox with event tracing for TestBench.

Written 2014 - 2020 by Jürgpeter Huber 
Contact: PeterCode at Peterbox dot com

To the extent possible under law, the author(s) have dedicated all copyright and 
related and neighboring rights to this software to the public domain worldwide under
the Creative Commons 0 license (details see COPYING.txt file, see also
<http://creativecommons.org/publicdomain/zero/1.0/>). 

This software is distributed without any warranty. 
**************************************************************************************/
using System;
using System.Text;
using System.Windows;


namespace WpfTestbench {

  /// <summary>
  /// Interaction logic for MainWindow.xaml
  /// </summary>
  public partial class MainWindow: Window {
    public MainWindow() {
      InitializeComponent();
      StackPanelButton.Click += StackPanelButton_Click;
      TextBoxButton.Click += TextBoxButton_Click;

      TodoTexBox.Text =
        "Add file headers" + Environment.NewLine +
        "Move DummyConverter to CustomControlBaseLib" + Environment.NewLine +
        "Can Generic.xaml be replaced" + Environment.NewLine +
        "Can TracedLib be replaced with reflection ? Probably not" + Environment.NewLine +
        @"Original code: D:\Visual Studio 2010\Projects\CodeProject\WpfControlTestbench\WpfControlTestbenchDev";
    }


    void StackPanelButton_Click(object sender, RoutedEventArgs e) {
      StackPanelWindow.Show(this);
    }


    void TextBoxButton_Click(object sender, RoutedEventArgs e) {
      TextBoxWindow.Show(this);
    }
  }
}
