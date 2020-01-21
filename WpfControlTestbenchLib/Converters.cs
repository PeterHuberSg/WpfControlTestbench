using System;
using System.Globalization;
using System.Windows.Data;


namespace WpfTestbench {

  #region Double NAN to String Converter
  //      ------------------------------

  /// <summary>
  /// Converts between a double and string, converting any unparsable string to NAN.
  /// </summary>
  public class DoubleNanConverter: IValueConverter {

    public object? Convert(object? value, Type targetType, object parameter, CultureInfo culture) {
      if (value==null) return null;

      return value.ToString();
    }


    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
      if (value is string stringValue && double.TryParse(stringValue, out var newValue)) {
        return newValue;
      } else {
        return double.NaN;
      }
    }
  }
  #endregion


  #region Double Positive to String Converter
  //      -----------------------------------

  /// <summary>
  /// Converts between a double and string, allowing only positive values. All other values become PositiveInfinity.
  /// </summary>
  public class DoublePositiveConverter: IValueConverter {

    public object? Convert(object? value, Type targetType, object parameter, CultureInfo culture) {
      if (value==null) return null;

      return value.ToString();
    }


    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
      if (value is string stringValue && double.TryParse(stringValue, out var newValue) && newValue>=0) {
        return newValue;
      } else {
        return double.PositiveInfinity;
      }
    }
  }
  #endregion
}
