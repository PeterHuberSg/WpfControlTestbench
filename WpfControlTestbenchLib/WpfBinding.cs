/********************************************************************************************************

WpfTestbench.WpfBinding
=======================

WpfBinding.Setup() makes code setting up a WPF Binding look a little bit nicer

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
      var newBinding = new Binding(sourcePath) {
        Source = sourceObject,
        Mode = bindingMode,
        Converter = converter,
        StringFormat = stringFormat
      };
      return (BindingExpression)targetFrameworkElement.SetBinding(tragetDependencyProperty, newBinding);
    }
  }
}
