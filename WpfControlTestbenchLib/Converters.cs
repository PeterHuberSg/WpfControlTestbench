/**************************************************************************************

WpfTestbench.Converters
=======================

WPF converters for double

Written 2014-2020 by Jürgpeter Huber 
Contact: PeterCode at Peterbox dot com

To the extent possible under law, the author(s) have dedicated all copyright and 
related and neighboring rights to this software to the public domain worldwide under
the Creative Commons 0 license (details see COPYING.txt file, see also
<http://creativecommons.org/publicdomain/zero/1.0/>). 

This software is distributed without any warranty. 
**************************************************************************************/
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

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
      if (value==null) return null!;

      return value.ToString()!;
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

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
      if (value==null) return null!;

      return value.ToString()!;
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
