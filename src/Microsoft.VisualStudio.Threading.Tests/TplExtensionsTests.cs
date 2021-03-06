﻿namespace Microsoft.VisualStudio.Threading.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Xunit;
    using Xunit.Abstractions;

    public class TplExtensionsTests : TestBase
    {
        public TplExtensionsTests(ITestOutputHelper logger)
            : base(logger)
        {
        }

        [Fact]
        public void CompletedTask()
        {
            Assert.True(TplExtensions.CompletedTask.IsCompleted);
        }

        [Fact]
        public void AppendActionTest()
        {
            var evt = new ManualResetEventSlim();
            Action a = () => evt.Set();
            var cts = new CancellationTokenSource();
            var result = TplExtensions.CompletedTask.AppendAction(a, TaskContinuationOptions.DenyChildAttach, cts.Token);
            Assert.NotNull(result);
            Assert.Equal(TaskContinuationOptions.DenyChildAttach, (TaskContinuationOptions)result.CreationOptions);
            Assert.True(evt.Wait(TestTimeout));
        }

        [Fact]
        public void ApplyResultToNullTask()
        {
            Assert.Throws<ArgumentNullException>(() => TplExtensions.ApplyResultTo(null, new TaskCompletionSource<object>()));
        }

        [Fact]
        public void ApplyResultToNullTaskSource()
        {
            var tcs = new TaskCompletionSource<object>();
            Assert.Throws<ArgumentNullException>(() => TplExtensions.ApplyResultTo(tcs.Task, null));
        }

        [Fact]
        public void ApplyResultTo()
        {
            var tcs1 = new TaskCompletionSource<GenericParameterHelper>();
            var tcs2 = new TaskCompletionSource<GenericParameterHelper>();
            tcs1.Task.ApplyResultTo(tcs2);
            tcs1.SetResult(new GenericParameterHelper(2));
            Assert.Equal(2, tcs2.Task.Result.Data);

            tcs1 = new TaskCompletionSource<GenericParameterHelper>();
            tcs2 = new TaskCompletionSource<GenericParameterHelper>();
            tcs1.Task.ApplyResultTo(tcs2);
            tcs1.SetCanceled();
            Assert.True(tcs2.Task.IsCanceled);

            tcs1 = new TaskCompletionSource<GenericParameterHelper>();
            tcs2 = new TaskCompletionSource<GenericParameterHelper>();
            tcs1.Task.ApplyResultTo(tcs2);
            tcs1.SetException(new ApplicationException());
            Assert.Same(tcs1.Task.Exception.InnerException, tcs2.Task.Exception.InnerException);
        }

        [Fact]
        public void ApplyResultToPreCompleted()
        {
            var tcs1 = new TaskCompletionSource<GenericParameterHelper>();
            var tcs2 = new TaskCompletionSource<GenericParameterHelper>();
            tcs1.SetResult(new GenericParameterHelper(2));
            tcs1.Task.ApplyResultTo(tcs2);
            Assert.Equal(2, tcs2.Task.Result.Data);

            tcs1 = new TaskCompletionSource<GenericParameterHelper>();
            tcs2 = new TaskCompletionSource<GenericParameterHelper>();
            tcs1.SetCanceled();
            tcs1.Task.ApplyResultTo(tcs2);
            Assert.True(tcs2.Task.IsCanceled);

            tcs1 = new TaskCompletionSource<GenericParameterHelper>();
            tcs2 = new TaskCompletionSource<GenericParameterHelper>();
            tcs1.SetException(new ApplicationException());
            tcs1.Task.ApplyResultTo(tcs2);
            Assert.Same(tcs1.Task.Exception.InnerException, tcs2.Task.Exception.InnerException);
        }

        [Fact]
        public void ApplyResultToNullTaskNonGeneric()
        {
            Assert.Throws<ArgumentNullException>(() => TplExtensions.ApplyResultTo((Task)null, new TaskCompletionSource<object>()));
        }

        [Fact]
        public void ApplyResultToNullTaskSourceNonGeneric()
        {
            var tcs = new TaskCompletionSource<object>();
            Assert.Throws<ArgumentNullException>(() => TplExtensions.ApplyResultTo((Task)tcs.Task, (TaskCompletionSource<object>)null));
        }

        [Fact]
        public void ApplyResultToNonGeneric()
        {
            var tcs1 = new TaskCompletionSource<GenericParameterHelper>();
            var tcs2 = new TaskCompletionSource<GenericParameterHelper>();
            ((Task)tcs1.Task).ApplyResultTo(tcs2);
            tcs1.SetResult(null);
            Assert.Equal(TaskStatus.RanToCompletion, tcs2.Task.Status);

            tcs1 = new TaskCompletionSource<GenericParameterHelper>();
            tcs2 = new TaskCompletionSource<GenericParameterHelper>();
            ((Task)tcs1.Task).ApplyResultTo(tcs2);
            tcs1.SetCanceled();
            Assert.True(tcs2.Task.IsCanceled);

            tcs1 = new TaskCompletionSource<GenericParameterHelper>();
            tcs2 = new TaskCompletionSource<GenericParameterHelper>();
            ((Task)tcs1.Task).ApplyResultTo(tcs2);
            tcs1.SetException(new ApplicationException());
            Assert.Same(tcs1.Task.Exception.InnerException, tcs2.Task.Exception.InnerException);
        }

        [Fact]
        public void ApplyResultToPreCompletedNonGeneric()
        {
            var tcs1 = new TaskCompletionSource<GenericParameterHelper>();
            var tcs2 = new TaskCompletionSource<GenericParameterHelper>();
            tcs1.SetResult(null);
            ((Task)tcs1.Task).ApplyResultTo(tcs2);
            Assert.Equal(TaskStatus.RanToCompletion, tcs2.Task.Status);

            tcs1 = new TaskCompletionSource<GenericParameterHelper>();
            tcs2 = new TaskCompletionSource<GenericParameterHelper>();
            tcs1.SetCanceled();
            ((Task)tcs1.Task).ApplyResultTo(tcs2);
            Assert.True(tcs2.Task.IsCanceled);

            tcs1 = new TaskCompletionSource<GenericParameterHelper>();
            tcs2 = new TaskCompletionSource<GenericParameterHelper>();
            tcs1.SetException(new ApplicationException());
            ((Task)tcs1.Task).ApplyResultTo(tcs2);
            Assert.Same(tcs1.Task.Exception.InnerException, tcs2.Task.Exception.InnerException);
        }

        [Fact]
        public void WaitWithoutInlining()
        {
            var originalThread = Thread.CurrentThread;
            var task = Task.Run(delegate
            {
                Assert.NotSame(originalThread, Thread.CurrentThread);
            });
            task.WaitWithoutInlining();
        }

        [Fact]
        public async Task NoThrowAwaitable()
        {
            var tcs = new TaskCompletionSource<object>();
            var nothrowTask = tcs.Task.NoThrowAwaitable();
            Assert.False(nothrowTask.GetAwaiter().IsCompleted);
            tcs.SetException(new InvalidOperationException());
            await nothrowTask;

            tcs = new TaskCompletionSource<object>();
            nothrowTask = tcs.Task.NoThrowAwaitable();
            Assert.False(nothrowTask.GetAwaiter().IsCompleted);
            tcs.SetCanceled();
            await nothrowTask;
        }

        [Fact]
        public void InvokeAsyncNullEverything()
        {
            AsyncEventHandler handler = null;
            var task = handler.InvokeAsync(null, null);
            Assert.True(task.IsCompleted);
        }

        [Fact]
        public void InvokeAsyncParametersCarry()
        {
            InvokeAsyncHelper(null, null);
            InvokeAsyncHelper(new object(), new EventArgs());
        }

        [Fact]
        public void InvokeAsyncOfTNullEverything()
        {
            AsyncEventHandler<EventArgs> handler = null;
            var task = handler.InvokeAsync(null, null);
            Assert.True(task.IsCompleted);
        }

        [Fact]
        public void InvokeAsyncOfTParametersCarry()
        {
            InvokeAsyncOfTHelper(null, null);
            InvokeAsyncHelper(new object(), new EventArgs());
        }

        [Fact]
        public void InvokeAsyncExecutesEachHandlerSequentially()
        {
            AsyncEventHandler handlers = null;
            int counter = 0;
            handlers += async (sender, args) =>
            {
                Assert.Equal(1, ++counter);
                await Task.Yield();
                Assert.Equal(2, ++counter);
            };
            handlers += async (sender, args) =>
            {
                Assert.Equal(3, ++counter);
                await Task.Yield();
                Assert.Equal(4, ++counter);
            };
            var task = handlers.InvokeAsync(null, null);
            task.GetAwaiter().GetResult();
        }

        [Fact]
        public void InvokeAsyncOfTExecutesEachHandlerSequentially()
        {
            AsyncEventHandler<EventArgs> handlers = null;
            int counter = 0;
            handlers += async (sender, args) =>
            {
                Assert.Equal(1, ++counter);
                await Task.Yield();
                Assert.Equal(2, ++counter);
            };
            handlers += async (sender, args) =>
            {
                Assert.Equal(3, ++counter);
                await Task.Yield();
                Assert.Equal(4, ++counter);
            };
            var task = handlers.InvokeAsync(null, null);
            task.GetAwaiter().GetResult();
        }

        [Fact]
        public void InvokeAsyncAggregatesExceptions()
        {
            AsyncEventHandler handlers = null;
            handlers += (sender, args) =>
            {
                throw new ApplicationException("a");
            };
            handlers += async (sender, args) =>
            {
                await Task.Yield();
                throw new ApplicationException("b");
            };
            var task = handlers.InvokeAsync(null, null);
            try
            {
                task.GetAwaiter().GetResult();
                Assert.True(false, "Expected AggregateException not thrown.");
            }
            catch (AggregateException ex)
            {
                Assert.Equal(2, ex.InnerExceptions.Count);
                Assert.Equal("a", ex.InnerExceptions[0].Message);
                Assert.Equal("b", ex.InnerExceptions[1].Message);
            }
        }

        [Fact]
        public void InvokeAsyncOfTAggregatesExceptions()
        {
            AsyncEventHandler<EventArgs> handlers = null;
            handlers += (sender, args) =>
            {
                throw new ApplicationException("a");
            };
            handlers += async (sender, args) =>
            {
                await Task.Yield();
                throw new ApplicationException("b");
            };
            var task = handlers.InvokeAsync(null, null);
            try
            {
                task.GetAwaiter().GetResult();
                Assert.True(false, "Expected AggregateException not thrown.");
            }
            catch (AggregateException ex)
            {
                Assert.Equal(2, ex.InnerExceptions.Count);
                Assert.Equal("a", ex.InnerExceptions[0].Message);
                Assert.Equal("b", ex.InnerExceptions[1].Message);
            }
        }

        [Fact]
        public void FollowCancelableTaskToCompletionEndsInCompletion()
        {
            var currentTCS = new TaskCompletionSource<int>();
            Task<int> latestTask = currentTCS.Task;
            var followingTask = TplExtensions.FollowCancelableTaskToCompletion(() => latestTask, CancellationToken.None);

            for (int i = 0; i < 3; i++)
            {
                var oldTCS = currentTCS;
                currentTCS = new TaskCompletionSource<int>();
                latestTask = currentTCS.Task;
                oldTCS.SetCanceled();
            }

            currentTCS.SetResult(3);
            Assert.Equal(3, followingTask.Result);
        }

        [Fact]
        public void FollowCancelableTaskToCompletionEndsInCompletionWithSpecifiedTaskSource()
        {
            var specifiedTaskSource = new TaskCompletionSource<int>();
            var currentTCS = new TaskCompletionSource<int>();
            Task<int> latestTask = currentTCS.Task;
            var followingTask = TplExtensions.FollowCancelableTaskToCompletion(() => latestTask, CancellationToken.None, specifiedTaskSource);
            Assert.Same(specifiedTaskSource.Task, followingTask);

            for (int i = 0; i < 3; i++)
            {
                var oldTCS = currentTCS;
                currentTCS = new TaskCompletionSource<int>();
                latestTask = currentTCS.Task;
                oldTCS.SetCanceled();
            }

            currentTCS.SetResult(3);
            Assert.Equal(3, followingTask.Result);
        }

        [Fact]
        public void FollowCancelableTaskToCompletionEndsInUltimateCancellation()
        {
            var currentTCS = new TaskCompletionSource<int>();
            Task<int> latestTask = currentTCS.Task;
            var cts = new CancellationTokenSource();
            var followingTask = TplExtensions.FollowCancelableTaskToCompletion(() => latestTask, cts.Token);

            for (int i = 0; i < 3; i++)
            {
                var oldTCS = currentTCS;
                currentTCS = new TaskCompletionSource<int>();
                latestTask = currentTCS.Task;
                oldTCS.SetCanceled();
            }

            cts.Cancel();
            Assert.True(followingTask.IsCanceled);
        }

        [Fact]
        public void FollowCancelableTaskToCompletionEndsInFault()
        {
            var currentTCS = new TaskCompletionSource<int>();
            Task<int> latestTask = currentTCS.Task;
            var followingTask = TplExtensions.FollowCancelableTaskToCompletion(() => latestTask, CancellationToken.None);

            for (int i = 0; i < 3; i++)
            {
                var oldTCS = currentTCS;
                currentTCS = new TaskCompletionSource<int>();
                latestTask = currentTCS.Task;
                oldTCS.SetCanceled();
            }

            currentTCS.SetException(new InvalidOperationException());
            Assert.IsType(typeof(InvalidOperationException), followingTask.Exception.InnerException);
        }

        [Fact]
        public async Task ToApmOfTWithNoTaskState()
        {
            var state = new object();
            var tcs = new TaskCompletionSource<int>();
            IAsyncResult beginResult = null;

            var callbackResult = new TaskCompletionSource<object>();
            AsyncCallback callback = ar =>
            {
                try
                {
                    Assert.Same(beginResult, ar);
                    Assert.Equal(5, EndTestOperation<int>(ar));
                    callbackResult.SetResult(null);
                }
                catch (Exception ex)
                {
                    callbackResult.SetException(ex);
                }
            };
            beginResult = BeginTestOperation(callback, state, tcs.Task);
            Assert.Same(state, beginResult.AsyncState);
            tcs.SetResult(5);
            await callbackResult.Task;
        }

        [Fact]
        public async Task ToApmOfTWithMatchingTaskState()
        {
            var state = new object();
            var tcs = new TaskCompletionSource<int>(state);
            IAsyncResult beginResult = null;

            var callbackResult = new TaskCompletionSource<object>();
            AsyncCallback callback = ar =>
            {
                try
                {
                    Assert.Same(beginResult, ar);
                    Assert.Equal(5, EndTestOperation<int>(ar));
                    callbackResult.SetResult(null);
                }
                catch (Exception ex)
                {
                    callbackResult.SetException(ex);
                }
            };
            beginResult = BeginTestOperation(callback, state, tcs.Task);
            Assert.Same(state, beginResult.AsyncState);
            tcs.SetResult(5);
            await callbackResult.Task;
        }

        [Fact]
        public async Task ToApmWithNoTaskState()
        {
            var state = new object();
            var tcs = new TaskCompletionSource<object>();
            IAsyncResult beginResult = null;

            var callbackResult = new TaskCompletionSource<object>();
            AsyncCallback callback = ar =>
            {
                try
                {
                    Assert.Same(beginResult, ar);
                    EndTestOperation(ar);
                    callbackResult.SetResult(null);
                }
                catch (Exception ex)
                {
                    callbackResult.SetException(ex);
                }
            };
            beginResult = BeginTestOperation(callback, state, (Task)tcs.Task);
            Assert.Same(state, beginResult.AsyncState);
            tcs.SetResult(null);
            await callbackResult.Task;
        }

        [Fact]
        public async Task ToApmWithMatchingTaskState()
        {
            var state = new object();
            var tcs = new TaskCompletionSource<object>(state);
            IAsyncResult beginResult = null;

            var callbackResult = new TaskCompletionSource<object>();
            AsyncCallback callback = ar =>
            {
                try
                {
                    Assert.Same(beginResult, ar);
                    EndTestOperation(ar);
                    callbackResult.SetResult(null);
                }
                catch (Exception ex)
                {
                    callbackResult.SetException(ex);
                }
            };
            beginResult = BeginTestOperation(callback, state, (Task)tcs.Task);
            Assert.Same(state, beginResult.AsyncState);
            tcs.SetResult(null);
            await callbackResult.Task;
        }

#if NET452

        [Fact]
        public void ToTaskReturnsCompletedTaskPreSignaled()
        {
            var handle = new ManualResetEvent(initialState: true);
            Task<bool> actual = TplExtensions.ToTask(handle);
            Assert.Same(TplExtensions.TrueTask, actual);
        }

        [Fact]
        public async Task ToTaskOnHandleSignaledLater()
        {
            var handle = new ManualResetEvent(initialState: false);
            Task<bool> actual = TplExtensions.ToTask(handle);
            Assert.False(actual.IsCompleted);
            handle.Set();
            bool result = await actual;
            Assert.True(result);
        }

        [Fact]
        public void ToTaskUnsignaledHandleWithZeroTimeout()
        {
            var handle = new ManualResetEvent(initialState: false);
            Task<bool> actual = TplExtensions.ToTask(handle, timeout: 0);
            Assert.Same(TplExtensions.FalseTask, actual);
        }

        [Fact]
        public void ToTaskSignaledHandleWithZeroTimeout()
        {
            var handle = new ManualResetEvent(initialState: true);
            Task<bool> actual = TplExtensions.ToTask(handle, timeout: 0);
            Assert.Same(TplExtensions.TrueTask, actual);
        }

        [Fact]
        public async Task ToTaskOnHandleSignaledAfterNonZeroTimeout()
        {
            var handle = new ManualResetEvent(initialState: false);
            Task<bool> actual = TplExtensions.ToTask(handle, timeout: 1);
            await Task.Delay(2);
            handle.Set();
            bool result = await actual;
            Assert.False(result);
        }

        [Fact]
        public void ToTaskOnHandleSignaledAfterCancellation()
        {
            var handle = new ManualResetEvent(initialState: false);
            var cts = new CancellationTokenSource();
            Task<bool> actual = TplExtensions.ToTask(handle, cancellationToken: cts.Token);
            cts.Cancel();
            Assert.True(actual.IsCanceled);
            handle.Set();
        }

        [Fact]
        public async Task ToTaskOnDisposedHandle()
        {
            var handle = new ManualResetEvent(false);
            handle.Dispose();
            await Assert.ThrowsAsync<ObjectDisposedException>(() => TplExtensions.ToTask(handle));
        }

#endif

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void WithTimeout_NullTask(bool generic)
        {
            // Verify that a faulted task is returned instead of throwing.
            Task timeoutTask = generic
                ? TplExtensions.WithTimeout<int>(null, TimeSpan.FromSeconds(1))
                : TplExtensions.WithTimeout(null, TimeSpan.FromSeconds(1));
            Assert.Throws<ArgumentNullException>(() => timeoutTask.GetAwaiter().GetResult());
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void WithTimeout_MinusOneMeansInfiniteTimeout(bool generic)
        {
            this.ExecuteOnDispatcher(async delegate
            {
                var tcs = new TaskCompletionSource<object>();
                var timeoutTask = generic
                    ? TplExtensions.WithTimeout<object>(tcs.Task, TimeSpan.FromMilliseconds(-1))
                    : TplExtensions.WithTimeout((Task)tcs.Task, TimeSpan.FromMilliseconds(-1));
                Assert.False(timeoutTask.IsCompleted);
                await Task.Delay(AsyncDelay / 2);
                Assert.False(timeoutTask.IsCompleted);
                tcs.SetResult(null);
                timeoutTask.GetAwaiter().GetResult();
            });
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void WithTimeout_TimesOut(bool generic)
        {
            // Use a SynchronizationContext to ensure that we never deadlock even when synchronously blocking.
            this.ExecuteOnDispatcher(delegate
            {
                var tcs = new TaskCompletionSource<object>();
                Task timeoutTask = generic
                    ? tcs.Task.WithTimeout(TimeSpan.FromMilliseconds(1))
                    : ((Task)tcs.Task).WithTimeout(TimeSpan.FromMilliseconds(1));
                Assert.Throws<TimeoutException>(() => timeoutTask.GetAwaiter().GetResult()); // sync block to ensure no deadlock occurs
            });
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void WithTimeout_CompletesFirst(bool generic)
        {
            // Use a SynchronizationContext to ensure that we never deadlock even when synchronously blocking.
            this.ExecuteOnDispatcher(delegate
            {
                var tcs = new TaskCompletionSource<object>();
                Task timeoutTask = generic
                    ? tcs.Task.WithTimeout(TimeSpan.FromDays(1))
                    : ((Task)tcs.Task).WithTimeout(TimeSpan.FromDays(1));
                Assert.False(timeoutTask.IsCompleted);
                tcs.SetResult(null);
                timeoutTask.GetAwaiter().GetResult();
            });
        }

        [Fact]
        public void WithTimeout_CompletesFirstWithResult()
        {
            // Use a SynchronizationContext to ensure that we never deadlock even when synchronously blocking.
            this.ExecuteOnDispatcher(delegate
            {
                var tcs = new TaskCompletionSource<object>();
                var timeoutTask = tcs.Task.WithTimeout(TimeSpan.FromDays(1));
                Assert.False(timeoutTask.IsCompleted);
                tcs.SetResult("success");
                Assert.Same(tcs.Task.Result, timeoutTask.Result);
            });
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void WithTimeout_CompletesFirstAndThrows(bool generic)
        {
            // Use a SynchronizationContext to ensure that we never deadlock even when synchronously blocking.
            this.ExecuteOnDispatcher(async delegate
            {
                var tcs = new TaskCompletionSource<object>();
                Task timeoutTask = generic
                    ? tcs.Task.WithTimeout(TimeSpan.FromDays(1))
                    : ((Task)tcs.Task).WithTimeout(TimeSpan.FromDays(1));
                Assert.False(timeoutTask.IsCompleted);
                tcs.SetException(new ApplicationException());
                await Assert.ThrowsAsync<ApplicationException>(() => timeoutTask);
                Assert.Same(tcs.Task.Exception.InnerException, timeoutTask.Exception.InnerException);
            });
        }

        private static void InvokeAsyncHelper(object sender, EventArgs args)
        {
            int invoked = 0;
            AsyncEventHandler handler = (s, a) =>
            {
                Assert.Same(sender, s);
                Assert.Same(args, a);
                invoked++;
                return TplExtensions.CompletedTask;
            };
            var task = handler.InvokeAsync(sender, args);
            Assert.True(task.IsCompleted);
            Assert.Equal(1, invoked);
        }

        private static void InvokeAsyncOfTHelper(object sender, EventArgs args)
        {
            int invoked = 0;
            AsyncEventHandler<EventArgs> handler = (s, a) =>
            {
                Assert.Same(sender, s);
                Assert.Same(args, a);
                invoked++;
                return TplExtensions.CompletedTask;
            };
            var task = handler.InvokeAsync(sender, args);
            Assert.True(task.IsCompleted);
            Assert.Equal(1, invoked);
        }

        private static IAsyncResult BeginTestOperation<T>(AsyncCallback callback, object state, Task<T> asyncTask)
        {
            return asyncTask.ToApm(callback, state);
        }

        private static IAsyncResult BeginTestOperation(AsyncCallback callback, object state, Task asyncTask)
        {
            return asyncTask.ToApm(callback, state);
        }

        private static T EndTestOperation<T>(IAsyncResult asyncResult)
        {
            return ((Task<T>)asyncResult).Result;
        }

        private static void EndTestOperation(IAsyncResult asyncResult)
        {
            ((Task)asyncResult).Wait(); // rethrow exceptions
        }
    }
}
