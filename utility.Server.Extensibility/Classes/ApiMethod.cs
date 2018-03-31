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
        public string CodeBlob { get; set; }

        public string ToClassCode()
        {
            string code = @"        [Http~~HttpMethod~~]
        [Route( ~~Route~~ )]
        public ~~ReturnType~~ ~~Name~~()
        {
            ~~CodeBlob~~
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
            code = Regex.Replace( code, "~~CodeBlob~~", CodeBlob );

            return code;
        }

        public static ApiMethod CreateHello(string helloFrom)
        {
            return new ApiMethod { Name = "Hello", Route = "hello", CodeBlob = $"return \"Hello from {helloFrom}Controller, World!\";" };
        }

        public static ApiMethod CreateWhoAmI()
        {
            return new ApiMethod { Name = "WhoAmI", Route = "hello/whoami", CodeBlob = "return ExtensibilityUtility.GetExecuteControllerInstance( null, null, null ).WhoAmI();" };
        }

        public static ApiMethod CreateSample()
        {
            ApiMethod sample = new ApiMethod
            {
                Name = "MyCustomMethod",
                Route = "custom/path",
                CodeBlob = "return \"foo\";"
            };

            return sample;
        }
    }
}