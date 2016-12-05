using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.IO;
using System.Xml;

namespace TfsUtils.Common.Configuration
{
    public class TfsUtilsConfigurationSection : ConfigurationSection
    {
        private const string SectionName = "TfsUtils";

        [ConfigurationProperty("commands")]
        public CommandElementElementsCollection CommandsSection
        {
            get { return (CommandElementElementsCollection) base["commands"]; }
            set { base["commands"] = value; }
        }

        public static TfsUtilsConfigurationSection LoadSettings(string configFileContents)
        {
            var tempConfigFile = Path.GetTempFileName();
            File.WriteAllText(tempConfigFile, configFileContents);

            var fileMap = new ExeConfigurationFileMap
            {
                ExeConfigFilename = tempConfigFile
            };

            var config = ConfigurationManager.OpenMappedExeConfiguration(fileMap, ConfigurationUserLevel.None);
            return GetConfiguration(config, SectionName);
        }

        public static TfsUtilsConfigurationSection GetSettings(string configFilePath)
        {
            return GetSettings(configFilePath, SectionName);
        }

        public static TfsUtilsConfigurationSection GetSettings(string configFilePath, string sectionName)
        {
            var configFileMap = new ExeConfigurationFileMap
            {
                ExeConfigFilename = configFilePath
            };

            var config = ConfigurationManager.OpenMappedExeConfiguration(configFileMap, ConfigurationUserLevel.None);

            return GetConfiguration(config, sectionName);
        }

        private static TfsUtilsConfigurationSection GetConfiguration(System.Configuration.Configuration config, string sectionName)
        {
            try
            {
                return config.GetSection(sectionName) as TfsUtilsConfigurationSection;
            }
            catch (ConfigurationErrorsException ex)
            {
                throw new InvalidOperationException(ex.BareMessage);
            }
        }

        public class CommandDefinitionElement : ConfigurationElement
        {
            [TypeConverter(typeof(TypeNameConverter))]
            [ConfigurationProperty("Type", IsKey = true, IsRequired = true)]
            public Type Type
            {
                get { return (Type) this["Type"]; }
                set { this["Type"] = value; }
            }

            [ConfigurationProperty("Alias", IsRequired = false)]
            public string Alias
            {
                get { return (string) this["Alias"]; }
                set { this["Alias"] = value; }
            }

            [ConfigurationProperty("properties")]
            public PropertiesDefinitionElement PropertiesSection
            {
                get { return (PropertiesDefinitionElement) base["properties"]; }
                set { base["properties"] = value; }
            }

            public class PropertiesDefinitionElement : ConfigurationElement
            {
                [TypeConverter(typeof(TypeNameConverter))]
                [ConfigurationProperty("Type", IsKey = true, IsRequired = true)]
                public Type Type
                {
                    get { return (Type) this["Type"]; }
                    set { this["Type"] = value; }
                }

                public IDictionary<string, object> PropertySettings { get; private set; }

                public T GetConfigurationProperty<T>()
                {
                    return DynamicMapper.MapTo<T>(PropertySettings);
                }

                protected override void DeserializeElement(XmlReader reader, bool serializeCollectionKey)
                {
                    if (reader.Name != "properties")
                    {
                        return;
                    }

                    var attributeValue = reader.GetAttribute("Type");
                    if (string.IsNullOrWhiteSpace(attributeValue))
                    {
                        throw new InvalidOperationException("properties type undefined.");
                    }

                    var converter = new TypeNameConverter();
                    Type = converter.ConvertFromString(attributeValue) as Type;

                    var propertySettings = new Dictionary<string, object>();

                    reader.MoveToElement();
                    while (reader.Read())
                    {
                        if (reader.IsEmptyElement || string.IsNullOrWhiteSpace(reader.Name.Trim()))
                        {
                            continue;
                        }

                        if ((reader.NodeType == XmlNodeType.EndElement) && (reader.Name == "properties"))
                        {
                            reader.MoveToElement();
                            break;
                        }

                        propertySettings.Add(reader.Name, reader.ReadInnerXml());
                    }

                    PropertySettings = propertySettings;
                }
            }
        }

        public class CommandElementElementsCollection : ConfigurationElementCollection
        {
            [ConfigurationProperty("ServerUri", IsRequired = true)]
            public string ServerUri
            {
                get { return (string) this["ServerUri"]; }
                set { this["ServerUri"] = value; }
            }

            public IEnumerable<CommandDefinitionElement> Commands
            {
                get
                {
                    var iterator = GetEnumerator();
                    while (iterator.MoveNext())
                    {
                        var item = iterator.Current as CommandDefinitionElement;

                        if (item == null)
                        {
                            continue;
                        }

                        yield return item;
                    }
                }
            }

            public override ConfigurationElementCollectionType CollectionType => ConfigurationElementCollectionType.BasicMap;

            protected override ConfigurationElement CreateNewElement()
            {
                throw new NotImplementedException();
            }

            protected override ConfigurationElement CreateNewElement(string elementName)
            {
                switch (elementName)
                {
                    case "command":
                        return new CommandDefinitionElement();

                    default:
                        throw new ConfigurationErrorsException($"Unrecognized element '{elementName}'.");
                }
            }

            protected override bool IsElementName(string elementName)
            {
                return true;
            }

            protected override object GetElementKey(ConfigurationElement element)
            {
                var e = element as CommandDefinitionElement;
                return e.GetHashCode();
            }
        }
    }
}