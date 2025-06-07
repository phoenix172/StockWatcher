using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace StockWatcher.Tests;

public class StubHandler : DelegatingHandler
{
    private readonly string _html;
    public StubHandler(string html) => _html = html;
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        => Task.FromResult(new HttpResponseMessage
        {
            StatusCode = System.Net.HttpStatusCode.OK,
            Content = new StringContent(_html)
        });
}