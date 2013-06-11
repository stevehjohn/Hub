using System;
using System.Threading;
using System.Threading.Tasks;

namespace MingPluginInterfaces
{
    public class CancelableTask
    {
        private readonly CancellationTokenSource _cancellationTokenSource;
        private readonly CancellationToken _cancellationToken;
        private readonly Action _task;
        private readonly Action _onSuccess;

        private bool _executed = false;

        public CancelableTask(Action task, Action onSuccess)
        {
            _cancellationTokenSource = new CancellationTokenSource();
            _cancellationToken = _cancellationTokenSource.Token;
            _task = task;
            _onSuccess = onSuccess;
        }

        public CancelableTask Execute()
        {
            if (_executed)
            {
                throw new ThreadStateException("Task already executed. Instantiate a new CancelableTask to re-execute.");
            }

            var task = Task.Factory.StartNew(_task, _cancellationToken)
                .ContinueWith(t => { if (_onSuccess != null) _onSuccess(); }, _cancellationToken, TaskContinuationOptions.OnlyOnRanToCompletion, TaskScheduler.FromCurrentSynchronizationContext());

            return this;
        }

        public void Cancel()
        {
            _cancellationTokenSource.Cancel();
        }
    }

    public class CancelableTask<TReturn>
    {
        private readonly CancellationTokenSource _cancellationTokenSource;
        private readonly CancellationToken _cancellationToken;
        private readonly Func<TReturn> _task;
        private readonly Action<TReturn> _onSuccess;

        private bool _executed = false;

        public CancelableTask(Func<TReturn> task, Action<TReturn> onSuccess)
        {
            _cancellationTokenSource = new CancellationTokenSource();
            _cancellationToken = _cancellationTokenSource.Token;
            _task = task;
            _onSuccess = onSuccess;
        }

        public CancelableTask<TReturn> Execute()
        {
            if (_executed)
            {
                throw new ThreadStateException("Task already executed. Instantiate a new CancelableTask to re-execute.");
            }

            var task = Task<TReturn>.Factory.StartNew(_task, _cancellationToken)
                .ContinueWith(t => { if (_onSuccess != null) _onSuccess(t.Result); }, _cancellationToken, TaskContinuationOptions.OnlyOnRanToCompletion, TaskScheduler.FromCurrentSynchronizationContext());

            return this;
        }

        public void Cancel()
        {
            _cancellationTokenSource.Cancel();
        }

        /*
        private void Complete(TReturn result)
        {
            _onSuccess(result);
        }
         */
    }
}
