using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace Networking;

public class Request
{
	public string url { get; private set; }

	public Func
	<
		string,
		HttpResponseMessage,
		Dictionary<string, object>,
		Task<HandlerResult>
	> callback { get; private set; }

	public Action<string, HttpResponseMessage> errback { get; private set; }
	public Dictionary<string, string> cookies { get; private set; } = null;
	public Dictionary<string, object> state { get; private set; } = null;
	public byte[] content { get; private set; } = null;

	public Request(
		string url,
		Func
		<
			string,
			HttpResponseMessage,
			Dictionary<string, object>,
			Task<HandlerResult>
		> callback,
		Action<string, HttpResponseMessage> errback = null,
		Dictionary<string, string> cookies = null,
		Dictionary<string, object> state = null,
		byte[] content = null)
	{
		this.url = url;
		this.callback = callback;

		if(errback != null)
			this.errback = errback;
		else
			this.errback = DefaultErrorHandler;

		this.cookies = cookies;
		this.state = state;
		this.content = content;
	}

	private void DefaultErrorHandler(string url, HttpResponseMessage response)
	{
		Console.Error.WriteLine($"Got <{(int)response.StatusCode} ({response.StatusCode.ToString()})> from: {url}");
	}
}
