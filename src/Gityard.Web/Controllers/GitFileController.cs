using Microsoft.AspNetCore.Mvc;
using Gityard.Services;
using Gityard.Application.Dtos;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace Gityard.Controllers;

[Authorize]
[Produces("application/json")]
[ApiController]
[Route("")]
public class GitFileController : ControllerBase
{
    private readonly ILogger<GitFileController> _logger;
    private GitRepositoryService _repoService;
    public GitFileController(ILogger<GitFileController> logger, GitRepositoryService repoService)
    {
        _logger = logger;
        _repoService = repoService;
    }

    [HttpGet("{userName}/repositories")]
    public IEnumerable<string> UserRepositories(string userName)
    {
        var user = User.FindFirst(ClaimTypes.Name);
        if (user != null && user.Value == userName)
        {
            return _repoService.GetRepositories(userName);
        }
        return new List<string>();
    }

    [HttpPost("{userName}/{repoName}")]
    public string CreateRepository(string userName, string repoName, [FromBody] CreateRepositoryDto? repoDto)
    {
        var user = User.FindFirst(ClaimTypes.Name);
        if (user != null && user.Value == userName)
        {
            return "";
        }

        string result;
        if (repoDto == null || string.IsNullOrEmpty(repoDto.RemoteUrl))
        {
            result = _repoService.CreateRepository(userName, repoName);
        }
        else if (repoDto != null && !string.IsNullOrEmpty(repoDto.RemoteUrl))
        {
            result = _repoService.CreateRepository(userName, repoName, repoDto.RemoteUrl);
        }
        else
        {
            result = "";
        }
        return result;
    }

    [HttpGet("{userName}/{repoName}/branches")]
    public IEnumerable<BranchDto> Branches(string userName, string repoName)
    {
        return _repoService.Branches(userName, repoName);
    }

    [HttpGet("{userName}/{repoName}/commits")]

    public IEnumerable<CommitDto> Commits(string userName, string repoName)
    {
        return _repoService.Commits(userName, repoName);
    }

    [HttpGet("{userName}/{repoName}/commits/{branchName}")]

    public IEnumerable<CommitDto> Commits(string userName, string repoName, string branchName)
    {
        return _repoService.Commits(userName, repoName, branchName);
    }

    [HttpGet("{userName}/{repoName}/tags")]

    public IEnumerable<string> Tags(string userName, string repoName)
    {
        return _repoService.Tags(userName, repoName);
    }

    [HttpGet("{userName}/{repoName}")]
    [HttpGet("{userName}/{repoName}.git")]
    public IEnumerable<TreeDto> TreeView(string userName, string repoName)
    {
        return _repoService.GetTree(userName, repoName, "master", "");
    }

    [HttpGet("{userName}/{repoName}/tree/{brancheName}")]
    public IEnumerable<TreeDto> TreeView(string userName, string repoName, string brancheName)
    {
        return _repoService.GetTree(userName, repoName, brancheName, "");
    }

    [HttpGet("{userName}/{repoName}/tree/{brancheName}/{*path}")]

    public IEnumerable<TreeDto> TreeView(string userName, string repoName, string brancheName, string path)
    {
        return _repoService.GetTree(userName, repoName, brancheName, path);
    }
}
