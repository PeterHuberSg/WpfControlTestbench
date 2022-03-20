/********************************************************************************************************

WpfTestbench.StackPanelWindow
=============================

Test window for TestStackPanelTraced.

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
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;


namespace WpfTestbench {


  /// <summary>
  /// Interaction logic for StackPanelWindow.xaml
  /// </summary>
  public partial class StackPanelWindow: Window {

    #region Constructor
    //      -----------

    /// <summary>
    /// Creates and opens a new StackPanelWindow
    /// </summary>
    public static void Show(Window ownerWindow) {
      var newWindow = new StackPanelWindow {
        Owner = ownerWindow
      };
      newWindow.Show();
    }


    public StackPanelWindow() {
      InitializeComponent();
      Width = (int)System.Windows.SystemParameters.PrimaryScreenWidth*4/5;
      Height = (int)System.Windows.SystemParameters.PrimaryScreenHeight*4/5;

      BackgroundStandardColorComboBox.SetSelectedBrush(TestStackPanelTraced.Background);
      WpfBinding.Setup(BackgroundStandardColorComboBox, "SelectedColorBrush", TestStackPanelTraced, Panel.BackgroundProperty, BindingMode.TwoWay);

      VerticalRadioButton.Click += OrientationRadioButton_Click;
      HorizontalRadioButton.Click += OrientationRadioButton_Click;
      AddChildButton.Click += AddChildButton_Click;
    }
    #endregion


    #region Eventhandler
    //      ------------

    private void OrientationRadioButton_Click(object sender, RoutedEventArgs e) {
      TestStackPanelTraced.Orientation = VerticalRadioButton.IsChecked!.Value ? Orientation.Vertical : Orientation.Horizontal; 
    }


    int childNo = 1;


    void AddChildButton_Click(object sender, RoutedEventArgs e) {
      string childName = "Child " + childNo;
      var childTextBox = new TextBoxTraced(childName) {
        HorizontalAlignment = HorizontalAlignment.Left,
        VerticalAlignment = VerticalAlignment.Top
      };
      byte grayShade = (byte)(256 - childNo*10);
      childTextBox.Background = new SolidColorBrush(Color.FromRgb(grayShade, grayShade, grayShade));
      childTextBox.AcceptsReturn = true;
      childTextBox.Text = childName + Environment.NewLine +
        new string('*', childNo*10);
      TestStackPanelTraced.Children.Add(childTextBox);
      childNo++;
    }
    #endregion
  }
}