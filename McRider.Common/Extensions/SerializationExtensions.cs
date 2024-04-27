using Newtonsoft.Json;
using System.IO.Compression;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;

namespace McRider.Common.Extensions;

public static class SerializationExtensions
{
    public static bool DISABLE_COMPRESSION = true;

    /// <summary>
    /// Convert an object to a byte array
    /// </summary>
    /// <typeparam name="E"></typeparam>
    /// <param name="obj"></param>
    /// <returns></returns>
    public static byte[] ToBytes<E>(this E obj) where E : class
    {
        //var selializable = Wrapper<E>.Wrap(obj);
        var attribute = (SerializableAttribute)obj.GetType().GetCustomAttribute(typeof(SerializableAttribute));
        var selializable = attribute != null ? (object)obj : Wrapper<E>.Wrap(obj);

        var json = JsonConvert.SerializeObject(selializable);
        var data = Encoding.ASCII.GetBytes(json);

        if (!DISABLE_COMPRESSION)
            data = data.Compress();

        return data;
    }

    /// <summary>
    /// Convert a byte array to an Object
    /// </summary>
    /// <typeparam name="E"></typeparam>
    /// <param name="data"></param>
    /// <returns></returns>
    public static E ToObject<E>(this byte[] data) where E : class
    {
        try
        {
            if (!DISABLE_COMPRESSION)
                data = data.Decompress();

            var json = Encoding.ASCII.GetString(data);

            if (json.IsJSON(out Wrapper<E> wrapper) && wrapper?.Values?.Any() == true)
                return wrapper.GetWrappedObject();

            if (json.IsJSON(out E e))
                return e;
        }
        catch (Exception e)
        {
            Console.WriteLine("Error while deserializing bytes to Object!", e);
        }

        return default(E);
    }

    /// <summary>
    /// Clone using obj.ToBytes().ToObject<E>()
    /// </summary>
    /// <typeparam name="E"></typeparam>
    /// <param name="obj"></param>
    /// <returns></returns>
    public static E Clone<E>(this E obj) where E : class => obj.ToBytes().ToObject<E>();

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="E"></typeparam>
    /// <param name="obj"></param>
    /// <returns></returns>
    public static string ToJson<E>(this E obj)
    {
        return JsonConvert.SerializeObject(obj);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="E"></typeparam>
    /// <param name="json"></param>
    /// <returns></returns>
    public static E ToObject<E>(this string json)
    {
        if (json.IsJSON(out E obj))
            return obj;

        return default;
    }

    /// <summary>
    /// Zip compress bytes
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    public static byte[] Compress(this byte[] data)
    {
        byte[] compressArray = null;
        try
        {
            using (var memoryStream = new MemoryStream())
            {
                using (var deflateStream = new DeflateStream(memoryStream, CompressionMode.Compress))
                {
                    deflateStream.Write(data, 0, data.Length);
                }
                compressArray = memoryStream.ToArray();
            }
        }
        catch (Exception e)
        {
            compressArray = data;
            Console.WriteLine("Error while running compression!", e);
        }
        return compressArray;
    }

    /// <summary>
    /// Unzip decompress bytes
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    public static byte[] Decompress(this byte[] data)
    {
        byte[] decompressedArray = null;
        try
        {
            using (MemoryStream decompressedStream = new MemoryStream())
            {
                using (MemoryStream compressStream = new MemoryStream(data))
                {
                    using (DeflateStream deflateStream = new DeflateStream(compressStream, CompressionMode.Decompress))
                    {
                        deflateStream.CopyTo(decompressedStream);
                    }
                }
                decompressedArray = decompressedStream.ToArray();
            }
        }
        catch (Exception e)
        {
            decompressedArray = data;
            Console.WriteLine("Error while running decompression!", e);
        }

        return decompressedArray;
    }

    /// <summary>
    /// Convert Object to Dictionary Key-Value pairs
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public static Dictionary<string, object> ToDictionary(this object obj)
    {
        if (obj == null) return null;

        var values = new Dictionary<string, object>();
        var properties = obj.GetType().GetRuntimeProperties();

        foreach (var p in properties)
        {
            try
            {
                if (p.CanRead)
                    values[p.Name] = p.GetValue(obj);
            }
            catch (Exception e)
            {
                Console.WriteLine("Error loading " + p.Name, e);
            }
        }

        return values;
    }

    /// <summary>
    /// Mapps Disctionary Key-Value to object type <typeparamref name="E"/>
    /// </summary>
    /// <typeparam name="E"></typeparam>
    /// <param name="values"></param>
    /// <returns></returns>
    public static E FromDictionary<E>(this IDictionary<string, object> values)
    {
        var obj = CreateInstance<E>();
        var properties = typeof(E).GetRuntimeProperties();

        foreach (var p in properties)
        {
            var attr = p.GetCustomAttribute<DataMemberAttribute>();
            var pnames = new[] { p.Name, attr?.Name }.Where(x => !string.IsNullOrEmpty(x));

            foreach (var pname in pnames)
                if (values.ContainsKey(pname))
                {
                    try
                    {
                        if (p.CanWrite)
                        {
                            p.SetValue(obj, values[pname]?.Cast(p.PropertyType));
                            continue;
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Error assinging " + pname + "=" + values[pname], e);
                    }
                }
        }

        return obj;
    }

    /// <summary>
    /// Create an instance to type <typeparamref name="E"/>
    /// </summary>
    /// <typeparam name="E"></typeparam>
    /// <returns></returns>
    private static E CreateInstance<E>()
    {
        return (E)CreateInstance(typeof(E)) ?? default;
    }

    /// <summary>
    /// Create an instance to type <paramref name="type"/>
    /// </summary>
    /// <param name="type"></param>
    /// <param name="parameters"></param>
    /// <returns></returns>
    public static object CreateInstance(this Type type, params object[] parameters)
    {
        var constructors = type.GetConstructors()
            .OrderBy(c => Math.Abs(c.GetParameters().Length - parameters.Length))
            .Take(3).ToList();

        foreach (var ctor in constructors)
        {
            if (ctor != null)
            {
                var args = ctor.GetParameters();
                try
                {
                    var _params = args
                        .Select((x, i) => parameters.ElementAtOrDefault(i) ?? (x.ParameterType != type ? CreateInstance(x.ParameterType) : null))
                        .ToArray();

                    return Activator.CreateInstance(type, _params);
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Error while creating object instance of type '{type.Name}', args:[{string.Join(",", args.Select(a => a.Name))}]!", e);
                    //Ignore exception
                }
            }
        }

        try
        {
            return Activator.CreateInstance(type);
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error while creating object instance of type '{type.Name}'!", e);
            //Ignore exception
        }

        return null;
    }

    /// <summary>
    /// Wraps any Object in a class with attribute decoration <see cref="SerializableAttribute"/>
    /// This allows serialization to bytes for any object
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [Serializable]
    public class Wrapper<T> where T : class
    {
        public Dictionary<string, object> Values { get; set; }

        internal T GetWrappedObject()
        {
            return Values.FromDictionary<T>();
        }

        internal static Wrapper<T> Wrap(T obj)
        {
            return new Wrapper<T>(obj);
        }

        private Wrapper(T obj)
        {
            Values = obj.ToDictionary();
        }

        public Wrapper()
        {
            Values = new Dictionary<string, object>();
        }
    }
}
