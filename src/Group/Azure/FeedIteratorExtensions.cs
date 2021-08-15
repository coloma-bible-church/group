namespace Group.Azure
{
    using System;
    using System.Reactive.Linq;
    using Microsoft.Azure.Cosmos;

    public static class FeedIteratorExtensions
    {
        public static IObservable<T> AsObservable<T>(this FeedIterator<T> feedIterator) => Observable
            .Create<T>(async (observer, cancellationToken) =>
            {
                while (feedIterator.HasMoreResults)
                {
                    var response = await feedIterator.ReadNextAsync(cancellationToken);
                    foreach (var item in response)
                    {
                        observer.OnNext(item);
                    }
                }
            })
            .Publish()
            .RefCount();
    }
}