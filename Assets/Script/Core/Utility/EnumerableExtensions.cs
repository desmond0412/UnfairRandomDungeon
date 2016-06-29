using System;
using System.Collections.Generic;
using System.Linq;

public static class EnumerableExtensions
{
	public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> source)
	{
		if (source == null) throw new ArgumentNullException("source");		
		return source.ShuffleIterator();
	}
	
	private static IEnumerable<T> ShuffleIterator<T>(this IEnumerable<T> source)
	{
		var buffer = source.ToList();
		for (int i = 0; i < buffer.Count; i++)
		{
			int j = UnityEngine.Random.Range(i, buffer.Count);
			yield return buffer[j];
			
			buffer[j] = buffer[i];
		}
	}

    public static void ForEach<T>(this IEnumerable<T> source, Action<T> action)
    {
        if (source == null) throw new ArgumentNullException("source");
        foreach (T item in source)
        {
            action(item);
        }
    }

    /// <summary>
    /// Get random element from an Enumerable
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="source">Enumerable to be randomized</param>
    /// <returns>Returns random element or a default value if source length is 0.</returns>
    public static T Random<T>(this IEnumerable<T> source)
    {
        if (source == null) throw new ArgumentNullException("source");
        int count = source.Count();
        if (count == 0) return default(T);
        return source.ElementAtOrDefault(UnityEngine.Random.Range(0, count));
    }

    public static IEnumerable<T> Random<T>(this IEnumerable<T> source, int count)
    {
        if (source == null) throw new ArgumentNullException("source");
        return source.Shuffle().Take(count);
    }
}