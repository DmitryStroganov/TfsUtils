using System;
using System.Linq;
using System.Reflection;
using TfsUtils.Common;
using TfsUtils.Common.Configuration;

namespace TfsUtils
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var config = UtilConfigurationManager.LoadConfig(Assembly.GetExecutingAssembly().Location + ".config");

            var commandNameArgument = args.Any() ? args.First().Trim() : string.Empty;
            if ((args.Length < 1) || !args.Any() || string.IsNullOrEmpty(commandNameArgument) || !config.Commands.Keys.Contains(commandNameArgument))
            {
                Console.Error.WriteLine($"Usage: {typeof(Program).Assembly.GetName().Name} commandname {Environment.NewLine}");
                Console.Error.WriteLine($"command list: {Environment.NewLine}\t {string.Join($"{Environment.NewLine}\t", config.Commands.Keys)}");
                Environment.Exit(-1);
            }

            var utilCommand = config.Commands.First(cmd => cmd.Key == commandNameArgument);

            Console.Error.WriteLine($"Initializing {utilCommand.Value.Item1.Name}...");

            var tfsUtilInitializer = TfsUtilsFactory.GetInitializer(config.ServerUri, utilCommand.Value.Item2);
            var impl = TfsUtilsFactory.GetUtil<string>(utilCommand.Value.Item1, tfsUtilInitializer);

            if (impl.ValidateArguments(args))
            {
                impl.Invoke(args[1]);
            }
        }
    }
}