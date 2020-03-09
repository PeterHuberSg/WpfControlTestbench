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
using TracerLib;


namespace WpfTestbench {

  public class WpfTraceViewer: DockPanel {

    public const int MaxLinesStored = 400;


    /// <summary>
    /// A filter function returning true prevents the message to be traced
    /// </summary>
    public Func<TraceMessage, bool> FilterFunc;


    #region Constructor
    //      -----------

    static List<TraceMessage> staticTtraceMessages;
    static Action<IEnumerable<TraceMessage>> traceAction;


    static void setTraceAction(Action<IEnumerable<TraceMessage>> traceAction) {
      WpfTraceViewer.traceAction = traceAction;
      if (staticTtraceMessages.Count>0){
        //display previously received messages
        traceAction(staticTtraceMessages);
        staticTtraceMessages.Clear();
      }
    }


    static WpfTraceViewer() {
      staticTtraceMessages = new List<TraceMessage>();
      foreach (TraceMessage traceMessage in Tracer.GetTrace()) {
        staticTtraceMessages.Add(traceMessage);
      }
      Tracer.MessagesTraced += Tracer_MessagesTraced;
    }


    DockPanel buttonDockPanel;
    TextBox traceTextBox;
    RichTextBox traceRichTextBox;
    TableRowGroup traceTableRowGroup;
   

    string traceString;
    DateTime traceDateTime;
    static string[] lineSeparator = new string[] { Environment.NewLine };


    private void initTraceTextBox() {
      lines.Clear();
      traceTextBox.Text = "";
      traceTableRowGroup.Rows.Clear();
      traceString = "";
      traceDateTime = DateTime.MaxValue; //This prevents that right at the beginning an empty line gets added
    }


    CheckBox isRichTextTracingCheckBox;
    CheckBox isFilterCheckBox;


