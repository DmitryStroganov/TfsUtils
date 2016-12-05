using System.IO;
using Microsoft.TeamFoundation.Client;

namespace TfsUtils.Common.Model
{
    public interface ITfsUtilInitializer
    {
        TfsConnection Connection { get; }
        TextWriter OutputWriter { get; set; }
    }
}