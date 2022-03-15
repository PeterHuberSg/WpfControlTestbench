//==========================================================================================================================================
// Copyright: Peter Huber, Singapore, 2014
// This code is contributed to the Public Domain. You might use it freely for any purpose, commercial or non-commercial. It is provided 
// "as-is." The author gives no warranty of any kind whatsoever. It is up to you to ensure that there are no defects, the code is 
// fit for your purpose and does not infringe on other copyrights. Use this code only if you agree with these conditions. The entire risk of 
// using it lays with you :-)
//==========================================================================================================================================


/*******************************************************************************************************************************************
Use Tracer to trace messages, warnings, errors and exceptions from various threads. The messages get collected multi-threading
safe in messageQueue without any blocking (only microseconds delay). A low priority thread empties messageQueue into messageBuffer and raises the 
MessagesRaised event
 
         lock===============+  lock==============+
Trace()--->|                |  |                 |->raise event MessagesRaised
Warning()->|->messageQueue->|->|->messageBuffer->|->GetTrace()
Error()--->|        v             
Exception->|        +----->Pulse tracerThread

tracerThread is a background thread, meaning the application can stop without explicitely stopping tracerThread. To shut down nicely, 
StopThread() should be used
*******************************************************************************************************************************************/


//// In TimerMode, the trace is copied to the trace thread for further processing every millisends. Collectine messages and
//// processing them from time to time is often more efficient, because the processing might invovle thread switching.
//// If TimerMode is off, tracing a time will signal a lower priority thread to process. Since modern processors have many
//// cores, often the low priority thread runs as fast as the higher priority and most trace messages get processed on their
//// own.
//#define IsTimerMode


// Used for debugging Tracer in real time. Comment out the next line when using Tracer in your application
//#define RealTimeTraceing

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Collections.Concurrent;
using System.Diagnostics;


namespace WpfTestbench {


  /// <summary>
  /// Collects trace messages, multi-threading safe, stores them in a buffer and distributes them to various TraceReaders using a
  /// lower priority thread.
  /// </summary>
  public static class Tracer {

    #region Configuration Data
    //      ------------------

    //The following variables should be made volatile if the settings get changed dynamically. But normally the values are
    //set at the start of the application and not changed afterwards.

    /// <summary>
    /// Should tracing messaged be traced ? Default: true. Set to false to filter out tracer messages
    /// </summary>
    public static bool IsTracing = true;
    public static bool IsWarningTracing = true;
    public static bool IsErrorTracing = true;
    public static bool IsExceptionTracing = true;


    /// <summary>
    /// Trace messages get processed every TimerIntervallMilliseconds.
    /// </summary>
    public const int TimerIntervallMilliseconds = 100;


    /// <summary>
    /// The number of trace messages Tracer stores in MessageBuffer before overwriting them. The messages in MessageBuffer can be 
    /// read with GetTrace().
    /// </summary>
    public const int MaxMessageBuffer = 1000;


    /// <summary>
    /// The number of trace messages Tracer stores in messageQueue before reporting an overflow. messageQueue is an internal buffer and 
    /// gets continously emptied.
    /// </summary>
    public const int MaxMessageQueue = MaxMessageBuffer/3;


    /// <summary>
    /// Stop in the debugger if one is attached and the trace is a warning
    /// </summary>
    public static bool IsBreakOnWarning = true;

    
    /// <summary>
    /// Stop in the debugger if one is attached and the trace is an error
    /// </summary>
    public static bool IsBreakOnError = true;

    
    /// <summary>
    /// Stop in the debugger if one is attached and the trace is an exception
    /// </summary>
    public static bool IsBreakOnException = true;
    #endregion


    #region Public tracing methods
    //      ----------------------

    /// <summary>
    /// Writes a part of a message to Tracer. The message gets only traced once TraceLine() is called. Trace()
    /// is multithreading safe, meaning if 2 different threads are using Trace(), 2 different messages will get traced.
    /// </summary>
    public static void Trace(string message, params object[] args) {
      if (IsTracing) {
        tracePerThread(TraceTypeEnum.Trace, false, null, message, args);
      }
    }


    /// <summary>
    /// Writes a message to Tracer. 
    /// </summary>
    public static void TraceLine(string message, params object[] args) {
      if (IsTracing) {
        tracePerThread(TraceTypeEnum.Trace, true, null, message, args);
      }
    }


    /// <summary>
    /// Writes a message with empty string as filter information to Tracer. FilterText provides the information needed for filtering.
    /// </summary>
    public static void TraceLineFiltered(string message, params object[] args) {
      TraceLineWithFilter("", message, args);
    }


