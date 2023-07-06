/********************************************************************************************************

WpfTestbench.StandardColorComboBox
==================================

Displays all the colors defined in Colors in a ComboBox

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
            if (color.G>=color.B) {
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
    #endregion


    #region Constructor
    //      -----------
  
    public StandardColorComboBox() {
      foreach (var colorItem in colorItems) {
        Items.Add(new ColorSamplePanel(colorItem.Name, colorItem.Brush));
      }
      SelectedValuePath = "ColorBrush";
      SelectionChanged += StandardColorComboBox_SelectionChanged;
    }
    #endregion


    #region Eventhandler
    //      ------------

    void StandardColorComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e) {
      if (e.AddedItems.Count>0) {
        var colorSamplePanel = (ColorSamplePanel)e.AddedItems[0]!;
        SelectedColorBrush = colorSamplePanel.ColorBrush;
      }
    }
    #endregion
  }
  #endregion


  #region ColorSamplePanel
  //      ================

  public class ColorSamplePanel: StackPanel {

    #region Properties
    //      ----------

    public string ColorName {get;}
    public Brush ColorBrush {get;}
    #endregion


    #region Constructor
    //      -----------

    readonly Rectangle colorRectangle;
    readonly TextBlock textBlock;

    public ColorSamplePanel(string colorName, Brush colorBrush) {
      ColorName = colorName;
      ColorBrush = colorBrush;
      Orientation = Orientation.Horizontal;
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