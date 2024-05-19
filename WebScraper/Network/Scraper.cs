using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Networking;

using Settings;
using Data;

public class Scraper
{
	private string baseUrl;
	private JSONWriter writer;
	private bool done;

	public Scraper(string baseUrl, JSONWriter writer)
	{
		this.baseUrl = baseUrl;
		this.writer = writer;
		done = false;
	}

	public void Run(Func<Request> nextRequest)
	{
		HttpClientHandler clientHandler = new();
		clientHandler.UseCookies = false;

		HttpClient client = new(new RetryHandler(clientHandler));
		client.DefaultRequestHeaders.AcceptLanguage.Add(new("en-US"));
		client.DefaultRequestHeaders.AcceptLanguage.Add(new("en"));
		client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/110.0.0.0 Safari/537.36");

		GenAndQueue(client, nextRequest).Wait();
	}

	private async Task GenAndQueue(HttpClient client, Func<Request> nextRequest)
	{
		List<Task> handlers = new(Settings.maxConcurrency);
		List<Exception> exceptions = new();

		while(true)
		{
			Task finishedTask = null;

			try
			{
				while(handlers.Count < Settings.maxConcurrency && !done)
				{
					Request request = nextRequest();
					if(request == null)
					{
						done = true;
						break;
					}

					Task handler = HandleRequest(client, request);
					handlers.Add(handler);
				}

				if(handlers.Count == 0)
					break;

				finishedTask = await Task.WhenAny(handlers);
				await finishedTask;
				handlers.Remove(finishedTask);
			}
			catch(Exception e)
			{
				done = true;
				exceptions.Add(e);
				handlers.Remove(finishedTask);
			}
		}

		if(exceptions.Count > 0)
		{
			throw new ScrapeException(exceptions);
		}
	}

	private async Task HandleRequest(HttpClient client, Request request)
	{
		string url = request.url;
		if(!url.StartsWith("http"))
		{
			string sep = url.StartsWith('/') || url.StartsWith('?') || url.StartsWith('&')
				? ""
				: "/";
			url = $"{baseUrl}{sep}{request.url}";
		}

		HttpMethod method = request.content == null ? HttpMethod.Get : HttpMethod.Post;
		HttpRequestMessage message = new(method, url);

		if(method == HttpMethod.Post)
		{
			message.Content = new ByteArrayContent(request.content);
			message.Content.Headers.Add("Content-Length", request.content.Length.ToString());
			message.Content.Headers.Add("Content-Type", "application/x-www-form-urlencoded");
		}

		StringBuilder cookieBuilder = new();
		var keys = request.cookies.Keys.GetEnumerator();
		keys.MoveNext();
		for(int i = 0; i < request.cookies.Count; ++i)
		{
			string key = keys.Current;
			bool shouldAddComma = i < (request.cookies.Count - 1);
			string comma = shouldAddComma ? "; " : "";

			cookieBuilder.Append($"{key}={request.cookies[key]}{comma}");

			keys.MoveNext();
		}

		message.Headers.Add("Cookie", cookieBuilder.ToString());

		HttpResponseMessage response = await client.SendAsync(message);

		if(!response.IsSuccessStatusCode)
		{
			request.errback(request.url, response);
			return;
		}

		HandlerResult result = await request.callback(
			request.url,
			response,
			request.state);

		if(result.json != null)
			await writer.Write(result.json);

		if(result.nextRequest != null)
		{
			foreach(Request rq in result.nextRequest)
				await HandleRequest(client, rq);
		}
	}

	public void Stop()
	{
		done = true;
	}
}
