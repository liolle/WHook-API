using Microsoft.AspNetCore.Mvc;
using whook.models;
using whook.services;

namespace whook.controllers;

public class WebHookController : ControllerBase 
{
  private readonly ILogger<WebHookController> _logger;
  private readonly IConfiguration _config;
  private readonly IScriptService _scriptService;
  private readonly IScriptLauncherService _scriptLauncher;

  public WebHookController(ILogger<WebHookController> logger, IConfiguration config, IScriptService scriptService, IScriptLauncherService scriptLauncher)
  {
    _scriptLauncher = scriptLauncher;
    _logger = logger;
    _scriptService = scriptService;
    _config = config;

    _logger.LogInformation("Incomming request");
  }

  public IActionResult Ping()
  {
    return Ok("Pong");
  }

  [Route("webhooks/deploy")]
  [HttpPost]
  public async Task<IActionResult> DeployWebHook([FromBody] ApiKeyPayload payload)
  {
    _logger.LogInformation("1");
    if (!ModelState.IsValid){
      _logger.LogInformation("1");
      return BadRequest("Missing body elements");
    }
    _logger.LogInformation("3");
    if (!_scriptService.IsApiKeyValidForScript(payload)){
      _logger.LogInformation("4");
      return Unauthorized();
    }
    _logger.LogInformation("5");
    string scriptDirectory = _config["SCRIPT_DIRECTORY"] ?? "";
    if (String.IsNullOrEmpty(scriptDirectory)){
      _logger.LogInformation("6");
      return BadRequest("Deployment failed");
    }
    _logger.LogInformation("7");
    bool result = await _scriptLauncher.Execute(payload.PROJECT_ID,""); 
    _logger.LogInformation("8");
    if (!result){
      _logger.LogInformation("9");
      return BadRequest("Deployment failed");
    }

    return Ok();
  }

  [Route("webhooks/key/generate")]
  [HttpPost]
  [RequireAdmin]
  public IActionResult AddKey([FromBody] ProjectModel model)
  {
    if (!ModelState.IsValid){return BadRequest("Missing project_id");}
    string? key = _scriptService.GenerateApiKey(model.PROJECT_ID); 
    if(key is null){return BadRequest("Key generation failed"); }

    return Ok(key);
  }

  [Route("webhooks/key/admin/generate")]
  [HttpPost]
  [RequireAdmin]
  public IActionResult AddAdminKey()
  {
    string? key = _scriptService.GenerateAdminApiKey(); 
    if(key is null){return BadRequest("Key generation failed"); }
    return Ok(key);
  }
}
