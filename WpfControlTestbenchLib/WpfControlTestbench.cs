using System.Windows;
using System.Windows.Controls;


namespace WpfTestbench {

  /// <summary>
  /// Hosts a FrameworkElement and displays some FrameworkElement properties and Control Properties, which the user can change. The
  /// WpfControlTestbench displays also a trace box (EventViewer), where the all property changes and the calls and parameters of the
  /// layout methods (Measure and Arranged) get displayed.
  /// </summary>
  [TemplatePart(Name="PART_WpfTraceViewer", Type=typeof(WpfTraceViewer))]
  public class WpfControlTestbench: ContentControl {

  //the content of WpfControlTestbench is defined in Generic.XAML. It creates a control with 4 areas:
  //+--------------------------------+
  //|Additional Properties           |
  //+--------------------------------+
  //|Standard Properties             |
  //+--------------------------------+
  //|TestFrameworkElement|EventViewer|
  //|--------------------+-----------|
  //+--------------------------------+
  //
  //Standard Properties and EventViewer get automatically filled in
  //TestFrameworkElement: Add to this property in XAML the FrameworkElement to be tested
  //Additional Properties: The XAML in the body of WpfControlTestbench will be displayed there

  //WpfControlTestbench gets used in a XAML file like this:
  //
  //xmlns:WpfControlTestbenchLib="clr-namespace:WpfTestbench;assembly=WpfControlTestbenchLib"
  //xmlns:YourOwnLib="clr-namespace:YourOwnNamespave;assembly=YourOwnLib"
  //
  //<WpfControlTestbenchLib:WpfControlTestbench Name="TestBench">
  //  <WpfControlTestbenchLib:WpfControlTestbench.TestFrameworkElement>
  //    ===Definition of FrameworkElement to be tested==================================
  //    <YourOwnLib:SomeFrameworkElement Name="TestSomeFrameworkElement" Background="AliceBlue" Margin="10" />
  //    =================================================================================
  //  </WpfControlTestbenchLib:WpfControlTestbench.TestFrameworkElement>
  //  =====Content of WpfControlTestbenchLib with additional properties like textboxes===
  //  <Grid Margin="5,3,0,3">
  //    <Grid.RowDefinitions>
  //      <RowDefinition Height="auto"/>
  //      <RowDefinition/>
  //    </Grid.RowDefinitions>
  //    <Grid.ColumnDefinitions>
  //      <ColumnDefinition Width="auto"/>
  //      <ColumnDefinition/>
  //    </Grid.ColumnDefinitions>
  //    <Label Grid.Row="0" Grid.Column="0" Content="_SomeProperty"/>
  //    <TextBox Grid.Row="0" Grid.Column="1" Text="{Binding ElementName=TestSomeFrameworkElement, Path=SomeProperty}"/>
  //  </Grid>
  //  ===================================================================================
  //</WpfControlTestbenchLib:WpfControlTestbench>

  
    #region Properties
    //      ----------

    /// <summary>
    /// A control inheriting from FrameworkElement to be inspected in WpfControlTestbench
    /// </summary>
    public FrameworkElement TestFrameworkElement {
      get { return (FrameworkElement)GetValue(TestFrameworkElementProperty); }
      set { SetValue(TestFrameworkElementProperty, value); }
    }
    /// <summary>
    /// Dependency Property definition for TestFrameworkElement
    /// </summary>
    public static readonly DependencyProperty TestFrameworkElementProperty = 
    DependencyProperty.Register("TestFrameworkElement", typeof(FrameworkElement), typeof(WpfControlTestbench), new UIPropertyMetadata(null));


    /// <summary>
    /// (Rich)Textbox displaying trace in WpfControlTestbench. Among others, accessing it through this property allows to add a 
    /// special trace filter.
    /// </summary>
    public WpfTraceViewer Part_WpfTraceViewer { get; private set; }
    #endregion


    #region Constructor
    //      -----------

    static WpfControlTestbench() {
      DefaultStyleKeyProperty.OverrideMetadata(typeof(WpfControlTestbench), new FrameworkPropertyMetadata(typeof(WpfControlTestbench)));
    }


    public WpfControlTestbench() {
    }


    public override void OnApplyTemplate() {
      base.OnApplyTemplate();

      Part_WpfTraceViewer = GetTemplateChild("PART_WpfTraceViewer") as WpfTraceViewer;
    }
    #endregion
  }
}
