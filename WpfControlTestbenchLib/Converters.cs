/********************************************************************************************************

WpfTestbench.Converters
=======================

WPF converters for double

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
using System.Globalization;
using System.Windows.Data;


namespace WpfTestbench {

  #region Double NAN to String Converter
  //      ------------------------------

  /// <summary>
  /// Converts between a double and string, converting any unparseable string to NAN.
  /// </summary>
  public class DoubleNanConverter: IValueConverter {

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
      return value==null ? null! : (object)value.ToString()!;
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

    public object? Convert(object value, Type targetType, object parameter, CultureInfo culture) {
      return value?.ToString();
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
