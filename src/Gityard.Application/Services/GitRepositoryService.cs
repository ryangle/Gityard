using LibGit2Sharp;
using Microsoft.Extensions.Options;

namespace Gityard.Services;

public class GitRepositoryService
{
    private IOptions<GityardSettings> _settings;
    public GityardSettings Settings => _settings.Value;
    public IEnumerable<Repository> AllRepositories => RepositoryDirectories(Settings.BasePath).Select(d => new Repository(d.FullName));

    public GitRepositoryService(IOptions<GityardSettings> settings)
    {
        _settings = settings;
    }
    public IEnumerable<DirectoryInfo> RepositoryDirectories(string basePath)
    {
        var repos = new List<DirectoryInfo>();
        DirectoryInfo baseDir = new(basePath);
        foreach (DirectoryInfo path in baseDir.EnumerateDirectories())
        {
            string repPath = Repository.Discover(path.FullName);
            if (repPath != null)
            {
                repos.Add(new DirectoryInfo(repPath));
            }
        }
        return repos;
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="repoPath">user/repo</param>
    /// <returns></returns>
    public Repository GetRepository(string userName, string repoName) => new(Path.Combine(Settings.BasePath, userName, repoName));

    protected Commit GetLatestCommit(string userName, string repoName, string? branchName = null)
    {
        Repository repo = GetRepository(userName, repoName);

        Branch barnch = branchName == null ? repo.Head : repo.Branches.First(d => d.CanonicalName == branchName);
        return barnch.Tip;
    }
    public Repository CreateRepository(string userName, string repoName)
    {
        string repoPath = Path.Combine(Settings.BasePath, userName, repoName);
        Repository repo = new(Repository.Init(repoPath, true));
        return repo;
    }

    public Repository? CreateRepository(string userName, string repoName, string remoteUrl)
    {
        var repoPath = Path.Combine(Settings.BasePath, userName, repoName);
        try
        {
            using (var repo = new Repository(Repository.Init(repoPath, true)))
            {
                repo.Config.Set("core.logallrefupdates", true);
                repo.Network.Remotes.Add("origin", remoteUrl, "+refs/*:refs/*");
                var logMessage = "";
                foreach (var remote in repo.Network.Remotes)
                {
                    IEnumerable<string> refSpecs = remote.FetchRefSpecs.Select(x => x.Specification);
                    Commands.Fetch(repo, remote.Name, refSpecs, null, logMessage);
                }
                return repo;
            }
        }
        catch
        {
            try
            {
                Directory.Delete(repoPath, true);
            }
            catch
            {
                //todo:log create failed
            }
            return null;
        }
    }

    public void DeleteRepository(string userName, string repoName)
    {
        try
        {
            string repoPath = Path.Combine(Settings.BasePath, userName, repoName);
            Directory.Delete(repoPath, true);
        }
        catch
        {
            //log.delete failed
        }
    }

    public ReferenceCollection GetReferences(string userName, string repoName) => GetRepository(userName, repoName).Refs;

    public Tree GetFileTree(string userName, string repoName, string? branch = null)
    {
        return GetLatestCommit(userName, repoName, branch).Tree;
    }

    public TreeEntry GetFileTreeEntry(string userName, string repoName, string path, string? branch = null) => GetFileTree(userName, repoName, branch)[path];

    public IEnumerable<string> GetFiles(string userName, string repoName, string? branch = null)
    {
        Tree tree = GetFileTree(userName, repoName);
        Stack<TreeEntry> nodes = new Stack<TreeEntry>(tree);

        while (nodes.Count > 0)
        {
            TreeEntry entry = nodes.Pop();
            switch (entry.TargetType)
            {
                case TreeEntryTargetType.Blob:
                    yield return entry.Path.Replace(Path.DirectorySeparatorChar, '/');
                    break;
                case TreeEntryTargetType.Tree:
                    foreach (TreeEntry e in ((Tree)entry.Target))
                    {
                        nodes.Push(e);
                    }
                    break;
            }
        }
    }

    public string GetFileContents(string userName, string repoName, string filePath)
    {
        Tree fileTree = GetFileTree(userName, repoName);
        TreeEntry entry = fileTree[filePath];

        if (entry == null)
        {
            throw new Exception($"No file found at path \"{filePath}\"", null);
        }

        if (entry.TargetType != TreeEntryTargetType.Blob)
        {
            throw new Exception("Path doesn't point to a file", null);
        }

        return ((Blob)entry.Target).GetContentText();
    }
}
