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
                string parm = args[0].ToLower();
                if( parm == "sample" )
                    CreateSample();
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

            ConsoleColor defaultColor = Console.ForegroundColor;
            foreach( CompilerError err in results.Errors )
                Console_WriteLine( $"[{err.IsWarning.FormatString( "Warning", "-Error-" )}: ln {err.Line}/col {err.Column}]  {err.ErrorText}",
                    err.IsWarning ? ConsoleColor.Yellow : ConsoleColor.Red );
            Console.ForegroundColor = defaultColor;
            Console.WriteLine();

            if( gs.CreateMakeFile )
                gs.SerializeMakeFile();
        }

        static void Console_WriteLine(string s, ConsoleColor color, params object[] args)
        {
            Console.ForegroundColor = color;
            Console.WriteLine( s, args );
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

        static void WriteHelpAndExit()
        {
            Console.WriteLine( "Syntax: MakeAssembly.exe {path to settings file}|sample" );
            Console.WriteLine( "        sample: Will generate sample settings file.\r\n" );

            Environment.Exit( 0 );
        }
    }
}