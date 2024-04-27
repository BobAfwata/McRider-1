using System.Collections;
using System.Collections.ObjectModel;

namespace McRider.Common.Extensions;

public static class EnumerableExtensions
{
    public static void Sort<T, E>(this ObservableCollection<T> collection, Func<T, E> compare)
    {
        var sortedList = collection.OrderBy(compare).ToList();

        lock (collection)
            for (int i = 0; i < sortedList.Count; i++)
                collection[i] = sortedList[i];
    }

    public static void SortDescending<T, E>(this ObservableCollection<T> collection, Func<T, E> compare)
    {
        var sortedList = collection.OrderByDescending(compare).ToList();

        lock (collection)
            for (int i = 0; i < sortedList.Count; i++)
                collection[i] = sortedList[i];
    }

    public static int Count(this IEnumerable enumerable)
    {
        if (enumerable is IList list)
            return list.Count;

        var count = 0;
        foreach (var x in enumerable)
            count++;

        return count;
    }

    public static bool Any(this IEnumerable enumerable, Func<object, bool>? func = null)
    {
        foreach (var x in enumerable)
            if (func == null || func.Invoke(x)) return true;

        return false;
    }

    public static bool ContainsAny<T>(this IEnumerable<T> @this, params T[] check)
    {
        return @this.Any(check.Contains);
    }

    public static T? Max<T>(this IEnumerable enumerable, Func<object, T> func)
    {
        if (!enumerable.Any()) return default(T);

        T? max = default(T);
        foreach (var x in enumerable)
        {
            var obj = func.Invoke(x);
            if (max == null && obj != null)
            {
                max = obj;
                continue;
            }

            if (obj is IComparable comp && comp?.CompareTo(max) > 1)
                max = obj;
        }

        return max;
    }

    public static T Min<T>(this IEnumerable enumerable, Func<object, T> func)
    {
        if (!enumerable.Any()) return default(T);

        T min = func.Invoke(enumerable.FirstOrNull());

        foreach (var x in enumerable)
        {
            var obj = func.Invoke(x);
            if (min == null && obj != null)
            {
                min = obj;
                continue;
            }

            if (obj is IComparable comp && comp?.CompareTo(min) < 1)
                min = obj;
        }

        return min;
    }

    public static object? ElementAtOrNull(this IEnumerable enumerable, int index)
    {
        if (enumerable is IList list && list?.Count < index)
            return list[index];

        var i = 0;
        foreach (var n in enumerable)
            if (i++ == index) return n;

        return null;
    }

    public static object FirstOrNull(this IEnumerable enumerable, Func<object, bool>? func = null)
    {
        foreach (var x in enumerable)
            if (func == null || func.Invoke(x)) return x;

        return null;
    }

    public static IEnumerable<T> Randomize<T>(this IEnumerable<T> enumerable)
    {
        var list = enumerable.ToList();
        int count = list.Count, last = count - 1;

        for (var i = 0; i < last; ++i)
        {
            var r = ThreadSafe.Random.Next(i, last);
            var tmp = list[i];
            list[i] = list[r];
            list[r] = tmp;
        }

        return list;
    }

    public static IEnumerable<T> TakeRandomize<T>(this IEnumerable<T> enumerable, int count)
    {
        var list = enumerable.ToList();
        int last = Math.Min(count, list.Count) - 1;

        for (var i = 0; i < last; ++i)
        {
            var r = ThreadSafe.Random.Next(i, last);
            var tmp = list[i];
            list[i] = list[r];
            list[r] = tmp;
        }

        return list.Take(count);
    }

    public static T FirstRandom<T>(this IEnumerable<T> enumerable)
        => enumerable.ElementAtOrDefault(ThreadSafe.Random.Next(enumerable.Count()));

    public static int IndexOf<T>(this IEnumerable<T> source, T obj)
    {
        var count = source.Count();

        for (var i = 0; i < count; i++)
        {
            var val = source.ElementAt(i);
            if (val != null && val.Equals(obj))
                return i;
            if (obj != null && obj.Equals(val))
                return i;
        }

        return -1;
    }

    public static int IndexOf<T>(this IEnumerable<T> source, Func<T, bool> compare)
    {
        var count = source?.Count();

        for (var i = 0; i < count; i++)
        {
            var val = source.ElementAt(i);
            if (compare(val))
                return i;
        }

        return -1;
    }

    public static IEnumerable<T> DistinctBy<T>(this IEnumerable<T> source, Func<T, object> compare)
    {
        return source?.GroupBy(compare).Select(g => g.First());
    }

    public static I Merge<I, T>(this I source, IEnumerable<T> fetched, int page, int pageSize) where I : IList<T>
    {
        var offsetIndex = (page - 1) * pageSize;
        return source.Merge(fetched, offsetIndex);
    }

    public static I Merge<I, T>(this I source, IEnumerable<T> fetched, int offsetIndex = 0) where I : IList<T>
    {
        var count = source.Count;

        for (var i = 0; i < fetched.Count(); i++)
        {
            var item = fetched.ElementAt(i);
            try
            {
                if (i + offsetIndex >= count)
                    source.Add(item);
                else
                    source[i + offsetIndex] = item;
            }
            catch (Exception ex)
            {
                //Log.Error($"Error while updating list.. {i}, count={count}", ex);
            }
        }

        return source;
    }
}

public static class ThreadSafe
{
    [ThreadStatic] private static Random? _local;

    public static Random Random
    {
        get
        {
            return _local ??= new Random(unchecked(Environment.TickCount * 31 + Thread.CurrentThread.ManagedThreadId));
        }
    }
}
