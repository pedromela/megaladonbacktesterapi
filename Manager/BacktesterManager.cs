using BacktesterAPI.HubConfig;
using BacktesterAPI.Models;
using BacktesterLib.Lib;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using UtilsLib.Utils;

namespace BacktesterAPI.Manager
{
    public class BacktesterManager
    {
        private readonly IBackgroundTaskQueue _taskQueue;
        private IHubContext<ChartHub> _hub;
        private readonly CancellationToken _cancellationToken;

        public BacktesterManager(IBackgroundTaskQueue taskQueue, IHostApplicationLifetime applicationLifetime, IHubContext<ChartHub> hub)
        {
            _taskQueue = taskQueue;
            _cancellationToken = applicationLifetime.ApplicationStopping;
            _hub = hub;
        }

        public void StartBacktestBot(string botId, DateTime fromDate, DateTime toDate)
        {
            // Run a console user input loop in a background thread
            Task.Run(async () => await BacktestBotAsync(botId, fromDate, toDate));
        }

        public async ValueTask BacktestBotAsync(string botId, DateTime fromDate, DateTime toDate)
        {
            if (!_cancellationToken.IsCancellationRequested)
            {
                // Enqueue a background work item
                await _taskQueue.QueueBackgroundWorkItemAsync(ct => BacktestBot(botId, fromDate, toDate, ct));
            }
        }

        public IObservable<BacktestData> BacktestBotObservable(string botId, DateTime fromDate, DateTime toDate)
        {
            return Observable.Create<BacktestData>((o) => {
                Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("pt-PT");
                BacktesterEngine.BacktesterEngine backtesterEngine = new BacktesterEngine.BacktesterEngine(fromDate, toDate);

                backtesterEngine.BacktestBot(botId, fromDate, toDate, o);
                o.OnCompleted();
                return () => { };
            });

        }

        public ValueTask BacktestBot(string botId, DateTime fromDate, DateTime toDate, CancellationToken cancellationToken)
        {
            BacktestBotObservable(botId, fromDate, toDate)
            .Subscribe((data) => {
                var list = new List<ChartModel>()
                {
                    new ChartModel { Data = new List<int> { data.Positions }, Label = "Positions" },
                    new ChartModel { Data = new List<int> { data.Successes }, Label = "Successes" },
                };
                _hub.Clients.All.SendAsync("transferchartdata", list);
            });

            //await Task.Run(() => backtesterEngine.BacktestBot(botId, fromDate, toDate));
            return new ValueTask();
        }
    }
}
