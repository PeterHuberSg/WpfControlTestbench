using System;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Collections.Generic;
using TracerLib;


namespace WpfTestbench {


#if DEBUG //use this class only for debugging


  /// <summary>
  /// Provides a name for tracing
  /// </summary>
  public interface ITraceName {

    /// <summary>
    /// Name to be used for tracing
    /// </summary>
    string  TraceName { get; }
  }


  /// <summary>
  /// Control can decide if it should get traced
  /// </summary>
  public interface IIsTracing: ITraceName {

    /// <summary>
    /// Controls if trace should get written
    /// </summary>
    public bool IsTracing { get; set; }
  }



  /// <summary>
  /// Class is used as dummy parameter by the XyzTraced constructors
  /// </summary>
  public class DummyTraceClass { }


  /// <summary>
  /// Tracing of WPF property changes and WPF events
  /// </summary>
  public static class TraceWPFEvents {
    /// <summary>
    /// Should tracing be done ?
    /// </summary>
    public static bool IsTracingOn = true;


    /// <summary>
    /// Trace creation start of traced object
    /// </summary>
    public static DummyTraceClass? TraceCreateStart(string traceObjectName, bool isTrace = true, string? argumentValue=null) {
      if (isTrace) {
        TraceStart(traceObjectName + ".create", argumentValue);
      }
      return null;
    }


    /// <summary>
    /// Trace creation end of traced object
    /// </summary>
    public static void TraceCreateEnd(string traceObjectName, bool isTrace = true, string? argumentValue=null) {
      if (isTrace) {
        TraceEnd(traceObjectName+".create", argumentValue);
      }
    }


    /// <summary>
    /// Trace change of property
    /// </summary>
    public static void OnPropertyChanged(
      FrameworkElement frameworkElement, 
      DependencyPropertyChangedEventArgs e, 
      Action<DependencyPropertyChangedEventArgs> baseMethod,
      bool isTracing = true)
    {
      if (isExceptionProcessing) return;

      if (!IsTracingOn || !isTracing) {
        baseMethod(e);
        return;
      }

      if (
        e.Property!=FrameworkElement.IsMouseOverProperty && 
        e.Property!=UIElement.IsMouseDirectlyOverProperty)//
      {
        TraceProperty(GetFrameWorkElementName(frameworkElement)!, e);
      }
      baseMethod(e);
    }


    static readonly List<FrameworkElement> arrangeExceptionList = new();


    /// <summary>
    /// Trace Arrange
    /// </summary>
    public static Size ArrangeOverride(
      FrameworkElement frameworkElement,
      Size finalSize,
      Func<Size, Size>? baseMethod,
      bool isTracing = true) 
    {
      if (isExceptionProcessing) return finalSize;

      if (!isTracing || !IsTracingOn) return baseMethod==null ? finalSize : baseMethod(finalSize);

      string baseMethodName = GetFrameWorkElementName(frameworkElement) + "." + "Arrange";
      TraceStart(baseMethodName, toString(finalSize));
      Size returnFinalSize = removeInfinity(finalSize);
      if (baseMethod!=null) {
        try {
          returnFinalSize = baseMethod(finalSize);
          arrangeExceptionList.Remove(frameworkElement);//does not throw an exception if list is empty
        } catch (Exception ex) {
          if (!arrangeExceptionList.Contains(frameworkElement)) {
            //report the exception only once. Because of the exception reporting, WPF will call Arrange() again.
            arrangeExceptionList.Add(frameworkElement);
            Tracer.Exception(ex);
          }
        }
      }
      TraceEnd(baseMethodName, toString(finalSize));
      return returnFinalSize;
    }


    private static string toString(Size size) {
      return size.IsEmpty ? "IsEmpty" : $"{size.Width:N0}, {size.Height:N0}";
    }


    private static string? toString(Thickness thickness) {
      return $"{thickness.Left:N0}, {thickness.Top:N0}, {thickness.Right:N0}, {thickness.Bottom:N0}";
    }


    static readonly List<FrameworkElement> measureExceptionList = new();


    /// <summary>
    /// Trace Measure
    /// </summary>
    public static Size MeasureOverride(
      FrameworkElement frameworkElement, 
      Size constraint, 
      Func<Size, Size>? baseMethod,
      bool isTracing = true)
    {
      if (isExceptionProcessing) return constraint;

      if (!isTracing || !IsTracingOn) return baseMethod==null ? constraint : baseMethod(constraint);

      string baseMethodName = GetFrameWorkElementName(frameworkElement) + "." + "Measure";
      TraceStart(baseMethodName, toString(constraint));
      Size returnConstraint = removeInfinity(constraint);
      if (baseMethod!=null) {
        try {
          returnConstraint = baseMethod(constraint);
          measureExceptionList.Remove(frameworkElement);//does not throw an exception if list is empty
        } catch (Exception ex) {
          if (!measureExceptionList.Contains(frameworkElement)) {
            //report the exception only once. Because of the exception reporting, WPF will call Measure() again.
            measureExceptionList.Add(frameworkElement);
            Tracer.Exception(ex);
          }
        }
      }
      TraceEnd(baseMethodName, toString(returnConstraint));
      return returnConstraint;
    }


