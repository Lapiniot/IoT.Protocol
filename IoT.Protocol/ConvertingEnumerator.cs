namespace IoT.Protocol;

public abstract class ConvertingEnumerator<TThing1, TThing2> : IAsyncEnumerable<TThing2>
{
    private readonly IEqualityComparer<TThing1> comparer;
    private readonly IAsyncEnumerable<TThing1> enumerator;

    protected ConvertingEnumerator(IAsyncEnumerable<TThing1> enumerator, IEqualityComparer<TThing1> comparer)
    {
        ArgumentNullException.ThrowIfNull(enumerator);
        ArgumentNullException.ThrowIfNull(comparer);

        this.enumerator = enumerator;
        this.comparer = comparer;
    }

    #region Implementation of IAsyncEnumerable<out TThing2>

    public async IAsyncEnumerator<TThing2> GetAsyncEnumerator(CancellationToken cancellationToken = default)
    {
        var set = new HashSet<TThing1>(comparer);

        await foreach(var thing in enumerator.WithCancellation(cancellationToken).ConfigureAwait(false))
        {
            if(set.Add(thing)) yield return Convert(thing);
        }
    }

    #endregion

    protected abstract TThing2 Convert(TThing1 thing);
}