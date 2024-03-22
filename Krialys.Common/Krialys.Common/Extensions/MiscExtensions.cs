namespace Krialys.Common.Extensions;

public static class MiscExtensions
{
    /// <summary>
    /// Mimmics VB's With construction
    /// </summary>
    /// <typeparam name="T">this</typeparam>
    /// <param name="item"></param>
    /// <param name="action"></param>
    /// <returns></returns>
    public static T With<T>(this T item, Action<T> action)
    {
        action(item);

        return item;
    }
}
