using System.Diagnostics.CodeAnalysis;

namespace IoT.Protocol;

public abstract class ConvertingEnumerator<TIn, TOut> : IAsyncEnumerable<TOut>
{
    private readonly IEqualityComparer<TIn> comparer;
    private readonly IAsyncEnumerable<TIn> enumerator;

    protected ConvertingEnumerator(IAsyncEnumerable<TIn> enumerator, IEqualityComparer<TIn> comparer)
    {
        ArgumentNullException.ThrowIfNull(enumerator);
        ArgumentNullException.ThrowIfNull(comparer);

        this.enumerator = enumerator;
        this.comparer = comparer;
    }

    #region Implementation of IAsyncEnumerable<out TThing2>

    public async IAsyncEnumerator<TOut> GetAsyncEnumerator(CancellationToken cancellationToken = default)
    {
        var set = new HashSet<TIn>(comparer);

        await foreach (var thing in enumerator.WithCancellation(cancellationToken).ConfigureAwait(false))
        {
            if (set.Add(thing)) yield return Convert(thing);
        }
    }

    #endregion

    protected abstract TOut Convert([NotNull] TIn thing);
}