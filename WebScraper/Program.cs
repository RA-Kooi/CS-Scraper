using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;
using System.Threading.Tasks;

using Fizzler.Systems.HtmlAgilityPack;
using HtmlAgilityPack;
using ShellProgressBar;

using Data;
using Networking;
using Utils;

TextWriter errLog = new StreamWriter(File.OpenWrite("error.log"), Encoding.UTF8);
TextWriter stderr = Console.Error;
using SplitWriter errorLog = new(errLog, stderr);
Console.SetError(errorLog);

Dictionary<string, string> cookies = new();

async Task<HandlerResult> HandleRequest(
	string url,
	HttpResponseMessage response,
	Dictionary<string, object> state)
{
	Stream stream = await response.Content.ReadAsStreamAsync();
	return new(null, null);
}

ProgressBar progress = new(0, "Downloading entries");

Request GenRequests()
{
	return new("/", HandleRequest, cookies: cookies);
}

try
{
	using (StreamWriter writer = new("out/output.json", true))
	{
		Scraper scraper = new("https://example.com", new(writer));

		ConsoleCancelEventHandler stopDelegate = delegate(object sender, ConsoleCancelEventArgs e)
		{
			e.Cancel = true;
			scraper.Stop();
		};

		Console.CancelKeyPress += stopDelegate;
		scraper.Run(GenRequests);
		Console.CancelKeyPress -= stopDelegate;

		progress.Dispose();
	}
}
catch(AggregateException ae)
{
	if(progress != null)
		progress.Dispose();

	ae.Handle(
		(x) =>
		{
			if(x is ScrapeException)
			{
				ScrapeException error = x as ScrapeException;
				foreach(Exception e in error.exceptions)
				{
					Exception current = e;
					do
					{
						Console.Error.WriteLine(current.Message);
						Console.Error.WriteLine("--------------------------------");
						Console.Error.WriteLine(current.StackTrace);
						Console.Error.WriteLine("--------------------------------");
						Console.Error.WriteLine("--------------------------------");
					} while((current = current.InnerException) != null);
				}

				return true;
			}

			return false;
		});
}
