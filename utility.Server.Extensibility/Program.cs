using System;
using System.CodeDom.Compiler;
using System.IO;
using System.Linq;

using Microsoft.CSharp;

namespace Synapse.Server.Extensibility.Utility
{
    class Program
    {
        static void Main(string[] args)
        {
            try
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
            catch( Exception ex )
            {
                WriteExceptionAndExit( ex );
            }
        }

        static void CreateAssembly(GeneratorSettings gs)
        {
            ConsoleColor defaultColor = Console.ForegroundColor;

            //create output folder for this dll
            Directory.CreateDirectory( gs.OutputFolder );

            //loop through the ApiControllers and create a .cs for each one
            foreach( ApiController c in gs.ApiControllers )
            {
                string code = c.ToClassCode();
                string name = $"{gs.OutputFolder}\\{c.Name}.cs";
                //optionally include the file in the list to be compiled
                if( !c.CreateClassFileOnly && !gs.Files.Contains( name, StringComparer.OrdinalIgnoreCase ) )
                    gs.Files.Add( name );

                if( c.HasDalApi )
                {
                    gs.Files.Add( c.CreateDalApi.File );

                    string dalFile = $"{gs.OutputFolder}\\{c.CreateDalApi.Class}Dal.cs";
                    File.WriteAllText( dalFile, c.CreateDalApi.DalCode );
                    gs.Files.Add( dalFile );

                    gs.Compiler.ReferencedAssemblies.AddRange( new string[] { "System.Core.dll", "LiteDB.dll" } );
                }

                //write the file to disk
                File.WriteAllText( name, code );

                Console_WriteLine( $"Created {name}.", ConsoleColor.Cyan );
            }
            Console.WriteLine();


            if( gs.Files.Count > 0 )
            {
                //make the assm
                CompilerResults results = new CSharpCodeProvider().CompileAssemblyFromFile(
                    gs.Compiler.ToCompilerParameters( $"{gs.OutputFolder}\\{gs.OutputAssembly}" ), gs.Files.ToArray() );

                //write out any warnings/errors
                foreach( CompilerError err in results.Errors )
                    Console_WriteLine( $"[{err.IsWarning.FormatString( "Warning", "-Error-" )}: ln {err.Line}/col {err.Column}]  {err.ErrorText}\r\n  in {err.FileName}",
                        err.IsWarning ? ConsoleColor.Yellow : ConsoleColor.Red );
                Console.WriteLine();

                //write out last msg
                if( !results.Errors.HasErrors && File.Exists( $"{gs.OutputFolder}\\{gs.OutputAssembly}" ) )
                {
                    Console_WriteLine( $"Created {gs.OutputAssembly}.", ConsoleColor.Green );
                    Console.WriteLine();
                }
                Console.ForegroundColor = defaultColor;

                //optionally create make_file
                if( gs.CreateMakeFile )
                    gs.SerializeMakeFile( gs.OutputFolder );
            }
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
            Console.WriteLine( "Synapse Server Custom Controller Utility" );
            Console.WriteLine( "Syntax: Synapse.CustomController.exe {path to settings file}|sample [verbose]" );
            Console.WriteLine( "        sample: Will generate sample settings file.\r\n" );

            Environment.Exit( 0 );
        }

        static void WriteExceptionAndExit(Exception ex)
        {
            Exception exc = ex.ToFriendlyYamlException();
            string msg = ExceptionHelpers.UnwindException( ex );
            ConsoleColor defaultColor = Console.ForegroundColor;
            Console_WriteLine( msg, ConsoleColor.Red );
            Console.WriteLine();
            Console.ForegroundColor = defaultColor;

            Environment.Exit( 1 );
        }

        static void Console_WriteLine(string s, ConsoleColor color, params object[] args)
        {
            Console.ForegroundColor = color;
            Console.WriteLine( s, args );
        }
    }
}