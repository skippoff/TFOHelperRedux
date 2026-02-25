using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace TFOHelperRedux.Services.Data
{
    /// <summary>
    /// Сервис для отложенного сохранения данных
    /// </summary>
    public class SaveDebouncer
    {
        private readonly IDataLoadSaveService _loadSaveService;
        private readonly TimeSpan _delay = TimeSpan.FromMilliseconds(800);
        private CancellationTokenSource? _cts;

        public SaveDebouncer(IDataLoadSaveService loadSaveService)
        {
            _loadSaveService = loadSaveService;
        }

        public void ScheduleSaveFishes()
        {
            Schedule(() => _loadSaveService.SaveFishes(DataStore.Fishes));
        }

        public void ScheduleSaveFeeds()
        {
            Schedule(() => _loadSaveService.SaveFeeds(DataStore.Feeds));
        }

        public void ScheduleSaveDips()
        {
            Schedule(() => _loadSaveService.SaveDips(DataStore.Dips));
        }

        public void ScheduleSaveLures()
        {
            Schedule(() => _loadSaveService.SaveLures(DataStore.Lures));
        }

        public void ScheduleSaveCatchPoints()
        {
            Schedule(() =>
            {
                var localDataDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Maps");
                var localCatchFile = Path.Combine(localDataDir, "CatchPoints_Local.json");
                JsonService.Save(localCatchFile, DataStore.CatchPoints);
            });
        }

        private void Schedule(Action saveAction)
        {
            _cts?.Cancel();
            _cts = new CancellationTokenSource();
            var token = _cts.Token;

            Task.Delay(_delay, token).ContinueWith(t =>
            {
                if (t.IsCanceled) return;
                try
                {
                    saveAction();
                }
                catch { }
            }, TaskScheduler.Default);
        }
    }
}
