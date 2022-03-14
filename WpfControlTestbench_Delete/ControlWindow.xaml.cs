using System;
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


namespace WpfControlTestbench {
  /// <summary>
  /// Interaction logic for ControlWindow.xaml
  /// </summary>
  /// 

  public partial class ControlWindow: Window {
    public ControlWindow() {
      InitializeComponent();
      WpfBinding.Setup(TextTextBox, "Text", TestControlTraced,
        ControlTraced.TextProperty, BindingMode.TwoWay);

      FillStandardColorComboBox.SetSelectedBrush(TestControlTraced.Fill??Brushes.Transparent);
      WpfBinding.Setup(FillStandardColorComboBox, "SelectedColorBrush", TestControlTraced,
        ControlTraced.FillProperty, BindingMode.TwoWay);

    }
  }
}
