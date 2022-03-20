/********************************************************************************************************

WpfTestbench.AxisLine
=====================

WPF Line used to display margin, border or padding

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
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Controls;
using System.Windows.Media.Animation;


namespace WpfTestbench {

  #region Enums
  //    -------

  /// <summary>
  /// Displaying a margin, border or padding ?
  /// </summary>
  public enum LineTypeEnum {
    margin,
    border,
    padding
  }


  /// <summary>
  /// Is the line horizontal or vertical ?
  /// </summary>
  public enum DimensionEnum {
    width,
    height
  }


  /// <summary>
  /// Is it the first line (left or top) or the second line (right, bottom)
  /// </summary>
  public enum LineOrderEnum {
    first,
    second
  }
  #endregion


  #region AxisLineContext
  //      ---------------

  /// <summary>
  /// Provides some configuration data for AxisLines, like Brushes, StrokeDashes, etc.
  /// </summary>
  public class AxisLineContext {

    #region Properties
    //      ----------

    /// <summary>
    /// Returns an animated brush for the LineType
    /// </summary>
    public Brush this[LineTypeEnum lineType] {
      get => Brushes[(int)lineType];
    }


    /// <summary>
    /// Animated Brushes
    /// </summary>
    public readonly Brush[] Brushes;


    public readonly int[] SinglelineStrokeDashesVisible = {8, 8};
    public readonly int[][] MultilineStrokeDashesVisible = {
      new int[] {24, 72},     //line1
      new int[] {0, 24, 24, 48}, //line2
      new int[] {0, 48, 24, 24}  //line3
    };


    public readonly int[] SinglelineStrokeDashesTransparent = {8, 8};
    public readonly int[][] MultilineStrokeDashesTransparent = {
      new int[] {8, 24},     //line1
      new int[] {0, 8, 8, 16}, //line2
      new int[] {0, 16, 8, 8}  //line3
    };
    #endregion


    #region Constructor
    //      -----------

    public readonly double StrokeThickness;
    public readonly double StrokeThicknessHalf;
    public readonly FrameworkElement TestFrameworkElement;
    public readonly Control? TestControl; //treats TestFrameworkElement as a Control which gives access to Fonts and Padding
    public readonly Grid HostGrid;
    public readonly int HostGridRow;
    public readonly int HostGirdColumn;

    public AxisLineContext(double strokeThickness, FrameworkElement testFrameworkElement, Grid hostGrid) {
      StrokeThickness = strokeThickness;
      StrokeThicknessHalf = strokeThickness/2;
      TestFrameworkElement = testFrameworkElement;
      TestControl = (testFrameworkElement as Control)!;
      HostGrid = hostGrid;
      var containerOfTestFrameworkElement = (UIElement)testFrameworkElement.Parent;
      HostGridRow = Grid.GetColumn(containerOfTestFrameworkElement);
      HostGirdColumn = Grid.GetRow(containerOfTestFrameworkElement);

      Brushes = new Brush[3];
      for (int brushesIndex = 0; brushesIndex < Brushes.Length; brushesIndex++) {
        Color color1, color2;
        switch (brushesIndex) {
        case 0: color1 = Colors.Cyan; color2 = Colors.Blue; break;
        case 1: color1 = Colors.Yellow; color2 = Colors.Green; break;
        case 2: color1 = Colors.Magenta; color2 = Colors.Red; break;
        default:
          throw new NotSupportedException();
        }

        var brush = new SolidColorBrush {Color=color1, Opacity=0.6};
        var colorAnimation = new ColorAnimation {
          From = color1,
          To = color2,
          Duration =  new Duration(TimeSpan.FromSeconds(1)),
          AutoReverse = true,
          RepeatBehavior = RepeatBehavior.Forever
        };

        // Apply the animation to the brush's Color property.
        brush.BeginAnimation(SolidColorBrush.ColorProperty, colorAnimation);
        Brushes[brushesIndex] = brush;
      }
    }
    #endregion
  }
  #endregion


  #region AxisLine
  //      --------

  /// <summary>
  /// WPF Line used to display margin, border or padding. It consists of a thinner visible line and a wider 
  /// transparent line which detects if the mouse is over it
  /// </summary>
  class AxisLine {//inheriting from Line is not possible :-(

    #region Properties
    //      ----------

    /// <summary>
    /// Displaying a margin, border or padding ?
    /// </summary>
    public readonly LineTypeEnum LineType;


    /// <summary>
    /// Is the line horizontal or vertical ?
    /// </summary>
    public readonly DimensionEnum Dimension;


    /// <summary>
    /// Is it the first line (left or top) or the second line (right, bottom)
    /// </summary>
    public readonly LineOrderEnum LineOrder;


    /// <summary>
    /// Is it the first line (left or top) or the second line (right, bottom)
    /// </summary>
    public readonly string LineName;


    /// <summary>
    /// The line the user can see
    /// </summary>
    public readonly Line VisibleLine;


    /// <summary>
    /// The line the user grab with the mouse
    /// </summary>
    public readonly Line TransparentLine;
    #endregion


    #region Constructor
    //      -----------

    readonly AxisLineContext axisLineContext;
    readonly DependencyProperty changeThicknessProperty;


    /// <summary>
    /// Constructor
    /// </summary>
    public AxisLine(LineTypeEnum lineType, DimensionEnum dimension, LineOrderEnum lineOrder,
      AxisLineContext axisLineContext) 
    {
      LineType = lineType;
      Dimension = dimension;
      LineOrder = lineOrder;
      this.axisLineContext = axisLineContext;

      VisibleLine = new Line {
        StrokeThickness = axisLineContext.StrokeThickness
      };
      TransparentLine = new Line {
        Stroke = Brushes.Transparent, 
        StrokeThickness = 3*axisLineContext.StrokeThickness
      };
      if (dimension==DimensionEnum.width) {
        TransparentLine.VerticalAlignment = VisibleLine.VerticalAlignment = VerticalAlignment.Stretch;
      } else /* height*/ {
        TransparentLine.HorizontalAlignment = VisibleLine.HorizontalAlignment = HorizontalAlignment.Stretch;
      }
      
      VisibleLine.Stroke = axisLineContext[lineType];

      int[] strokeDashesVisible;
      int[] strokeDashesTransparent;
      if (axisLineContext.TestControl==null) {
        if (lineType!=LineTypeEnum.margin) {
          throw new Exception("lineType '' only support for FrameworkElements inheriting from Control.");
        }
        strokeDashesVisible = axisLineContext.SinglelineStrokeDashesVisible;
        strokeDashesTransparent = axisLineContext.SinglelineStrokeDashesTransparent;
      } else {
        strokeDashesVisible = axisLineContext.MultilineStrokeDashesVisible[(int)lineType];
        strokeDashesTransparent = axisLineContext.MultilineStrokeDashesTransparent[(int)lineType];
      }
      VisibleLine.StrokeDashArray = new DoubleCollection();
      TransparentLine.StrokeDashArray = new DoubleCollection();
      foreach (double strokeDashVisible in strokeDashesVisible) {
        VisibleLine.StrokeDashArray.Add(strokeDashVisible);
      }
      foreach (double strokeDashTransparent in strokeDashesTransparent) {
        TransparentLine.StrokeDashArray.Add(strokeDashTransparent);
      }

      if (dimension==DimensionEnum.width) {
        TransparentLine.Y2 = VisibleLine.Y2 = SystemParameters.VirtualScreenHeight;
        VisibleLine.X1 = VisibleLine.X2 = axisLineContext.StrokeThicknessHalf;
        TransparentLine.X1 = TransparentLine.X2 = 3*axisLineContext.StrokeThicknessHalf;
        TransparentLine.Cursor = Cursors.ScrollWE;
        LineName = lineOrder==LineOrderEnum.first ? "Left" : "Right";
      } else {
        TransparentLine.X2 = VisibleLine.X2 = SystemParameters.VirtualScreenWidth;
        VisibleLine.Y1 = VisibleLine.Y2 = axisLineContext.StrokeThicknessHalf;
        TransparentLine.Y1 = TransparentLine.Y2 = 3*axisLineContext.StrokeThicknessHalf;
        TransparentLine.Cursor = Cursors.ScrollNS;
        LineName = lineOrder==LineOrderEnum.first ? "Top" : "Bottom";
      }

      switch (LineType) {
      case LineTypeEnum.margin: changeThicknessProperty = FrameworkElement.MarginProperty; LineName += "Margin"; break;
      case LineTypeEnum.border: changeThicknessProperty = Control.BorderThicknessProperty; LineName += "Border"; break;
      case LineTypeEnum.padding: changeThicknessProperty = Control.PaddingProperty; LineName += "Padding"; break;
      default: throw new Exception("LineType '" + LineType + "' not supported");
      }

      axisLineContext.HostGrid.Children.Add(VisibleLine);
      Grid.SetRow(VisibleLine, axisLineContext.HostGridRow);
      Grid.SetColumn(VisibleLine, axisLineContext.HostGirdColumn);
      axisLineContext.HostGrid.Children.Add(TransparentLine);
      Grid.SetRow(TransparentLine, axisLineContext.HostGridRow);
      Grid.SetColumn(TransparentLine, axisLineContext.HostGirdColumn);
      TransparentLine.MouseDown += mouseDown;
      TransparentLine.MouseMove += mouseMove;
      TransparentLine.MouseUp   += mouseUp;
    }
    #endregion


    #region Events
    //      ------

    Point startMousePosition;
    Thickness startThickness;


    void mouseDown(object sender, MouseButtonEventArgs e) {
      e.MouseDevice.Capture((FrameworkElement)sender, CaptureMode.Element);
      startMousePosition = e.MouseDevice.GetPosition(axisLineContext.HostGrid);
      startThickness = (Thickness)axisLineContext.TestFrameworkElement.GetValue(changeThicknessProperty);
    }


    void mouseMove(object sender, MouseEventArgs e) {
      if (e.MouseDevice.Captured!=sender) return;

      double multiplier;
      if (Dimension==DimensionEnum.height) {
        var verticalAlignment = axisLineContext.TestFrameworkElement.VerticalAlignment;
        multiplier = verticalAlignment==VerticalAlignment.Center ||
          (verticalAlignment==VerticalAlignment.Stretch && 
          !double.IsNaN(axisLineContext.TestFrameworkElement.Height)) ? 2 : 1;
      } else {
        var horizontalAlignment = axisLineContext.TestFrameworkElement.HorizontalAlignment;
        multiplier = horizontalAlignment==HorizontalAlignment.Center ||
          (horizontalAlignment==HorizontalAlignment.Stretch && 
          !double.IsNaN(axisLineContext.TestFrameworkElement.Width)) ? 2 : 1;
      }
      Point newMousePosition = e.MouseDevice.GetPosition(axisLineContext.HostGrid);
      double change = Dimension==DimensionEnum.height
          ? (newMousePosition.Y - startMousePosition.Y)*multiplier
          : (newMousePosition.X - startMousePosition.X)*multiplier;
      if (change==0) return;

      Thickness newThickness;
      if (Dimension==DimensionEnum.width) {
        if (LineOrder==LineOrderEnum.first) {
          //Left
          newThickness = new Thickness(limitBorder(startThickness.Left + change), startThickness.Top, startThickness.Right, startThickness.Bottom);
        } else {
          //Right
          newThickness = new Thickness(startThickness.Left, startThickness.Top, limitBorder(startThickness.Right - change), startThickness.Bottom);
        }
      } else {
        //height
        if (LineOrder==LineOrderEnum.first) {
          //Top
          newThickness = new Thickness(startThickness.Left, limitBorder(startThickness.Top + change), startThickness.Right, startThickness.Bottom);
        } else {
          //Bottom
          newThickness = new Thickness(startThickness.Left, startThickness.Top, startThickness.Right, limitBorder(startThickness.Bottom - change));
        }
      }
      axisLineContext.TestFrameworkElement.SetValue(changeThicknessProperty, newThickness);
    }


    /// <summary>
    /// Limits newValue to positive numbers for borders
    /// </summary>
    private double limitBorder(double newValue) {
      return LineType==LineTypeEnum.border && newValue<0 ? 0 : newValue;
    }

    
    void mouseUp(object sender, MouseButtonEventArgs e) {
      e.MouseDevice.Capture((IInputElement)sender, CaptureMode.None);
    }
    #endregion


    #region Methods
    //      -------

    /// <summary>
    /// Positions AxisLine based on offset of TestControl
    /// </summary>
    public void UpdateLinePosition(Point offsetPoint) {
      double visibleMargin;
      double transparentMargin;
      if (Dimension==DimensionEnum.width) {
        if (LineOrder==LineOrderEnum.first) {
          //left
          if (axisLineContext.TestFrameworkElement.HorizontalAlignment==HorizontalAlignment.Right && 
            LineType==LineTypeEnum.margin) 
          {
            transparentMargin = visibleMargin = axisLineContext.TestFrameworkElement.Margin.Left;
          } else {
            transparentMargin = visibleMargin = offsetPoint.X;
          }
          if (LineType>=LineTypeEnum.border) {
            transparentMargin = visibleMargin += axisLineContext.TestControl!.BorderThickness.Left;
          }
          if (LineType>=LineTypeEnum.padding) {
            transparentMargin = visibleMargin += axisLineContext.TestControl!.Padding.Left;
          }
          positionLine(HorizontalAlignment.Left, visibleMargin, transparentMargin);
        } else {
          //right
          HorizontalAlignment newHorizontalAlignment;
          if (axisLineContext.TestFrameworkElement.HorizontalAlignment==HorizontalAlignment.Left && 
            LineType==LineTypeEnum.margin) 
          {
            newHorizontalAlignment = HorizontalAlignment.Right;
            transparentMargin = visibleMargin = axisLineContext.TestFrameworkElement.Margin.Right;
          } else {
            newHorizontalAlignment = HorizontalAlignment.Left;
            visibleMargin = offsetPoint.X + axisLineContext.TestFrameworkElement.ActualWidth - VisibleLine.StrokeThickness;
            transparentMargin = offsetPoint.X + axisLineContext.TestFrameworkElement.ActualWidth - TransparentLine.StrokeThickness;
          }
          if (LineType>=LineTypeEnum.border) {
            visibleMargin -= axisLineContext.TestControl!.BorderThickness.Right;
            transparentMargin -= axisLineContext.TestControl!.BorderThickness.Right;
          }
          if (LineType>=LineTypeEnum.padding) {
            visibleMargin -= axisLineContext.TestControl!.Padding.Right;
            transparentMargin -= axisLineContext.TestControl!.Padding.Right;
          }
          positionLine(newHorizontalAlignment, visibleMargin, transparentMargin);
        }

      } else {
        //height
        if (LineOrder==LineOrderEnum.first) {
          //top
          if (axisLineContext.TestFrameworkElement.VerticalAlignment==VerticalAlignment.Bottom && LineType==LineTypeEnum.margin) {
            transparentMargin = visibleMargin = axisLineContext.TestFrameworkElement.Margin.Top;
          } else {
            transparentMargin = visibleMargin = offsetPoint.Y;
          }
          if (LineType>=LineTypeEnum.border) {
            transparentMargin = visibleMargin += axisLineContext.TestControl!.BorderThickness.Top;
          }
          if (LineType>=LineTypeEnum.padding) {
            transparentMargin = visibleMargin += axisLineContext.TestControl!.Padding.Top;
          }
          positionLine(VerticalAlignment.Top, visibleMargin, transparentMargin);
        } else {
          //bottom
          VerticalAlignment newVerticalAlignment;
          if (axisLineContext.TestFrameworkElement.VerticalAlignment==VerticalAlignment.Top && 
            LineType==LineTypeEnum.margin) 
          {
            newVerticalAlignment = VerticalAlignment.Bottom;
            transparentMargin = visibleMargin = axisLineContext.TestFrameworkElement.Margin.Bottom;
          } else {
            newVerticalAlignment = VerticalAlignment.Top;
            visibleMargin = offsetPoint.Y + axisLineContext.TestFrameworkElement.ActualHeight - VisibleLine.StrokeThickness;
            transparentMargin = offsetPoint.Y + axisLineContext.TestFrameworkElement.ActualHeight - TransparentLine.StrokeThickness;
          }
          if (LineType>=LineTypeEnum.border) {
            visibleMargin -= axisLineContext.TestControl!.BorderThickness.Bottom;
            transparentMargin -= axisLineContext.TestControl!.BorderThickness.Bottom;
          }
          if (LineType>=LineTypeEnum.padding) {
            visibleMargin -= axisLineContext.TestControl!.Padding.Bottom;
            transparentMargin -= axisLineContext.TestControl!.Padding.Bottom;
          }
          positionLine(newVerticalAlignment, visibleMargin, transparentMargin);
        }
      }
    }


    private void positionLine(VerticalAlignment newVerticalAlignment, double visibleMargin, double transparentMargin) {
      if (Dimension!=DimensionEnum.height) throw new Exception("verticalAlignment is for height, but this line is for " + Dimension + ".");

      TransparentLine.VerticalAlignment = VisibleLine.VerticalAlignment = newVerticalAlignment;

      if (LineOrder==LineOrderEnum.first) {
        //top
        if (VisibleLine.Margin.Top!=visibleMargin) {
          VisibleLine.Margin = new Thickness(0, visibleMargin, 0, 0);
          TransparentLine.Margin = new Thickness(0, transparentMargin, 0, 0);
        }
      } else {
        //bottom
        if (newVerticalAlignment==VerticalAlignment.Top) {
          if (VisibleLine.Margin.Top!=visibleMargin || VisibleLine.Margin.Bottom!=0) {
            VisibleLine.Margin = new Thickness(0, visibleMargin, 0, 0);
            TransparentLine.Margin = new Thickness(0, transparentMargin, 0, 0);
          }
        } else {
          if (VisibleLine.Margin.Bottom!=visibleMargin || VisibleLine.Margin.Top!=0) {
            VisibleLine.Margin = new Thickness(0, 0, 0, visibleMargin);
            TransparentLine.Margin = new Thickness(0, 0, 0, transparentMargin);
          }
        }
      }
    }


    private void positionLine(HorizontalAlignment newHorizontalAlignment, double visibleMargin, double transparentMargin) {
      if (Dimension!=DimensionEnum.width) throw new Exception("HorizontalAlignment is for width, but this line is for " + Dimension + ".");

      TransparentLine.HorizontalAlignment = VisibleLine.HorizontalAlignment = newHorizontalAlignment;

      if (LineOrder==LineOrderEnum.first) {
        //left
        if (VisibleLine.Margin.Left!=visibleMargin) {
          VisibleLine.Margin = new Thickness(visibleMargin, 0, 0, 0);
          TransparentLine.Margin = new Thickness(transparentMargin, 0, 0, 0);
        }
      } else {
        //right
        if (newHorizontalAlignment==HorizontalAlignment.Left) {
          if (VisibleLine.Margin.Left!=visibleMargin || VisibleLine.Margin.Right!=0) {
            VisibleLine.Margin = new Thickness(visibleMargin, 0, 0, 0);
            TransparentLine.Margin = new Thickness(transparentMargin, 0, 0, 0);
          }
        } else {
          if (VisibleLine.Margin.Right!=visibleMargin || VisibleLine.Margin.Left!=0) {
            VisibleLine.Margin = new Thickness(0, 0, visibleMargin, 0);
            TransparentLine.Margin = new Thickness(0, 0, transparentMargin, 0);
          }
        }
      }
    }
    #endregion
  }
  #endregion
}
