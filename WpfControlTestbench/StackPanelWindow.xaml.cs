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

namespace WpfTestbench {
  /// <summary>
  /// Interaction logic for StackPanelWindow.xaml
  /// </summary>
  public partial class StackPanelWindow: TestbenchWindow {

    public static void Show(Window ownerWindow) {
      ShowProtected( () => new StackPanelWindow(), ownerWindow);
    }


    public StackPanelWindow() {
      InitializeComponent();

      BackgroundStandardColorComboBox.SetSelectedBrush(TestStackPanelTraced.Background);
      WpfBinding.Setup(BackgroundStandardColorComboBox, "SelectedColorBrush", TestStackPanelTraced, Panel.BackgroundProperty, BindingMode.TwoWay);

      AddChildButton.Click += addChildButton_Click;
    }


    int childNo = 1;


    void addChildButton_Click(object sender, RoutedEventArgs e) {
      string childName = "Child " + childNo;
      TextBoxTraced childTextBox = new TextBoxTraced(childName) {
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
