using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.Framework.Common;
using Microsoft.TeamFoundation.VersionControl.Client;
using TfsUtils.Common;
using TfsUtils.Common.Model;

namespace TfsUtils.Implementations
{
    public class CommentsSearcher : ITfsUtil<string>
    {
        private readonly TfsConnection _connection;
        private readonly TextWriter _outputWriter;
        private readonly CommentsSearcherSettings _settings;

        public CommentsSearcher(TfsUtilInitializer<CommentsSearcherSettings> initializer)
        {
            _connection = initializer.Connection;
            _outputWriter = initializer.OutputWriter ?? Console.Out;
            _settings = initializer.Setting;
        }

        public void Invoke(params string[] args)
        {
            if (!args.Any() || string.IsNullOrEmpty(args.First()))
            {
                throw new ArgumentNullException();
            }

            var projectPath = _settings.ProjectPath ?? @"$/product1/branch1";
            var dateFrom = _settings.DateFrom == default(DateTime) ? DateTime.Today.AddMonths(-6) : _settings.DateFrom;
            var dateTo = _settings.DateTo == default(DateTime) ? DateTime.UtcNow : _settings.DateTo;

            _connection.Connect(ConnectOptions.None);
            var vcs = _connection.GetService<VersionControlServer>();

            var changesets = vcs.QueryHistory(
                projectPath,
                VersionSpec.Latest,
                0,
                RecursionType.Full,
                null,
                new DateVersionSpec(dateFrom),
                new DateVersionSpec(dateTo),
                int.MaxValue,
                true,
                false);

            Console.Error.WriteLine($"Searching in {changesets.OfType<Changeset>().Count()} items...");

            foreach (Changeset changeset in changesets)
            {
                if (!string.IsNullOrEmpty(changeset.Comment.Trim()))
                {
                    if (changeset.Comment.IndexOf(args.First(), StringComparison.InvariantCultureIgnoreCase) == -1)
                    {
                        continue;
                    }

                    _outputWriter.WriteLine(
                        $"{changeset.Comment}{Environment.NewLine}{changeset.ChangesetId}{Environment.NewLine}{changeset.CreationDate}{Environment.NewLine}{changeset.Owner}{Environment.NewLine}");

                    if (changeset.Changes != null)
                    {
                        var files = new HashSet<string>();
                        foreach (var changedItem in changeset.Changes)
                        {
                            var item = changedItem.Item.ServerItem.Trim();
                            files.Add(item);
                        }

                        _outputWriter.WriteLine($"Files:{Environment.NewLine}{string.Join(Environment.NewLine, files.Select(item => $"\t{item}"))}");
                    }

                    if (changeset.AssociatedWorkItems != null)
                    {
                        var workitemMeta = new HashSet<string>();
                        foreach (var workItems in changeset.AssociatedWorkItems.Where(wi => wi.WorkItemType == "Task"))
                        {
                            workitemMeta.Add(workItems.Title.Trim());
                        }

                        _outputWriter.WriteLine(
                            $"{Environment.NewLine}WorkItems:{Environment.NewLine}{string.Join(Environment.NewLine, workitemMeta.Select(item => $"\t{item}"))}");
                    }

                    _outputWriter.WriteLine(Environment.NewLine);
                    _outputWriter.WriteLine(string.Join("", Enumerable.Repeat("#", 100)));
                }
            }

            _connection.Disconnect();
        }

        public bool ValidateArguments(params string[] args)
        {
            if ((args.Length < 2) || !args.Any() || string.IsNullOrEmpty(args.First()))
            {
                Console.Error.WriteLine($"Usage params of this command: \"comment keyword\"");
                return false;
            }

            return true;
        }
    }

    public class CommentsSearcherSettings
    {
        public string ProjectPath { get; set; }
        public DateTime DateFrom { get; set; }
        public DateTime DateTo { get; set; }
    }
}