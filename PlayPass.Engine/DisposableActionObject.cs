using System;

namespace PlayPass.Engine
{
    public class DisposableActionObject : IDisposable
    {
        public delegate void DisposeAction();
        private DisposeAction _disposeAction;

        public DisposableActionObject(DisposeAction disposeAction)
        {
            _disposeAction = disposeAction;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposeAction == null) return;
            if (disposing) _disposeAction();
            _disposeAction = null;
        }
    }
}
