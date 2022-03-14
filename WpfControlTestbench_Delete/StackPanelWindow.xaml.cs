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

    /// <summary>
    /// Creates and opens a new StackPanelWindow
    /// </summary>
    public static void Show(Window ownerWindow) {
      ShowProtected(() => new StackPanelWindow(), ownerWindow);
    }


    public StackPanelWindow() {
      InitializeComponent();

      BackgroundStandardColorComboBox.SetSelectedBrush(TestStackPanelTraced.Background);
      WpfBinding.Setup(BackgroundStandardColorComboBox, "SelectedColorBrush", TestStackPanelTraced, Panel.BackgroundProperty, BindingMode.TwoWay);

      AddChildButton.Click += AddChildButton_Click;
    }


    int childNo = 1;


    void AddChildButton_Click(object sender, RoutedEventArgs e) {
      string childName = "Child " + childNo;
      TextBoxTraced childTextBox = new TextBoxTraced(childName);
      childTextBox.HorizontalAlignment = HorizontalAlignment.Left;
      childTextBox.VerticalAlignment = VerticalAlignment.Top;
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
