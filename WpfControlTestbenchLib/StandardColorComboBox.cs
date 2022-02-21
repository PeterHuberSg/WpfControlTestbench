using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System;
using System.Collections.Generic;
using System.Linq;

namespace WpfTestbench {

  #region StandardColorComboBox
  //      =====================

  /// <summary>
  /// Displays all standard WPF colors in a dropdown.
  /// </summary>
  public class StandardColorComboBox: ComboBox {
    #region static colors
    //      -------------

    class ColorItem {
      public string Name { get; }
      public Color Color { get; }
      public (int Hue, double Saturation, double Brightness) HSB { get; }
      public int Hue { get; }
      public double Rank { get; }
      public SolidColorBrush Brush { get; }

      public ColorItem(string name, Color color) {
        Name = name;
        Color = color;
        Brush = new SolidColorBrush(Color);
        HSB = getHSB(color);
        Hue = color.R==color.G && color.G==color.B ? -10 : HSB.Hue;
        Rank = 10000 * Hue + 10000 * (1-Math.Round(HSB.Saturation, 2)) + 100 * (1-Math.Round(HSB.Brightness, 2));
      }

      private static (int Hue, double Saturation, double Brightness) getHSB(Color color) {
        int max = Math.Max(color.R, Math.Max(color.G, color.B));
        int min = Math.Min(color.R, Math.Min(color.G, color.B));
        int hue = 0;//for black, gray or white, hue could be actually any number, but usually 0 is assign, which means red
        if (max-min!=0) {
          //not black, gray or white
          int maxMinDif = max-min;
          if (max==color.R) {
            #pragma warning disable IDE0045 // Convert to conditional expression
            if (color.G>=color.B) {
            #pragma warning restore IDE0045
              hue = 60 * (color.G-color.B)/maxMinDif;
            } else {
              hue = 60 * (color.G-color.B)/maxMinDif + 360;
            }
          } else if (max==color.G) {
            hue = 60 * (color.B-color.R)/maxMinDif + 120;
          } else if (max == color.B) {
            hue = 60 * (color.R-color.G)/maxMinDif + 240;
          }
        }

        double saturation = (max == 0) ? 0.0 : (1.0-((double)min/(double)max));

        return (hue, saturation, (double)max/0xFF);
      }

      public override string ToString() {
        return $"{Name} {Color} {Rank}";
      }
    }


    static readonly List<ColorItem> colorItems = createColorItems();


    private static List<ColorItem> createColorItems() {
      var colorsQuery = typeof(Colors)
        .GetProperties(BindingFlags.Static | BindingFlags.Public)
        .Select(p => new ColorItem(p.Name, (Color)p.GetValue(null, null)!));
      return colorsQuery.OrderBy(ci => ci.Rank).ToList();
    }
    #endregion


    #region Properties
    //      ----------

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
      if (setBrush is SolidColorBrush setSolidColorBrush) {
        //is a SolidColorBrush. Look for the same color
        int itemIndex;
        for (itemIndex = 0; itemIndex < Items.Count; itemIndex++) {
          ColorSamplePanel colorSamplePanel = (ColorSamplePanel)Items[itemIndex];
          if (colorSamplePanel.ColorBrush is SolidColorBrush sampleSolidColorBrush && 
            sampleSolidColorBrush.Color==setSolidColorBrush.Color) 
          {
            SelectedIndex = itemIndex;
            return;
          }
        }
        //brush not found, add it
        Items.Insert(0, new ColorSamplePanel("Brush"+(++newBrushCount), setBrush));
        SelectedIndex = 0;

      } else {
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
    #endregion


    #region Constructor
    //      -----------
    public StandardColorComboBox() {
      //foreach (PropertyInfo brushPropertyInfo in typeof(Brushes).GetProperties()) {
      //  SolidColorBrush brush = (SolidColorBrush)brushPropertyInfo.GetValue(null, null);
      //  Items.Add(new ColorSamplePanel(brushPropertyInfo.Name, brush));
      //}
      foreach (var coloritem in colorItems) {
        Items.Add(new ColorSamplePanel(coloritem.Name, coloritem.Brush));
      }
      //////      TextSearch.SetTextPath(this, "ColorName");
      SelectedValuePath = "ColorBrush";
      SelectionChanged += StandardColorComboBox_SelectionChanged;
    }
    #endregion


    #region Eventhandler
    //      ------------

    void StandardColorComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e) {
      if (e.AddedItems.Count>0) {
        var colorSamplePanel = (ColorSamplePanel)e.AddedItems[0];
        SelectedColorBrush = colorSamplePanel.ColorBrush;
        //////SetValue(SelectedColorNameProperty, colorSamplePanel.ColorName);
      }
    }
    #endregion

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
  }
  #endregion


  #region ColorSamplePanel
  //      ================

  public class ColorSamplePanel: StackPanel {

    #region Properties
    //      ----------

    public string ColorName {get;}
    public Brush ColorBrush {get;}

    readonly Rectangle colorRectangle;
    readonly TextBlock textBlock;
    #endregion


    #region Constructor
    //      -----------

    public ColorSamplePanel(string colorName, Brush colorBrush) {
      ColorName = colorName;
      ColorBrush = colorBrush;
      Orientation = Orientation.Horizontal;
      //TextSearch.SetText(this, colorName);

      colorRectangle = new Rectangle {
        Fill = colorBrush,
        Margin = new Thickness(2, 0, 2, 0),
        VerticalAlignment = VerticalAlignment.Center
      };
      Children.Add(colorRectangle);

      this.VerticalAlignment = VerticalAlignment.Stretch;
      textBlock = new TextBlock {
        Text = colorName
      };
      Children.Add(textBlock);

      SizeChanged += ColorSamplePanel_SizeChanged;
    }
    #endregion


    #region Eventhandler
    //      ------------

    void ColorSamplePanel_SizeChanged(object sender, SizeChangedEventArgs e) {
      double dimensions = e.NewSize.Height * 2 / 3;
      colorRectangle.Height = dimensions;
      colorRectangle.Width = dimensions;
    }
    #endregion


    #region Methods
    //      -------

    public override string ToString() {
      return textBlock.Text;
    }
    #endregion
  }
  #endregion
}