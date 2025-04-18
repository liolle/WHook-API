using System.Diagnostics;

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


    Console.WriteLine($"Starting script"); // Real-time forwarding


    process.OutputDataReceived += (sender, e) =>
    {
      if (!string.IsNullOrEmpty(e.Data)) // Important: e.Data will be null when stream closes
      {
        string output = e.Data;
        Console.WriteLine($"[STDOUT] {output}"); // Real-time forwarding
      }
    };

    process.ErrorDataReceived += (sender, e) =>
    {
      if (!string.IsNullOrEmpty(e.Data))
      {
        string error = e.Data;
        Console.WriteLine($"[STDERR] {error}"); // Real-time forwarding
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

      if (process.ExitCode != 0)
      {
        Console.WriteLine($"Batch process failed with exit code {process.ExitCode}.");
        return false; 
      }

      Console.WriteLine($"Process completed successfully.");
      return true;
    }
    catch (Exception e)
    {
      if (!process.HasExited){process.Kill();}
      Console.WriteLine($"Error executing process: {e.Message}");
      return false;
    }
    finally
    {
      process.Dispose();
    }
  }

}
