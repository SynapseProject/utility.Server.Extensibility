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
            if( args.Length == 0 )
                WriteHelpAndExit();
            else
            {
                string parm = args[0];
                if( parm.Equals( "sample", StringComparison.OrdinalIgnoreCase ) )
                {
                    bool verbose = args.Length > 1 && args[1].Equals( "verbose", StringComparison.OrdinalIgnoreCase );
                    CreateSample( verbose );
                }
                else if( File.Exists( parm ) )
                {
                    GeneratorSettings gs = GeneratorSettings.Deserialize( parm );
                    gs.Validate();
                    CreateAssembly( gs );
                }
                else
                    WriteHelpAndExit();
            }
        }

        static void CreateAssembly(GeneratorSettings gs)
        {
            ConsoleColor defaultColor = Console.ForegroundColor;

            Directory.CreateDirectory( gs.OutputFolder );

            foreach( ApiController c in gs.ApiControllers )
            {
                string code = c.ToClassCode();
                string name = $"{gs.OutputFolder}\\{c.Name}.cs";
                if( !c.CreateClassFileOnly && !gs.Files.Contains( name, StringComparer.OrdinalIgnoreCase ) )
                    gs.Files.Add( name );
                File.WriteAllText( name, code );
                Console_WriteLine( $"Created {name}.", ConsoleColor.Cyan );
            }
            Console.WriteLine();

            CompilerResults results = new CSharpCodeProvider().CompileAssemblyFromFile(
                gs.Compiler.ToCompilerParameters( $"{gs.OutputFolder}\\{gs.OutputAssembly}" ), gs.Files.ToArray() );

            foreach( CompilerError err in results.Errors )
                Console_WriteLine( $"[{err.IsWarning.FormatString( "Warning", "-Error-" )}: ln {err.Line}/col {err.Column}]  {err.ErrorText}",
                    err.IsWarning ? ConsoleColor.Yellow : ConsoleColor.Red );
            Console.WriteLine();

            if( !results.Errors.HasErrors && File.Exists( $"{gs.OutputFolder}\\{gs.OutputAssembly}" ) )
            {
                Console_WriteLine( $"Created {gs.OutputAssembly}.", ConsoleColor.Green );
                Console.WriteLine();
            }
            Console.ForegroundColor = defaultColor;


            if( gs.CreateMakeFile )
                gs.SerializeMakeFile( gs.OutputFolder );
        }

        static void Console_WriteLine(string s, ConsoleColor color, params object[] args)
        {
            Console.ForegroundColor = color;
            Console.WriteLine( s, args );
        }

        static void CreateSample(bool verbose)
        {
            GeneratorSettings gs = new GeneratorSettings
            {
                OutputAssembly = "Synapse.CustomAssm.Sample",
                CreateMakeFile = true
            };

            gs.Validate();
            gs.ApiControllers.Add( ApiController.CreateSample() );

            CreateAssembly( gs );

            gs.SerializeSample( gs.OutputFolder, verbose );
        }

        static void WriteHelpAndExit()
        {
            Console.WriteLine( "Syntax: MakeAssembly.exe {path to settings file}|sample [verbose]" );
            Console.WriteLine( "        sample: Will generate sample settings file.\r\n" );

            Environment.Exit( 0 );
        }
    }
}