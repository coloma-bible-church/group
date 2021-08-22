namespace Group.Hub.Services.Azure
{
    using System.Collections.Generic;
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
    }
}