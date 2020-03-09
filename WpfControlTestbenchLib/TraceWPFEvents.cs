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
    public static DummyTraceClass TraceCreateStart(string traceObjectName, string argumentValue=null) {
      TraceWPFEvents.TraceStart(traceObjectName + ".create", argumentValue);
      return null;
    }


    ///// <summary>
    ///// Trace creation start of traced object
    ///// </summary>
    //public static string TraceCreateStartOld(string traceObjectName) {
    //  return TraceCreateStart(traceObjectName, "");
    //}


    ///// <summary>
    ///// Trace creation start of traced object
    ///// </summary>
    //public static string TraceCreateStart(string traceObjectName, string argumentValue) {
    //  TraceWPFEvents.TraceStart(traceObjectName + ".create", argumentValue);
    //  return traceObjectName;
    //}


    ///// <summary>
    ///// Trace creation end of traced object
    ///// </summary>
    //public static string TraceCreateEnd(string traceObjectName) {
    //  return TraceCreateEnd(traceObjectName, "");
    //}


    /// <summary>
    /// Trace creation end of traced object
    /// </summary>
    public static void TraceCreateEnd(string traceObjectName, string argumentValue=null) {
      TraceWPFEvents.TraceEnd(traceObjectName+".create", argumentValue);
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
        TraceProperty(GetFrameWorkElementName(frameworkElement), e);
      }
      baseMethod(e);
    }


    static List<FrameworkElement> arrangeExceptionList = new List<FrameworkElement>();


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
      TraceEnd(baseMethodName, returnFinalSize.ToString());
      return returnFinalSize;
    }


    static List<FrameworkElement> measureExceptionList = new List<FrameworkElement>();


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
      TraceEnd(baseMethodName, returnConstraint.ToString());
      return returnConstraint;
    }


    private static Size removeInfinity(Size constraint) {
      return new Size(
        double.IsInfinity(constraint.Width ) ? 0 : constraint.Width,
        double.IsInfinity(constraint.Height) ? 0 : constraint.Height);
    }


    static List<FrameworkElement> renderExceptionList = new List<FrameworkElement>(); 


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
      TraceStart(baseMethodName, frameworkElement.RenderSize.ToString());
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
    public static string GetFrameWorkElementName(FrameworkElement frameworkElement) {
      if (!IsTracingOn) return null;

      ITraceName iTraceName = frameworkElement as ITraceName;
      if (iTraceName!=null  && iTraceName.TraceName!=null  && iTraceName.TraceName.Length>0) {
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
      if (!IsTracingOn) return null;

      string fullName = "???";
      FrameworkElement predecessor = frameworkElement;
      while (predecessor.Parent!=null) {
        predecessor = predecessor.Parent as FrameworkElement;
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


    private static StringBuilder lineStringBuilder = new StringBuilder();
    private static int indentCount = 0;


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
    private static void TraceProperty(string objectName, DependencyPropertyChangedEventArgs e) {
      if (!IsTracingOn) return;

      if (isFiltered(objectName, e.Property.Name)) return;
////YLegend.MinValue=-200
//if (objectName=="YLegend" && e.Property.Name=="MinValue" && e.NewValue!=null && e.NewValue.ToString()=="-200") System.Diagnostics.Debugger.Break();
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


    private static bool isFiltered(string objectName, string message) {
      if (message==null) return false;

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


    static FrameworkElement reportedLayoutFrameworkElement = null;
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
    public static void TraceStart(string baseMethodName, string argumentValue=null) {
      if (!IsTracingOn) return;

      if (isFiltered(baseMethodName, argumentValue)) return;

      traceIndent();
      indentCount++;
      lineStringBuilder.Append(baseMethodName);
      lineStringBuilder.Append("(");
      if (argumentValue!=null) {
        lineStringBuilder.Append(argumentValue);
      }
      lineStringBuilder.Append(")");
      commitTraceLine();
    }


    /// <summary>
    /// Trace end of method call
    /// </summary>
    public static void TraceEnd(string baseMethodName, string argumentValue=null) {
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
      if (argumentValue!=null) {
        lineStringBuilder.Append("(");
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
