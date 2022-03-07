///**************************************************************************************

//WpfTestbench.AxisLine
//=====================

//WPF Line used to display margin, border or padding

//Written 2014-2020 by Jürgpeter Huber 
//Contact: PeterCode at Peterbox dot com

//To the extent possible under law, the author(s) have dedicated all copyright and 
//related and neighboring rights to this software to the public domain worldwide under
//the Creative Commons 0 license (details see COPYING.txt file, see also
//<http://creativecommons.org/publicdomain/zero/1.0/>). 

//This software is distributed without any warranty. 
//**************************************************************************************/
//using System;
//using System.Windows;
//using System.Windows.Input;
//using System.Windows.Media;
//using System.Windows.Shapes;
//using System.Windows.Controls;
//using System.Windows.Media.Animation;

//namespace WpfTestbench {

//  #region Enum
//  //    ------

//  /// <summary>
//  /// Displaying a margin, border or padding ?
//  /// </summary>
//  public enum LineTypeEnum {
//    margin,
//    border,
//    padding
//  }


//  /// <summary>
//  /// Is the line horizontal or vertical ?
//  /// </summary>
//  public enum DimensionEnum {
//    width,
//    height
//  }


//  /// <summary>
//  /// Is it the first line (left or top) or the second line (right, bottom)
//  /// </summary>
//  public enum LineOrderEnum {
//    first,
//    second
//  }
//  #endregion


//  /// <summary>
//  /// WPF Line used to display margin, border or padding
//  /// </summary>
//  class AxisLine {

//    #region Properties
//    //      ----------

//    /// <summary>
//    /// Displaying a margin, border or padding ?
//    /// </summary>
//    public readonly LineTypeEnum LineType;


//    /// <summary>
//    /// Is the line horizontal or vertical ?
//    /// </summary>
//    public readonly DimensionEnum Dimension;


//    /// <summary>
//    /// Is it the first line (left or top) or the second line (right, bottom)
//    /// </summary>
//    public readonly LineOrderEnum LineOrder;


//    /// <summary>
//    /// Is it the first line (left or top) or the second line (right, bottom)
//    /// </summary>
//    public readonly string LineName;


//    /// <summary>
//    /// Gives access to the normal Line properties and methods.
//    /// </summary>
//    public readonly Line ThisLine;
//    #endregion


//    #region Constructor
//    //      -----------

//    readonly FrameworkElement testFrameworkElement;
//    readonly Control? testControl; //gives access to TestFrameworkElement as a Control. This is needed for access to Fonts and Padding
//    readonly Grid hostGrid;
//    readonly int hostGridRow;
//    readonly int hostGirdColumn;
//    readonly DependencyProperty changeThicknessProperty;
//    readonly Storyboard storyboard;


//    static readonly int[] singlelineStrokeDashes = {4, 4};
//    static readonly int[][] multilineStrokeDashes = {
//      new int[] {4, 12 },     //line1
//      new int[] {0, 4, 4, 8}, //line2
//      new int[] {0, 8, 4, 4}  //line3
//    };

    
//    /// <summary>
//    /// Constructor
//    /// </summary>
//    public AxisLine(LineTypeEnum lineType, DimensionEnum dimension, LineOrderEnum lineOrder,
//      Color lineColor, double strokeThickness, FrameworkElement testFrameworkElement, Grid hostGrid) 
//    {
//      LineType = lineType;
//      Dimension = dimension;
//      LineOrder = lineOrder;
//      this.testFrameworkElement = testFrameworkElement;
//      testControl = (testFrameworkElement as Control)!;
//      this.hostGrid = hostGrid;
//      hostGridRow = Grid.GetColumn(testFrameworkElement);
//      hostGirdColumn = Grid.GetRow(testFrameworkElement);

//      ThisLine = new Line {
//        StrokeThickness = strokeThickness
//      };
//      if (dimension==DimensionEnum.width) {
//        ThisLine.VerticalAlignment = VerticalAlignment.Stretch;
//      } else /* height*/ {
//        ThisLine.HorizontalAlignment = HorizontalAlignment.Stretch;
//      }
      
