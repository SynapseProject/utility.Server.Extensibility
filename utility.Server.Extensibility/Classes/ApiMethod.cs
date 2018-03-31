using System;
using System.Collections.Generic;
using System.Linq;
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
        public ApiMethodOptions Options { get; set; }
        bool HasOptions { get { return Options != null; } }

        public string ToClassCode()
        {
            string code = @"        [Http~~HttpMethod~~]
        [Route( ~~Route~~ )]
        public ~~ReturnType~~ ~~Name~~()
        {
            ~~CodeBlob~~~~plan~~
        }
";

            if( string.IsNullOrWhiteSpace( Name ) )
                Name = $"CustomMethod{Utilities.GetSystemGeneratedName()}";
            if( string.IsNullOrWhiteSpace( Route ) )
                Route = Name.ToLower();
            if( string.IsNullOrWhiteSpace( ReturnType ) )
                ReturnType = "string";

            string plan = string.Empty;
            if( !string.IsNullOrWhiteSpace( PlanName ) )
            {
                plan = @"return (~~ReturnType~~)CallPlan( planUniqueName: ~~planUniqueName~~~~options~~);";
                plan = Regex.Replace( plan, "~~planUniqueName~~", $"{PlanName}" );
                plan = Regex.Replace( plan, "~~options~~", HasOptions ? Options.ToString() : null );
                if( !string.IsNullOrWhiteSpace( CodeBlob ) )
                    plan = "\r\n\r\n            " + plan;
            }
            code = Regex.Replace( code, "~~plan~~", plan );

            code = Regex.Replace( code, "~~HttpMethod~~", $"{HttpMethod}" );
            code = Regex.Replace( code, "~~Route~~", $"\"{Route}\"" );
            code = Regex.Replace( code, "~~ReturnType~~", ReturnType );
            code = Regex.Replace( code, "~~Name~~", Name );
            code = Regex.Replace( code, "~~CodeBlob~~", !string.IsNullOrWhiteSpace( CodeBlob ) ? CodeBlob : string.Empty );

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
                PlanName = "SomethingWonderful",
                CodeBlob = "string foo = \"foo\";",
                Options = new ApiMethodOptions
                {
                    SerializationType = SerializationType.Html,
                    TimeoutSeconds = 10,
                    PollingIntervalSeconds = 2
                }
            };

            return sample;
        }
    }

    public class ApiMethodOptions
    {
        public string Path { get; set; } = "Actions[0]:Result:ExitData";
        public SerializationType SerializationType { get; set; } = SerializationType.Json;
        public bool SetContentType { get; set; } = true;
        public int PollingIntervalSeconds { get; set; } = 1;
        public int TimeoutSeconds { get; set; } = 120;
        public string NodeRootUrl { get; set; } = null;

        public override string ToString()
        {
            List<string> v = new List<string>();
            string path = !string.IsNullOrWhiteSpace( Path ) && Path != "Actions[0]:Result:ExitData" ? $"path: \"{Path}\"" : null;
            string st = SerializationType != SerializationType.Json ? $"serializationType: {SerializationType}" : null;
            string sct = SetContentType ? null : $"setContentType: {SetContentType}";
            string pi = PollingIntervalSeconds == 1 ? null : $"pollingIntervalSeconds: {PollingIntervalSeconds}";
            string ts = TimeoutSeconds == 120 ? null : $"timeoutSeconds: {TimeoutSeconds}";
            string nru = !string.IsNullOrWhiteSpace( NodeRootUrl ) ? $"nodeRootUrl: \"{NodeRootUrl}\"" : null;

            string vars = ", " + string.Join( ", ", new string[] { path, st, sct, pi, ts, nru } ) + " ";

            return vars;
        }
    }
}