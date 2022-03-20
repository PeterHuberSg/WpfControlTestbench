/********************************************************************************************************

WpfTestbench.
=================

Lists different types of trace entries 

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
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace WpfTestbench {

  /// <summary>
  /// Different types of trace entries
  /// </summary>
  public enum TraceTypeEnum {
    undef = 0,
    Trace,
    Warning,
    Error,
    Exception
  }


  public static class TraceTypeEnumExtension {
    public static string ShortString(this TraceTypeEnum tracerSource) {
      return tracerSource switch {
        TraceTypeEnum.Trace => "Trc",
        TraceTypeEnum.Warning => "War",
        TraceTypeEnum.Error => "Err",
        TraceTypeEnum.Exception => "Exc",
        _ => tracerSource.ToString(),
      };
    }
  }

}