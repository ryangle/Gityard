using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;
using Gityard.Application;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;

namespace Gityard.Web;

public class GitCommandResult : IActionResult
{
    private string _gitPath;
    private GitCommandOptions _options;

    public GitCommandResult(string gitPath, GitCommandOptions options)
    {
        _gitPath = gitPath;
        _options = options;
    }

    public async Task ExecuteResultAsync(ActionContext context)
    {
        var httpBodyControlFeature = context.HttpContext.Features.Get<IHttpBodyControlFeature>();
        if (httpBodyControlFeature != null)
        {
            httpBodyControlFeature.AllowSynchronousIO = true;
        }
        HttpResponse response = context.HttpContext.Response;
        Stream responseStream = GetOutputStream(context.HttpContext);

        string contentType = $"application/x-{_options.Service}";
        if (_options.AdvertiseRefs)
        {
            contentType += "-advertisement";
        }
        response.ContentType = contentType;

        response.Headers.Add("Expires", "Fri, 01 Jan 1980 00:00:00 GMT");
        response.Headers.Add("Pragma", "no-cache");
        response.Headers.Add("Cache-Control", "no-cache, max-age=0, must-revalidate");

        ProcessStartInfo info = new ProcessStartInfo(_gitPath, _options.ToString())
        {
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true
        };

        var process = Process.Start(info);
        if (process != null)
        {
            using (process)
            {
                GetInputStream(context.HttpContext).CopyTo(process.StandardInput.BaseStream);

                if (_options.EndStreamWithNull)
                {
                    process.StandardInput.Write('\0');
                }
                process.StandardInput.Dispose();

                using (StreamWriter writer = new StreamWriter(responseStream))
                {
                    if (_options.AdvertiseRefs)
                    {
                        string service = $"# service={_options.Service}\n";
                        writer.Write($"{service.Length + 4:x4}{service}0000");
                        writer.Flush();
                    }

                    process.StandardOutput.BaseStream.CopyTo(responseStream);
                }

                process.WaitForExit();
            }
        }
        await Task.CompletedTask;
    }

    private Stream GetInputStream(HttpContext context)
    {
        return string.Equals(context.Request.Headers["Content-Encoding"], "gzip")
            ? new GZipStream(context.Request.Body, CompressionMode.Decompress)
            : context.Request.Body;
    }

    private Stream GetOutputStream(HttpContext context)
    {
        string acceptEncoding;
        if ((acceptEncoding = context.Request.Headers["Accept-Encoding"]) != null && acceptEncoding.Contains("gzip"))
        {
            context.Response.Headers.Add("Content-Encoding", "gzip");
            return new GZipStream(context.Response.Body, CompressionMode.Compress);
        }
        return context.Response.Body;
    }
}
