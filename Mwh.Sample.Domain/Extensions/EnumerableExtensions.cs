
namespace Mwh.Sample.Domain.Extensions
{
    public static class EnumerableExtensions
    {

        public static T? WithMinium<T, TKey>(this IEnumerable<T> sequence, Func<T, TKey> keySelector)
            where T : class
            where TKey : IComparable<TKey>
        {
            if (!sequence?.Any() ?? default) return default;

            return sequence?
                .Select(obj => Tuple.Create(obj, keySelector(obj)))
                .Aggregate((Tuple<T, TKey>)null,
                (best, cur) => best == null || cur.Item2.CompareTo(best.Item2) < 0 ? cur : best).Item1
                ?? default;
        }

    }
}
