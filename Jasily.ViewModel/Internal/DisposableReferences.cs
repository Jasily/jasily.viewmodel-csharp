using System;
using System.Diagnostics;
using System.Threading;

namespace Jasily.ViewModel.Internal
{
    internal class DisposableReferences
    {
        private int _disposed; // -1 mean disposed.
        private readonly IDisposable _disposable;

        public DisposableReferences(IDisposable disposable)
        {
            this._disposable = disposable ?? throw new ArgumentNullException(nameof(disposable));
        }

        /// <summary>
        /// Dispose once.
        /// </summary>
        private void DisposeOnce()
        {
            while (true)
            {
                var disposed = _disposed;

                if (disposed == -1)
                    return; // disposed

                Debug.Assert(disposed > 0);

                var newDisposed = disposed - 1;
                if (newDisposed == 0) newDisposed = -1; // to dispose

                if (Interlocked.CompareExchange(ref _disposed, newDisposed, disposed) == disposed)
                {
                    if (newDisposed == -1) // the last reference is dispose.
                    {
                        Debug.Assert(_disposed == -1);
                        _disposable.Dispose();
                    }

                    return;
                }

                // continue
            }
        }

        public bool TryGetReference(out IDisposable disposable)
        {
            disposable = default;

            while (true)
            {
                var disposed = _disposed;

                if (disposed == -1)
                    return false; // disposed

                Debug.Assert(disposed >= 0);

                if (Interlocked.CompareExchange(ref _disposed, disposed + 1, disposed) == disposed)
                {
                    disposable = new ActionDisposable(() => this.DisposeOnce());
                    return true;
                }

                // continue
            }
        }
    }
}