//      GradientStopCollection gradientStopCollection = new(4);
//      GradientStop gradientStop0, gradientStop1;
//      if (lineOrder==LineOrderEnum.first) {
//        gradientStop0 = new GradientStop(lineColor, 0);
//        gradientStopCollection.Add(gradientStop0);
//        gradientStop1 = new GradientStop(lineColor, 0.5);
//        gradientStopCollection.Add(gradientStop1);
//        gradientStopCollection.Add(new GradientStop(Colors.Transparent, 0.5));
//        gradientStopCollection.Add(new GradientStop(Colors.Transparent, 1));
//      } else {
//        gradientStopCollection.Add(new GradientStop(Colors.Transparent, 0));
//        gradientStopCollection.Add(new GradientStop(Colors.Transparent, 0.5));
//        gradientStop0 = new GradientStop(lineColor, 0.5);
//        gradientStopCollection.Add(gradientStop0);
//        gradientStop1 = new GradientStop(lineColor, 0);
//        gradientStopCollection.Add(gradientStop1);
//      }

//      NameScope.SetNameScope(ThisLine, new NameScope());//used by storyboard to find named gradientstops
//      storyboard = new Storyboard();
//      addAnimation("GradientStop0", gradientStop0, lineColor, storyboard);
//      addAnimation("GradientStop1", gradientStop1, lineColor, storyboard);

//      ThisLine.Loaded += ThisLine_Loaded;

//      LinearGradientBrush brush = dimension==DimensionEnum.width
//          ? new LinearGradientBrush(gradientStopCollection, 0)
//          : new LinearGradientBrush(gradientStopCollection, 90);
//      brush.Opacity = 0.4;
//      ThisLine.Stroke = brush;

//      int[] strokeDashes;
//      if (testControl==null) {
//        if (lineType!=LineTypeEnum.margin) {
//          throw new Exception("lineType '' only support for FrameworkElements inheriting from Control.");
//        }
//        strokeDashes = singlelineStrokeDashes;
//      } else {
//        strokeDashes = multilineStrokeDashes[(int)lineType];
//      }
//      ThisLine.StrokeDashArray = new DoubleCollection();
//      foreach (double strokeDash in strokeDashes) {
//        ThisLine.StrokeDashArray.Add(strokeDash);
//      }

//      if (dimension==DimensionEnum.width) {
//        ThisLine.Y2 = SystemParameters.VirtualScreenHeight;
//        ThisLine.X1 = strokeThickness/2;
//        ThisLine.X2 = strokeThickness/2;
//        ThisLine.Cursor = Cursors.ScrollWE;
//        LineName = lineOrder==LineOrderEnum.first ? "Left" : "Right";
//      } else {
//        ThisLine.X2 = SystemParameters.VirtualScreenWidth;
//        ThisLine.Y1 = strokeThickness/2;
//        ThisLine.Y2 = strokeThickness/2;
//        ThisLine.Cursor = Cursors.ScrollNS;
//        LineName = lineOrder==LineOrderEnum.first ? "Top" : "Bottom";
//      }

//      switch (LineType) {
//      case LineTypeEnum.margin: changeThicknessProperty = FrameworkElement.MarginProperty; LineName += "Margin"; break;
//      case LineTypeEnum.border: changeThicknessProperty = Control.BorderThicknessProperty; LineName += "Border"; break;
//      case LineTypeEnum.padding: changeThicknessProperty = Control.PaddingProperty; LineName += "Padding"; break;
//      default: throw new Exception("LineType '" + LineType + "' not supported");
//      }

//      hostGrid.Children.Add(ThisLine);
//      Grid.SetRow(ThisLine, hostGridRow);
//      Grid.SetColumn(ThisLine, hostGirdColumn);
//      ThisLine.MouseDown += mouseDown;
//      ThisLine.MouseMove += mouseMove;
//      ThisLine.MouseUp   += mouseUp;
//    }


//    private void addAnimation(string gradientName, GradientStop gradientStop, Color lineColor, Storyboard storyboard) {
//      // Register a name for each animated gradient stop with the line so that they can be controlled by
//      // a storyboard.
//      ThisLine.RegisterName(gradientName, gradientStop);

//      var gradientStopColorAnimation = new ColorAnimation {
//        From = Color.FromRgb((byte)(lineColor.R/2), (byte)(lineColor.G/2), (byte)(lineColor.B/2)),
//        To = Color.FromRgb((byte)((lineColor.R + 0xFF)/2), (byte)((lineColor.G + 0xFF)/2), (byte)((lineColor.B + 0xFF)/2)),
//        Duration = TimeSpan.FromSeconds(4),
//        AutoReverse = true,
//        RepeatBehavior = RepeatBehavior.Forever
//      };
//      Storyboard.SetTargetName(gradientStopColorAnimation, gradientName);
//      Storyboard.SetTargetProperty(gradientStopColorAnimation,
//          new PropertyPath(GradientStop.ColorProperty));
//      storyboard.Children.Add(gradientStopColorAnimation);
//    }


