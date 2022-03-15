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


namespace WpfTestbench {
  /// <summary>
  /// Interaction logic for TextBoxWindow.xaml
  /// </summary>
  public partial class TextBoxWindow: Window {

    /// <summary>
    /// Creates and displays a Testbench Window to test TextBox
    /// </summary>
    public static void Show(Window ownerWindow) {
      var newWindow = new TextBoxWindow {
        Owner = ownerWindow
      };
      newWindow.Show();
    }


    /// <summary>
    /// Don't call default constructor directly. Use Show() instead
    /// </summary>
    public TextBoxWindow() {
      InitializeComponent();

      Width = (int)System.Windows.SystemParameters.PrimaryScreenWidth*4/5;
      Height = (int)System.Windows.SystemParameters.PrimaryScreenHeight*4/5;

      foreach (TextAlignment itemTextAlignment in Enum.GetValues(typeof(TextAlignment))) {
        TextAlignmentComboBox.Items.Add(itemTextAlignment);
      }

      foreach (TextWrapping itemTextWrapping in Enum.GetValues(typeof(TextWrapping))) {
        TextWrappingComboBox.Items.Add(itemTextWrapping);
      }

      TextDecorationsComboBox.Items.Add("None");
      TextDecorationsComboBox.Items.Add("OverLine");
      TextDecorationsComboBox.Items.Add("Strikethrough");
      TextDecorationsComboBox.Items.Add("Baseline");
      TextDecorationsComboBox.Items.Add("Underline");
      TextDecorationsComboBox.SelectedIndex = 0;
      foreach (ScrollBarVisibility itemScrollBarVisibility in Enum.GetValues(typeof(ScrollBarVisibility))) {
        HorizontalScrollBarVisibilityComboBox.Items.Add(itemScrollBarVisibility);
        VerticalScrollBarVisibilityComboBox.Items.Add(itemScrollBarVisibility);
      }

      WpfBinding.Setup(TestTextBoxTraced, "SelectionOpacity", SelectionOpacityTextBox, TextBox.TextProperty, BindingMode.TwoWay, new DoubleNanConverter());

      foreach (var characterCasing in Enum.GetValues(typeof(CharacterCasing))) {
        CharacterCasingComboBox.Items.Add(characterCasing);
      }

      CaretBrushColorComboBox.SetSelectedBrush(TestTextBoxTraced.CaretBrush);
      WpfBinding.Setup(CaretBrushColorComboBox, "SelectedColorBrush", TestTextBoxTraced, TextBox.CaretBrushProperty, BindingMode.TwoWay);

      SelectionBrushColorComboBox.SetSelectedBrush(TestTextBoxTraced.SelectionBrush);
      WpfBinding.Setup(SelectionBrushColorComboBox, "SelectedColorBrush", TestTextBoxTraced, TextBox.SelectionBrushProperty, BindingMode.TwoWay);

      SelectionTextBrushComboBox.SetSelectedBrush(TestTextBoxTraced.SelectionTextBrush);
      WpfBinding.Setup(SelectionTextBrushComboBox, "SelectedColorBrush", TestTextBoxTraced, TextBox.SelectionTextBrushProperty, BindingMode.TwoWay);

      TestTextBoxTraced.SelectionChanged += TestTextBoxTraced_SelectionChanged;

      LineUpButton.Click += LineUpButton_Click;
      LineLeftButton.Click += LineLeftButton_Click;
      LineRightButton.Click += LineRightButton_Click;
      LineDownButton.Click += LineDownButton_Click;

      PageUpButton.Click += PageUpButton_Click;
      PageLeftButton.Click += PageLeftButton_Click;
      PageRightButton.Click += PageRightButton_Click;
      PageDownButton.Click += PageDownButton_Click;

      CopyButton.Click += CopyButton_Click;
      CutButton.Click += CutButton_Click;
      PasteButton.Click += PasteButton_Click;
      SelecteAllButton.Click += SelecteAllButton_Click;
      UndoButton.Click += UndoButton_Click;
      RedoButton.Click += RedoButton_Click;
      ScrollToHomeButton.Click += ScrollToHomeButton_Click;
      ScrollToEndButton.Click += ScrollToEndButton_Click;
      ClearButton.Click += ClearButton_Click;

      TestTextBoxTraced.Text =
@"Some Sample Text to help with testing 

It must have some long lines 0123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789 to demonstrate horizontal scrolling

and many lines for vertical scrolling.

To see the ScrollBars, set Horzontal SB and Vertical SB ComboBox.

Unfortunately, some selection related function cannot be demonstrated here properly, because clicking
outside of the TextBox makes the selection disappear.
-----------------------------------------------------------------------------------------------------

Explanation IsInactiveSelectionHighlightEnabled in Help:
""Gets or sets a value that indicates whether the text box displays selected text when the text box 
does not have focus.""
-----------------------------------------------------------------------------------------------------


Explanation IsReadOnlyCaretVisible in Help:
""When IsReadOnly is true, a user can still select and copy text. If the IsReadOnlyCaretVisible property 
is also set to true, a caret will appear in the text box when the text box has keyboard focus. When 
IsReadOnly is false, the IsReadOnlyCaretVisible property has no effect.""
I can not figure out how to make it work, though :-(
-----------------------------------------------------------------------------------------------------


Explanation MaxLength in Help:
""You can use this property to restrict the length of text entered in the control for values such as
postal codes and telephone numbers. You can also use this property to restrict the length of text 
entered when the data is to be stored in a database so that the text entered into the control does not 
exceed the maximum length of the corresponding field in the database.

This property does not affect characters that are added programmatically.

When this property is set to 0, the maximum length of the text that can be entered in the control is 
limited only by available memory.""
-----------------------------------------------------------------------------------------------------

Explanation MinLines in Help:
""Getting this property returns the current value of MinLines. Setting this property causes the text 
box to resize if the number of visible lines is less than value specified by MinLines.

If the Height property is explicitly set on a TextBox, the MaxLines and MinLines property values are 
ignored.""
-----------------------------------------------------------------------------------------------------

Help explains baddly the difference between HorizontalContentAlignment and TextAlignment:
""HorizontalContentAlignment: In addition to Left, Right, and Center, you can set the 
HorizontalContentAlignment property to Stretch, which stretches the child element to fill the 
allocated space of the parent element. For more information, see Alignment, Margins, and Padding 
Overview.

This property only affects a control whose template uses the HorizontalContentAlignment property as a 
parameter. On other controls, this property has no impact.""

""TextAlignment: Getting this property returns the current alignment. Setting this property adjusts 
the contents of the text box to reflect the specified alignment.

This property has a higher precedence than the HorizontalContentAlignment property.""
";
    }


    private void LineUpButton_Click(object sender, RoutedEventArgs e) {
      TestTextBoxTraced.LineUp();
    }


    private void LineLeftButton_Click(object sender, RoutedEventArgs e) {
      TestTextBoxTraced.LineLeft();
    }


    private void LineRightButton_Click(object sender, RoutedEventArgs e) {
      TestTextBoxTraced.LineRight();
    }


    private void LineDownButton_Click(object sender, RoutedEventArgs e) {
      TestTextBoxTraced.LineDown();
    }


    private void PageUpButton_Click(object sender, RoutedEventArgs e) {
      TestTextBoxTraced.PageUp();
    }


    private void PageLeftButton_Click(object sender, RoutedEventArgs e) {
      TestTextBoxTraced.PageLeft();
    }


    private void PageRightButton_Click(object sender, RoutedEventArgs e) {
      TestTextBoxTraced.PageRight();
    }


    private void PageDownButton_Click(object sender, RoutedEventArgs e) {
      TestTextBoxTraced.PageDown();
    }


    private void ScrollToHomeButton_Click(object sender, RoutedEventArgs e) {
      TestTextBoxTraced.ScrollToHome();
    }


    private void ScrollToEndButton_Click(object sender, RoutedEventArgs e) {
      TestTextBoxTraced.ScrollToEnd();
    }


    private void CopyButton_Click(object sender, RoutedEventArgs e) {
      TestTextBoxTraced.Copy();
    }

    private void CutButton_Click(object sender, RoutedEventArgs e) {
      TestTextBoxTraced.Cut();
    }

    private void PasteButton_Click(object sender, RoutedEventArgs e) {
      TestTextBoxTraced.Paste();
    }

    private void SelecteAllButton_Click(object sender, RoutedEventArgs e) {
      TestTextBoxTraced.SelectAll();
    }

    private void UndoButton_Click(object sender, RoutedEventArgs e) {
      TestTextBoxTraced.Undo();
    }

    private void RedoButton_Click(object sender, RoutedEventArgs e) {
      TestTextBoxTraced.Redo();
    }

    private void TestTextBoxTraced_SelectionChanged(object sender, RoutedEventArgs e) {
      var start = TestTextBoxTraced.SelectionStart;
      var length = TestTextBoxTraced.SelectionLength;
      var text = $"S:{start} L:{length} " + TestTextBoxTraced.Text[start..(start + length)];
      var pos = text.IndexOf('\n');
      if (pos>=0) {
        text = text[..pos];
      }
      pos = text.IndexOf('\r');
      if (pos>=0) {
        text = text[..pos];
      }
      SelectionTextBox.Text = text;
    }


    void ClearButton_Click(object sender, RoutedEventArgs e) {
      TestTextBoxTraced.Clear();
    }
  }
}
