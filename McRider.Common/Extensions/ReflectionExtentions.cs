using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using System.Runtime.Serialization;

namespace McRider.Common.Extensions;


public static class ReflectionExtentions
{
    public static DateTime GetLastBuildDate(this Assembly assembly)
    {
        var buildPrefix = "+build";
        var attribute = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>();
        if (attribute?.InformationalVersion != null)
        {
            var value = attribute.InformationalVersion;
            var index = value.IndexOf(buildPrefix);
            if (index > 0)
            {
                value = value.Substring(index + buildPrefix.Length);
                if (DateTime.TryParseExact(value, "yyyyMMddHHmmss", CultureInfo.InvariantCulture, DateTimeStyles.None, out var result))
                    return result;
            }
        }
        else
        {
            var version = assembly.GetName().Version;
            return new DateTime(2000, 1, 1).AddDays(version.Build);
        }

        return DateTime.Parse("2024-01-17T22:49");
    }

    public static TOut Cast<TOut, T>(this T obj) where T : class => (TOut)Cast(obj, typeof(TOut));

    public static object? Cast<T>(this T @this, Type type) where T : class
    {
        if (@this == null)
            return FormatterServices.GetUninitializedObject(type);

        object? _return = null;
        type = Nullable.GetUnderlyingType(type) ?? type;

        try
        {
            var converter = TypeDescriptor.GetConverter(type);
            if (type.IsInstanceOfType(@this))
                _return = @this;

            else if (converter.IsValid(@this))
                _return = converter.ConvertFrom(@this);

            else if (@this is IConvertible && typeof(IConvertible).IsAssignableFrom(type))
                _return = Convert.ChangeType(@this, type);

            else if (@this is JArray && @this.ToString().IsJSON(out object arr, type))
                _return = arr;

            else if (@this is JObject && @this.ToString().IsJSON(out object obj, type))
                _return = obj;

            else if (JsonConvert.SerializeObject(@this).IsJSON(out object _obj, type))
                _return = _obj;

            else
                _return = null;
        }
        catch (Exception ex)
        {
            //Log.Error("Error casting object", ex);
            //throw new InvalidCastException("Error casting object", ex);
        }

        return _return;
    }

    public static Action Debounce(this Action action, int milliseconds = 500)
    {
        CancellationTokenSource cts = null;
        return () =>
        {
            cts?.Cancel();
            cts = new CancellationTokenSource();

            Task.Delay(milliseconds, cts.Token).ContinueWith(t =>
            {
                if (t.IsCompletedSuccessfully)
                    action.Invoke();
            }, TaskScheduler.Default);
        };
    }
    public static Action<T> Debounce<T>(this Action<T> func, int milliseconds = 500)
    {
        CancellationTokenSource cts = null;
        return arg =>
        {
            cts?.Cancel();
            cts = new CancellationTokenSource();

            Task.Delay(milliseconds, cts.Token).ContinueWith(t =>
            {
                if (t.IsCompletedSuccessfully)
                    func.Invoke(arg);
            }, TaskScheduler.Default);
        };
    }
    public static Action<T, T2> Debounce<T, T2>(this Action<T, T2> func, int milliseconds = 500)
    {
        CancellationTokenSource cts = null;
        return (arg, arg2) =>
        {
            cts?.Cancel();
            cts = new CancellationTokenSource();

            Task.Delay(milliseconds, cts.Token).ContinueWith(t =>
            {
                if (t.IsCompletedSuccessfully)
                    func.Invoke(arg, arg2);
            }, TaskScheduler.Default);
        };
    }
    public static Action<T, T2, T3> Debounce<T, T2, T3>(this Action<T, T2, T3> func, int milliseconds = 500)
    {
        CancellationTokenSource cts = null;
        return (arg, arg2, arg3) =>
        {
            cts?.Cancel();
            cts = new CancellationTokenSource();

            Task.Delay(milliseconds, cts.Token).ContinueWith(t =>
            {
                if (t.IsCompletedSuccessfully)
                    func.Invoke(arg, arg2, arg3);
            }, TaskScheduler.Default);
        };
    }

