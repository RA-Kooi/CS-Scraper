using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Networking;

using Settings;
using Data;

public class Scraper
{
	private readonly string baseUrl;
	private readonly JSONWriter writer;
	private bool done;

	public Scraper(string baseUrl, JSONWriter writer)
	{
		this.baseUrl = baseUrl;
		this.writer = writer;
		done = false;
	}

	public void Run(Func<Request> nextRequest)
	{
		HttpClientHandler clientHandler = new(){ UseCookies = false };

		HttpClient client = new(new RetryHandler(clientHandler));
		client.Timeout = TimeSpan.FromSeconds(10);

		client.DefaultRequestHeaders.AcceptLanguage.Add(new("en-US"));
		client.DefaultRequestHeaders.AcceptLanguage.Add(new("en"));
		client.DefaultRequestHeaders.UserAgent.ParseAdd(
			"Mozilla/5.0 (X11; Linux x86_64) "
			+ "AppleWebKit/537.36 (KHTML, like Gecko) "
			+ "Chrome/110.0.0.0 Safari/537.36");

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
				List<Task> extraHandlers = (finishedTask as Task<List<Task>>)
					.Result;

				handlers.AddRange(extraHandlers);
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

	private async Task<List<Task>> HandleRequest(
		HttpClient client,
		Request request)
	{
		string url = request.url;
		if(!url.StartsWith("http"))
		{
			bool condition = url.StartsWith('/')
				|| url.StartsWith('?')
				|| url.StartsWith('&');

			string sep = condition ? "" : "/";
			url = $"{baseUrl}{sep}{request.url}";
		}

		HttpMethod method = request.content == null
			? HttpMethod.Get
			: HttpMethod.Post;

		HttpRequestMessage message = new(method, url);

		if(method == HttpMethod.Post)
		{
			message.Content = new ByteArrayContent(request.content);
			message.Content.Headers.Add(
				"Content-Length",
				request.content.Length.ToString());

			message.Content.Headers.Add(
				"Content-Type",
				"application/x-www-form-urlencoded");
		}

		if(request.cookies != null && request.cookies.Keys.Count > 0)
		{
			StringBuilder cookieBuilder = new();
			var keys = request.cookies.Keys.GetEnumerator();
			keys.MoveNext();
			for (int i = 0; i < request.cookies.Count; ++i)
			{
				string key = keys.Current;
				bool shouldAddComma = i < (request.cookies.Count - 1);
				string comma = shouldAddComma ? "; " : "";

				cookieBuilder.Append($"{key}={request.cookies[key]}{comma}");

				keys.MoveNext();
			}

			message.Headers.Add("Cookie", cookieBuilder.ToString());
		}

		HttpResponseMessage response;

		while(true)
		{
			try
			{
				response = await client.SendAsync(message);
				break;
			}
			catch(TaskCanceledException)
			{
				Console.Error.WriteLine($"Timeout... Retrying: {url}");
				HttpRequestMessage msg = CloneHttpRequestMessage(message);
				message = msg;
			}
			catch (HttpRequestException ex)
			{
				Console.Error.WriteLine(ex.Message);
				if(ex.Message.Contains("Connection reset"))
				{
					Console.Error.WriteLine("Retrying request...");
					continue;
				}

				throw;
			}
		}

		if(!response.IsSuccessStatusCode)
		{
			request.errback(request.url, response);
			return new();
		}

		HandlerResult result = await request.callback(
			request.url,
			response,
			request.state);

		if(result.json != null)
			await writer.Write(result.json);

		List<Task> ret = new();

		if(result.nextRequest != null)
		{
			foreach(Request rq in result.nextRequest)
				ret.Add(HandleRequest(client, rq));
		}

		return ret;
	}

	public void Stop()
	{
		done = true;
	}

	private static HttpRequestMessage CloneHttpRequestMessage(
		HttpRequestMessage req)
	{
		HttpRequestMessage clone = new(req.Method, req.RequestUri);

		MemoryStream ms = new();
		if (req.Content != null)
		{
			req.Content.CopyToAsync(ms).Wait();
			ms.Position = 0;
			clone.Content = new StreamContent(ms);

			foreach (var h in req.Content.Headers)
				clone.Content.Headers.Add(h.Key, h.Value);
		}

		clone.Version = req.Version;

		foreach (KeyValuePair<string, object> option in req.Options)
			clone.Options.Set(new(option.Key), option.Value);

		foreach (KeyValuePair<string, IEnumerable<string>> header in req.Headers)
			clone.Headers.TryAddWithoutValidation(header.Key, header.Value);

		return clone;
	}
}