//    private void ThisLine_Loaded(object sender, RoutedEventArgs e) {
//      storyboard.Begin(ThisLine);
//    }
//    #endregion


//    #region Events
//    //      ------

//    Point startMousePosition;
//    Thickness startThickness;


//    void mouseDown(object sender, MouseButtonEventArgs e) {
//      e.MouseDevice.Capture((FrameworkElement)sender, CaptureMode.Element);
//      startMousePosition = e.MouseDevice.GetPosition(hostGrid);
//      startThickness = (Thickness)testFrameworkElement.GetValue(changeThicknessProperty);
//    }


//    void mouseMove(object sender, MouseEventArgs e) {
//      if (e.MouseDevice.Captured!=sender) return;

//      double multiplier;
//      if (Dimension==DimensionEnum.height) {
//        var verticalAlignment = testFrameworkElement.VerticalAlignment;
//        multiplier =verticalAlignment==VerticalAlignment.Center ||
//          (verticalAlignment==VerticalAlignment.Stretch && !double.IsNaN(testFrameworkElement.Height)) ? 2 : 1;
//      } else {
//        var horizontalAlignment = testFrameworkElement.HorizontalAlignment;
//        multiplier =horizontalAlignment==HorizontalAlignment.Center ||
//          (horizontalAlignment==HorizontalAlignment.Stretch && !double.IsNaN(testFrameworkElement.Width)) ? 2 : 1;
//      }
//      Point newMousePosition = e.MouseDevice.GetPosition(hostGrid);
//      double change = Dimension==DimensionEnum.height
//          ? (newMousePosition.Y - startMousePosition.Y)*multiplier
//          : (newMousePosition.X - startMousePosition.X)*multiplier;
//      if (change==0) return;

//      //|| (LineType==LineTypeEnum.border && change<0)
//      Thickness newThickness;
//      if (Dimension==DimensionEnum.width) {
//        if (LineOrder==LineOrderEnum.first) {
//          //Left
//          newThickness = new Thickness(limitBorder(startThickness.Left + change), startThickness.Top, startThickness.Right, startThickness.Bottom);
//        } else {
//          //Right
//          newThickness = new Thickness(startThickness.Left, startThickness.Top, limitBorder(startThickness.Right - change), startThickness.Bottom);
//        }
//      } else {
//        //height
//        if (LineOrder==LineOrderEnum.first) {
//          //Top
//          newThickness = new Thickness(startThickness.Left, limitBorder(startThickness.Top + change), startThickness.Right, startThickness.Bottom);
//        } else {
//          //Bottom
//          newThickness = new Thickness(startThickness.Left, startThickness.Top, startThickness.Right, limitBorder(startThickness.Bottom - change));
//        }
//      }
//      testFrameworkElement.SetValue(changeThicknessProperty, newThickness);
//    }


//    /// <summary>
//    /// Limits newValue to positive numbers for borders
//    /// </summary>
//    private double limitBorder(double newValue) {
//      return LineType==LineTypeEnum.border && newValue<0 ? 0 : newValue;
//    }

    
//    void mouseUp(object sender, MouseButtonEventArgs e) {
//      e.MouseDevice.Capture((IInputElement)sender, CaptureMode.None);
//    }
//    #endregion


//    #region Methods
//    //      -------

//    public void UpdateLinePosition(Point offsetPoint) {
//      double newMargin;
//      if (Dimension==DimensionEnum.width) {
//        if (LineOrder==LineOrderEnum.first) {
//          //left
//          if (testFrameworkElement.HorizontalAlignment==HorizontalAlignment.Right && LineType==LineTypeEnum.margin) {
//            newMargin = testFrameworkElement.Margin.Left;
//          } else {
//            newMargin = offsetPoint.X;
//          }
//          if (LineType>=LineTypeEnum.border) {
//            newMargin += testControl!.BorderThickness.Left;
//          }
//          if (LineType>=LineTypeEnum.padding) {
//            newMargin += testControl!.Padding.Left;
//          }
//          positionLine(HorizontalAlignment.Left, newMargin);
//        } else {
//          //right
//          HorizontalAlignment newHorizontalAlignment;
//          if (testFrameworkElement.HorizontalAlignment==HorizontalAlignment.Left && LineType==LineTypeEnum.margin) {
//            newHorizontalAlignment = HorizontalAlignment.Right;
//            newMargin = testFrameworkElement.Margin.Right;
//          } else {
//            newHorizontalAlignment = HorizontalAlignment.Left;
//            newMargin = offsetPoint.X + testFrameworkElement.ActualWidth - ThisLine.StrokeThickness;
//          }
//          if (LineType>=LineTypeEnum.border) {
//            newMargin -= testControl!.BorderThickness.Right;
//          }
//          if (LineType>=LineTypeEnum.padding) {
//            newMargin -= testControl!.Padding.Right;
//          }
//          positionLine(newHorizontalAlignment, newMargin);
//        }

