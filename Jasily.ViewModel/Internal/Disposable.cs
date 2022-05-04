using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Jasily.ViewModel.Internal
{
    /// <summary>
    /// A base class for atomic call <see cref="IDisposable.Dispose"/>.
    /// </summary>
    internal abstract class Disposable : IDisposable
    {
        private int _isDisposed;

        protected bool IsDisposed => _isDisposed != 0;

        public void Dispose()
        {
            if (IsDisposed) return;
            if (Interlocked.CompareExchange(ref _isDisposed, 1, 0) != 0) return;
            DisposeCore();
        }

        protected abstract void DisposeCore();

        protected void ThrowIfDisposed()
        {
            if (this.IsDisposed) throw new ObjectDisposedException(this.ToString());
        }
    }
}
