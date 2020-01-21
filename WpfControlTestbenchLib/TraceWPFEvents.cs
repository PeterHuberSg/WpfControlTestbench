using System;
using System.Text;
using System.Windows;
using System.Windows.Media;


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
    public static string TraceCreateStart(string traceObjectName) {
      return TraceCreateStart(traceObjectName, "");
    }


    /// <summary>
    /// Trace creation start of traced object
    /// </summary>
    public static string TraceCreateStart(string traceObjectName, string argumentValue) {
      TraceWPFEvents.TraceStart(traceObjectName + ".create", argumentValue);
      return traceObjectName;
    }


    /// <summary>
    /// Trace creation end of traced object
    /// </summary>
    public static string TraceCreateEnd(string traceObjectName) {
      return TraceCreateEnd(traceObjectName, "");
    }


    /// <summary>
    /// Trace creation end of traced object
    /// </summary>
    public static string TraceCreateEnd(string traceObjectName, string argumentValue) {
      TraceWPFEvents.TraceEnd(traceObjectName+".create", argumentValue);
      return traceObjectName;
    }


    /// <summary>
    /// Trace change of property
    /// </summary>
    public static void OnPropertyChanged(
      FrameworkElement frameworkElement, 
      DependencyPropertyChangedEventArgs e, 
      Action<DependencyPropertyChangedEventArgs> baseMethod) //
    {
      if (isExceptionProcessing) return;

      if (!IsTracingOn) {
        baseMethod(e);
        return;
      }

      if (
        e.Property!=FrameworkElement.IsMouseOverProperty && 
        e.Property!=UIElement.IsMouseDirectlyOverProperty)//
      {
        traceProperty(GetFrameWorkElementName(frameworkElement)!, e);
      }
      baseMethod(e);
    }


    /// <summary>
    /// Trace Measure
    /// </summary>
    public static Size MeasureOverride(
      FrameworkElement frameworkElement, 
      Size constraint, 
      Func<Size, Size> baseMethod) //
    {
      if (isExceptionProcessing) return constraint;

      if (!IsTracingOn) return baseMethod==null ? constraint : baseMethod(constraint);

      string baseMethodName = GetFrameWorkElementName(frameworkElement) + "." + "Measure";
      TraceStart(baseMethodName, constraint.ToString());
      Size returnConstraint;
      if (baseMethod==null) {
        returnConstraint = constraint;
      } else {
        returnConstraint = baseMethod(constraint);
      }
      if (double.IsInfinity(returnConstraint.Height) || double.IsInfinity(returnConstraint.Width)){
        //System.Diagnostics.Debugger.Break();
      }
      TraceEnd(baseMethodName, returnConstraint.ToString());
      return returnConstraint;
    }


    /// <summary>
    /// Trace Arrange
    /// </summary>
    public static Size ArrangeOverride(
      FrameworkElement frameworkElement, 
      Size finalSize, 
      Func<Size, Size> baseMethod) //
    {
      if (isExceptionProcessing) return finalSize;

      if (!IsTracingOn) return baseMethod==null ? finalSize : baseMethod(finalSize);

      string baseMethodName = GetFrameWorkElementName(frameworkElement) + "." + "Arrange";
      TraceStart(baseMethodName, finalSize.ToString());
      Size returnFinalSize;
      if (baseMethod==null) {
        returnFinalSize = finalSize;
      } else {
        returnFinalSize = baseMethod(finalSize);
      }
      TraceEnd(baseMethodName, returnFinalSize.ToString());
      return returnFinalSize;
    }


    /// <summary>
    /// Trace Render
    /// </summary>
    public static void OnRender(
      FrameworkElement frameworkElement, 
      DrawingContext drawingContext, 
      Action<DrawingContext> baseMethod) //
    {
      if (isExceptionProcessing) return;

      if (!IsTracingOn) {
        baseMethod(drawingContext);
        return;
      }

      string baseMethodName = GetFrameWorkElementName(frameworkElement) + "." + "Render";
      TraceStart(baseMethodName, "");
      baseMethod(drawingContext);
      TraceEnd(baseMethodName);
    }

    
    /// <summary>
    /// Returns frameworkElement Name, if one is defined. Otherwise uses the frameworkElement's type
    /// </summary>
    public static string? GetFrameWorkElementName(FrameworkElement frameworkElement) {
      if (!IsTracingOn) return null;

      if (frameworkElement is ITraceName iTraceName  && iTraceName.TraceName!=null  && iTraceName.TraceName.Length>0) {
        return iTraceName.TraceName;
      }

      if (frameworkElement.Name!=null && frameworkElement.Name.Length>0) {
        return frameworkElement.Name;
      }
      return frameworkElement.GetType().ToString();
    }


    /// <summary>
    /// Returns ParentName + '_' + frameworkElement Name
    /// </summary>
    public static string GetParentFrameWorkElementName(FrameworkElement frameworkElement) {
      if (!IsTracingOn) return "";

      string fullName = "???";
      FrameworkElement predecessor = frameworkElement;
      while (predecessor.Parent!=null) {
        predecessor = (FrameworkElement)predecessor.Parent;
        if (predecessor.Name!=null && predecessor.Name.Length>0) {
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


    static readonly StringBuilder lineStringBuilder = new StringBuilder();
    static int indentCount = 0;


    public static void ResetTrace() {
      if (!IsTracingOn) return;

      lineStringBuilder.Length = 0;
      indentCount = 0;
    }


    private static void traceIndent() {
      int charCount;
      if (indentCount<20){
        charCount = indentCount;
      }else{
        charCount = 20;
      }
      for (int charIndex = 0; charIndex<charCount; charIndex++) {
        lineStringBuilder.Append("  ");
      }
      lineStringBuilder.Append(new String(' ', charCount*2));
    }


    /// <summary>
    /// Trace Property
    /// </summary>
    private static void traceProperty(string objectName, DependencyPropertyChangedEventArgs e) {
      if (!IsTracingOn) return;

      if (isFiltered(objectName, e.Property.Name)) return;

      traceIndent();
      lineStringBuilder.Append(objectName);
      lineStringBuilder.Append(".");
      lineStringBuilder.Append(e.Property.Name);
      lineStringBuilder.Append("=");
      if (e.NewValue==null) {
        lineStringBuilder.Append("null");
      } else {
        lineStringBuilder.Append(e.NewValue.ToString());
      }
      commitTraceLine();
    }


    private static bool isFiltered(string? _/*objectName*/, string message) {
      return
        message.IndexOf("IsKeyboardFocusWithin", StringComparison.InvariantCultureIgnoreCase)>=0 ||
        message.IndexOf("IsMouseCaptureWithin", StringComparison.InvariantCultureIgnoreCase)>=0 ||
        message.IndexOf("IWindowService", StringComparison.InvariantCultureIgnoreCase)>=0 ||
        message.IndexOf("ShowKeyboardCues", StringComparison.InvariantCultureIgnoreCase)>=0 ||
        message.IndexOf("XmlNamespaceMaps", StringComparison.InvariantCultureIgnoreCase)>=0 ||
        message.IndexOf("XmlnsDictionary", StringComparison.InvariantCultureIgnoreCase)>=0;
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
    public static void TraceStart(string baseMethodName, string argumentValue) {
      if (!IsTracingOn) return;

      if (isFiltered(baseMethodName, argumentValue)) return;

      traceIndent();
      indentCount++;
      lineStringBuilder.Append(baseMethodName);
      lineStringBuilder.Append("(");
      lineStringBuilder.Append(argumentValue);
      lineStringBuilder.Append(")");
      commitTraceLine();
    }


    /// <summary>
    /// Trace end of method call
    /// </summary>
    public static void TraceEnd(string baseMethodName) {
      if (!IsTracingOn) return;

      if (isFiltered(baseMethodName, baseMethodName)) return;

      if (indentCount>0) {
        indentCount--;
      }
      traceIndent();
      lineStringBuilder.Append(baseMethodName);
      lineStringBuilder.Append(" end");
      commitTraceLine();
    }


    public static void TraceEnd(string baseMethodName, string argumentValue) {
      if (!IsTracingOn) return;

      if (isFiltered(baseMethodName, argumentValue)) return;

      //var isOutdent = false;
      if (indentCount>0) {
        //isOutdent = true;
        indentCount--;
      }
      traceIndent();
      lineStringBuilder.Append(baseMethodName);
      lineStringBuilder.Append("(");
      lineStringBuilder.Append(argumentValue);
      string endString = ") end";
      //////if (isOutdent && indentCount==0) {
      //////  endString += Environment.NewLine;
      //////}
      lineStringBuilder.Append(endString);
      commitTraceLine();
    }


    public static bool IsTraceToDebugger = true;

    
    private static void commitTraceLine() {
      TracerLib.Tracer.TraceLine(lineStringBuilder.ToString());
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
