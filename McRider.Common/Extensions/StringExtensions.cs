using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace McRider.Common.Extensions;


public static class StringExtensions
{
    #region String Formating

    /// <summary>
    /// Capitalizes the first character of the given sentence.
    /// </summary>
    /// <param name="sentence">The sentence to capitalize.</param>
    /// <returns>A string with the first character capitalized.</returns>
    /// <example>
    /// <code>
    /// string example = "hello world";
    /// string capitalized = example.Capitalize();
    /// Console.WriteLine(capitalized); // Outputs: Hello world
    /// </code>
    /// </example>
    public static string Capitalize(this string sentence)
    {
        if (string.IsNullOrEmpty(sentence))
        {
            throw new ArgumentException("Sentence cannot be null or empty.", nameof(sentence));
        }

        return $"{char.ToUpper(sentence[0])}{sentence.Substring(1)}";
    }
    #endregion

    #region IsHtml
    /// <summary>
    ///  Regex in C# to check if a string is an HTML string:
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public static bool IsHTML(this string input)
    {
        string pattern = @"<[^>]+>";
        Regex regex = new Regex(pattern);
        Match match = regex.Match(input);
        return match.Success;
    }
    #endregion

    #region IsJSON
    private static JsonSerializerSettings JsonSerializerSettings { get; set; } = new JsonSerializerSettings()
    {
        NullValueHandling = NullValueHandling.Ignore,
        ReferenceLoopHandling = ReferenceLoopHandling.Ignore
    };

    /// <summary>
    /// 
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public static bool IsJSON(this string input) => IsJSON(input, out object _);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public static bool IsJSON<T>(this string input, out T dic)
    {
        if (string.IsNullOrEmpty(input))
        {
            dic = default;
            return false;
        }

        try
        {
            dic = JsonConvert.DeserializeObject<T>(input, JsonSerializerSettings);
            return dic != null;
        }
        catch (Exception)
        {
            dic = default(T);
            return false;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public static bool IsJSON(this string input, out object dic, Type type)
    {
        if (string.IsNullOrEmpty(input))
        {
            dic = default;
            return false;
        }

        try
        {
            dic = JsonConvert.DeserializeObject(input, type, JsonSerializerSettings);
            return dic != null;
        }
        catch (Exception)
        {
            dic = null;
            return false;
        }
    }
    #endregion

    #region IsValidUrl
    /// <summary>
    /// Check is a string can be parsed to a Uri. 
    /// </summary>
    /// <param name="url"></param>
    /// <returns></returns>
    public static bool IsValidUrl(this string url) => url.IsValidUrl(false);

    /// <summary>
    /// Check is a string can be parsed to a Uri. 
    /// </summary>
    /// <param name="url"></param>
    /// <param name="sendHeadRequest"> If set to true the a HEAD Http request is used to check is the URL is alive</param>
    /// <returns></returns>
    public static bool IsValidUrl(this string url, bool sendHeadRequest)
    {
        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri) || uri == null)
            return false;

        if (!sendHeadRequest)
            return true;

        return uri.IsValidUrl();
    }

    /// <summary>
    /// This method uses HTTP HEAD request to check if the Uri exists
    /// </summary>
    /// <param name="uri"></param>
    /// <returns></returns>
    public static bool IsValidUrl(this Uri uri)
    {
        using var httpClient = new HttpClient();

        try
        {
            var headRequest = new HttpRequestMessage(HttpMethod.Head, uri);
            var response = httpClient.SendAsync(headRequest).Result; // Use .Result for synchronous call

            return response.IsSuccessStatusCode;
        }
        catch (HttpRequestException)
        {
            // Handle specific exception if needed
            return false;
        }
    }

    #endregion

    #region IsEmail
    /// <summary>
    /// Returns true if the email is (probably) a valid email address
    /// </summary>
    /// <param name="email"></param>
    /// <returns></returns>
    public static bool IsEmail(this string email)
    {
        if (String.IsNullOrEmpty(email))
            return false;

        if (_validEmailRegex == null)
            lock (_validEmailRegexLock)
                _validEmailRegex ??= CreateValidEmailRegex();

        return _validEmailRegex.IsMatch(email);
    }
    private static Regex? _validEmailRegex = null;
    private static readonly Object _validEmailRegexLock = new object();

    /// <summary>
    /// Complex Regex pattern to make a valid email pattern.
    /// </summary>
    /// <returns></returns>
    private static Regex CreateValidEmailRegex()
    {
        string validEmailPattern = @"^(?!\.)(""([^""\r\\]|\\[""\r\\])*""|"
            + @"([-a-z0-9!#$%&'*+/=?^_`{|}~]|(?<!\.)\.)*)(?<!\.)"
            + @"@[a-z0-9][\w\.-]*[a-z0-9]\.[a-z][a-z\.]*[a-z]$";

        return new Regex(validEmailPattern, RegexOptions.IgnoreCase);
    }
    #endregion

    #region IsBase64

    /// <summary>
    /// 
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static bool IsBase64(this string value, out Stream stream)
    {
        stream = null;

        if (string.IsNullOrEmpty(value) || value.Length % 4 != 0 || value.Contains(" ") || value.Contains("\t") || value.Contains("\r") || value.Contains("\n"))
            return false;

        try
        {
            stream = value.Base64StringToStream();
            return true;
        }
        catch (FormatException)
        {
            return false;
        }
    }

    public static bool IsBase64(this string value) => IsBase64(value, out var _);

    public static Stream Base64StringToStream(this string base64String)
    {
        byte[] bytes = Convert.FromBase64String(base64String);
        var ms = new MemoryStream(bytes);
        ms.Position = 0;
        return ms;
    }


    #endregion
}
