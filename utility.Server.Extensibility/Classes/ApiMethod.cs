using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Synapse.Server.Extensibility.Utility
{
    public class ApiMethod
    {
        public ApiHttpMethod HttpMethod { get; set; } = ApiHttpMethod.Get;
        public string Route { get; set; }
        public string ReturnType { get; set; } = "string";
        public string Name { get; set; }
        public string Parms { get; set; }
        public string PlanName { get; set; }
        public bool ExecuteAsync { get; set; }
        public string CodeBlob { get; set; }
        public ApiMethodOptions Options { get; set; }
        bool HasOptions { get { return Options != null; } }

        public string ToClassCode()
        {
            string code = @"        [Http~~HttpMethod~~]
        [Route( ~~Route~~ )]
        public ~~ReturnType~~ ~~Name~~(~~parms~~)
        {
            ~~pd~~~~CodeBlob~~~~plan~~
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
                plan = @"return (~~ReturnType~~)StartPlan( planUniqueName: ~~planUniqueName~~~~pdp~~~~options~~~~async~~ );";
                plan = Regex.Replace( plan, "~~planUniqueName~~", $"\"{PlanName}\"" );
                plan = Regex.Replace( plan, "~~options~~", HasOptions ? Options.ToString() : string.Empty );
                plan = Regex.Replace( plan, "~~async~~", ExecuteAsync ? ", executeAsync: true" : string.Empty );
                if( !string.IsNullOrWhiteSpace( CodeBlob ) )
                    plan = "\r\n\r\n            " + plan;
            }
            code = Regex.Replace( code, "~~plan~~", plan );

            code = Regex.Replace( code, "~~HttpMethod~~", $"{HttpMethod}" );
            code = Regex.Replace( code, "~~Route~~", $"\"{Route}\"" );
            code = Regex.Replace( code, "~~ReturnType~~", ReturnType );
            code = Regex.Replace( code, "~~Name~~", Name );
            code = Regex.Replace( code, "~~parms~~", $"{Parms}" );
            string pd = SplitParmsToDict( Parms );
            code = Regex.Replace( code, "~~pd~~", pd );
            string pdp = !string.IsNullOrWhiteSpace( pd ) ? ", parms: parms" : string.Empty;
            code = Regex.Replace( code, "~~pdp~~", pdp );
            code = Regex.Replace( code, "~~CodeBlob~~", !string.IsNullOrWhiteSpace( CodeBlob ) ? CodeBlob : string.Empty );

            return code;
        }

        public string SplitParmsToDict(string methodParameters)
        {
            if( string.IsNullOrWhiteSpace( methodParameters ) )
                return string.Empty;

            StringBuilder code = new StringBuilder();
            code.AppendLine( "Dictionary<string, string> parms = new Dictionary<string, string>();" );
            string[] parms = methodParameters.Split( ',' );
            foreach( string parm in parms )
            {
                string[] p = parm.Trim().Split( ' ' );
                code.AppendLine( $"            parms[\"{p[1]}\"] = string.Format( \"{{{0}}}\", {p[1]} );" );
            }
            code.Append( "\r\n            " );

            return code.ToString();
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
                Route = "{interesting}/path",
                ReturnType = "object",
                Name = "MyCustomMethod",
                Parms = "string interesting, string aaa, string bbb, string ccc = \"foo\"",
                PlanName = "sampleHtml",
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

            if( !string.IsNullOrWhiteSpace( Path ) && Path != "Actions[0]:Result:ExitData" )
                v.Add( $"path: \"{Path}\"" );
            if( SerializationType != SerializationType.Json )
                v.Add( $"serializationType: SerializationType.{SerializationType}" );
            if( !SetContentType )
                v.Add( $"setContentType: {SetContentType}" );
            if( PollingIntervalSeconds != 1 )
                v.Add( $"pollingIntervalSeconds: {PollingIntervalSeconds}" );
            if( TimeoutSeconds != 120 )
                v.Add( $"timeoutSeconds: {TimeoutSeconds}" );
            if( !string.IsNullOrWhiteSpace( NodeRootUrl ) )
                v.Add( $"nodeRootUrl: \"{NodeRootUrl}\"" );

            return ", " + string.Join( ", ", v.ToArray() );
        }
    }
}