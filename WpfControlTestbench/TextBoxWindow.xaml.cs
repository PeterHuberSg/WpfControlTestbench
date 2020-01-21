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
  /// Interaction logic for TextBoxWindow.xaml
  /// </summary>
  public partial class TextBoxWindow: TestbenchWindow {

    public static void Show(Window ownerWindow) {
      ShowProtected( () => new TextBoxWindow(), ownerWindow);
    }

    public TextBoxWindow() {
      //TextBox t;
      //t.SelectedText;
      InitializeComponent();
      foreach (TextAlignment? itemTextAlignment in Enum.GetValues(typeof(TextAlignment))) {
        TextAlignmentComboBox.Items.Add(itemTextAlignment);
      }

      foreach (TextWrapping? itemTextWrapping in Enum.GetValues(typeof(TextWrapping))) {
        TextWrappingComboBox.Items.Add(itemTextWrapping);
      }

      foreach (ScrollBarVisibility? itemScrollBarVisibility in Enum.GetValues(typeof(ScrollBarVisibility))) {
        HorizontalScrollBarVisibilityComboBox.Items.Add(itemScrollBarVisibility);
        VerticalScrollBarVisibilityComboBox.Items.Add(itemScrollBarVisibility);
      }

      ClearButton.Click += clearButton_Click;
    }


    void clearButton_Click(object sender, RoutedEventArgs e) {
      TestTextBoxTraced.Clear();
    }
  }
}
