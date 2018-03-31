using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Synapse.Server.Extensibility.Utility.Properties;

namespace Synapse.Server.Extensibility.Utility
{
    public class ApiController
    {
        public string Name { get; set; }
        public string RoutePrefix { get; set; }
        public bool CreateHelloApiMethod { get; set; } = true;
        public bool CreateWhoAmIApiMethod { get; set; } = true;
        public bool CreateClassFileOnly { get; set; } = false;
        public List<ApiMethod> ApiMethods { get; set; } = new List<ApiMethod>();

        public virtual string ToClassCode()
        {
            string code = Resources.ControllerCode;

            if( string.IsNullOrWhiteSpace( Name ) )
                Name = $"SynapseCustomApi{Utilities.GetSystemGeneratedName()}";
            if( string.IsNullOrWhiteSpace( RoutePrefix ) )
                RoutePrefix = Name.ToLower();
            if( ApiMethods == null )
                ApiMethods = new List<ApiMethod>();
            if( CreateWhoAmIApiMethod )
                ApiMethods.Insert( 0, ApiMethod.CreateWhoAmI() );
            if( CreateHelloApiMethod )
                ApiMethods.Insert( 0, ApiMethod.CreateHello( Name ) );

            code = Regex.Replace( code, "~~RoutePrefix~~", $"\"{RoutePrefix}\"" );
            code = Regex.Replace( code, "~~Name~~", Name );
            code = Regex.Replace( code, "~~ApiMethods~~", ApiMethods.ToClassCode() );

            return code;
        }

        public static ApiController CreateSample()
        {
            ApiController sample = new ApiController { Name = "Custom", RoutePrefix = "my/route" };
            sample.ApiMethods.Add( ApiMethod.CreateSample() );

            return sample;
        }
    }
}