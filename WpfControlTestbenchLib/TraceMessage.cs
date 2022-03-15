//==========================================================================================================================================
// Copyright: Peter Huber, Singapore, 2014
// This code is contributed to the Public Domain. You might use it freely for any purpose, commercial or non-commercial. It is provided 
// "as-is." The author gives no warranty of any kind whatsoever. It is up to you to ensure that there are no defects, the code is 
// fit for your purpose and does not infringe on other copyrights. Use this code only if you agree with these conditions. The entire risk of 
// using it lays with you :-)
//==========================================================================================================================================


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace WpfTestbench {

  
  public class TraceMessage {
    public readonly TraceTypeEnum TraceType;
    public readonly DateTime Created;
    public readonly string Message;
    public readonly string? FilterText;

    private string? asString;


    public TraceMessage(TraceTypeEnum tracrType, string message, string? filterText = null) {
      TraceType = tracrType;
      Created = DateTime.Now;
      Message = message;
      FilterText = filterText;
    }


    public override string ToString() {
      if (asString==null) {
        asString = TraceType.ShortString() + Created.ToString(" HH:mm:ss.fff ") + Message;
      }
      return asString;
    }
  }
}