    public WpfTraceViewer() {
      buttonDockPanel = new DockPanel();
      buttonDockPanel.Margin = new Thickness(5, 3, 0, 3);
      DockPanel.SetDock(buttonDockPanel, Dock.Bottom);
      Children.Add(buttonDockPanel);

      isRichTextTracingCheckBox = new CheckBox();
      isRichTextTracingCheckBox.Content = "_Rich Text";
      isRichTextTracingCheckBox.Margin = new Thickness(0, 0, 5, 0);
      isRichTextTracingCheckBox.VerticalAlignment = VerticalAlignment.Center;
      isRichTextTracingCheckBox.Checked += isRichTextTracingCheckBox_CheckedChanged;
      isRichTextTracingCheckBox.Unchecked += isRichTextTracingCheckBox_CheckedChanged;
      DockPanel.SetDock(isRichTextTracingCheckBox, Dock.Left);
      buttonDockPanel.Children.Add(isRichTextTracingCheckBox);

      isFilterCheckBox = new CheckBox();
      isFilterCheckBox.Content = "_Filter";
      isFilterCheckBox.Margin = new Thickness(0, 0, 5, 0);
      isFilterCheckBox.VerticalAlignment = VerticalAlignment.Center;
      //isFilterCheckBox.Checked += isFilterCheckBox_CheckedChanged;
      //isFilterCheckBox.Unchecked += isFilterCheckBox_CheckedChanged;
      DockPanel.SetDock(isFilterCheckBox, Dock.Left);
      buttonDockPanel.Children.Add(isFilterCheckBox);

      var copyAllButton = new Button();
      copyAllButton.Content = "C_opy All";
      copyAllButton.Margin = new Thickness(0, 0, 5, 0);
      copyAllButton.Click += copyAllButton_Click;
      DockPanel.SetDock(copyAllButton, Dock.Left);
      buttonDockPanel.Children.Add(copyAllButton);

      var clearAllButton = new Button();
      clearAllButton.Content = "_Clear All";
      clearAllButton.Margin = new Thickness(0, 0, 5, 0);
      clearAllButton.Click += clearAllButton_Click;
      DockPanel.SetDock(clearAllButton, Dock.Left);
      buttonDockPanel.Children.Add(clearAllButton);

      var stopContinueButton = new Button();
      stopContinueButton.Content = "Stop";
      stopContinueButton.Margin = new Thickness(0, 0, 5, 0);
      stopContinueButton.Click += stopContinueButton_Click;
      DockPanel.SetDock(stopContinueButton, Dock.Left);
      buttonDockPanel.Children.Add(stopContinueButton);

      var dockFillRectangle = new Rectangle();
      dockFillRectangle.HorizontalAlignment = HorizontalAlignment.Stretch;
      buttonDockPanel.Children.Add(dockFillRectangle);

      var tracerGrid = new Grid();
      Children.Add(tracerGrid);


      traceTextBox = new TextBox();
      traceTextBox.BorderBrush= Brushes.DarkGray;
      traceTextBox.IsReadOnly = true;
      traceTextBox.UndoLimit = 0;
      traceTextBox.TextWrapping = TextWrapping.Wrap;
      traceTextBox.Margin = new Thickness(3);
      traceTextBox.HorizontalAlignment = HorizontalAlignment.Stretch;
      traceTextBox.VerticalAlignment = VerticalAlignment.Stretch;
      traceTextBox.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
      traceTextBox.HorizontalScrollBarVisibility = ScrollBarVisibility.Visible;

      traceRichTextBox = new RichTextBox();
      traceRichTextBox.BorderBrush= Brushes.DarkGray;
      traceRichTextBox.IsReadOnly = true;
      traceRichTextBox.UndoLimit = 0;
      traceRichTextBox.Margin = new Thickness(3);
      traceRichTextBox.HorizontalAlignment = HorizontalAlignment.Stretch;
      traceRichTextBox.VerticalAlignment = VerticalAlignment.Stretch;
      traceRichTextBox.VerticalScrollBarVisibility = ScrollBarVisibility.Visible;
      traceRichTextBox.HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled;
      FlowDocument traceFlowDocument = new FlowDocument();
      traceRichTextBox.Document = traceFlowDocument;
      Table table = new Table();
      traceFlowDocument.Blocks.Add(table);
      Typeface typeface = 
        new Typeface(traceRichTextBox.FontFamily, traceRichTextBox.FontStyle, traceRichTextBox.FontWeight, traceRichTextBox.FontStretch);
      var dateFormattedText = 
        new FormattedText("00:00.000", CultureInfo.CurrentUICulture, FlowDirection.LeftToRight, typeface, traceRichTextBox.FontSize, Brushes.Black);
      var typeFormattedText = 
        new FormattedText("nnn", CultureInfo.CurrentUICulture, FlowDirection.LeftToRight, typeface, traceRichTextBox.FontSize, Brushes.Black);
      table.Columns.Add(new TableColumn {Width=new GridLength(dateFormattedText.Width, GridUnitType.Pixel)});
      table.Columns.Add(new TableColumn {Width=new GridLength(typeFormattedText.Width, GridUnitType.Pixel)});
      table.Columns.Add(new TableColumn{Width=GridLength.Auto});
//      table.Columns.Add(new TableColumn {Width=new GridLength(1, GridUnitType.Star)});
      traceTableRowGroup = new TableRowGroup();
      table.RowGroups.Add(traceTableRowGroup);

      initTraceTextBox();
      tracerGrid.Children.Add(traceTextBox);
      tracerGrid.Children.Add(traceRichTextBox);
      configureTextTracing();

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


    void parentWindow_Activated(object sender, EventArgs e) {
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
          WpfTraceViewer.staticTtraceMessages.Add(traceMessage);
        }
      }else{
        traceAction(traceMessages);
      }
    }


    void isRichTextTracingCheckBox_CheckedChanged(object sender, RoutedEventArgs e) {
      configureTextTracing();
    }


    //void isFilterCheckBox_CheckedChanged(object sender, RoutedEventArgs e) {
    //}


    void copyAllButton_Click(object sender, RoutedEventArgs e) {
      TextPointer startTextPointer = traceRichTextBox.Selection.Start;
      TextPointer endTextPointer = traceRichTextBox.Selection.End;
      traceRichTextBox.SelectAll();
      traceRichTextBox.Copy();
      traceRichTextBox.Selection.Select(startTextPointer, endTextPointer);
    }


    void clearAllButton_Click(object sender, RoutedEventArgs e) {
      initTraceTextBox();
    }


    bool isRunning = true;


    void stopContinueButton_Click(object sender, RoutedEventArgs e) {
      Button stopContinueButton = (Button)sender;
      if (isRunning) {
        stopContinueButton.Content = "Continue";
      } else {
        stopContinueButton.Content = "Stop";
      }
      isRunning = !isRunning;
    }
    #endregion


    #region Methods
    //      -------

