using System.Diagnostics;
using System.Text;

namespace whook.services;

public interface IScriptLauncherService
{
  Task<bool> Execute(string project_id,string arguments);
}

public class ScriptLauncherService : IScriptLauncherService
{

  private readonly string SCRIPT_DIRECTORY =""; 
  private readonly ILogger<ScriptLauncherService> _logger;

  public ScriptLauncherService(IConfiguration config, ILogger<ScriptLauncherService> logger)
  {
    _logger = logger;
    SCRIPT_DIRECTORY = config["SCRIPT_DIRECTORY"] ?? "";
    if (String.IsNullOrEmpty(SCRIPT_DIRECTORY)){
      throw new Exception("Missing configuration: SCRIPT_DIRECTORY");
    }
  }

  public async Task<bool> Execute(string project_id, string arguments = "")
  {
    if (string.IsNullOrWhiteSpace(project_id)){return false;}

    CancellationToken cancellationToken = new CancellationTokenSource(TimeSpan.FromMinutes(5)).Token;
    string directory = $"{SCRIPT_DIRECTORY}/{project_id}";

    var process = new Process
    {
      StartInfo = new ProcessStartInfo
      {
        FileName = $"/bin/bash",
                 Arguments = $"{directory}/deploy.sh",
                 RedirectStandardOutput = true,
                 RedirectStandardError = true,
                 UseShellExecute = false,
                 CreateNoWindow = true,
                 WorkingDirectory = directory 
      },
        EnableRaisingEvents =true
    };

    StringBuilder outputBuilder = new();
    StringBuilder errorBuilder = new ();

    using AutoResetEvent outputWaitHandle = new(false);
    using AutoResetEvent errorWaitHandle = new(false);

    process.OutputDataReceived += (sender, e) =>
    {
      if (e.Data == null)
      {
        outputWaitHandle.Set();
      }
      else
      {
        outputBuilder.AppendLine(e.Data);
      }
    };

    process.ErrorDataReceived += (sender, e) =>
    {
      if (e.Data == null)
      {
        errorWaitHandle.Set();
      }
      else
      {
        errorBuilder.AppendLine(e.Data);
      }
    };

    try
    {
      process.Start();

      // Begin asynchronous output/error reading
      process.BeginOutputReadLine();
      process.BeginErrorReadLine();

      // Wait for process exit asynchronously
      await process.WaitForExitAsync(cancellationToken);

      // Wait for output/error streams to complete
      await Task.Run(() =>
          {
          WaitHandle.WaitAll(new[] { outputWaitHandle, errorWaitHandle });
          }, cancellationToken);

      if (process.ExitCode != 0)
      {
        _logger.LogError($"Batch process failed with exit code {process.ExitCode}.\n Error: {errorBuilder}\n");
        return false; 
      }

      return true;
    }
    catch (OperationCanceledException)
    {
      if (!process.HasExited){process.Kill();}
      _logger.LogError($"Process timeout exceeded\n");
      return false;
    }
    finally
    {
      process.Dispose();
    }
  }

}
