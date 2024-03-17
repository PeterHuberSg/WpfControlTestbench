/********************************************************************************************************

WpfTestbench.NumberScrollBar
============================

A control consisting of a TextBox and a Scrollbar, allowing the user to enter a number with the keyboard or mouse

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

Written 2014-2024 in Switzerland & Singapore by Jürgpeter Huber 

Contact: https://github.com/PeterHuberSg/WpfControlTestbench
********************************************************************************************************/using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows;
using System;


namespace WpfTestbench {

  #region Value Changed EventArgs
  //      =======================

  /// <summary>
  /// ValueChangedEventArgs contains old and new values when event is raised.
  /// </summary>
  public class ValueChangedEventArgs<T>: EventArgs {
    /// <summary>
    /// Constructor
    /// </summary>
    public ValueChangedEventArgs(T oldValue, T newValue): base() {
      _oldValue = oldValue;
      _newValue = newValue;
    }


    /// <summary>
    /// Return value before event occurred
    /// </summary>
    public T OldValue {
      get { return _oldValue; }
    }


    /// <summary>
    /// Return value after event occurred
    /// </summary>
    public T NewValue {
      get { return _newValue; }
    }


    readonly T _oldValue;
    readonly T _newValue;
  }
  #endregion


  /// <summary>
  /// Control consisting of ScrollBar and TextBox. User can use mouse with the ScrollBar or keyboard with the TextBox to 
  /// change Value. 
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


    /// <summary>
    /// User can enter Value here with the keyboard.
    /// </summary>
    public SelectAllTextBox ValueTextBox { get; private set; }


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
      ValueTextBox = new SelectAllTextBox {
        Text = Value.ToString(),
        VerticalAlignment = VerticalAlignment.Center,
        TextAlignment = TextAlignment.Right
      };
      ValueTextBox.TextChanged += valueTextBox_TextChanged;
      ValueTextBox.LostFocus += valueTextBox_LostFocus;
      WpfTestbench.WpfBinding.Setup(this, "TextBoxWidth", ValueTextBox, TextBox.WidthProperty, System.Windows.Data.BindingMode.OneWay);
      DockPanel.SetDock(ValueTextBox, Dock.Right);
      Children.Add(ValueTextBox);

      ValueScrollBar = new ScrollBar { Orientation = Orientation.Horizontal };
      ValueScrollBar.ValueChanged += valueScrollBar_ValueChanged;
      WpfTestbench.WpfBinding.Setup(this, "Minimum", ValueScrollBar, ScrollBar.MinimumProperty, System.Windows.Data.BindingMode.OneWay);
      WpfTestbench.WpfBinding.Setup(this, "Maximum", ValueScrollBar, ScrollBar.MaximumProperty, System.Windows.Data.BindingMode.OneWay);
      WpfTestbench.WpfBinding.Setup(this, "SmallChange", ValueScrollBar, ScrollBar.SmallChangeProperty, System.Windows.Data.BindingMode.OneWay);
      WpfTestbench.WpfBinding.Setup(this, "LargeChange", ValueScrollBar, ScrollBar.LargeChangeProperty, System.Windows.Data.BindingMode.OneWay);
      WpfTestbench.WpfBinding.Setup(this, "LargeChange", ValueScrollBar, ScrollBar.ViewportSizeProperty, System.Windows.Data.BindingMode.OneWay);
      Children.Add(ValueScrollBar);
    }
    #endregion


    #region Event Handlers
    //      --------------

    bool isEventHandlerActive = false;


    void valueTextBox_TextChanged(object sender, TextChangedEventArgs e) {
      if (isEventHandlerActive) return;

      isEventHandlerActive = true;
      if (double.TryParse(ValueTextBox.Text, out double newValue)) {
        ValueScrollBar.Value = newValue;
      }
      isEventHandlerActive = false;
    }


    void valueTextBox_LostFocus(object sender, System.Windows.RoutedEventArgs e) {
      if (isEventHandlerActive) return;

      isEventHandlerActive = true;
      if (double.TryParse(ValueTextBox.Text, out double newValue)) {
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


    private static void valueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
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

      if (numberScrollBar.ValueChanged!=null) {
        numberScrollBar.ValueChanged(numberScrollBar, new ValueChangedEventArgs<double>((double)e.OldValue, newValue));
      }
    }
    #endregion

  }
}
