using System;

namespace PlayPass.Engine
{
    public class DisposableActionObject : IDisposable
    {
        public delegate void DisposeAction();
        private readonly DisposeAction _disposeAction;

        public DisposableActionObject(DisposeAction disposeAction)
        {
            _disposeAction = disposeAction;
        }

        public void Dispose()
        {
            _disposeAction();
        }
    }
}
