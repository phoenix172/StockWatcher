using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using StockWatcher.Core;
using StockWatcher.Core.Model;
using Xunit;

namespace StockWatcher.Tests
{
    public class StockWatcherTests
    {
        private readonly IProductSource _source;

        private const string SampleHtmlPath =
            "testResponse.html";

        public StockWatcherTests()
        {
            var html = File.ReadAllText(SampleHtmlPath);
            var client = new HttpClient(new StubHandler(html));
            _source = new ProductSource(client);
        }

        [Fact]
        public async Task GreenIconSetsIsAvailableTrue()
        {
            var products = (await _source.Fetch("http://test")).ToList();

            Assert.Contains(products, p => p.IsAvailable);
            Assert.Contains(products, p => !p.IsAvailable);
        }

        [Fact]
        public async Task StateChangeCausesNotification()
        {
            var notifierMock = new Mock<INotifier<WatchState>>();
            var stateStore = Mock.Of<IStateStore<WatchState>>(x =>
                x.Get(It.IsAny<string>()) == Task.FromResult<WatchState>(null) &&
                x.Set(It.IsAny<string>(), It.IsAny<WatchState>()) ==
                Task.CompletedTask);
            Worker worker = new Worker(
                NullLogger<Worker>.Instance, 
                _source,
                stateStore,
                notifierMock.Object,
                new AppSettings
                {
                    Pages = new[] { "http://localhost" },
                    Watches = [new Watch("Tresiba", "TRESIBA", "100")],
                    Mail = null
                }
            );
            CancellationTokenSource cts = new CancellationTokenSource();
            cts.CancelAfter(10000);
            await worker.StartAsync(cts.Token);
            
            while (!notifierMock.Invocations.Any() && !cts.IsCancellationRequested)
            {
            }

            notifierMock.Verify(x => x.NotifyAsync(It.Is<IEnumerable<WatchState>>(state=>
                state.Any(ws => ws.Watch.Label == "Tresiba" && ws.IsAvailable))),
            Times.Once());
        }
    }
}