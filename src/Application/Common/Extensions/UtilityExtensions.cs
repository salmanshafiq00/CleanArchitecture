using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Text.RegularExpressions;
using Application.Common.Models;

namespace Application.Common.Extensions;

public static class UtilityExtensions
{
    public static string GetDisplayName(this Enum enumValue)
    {
        var member = enumValue
             .GetType()
             .GetMember(enumValue.ToString())
             .FirstOrDefault();

        var displayAttribute = member?
            .GetCustomAttributes(typeof(DisplayAttribute), false)
            .OfType<DisplayAttribute>()
            .FirstOrDefault();

        return displayAttribute?.GetName() ?? nameof(enumValue);
    }

    public static string SplitWordByUppper(this string value)
    {
        return Regex.Replace(value, "(\\B[A-Z])", " $1").Trim();
    }

    public static bool IsNullOrEmpty(this Guid value) => value == Guid.Empty;
    public static bool IsNullOrEmpty(this Guid? value) => value is null || value == Guid.Empty;

    public static IEnumerable<string> GetColumnNameFromClass(Type model)
    {
        PropertyInfo[] properties = model.GetProperties();

        return properties.Select(prop => prop.Name);
    }

    public static string GetDateTimeStampRef(string? prefix = null, string? postfix = null)
    {
        if (!string.IsNullOrWhiteSpace(prefix) && string.IsNullOrWhiteSpace(postfix))
        {
            return $"{prefix}{DateTime.Now:yyyyMMddhhmmssffff}";
        }
        else if (string.IsNullOrWhiteSpace(prefix) && !string.IsNullOrWhiteSpace(postfix))
        {
            return $"{DateTime.Now:yyyyMMddhhmmssffff}{postfix}";
        }
        else if (!string.IsNullOrWhiteSpace(prefix) && !string.IsNullOrWhiteSpace(postfix))
        {
            return $"{prefix}{DateTime.Now:yyyyMMddhhmmssffff}{postfix}";
        }
        else
        {
            return DateTime.Now.ToString("yyyyMMddhhmmssffff");
        }
    }

    public static List<SelectListModel> GetActiveInactiveSelectList() =>
        [
            new() {Id = 1, Name = "Active", Severity = "success"},
            new() {Id = 0, Name = "Inactive", Severity = "danger"}
        ];
}

