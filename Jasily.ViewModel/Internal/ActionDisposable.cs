using System;

namespace Jasily.ViewModel.Internal
{
    internal class ActionDisposable : Disposable
    {
        private readonly Action _action;

        public ActionDisposable(Action action) => _action = action ?? throw new ArgumentNullException(nameof(action));

        protected override void DisposeCore() => this._action();
    }
}
