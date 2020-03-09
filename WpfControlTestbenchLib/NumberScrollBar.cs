/**************************************************************************************

WpfTestbench.NumberScrollBar
============================

Control consisting of ScrollBar and TextBox.

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
using System.Windows.Controls;
using System.Windows.Controls.Primitives;


namespace WpfTestbench {

  #region Value Changed EventArgs
  //      -----------------------

  /// <summary>
  /// This ValueChangedEventArgs class contains old and new value when 
  /// event is raised.
  /// </summary>
  public class ValueChangedEventArgs<T>: EventArgs {
    private readonly T oldValue;
    private readonly T newValue;

    /// <summary>
    /// This is an instance constructor for the RoutedPropertyChangedEventArgs class.
    /// It is constructed with a reference to the event being raised.
    /// </summary>
    /// <param name="oldValue">The old property value</param>
    /// <param name="newValue">The new property value</param>
    /// <returns>Nothing.</returns>
    public ValueChangedEventArgs(T oldValue, T newValue)
      : base() {
      this.oldValue = oldValue;
      this.newValue = newValue;
    }


    /// <summary>
    /// Return the old value
    /// </summary>
    public T OldValue {
      get { return oldValue; }
    }


    /// <summary>
    /// Return the new value
    /// </summary>
    public T NewValue {
      get { return newValue; }
    }
  }
  #endregion


  /// <summary>
  /// Control consisting of ScrollBar and TextBox. User can use mouse with the ScrollBar or keyboard with the TextBox to change Value. 
  /// </summary>
  public class NumberScrollBar: DockPanel {

    #region Properties
    //      ----------

    /// <summary>
    /// Value entered by User
    /// </summary>
    public double Value {
      get { return (double)GetValue(ValueProperty); }
      set { SetValue(ValueProperty, value); }
    }

    /// <summary>
    /// Dependency Property key for Value
    /// </summary>
    public static readonly DependencyProperty ValueProperty = 
    DependencyProperty.Register("Value", typeof(double), typeof(NumberScrollBar), new UIPropertyMetadata(0.0, valueChanged));


    /// <summary>
    /// Occurs when the range value changes.
    /// </summary>
    public event EventHandler<ValueChangedEventArgs<double>>? ValueChanged;



    //routedEvent version of ValueChanged
    //-----------------------------------
    ///// <summary>
    ///// RoutedEvent definition for ValueChanged
    ///// </summary>
    //public static readonly RoutedEvent ValueChangedEvent = EventManager.RegisterRoutedEvent("ValueChanged", RoutingStrategy.Bubble, typeof(RoutedPropertyChangedEventHandler<double>), typeof(RangeBase));


    ///// <summary>
    ///// Occurs when the range value changes.
    ///// </summary>
    //[Category("Behavior")]
    //public event RoutedPropertyChangedEventHandler<double> ValueChanged { add { AddHandler(ValueChangedEvent, value); } remove { RemoveHandler(ValueChangedEvent, value); } }


    /// <summary>
    /// The number of decimal places in the value returned by the scrollbar. Default is 1.
    /// </summary>
    public int DecimalPlaces {
      get { return (int)GetValue(DecimalPlacesProperty); }
      set { SetValue(DecimalPlacesProperty, value); }
    }

    /// <summary>
    /// Dependency Property key for DecimalPlaces
    /// </summary>
    public static readonly DependencyProperty DecimalPlacesProperty = 
    DependencyProperty.Register("DecimalPlaces", typeof(int), typeof(NumberScrollBar), new UIPropertyMetadata(1));


    /// <summary>
    /// Width of TextBox. Default is 40.
    /// </summary>
    public double TextBoxWidth {
      get { return (int)GetValue(TextBoxWidthProperty); }
      set { SetValue(TextBoxWidthProperty, value); }
    }

    /// <summary>
    /// Dependency property identifier for TextBoxWidth
    /// </summary>
    public static readonly DependencyProperty TextBoxWidthProperty = 
    DependencyProperty.Register("TextBoxWidth", typeof(double), typeof(NumberScrollBar), new UIPropertyMetadata(40.0));


    /// <summary>
    /// Gets or sets the smallest possible Value. Default: 0
    /// </summary>
    public double Minimum {
      get { return (double)GetValue(MinimumProperty); }
      set { SetValue(MinimumProperty, value); }
    }

    // Using a DependencyProperty as the backing store for Minimum.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty MinimumProperty = 
    DependencyProperty.Register("Minimum", typeof(double), typeof(NumberScrollBar), new UIPropertyMetadata(0.0));

    
    /// <summary>
    /// Gets or sets the highest possible Value. Default: 100
    /// </summary>
    public double Maximum {
      get { return (double)GetValue(MaximumProperty); }
      set { SetValue(MaximumProperty, value); }
    }

    /// <summary>
    /// Dependency property identifier for Maximum
    /// </summary>
    public static readonly DependencyProperty MaximumProperty = 
    DependencyProperty.Register("Maximum", typeof(double), typeof(NumberScrollBar), new UIPropertyMetadata(100.0));


    /// <summary>
    /// Gets or sets a value to be added to or subtracted from the Value when the user clicks on Scrollbar buttons
    /// </summary>
    public double SmallChange {
      get { return (double)GetValue(SmallChangeProperty); }
      set { SetValue(SmallChangeProperty, value); }
    }

    /// <summary>
    /// Dependency property identifier for SmallChange
    /// </summary>
    public static readonly DependencyProperty SmallChangeProperty = 
    DependencyProperty.Register("SmallChange", typeof(double), typeof(NumberScrollBar), new UIPropertyMetadata(1.0));



    /// <summary>
    /// Gets or sets a value to be added to or subtracted from the Value when the user clicks on Scrollbar background
    /// </summary>
    public double LargeChange {
      get { return (double)GetValue(LargeChangeProperty); }
      set { SetValue(LargeChangeProperty, value); }
    }

    /// <summary>
    /// Dependency property identifier for LargeChange
    /// </summary>
    public static readonly DependencyProperty LargeChangeProperty = 
    DependencyProperty.Register("LargeChange", typeof(double), typeof(NumberScrollBar), new UIPropertyMetadata(10.0));


    ///// <summary>
    ///// Gets or sets the amount of the scrollable content that is currently visible.
    ///// </summary>
    //public double ViewportSize {
    //  get { return (double)GetValue(ViewportSizeProperty); }
    //  set { SetValue(ViewportSizeProperty, value); }
    //}

    ///// <summary>
    ///// Dependency property identifier for ViewportSize
    ///// </summary>
    //public static readonly DependencyProperty ViewportSizeProperty = 
    //DependencyProperty.Register("ViewportSize", typeof(double), typeof(NumberScrollBar), new UIPropertyMetadata(10.0));

    
    /// <summary>
    /// User can enter Value here with the keyboard.
    /// </summary>
    public SmallTextBox ValueTextBox { get; private set; }


    /// <summary>
    /// Provides access to ScrollBar from XAML. This is useful to set MinValue and MaxValue, etc.
    /// </summary>
    public ScrollBar ValueScrollBar { get; private set; }

    
    #endregion


    #region Constructor
    //      -----------

    /// <summary>
    /// constructor
    /// </summary>
    public NumberScrollBar() {
      ValueTextBox = new SmallTextBox {
        Text = Value.ToString(),
        VerticalAlignment = VerticalAlignment.Center,
        TextAlignment = TextAlignment.Right
      };
      ValueTextBox.TextChanged += valueTextBox_TextChanged;
      ValueTextBox.LostFocus += valueTextBox_LostFocus;
      WpfBinding.Setup(this, "TextBoxWidth", ValueTextBox, TextBox.WidthProperty, System.Windows.Data.BindingMode.OneWay);
      DockPanel.SetDock(ValueTextBox, Dock.Right);
      Children.Add(ValueTextBox);

      ValueScrollBar = new ScrollBar {
        Orientation = Orientation.Horizontal
      };
      ValueScrollBar.ValueChanged += valueScrollBar_ValueChanged;
      WpfBinding.Setup(this, "Minimum", ValueScrollBar, ScrollBar.MinimumProperty, System.Windows.Data.BindingMode.OneWay);
      WpfBinding.Setup(this, "Maximum", ValueScrollBar, ScrollBar.MaximumProperty, System.Windows.Data.BindingMode.OneWay);
      WpfBinding.Setup(this, "SmallChange", ValueScrollBar, ScrollBar.SmallChangeProperty, System.Windows.Data.BindingMode.OneWay);
      WpfBinding.Setup(this, "LargeChange", ValueScrollBar, ScrollBar.LargeChangeProperty, System.Windows.Data.BindingMode.OneWay);
      WpfBinding.Setup(this, "LargeChange", ValueScrollBar, ScrollBar.ViewportSizeProperty, System.Windows.Data.BindingMode.OneWay);
      Children.Add(ValueScrollBar);
    }
    #endregion


    #region Event Handlers
    //      --------------

    bool isEventHandlerActive = false;


    void valueTextBox_TextChanged(object sender, TextChangedEventArgs e) {
      if (isEventHandlerActive) return;

      isEventHandlerActive = true;
      if (double.TryParse(ValueTextBox.Text, out var newValue)) {
        ValueScrollBar.Value = newValue;
      }
      isEventHandlerActive = false;
    }

    
    void valueTextBox_LostFocus(object sender, System.Windows.RoutedEventArgs e) {
      if (isEventHandlerActive) return;

      isEventHandlerActive = true;
      if (double.TryParse(ValueTextBox.Text, out var newValue)) {
        ValueScrollBar.Value = newValue;
      } else {
        ValueScrollBar.Value = ValueScrollBar.Minimum;
        newValue = double.NaN;
        ValueTextBox.Text = "NaN";
      }
      Value = newValue;
      isEventHandlerActive = false;
    }


    void valueScrollBar_ValueChanged(object sender, System.Windows.RoutedPropertyChangedEventArgs<double> e) {
      if (isEventHandlerActive) return;

      isEventHandlerActive = true;
      double newValue = Math.Round(ValueScrollBar.Value, DecimalPlaces);
      ValueTextBox.Text = newValue.ToString();
      Value = newValue;
      isEventHandlerActive = false;
    }


    private static void valueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e){
      var numberScrollBar = (NumberScrollBar)d;
      double newValue = (double)e.NewValue;

      if (!numberScrollBar.isEventHandlerActive) {
        numberScrollBar.isEventHandlerActive = true;
        if (double.IsNaN(newValue)) {
          numberScrollBar.ValueScrollBar.Value = numberScrollBar.ValueScrollBar.Minimum;
          newValue = double.NaN;
          numberScrollBar.ValueTextBox.Text = "NaN";
        } else {
          numberScrollBar.ValueScrollBar.Value = newValue;
          numberScrollBar.ValueTextBox.Text = newValue.ToString();
        }
        numberScrollBar.isEventHandlerActive = false;
      }

      //RoutedPropertyChangedEventArgs<double> routedEventArgs = new RoutedPropertyChangedEventArgs<double>((double)e.OldValue, newValue);
      //routedEventArgs.RoutedEvent=RangeBase.ValueChangedEvent;
      //routedEventArgs.ClearUserInitiated();
      //UIElement.RaiseEventImpl(numberScrollBar, routedEventArgs);
      numberScrollBar.ValueChanged?.Invoke(numberScrollBar, new ValueChangedEventArgs<double>((double)e.OldValue, newValue));
    }
    #endregion

  }
}
