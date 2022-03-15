//==========================================================================================================================================
// Copyright: Peter Huber, Singapore, 2014
// This code is contributed to the Public Domain. You might use it freely for any purpose, commercial or non-commercial. It is provided 
// "as-is." The author gives no warranty of any kind whatsoever. It is up to you to ensure that there are no defects, the code is 
// fit for your purpose and does not infringe on other copyrights. Use this code only if you agree with these conditions. The entire risk of 
// using it lays with you :-)
//==========================================================================================================================================


using System;
//using System.Data.SqlClient;
using System.Reflection;
using System.Text;


namespace WpfTestbench {

  
  public static class ExceptionExtension {
    		/// <summary>
		/// Lists all details of any exception type (not just PBoxExceptions) into
		/// a string
		/// </summary>
		static public string ToDetailString(this Exception thisException){
			StringBuilder exceptionInfo = new StringBuilder();
			int startPos;
			int titelLength; 
			
			// Loop through all exceptions
			Exception? currentException = thisException;	// Temp variable to hold InnerException object during the loop.
			int exceptionCount = 1;				// Count variable to track the number of exceptions in the chain.
			do {
				// exception type and message as title
				startPos = exceptionInfo.Length;
				exceptionInfo.Append(currentException.GetType().FullName);
				titelLength = exceptionInfo.Length - startPos;
				exceptionInfo.Append("\r\n");
				if (exceptionCount==1) {
				    //main exception
					exceptionInfo.Append('=', titelLength);
				} else {
					//inner exceptions
					exceptionInfo.Append('-', titelLength);
				}
				
//				if (currentException.GetType()==typeof(SqlException)) {
//					//display additional info for sql server exception
//					SqlErrorCollection errorCollection = ((SqlException)currentException).Errors;
//					SqlError thisError; 
//					// list all SQL errors
//					for (int i=0; i < errorCollection.Count; i++) {
//						thisError = errorCollection[i];
//						exceptionInfo.AppendFormat("\r\nError " + (i+1) + 
//							", Severity " + thisError.Class +
//							", Number " + thisError.Number +
//							", State " + thisError.State +
//							", Server " + thisError.Server);
//						if (thisError.Procedure.Length>0) {
//							//list stored procedure name only if not empty
//							exceptionInfo.AppendFormat("\r\nat " + thisError.Procedure + " Line " + errorCollection[i].LineNumber);
//						}
//						exceptionInfo.AppendFormat(": \r\n" + thisError.Message);
						
////						exceptionInfo.AppendFormat("\r\nError " + (i+1) + 
////							", Severity " + errorCollection[i].Class +
////							", Number " + errorCollection[i].Number +
////							", State " + errorCollection[i].State +
////							", Server " + errorCollection[i].Server);
////						if (errorCollection[i].Procedure.Length>0) {
////							//list stored procedure name only if not empty
////							exceptionInfo.AppendFormat("\r\nat " + errorCollection[i].Procedure + " Line " + errorCollection[i].LineNumber);
////						}
////						exceptionInfo.AppendFormat(": \r\n" + errorCollection[i].Message);
//					}
				
//				// non SQL exceptions
//				} else {
					exceptionInfo.Append("\r\n" + currentException.Message);
					// List the remaining properties of all other exceptions
					PropertyInfo[] propertiesArray = currentException.GetType().GetProperties();
					foreach (PropertyInfo property in propertiesArray) {
						// skip message, inner exception and stack trace
						if (property.Name != "InnerException" && property.Name != "StackTrace" && property.Name != "Message" && property.Name != "TargetSite") {
							if (property.GetValue(currentException, null) == null) {
								//skip empty properties
							} else {
								exceptionInfo.AppendFormat("\r\n" + property.Name + ": " + property.GetValue(currentException, null));
							}
						}
					}
				//}

				// record the StackTrace with separate label.
				if (currentException.StackTrace != null) {
					exceptionInfo.Append("\r\n" + currentException.StackTrace + "\r\n");
				}
				exceptionInfo.Append("\r\n");

				// continue with inner exception
				currentException = currentException.InnerException;
				exceptionCount++;
			} while (currentException!=null);
			
			return exceptionInfo.ToString();
		}
		
  }
}