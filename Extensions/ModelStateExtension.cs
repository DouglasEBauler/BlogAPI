using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace BlogAPI.Extensions;

public static class ModelStateExtension
{
    public static List<string> GetErrors(this ModelStateDictionary modelState)
    {
        var result = new List<string>();

        foreach (var value in modelState.Values)
            result.AddRange(value.Errors.Select(r => r.ErrorMessage));

        return result;
    }
}
