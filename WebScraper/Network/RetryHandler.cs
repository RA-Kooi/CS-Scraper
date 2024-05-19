using System;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

using Polly;

namespace Networking;

using Settings;

public class RetryHandler : DelegatingHandler
{
	public RetryHandler(HttpClientHandler handler)
		: base(handler)
	{
	}

	protected override async Task<HttpResponseMessage> SendAsync(
		HttpRequestMessage request,
		CancellationToken cancellationToken)
	{
		return await Policy
			.Handle<HttpRequestException>()
			.Or<TaskCanceledException>()
			.OrResult<HttpResponseMessage>(x => Settings.retryCodes.Contains((int)x.StatusCode))
			.WaitAndRetryForeverAsync(
				retryAttempt =>
				{
					int waitTime = Settings.initialErrorWaitTime;
					waitTime += (Settings.errorWaitStep * (retryAttempt - 1));

					return TimeSpan.FromSeconds(Math.Min(Settings.maxErrorWaitTime, waitTime));
				})
			.ExecuteAsync(() => base.SendAsync(request, cancellationToken));
	}
}
