using System;

public class EnumHelper
{
    /// <summary>
    /// Returns a random enum from the specified enum type
    /// </summary>
    /// <typeparam name="T">The enum</typeparam>
    public static T GetRandomEnum<T>()
    {
        Array values = Enum.GetValues(typeof(T));
        return (T)values.GetValue(UnityEngine.Random.Range(0, values.Length));
    }
}