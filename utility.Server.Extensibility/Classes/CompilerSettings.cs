using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;

namespace Synapse.Server.Extensibility.Utility
{
    public class CompilerSettings
    {
        public int WarningLevel { get; set; }
        public bool TreatWarningsAsErrors { get; set; }
        public bool IncludeDebugInformation { get; set; }
        public string CompilerOptions { get; set; }
        public string CoreAssemblyFileName { get; set; }
        public string Win32Resource { get; set; }
        public List<string> ReferencedAssemblies { get; set; } = new List<string>();
        public List<string> LinkedResources { get; set; }
        public List<string> EmbeddedResources { get; }

        public CompilerParameters ToCompilerParameters(string outputAssembly)
        {
            CompilerParameters cp = new CompilerParameters
            {
                GenerateExecutable = false,
                GenerateInMemory = false,
                WarningLevel = WarningLevel,
                TreatWarningsAsErrors = TreatWarningsAsErrors,
                IncludeDebugInformation = IncludeDebugInformation,
                CompilerOptions = CompilerOptions,
                OutputAssembly = outputAssembly,
                CoreAssemblyFileName = CoreAssemblyFileName,
                Win32Resource = Win32Resource
            };

            if( LinkedResources != null && LinkedResources.Count > 0 )
                cp.LinkedResources.AddRange( LinkedResources.ToArray() );

            if( EmbeddedResources != null && EmbeddedResources.Count > 0 )
                cp.EmbeddedResources.AddRange( EmbeddedResources.ToArray() );

            if( ReferencedAssemblies != null && ReferencedAssemblies.Count > 0 )
                cp.ReferencedAssemblies.AddRange( ReferencedAssemblies.ToArray() );

            return cp;
        }
    }
}