/**************************************************************************************

WpfTestbench.AxisLine
=====================

WPF Line used to display margin, border or padding

Written 2014-2020 by Jürgpeter Huber 
Contact: PeterCode at Peterbox dot com

To the extent possible under law, the author(s) have dedicated all copyright and 
related and neighboring rights to this software to the public domain worldwide under
the Creative Commons 0 license (details see COPYING.txt file, see also
<http://creativecommons.org/publicdomain/zero/1.0/>). 

This software is distributed without any warranty. 
**************************************************************************************/
using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Controls;


namespace WpfTestbench {

  #region Enum
  //    ------

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


  /// <summary>
  /// WPF Line used to display margin, border or padding
  /// </summary>
  class AxisLine {

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
    /// Gives access to the normal Line properties and methods.
    /// </summary>
    public readonly Line ThisLine;
    #endregion


    #region Constructor
    //      -----------

    readonly FrameworkElement testFrameworkElement;
    readonly Control testControl; //gives access to TestFrameworkElement as a Control. This is needed for access to Fonts and Padding
    readonly Grid hostGrid;
    readonly int hostGridRow;
    readonly int hostGirdColumn;
    readonly DependencyProperty changeThicknessProperty;


    static readonly int[] singlelineStrokeDashes = {4, 4};
    static readonly int[][] multilineStrokeDashes = {
      new int[] {4, 12 },     //line1
      new int[] {0, 4, 4, 8}, //line2
      new int[] {0, 8, 4, 4}  //line3
    };

    
    /// <summary>
    /// default Constructor
    /// </summary>
    public AxisLine(LineTypeEnum lineType, DimensionEnum dimension, LineOrderEnum lineOrder,
      Color lineColor, double strokeThickness, FrameworkElement testFrameworkElement) 
    {
      LineType = lineType;
      Dimension = dimension;
      LineOrder = lineOrder;
      this.testFrameworkElement = testFrameworkElement;
      testControl = (testFrameworkElement as Control)!;
      DependencyObject vparent = VisualTreeHelper.GetParent(testFrameworkElement);
      DependencyObject vgparent = VisualTreeHelper.GetParent(vparent);
      hostGrid = (vgparent as Grid)!;
      if (hostGrid==null) {
        throw new NotSupportedException("TestFrameworkElement must be placed directly in a Grid.");
      }
      hostGridRow = Grid.GetColumn(testFrameworkElement);
      hostGirdColumn = Grid.GetRow(testFrameworkElement);

      ThisLine = new Line {
        StrokeThickness = strokeThickness
      };
      if (dimension==DimensionEnum.width) {
        ThisLine.VerticalAlignment = VerticalAlignment.Stretch;
      } else /* height*/ {
        ThisLine.HorizontalAlignment = HorizontalAlignment.Stretch;
      }
      GradientStopCollection gradientStopCollection = new GradientStopCollection(4);
      if (lineOrder==LineOrderEnum.first) {
        gradientStopCollection.Add(new GradientStop(lineColor, 0));
        gradientStopCollection.Add(new GradientStop(lineColor, 0.5));
        gradientStopCollection.Add(new GradientStop(Colors.Transparent, 0.5));
        gradientStopCollection.Add(new GradientStop(Colors.Transparent, 1));
      } else {
        gradientStopCollection.Add(new GradientStop(Colors.Transparent, 0));
        gradientStopCollection.Add(new GradientStop(Colors.Transparent, 0.5));
        gradientStopCollection.Add(new GradientStop(lineColor, 0.5));
        gradientStopCollection.Add(new GradientStop(lineColor, 1));
      }
      LinearGradientBrush brush;
      if (dimension==DimensionEnum.width) {
        brush = new LinearGradientBrush(gradientStopCollection, 0);
      } else {
        brush = new LinearGradientBrush(gradientStopCollection, 90);
      }
      brush.Opacity = 0.4;
      brush.Freeze();
      ThisLine.Stroke = brush;

      ThisLine.StrokeThickness = strokeThickness;
      int[] strokeDashes;
      if (testControl==null) {
        if (lineType!=LineTypeEnum.margin) {
          throw new Exception("lineType '' only support for FrameworkElements inheriting from Control.");
        }
        strokeDashes = singlelineStrokeDashes;
      } else {
        strokeDashes = multilineStrokeDashes[(int)lineType];
      }
      ThisLine.StrokeDashArray = new DoubleCollection();
      foreach (double strokeDash in strokeDashes) {
        ThisLine.StrokeDashArray.Add(strokeDash);
      }

      if (dimension==DimensionEnum.width) {
        ThisLine.Y2 = SystemParameters.VirtualScreenHeight;
        ThisLine.X1 = strokeThickness/2;
        ThisLine.X2 = strokeThickness/2;
        ThisLine.Cursor = Cursors.ScrollWE;
        if (lineOrder==LineOrderEnum.first) {
          LineName = "Left";
        } else {
          LineName = "Right";
        }
      } else {
        ThisLine.X2 = SystemParameters.VirtualScreenWidth;
        ThisLine.Y1 = strokeThickness/2;
        ThisLine.Y2 = strokeThickness/2;
        ThisLine.Cursor = Cursors.ScrollNS;
        if (lineOrder==LineOrderEnum.first) {
          LineName = "Top";
        } else {
          LineName = "Bottom";
        }
      }

      switch (LineType) {
      case LineTypeEnum.margin: changeThicknessProperty = FrameworkElement.MarginProperty; LineName += "Margin"; break;
      case LineTypeEnum.border: changeThicknessProperty = Control.BorderThicknessProperty; LineName += "Border"; break;
      case LineTypeEnum.padding: changeThicknessProperty = Control.PaddingProperty; LineName += "Padding"; break;
      default: throw new Exception("LineType '" + LineType + "' not supported");
      }

      hostGrid.Children.Add(ThisLine);
      Grid.SetRow(ThisLine, hostGridRow);
      Grid.SetColumn(ThisLine, hostGirdColumn);
      ThisLine.MouseDown += mouseDown;
      ThisLine.MouseMove += mouseMove;
      ThisLine.MouseUp   += mouseUp;
    }
    #endregion


