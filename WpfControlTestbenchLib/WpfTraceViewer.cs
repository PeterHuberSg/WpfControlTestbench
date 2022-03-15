using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;


namespace WpfTestbench {

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
   

    DateTime traceDateTime;
    static readonly string[] lineSeparator = new string[] { Environment.NewLine };


    private void initTraceTextBox() {
      lines.Clear();
      traceTextBox.Text = "";
      traceDateTime = DateTime.MaxValue; //This prevents that right at the beginning an empty line gets added
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

    void WpfTraceViewer_Loaded(object sender, RoutedEventArgs e) {
      Window parentWindow = Window.GetWindow(this);
      if (parentWindow!=null) {
        parentWindow.Activated += new EventHandler(parentWindow_Activated);
      }
    }


    void parentWindow_Activated(object? sender, EventArgs e) {
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


    static void Tracer_MessagesTracedOnWpfThread(TraceMessage[] traceMessages) {
      if (traceAction==null) {
        foreach (TraceMessage traceMessage in traceMessages) {
          WpfTraceViewer.staticTraceMessages.Add(traceMessage);
        }
      }else{
        traceAction(traceMessages);
      }
    }


    void copyAllButton_Click(object sender, RoutedEventArgs e) {
      Clipboard.SetText(traceTextBox.Text);
    }


    void clearAllButton_Click(object sender, RoutedEventArgs e) {
      initTraceTextBox();
    }


    bool isRunning = true;


    void stopContinueButton_Click(object sender, RoutedEventArgs e) {
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