    /// <summary>
    /// Writes a message with filter information to Tracer. FilterText provides the information needed for filtering.
    /// </summary>
    public static void TraceLineWithFilter(string filterText, string message, params object[] args) {
      if (IsTracing) {
        tracePerThread(TraceTypeEnum.Trace, true, filterText, message, args);
      }
    }


    /// <summary>
    /// Writes a part of a warning to Tracer. The warning gets only traced once WarningLine() is called. Warning()
    /// is multithreading safe, meaning if 2 different threads are using Trace(), 2 different warnings will get traced.
    /// </summary>
    public static void Warning(string message, params object[] args) {
      if (IsWarningTracing) {
        tracePerThread(TraceTypeEnum.Warning, false, null, message, args);
      }
    }


    /// <summary>
    /// Writes a warning to Tracer. 
    /// </summary>
    public static void WarningLine(string message, params object[] args) {
      if (IsWarningTracing) {
        tracePerThread(TraceTypeEnum.Warning, true, null, message, args);
      }
    }


    /// <summary>
    /// Writes a part of an error to Tracer. The error gets only traced once ErrorLine() is called. Error()
    /// is multithreading safe, meaning if 2 different threads are using Error(), 2 different errors will get traced.
    /// </summary>
    public static void Error(string message, params object[] args) {
      if (IsErrorTracing) {
        tracePerThread(TraceTypeEnum.Error, false, null, message, args);
      }
    }


    /// <summary>
    /// Writes an error to Tracer. 
    /// </summary>
    public static void ErrorLine(string message, params object[] args) {
      if (IsErrorTracing) {
        tracePerThread(TraceTypeEnum.Error, true, null, message, args);
      }
    }


    /// <summary>
    /// Writes an exception to Tracer. 
    /// </summary>
    public static void Exception(Exception ex) {
      if (IsExceptionTracing) {
        tracePerThread(TraceTypeEnum.Exception, true, null, ex.ToDetailString());
      }
    }


    /// <summary>
    /// Writes an exception to Tracer. 
    /// </summary>
    public static void Exception(Exception ex, string message, params object[] args) {
      if (IsExceptionTracing) {
        tracePerThread(TraceTypeEnum.Exception, true, null, message + Environment.NewLine + ex.ToDetailString(), args);
      }
    }
    #endregion


    #region Get stored messages
    //      -------------------

    /// <summary>
    /// Returns a copy of all messages stored.
    /// </summary>
    public static TraceMessage[] GetTrace() {
      lock (messageBuffer) {
        return messageBuffer.ToArray();
      }
    }


    /// <summary>
    /// Returns a copy of all messages stored and installs an event handler for MessagesTraced. Doing this
    /// at the same time guarantees that no messages get lost between calling GetTrace() and adding an event 
    /// handler for MessagesTraced.
    /// </summary>
    public static TraceMessage[] GetTrace(Action<TraceMessage[]> MessagesTracedHandler) {
      lock (messageBuffer) {
        MessagesTraced += MessagesTracedHandler;
        return messageBuffer.ToArray();
      }
    }

    
    /// <summary>
    /// Event gets raised when a message get traced.
    /// </summary>
    public static event Action<TraceMessage[]>? MessagesTraced;


    #endregion


    #region Message handling per thread
    //      ---------------------------

    //[ThreadStatic] creates for each threat a different previousTraceType and threadMessageString
    [ThreadStatic]static TraceTypeEnum previousTraceType; //default 0: TraceTypeEnum.undef
    [ThreadStatic]static string? threadMessageString;


    private static void tracePerThread(TraceTypeEnum traceType, bool isNewLine, string? filterText, string message, params object[] args) {
      if (previousTraceType!=traceType) {
        //queue previous message having different type
        if (threadMessageString!=null) {
          enqueueMessage(previousTraceType, filterText, ref threadMessageString); //sets threadMessageString=null
        }
        previousTraceType = traceType;
      }

      //concatenate message in threadMessageString
      if (args==null || args.Length==0) {
        threadMessageString += message;
      } else {
        try {
          threadMessageString += string.Format(message, args);
        } catch (Exception) {
          //message is not properly formatted, might contain '{'. Try without args
          threadMessageString += message;
        }
      }

      if (isNewLine) {
        //queue complete, queue now
        enqueueMessage(previousTraceType, filterText, ref threadMessageString); //sets threadMessageString=null
      }
    }
    #endregion


