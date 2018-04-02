using System;
using System.Collections.Generic;

namespace Synapse.Server.Extensibility.Utility
{
    public class GeneratorSettings
    {
        public string OutputAssembly { get; set; }
        public bool CreateMakeFile { get; set; }
        public CompilerSettings Compiler { get; set; }
        public List<string> Files { get; set; } = new List<string>();
        public List<ApiController> ApiControllers { get; set; } = new List<ApiController>();

        [YamlDotNet.Serialization.YamlIgnore]
        public string OutputFolder { get; private set; }

        public void Validate()
        {
            if( string.IsNullOrWhiteSpace( OutputAssembly ) )
                OutputAssembly = Utilities.GetSystemGeneratedName() + ".dll";
            else if( !OutputAssembly.EndsWith( ".dll", StringComparison.OrdinalIgnoreCase ) )
                OutputAssembly = $"{OutputAssembly}.dll";

            OutputFolder = OutputAssembly.Remove( OutputAssembly.Length - 4, 4 );


            if( Compiler == null )
            {
                Compiler = new CompilerSettings
                {
                    IncludeDebugInformation = false,
                    TreatWarningsAsErrors = false,
                    WarningLevel = 3,
                    CompilerOptions = "/optimize"
                };
                Compiler.ReferencedAssemblies.AddRange( new string[] {
                    "Newtonsoft.Json.dll",
                    "Synapse.Core.dll",
                    "Synapse.Server.Extensibility.dll",
                    "System.Net.Http.dll",
                    "System.Net.Http.Formatting.dll",
                    "System.Web.Http.dll",
                    "YamlDotNet.dll"} );
            }

            if( Files == null )
                Files = new List<string>();
        }

        public void SerializeSample(string path, bool verbose = false)
        {
            GeneratorSettings clone = new GeneratorSettings
            {
                OutputAssembly = OutputAssembly,
                CreateMakeFile = true,
                Compiler = null,
                ApiControllers = ApiControllers,
                Files = null
            };

            YamlHelpers.SerializeFile( $"{path}\\{OutputAssembly}.sample.yaml", verbose ? this : clone, emitDefaultValues: verbose );
        }

        public void SerializeMakeFile(string path)
        {
            GeneratorSettings clone = new GeneratorSettings
            {
                OutputAssembly = OutputAssembly,
                CreateMakeFile = false,
                Compiler = Compiler,
                ApiControllers = null,
                Files = Files
            };

            YamlHelpers.SerializeFile( $"{path}\\{OutputAssembly}.make.yaml", clone );
        }

        public static GeneratorSettings Deserialize(string path)
        {
            return YamlHelpers.DeserializeFile<GeneratorSettings>( path );
        }
    }
}