    public static Func<T1, Task<T2>> Debounce<T1, T2>(this Func<T1, T2> func, int milliseconds = 500)
    {
        var cts = default(CancellationTokenSource);
        var tcSources = new List<TaskCompletionSource<T2>>();
        var tcsLock = new Object();

        return arg =>
        {
            cts?.Cancel();
            cts = new CancellationTokenSource();
            var tcs = new TaskCompletionSource<T2>();
            tcSources.Add(tcs);

            Task.Delay(milliseconds, cts.Token).ContinueWith(t =>
            {
                lock (tcsLock)
                {
                    if (t.IsCompletedSuccessfully)
                    {
                        var obj = func.Invoke(arg);
                        foreach (var _t in tcSources)
                            _t.SetResult(obj);

                        tcSources.Clear();
                    }
                }
            }, TaskScheduler.Default);

            return tcs.Task;
        };
    }
    public static Func<T1, T2, Task<T3>> Debounce<T1, T2, T3>(this Func<T1, T2, T3> func, int milliseconds = 500)
    {
        var cts = default(CancellationTokenSource);
        var tcSources = new List<TaskCompletionSource<T3>>();
        var tcsLock = new object();

        return (arg1, arg2) =>
        {
            cts?.Cancel();
            cts = new CancellationTokenSource();
            var tcs = new TaskCompletionSource<T3>();
            tcSources.Add(tcs);

            Task.Delay(milliseconds, cts.Token).ContinueWith(t =>
            {
                lock (tcsLock)
                {
                    if (t.IsCompletedSuccessfully)
                    {
                        var obj = func.Invoke(arg1, arg2);
                        foreach (var _t in tcSources)
                            _t.SetResult(obj);

                        tcSources.Clear();
                    }
                }
            }, TaskScheduler.Default);

            return tcs.Task;
        };
    }

    public static object CallMethod(this object obj, string methodName, params object[] args)
    {
        var methods = obj.GetType().GetRuntimeMethods().ToArray();
        var method = methods.FirstOrDefault(p => p.Name == methodName);

        if (method != null)
        {
            var _args = method.GetParameters().Select((p, i) => args.ElementAtOrDefault(i)).ToArray();
            return method.Invoke(obj, _args);
        }

        return null;
    }

    public static object GetValue(this object obj, string propName)
    {
        if (obj == null) return null;
        if (string.IsNullOrEmpty(propName)) return obj;

        if (propName.Contains("."))
        {
            var parts = propName.Split('.');
            var _obj = GetValue(obj, parts.First());
            if (_obj != null)
                return GetValue(_obj, string.Join(".", parts.Skip(1)).TrimStart('.', '$', '[', ']'));
            return null;
        }

        if (propName.StartsWith("$") || propName.StartsWith("[]") || obj is IEnumerable)
        {
            var array = obj as IEnumerable;
            if (array != null)
            {
                var _array = array.Cast<object>()
                    .Select(a => GetValue(a, propName.TrimStart('$', '[', ']')))
                    .Where(a => a != null)
                    .ToArray();
                if (_array.Any()) return _array;
            }
            return null;
        }

        var props = obj.GetType().GetRuntimeProperties();
        var prop = props.FirstOrDefault(p => p.Name == propName) ?? props.FirstOrDefault(p => p.Name.ToLower() == propName.ToLower());

        if (prop != null)
            return prop.GetValue(obj);

        var token = obj as JToken;
        if (token == null) return null;

        try
        {
            var dic = token.ToObject<IDictionary<string, object>>();
            var key = dic.Keys.FirstOrDefault(x => x.ToLower() == propName.ToLower());

            if (key != null && dic.ContainsKey(key))
                return dic[key];
        }
        catch (JsonSerializationException e)
        {
            //Log.Error("Error getting object value!", e);
        }

        return null;
    }

    public static object GetFirstValue(this object obj, params string[] possiblePropNames)
    {
        return possiblePropNames.Select(p => GetValue(obj, p)).FirstOrDefault(v => v != null);
    }

    public static PropertyInfo SetValue(this object obj, string propName, object value)
    {
        if (obj == null) return null;
        if (string.IsNullOrEmpty(propName)) return null;

        if (propName.Contains("."))
        {
            var parts = propName.Split('.');
            var _obj = GetValue(obj, parts.First());
            if (_obj != null)
                return SetValue(_obj, string.Join(".", parts.Skip(1)).TrimStart('.', '$', '[', ']'), value);

            return null;
        }

        var props = obj.GetType().GetRuntimeProperties().Where(p => p.CanWrite);
        var prop = props.FirstOrDefault(p => p.Name == propName) ?? props.FirstOrDefault(p => p.Name.ToLower() == propName.ToLower());

        if (prop != null && value != null)
            prop.SetValue(obj, value.Cast(prop.PropertyType));

        return prop;
    }

    public static PropertyInfo SetFirstValue(this object obj, object value, params string[] possiblePropNames)
    {
        foreach (var p in possiblePropNames)
        {
            var r = obj.SetValue(p, value);
            if (r != null) return r;
        }

        return null;
    }

}