    #region Multithreaded Queue
    //      -------------------

    static readonly Queue<TraceMessage> messagesQueue = new Queue<TraceMessage>(MaxMessageQueue);
    static bool isMessagesQueueOverflow;


    private static void enqueueMessage(TraceTypeEnum traceType, string? filterText, ref string? threadMessageBuffer) {
      #if RealTimeTraceing
        RealTimeTracer.Trace("enqueueMessage(): start " + traceType.ShortString() + ": " + threadMessageBuffer);
      #endif
      TraceMessage message = new TraceMessage(traceType, threadMessageBuffer!, filterText);
      threadMessageBuffer = null;

      //break in debugger if needed
      if (Debugger.IsAttached) {
        if ((traceType==TraceTypeEnum.Warning && IsBreakOnWarning) ||
            (traceType==TraceTypeEnum.Error && IsBreakOnError) ||
            (traceType==TraceTypeEnum.Exception && IsBreakOnException)) 
        {
          Debug.WriteLine(traceType + ": " + message);
          Debugger.Break();
        }
      }

      #if RealTimeTraceing
        RealTimeTracer.Trace("enqueueMessage(): lock messagesQueue");
      #endif
      lock(messagesQueue){
        #if RealTimeTraceing
          RealTimeTracer.Trace("enqueueMessage(): locked messagesQueue");
        #endif
        if (messagesQueue.Count>=MaxMessageQueue-1) { //leave 1 space empty for overflow error message
          #if RealTimeTraceing
            RealTimeTracer.Trace("enqueueMessage(): messagesQueue overflow (" + messagesQueue.Count + " messages)");
          #endif
          if (!isMessagesQueueOverflow) {
            isMessagesQueueOverflow = true;
            messagesQueue.Enqueue(new TraceMessage(TraceTypeEnum.Error, "Tracer.enqueueMessage(): MessagesQueue overflow (" + messagesQueue.Count + " messages) for:" + 
              Environment.NewLine + message.ToString()));
          }
        } else {
          isMessagesQueueOverflow = false;
          messagesQueue.Enqueue(message);
          #if RealTimeTraceing
            RealTimeTracer.Trace("enqueueMessage(): message added to messagesQueue");
          #endif
        }
        //Monitor.Pulse(messagesQueue);
        #if RealTimeTraceing
          RealTimeTracer.Trace("enqueueMessage(): messagesQueue pulsed, release lock");
        #endif
      }
      #if RealTimeTraceing
        RealTimeTracer.Trace("enqueueMessage(): messagesQueue lock released, end");
      #endif
    }
    #endregion


    #region Background tracer
    //      -----------------

    //storage of all messages. Other threads can get a copy with GetTrace()
    static readonly Queue<TraceMessage> messageBuffer = new Queue<TraceMessage>(MaxMessageBuffer);


    static volatile bool isDoTracing = true;
    static readonly Timer tracerTimer = createTracerTimer();


    private static Timer createTracerTimer() {
      Timer newTimer = new Timer(tracerTimerMethod);
      newTimer.Change(TimerIntervallMilliseconds, TimerIntervallMilliseconds);
      return newTimer;
    }


    static bool isTracerTimerMethodRunning = false;


