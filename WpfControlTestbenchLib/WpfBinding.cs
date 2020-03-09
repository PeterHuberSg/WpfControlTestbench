using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Windows;

namespace WpfTestbench {

  /// <summary>
  /// Helper class for setting up WPF bindings
  /// </summary>
  public static class WpfBinding {

    /// <summary>
    /// Allows the setup of a WPF binding with 1 line of code
    /// </summary>
    public static BindingExpression Setup(
      object sourceObject, string sourcePath,
      FrameworkElement targetFrameworkElement, DependencyProperty tragetDependencyProperty,
      BindingMode bindingMode,
      IValueConverter? converter = null,
      string? stringFormat = null) 
    {
      Binding newBinding = new Binding(sourcePath) {
        Source = sourceObject,
        Mode = bindingMode,
        Converter = converter,
        StringFormat = stringFormat
      };
      //newBinding.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
      return (BindingExpression)targetFrameworkElement.SetBinding(tragetDependencyProperty, newBinding);
    }

  
  }
}
