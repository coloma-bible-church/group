namespace Group
{
    using System;

    public sealed class Owned<T> : IDisposable
    {
        readonly IDisposable _disposable;

        public Owned(IDisposable disposable, T value)
        {
            _disposable = disposable;
            Value = value;
        }

        public T Value { get; }

        public void Dispose()
        {
            _disposable.Dispose();
        }
    }
}