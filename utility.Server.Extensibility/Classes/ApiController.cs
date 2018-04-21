using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

using Synapse.Server.Extensibility.Utility.Properties;


namespace Synapse.Server.Extensibility.Utility
{
    public class ApiController
    {
        public string Name { get; set; }
        public string AuthorizationTopic { get; set; }
        public string RoutePrefix { get; set; }
        public bool CreateHelloApiMethod { get; set; } = true;
        public bool CreateWhoAmIApiMethod { get; set; } = true;
        public bool CreateClassFileOnly { get; set; } = false;
        public DalInfo CreateDalApi { get; set; }
        public bool HasDalApi{ get { return CreateDalApi != null; } }
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
            {
                ApiMethod m = ApiMethod.CreateWhoAmI();
                if( !ApiMethods.Exists( a => a.Name == m.Name ) )
                    ApiMethods.Insert( 0, m );
            }

            if( CreateHelloApiMethod )
            {
                ApiMethod m = ApiMethod.CreateHello( Name );
                if( !ApiMethods.Exists( a => a.Name == m.Name ) )
                    ApiMethods.Insert( 0, m );
            }

            string dalApiCode = string.Empty;
            if( HasDalApi )
            {
                dalApiCode = Regex.Replace( Resources.DalApiCode, "~~class~~", CreateDalApi.Class ) + "\r\n";
                CreateDalApi.DalCode = Regex.Replace( Resources.DalCode, "~~class~~", CreateDalApi.Class );
            }
            code = Regex.Replace( code, "~~DalApiCode~~", dalApiCode );

            code = Regex.Replace( code, "~~AuthorizationTopic~~",
                string.IsNullOrWhiteSpace( AuthorizationTopic ) ? string.Empty : $"    [SynapseCustomAuthorize( \"{AuthorizationTopic}\" )]\r\n" );
            code = Regex.Replace( code, "~~RoutePrefix~~", $"\"{RoutePrefix}\"" );
            code = Regex.Replace( code, "~~Name~~", Name );
            code = Regex.Replace( code, "~~ApiMethods~~", ApiMethods.ToClassCode() );

            return code;
        }

        public static ApiController CreateSample()
        {
            ApiController sample = new ApiController
            {
                Name = "Custom",
                RoutePrefix = "my/route",
                AuthorizationTopic = "ApiTopic"
            };
            sample.ApiMethods.Add( ApiMethod.CreateSample() );

            return sample;
        }
    }
}