    private static void tracerTimerMethod(object? state) {
      try { //thread needs to catch its exceptions
        #if RealTimeTraceing
          RealTimeTracer.Trace("TracerTimer: start");
        #endif
        if (!isDoTracing) {
          #if RealTimeTraceing
            RealTimeTracer.Trace("TracerTimer: tracing has stopped");
          #endif
          return;
        }

        if (isTracerTimerMethodRunning) {
          //if tracerTimerMethod is still running from last scheduled call, there is no point to execute it in parallel
          //on a different thread.
#if RealTimeTraceing
            RealTimeTracer.Trace("TracerTimer: new execution was stopped, because previous call is still active.");
#endif
          return;
        }

        try {
          isTracerTimerMethodRunning = true;
          TraceMessage[] newTracerMessages;
#if RealTimeTraceing
          RealTimeTracer.Trace("TracerTimer:lock messagesQueue");
#endif
          lock (messagesQueue) {

#if RealTimeTraceing
          RealTimeTracer.Trace("TracerTimer:messagesQueue locked");
#endif
            if (messagesQueue.Count==0) {
#if RealTimeTraceing
              RealTimeTracer.Trace("TracerTimer: queue empty, unlock messagesQueue");
#endif
              return;
            }

            //process new message
            newTracerMessages = messagesQueue.ToArray();
            messagesQueue.Clear();
#if RealTimeTraceing
            RealTimeTracer.Trace("TracerTimer: read " + newTracerMessages.Length + " message(s), unlock messagesQueue");
#endif
          }
#if RealTimeTraceing
          RealTimeTracer.Trace("TracerTimer: messagesQueue unlocked");
#endif

          //copy message to messageBuffer
#if RealTimeTraceing
          RealTimeTracer.Trace("TracerTimer: lock messageBuffer");
#endif
          lock (messageBuffer) {//need to lock writing so that reading can lock too to get a consistent set of messages
#if RealTimeTraceing
            RealTimeTracer.Trace("TracerTimer: messageBuffer locked, copy messages");
#endif
            foreach (TraceMessage newTracerMessage in newTracerMessages) {
              if (messageBuffer.Count==MaxMessageBuffer-1) {
                messageBuffer.Dequeue();
              }
              messageBuffer.Enqueue(newTracerMessage);
            }
#if RealTimeTraceing
            RealTimeTracer.Trace("TracerTimer: unlock messageBuffer");
#endif
          }
#if RealTimeTraceing
          RealTimeTracer.Trace("TracerTimer: messageBuffer unlocked");
#endif

          //call event handlers for MessagesTraced
          if (MessagesTraced!=null) {
            foreach (System.Action<TraceMessage[]> handler in MessagesTraced.GetInvocationList()) {
              try {
                handler(newTracerMessages);
              } catch (Exception ex) {
#if RealTimeTraceing
                RealTimeTracer.Trace("TracerTimer: Exception in EventHandler !!!: " + ex.Message);
#endif
                ShowExceptionInDebugger(ex);
                //todo: show exception in the other exception handlers
              }
            }
#if RealTimeTraceing
            RealTimeTracer.Trace("TracerTimer: all eventhandlers executed");
#endif
          }
        } finally {
          isTracerTimerMethodRunning = false;
        }
      } catch (Exception ex) {
#if RealTimeTraceing
        RealTimeTracer.Trace("TracerTimer: Exception !!!: " + ex.Message);
#endif
        ShowExceptionInDebugger(ex);
//        Console.WriteLine("Error in tracerThread." + ex.ToDetailString());
      }
#if RealTimeTraceing
        RealTimeTracer.Trace("TracerTimer: completed");
#endif
    }


    /// <summary>
    /// Stops tracing. 
    /// </summary>
    public static void StopTracing() {
      #if RealTimeTraceing
        RealTimeTracer.Trace("TracerTimer: StopTracing()");
      #endif
      lock (tracerTimer) {
        if (!isDoTracing) return;

        isDoTracing = true;
        tracerTimer.Dispose();
      }
    }
    #endregion


    #region Methods
    //      -------

    /// <summary>
    /// Use Throw to throw exceptions. This has the advantage that the debugger will break before the exception is thrown and
    /// all data is still available.
    /// </summary>
    public static void Throw(Exception ex) {
#if DEBUG
      try {
        if (IsBreakOnException && Debugger.IsAttached) {
          //if an exception has occured, then the messge is available in the ouput window of the debugger
          Debug.WriteLine("going to throw exception '" + ex.Message + "'");
          Debugger.Break();
        }
      } catch { }
#endif

      throw ex;
    }
  

    /// <summary>
    /// Make a break in Visual Studio, if it is attached
    /// </summary>
    public static void BreakInDebuggerOrDoNothing() {
#if DEBUG
      BreakInDebuggerOrDoNothing(null);
#endif
    }


    /// <summary>
    /// Make a break in Visual Studio, if it is attached
    /// </summary>
    public static void BreakInDebuggerOrDoNothing(string? message) {
#if DEBUG
      try {
        if (Debugger.IsAttached) {
          //if an exception has occured, then the messge is available in the ouput window of the debugger
          Debug.WriteLine(DateTime.Now.ToString("mm:ss.fff") + " BreakInDebuggerOrDoNothing " + message);
          Debugger.Break();
        }
      } catch { }
#endif
    }


    /// <summary>
    /// Causes a break in the debugger, if one is attached and shows exception content
    /// </summary>
    public static void ShowExceptionInDebugger(Exception ex) {
#if DEBUG
      try {
        if (Debugger.IsAttached) {
          string exceptionString = ex.ToDetailString();
          Debug.WriteLine(exceptionString);
          Debugger.Break();
        }
      } catch { }
#endif
    }

    #endregion
  }

}
