using System;
using System.Text;
using System.Text.RegularExpressions;

namespace Synapse.Server.Extensibility.Utility
{
    public class ApiMethod
    {
        public string Name { get; set; }
        public string ReturnType { get; set; } = "string";
        public ApiHttpMethod HttpMethod { get; set; } = ApiHttpMethod.Get;
        public string Route { get; set; }
        public string PlanName { get; set; }
        public string ReturnStub { get; set; }

        public string ToClassCode_()
        {
            if( string.IsNullOrWhiteSpace( Name ) )
                Name = $"CustomMethod{Utilities.GetSystemGeneratedName()}";
            if( string.IsNullOrWhiteSpace( Route ) )
                Route = Name.ToLower();
            if( string.IsNullOrWhiteSpace( ReturnType ) )
                ReturnType = "string";

            StringBuilder s = new StringBuilder( 10 );
            s.Append( $"\t\t[Http{HttpMethod}]\r\n" );
            s.Append( $"\t\t[Route( \"{Route}\" )]\r\n" );
            s.Append( $"\t\tpublic {ReturnType} {Name}()\r\n" );
            s.Append( "\t\t{\r\n" );
            s.Append( $"\t\t\treturn {ReturnStub};\r\n" );
            s.Append( "\t\t}\r\n" );
            //s.Append( $"\t\t{}" );
            //s.Append( $"\t\t{}" );
            //s.Append( $"\t\t{}" );
            //s.Append( $"\t\t{}" );

            return s.Replace( "\t", "    " ).ToString();
        }

        public string ToClassCode()
        {
            string code = @"        [Http~~HttpMethod~~]
        [Route( ~~Route~~ )]
        public ~~ReturnType~~ ~~Name~~()
        {
            return ~~ReturnStub~~;
        }
";

            if( string.IsNullOrWhiteSpace( Name ) )
                Name = $"CustomMethod{Utilities.GetSystemGeneratedName()}";
            if( string.IsNullOrWhiteSpace( Route ) )
                Route = Name.ToLower();
            if( string.IsNullOrWhiteSpace( ReturnType ) )
                ReturnType = "string";

            code = Regex.Replace( code, "~~HttpMethod~~", $"{HttpMethod}" );
            code = Regex.Replace( code, "~~Route~~", $"\"{Route}\"" );
            code = Regex.Replace( code, "~~ReturnType~~", ReturnType );
            code = Regex.Replace( code, "~~Name~~", Name );
            code = Regex.Replace( code, "~~ReturnStub~~", ReturnStub );

            return code;
        }

        public static ApiMethod CreateSample()
        {
            ApiMethod sample = new ApiMethod
            {
                Name = "MyCustomMethod",
                Route = "/custompath",
                ReturnStub = "\"foo\""
            };

            return sample;
        }
    }
}