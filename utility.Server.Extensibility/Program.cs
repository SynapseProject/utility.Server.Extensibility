using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using Microsoft.CSharp;

namespace Synapse.Server.Extensibility.Utility
{
    class Program
    {
        static void Main(string[] args)
        {
            CreateSample();
        }

        static void CreateAssembly(GeneratorSettings gs)
        {
            foreach( ApiController c in gs.ApiControllers )
            {
                string code = c.ToClassCode();
                c.Name = $"{c.Name}.cs";
                if( !gs.Files.Contains( c.Name, StringComparer.OrdinalIgnoreCase ) )
                    gs.Files.Add( c.Name );
                File.WriteAllText( c.Name, code );
            }

            CompilerResults results = new CSharpCodeProvider().CompileAssemblyFromFile(
                gs.Compiler.ToCompilerParameters( gs.OutputAssembly ), gs.Files.ToArray() );

            foreach( CompilerError err in results.Errors )
                Console.WriteLine( err.ErrorText );

            if( gs.CreateMakeFile )
                gs.SerializeMakeFile();
        }

        static void CreateSample()
        {
            GeneratorSettings gs = new GeneratorSettings
            {
                OutputAssembly = "Synapse.CustomAssm.Sample",
                CreateMakeFile = true
            };

            gs.Validate();
            gs.ApiControllers.Add( ApiController.CreateSample() );
            gs.SerializeSample();

            CreateAssembly( gs );
        }
    }
}