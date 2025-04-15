using System.ComponentModel.DataAnnotations;

namespace whook.models;

public class ApiKeyPayload
{
  [Required]
  public string API_KEY {get;set;}="";
  [Required]
  public string PROJECT_ID {get;set;}="";

  public override string ToString()
  {
    return $"DeployPayload:\n {API_KEY}\n {PROJECT_ID}\n";
  }

}


public class ProjectModel
{
  [Required]
  public string PROJECT_ID {get;set;}="";

  public override string ToString()
  {
    return $"Deploy request:\n {PROJECT_ID}\n";
  }

}

