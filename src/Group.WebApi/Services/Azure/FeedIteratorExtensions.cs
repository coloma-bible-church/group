namespace Group.WebApi.Services.Azure
{
    using System;
    using System.Collections.Generic;
    using System.Reactive.Linq;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using Microsoft.Azure.Cosmos;

    public static class FeedIteratorExtensions
    {
        public static async IAsyncEnumerable<T> ToAsyncEnumerable<T>(this FeedIterator<T> feedIterator, [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            while (feedIterator.HasMoreResults)
            {
                var response = await feedIterator.ReadNextAsync(cancellationToken);
                foreach (var item in response)
                    yield return item;
            }
        }

        public static IObservable<T> ToObservable<T>(this FeedIterator<T> feedIterator) => Observable
            .Create<T>(async (observer, cancellationToken) =>
            {
                using var _ = feedIterator;
                while (feedIterator.HasMoreResults && !cancellationToken.IsCancellationRequested)
                {
                    var response = await feedIterator.ReadNextAsync(cancellationToken);
                    foreach (var item in response)
                        observer.OnNext(item);
                }
                observer.OnCompleted();
            })
            .Publish()
            .RefCount();
    }
}