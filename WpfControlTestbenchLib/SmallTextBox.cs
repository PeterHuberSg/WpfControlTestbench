/**************************************************************************************

WpfTestbench.SmallTextBox
=========================

When user clicks on the TextBox, all text gets immediately selected

Written 2014-2020 by Jürgpeter Huber 
Contact: PeterCode at Peterbox dot com

To the extent possible under law, the author(s) have dedicated all copyright and 
related and neighboring rights to this software to the public domain worldwide under
the Creative Commons 0 license (details see COPYING.txt file, see also
<http://creativecommons.org/publicdomain/zero/1.0/>). 

This software is distributed without any warranty. 
**************************************************************************************/
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;


namespace WpfTestbench {

  /// <summary>
  /// When user clicks on the TextBox, all text gets immediately selected
  /// </summary>
  public class SmallTextBox: TextBox {
    public SmallTextBox() {
      PreviewMouseDown += new MouseButtonEventHandler(textBox_PreviewMouseDown);
      GotFocus += new RoutedEventHandler(textBox_GotFocus);
      SelectionChanged += new RoutedEventHandler(textBox_SelectionChanged);
    }


    bool isMouseDown;


    void textBox_PreviewMouseDown(object sender, MouseButtonEventArgs e) {
      isMouseDown = true;
    }


    bool hasGotFocused;


    void textBox_GotFocus(object sender, RoutedEventArgs e) {
      if (isMouseDown) {
        //user clicked with mouse on TextBox. Wait for the SelectionChanged event to select all the text
        isMouseDown = false;
        hasGotFocused = true;
      } else {
        //user used Tab key, which does not change the selection and the SelectionChanged event will not get fired.
        SelectAll();
      }
    }

    void textBox_SelectionChanged(object sender, RoutedEventArgs e) {
      if (hasGotFocused) {
        hasGotFocused = false;
        SelectAll();
      }        
    }

  }
}
