/********************************************************************************************************

WpfTestbench.MainWindow
=======================

Application main window showing all available tests

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


namespace WpfTestbench {


  /// <summary>
  /// Interaction logic for MainWindow.xaml
  /// </summary>
  public partial class MainWindow: Window {

    #region Constructor
    //      -----------

    public MainWindow() {
      InitializeComponent();
      StackPanelButton.Click += StackPanelButton_Click;
      TextBoxButton.Click += TextBoxButton_Click;

      ControlButton.Click += ControlButton_Click;
    }
    #endregion


    #region Eventhandler
    //      ------------

    private void ControlButton_Click(object sender, RoutedEventArgs e) {
      ControlWindow.Show(this);
    }


    void StackPanelButton_Click(object sender, RoutedEventArgs e) {
      StackPanelWindow.Show(this);
    }


    void TextBoxButton_Click(object sender, RoutedEventArgs e) {
      TextBoxWindow.Show(this);
    }
    #endregion
  }
}
