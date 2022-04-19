using System;
using System.IO;
using System.Text;
using LibGit2Sharp;

namespace Gityard.Application;

public class GitCommandOptions
{
    public bool AdvertiseRefs { get; set; }
    public bool EndStreamWithNull { get; set; }
    public string Service { get; set; }
    public Repository Repo { get; set; }

    public GitCommandOptions(Repository repo,
        string service,
        bool advertiseRefs,
        bool endStreamWithNull = true)
    {
        Repo = repo;
        Service = service;
        AdvertiseRefs = advertiseRefs;
        EndStreamWithNull = endStreamWithNull;
    }

    public override string ToString()
    {
        if (!Service.StartsWith("git-"))
        {
            throw new InvalidOperationException();
        }

        StringBuilder builder = new();

        builder.Append(Service.Substring(4));
        builder.Append(" --stateless-rpc");

        if (AdvertiseRefs)
            builder.Append(" --advertise-refs");

        builder.Append($@" ""{Repo.Info.Path.TrimEnd(Path.DirectorySeparatorChar)}""");

        return builder.ToString();
    }
}