//      } else {
//        //height
//        if (LineOrder==LineOrderEnum.first) {
//          //top
//          if (testFrameworkElement.VerticalAlignment==VerticalAlignment.Bottom && LineType==LineTypeEnum.margin) {
//            newMargin = testFrameworkElement.Margin.Top;
//          } else {
//            newMargin = offsetPoint.Y;
//          }
//          if (LineType>=LineTypeEnum.border) {
//            newMargin += testControl!.BorderThickness.Top;
//          }
//          if (LineType>=LineTypeEnum.padding) {
//            newMargin += testControl!.Padding.Top;
//          }
//          positionLine(VerticalAlignment.Top, newMargin);
//        } else {
//          //bottom
//          VerticalAlignment newVerticalAlignment;
//          if (testFrameworkElement.VerticalAlignment==VerticalAlignment.Top && LineType==LineTypeEnum.margin) {
//            newVerticalAlignment = VerticalAlignment.Bottom;
//            newMargin = testFrameworkElement.Margin.Bottom;
//          } else {
//            newVerticalAlignment = VerticalAlignment.Top;
//            newMargin = offsetPoint.Y + testFrameworkElement.ActualHeight - ThisLine.StrokeThickness;
//          }
//          if (LineType>=LineTypeEnum.border) {
//            newMargin -= testControl!.BorderThickness.Bottom;
//          }
//          if (LineType>=LineTypeEnum.padding) {
//            newMargin -= testControl!.Padding.Bottom;
//          }
//          positionLine(newVerticalAlignment, newMargin);
//        }
//      }
//    }


//    private void positionLine(VerticalAlignment newVerticalAlignment, double newMargin) {
//      if (Dimension!=DimensionEnum.height) throw new Exception("verticalAlignment is for height, but this line is for " + Dimension + ".");

//      ThisLine.VerticalAlignment = newVerticalAlignment;

//      if (LineOrder==LineOrderEnum.first) {
//        //top
//        if (ThisLine.Margin.Top!=newMargin) {
//          ThisLine.Margin = new Thickness(0, newMargin, 0, 0);
//        }
//      } else {
//        //bottom
//        if (newVerticalAlignment==VerticalAlignment.Top) {
//          if (ThisLine.Margin.Top!=newMargin || ThisLine.Margin.Bottom!=0) {
//            ThisLine.Margin = new Thickness(0, newMargin, 0, 0);
//          }
//        } else {
//          if (ThisLine.Margin.Bottom!=newMargin || ThisLine.Margin.Top!=0) {
//            ThisLine.Margin = new Thickness(0, 0, 0, newMargin);
//          }
//        }
//      }
//    }


//    private void positionLine(HorizontalAlignment newHorizontalAlignment, double newMargin) {
//      if (Dimension!=DimensionEnum.width) throw new Exception("HorizontalAlignment is for width, but this line is for " + Dimension + ".");

//      ThisLine.HorizontalAlignment = newHorizontalAlignment;

//      if (LineOrder==LineOrderEnum.first) {
//        //left
//        if (ThisLine.Margin.Left!=newMargin) {
//          ThisLine.Margin = new Thickness(newMargin, 0, 0, 0);
//        }
//      } else {
//        //right
//        if (newHorizontalAlignment==HorizontalAlignment.Left) {
//          if (ThisLine.Margin.Left!=newMargin || ThisLine.Margin.Right!=0) {
//            ThisLine.Margin = new Thickness(newMargin, 0, 0, 0);
//          }
//        } else {
//          if (ThisLine.Margin.Right!=newMargin || ThisLine.Margin.Left!=0) {
//            ThisLine.Margin = new Thickness(0, 0, newMargin, 0);
//          }
//        }
//      }
//    }
//    #endregion
//  }
//}
