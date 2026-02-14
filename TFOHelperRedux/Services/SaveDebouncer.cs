using System;
using System.Threading;
using System.Threading.Tasks;

namespace TFOHelperRedux.Services
{
    internal static class SaveDebouncer
    {
        private static readonly TimeSpan Delay = TimeSpan.FromMilliseconds(800);
        private static CancellationTokenSource? _cts;

        public static void ScheduleSaveFishes()
        {
            Schedule(async () => await Task.Run(() => DataService.SaveFishes(DataStore.Fishes)));
        }

        public static void ScheduleSaveFeeds()
        {
            Schedule(async () => await Task.Run(() => DataService.SaveFeeds(DataStore.Feeds)));
        }

        public static void ScheduleSaveDips()
        {
            Schedule(async () => await Task.Run(() => DataService.SaveDips(DataStore.Dips)));
        }

        public static void ScheduleSaveLures()
        {
            Schedule(async () => await Task.Run(() => DataService.SaveLures(DataStore.Lures)));
        }

        private static void Schedule(Func<Task> saveAction)
        {
            _cts?.Cancel();
            _cts = new CancellationTokenSource();
            var token = _cts.Token;

            Task.Delay(Delay, token).ContinueWith(t =>
            {
                if (t.IsCanceled) return;
                try
                {
                    saveAction().Wait();
                }
                catch { }
            }, TaskScheduler.Default);
        }
    }
}
