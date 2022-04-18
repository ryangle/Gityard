using Gityard.Application.Dtos;
using LibGit2Sharp;
using Microsoft.Extensions.Options;

namespace Gityard.Services;

public class GitRepositoryService
{
    private IOptions<GityardSettings> _settings;
    public GityardSettings Settings => _settings.Value;

    public GitRepositoryService(IOptions<GityardSettings> settings)
    {
        _settings = settings;
    }
    public IEnumerable<string> GetRepositories(string userName)
    {
        DirectoryInfo userDir = new(Path.Combine(Settings.BasePath, userName));
        if (userDir.Exists)
        {
            foreach (var path in userDir.EnumerateDirectories())
            {
                string repPath = Repository.Discover(path.FullName);
                if (!string.IsNullOrEmpty(repPath))
                {
                    yield return path.Name;
                }
            }
        }
    }
    public string CreateRepository(string userName, string repoName)
    {
        string repoPath = Path.Combine(Settings.BasePath, userName, repoName);
        if (Directory.Exists(repoPath))
        {
            return "";
        }
        Repository repo = new(Repository.Init(repoPath, true));
        if (repo == null)
        {
            return "";
        }
        return repoName;
    }

    public string CreateRepository(string userName, string repoName, string remoteUrl)
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
                return repoName;
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
            return "";
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="repoPath">user/repo</param>
    /// <returns></returns>
    public Repository? GetRepository(string userName, string repoName)
    {
        string repoPath = Path.Combine(Settings.BasePath, userName, repoName);
        if (Directory.Exists(repoPath))
        {
            var d = Repository.Discover(repoPath);
            if (!string.IsNullOrEmpty(d))
            {
                return new(repoPath);
            }
        }
        return null;
    }
    public IEnumerable<string> Branches(string userName, string repoName)
    {
        var repo = GetRepository(userName, repoName);
        if (repo == null)
        {
            return new List<string>();
        }
        return repo.Branches.Select(b => b.FriendlyName);
    }
    public IEnumerable<CommitDto> Commits(string userName, string repoName, string brancheName = "", int pageNo = 1, int pageSize = 10)
    {
        var repo = GetRepository(userName, repoName);
        if (repo == null)
        {
            return new List<CommitDto>();
        }
        if (string.IsNullOrEmpty(brancheName))
        {
            return repo.Commits.Skip((pageNo - 1) * pageSize).Take(pageSize).Select(c => new CommitDto(c.Id.ToString().Substring(0, 7), c.MessageShort, c.Author.Name, c.Sha, c.Author.When));
        }
        var branch = repo.Branches[brancheName];
        if (branch == null)
        {
            return new List<CommitDto>();
        }
        return branch.Commits.Skip((pageNo - 1) * pageSize).Take(pageSize).Select(c => new CommitDto(c.Id.ToString().Substring(0, 7), c.MessageShort, c.Author.Name, c.Sha, c.Author.When));
    }
    public IEnumerable<string> Tags(string userName, string repoName)
    {
        var repo = GetRepository(userName, repoName);
        if (repo == null)
        {
            return new List<string>();
        }
        return repo.Tags.Select(c => c.FriendlyName);
    }

    public List<TreeDto> GetTree(string userName, string repoName, string brancheName, string path)
    {
        var result = new List<TreeDto>();
        var repo = GetRepository(userName, repoName);
        if (repo != null)
        {
            var commit = repo.Head.Tip;

            if (!string.IsNullOrEmpty(brancheName))
            {
                var branche = repo.Branches[brancheName];
                if (branche != null)
                {
                    commit = branche.Tip;
                }
            }
            var tree = commit.Tree;
            if (!string.IsNullOrEmpty(path))
            {
                var entry = commit[path];
                if (entry != null && entry.TargetType == TreeEntryTargetType.Tree)
                {
                    if (entry.Target is Tree)
                    {
                        tree = (Tree)entry.Target;
                    }
                }
            }
            result = RecursivelyDumpTreeContent(repo, "", tree);
        }
        return result;
    }
    protected Commit GetLatestCommit(string userName, string repoName, string? branchName = null)
    {
        Repository repo = GetRepository(userName, repoName);

        Branch barnch = branchName == null ? repo.Head : repo.Branches.First(d => d.CanonicalName == branchName);
        return barnch.Tip;
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
    private List<TreeDto> RecursivelyDumpTreeContent(IRepository repo, string prefix, Tree tree)
    {
        List<TreeDto> treeDtos = new();
        foreach (var treeEntry in tree)
        {
            var path = prefix + treeEntry.Name;
            var gitObject = treeEntry.Target;

            var meta = repo.ObjectDatabase.RetrieveObjectMetadata(gitObject.Id);
            treeDtos.Add(new TreeDto(gitObject.Id.ToString(), treeEntry.Mode.ToString(), meta.Size, path));

            if (treeEntry.TargetType == TreeEntryTargetType.Tree)
            {
                treeDtos.AddRange(RecursivelyDumpTreeContent(repo, path + "/", (Tree)gitObject));
            }
        }
        return treeDtos;
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
