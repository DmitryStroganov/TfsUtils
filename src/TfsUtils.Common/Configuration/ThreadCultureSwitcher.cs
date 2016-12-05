using System;
using System.Globalization;
using System.Threading;

namespace TfsUtils.Common.Configuration
{
    public class ThreadCultureSwitcher : IDisposable
    {
        private readonly CultureInfo backupCultureInfo;

        public ThreadCultureSwitcher(CultureInfo cultureInfo)
        {
            backupCultureInfo = Thread.CurrentThread.CurrentCulture;
            Thread.CurrentThread.CurrentCulture = cultureInfo;
        }

        public void Dispose()
        {
            Thread.CurrentThread.CurrentCulture = backupCultureInfo;
        }
    }
}