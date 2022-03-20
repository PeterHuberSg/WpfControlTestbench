/********************************************************************************************************

WpfTestbench.StandardPropertyViewer
===================================

Displays a property grid for a control (TestFrameworkElement) inheriting from FrameworkElement and possibly Control.

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
using System.ComponentModel;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Shapes;
using System.Xml;


namespace WpfTestbench {

  /// <summary>
  /// Displays a property grid for a control (TestFrameworkElement) inheriting from FrameworkElement and possibly Control.
  /// </summary>
  public partial class StandardPropertyViewer: UserControl {

    #region Constructor
    //      -----------

    readonly FrameworkElement testFrameworkElement; //the FrameworkElement getting tested
    readonly Control? testControl; //gives access to TestFrameworkElement as a Control. This is needed for access to Fonts, Border, Padding, etc.
    readonly Grid? hostGrid; //testFrameworkElement is supposed to be hosted in a grid
    readonly int testFrameworkElementGridRow;
    readonly int testFrameworkElementGirdColumn;


    #pragma warning disable CS8618 // Non-nullable field testFrameworkElement, referenceLine and origineShadow are uninitialized. Consider declaring as nullable.
    public StandardPropertyViewer(FrameworkElement testFrameworkElement, Grid hostGrid): this() {
    #pragma warning restore CS8618
      try { //improve how WPF handles exceptions in the constructor
        this.testFrameworkElement = testFrameworkElement;
        this.hostGrid = hostGrid;

        testFrameworkElementGridRow = Grid.GetColumn(testFrameworkElement);
        testFrameworkElementGirdColumn = Grid.GetRow(testFrameworkElement);
        this.testFrameworkElement = testFrameworkElement;
        testControl = testFrameworkElement as Control;

        setupWidthHeightMarginBorderTextBoxes();
        setupAlignment();

        if (testControl==null) {
          //TestFrameworkElement does not support Fonts and Padding. Hide them
          ContentColumn.MaxWidth = 0;
          BorderLeftColumn.MaxWidth = 0;
          PaddingLeftColumn.MaxWidth = 0;
          PaddingRightColumn.MaxWidth = 0;
          BorderRightColumn.MaxWidth = 0;
          ColorEmptyColumn.MaxWidth = 0;
          ColorColumn.MaxWidth = 0;
          FontColumn.MaxWidth = 0;
          FontSizeColumn.MaxWidth = 0;
          FontWeightColumn.MaxWidth = 0;
          TemplateButton.IsEnabled = false;
          TextBlock textBlock = new() {
            Text =  "Template can only be displayed for FrameworkElement inheriting from Control"
          };
          TemplateButton.ToolTip = textBlock;
        } else {
          //TestFrameworkElement does support Fonts, Border and Padding. Show them
          setupFontComboBoxes();
        }

        testFrameworkElement.LayoutUpdated += testFrameworkElement_LayoutUpdated;

      } catch (Exception ex) {
        Tracer.Exception(ex, "");
        throw;
      }
    }


    /// <summary>
    /// Default constructor
    /// </summary>
    #pragma warning disable CS8618 // Non-nullable field testFrameworkElement and origineShadow are uninitialized. Consider declaring as nullable.
    public StandardPropertyViewer() {
    #pragma warning restore CS8618
      InitializeComponent();

      GridLinesCheckBox.Click += gridLinesCheckBox_Click;
      TemplateButton.Click += templateButton_Click;
      DebugButton.Click += debugButton_Click;
      BreakOnExceptionCheckBox.IsChecked = Tracer.IsBreakOnException;
      BreakOnExceptionCheckBox.Checked += new RoutedEventHandler(breakOnExceptionCheckBox_Checked);
      BreakOnExceptionCheckBox.Unchecked += new RoutedEventHandler(breakOnExceptionCheckBox_Checked);

      FontSize = 11;
    }
    #endregion


    #region General Events
    //      --------------

    void gridLinesCheckBox_Click(object sender, RoutedEventArgs e) {
      foreach (var item in hostGrid!.Children) {
        if (item is Line line) {
          line.Visibility =GridLinesCheckBox.IsChecked!.Value ? System.Windows.Visibility.Visible : System.Windows.Visibility.Hidden;
        }
      }
    }


    private void breakOnExceptionCheckBox_Checked(object sender, RoutedEventArgs e) {
      Tracer.IsBreakOnException = BreakOnExceptionCheckBox.IsChecked!.Value;
      Tracer.IsBreakOnError = Tracer.IsBreakOnException;
      Tracer.IsBreakOnWarning = Tracer.IsBreakOnException;
    }


    void templateButton_Click(object sender, RoutedEventArgs e) {
      if (testControl!=null) {
        var stringBuilder = new StringBuilder();

        if (testControl==null) {
          stringBuilder.AppendLine("'" + testFrameworkElement.GetType() + "' does not inherit from Control");
        } else  if (testControl.Template==null) {
          stringBuilder.AppendLine("Control has no template assigned.");
        } else {
          var xmlSettings = new XmlWriterSettings { Indent = true };
          using var xmlWriter = XmlWriter.Create(stringBuilder, xmlSettings);
          System.Windows.Markup.XamlWriter.Save(testControl.Template, xmlWriter);
        }
        MessageBox.Show(stringBuilder.ToString());
      }
    }


    void debugButton_Click(object sender, RoutedEventArgs e) {
      System.Diagnostics.Debugger.Break();
    }


    void testFrameworkElement_LayoutUpdated(object? sender, EventArgs e) {
      DesiredHeightTextBox.Text = testFrameworkElement.DesiredSize.Height.ToString(".0");
      DesiredWidthTextBox.Text = testFrameworkElement.DesiredSize.Width.ToString(".0");
      RenderHeightTextBox.Text = testFrameworkElement.RenderSize.Height.ToString(".0");
      RenderWidthTextBox.Text = testFrameworkElement.RenderSize.Width.ToString(".0");
    }
    #endregion


    #region Width Height TextBoxes
    //      ----------------------

    private void setupWidthHeightMarginBorderTextBoxes() {
      //adds behavior that when user clicks on TextBox, all text gets selected first
      setupTextBox(HeightTextBox);
      setupTextBox(MinHeightTextBox);
      setupTextBox(MaxHeightTextBox);
      setupTextBox(WidthTextBox);
      setupTextBox(MinWidthTextBox);
      setupTextBox(MaxWidthTextBox);
      //bind TextBoxes with width and height properties to TestFrameworkElement
      WpfBinding.Setup(testFrameworkElement, "Height", HeightTextBox, TextBox.TextProperty, BindingMode.TwoWay, new DoubleNanConverter());
      WpfBinding.Setup(testFrameworkElement, "MinHeight", MinHeightTextBox, TextBox.TextProperty, BindingMode.TwoWay, new DoublePositiveConverter());
      WpfBinding.Setup(testFrameworkElement, "MaxHeight", MaxHeightTextBox, TextBox.TextProperty, BindingMode.TwoWay, new DoublePositiveConverter());
      WpfBinding.Setup(testFrameworkElement, "Width", WidthTextBox, TextBox.TextProperty, BindingMode.TwoWay, new DoubleNanConverter());
      WpfBinding.Setup(testFrameworkElement, "MinWidth", MinWidthTextBox, TextBox.TextProperty, BindingMode.TwoWay, new DoublePositiveConverter());
      WpfBinding.Setup(testFrameworkElement, "MaxWidth", MaxWidthTextBox, TextBox.TextProperty, BindingMode.TwoWay, new DoublePositiveConverter());

      setupMarginTextBoxes();
      if (testControl!=null) {
        setupBorderPaddingTextBoxes();
      }
    }


    /// <summary>
    /// When a user clicks on a TextBox, all content gets selected. Only when the TextBox loses the focus will some binding happen.
    /// </summary>
    private void setupTextBox(TextBox textBox, RoutedEventHandler lostFocusEventHandler) {
      textBox.LostFocus += lostFocusEventHandler;
      setupTextBox(textBox);
    }


    /// <summary>
    /// When a user clicks on a TextBox, all content gets selected
    /// </summary>
    private void setupTextBox(TextBox textBox) {
      textBox.PreviewMouseDown += new MouseButtonEventHandler(textBox_PreviewMouseDown);
      textBox.GotFocus += new RoutedEventHandler(textBox_GotFocus);
      textBox.SelectionChanged += new RoutedEventHandler(textBox_SelectionChanged);
    }


    bool isMouseDown;


    void textBox_PreviewMouseDown(object sender, MouseButtonEventArgs e) {
      isMouseDown = true;
    }


    bool hasGotFocused;


    void textBox_GotFocus(object sender, RoutedEventArgs e) {
      if (isMouseDown) {
        //user clicked with mouse on TextBox. Wait for the SelectionChanged event to select all the text
        isMouseDown = false;
        hasGotFocused = true;
      } else {
        //user used Tab key, which does not change the selection and the SelectionChanged event will not get fired.
        TextBox textBox = (TextBox)sender;
        textBox.SelectAll();
      }
    }

    void textBox_SelectionChanged(object sender, RoutedEventArgs e) {
      if (hasGotFocused) {
        hasGotFocused = false;
        TextBox textBox = (TextBox)sender;
        textBox.SelectAll();
      }        
    }
    #endregion


    #region Alignment Setup
    //      ---------------

    private void setupAlignment() {
      HorizontalAlignmentComboBox.Items.Add(new ComboBoxItem {Content=((HorizontalAlignment)0).ToString()});
      HorizontalAlignmentComboBox.Items.Add(new ComboBoxItem {Content=((HorizontalAlignment)1).ToString()});
      HorizontalAlignmentComboBox.Items.Add(new ComboBoxItem {Content=((HorizontalAlignment)2).ToString()});
      HorizontalAlignmentComboBox.Items.Add(new ComboBoxItem {Content=((HorizontalAlignment)3).ToString()});
      HorizontalAlignmentComboBox.SelectedIndex = (int)testFrameworkElement.HorizontalAlignment;

      DependencyPropertyDescriptor horizontalDescriptor = DependencyPropertyDescriptor.FromProperty(FrameworkElement.HorizontalAlignmentProperty, typeof(FrameworkElement));
      horizontalDescriptor.AddValueChanged(testFrameworkElement, testFrameworkElement_HorizontalAlignmentChanged);

      HorizontalAlignmentComboBox.SelectionChanged += new SelectionChangedEventHandler(horizontalAlignmentComboBox_SelectionChanged);

      VerticalAlignmentComboBox.Items.Add(new ComboBoxItem {Content=((VerticalAlignment)0).ToString()});
      VerticalAlignmentComboBox.Items.Add(new ComboBoxItem {Content=((VerticalAlignment)1).ToString()});
      VerticalAlignmentComboBox.Items.Add(new ComboBoxItem {Content=((VerticalAlignment)2).ToString()});
      VerticalAlignmentComboBox.Items.Add(new ComboBoxItem {Content=((VerticalAlignment)3).ToString()});
      VerticalAlignmentComboBox.SelectedIndex = (int)testFrameworkElement.VerticalAlignment;

      DependencyPropertyDescriptor verticalDescriptor = DependencyPropertyDescriptor.FromProperty(FrameworkElement.VerticalAlignmentProperty, typeof(FrameworkElement));
      verticalDescriptor.AddValueChanged(testFrameworkElement, testFrameworkElement_VerticalAlignmentChanged);

      VerticalAlignmentComboBox.SelectionChanged += new SelectionChangedEventHandler(verticalAlignmentComboBox_SelectionChanged);

      if (testControl!=null) {
        HorizontalContentAlignmentComboBox.Items.Add(new ComboBoxItem { Content=((HorizontalAlignment)0).ToString() });
        HorizontalContentAlignmentComboBox.Items.Add(new ComboBoxItem { Content=((HorizontalAlignment)1).ToString() });
        HorizontalContentAlignmentComboBox.Items.Add(new ComboBoxItem { Content=((HorizontalAlignment)2).ToString() });
        HorizontalContentAlignmentComboBox.Items.Add(new ComboBoxItem { Content=((HorizontalAlignment)3).ToString() });
        HorizontalContentAlignmentComboBox.SelectedIndex = (int)testControl.HorizontalContentAlignment;

        DependencyPropertyDescriptor horizontalContentDescriptor = DependencyPropertyDescriptor.FromProperty(Control.HorizontalContentAlignmentProperty, typeof(Control));
        horizontalContentDescriptor.AddValueChanged(testFrameworkElement, onHorizontalContentAlignment);

        HorizontalContentAlignmentComboBox.SelectionChanged += new SelectionChangedEventHandler(horizontalContentAlignmentComboBox_SelectionChanged);

        VerticalContentAlignmentComboBox.Items.Add(new ComboBoxItem { Content=((VerticalAlignment)0).ToString() });
        VerticalContentAlignmentComboBox.Items.Add(new ComboBoxItem { Content=((VerticalAlignment)1).ToString() });
        VerticalContentAlignmentComboBox.Items.Add(new ComboBoxItem { Content=((VerticalAlignment)2).ToString() });
        VerticalContentAlignmentComboBox.Items.Add(new ComboBoxItem { Content=((VerticalAlignment)3).ToString() });
        VerticalContentAlignmentComboBox.SelectedIndex = (int)testControl.VerticalContentAlignment;

        DependencyPropertyDescriptor verticalContentDescriptor = DependencyPropertyDescriptor.FromProperty(Control.VerticalContentAlignmentProperty, typeof(Control));
        verticalContentDescriptor.AddValueChanged(testFrameworkElement, onVerticalContentAlignment);

        VerticalContentAlignmentComboBox.SelectionChanged += new SelectionChangedEventHandler(verticalContentAlignmentComboBox_SelectionChanged);
      }
    }


    void testFrameworkElement_HorizontalAlignmentChanged(object? sender, EventArgs args) {
      HorizontalAlignmentComboBox.SelectedIndex = (int)testFrameworkElement.HorizontalAlignment;
    }


    void horizontalAlignmentComboBox_SelectionChanged(object? sender, SelectionChangedEventArgs e) {
      testFrameworkElement.HorizontalAlignment = (HorizontalAlignment)HorizontalAlignmentComboBox.SelectedIndex;
    }


    void testFrameworkElement_VerticalAlignmentChanged(object? sender, EventArgs args) {
      VerticalAlignmentComboBox.SelectedIndex = (int)testFrameworkElement.VerticalAlignment;
    }


    void verticalAlignmentComboBox_SelectionChanged(object? sender, SelectionChangedEventArgs e) {
      testFrameworkElement.VerticalAlignment = (VerticalAlignment)VerticalAlignmentComboBox.SelectedIndex;
    }


    void onHorizontalContentAlignment(object? sender, EventArgs args) {
      Control control = (Control)sender!;
      HorizontalContentAlignmentComboBox.SelectedIndex = (int)control.HorizontalContentAlignment;
    }


    void horizontalContentAlignmentComboBox_SelectionChanged(object? sender, SelectionChangedEventArgs e) {
      testControl!.HorizontalContentAlignment = (HorizontalAlignment)HorizontalContentAlignmentComboBox.SelectedIndex;
    }


    void onVerticalContentAlignment(object? sender, EventArgs args) {
      Control control = (Control)sender!;
      VerticalContentAlignmentComboBox.SelectedIndex = (int)control.VerticalContentAlignment;
    }


    void verticalContentAlignmentComboBox_SelectionChanged(object? sender, SelectionChangedEventArgs e) {
      testControl!.VerticalContentAlignment = (VerticalAlignment)VerticalContentAlignmentComboBox.SelectedIndex;
    }
    #endregion


    #region Margin TextBoxes
    //      ----------------

    private void setupMarginTextBoxes() {
      setupTextBox(MarginLeftTextBox, marginLeftTextBox_LostFocus);
      setupTextBox(MarginTopTextBox, marginTopTextBox_LostFocus);
      setupTextBox(MarginRightTextBox, marginRightTextBox_LostFocus);
      setupTextBox(MarginBottomTextBox, marginBottomTextBox_LostFocus);
      updateMarginTextBoxes();
    
      DependencyPropertyDescriptor descriptor = DependencyPropertyDescriptor.FromProperty( FrameworkElement.MarginProperty, typeof(FrameworkElement));
      descriptor.AddValueChanged(testFrameworkElement, testFrameworkElement_MarginChanged);
    }


    void testFrameworkElement_MarginChanged(object? sender, EventArgs args) {
      updateMarginTextBoxes();
    }


    private void updateMarginTextBoxes() {
      MarginLeftTextBox.Text = testFrameworkElement.Margin.Left.ToString(".0");
      MarginTopTextBox.Text = testFrameworkElement.Margin.Top.ToString(".0");
      MarginRightTextBox.Text = testFrameworkElement.Margin.Right.ToString(".0");
      MarginBottomTextBox.Text = testFrameworkElement.Margin.Bottom.ToString(".0");
    }


    void marginLeftTextBox_LostFocus(object sender, RoutedEventArgs e) {
      if (int.TryParse(MarginLeftTextBox.Text, out var newMargin)) {
        testFrameworkElement.Margin = new Thickness(newMargin, testFrameworkElement.Margin.Top, testFrameworkElement.Margin.Right, testFrameworkElement.Margin.Bottom);
      } else {
        MarginLeftTextBox.Text = "0";
        testFrameworkElement.Margin = new Thickness(0, testFrameworkElement.Margin.Top, testFrameworkElement.Margin.Right, testFrameworkElement.Margin.Bottom);
      }
    }


    void marginTopTextBox_LostFocus(object sender, RoutedEventArgs e) {
      if (int.TryParse(MarginTopTextBox.Text, out var newMargin)) {
        testFrameworkElement.Margin = new Thickness(testFrameworkElement.Margin.Left, newMargin, testFrameworkElement.Margin.Right, testFrameworkElement.Margin.Bottom);
      } else {
        MarginTopTextBox.Text = "0";
        testFrameworkElement.Margin = new Thickness(testFrameworkElement.Margin.Left, 0, testFrameworkElement.Margin.Right, testFrameworkElement.Margin.Bottom);
      }
    }


    void marginRightTextBox_LostFocus(object sender, RoutedEventArgs e) {
      if (int.TryParse(MarginRightTextBox.Text, out var newMargin)) {
        testFrameworkElement.Margin = new Thickness(testFrameworkElement.Margin.Left, testFrameworkElement.Margin.Top, newMargin, testFrameworkElement.Margin.Bottom);
      } else {
        MarginRightTextBox.Text = "0";
        testFrameworkElement.Margin = new Thickness(testFrameworkElement.Margin.Left, testFrameworkElement.Margin.Top, 0, testFrameworkElement.Margin.Bottom);
      }
    }


    void marginBottomTextBox_LostFocus(object sender, RoutedEventArgs e) {
      if (int.TryParse(MarginBottomTextBox.Text, out var newMargin)) {
        testFrameworkElement.Margin = new Thickness(testFrameworkElement.Margin.Left, testFrameworkElement.Margin.Top, testFrameworkElement.Margin.Right, newMargin);
      } else {
        MarginBottomTextBox.Text = "0";
        testFrameworkElement.Margin = new Thickness(testFrameworkElement.Margin.Left, testFrameworkElement.Margin.Top, testFrameworkElement.Margin.Right, 0);
      }
    }
    #endregion


    #region Border & Padding TextBoxes
    //      --------------------------

    private void setupBorderPaddingTextBoxes() {
      setupTextBox(BorderLeftTextBox, borderLeftTextBox_LostFocus);
      setupTextBox(BorderTopTextBox, borderTopTextBox_LostFocus);
      setupTextBox(BorderRightTextBox, borderRightTextBox_LostFocus);
      setupTextBox(BorderBottomTextBox, borderBottomTextBox_LostFocus);
      updateBorderTextBoxes();
      setupTextBox(PaddingLeftTextBox, paddingLeftTextBox_LostFocus);
      setupTextBox(PaddingTopTextBox, paddingTopTextBox_LostFocus);
      setupTextBox(PaddingRightTextBox, paddingRightTextBox_LostFocus);
      setupTextBox(PaddingBottomTextBox, paddingBottomTextBox_LostFocus);
      updatePaddingTextBoxes();
    
      DependencyPropertyDescriptor borderDescriptor = DependencyPropertyDescriptor.FromProperty(Control.BorderThicknessProperty, typeof(Control));
      borderDescriptor.AddValueChanged(testControl, testControl_BorderChanged);
    
      DependencyPropertyDescriptor paddingDescriptor = DependencyPropertyDescriptor.FromProperty(Control.PaddingProperty, typeof(Control));
      paddingDescriptor.AddValueChanged(testControl, testControl_PaddingChanged);
    }


    void testControl_BorderChanged(object? sender, EventArgs args) {
      updateBorderTextBoxes();
    }


    private void updateBorderTextBoxes() {
      BorderLeftTextBox.Text = testControl!.BorderThickness.Left.ToString(".0");
      BorderTopTextBox.Text = testControl.BorderThickness.Top.ToString(".0");
      BorderRightTextBox.Text = testControl.BorderThickness.Right.ToString(".0");
      BorderBottomTextBox.Text = testControl.BorderThickness.Bottom.ToString(".0");
    }


    void testControl_PaddingChanged(object? sender, EventArgs args) {
      updatePaddingTextBoxes();
    }


    private void updatePaddingTextBoxes() {
      PaddingLeftTextBox.Text = testControl!.Padding.Left.ToString(".0");
      PaddingTopTextBox.Text = testControl.Padding.Top.ToString(".0");
      PaddingRightTextBox.Text = testControl.Padding.Right.ToString(".0");
      PaddingBottomTextBox.Text = testControl.Padding.Bottom.ToString(".0");
    }


    void borderLeftTextBox_LostFocus(object? sender, RoutedEventArgs e) {
      if (int.TryParse(BorderLeftTextBox.Text, out var newBorder) && newBorder>0) {
        testControl!.BorderThickness = new Thickness(newBorder, testControl.BorderThickness.Top, testControl.BorderThickness.Right, testControl.BorderThickness.Bottom);
      } else {
        BorderLeftTextBox.Text = "0";
        testControl!.BorderThickness = new Thickness(0, testControl.BorderThickness.Top, testControl.BorderThickness.Right, testControl.BorderThickness.Bottom);
      }
    }


    void borderTopTextBox_LostFocus(object? sender, RoutedEventArgs e) {
      if (int.TryParse(BorderTopTextBox.Text, out var newBorder) && newBorder>0) {
        testControl!.BorderThickness = new Thickness(testControl.BorderThickness.Left, newBorder, testControl.BorderThickness.Right, testControl.BorderThickness.Bottom);
      } else {
        BorderTopTextBox.Text = "0";
        testControl!.BorderThickness = new Thickness(testControl.BorderThickness.Left, 0, testControl.BorderThickness.Right, testControl.BorderThickness.Bottom);
      }
    }


    void borderRightTextBox_LostFocus(object? sender, RoutedEventArgs e) {
      if (int.TryParse(BorderRightTextBox.Text, out var newBorder) && newBorder>0) {
        testControl!.BorderThickness = new Thickness(testControl.BorderThickness.Left, testControl.BorderThickness.Top, newBorder, testControl.BorderThickness.Bottom);
      } else {
        BorderRightTextBox.Text = "0";
        testControl!.BorderThickness = new Thickness(testControl.BorderThickness.Left, testControl.BorderThickness.Top, 0, testControl.BorderThickness.Bottom);
      }
    }


    void borderBottomTextBox_LostFocus(object? sender, RoutedEventArgs e) {
      if (int.TryParse(BorderBottomTextBox.Text, out var newBorder) && newBorder>0) {
        testControl!.BorderThickness = new Thickness(testControl.BorderThickness.Left, testControl.BorderThickness.Top, testControl.BorderThickness.Right, newBorder);
      } else {
        BorderBottomTextBox.Text = "0";
        testControl!.BorderThickness = new Thickness(testControl.BorderThickness.Left, testControl.BorderThickness.Top, testControl.BorderThickness.Right, 0);
      }
    }


    void paddingLeftTextBox_LostFocus(object? sender, RoutedEventArgs e) {
      if (int.TryParse(PaddingLeftTextBox.Text, out var newPadding)) {
        testControl!.Padding = new Thickness(newPadding, testControl.Padding.Top, testControl.Padding.Right, testControl.Padding.Bottom);
      } else {
        PaddingLeftTextBox.Text = "0";
        testControl!.Padding = new Thickness(0, testControl.Padding.Top, testControl.Padding.Right, testControl.Padding.Bottom);
      }
    }


    void paddingTopTextBox_LostFocus(object? sender, RoutedEventArgs e) {
      if (int.TryParse(PaddingTopTextBox.Text, out var newPadding)) {
        testControl!.Padding = new Thickness(testControl.Padding.Left, newPadding, testControl.Padding.Right, testControl.Padding.Bottom);
      } else {
        PaddingTopTextBox.Text = "0";
        testControl!.Padding = new Thickness(testControl.Padding.Left, 0, testControl.Padding.Right, testControl.Padding.Bottom);
      }
    }


    void paddingRightTextBox_LostFocus(object? sender, RoutedEventArgs e) {
      if (int.TryParse(PaddingRightTextBox.Text, out var newPadding)) {
        testControl!.Padding = new Thickness(testControl.Padding.Left, testControl.Padding.Top, newPadding, testControl.Padding.Bottom);
      } else {
        PaddingRightTextBox.Text = "0";
        testControl!.Padding = new Thickness(testControl.Padding.Left, testControl.Padding.Top, 0, testControl.Padding.Bottom);
      }
    }


    void paddingBottomTextBox_LostFocus(object? sender, RoutedEventArgs e) {
      if (int.TryParse(PaddingBottomTextBox.Text, out var newPadding)) {
        testControl!.Padding = new Thickness(testControl.Padding.Left, testControl.Padding.Top, testControl.Padding.Right, newPadding);
      } else {
        PaddingBottomTextBox.Text = "0";
        testControl!.Padding = new Thickness(testControl.Padding.Left, testControl.Padding.Top, testControl.Padding.Right, 0);
      }
    }
    #endregion


    #region Font ComboBoxes
    //      ---------------

    void setupFontComboBoxes() {
      if (testControl==null) return; //TestFrameworkElement does not support fonts

      FontFamilyComboBox.SelectedItem = testControl.FontFamily;
      WpfBinding.Setup(FontFamilyComboBox, "SelectedItem", testFrameworkElement, Control.FontFamilyProperty, BindingMode.TwoWay);

      FontWeightComboBox.ItemsSource = new FontWeight[]{
        FontWeights.Black,
        FontWeights.Bold,
        FontWeights.DemiBold,
        FontWeights.ExtraBlack,
        FontWeights.ExtraBold,
        FontWeights.ExtraLight,
        FontWeights.Heavy,
        FontWeights.Light,
        FontWeights.Medium,
        FontWeights.Normal,
        FontWeights.Regular,
        FontWeights.SemiBold,
        FontWeights.Thin,
        FontWeights.UltraBlack,
        FontWeights.UltraBold,
        FontWeights.UltraLight
      };
      FontWeightComboBox.SelectedItem = testControl.FontWeight;
      WpfBinding.Setup(FontWeightComboBox, "SelectedItem", testFrameworkElement, Control.FontWeightProperty, BindingMode.TwoWay);

      FontSizeComboBox.Items.Add(new ComboBoxItem {Content = "8", HorizontalAlignment = HorizontalAlignment.Right});
      FontSizeComboBox.Items.Add(new ComboBoxItem {Content = "9", HorizontalAlignment = HorizontalAlignment.Right});
      FontSizeComboBox.Items.Add(new ComboBoxItem {Content = "10", HorizontalAlignment = HorizontalAlignment.Right});
      FontSizeComboBox.Items.Add(new ComboBoxItem {Content = "11", HorizontalAlignment = HorizontalAlignment.Right});
      FontSizeComboBox.Items.Add(new ComboBoxItem {Content = "12", HorizontalAlignment = HorizontalAlignment.Right});
      FontSizeComboBox.Items.Add(new ComboBoxItem {Content = "14", HorizontalAlignment = HorizontalAlignment.Right});
      FontSizeComboBox.Items.Add(new ComboBoxItem {Content = "16", HorizontalAlignment = HorizontalAlignment.Right});
      FontSizeComboBox.Items.Add(new ComboBoxItem {Content = "18", HorizontalAlignment = HorizontalAlignment.Right});
      FontSizeComboBox.Items.Add(new ComboBoxItem {Content = "20", HorizontalAlignment = HorizontalAlignment.Right});
      FontSizeComboBox.Items.Add(new ComboBoxItem {Content = "22", HorizontalAlignment = HorizontalAlignment.Right});
      FontSizeComboBox.Items.Add(new ComboBoxItem {Content = "24", HorizontalAlignment = HorizontalAlignment.Right});
      FontSizeComboBox.Items.Add(new ComboBoxItem {Content = "26", HorizontalAlignment = HorizontalAlignment.Right});
      FontSizeComboBox.Items.Add(new ComboBoxItem {Content = "28", HorizontalAlignment = HorizontalAlignment.Right});
      FontSizeComboBox.Items.Add(new ComboBoxItem {Content = "36", HorizontalAlignment = HorizontalAlignment.Right});
      FontSizeComboBox.Items.Add(new ComboBoxItem {Content = "48", HorizontalAlignment = HorizontalAlignment.Right});
      FontSizeComboBox.Items.Add(new ComboBoxItem {Content = "72", HorizontalAlignment = HorizontalAlignment.Right});
      FontSizeComboBox.SelectedIndex = 4;
      string searchIntString = testControl.FontSize.ToString();
      for (int itemIndex = 0; itemIndex < FontSizeComboBox.Items.Count; itemIndex++){
        ComboBoxItem comboBoxItem = (ComboBoxItem)FontSizeComboBox.Items[itemIndex];
        if (((string)comboBoxItem.Content)==searchIntString) {
          FontSizeComboBox.SelectedIndex = itemIndex;
          break;
        }
			}
      WpfBinding.Setup(FontSizeComboBox, "Text", testFrameworkElement, Control.FontSizeProperty, BindingMode.TwoWay);

      FontStretchComboBox.ItemsSource = new FontStretch[]{
        FontStretches.UltraCondensed, 
        FontStretches.ExtraCondensed, 
        FontStretches.Condensed, 
        FontStretches.SemiCondensed, 
        //FontStretches.Medium, same as Normal
        FontStretches.Normal, 
        FontStretches.Expanded, 
        FontStretches.SemiExpanded, 
        FontStretches.ExtraExpanded, 
        FontStretches.UltraExpanded};
      FontStretchComboBox.SelectedItem = testControl.FontStretch;
      WpfBinding.Setup(FontStretchComboBox, "SelectedItem", testFrameworkElement, Control.FontStretchProperty, BindingMode.TwoWay);

      FontStyleComboBox.ItemsSource = new FontStyle[]{
        FontStyles.Normal,
        FontStyles.Italic,
        FontStyles.Oblique
      };
      FontStyleComboBox.SelectedItem = testControl.FontStyle;
      WpfBinding.Setup(FontStyleComboBox, "SelectedItem", testFrameworkElement, Control.FontStyleProperty, BindingMode.TwoWay);

      ForegroundColorComboBox.SetSelectedBrush(testControl.Foreground);
      WpfBinding.Setup(ForegroundColorComboBox, "SelectedColorBrush", testFrameworkElement, Control.ForegroundProperty, BindingMode.TwoWay);
//      WpfBinding.Setup(ForegroundColorComboBox, "SelectedColorBrush", testFrameworkElement, Control.ForegroundProperty, BindingMode.TwoWay, new PassThroughConverter());
      BackgroundColorComboBox.SetSelectedBrush(testControl.Background);
      WpfBinding.Setup(BackgroundColorComboBox, "SelectedColorBrush", testFrameworkElement, Control.BackgroundProperty, BindingMode.TwoWay);
      BorderColorComboBox.SetSelectedBrush(testControl.BorderBrush);
      WpfBinding.Setup(BorderColorComboBox, "SelectedColorBrush", testFrameworkElement, Control.BorderBrushProperty, BindingMode.TwoWay);
    }
    #endregion
  }
}