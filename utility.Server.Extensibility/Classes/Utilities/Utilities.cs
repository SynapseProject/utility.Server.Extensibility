using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Synapse.Server.Extensibility.Utility
{
    public static class Utilities
    {
        public static string ToClassCode(this List<ApiMethod> apiMethods)
        {
            StringBuilder code = new StringBuilder( apiMethods.Count );
            foreach( ApiMethod m in apiMethods )
                code.AppendLine( m.ToClassCode() );

            return code.ToString().TrimEnd();
        }

        public static string GetSystemGeneratedName()
        {
            string s = Convert.ToBase64String( Guid.NewGuid().ToByteArray() );
            return Regex.Replace( s, "(?:[^a-zA-Z0-9])", string.Empty ).Substring( 0, 8 );
        }

        public static string FormatString(this bool value, string trueString = "T", string falseString = "F")
        {
            return value ? trueString : falseString;
        }
    }
}