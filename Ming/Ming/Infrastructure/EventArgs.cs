using System;

namespace Ming.Infrastructure
{
    internal class EventArgs<TResult> : EventArgs
    {
        private readonly TResult _result;

        public EventArgs(TResult result)
        {
            _result = result;
        }

        public TResult Result
        {
            get { return _result; }
        }
    }
}
