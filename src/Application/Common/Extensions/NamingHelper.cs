namespace Application.Common.Extensions;

public static class NamingHelper
{
    public static string Pluralize(string singularValue)
    {
        if (singularValue.EndsWith("y", StringComparison.OrdinalIgnoreCase))
        {
            return singularValue.Remove(singularValue.Length - 1) + "ies";
        }
        else if (singularValue.EndsWith("s", StringComparison.OrdinalIgnoreCase))
        {
            return singularValue + "es";
        }
        else
        {
            return singularValue + "s";
        }
    }
}
