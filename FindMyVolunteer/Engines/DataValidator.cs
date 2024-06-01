using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace FindMyVolunteer.Engines {
  public static class DataValidator {
    public static object GetErrors(ModelStateDictionary modelState) {
      return new {
        type = "https://tools.ietf.org/html/rfc9110#section-15.5.1",
        title = "One or more validation errors occurred.",
        status = 400,
        errors = modelState.ToDictionary(k => k.Key, v => v.Value.Errors.Select(e => e.ErrorMessage).ToArray()),
        traceId = Guid.NewGuid().ToString()
      };
    }
    public static ModelStateDictionary AddIdentityErrors(ModelStateDictionary modelState, IdentityResult result) {
      foreach(IdentityError error in result.Errors) {
        if(error.Code.Contains("Password")) modelState.AddModelError("Password", error.Description);
        else if(error.Code.Contains("Email")) modelState.AddModelError("Email", error.Description);
        else if(error.Code.Contains("UserName")) modelState.AddModelError("Username", error.Description);
        else modelState.AddModelError(error.Code, error.Description);
      }
      return modelState;
    }
    public static object CreateError(string error, params string[] messages) => new {
      type = "https://tools.ietf.org/html/rfc9110#section-15.5.1",
      title = "One or more validation errors occurred.",
      status = 400,
      errors = new Dictionary<string, string[]> { { error, messages } },
      traceId = Guid.NewGuid().ToString()
    };

    public static object FormatErrors(Dictionary<string, string[]> errors) => new {
      type = "https://tools.ietf.org/html/rfc9110#section-15.5.1",
      title = "One or more validation errors occurred.",
      status = 400,
      errors,
      traceId = Guid.NewGuid().ToString()
    };

  }
}
