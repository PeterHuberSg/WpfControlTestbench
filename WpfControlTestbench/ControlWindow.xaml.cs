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
using WpfTestbench;


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



    public ControlWindow() {
      InitializeComponent();

      Width = (int)System.Windows.SystemParameters.PrimaryScreenWidth*4/5;
      Height = (int)System.Windows.SystemParameters.PrimaryScreenHeight*4/5;

      WpfBinding.Setup(TextTextBox, "Text", TestControlTraced,
        ControlTraced.TextProperty, BindingMode.TwoWay);

      FillStandardColorComboBox.SetSelectedBrush(TestControlTraced.Fill??Brushes.Transparent);
      WpfBinding.Setup(FillStandardColorComboBox, "SelectedColorBrush", TestControlTraced,
        ControlTraced.FillProperty, BindingMode.TwoWay);
    }
  }
}