    #region Events
    //      ------

    Point startMousePosition;
    Thickness startThickness;


    void mouseDown(object sender, MouseButtonEventArgs e) {
      e.MouseDevice.Capture((FrameworkElement)sender, CaptureMode.Element);
      startMousePosition = e.MouseDevice.GetPosition(hostGrid);
      if (LineType==LineTypeEnum.margin){
        startThickness = (Thickness)testFrameworkElement.GetValue(changeThicknessProperty);
      }else{
        startThickness = (Thickness)testControl.GetValue(changeThicknessProperty);
      }
    }


    void mouseMove(object sender, MouseEventArgs e) {
      if (e.MouseDevice.Captured!=sender) return;

      double multiplier;
      if (Dimension==DimensionEnum.height) {
        var verticalAlignment = testFrameworkElement.VerticalAlignment;
        if (verticalAlignment==VerticalAlignment.Center || 
          (verticalAlignment==VerticalAlignment.Stretch && !double.IsNaN(testFrameworkElement.Height))) {
          multiplier = 2;
        } else {
          multiplier = 1;
        }
      } else {
        var horizontalAlignment = testFrameworkElement.HorizontalAlignment;
        if (horizontalAlignment==HorizontalAlignment.Center || 
          (horizontalAlignment==HorizontalAlignment.Stretch && !double.IsNaN(testFrameworkElement.Width))) {
          multiplier = 2;
        } else {
          multiplier = 1;
        }
      }
      Point newMousePosition = e.MouseDevice.GetPosition(hostGrid);
      double change;
      if (Dimension==DimensionEnum.height) {
        change = (newMousePosition.Y - startMousePosition.Y)*multiplier;
      } else {
        change = (newMousePosition.X - startMousePosition.X)*multiplier;
      }
      if (change==0) return;

      //|| (LineType==LineTypeEnum.border && change<0)
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
      if (LineType==LineTypeEnum.margin){
        testFrameworkElement.SetValue(changeThicknessProperty, newThickness);
      }else{
        testControl.SetValue(changeThicknessProperty, newThickness);
      }
    }


    /// <summary>
    /// Limits newValue to positive numbers for borders
    /// </summary>
    private double limitBorder(double newValue) {
      if (LineType==LineTypeEnum.border && newValue<0) {
        return 0;
      } else {
        return newValue;
      }
    }

    
    void mouseUp(object sender, MouseButtonEventArgs e) {
      e.MouseDevice.Capture((IInputElement)sender, CaptureMode.None);
    }
    #endregion


    #region Methods
    //      -------

