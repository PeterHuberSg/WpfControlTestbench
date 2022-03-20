/********************************************************************************************************

WpfTestbench.WpfTraceViewer
===========================

Control displaying the tracing information in the TestBench

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
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;


namespace WpfTestbench {


  /// <summary>
  /// Control displaying the tracing information in the TestBench
  /// </summary>
  public class WpfTraceViewer: DockPanel {

    #region Properties
    //      ----------

    /// <summary>
    /// A filter function returning true prevents the message to be traced
    /// </summary>
    public Func<TraceMessage, bool>? FilterFunc;
    #endregion


    #region Constructor
    //      -----------

    static readonly List<TraceMessage> staticTraceMessages;
    static Action<IEnumerable<TraceMessage>>? traceAction;


    static void setTraceAction(Action<IEnumerable<TraceMessage>> traceAction) {
      WpfTraceViewer.traceAction = traceAction;
      if (staticTraceMessages.Count>0){
        //display previously received messages
        traceAction(staticTraceMessages);
        staticTraceMessages.Clear();
      }
    }


    static WpfTraceViewer() {
      staticTraceMessages = new List<TraceMessage>();
      foreach (TraceMessage traceMessage in Tracer.GetTrace()) {
        staticTraceMessages.Add(traceMessage);
      }
      Tracer.MessagesTraced += Tracer_MessagesTraced;
    }


    readonly DockPanel buttonDockPanel;
    readonly TextBox traceTextBox;
   

    private void initTraceTextBox() {
      lines.Clear();
      traceTextBox.Text = "";
    }


    public WpfTraceViewer() {
      buttonDockPanel = new DockPanel {Margin = new Thickness(5, 3, 0, 3)};
      DockPanel.SetDock(buttonDockPanel, Dock.Bottom);
      Children.Add(buttonDockPanel);

      var copyAllButton = new Button {
        Content = "C_opy All",
        Margin = new Thickness(0, 0, 5, 0)
      };
      copyAllButton.Click += copyAllButton_Click;
      DockPanel.SetDock(copyAllButton, Dock.Left);
      buttonDockPanel.Children.Add(copyAllButton);

      var clearAllButton = new Button {
        Content = "_Clear All",
        Margin = new Thickness(0, 0, 5, 0)
      };
      clearAllButton.Click += clearAllButton_Click;
      DockPanel.SetDock(clearAllButton, Dock.Left);
      buttonDockPanel.Children.Add(clearAllButton);

      var stopContinueButton = new Button {
        Content = "Stop",
        Margin = new Thickness(0, 0, 5, 0)
      };
      stopContinueButton.Click += stopContinueButton_Click;
      DockPanel.SetDock(stopContinueButton, Dock.Left);
      buttonDockPanel.Children.Add(stopContinueButton);

      var dockFillRectangle = new Rectangle {
        HorizontalAlignment = HorizontalAlignment.Stretch
      };
      buttonDockPanel.Children.Add(dockFillRectangle);

      var tracerGrid = new Grid();
      Children.Add(tracerGrid);


      traceTextBox = new TextBox {
        BorderBrush= Brushes.DarkGray,
        IsReadOnly = true,
        UndoLimit = 0,
        TextWrapping = TextWrapping.Wrap,
        Margin = new Thickness(3),
        HorizontalAlignment = HorizontalAlignment.Stretch,
        VerticalAlignment = VerticalAlignment.Stretch,
        VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
        HorizontalScrollBarVisibility = ScrollBarVisibility.Visible
      };

      initTraceTextBox();
      tracerGrid.Children.Add(traceTextBox);

      setTraceAction(addTraceLine);
      Loaded += new RoutedEventHandler(WpfTraceViewer_Loaded);
    }
    #endregion


    #region Events
    //      ------

    private void WpfTraceViewer_Loaded(object sender, RoutedEventArgs e) {
      Window parentWindow = Window.GetWindow(this);
      if (parentWindow!=null) {
        parentWindow.Activated += new EventHandler(parentWindow_Activated);
      }
    }


    private void parentWindow_Activated(object? sender, EventArgs e) {
      setTraceAction(addTraceLine);
    }

    
    private static void Tracer_MessagesTraced(TraceMessage[] traceMessages) {
      //Switch to WPF thread
      Application application = Application.Current;
      if (application==null) {
        //it can happen that WPF is shutting down already, but the tracer timer still fires. Then just don't do anything.
      } else {
        Dispatcher dispatcher = Application.Current.Dispatcher;
        if (dispatcher==null) {
        } else {
          dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Background,
            new Action<TraceMessage[]>(Tracer_MessagesTracedOnWpfThread), traceMessages);
        }
      }
    }


    private static void Tracer_MessagesTracedOnWpfThread(TraceMessage[] traceMessages) {
      if (traceAction==null) {
        foreach (TraceMessage traceMessage in traceMessages) {
          WpfTraceViewer.staticTraceMessages.Add(traceMessage);
        }
      }else{
        traceAction(traceMessages);
      }
    }


    private void copyAllButton_Click(object sender, RoutedEventArgs e) {
      Clipboard.SetText(traceTextBox.Text);
    }


    private void clearAllButton_Click(object sender, RoutedEventArgs e) {
      initTraceTextBox();
    }


    bool isRunning = true;


    private void stopContinueButton_Click(object sender, RoutedEventArgs e) {
      Button stopContinueButton = (Button)sender;
      stopContinueButton.Content =isRunning ? "Continue" : "Stop";
      isRunning = !isRunning;
    }
    #endregion


    #region Methods
    //      -------

    public const int MaxLinesStored = 400;

    readonly Queue<String> lines = new(MaxLinesStored);
    readonly StringBuilder traceStringBuilder = new();
    DateTime previousMessageDate = DateTime.MaxValue;


    private void addTraceLine(IEnumerable<TraceMessage> traceMessageEnumerable) {
      if (System.ComponentModel.DesignerProperties.GetIsInDesignMode(this)) {
        //don't trace while running in Visual Studio's designer
        return;
      }
      if (!isRunning) {
        //user doesn't want to trace
        return;
      }

      foreach (TraceMessage traceMessage in traceMessageEnumerable) {
        string line;
        if (traceMessage==null) {
          line = "";
          //prevent empty line to be generated if next message come late
          previousMessageDate = DateTime.MaxValue;
        } else {
          line = traceMessage.ToString();
          if (traceMessage.Created - previousMessageDate > TimeSpan.FromMilliseconds(3*Tracer.TimerIntervallMilliseconds)) {
            line = Environment.NewLine + line;
          }
          previousMessageDate = traceMessage.Created;
        }

        if (lines.Count>=MaxLinesStored) {
          lines.Dequeue();
        }
        lines.Enqueue(line);
      }

      traceStringBuilder.Length = 0;
      foreach (string lineString in lines) {
        traceStringBuilder.AppendLine(lineString);
      }

      traceTextBox.Text = traceStringBuilder.ToString();
      traceTextBox.ScrollToEnd();
    }
    #endregion
  }
}
