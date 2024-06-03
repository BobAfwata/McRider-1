using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;


namespace McRider.Domain;
public class ConcurrentList<T> : IList<T>
{
    private readonly List<T> _list = new List<T>();
    private readonly object _syncRoot = new object();

    public int Count
    {
        get
        {
            lock (_syncRoot)
            {
                return _list.Count;
            }
        }
    }

    public bool IsReadOnly => false;

    public T this[int index]
    {
        get
        {
            lock (_syncRoot)
            {
                return _list[index];
            }
        }
        set
        {
            lock (_syncRoot)
            {
                _list[index] = value;
            }
        }
    }

    public void Add(T item)
    {
        lock (_syncRoot)
        {
            _list.Add(item);
        }
    }

    public void Clear()
    {
        lock (_syncRoot)
        {
            _list.Clear();
        }
    }

    public bool Contains(T item)
    {
        lock (_syncRoot)
        {
            return _list.Contains(item);
        }
    }

    public void CopyTo(T[] array, int arrayIndex)
    {
        lock (_syncRoot)
        {
            _list.CopyTo(array, arrayIndex);
        }
    }

    public IEnumerator<T> GetEnumerator()
    {
        lock (_syncRoot)
        {
            return _list.GetEnumerator();
        }
    }

    public int IndexOf(T item)
    {
        lock (_syncRoot)
        {
            return _list.IndexOf(item);
        }
    }

    public int IndexOf(Func<T, bool> compare)
    {
        lock (_syncRoot)
        {
            for(var i = 0; i < _list.Count; i++)
            {
                if (compare?.Invoke(_list[i]) == true)
                    return i;
            }
        }

        return -1;
    }

    public void Insert(int index, T item)
    {
        lock (_syncRoot)
        {
            _list.Insert(index, item);
        }
    }

    public bool Remove(T item)
    {
        lock (_syncRoot)
        {
            return _list.Remove(item);
        }
    }

    public void RemoveAt(int index)
    {
        lock (_syncRoot)
        {
            _list.RemoveAt(index);
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        lock (_syncRoot)
        {
            return ((IEnumerable)_list).GetEnumerator();
        }
    }

    // Implicit conversion from List<T> to ConcurrentList<T>
    public static implicit operator ConcurrentList<T>(List<T> list)
    {
        var concurrentList = new ConcurrentList<T>();
        concurrentList.AssignFrom(list);
        return concurrentList;
    }

    public void AddRange(IEnumerable<T> collection)
    {
        lock (_syncRoot)
        {
            _list.AddRange(collection);
        }
    }

    // Allows assignment from a List<T>
    public void AssignFrom(List<T> list)
    {
        lock (_syncRoot)
        {
            _list.Clear();
            _list.AddRange(list);
        }
    }

    public void ForEach(Action<T> action)
    {
        lock (_syncRoot)
        {
            foreach (var item in _list)
            {
                action(item);
            }
        }
    }
}
