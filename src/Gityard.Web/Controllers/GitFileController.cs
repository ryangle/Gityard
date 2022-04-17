﻿using LibGit2Sharp;
using Microsoft.AspNetCore.Mvc;
using Gityard.Services;
using System.Text.Json;
using Gityard.Application.Dtos;

namespace Gityard.Controllers;

[ApiController]
[Route("")]
public class GitFileController : ControllerBase
{
    private GitRepositoryService _repoService;
    public GitFileController(GitRepositoryService repoService)
    {
        _repoService = repoService;
    }

    [HttpGet("{userName}/{repoName}")]
    [HttpGet("{userName}/{repoName}.git")]
    public IActionResult TreeView(string userName, string repoName)
    {
        return TreeView(userName, repoName, "master", null);
    }
    [HttpGet("{userName}/{repoName}/tree/{id}/{*path}")]
    public IActionResult TreeView(string userName, string repoName, string id, string? path)
    {
        var repoPath = Path.Combine(userName, repoName);
        Repository repo = _repoService.GetRepository(userName, repoName);
        Commit commit = repo.Branches[id]?.Tip ?? repo.Lookup<Commit>(id);

        if (commit == null)
        {
            return new JsonResult("");
        }
        Tree tree = commit.Tree;
        if (string.IsNullOrEmpty(path))
        {
            return new JsonResult(new TreeDto(tree.Id.ToString(), tree.Count, tree.Sha, ""));
        }
        TreeEntry entry = commit[path];
        return new JsonResult("");
    }

    [HttpGet("{userName}/{repoName}/blob/{id}/{*path}")]
    public IActionResult BlobView(string userName, string repoName, string id, string path)
    {
        return new JsonResult("");
    }
    [HttpGet("{userName}/{repoName}/raw/{id}/{*path}")]
    public IActionResult RawBlobView(string userName, string repoName, string id, string path)
    {
        return new JsonResult("");
    }
}
