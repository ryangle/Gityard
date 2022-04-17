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
    public IActionResult GitUploadPack(string userName, string repoName) =>
        new GitCommandResult(_gitRepositoryService.Settings.GitPath, new GitCommandOptions(
            _gitRepositoryService.GetRepository(userName, repoName),
            "git-upload-pack",
            false,
            false));

    [HttpPost("{userName}/{repoName}.git/git-receive-pack")]
    public IActionResult GitReceivePack(string userName, string repoName) =>
       new GitCommandResult(_gitRepositoryService.Settings.GitPath, new GitCommandOptions(
           _gitRepositoryService.GetRepository(userName, repoName),
           "git-receive-pack",
           false));

    [HttpGet("{userName}/{repoName}.git/info/refs")]
    public IActionResult GetInfoRefs(string userName, string repoName, string service) =>
        new GitCommandResult(_gitRepositoryService.Settings.GitPath, new GitCommandOptions(
            _gitRepositoryService.GetRepository(userName, repoName),
            service,
            true));
}