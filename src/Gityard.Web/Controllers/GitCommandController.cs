using Gityard.Application;
using Gityard.Services;
using Gityard.Web;
using Microsoft.AspNetCore.Mvc;

namespace Gityard.Controllers;

[ApiController]
[Route("")]
public class GitCommandController : ControllerBase
{
    private GitRepositoryService _gitRepositoryService;

    public GitCommandController(GitRepositoryService gitRepositoryService)
    {
        _gitRepositoryService = gitRepositoryService;
    }

    [HttpPost("{userName}/{repoName}.git/git-upload-pack")]
    public IActionResult GitUploadPack(string userName, string repoName)
    {
        var repo = _gitRepositoryService.GetRepository(userName, repoName);
        if (repo == null)
        {
            return NotFound($"not found {userName}/{repoName}.git");
        }
        var result = new GitCommandResult(_gitRepositoryService.Settings.GitPath, new GitCommandOptions(
             repo,
             "git-upload-pack",
             false,
             false));
        return result;
    }

    [HttpPost("{userName}/{repoName}.git/git-receive-pack")]
    public IActionResult GitReceivePack(string userName, string repoName)
    {
        var repo = _gitRepositoryService.GetRepository(userName, repoName);
        if (repo == null)
        {
            return NotFound($"not found {userName}/{repoName}.git");
        }
        var result = new GitCommandResult(_gitRepositoryService.Settings.GitPath, new GitCommandOptions(
           repo,
           "git-receive-pack",
           false));
        return result;
    }

    [HttpGet("{userName}/{repoName}.git/info/refs")]
    public IActionResult GetInfoRefs(string userName, string repoName, string service)
    {
        var repo = _gitRepositoryService.GetRepository(userName, repoName);
        if (repo == null)
        {
            return NotFound($"not found {userName}/{repoName}.git");
        }
        var result = new GitCommandResult(_gitRepositoryService.Settings.GitPath, new GitCommandOptions(
            repo,
            service,
            true));
        return result;
    }
}