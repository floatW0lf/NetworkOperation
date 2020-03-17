using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using CommandLine;
using NetworkOperation.Core;
using NetworkOperation.Core.Models;

namespace TemplateDispatcher
{
    public class Options
    {
        [Option("AOT", Required = false, HelpText = "Useful for IL2CPP compiler")]
        public bool AOTSupport { get; set; }
        
        [Option("assemblies", Required = true, HelpText = "Paths for dll")]
        public IEnumerable<string> AssemblyPaths { get; set; }
        
        [Option('o',"output", Required = true, HelpText = "Output path for generated .cs file")]
        public string Output { get; set; }
        
        [Option("side", Required = true, HelpText = "Side: Client or Server")]
        public Side Side { get; set; }
    }
    
    public static class Generator
    {
        static void Main(string[] args)
        {
            Parser.Default.ParseArguments<Options>(args).WithParsed(Generate).WithNotParsed(errors =>
            {
                Environment.Exit(1);
            });
        }

        private static void Generate(Options arg)   
        {
            var model = OperationRuntimeModel.CreateFromAttribute(arg.AssemblyPaths.Select(Assembly.LoadFile));
           
            Console.WriteLine($"Found operations:");
            foreach (var description in model)
            {
                Console.WriteLine(description.OperationType.FullName);
            }
            var dispatcher = new PreGeneratedDispatcherTemplate
            {
                Session = new Dictionary<string, object>()
                {
                    {"Model", model}, {"Side", arg.Side}, {"AOTSupport", arg.AOTSupport}
                }
            };
            dispatcher.Initialize();
            foreach (var error in dispatcher.Errors)
            {
                Console.WriteLine(error);
            }
            File.WriteAllText(Path.Combine(arg.Output,"PreGeneratedDispatcher.cs"), dispatcher.TransformText());
            
        }
    }
}
