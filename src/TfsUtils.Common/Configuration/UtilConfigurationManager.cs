using System;
using System.Collections.Generic;
using System.Linq;

namespace TfsUtils.Common.Configuration
{
    public class UtilConfigurationManager
    {
        public Dictionary<string, Tuple<Type, object>> Commands { get; private set; }
        public Uri ServerUri { get; private set; }

        public static UtilConfigurationManager LoadConfig(string filePath)
        {
            var config = TfsUtilsConfigurationSection.GetSettings(filePath);
            return Configure(config);
        }

        public static UtilConfigurationManager GetConfig(string configuration)
        {
            var config = TfsUtilsConfigurationSection.LoadSettings(configuration);
            return Configure(config);
        }

        private static UtilConfigurationManager Configure(TfsUtilsConfigurationSection config)
        {
            var commandList = config.CommandsSection.Commands.ToList();

            var r = from cmd in commandList
                let commandName = !string.IsNullOrWhiteSpace(cmd.Alias) ? cmd.Alias : cmd.Type.Name
                select new
                {
                    CommandName = commandName,
                    CommandType = cmd.Type,
                    CommandProps = DynamicMapper.MapTo(cmd.PropertiesSection.PropertySettings, cmd.PropertiesSection.Type)
                };

            return new UtilConfigurationManager
            {
                ServerUri = new Uri(config.CommandsSection.ServerUri),
                Commands =
                    r.ToDictionary(kvp => kvp.CommandName,
                        kvp => Tuple.Create(kvp.CommandType, kvp.CommandProps),
                        StringComparer.InvariantCultureIgnoreCase)
            };
        }
    }
}