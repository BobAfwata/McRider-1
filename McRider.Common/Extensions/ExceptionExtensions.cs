using System.Reflection;

namespace McRider.Common.Extensions;

public static class ExceptionExtensions
{
    /// <summary>
    /// Get a string display of a specific exception
    /// </summary>
    /// <param name="ex"></param>
    /// <returns></returns>
    public static string GetDetails(this Exception ex)
    {
        PropertyInfo[] properties = ex.GetType().GetProperties();
        List<string> fields = new List<string>();
        foreach (PropertyInfo property in properties)
        {
            if (property.Name == "Message") continue;

            try
            {
                object? value = property.GetValue(ex, null);
                if (!string.IsNullOrEmpty(value?.ToString()))
                    fields.Add(String.Format("\t{0} = {1}", property.Name, value?.ToString() ?? String.Empty));
            }
            catch
            {
                /* Eat the Exception */
            }
        }

        return $"\n{ex.Message}\n" + String.Join(",\n\t", fields.ToArray());
    }
}
