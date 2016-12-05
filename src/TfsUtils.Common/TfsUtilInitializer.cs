using System;
using System.IO;
using Microsoft.TeamFoundation.Client;
using TfsUtils.Common.Model;

namespace TfsUtils.Common
{
    public class TfsUtilInitializer : ITfsUtilInitializer
    {
        public TfsConnection Connection { get; set; }
        public TextWriter OutputWriter { get; set; }

        public static TfsUtilInitializer GetDefault(Uri tfsServerUrl)
        {
            return new TfsUtilInitializer
            {
                Connection = TfsTeamProjectCollectionFactory.GetTeamProjectCollection(tfsServerUrl)
            };
        }

        public static TfsConnection GetTeamProjectConnection(Uri tfsServerUrl)
        {
            return TfsTeamProjectCollectionFactory.GetTeamProjectCollection(tfsServerUrl);
        }
    }

    public class TfsUtilInitializer<TSetting> : ITfsUtilInitializer
    {
        public TfsUtilInitializer(Uri tfsServerUrl, TSetting setting)
        {
            Connection = TfsUtilInitializer.GetTeamProjectConnection(tfsServerUrl);
            Setting = setting;
        }

        public TSetting Setting { get; private set; }
        public TfsConnection Connection { get; }
        public TextWriter OutputWriter { get; set; }
    }
}