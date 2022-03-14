//using System;
//using System.Collections.Generic;
//using System.Globalization;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using System.Windows;
//using System.Windows.Data;


//namespace WpfTestbench {

//  /// <summary>
//  /// Converts between TextDecorations and string
//  /// </summary>
//  public class TextDecorationsConverter: IValueConverter {


//    /// <summary>
//    /// Converts TextDecorations to String
//    /// </summary>
//    public object? Convert(object? value, Type targetType, object parameter, CultureInfo culture) {
//      if (value == null)
//        return null;
//      if (value is TextDecorationCollectionConverter) {

//      }
//      if (value is TextDecoration textDecorationValue) {
//        if (textDecorationValue==TextDecorations.Underline[0]) {
//          return "Underline";
//        }
//        if (textDecorationValue==TextDecorations.OverLine[0]) {
//          return "OverLine";
//        }
//        if (textDecorationValue==TextDecorations.Baseline[0]) {
//          return "Baseline";
//        }
//        if (textDecorationValue==TextDecorations.Baseline[0]) {
//          return "Baseline";
//        }
//      }
//      throw new ArgumentException($"Cannot convert TextDecoration {value} to string.");
//    }


//    /// <summary>
//    /// Converts string to TextDecorations
//    /// </summary>
//    public object? ConvertBack(object? value, Type targetType, object parameter, CultureInfo culture) {
//      if (value == null)
//        return null;

//      if (value is string stringValue) {
//        switch (stringValue) {
//        case "Underline": return TextDecorations.Underline;
//        case "OverLine": return TextDecorations.OverLine;
//        case "Baseline": return TextDecorations.Baseline;
//        case "Strikethrough": return TextDecorations.Strikethrough;
//        }
//      }
//      throw new ArgumentException($"Cannot convert {value} to TextDecoration.");
//    }
//  }
//}