    public void UpdateLinePosition(Point offsetPoint) {
      double newMargin;
      if (Dimension==DimensionEnum.width) {
        if (LineOrder==LineOrderEnum.first) {
          //left
          if (testFrameworkElement.HorizontalAlignment==HorizontalAlignment.Right && LineType==LineTypeEnum.margin) {
            newMargin = testFrameworkElement.Margin.Left;
          } else {
            newMargin = offsetPoint.X;
          }
          if (LineType>=LineTypeEnum.border) {
            newMargin += testControl.BorderThickness==null ? 0 :testControl.BorderThickness.Left;
          }
          if (LineType>=LineTypeEnum.padding) {
            newMargin += testControl.Padding==null ? 0 :testControl.Padding.Left;
          }
          positionLine(HorizontalAlignment.Left, newMargin);
        } else {
          //right
          HorizontalAlignment newHorizontalAlignment;
          if (testFrameworkElement.HorizontalAlignment==HorizontalAlignment.Left && LineType==LineTypeEnum.margin) {
            newHorizontalAlignment = HorizontalAlignment.Right;
            newMargin = testFrameworkElement.Margin.Right;
          } else {
            newHorizontalAlignment = HorizontalAlignment.Left;
            newMargin = offsetPoint.X + testFrameworkElement.ActualWidth - ThisLine.StrokeThickness;
          }
          if (LineType>=LineTypeEnum.border) {
            newMargin -= testControl.BorderThickness==null ? 0 :testControl.BorderThickness.Right;
          }
          if (LineType>=LineTypeEnum.padding) {
            newMargin -= testControl.Padding==null ? 0 :testControl.Padding.Right;
          }
          positionLine(newHorizontalAlignment, newMargin);
        }

      } else {
        //height
        if (LineOrder==LineOrderEnum.first) {
          //top
          if (testFrameworkElement.VerticalAlignment==VerticalAlignment.Bottom && LineType==LineTypeEnum.margin) {
            newMargin = testFrameworkElement.Margin.Top;
          } else {
            newMargin = offsetPoint.Y;
          }
          if (LineType>=LineTypeEnum.border) {
            newMargin += testControl.BorderThickness==null ? 0 :testControl.BorderThickness.Top;
          }
          if (LineType>=LineTypeEnum.padding) {
            newMargin += testControl.Padding==null ? 0 :testControl.Padding.Top;
          }
          positionLine(VerticalAlignment.Top, newMargin);
        } else {
          //bottom
          VerticalAlignment newVerticalAlignment;
          if (testFrameworkElement.VerticalAlignment==VerticalAlignment.Top && LineType==LineTypeEnum.margin) {
            newVerticalAlignment = VerticalAlignment.Bottom;
            newMargin = testFrameworkElement.Margin.Bottom;
          } else {
            newVerticalAlignment = VerticalAlignment.Top;
            newMargin = offsetPoint.Y + testFrameworkElement.ActualHeight - ThisLine.StrokeThickness;
          }
          if (LineType>=LineTypeEnum.border) {
            newMargin -= testControl.BorderThickness==null ? 0 :testControl.BorderThickness.Bottom;
          }
          if (LineType>=LineTypeEnum.padding) {
            newMargin -= testControl.Padding==null ? 0 :testControl.Padding.Bottom;
          }
          positionLine(newVerticalAlignment, newMargin);
        }
      }
    }


    private void positionLine(VerticalAlignment newVerticalAlignment, double newMargin) {
      if (Dimension!=DimensionEnum.height) throw new Exception("verticalAlignment is for height, but this line is for " + Dimension + ".");

      if (ThisLine.VerticalAlignment!=newVerticalAlignment) {
        ThisLine.VerticalAlignment = newVerticalAlignment;
      }

      if (LineOrder==LineOrderEnum.first) {
        //top
        if (ThisLine.Margin.Top!=newMargin) {
          ThisLine.Margin = new Thickness(0, newMargin, 0, 0);
        }
      } else {
        //bottom
        if (newVerticalAlignment==VerticalAlignment.Top) {
          if (ThisLine.Margin.Top!=newMargin || ThisLine.Margin.Bottom!=0) {
            ThisLine.Margin = new Thickness(0, newMargin, 0, 0);
          }
        } else {
          if (ThisLine.Margin.Bottom!=newMargin || ThisLine.Margin.Top!=0) {
            ThisLine.Margin = new Thickness(0, 0, 0, newMargin);
          }
        }
      }
    }


    private void positionLine(HorizontalAlignment newHorizontalAlignment, double newMargin) {
      if (Dimension!=DimensionEnum.width) throw new Exception("HorizontalAlignment is for width, but this line is for " + Dimension + ".");

      if (ThisLine.HorizontalAlignment!=newHorizontalAlignment) {
        ThisLine.HorizontalAlignment = newHorizontalAlignment;
      }

      if (LineOrder==LineOrderEnum.first) {
        //left
        if (ThisLine.Margin.Left!=newMargin) {
          ThisLine.Margin = new Thickness(newMargin, 0, 0, 0);
        }
      } else {
        //right
        if (newHorizontalAlignment==HorizontalAlignment.Left) {
          if (ThisLine.Margin.Left!=newMargin || ThisLine.Margin.Right!=0) {
            ThisLine.Margin = new Thickness(newMargin, 0, 0, 0);
          }
        } else {
          if (ThisLine.Margin.Right!=newMargin || ThisLine.Margin.Left!=0) {
            ThisLine.Margin = new Thickness(0, 0, newMargin, 0);
          }
        }
      }
    }
    #endregion
  }
}