    private void configureTextTracing() {
      if (isRichTextTracingCheckBox.IsChecked.Value) {
        traceRichTextBox.Visibility = Visibility.Visible;
        traceTextBox.Visibility = Visibility.Collapsed;
      } else {
        traceRichTextBox.Visibility = Visibility.Collapsed;
        traceTextBox.Visibility = Visibility.Visible;
      }
    }


    Queue<String> lines = new Queue<string>(MaxLinesStored);
    Queue<String> paragraphs = new Queue<string>(MaxLinesStored);
    StringBuilder traceStringBuilder = new StringBuilder();
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

//var stopwatch = new System.Diagnostics.Stopwatch();
//stopwatch.Start();
      if (isRichTextTracingCheckBox.IsChecked.Value) {
        //RichTextBox
        foreach (TraceMessage traceMessage in traceMessageEnumerable) {
          if (isFiltered(traceMessage)) continue;

          TableRow tableRow = new TableRow();

          //traceTable.RowGroups
          if (traceMessage==null) {
            //force some distances before next message
            previousMessageDate = DateTime.MinValue;
          } else {
            bool isDistanceNeeded = traceMessage.Created - previousMessageDate > TimeSpan.FromMilliseconds(3*Tracer.TimerIntervallMilliseconds);
            previousMessageDate = traceMessage.Created;
            addCell(tableRow, traceMessage.Created.ToString("mm:ss.fff"), Brushes.DarkSlateGray, isDistanceNeeded);
            addCell(tableRow, traceMessage.TraceType.ShortString(), Brushes.DarkBlue, isDistanceNeeded);
            addCell(tableRow, traceMessage.Message, Brushes.Black, isDistanceNeeded);
          }

          if (lines.Count>=MaxLinesStored) {
            traceTableRowGroup.Rows.RemoveAt(0);
          }
          traceTableRowGroup.Rows.Add(tableRow);
        }
        traceRichTextBox.ScrollToEnd();

      } else {
        //TextBox
        foreach (TraceMessage traceMessage in traceMessageEnumerable) {
          if (isFiltered(traceMessage)) continue;

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
//stopwatch.Stop();
//Console.WriteLine(1000*stopwatch.Elapsed.TotalMilliseconds + "\t" + traceMessages.Length + "\t" + 1000*stopwatch.Elapsed.TotalMilliseconds/traceMessages.Length);
    }


    private bool isFiltered(TraceMessage traceMessage) {
      if (isFilterCheckBox.IsChecked.Value) {
        if (FilterFunc==null) {
          //standard filter
          if (traceMessage.FilterText==null) return true;
        } else {
          //custom filter
          if (FilterFunc(traceMessage)) return true;
        }
      }
      return false;
    }


    private void addCell(TableRow tableRow, string cellText, SolidColorBrush foregroundBrush, bool isDistanceNeeded) {
      TableCell tableCell = new TableCell(new Paragraph(new Run { Text = cellText, Foreground=foregroundBrush }));
      if (isDistanceNeeded) {
        tableCell.Padding = new Thickness(0, 10, 0, 0);
      }
      tableRow.Cells.Add(tableCell);
    }


    /// <summary>
    /// Display the newest line on the bottom, remove oldest line on top
    /// </summary>
    private void scrollDown() {
      int removeCharCount = traceString.IndexOf(Environment.NewLine)+2;
      ////////skip EoL which should be always there
      //////if (traceString[removeCharCount+1]==Environment.NewLine[0] && traceString[removeCharCount+2]==Environment.NewLine[1]) {
      //////  removeCharCount += 2;
      //////}
      traceString = traceString.Remove(0, removeCharCount);
    }


    /// <summary>
    /// Display the newest line on top, remove oldest line on bottom
    /// </summary>
    private void scrollUp() {
      int searchCharCount = traceString.Length;
      //skip EoL which should be always there
      if (traceString[traceString.Length-2]==Environment.NewLine[0] && traceString[traceString.Length-1]==Environment.NewLine[1]) {
        searchCharCount -= 2;
      }
      ////////skip second EoL if there was an empty line
      //////if (traceString[traceString.Length-4]==Environment.NewLine[0] && traceString[traceString.Length-3]==Environment.NewLine[1]) {
      //////  searchCharCount -= 2;
      //////}
      int removeStart = traceString.LastIndexOf(Environment.NewLine, searchCharCount)+2;
      traceString = traceString.Remove(removeStart);
    }
    #endregion

  }
}