    private static Size removeInfinity(Size constraint) {
      return new Size(
        double.IsInfinity(constraint.Width ) ? 0 : constraint.Width,
        double.IsInfinity(constraint.Height) ? 0 : constraint.Height);
    }


    static readonly List<FrameworkElement> renderExceptionList = new(); 


    /// <summary>
    /// Trace Render
    /// </summary>
    public static void OnRender(
      FrameworkElement frameworkElement, 
      DrawingContext drawingContext, 
      Action<DrawingContext> baseMethod,
      bool isTracing = true)
    {
      if (isExceptionProcessing) return;

      if (!isTracing || !IsTracingOn) {
        baseMethod(drawingContext);
        return;
      }

      string baseMethodName = GetFrameWorkElementName(frameworkElement) + "." + "Render";
      TraceStart(baseMethodName, toString(frameworkElement.RenderSize));
      try {
        baseMethod(drawingContext);
        renderExceptionList.Remove(frameworkElement);//does not throw an exception if list is empty
      } catch (Exception ex) {
        if (!renderExceptionList.Contains(frameworkElement)) {
          //report the exception only once. Because of the exception reporting, WPF might call Render() again.
          renderExceptionList.Add(frameworkElement);
          Tracer.Exception(ex);
        }
      }
      TraceEnd(baseMethodName);
    }

    
    /// <summary>
    /// Returns frameworkElement Name, if one is defined. Otherwise uses the frameworkElement's type
    /// </summary>
    public static string? GetFrameWorkElementName(FrameworkElement frameworkElement) {
      if (!IsTracingOn) return null;

      #pragma warning disable IDE0046 // Convert to conditional expression
      if (frameworkElement is ITraceName iTraceName && iTraceName.TraceName!=null && iTraceName.TraceName.Length>0) {
      #pragma warning restore IDE0046
        return iTraceName.TraceName;
      }

      return frameworkElement.Name!=null && frameworkElement.Name.Length>0 ? frameworkElement.Name : 
        frameworkElement.GetType().ToString();
    }


    /// <summary>
    /// Returns ParentName + '_' + frameworkElement Name
    /// </summary>
    public static string? GetParentFrameWorkElementName(FrameworkElement frameworkElement) {
      if (!IsTracingOn) return null;

      string fullName = "???";
      FrameworkElement? predecessor = frameworkElement;
      while (predecessor?.Parent!=null) {
        predecessor = predecessor.Parent as FrameworkElement;
        if (predecessor?.Name.Length>0) {
          fullName = predecessor.Name;
          break;
        }
      }

      if (frameworkElement.Name!=null && frameworkElement.Name.Length>0) {
        if (fullName=="???") {
          fullName = frameworkElement.Name;
        } else {
          fullName += "_" + frameworkElement.Name;
        }
      }
      return fullName;
    }


    private static readonly StringBuilder lineStringBuilder = new();
    private static int indentCount = 0;


    public static void ResetTrace() {
      if (!IsTracingOn) return;

      lineStringBuilder.Length = 0;
      indentCount = 0;
    }


    private static void traceIndent() {
      int charCount = indentCount<20 ? indentCount : 20;
      for (int charIndex = 0; charIndex<charCount; charIndex++) {
        lineStringBuilder.Append("  ");
      }
      lineStringBuilder.Append(new String(' ', charCount*2));
    }


    /// <summary>
    /// Trace Property
    /// </summary>
    private static void TraceProperty(string objectName, DependencyPropertyChangedEventArgs e) {
      if (!IsTracingOn) return;

      if (isFiltered(objectName, e.Property.Name)) return;
////YLegend.MinValue=-200
//if (objectName=="YLegend" && e.Property.Name=="MinValue" && e.NewValue!=null && e.NewValue.ToString()=="-200") System.Diagnostics.Debugger.Break();
      traceIndent();
      lineStringBuilder.Append(objectName);
      lineStringBuilder.Append('.');
      lineStringBuilder.Append(e.Property.Name);
      lineStringBuilder.Append('=');
      if (e.NewValue==null) {
        lineStringBuilder.Append("null");
      } else if (e.NewValue is double doubleValue) {
        if (Math.Abs(doubleValue)>100) {
          lineStringBuilder.Append(doubleValue.ToString("N0"));
        } else {
          lineStringBuilder.Append(doubleValue.ToString("N2"));
        }
      } else if (e.NewValue is Size sizeValue) {
        lineStringBuilder.Append(toString(sizeValue));
      } else if (e.NewValue is Thickness thicknessValue) {
        lineStringBuilder.Append(toString(thicknessValue));
      } else {
        lineStringBuilder.Append(e.NewValue.ToString());
      }
      commitTraceLine();
    }


