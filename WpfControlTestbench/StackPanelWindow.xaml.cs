﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;


namespace WpfTestbench {


  /// <summary>
  /// Interaction logic for StackPanelWindow.xaml
  /// </summary>
  public partial class StackPanelWindow: Window {

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
  }
}