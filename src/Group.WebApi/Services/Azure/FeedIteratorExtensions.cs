namespace Group.WebApi.Services.Azure
{
    using System;
    using System.Reactive.Linq;
    using Microsoft.Azure.Cosmos;

    public static class FeedIteratorExtensions
    {
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