    private static bool isFiltered(string? _/*objectName*/, string? message) {
      #pragma warning disable IDE0046 // Convert to conditional expression
      if (message==null) return false;
      #pragma warning restore IDE0046

      return
        message.Contains("IsKeyboardFocusWithin", StringComparison.InvariantCultureIgnoreCase) ||
        message.Contains("IsMouseCaptureWithin", StringComparison.InvariantCultureIgnoreCase) ||
        message.Contains("IWindowService", StringComparison.InvariantCultureIgnoreCase) ||
        message.Contains("ShowKeyboardCues", StringComparison.InvariantCultureIgnoreCase) ||
        message.Contains("XmlNamespaceMaps", StringComparison.InvariantCultureIgnoreCase) ||
        message.Contains("XmlnsDictionary", StringComparison.InvariantCultureIgnoreCase);
    }


    static bool isExceptionProcessing;


    /// <summary>
    /// Trace an exception
    /// </summary>
    public static void TraceException(Exception exception) {
      isExceptionProcessing = true;
      TraceLine("");
      TraceLine(">>>>> Exception: " + exception.Message + Environment.NewLine + exception.StackTrace + Environment.NewLine);
      indentCount = 0;
      isExceptionProcessing = false;
    }


    static FrameworkElement? reportedLayoutFrameworkElement = null;
    static bool isKeepReportedLayoutFrameworkElement = false;


    /// <summary>
    /// Trace one line, include Name in trace
    /// </summary>
    public static void TraceLayoutUpdated(FrameworkElement frameworkElement) {
      if (reportedLayoutFrameworkElement==frameworkElement) return;

      reportedLayoutFrameworkElement = frameworkElement;
      isKeepReportedLayoutFrameworkElement = true;
      TraceLine(GetFrameWorkElementName(frameworkElement) + ".LayoutUpdated()");
    }


    /// <summary>
    /// Trace one line, including FrameworkElement Name
    /// </summary>
    public static void TraceLine(FrameworkElement frameworkElement, string traceString) {
      if (frameworkElement==null) {
        TraceLine(traceString);
      } else {
        TraceLine(GetFrameWorkElementName(frameworkElement) + "." + traceString);
      }
    }
    
    
    /// <summary>
    /// Trace one line
    /// </summary>
    public static void TraceLine(string traceString) {
      if (!IsTracingOn) return;

      if (isFiltered(null, traceString)) return;

      traceIndent();
      lineStringBuilder.Append(traceString);
      commitTraceLine();
    }


    /// <summary>
    /// Trace one line with increasing indent
    /// </summary>
    public static void TraceLineStart(string traceString) {
      if (!IsTracingOn) return;

      if (isFiltered(null, traceString)) return;

      traceIndent();
      indentCount++;
      lineStringBuilder.Append(traceString);
      commitTraceLine();
    }


    /// <summary>
    /// Trace one line with decreasing indent
    /// </summary>
    public static void TraceLineEnd(string traceString) {
      if (!IsTracingOn) return;

      if (isFiltered(null, traceString)) return;

      if (indentCount>0) {
        indentCount--;
      }
      traceIndent();
      lineStringBuilder.Append(traceString);
      commitTraceLine();
    }


    /// <summary>
    /// Trace start of method call
    /// </summary>
    public static void TraceStart(string baseMethodName, string? argumentValue=null) {
      if (!IsTracingOn) return;

      if (isFiltered(baseMethodName, argumentValue)) return;

      traceIndent();
      indentCount++;
      lineStringBuilder.Append(baseMethodName);
      lineStringBuilder.Append('(');
      if (argumentValue!=null) {
        lineStringBuilder.Append(argumentValue);
      }
      lineStringBuilder.Append(')');
      commitTraceLine();
    }


    /// <summary>
    /// Trace end of method call
    /// </summary>
    public static void TraceEnd(string baseMethodName, string? argumentValue=null) {
      if (!IsTracingOn) return;

      if (isFiltered(baseMethodName, argumentValue)) return;

      bool isEmptyLineNeeded = false;
      if (indentCount>0) {
        indentCount--;
        if (indentCount==0) {
          isEmptyLineNeeded = true;
        }
      }
      traceIndent();
      lineStringBuilder.Append(baseMethodName);
      lineStringBuilder.Append('(');
      if (argumentValue!=null) {
        lineStringBuilder.Append(argumentValue);
      }
      lineStringBuilder.Append(") end");
      if (isEmptyLineNeeded) {
        lineStringBuilder.Append(Environment.NewLine);
      }
      commitTraceLine();
    }


    public static bool IsTraceToDebugger = true;

    
    private static void commitTraceLine() {
      Tracer.TraceLine(lineStringBuilder.ToString());
      lineStringBuilder.Length = 0;

      if (isKeepReportedLayoutFrameworkElement) {
        isKeepReportedLayoutFrameworkElement = false;
      } else {
        reportedLayoutFrameworkElement = null;
      }
    }
  }
#endif
}
