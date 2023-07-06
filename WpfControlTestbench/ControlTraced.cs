/********************************************************************************************************

WpfTestbench.StackPanelTraced
=============================

StackPanel with event tracing for TestBench.

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
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;


namespace WpfTestbench {


  /// <summary>
  /// Helper class to allow using StackPanelTraced constructor with a parameter.
  /// </summary>
  public class ControlWithConstructor: Control {
    public ControlWithConstructor(object? _) : base() { }
  }


  /// <summary>
  /// A control writing Text into a Rectangle filled with Fill Brush colour. The rectangle.X = 1/5 control's 
  /// width, rectangle.Y = 1/5 control's height, rectangle.Width = 2/5, rectangle.Height = 2/5. 
  /// </summary>
  public class ControlTraced: ControlWithConstructor, ITraceName{

    #region Property
    //      --------

    /// <summary>
    /// DependencyProperty for Fill property.
    /// </summary>
    public static readonly DependencyProperty FillProperty =
      DependencyProperty.RegisterAttached(
        "Fill",
        typeof(Brush),
        typeof(ControlTraced),
        new FrameworkPropertyMetadata(
          null,
          FrameworkPropertyMetadataOptions.AffectsMeasure |
          FrameworkPropertyMetadataOptions.AffectsRender));

    /// <summary>
    /// The Fill brush of the inner rectangle
    /// </summary>
    public Brush? Fill {
      get { return (Brush?)GetValue(FillProperty); }
      set { SetValue(FillProperty, value); }
    }


    /// <summary>
    /// DependencyProperty for Text property.
    /// </summary>
    public static readonly DependencyProperty TextProperty =
      DependencyProperty.RegisterAttached(
        "Text",
        typeof(string),
        typeof(ControlTraced),
        new FrameworkPropertyMetadata(
          null,
          FrameworkPropertyMetadataOptions.AffectsMeasure |
          FrameworkPropertyMetadataOptions.AffectsRender));

    /// <summary>
    /// The Text the control can display.
    /// </summary>
    public string? Text {
      get { return (string?)GetValue(TextProperty); }
      set { SetValue(TextProperty, value); }
    }


    /// <summary>
    /// Name to be used for tracing
    /// </summary>
    public string TraceName { get; private set; }
    #endregion


    #region Constructor
    //      -----------

    static ControlTraced() {
      Control.BorderThicknessProperty.OverrideMetadata(typeof(ControlTraced), new FrameworkPropertyMetadata(new Thickness(), FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender));
      Control.BorderBrushProperty.OverrideMetadata(typeof(ControlTraced), new FrameworkPropertyMetadata(Brushes.Black, FrameworkPropertyMetadataOptions.AffectsRender));
      Control.PaddingProperty.OverrideMetadata(typeof(ControlTraced), new FrameworkPropertyMetadata(new Thickness(), FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender));
      Control.BackgroundProperty.OverrideMetadata(typeof(ControlTraced), new FrameworkPropertyMetadata(Brushes.LightYellow, FrameworkPropertyMetadataOptions.AffectsRender));
      Control.HorizontalContentAlignmentProperty.OverrideMetadata(typeof(ControlTraced), new FrameworkPropertyMetadata(HorizontalAlignment.Left, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender));
      Control.VerticalContentAlignmentProperty.OverrideMetadata(typeof(ControlTraced), new FrameworkPropertyMetadata(VerticalAlignment.Top, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender));
    }


    /// <summary>
    /// Default Constructor
    /// </summary>
    public ControlTraced() : this("Control") { }


    #pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    public ControlTraced(string traceName): base(TraceWPFEvents.TraceCreateStart(traceName)) {
    #pragma warning restore CS8618
      TraceName = traceName;
      TraceWPFEvents.TraceCreateEnd(traceName);
    }
    #endregion


    #region Event Tracing
    //      -------------

    protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e) {
      TraceWPFEvents.OnPropertyChanged(this, e, base.OnPropertyChanged);
    }


    const int minWidth = 250;
    const int minHeight = 150;


    protected override Size MeasureOverride(Size constraint) {
      return TraceWPFEvents.MeasureOverride(this, constraint, doMeasureOverride);
      //return TraceWPFEvents.MeasureOverride(this, constraint, null);
    }


    private Size doMeasureOverride(Size constraint) {
      var width = constraint.Width==double.PositiveInfinity ? minWidth : constraint.Width;
      var height = constraint.Height==double.PositiveInfinity ? minHeight :constraint.Height;
      return new Size(width, height);
    }


    protected override Size ArrangeOverride(Size finalSize) {
      return TraceWPFEvents.ArrangeOverride(this, finalSize, null);
    }

    /*
                  RenderSize.Width
      +--------------------------------------------------+
    R |   BorderThickness.Top                            |
    e | B +++++++++++++++++++++++++++++++++++++++++++++++|
    n | o +      Padding.Top                            +|
    d | r +                  ContentWidth               +|
    e | d + P    +------------------------------------+ +|
    r | e + a    |      ContentHeight/5               | +|
    S | r + d  C |               2*CW/5               | +|
    i | T + d  H |      +++++++++++++++++++++++       | +|
    z | . + i  e | CW/5 +       Text.Width    +2*CW/5 | +|
    e | L + n  i |      +    +--------------+ +       | +|
    . | e + g  g |      +    |     Hi       | +       | +|
    H | f + .  h |      +    +--------------+ +       | +|
    e | t + L  t |      +                     +       | +|
    i |   + e    |      +++++++++++++++++++++++       | +|
    g |   + f    |      2*ContentHeight/5             | +|
    h |   + t    +------------------------------------+ +|
    t |   +++++++++++++++++++++++++++++++++++++++++++++++|
      +--------------------------------------------------+
    */
    static readonly Thickness emptyThickness = new(0);


    Typeface? typeface;
    FontFamily fontFamily;
    FontStyle fontStyle;
    FontWeight fontWeight;
    FontStretch fontStretch;
    GlyphTypeface glyphTypeface;
    double fontSize;
    string text;
    double totalTextWidth;
    float pixelsPerDip;
    Point origin;
    ushort[] glyphIndexes;
    double[] advanceWidths;
    GlyphRun glyphRun;


    protected override void OnRender(DrawingContext drawingContext) {
      TraceWPFEvents.OnRender(this, drawingContext, base.OnRender);
      if (RenderSize.Width==0 || RenderSize.Height==0) return;

      if (BorderThickness.Top + BorderThickness.Bottom > RenderSize.Height ||
          BorderThickness.Left + BorderThickness.Right > RenderSize.Width) 
      {
        //just enough space to draw border
        if (BorderBrush is null || BorderBrush==Brushes.Transparent) return;

        drawingContext.DrawRectangle(BorderBrush, null, new Rect(RenderSize));
        return;
      }

      if (Background is not null && Background!=Brushes.Transparent) {
        drawingContext.DrawRectangle(Background, null, new Rect(RenderSize));
      }

      if (BorderBrush is not null && BorderBrush!=Brushes.Transparent && BorderThickness!=emptyThickness) {
        //left border
        drawingContext.DrawRectangle(BorderBrush, null,
          new Rect(0, 0, BorderThickness.Left, RenderSize.Height));
        //top border
        drawingContext.DrawRectangle(BorderBrush, null,
          new Rect(0, 0, RenderSize.Width, BorderThickness.Top));
        //right border
        drawingContext.DrawRectangle(BorderBrush, null,
          new Rect(RenderSize.Width-BorderThickness.Right, 0, BorderThickness.Right, RenderSize.Height));
        //bottom border
        drawingContext.DrawRectangle(BorderBrush, null,
          new Rect(0, RenderSize.Height-BorderThickness.Bottom, RenderSize.Width, BorderThickness.Bottom));
      }

      var contentWidth = RenderSize.Width - BorderThickness.Left - Padding.Left - 
        Padding.Right - BorderThickness.Right;
      if (contentWidth<=0) return;

      var contentHeight = RenderSize.Height - BorderThickness.Top - Padding.Top -
        Padding.Bottom - BorderThickness.Bottom;
      if (contentHeight<=0) return;

      //inside
      var insideX = BorderThickness.Left + Padding.Left + contentWidth/5;
      var insideY = BorderThickness.Top + Padding.Top + contentHeight/5;
      var insideWidth = 2*contentWidth/5;
      var insideHeight = 2*contentHeight/5;
      var insideRect = new Rect(insideX, insideY, insideWidth, insideHeight);
      if (Fill is not null) {
        drawingContext.DrawRectangle(Fill, null, insideRect);
      }

      if (Text is null) return;

      //detect font changes
      var hasFontChanged = false;
      if (typeface is null || fontFamily!=FontFamily || fontStyle!=FontStyle || fontWeight!=FontWeight || 
        fontStretch!=FontStretch) 
      {
        hasFontChanged = true;
        fontFamily = FontFamily;
        fontStyle = FontStyle;
        fontWeight = FontWeight;
        fontStretch = FontStretch;
        typeface = new Typeface(fontFamily, fontStyle, fontWeight, fontStretch);
        if (!typeface.TryGetGlyphTypeface(out glyphTypeface))
          throw new InvalidOperationException("No GlyphTypeface found");
      }

      var hasTextFontChanged = false;
      if (text!=Text || hasFontChanged || fontSize!=FontSize) {
        hasTextFontChanged = true;
        text = Text;
        fontSize = FontSize;
        totalTextWidth = 0;
        glyphIndexes = new ushort[Text.Length];
        advanceWidths = new double[Text.Length];

        var glyphIndexesIndex = 0;
        for (int charIndex = 0; charIndex<Text.Length; charIndex++) {
          var codePoint = (int)Text[charIndex];
          if (codePoint<0xd800) {
            // codePoint consists of only 1 integer, nothing to do
          } else if (codePoint<0xdc00) {
            //high surrogate code point
            if (charIndex>=Text.Length) {
              //low surrogate code point missing
              System.Diagnostics.Debugger.Break();
              codePoint = (int)'?';
            } else {
              var lowCodPoint = (int)Text[++charIndex];
              if (lowCodPoint<0xdc00 || lowCodPoint>=0xe000) {
                //illegal second surrogate code point
                System.Diagnostics.Debugger.Break();
                codePoint = (int)'?';
              } else {
                codePoint = 0x10000 + ((codePoint - 0xD800) *0x0400) + (lowCodPoint - 0xDC00);
              }
            }
          } else if (codePoint<0xe000) {
            //illegal low surrogate code point, high should come first
            System.Diagnostics.Debugger.Break();
            codePoint = (int)'?';
          } else {
            // codePoint consists of only 1 integer, nothing to do
          }
          //ushort glyphIndex = glyphTypeface.CharacterToGlyphMap[codePoint];
          if (!glyphTypeface.CharacterToGlyphMap.TryGetValue(codePoint, out var glyphIndex)) {
            glyphIndex = glyphTypeface.CharacterToGlyphMap[(int)'?'];
          };
          glyphIndexes[glyphIndexesIndex] = glyphIndex;
          double width = glyphTypeface.AdvanceWidths[glyphIndex] * FontSize;
          advanceWidths[glyphIndexesIndex++] = width;
          totalTextWidth += width;
        }

        if (glyphIndexes.Length!=glyphIndexesIndex) {
          glyphIndexes = glyphIndexes[0..glyphIndexes.Length];
          advanceWidths = advanceWidths[0..glyphIndexes.Length];
        }
      }

      var pixelsPerDipNew = (float)VisualTreeHelper.GetDpi(this).PixelsPerDip;
      double textOriginX = insideX;
      switch (HorizontalContentAlignment) {
      case HorizontalAlignment.Left: 
        break;
      case HorizontalAlignment.Center:
      case HorizontalAlignment.Stretch:
        textOriginX += (insideWidth - totalTextWidth)/2;
        break;
      case HorizontalAlignment.Right:
        textOriginX += insideWidth - totalTextWidth;
        break;
      default:
        throw new NotSupportedException();
      }
      double textOriginY = insideY;
      textOriginY +=VerticalContentAlignment switch {
        VerticalAlignment.Top => FontSize,
        VerticalAlignment.Center or VerticalAlignment.Stretch => (insideHeight+FontSize)/2,
        VerticalAlignment.Bottom => insideHeight,
        _ => throw new NotSupportedException(),
      };
      if (hasTextFontChanged  || origin.X!=textOriginX || origin.Y!=textOriginY || 
        pixelsPerDip!=pixelsPerDipNew) 
      {
        origin = new Point(textOriginX, textOriginY);
        pixelsPerDip = pixelsPerDipNew;
        glyphRun = new GlyphRun(glyphTypeface, 0, false, FontSize, pixelsPerDip, glyphIndexes, origin, advanceWidths, null, null, null, null, null, null);
      }

      drawingContext.PushClip(new RectangleGeometry(insideRect));
      drawingContext.DrawGlyphRun(Foreground, glyphRun);
      drawingContext.Pop();
    }
    #endregion
  }
}
