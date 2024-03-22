using System.Collections;
using System.ComponentModel;

namespace Krialys.Common.Contracts;

/// <summary>
/// Ref. https://jacobcarpenter.wordpress.com/2008/03/13/dictionary-to-anonymous-type/
/// </summary>
public static class AnonymousTypeUtility
{
    /// <summary>
    /// Get key value from a Dictionary
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    /// <param name="dict"></param>
    /// <param name="key"></param>
    /// <returns></returns>
    public static TValue GetValueOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> dict, TKey key)
    {
        if (dict.TryGetValue(key, out TValue result))
            return result;

        return default(TValue);
    }

    /// <summary>
    /// Convert a Dictionary into an Anonymous type
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    /// <param name="dict"></param>
    /// <param name="anonymousType"></param>
    /// <returns></returns>
    public static T ToAnonymousType<T, TValue>(this IDictionary<string, TValue> dict, T anonymousType)
    {
        // get the sole constructor
        var ctor = anonymousType.GetType().GetConstructors().Single();

        // conveniently named constructor parameters make this all possible...
        var args = from p in ctor.GetParameters()
                   let val = dict.GetValueOrDefault(p.Name)
                   select val != null && p.ParameterType.IsAssignableFrom(val.GetType()) ? (object)val : null;

        return (T)ctor.Invoke(args.ToArray());
    }

    /// <summary>
    /// Convert an Anonymous type into a typed IDictionary
    /// </summary>
    /// <param name="withProperties"></param>
    /// <returns></returns>
    public static IDictionary<string, TValue> ToIDictionary<TValue>(this object withProperties)
    {
        IDictionary<string, TValue> dict = new Dictionary<string, TValue>();

        foreach (PropertyDescriptor property in System.ComponentModel.TypeDescriptor.GetProperties(withProperties))
            dict.Add(property.Name, (TValue)property.GetValue(withProperties));

        return dict;
    }

    /// <summary>
    /// Convert an Anonymous type into an IDictionary type
    /// </summary>
    /// <param name="withProperties"></param>
    /// <returns></returns>
    public static IDictionary ToIDictionary(this object withProperties)
    {
        IDictionary dict = new Dictionary<string, object>();

        foreach (PropertyDescriptor property in System.ComponentModel.TypeDescriptor.GetProperties(withProperties))
            dict.Add(property.Name, property.GetValue(withProperties));

        return dict;
    }
}

public static class TestAnonymousTypeUtility
{
    public static void Test()
    {
        // Case 1: dictionary to anonymous
        var person = new Dictionary<string, object>
        {
            { "Name", "Jacob" },
            { "Age", 26 },
            { "FavoriteColors", new[] { ConsoleColor.Blue, ConsoleColor.Green } }
        }.ToAnonymousType(new
        {
            //Name = default(string),
            Age = default(int),
            FavoriteColors = default(IEnumerable<ConsoleColor>),
            //Birthday = default(DateTime?),
        });

        Console.WriteLine(person);
        foreach (var color in person.FavoriteColors)
            Console.WriteLine(color);
        var json1 = System.Text.Json.JsonSerializer.Serialize(person);

        // Case 2: anonymous to dictionary
        var dictionary = new { Name = "Roy", Country = "Israel" }.ToIDictionary();
        var res = dictionary["Name"];
        var json2 = System.Text.Json.JsonSerializer.Serialize(dictionary);
    }

    private static object getPerson()
    {
        // Case 1: dictionary to anonymous
        return new Dictionary<string, object>
        {
            { "Name", "Jacob" },
            { "Age", 26 },
            { "FavoriteColors", new[] { ConsoleColor.Blue, ConsoleColor.Green } }
        }.ToAnonymousType(new
        {
            //Name = default(string),
            Age = default(int),
            FavoriteColors = default(IEnumerable<ConsoleColor>),
            //Birthday = default(DateTime?),
        });
    }
}