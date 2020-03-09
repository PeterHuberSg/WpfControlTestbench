using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System;


namespace WpfTestbench {


  /// <summary>
  /// Displays all standard WPF colors in a dropdown.
  /// </summary>
  public class StandardColorComboBox: ComboBox {

    public static readonly DependencyProperty SelectedColorBrushProperty =
      DependencyProperty.Register("SelectedColorBrush", typeof(Brush), typeof(StandardColorComboBox), new PropertyMetadata(SelectedColorBrushChanged));


    private static void SelectedColorBrushChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
      StandardColorComboBox standardColorComboBox = (StandardColorComboBox)d;
      standardColorComboBox.SetSelectedBrush((Brush)e.NewValue);
    }


    /// <summary>
    /// Can be used for binding to the selected color brush. The new brush gets added to SelectedColorBrush if not available yet.
    /// </summary>
    public Brush SelectedColorBrush {
      get { return (Brush)GetValue(SelectedColorBrushProperty); }
      private set { SetValue(SelectedColorBrushProperty, value); }
    }


    static int newBrushCount=0;

    public void SetSelectedBrush(Brush setBrush) {
      SolidColorBrush setSolidColorBrush = setBrush as SolidColorBrush;

      if (setSolidColorBrush==null) {
        //not a SolidColorBrush. look for the same brush object
        int itemIndex;
        for (itemIndex = 0; itemIndex < Items.Count; itemIndex++) {
          ColorSamplePanel colorSamplePanel = (ColorSamplePanel)Items[itemIndex];
          if (colorSamplePanel.ColorBrush==setBrush) {
            SelectedIndex = itemIndex;
            return;
          }
        }
        //brush not found, add it
        Items.Insert(0, new ColorSamplePanel("Brush"+(++newBrushCount), setBrush));
        SelectedIndex = 0;

      } else {
        //is a SolidColorBrush. Look for the same color
        int itemIndex;
        for (itemIndex = 0; itemIndex < Items.Count; itemIndex++) {
          ColorSamplePanel colorSamplePanel = (ColorSamplePanel)Items[itemIndex];
          SolidColorBrush sampleSolidColorBrush = colorSamplePanel.ColorBrush as SolidColorBrush;
          if (sampleSolidColorBrush!=null && sampleSolidColorBrush.Color==setSolidColorBrush.Color) {
            SelectedIndex = itemIndex;
            return;
          }
        }
        //brush not found, add it
        Items.Insert(0, new ColorSamplePanel("Brush"+(++newBrushCount), setBrush));
        SelectedIndex = 0;
      }
    }


    ///////// <summary>
    ///////// Can be used to set dfault colour from XAML
    ///////// </summary>
    //////public string SelectedColorName {
    //////  get { return (string)GetValue(SelectedColorNameProperty); }
    //////  set {
    //////    string valueLowercase = value.ToLowerInvariant();
    //////    for (int sampleIndex = 0; sampleIndex < Items.Count; sampleIndex++) {
    //////      var colorSamplePanel = (ColorSamplePanel)Items[sampleIndex];
    //////      if (colorSamplePanel.ColorName.ToLowerInvariant()==valueLowercase) {
    //////        SelectedIndex = sampleIndex;
    //////        SetValue(SelectedColorNameProperty, colorSamplePanel.ColorName); 
    //////        return;
    //////      }
    //////    }
    //////    throw new Exception("Unknown ColorName: " + value + ".");
    //////  }
    //////}


    //////public static readonly DependencyProperty SelectedColorNameProperty = 
    //////DependencyProperty.Register("SelectedColorName", typeof(string), typeof(ColorSamplePanel), new FrameworkPropertyMetadata("Green"));


    public StandardColorComboBox() {
      foreach (PropertyInfo brushPropertyInfo in typeof(Brushes).GetProperties()) {
        SolidColorBrush brush = (SolidColorBrush)brushPropertyInfo.GetValue(null, null);
        Items.Add(new ColorSamplePanel(brushPropertyInfo.Name, brush));
      }
      //////      TextSearch.SetTextPath(this, "ColorName");
      SelectedValuePath = "ColorBrush";
      SelectionChanged += StandardColorComboBox_SelectionChanged;
    }


    void StandardColorComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e) {
      if (e.AddedItems.Count>0) {
        var colorSamplePanel = (ColorSamplePanel)e.AddedItems[0];
        SelectedColorBrush = colorSamplePanel.ColorBrush;
        //////SetValue(SelectedColorNameProperty, colorSamplePanel.ColorName);
      }
    }


    /////// <summary>
    /////// StandardColorComboBox supports only standard colors. SetClosestColour takes as input any color and
    /////// sets the closest standard color
    /////// </summary>
    ////public Color SetClosestStandardColour(Brush setColorBrush) {
    ////  SolidColorBrush setSolidColorBrush = setColorBrush as SolidColorBrush;
    ////  if (setSolidColorBrush==null) return Colors.DarkRed;

    ////  Color setColor = setSolidColorBrush.Color;
    ////  foreach (ColorSamplePanel item in Items) {
    ////    SolidColorBrush sampleSolidColorBrush = item.ColorBrush as SolidColorBrush;
    ////    SolidColorBrush foundSolidColorBrush = null;
    ////    int minSquareDiff = int.MaxValue;
    ////    if (sampleSolidColorBrush!=null) {
    ////      if (sampleSolidColorBrush.Color==setSolidColorBrush.Color) {
    ////        foundSolidColorBrush = sampleSolidColorBrush;
    ////        break;
    ////      }
    ////      Color c1 = sampleSolidColorBrush.Color;
    ////      Color c2 = setSolidColorBrush.Color;
    ////      int squareDiff = square(c1.R-c2.R) + square(c1.G-c2.G) + square(c1.B-c2.B);
    ////      if (minSquareDiff>squareDiff) {
    ////        minSquareDiff = squareDiff;
    ////        foundSolidColorBrush = sampleSolidColorBrush;
    ////      }
    ////    }

    ////  }
    ////  return setColor;
    ////}


    private int square(int p) {
      return p*p;
    }
  }



  public class ColorSamplePanel: StackPanel {


    public string ColorName { get; private set; }
    public Brush ColorBrush { get; private set; }

    Rectangle colorRectangle;
    TextBlock textBlock;


    public ColorSamplePanel(string colorName, Brush colorBrush) {
      ColorName = colorName;
      ColorBrush = colorBrush;
      Orientation = Orientation.Horizontal;
      //TextSearch.SetText(this, colorName);

      colorRectangle = new Rectangle();
      colorRectangle.Fill = colorBrush;
      colorRectangle.Margin = new Thickness(2, 0, 2, 0);
      colorRectangle.VerticalAlignment = VerticalAlignment.Center;
      Children.Add(colorRectangle);

      this.VerticalAlignment = VerticalAlignment.Stretch;
      textBlock = new TextBlock();
      textBlock.Text = colorName;
      Children.Add(textBlock);

      SizeChanged += ColorSamplePanel_SizeChanged;
    }


    void ColorSamplePanel_SizeChanged(object sender, SizeChangedEventArgs e) {
      double dimensions = e.NewSize.Height * 2 / 3;
      colorRectangle.Height = dimensions;
      colorRectangle.Width = dimensions;
    }


    public override string ToString() {
      return textBlock.Text;
    }
  }
}