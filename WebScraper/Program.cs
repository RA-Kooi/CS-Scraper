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

using ShellProgressBar;

using Data;
using Networking;
using Utils;

HashSet<string> downloaded = new();

StreamWriter errLog = new(File.OpenWrite("error.log"), Encoding.UTF8);
errLog.AutoFlush = true;
TextWriter stderr = Console.Error;
using SplitWriter errorLog = new(errLog, stderr);
Console.SetError(errorLog);

int pages = 1, page = -1;
ProgressBar progress = new(pages, "Downloading entries");

if(!Directory.Exists("out/img"))
	Directory.CreateDirectory("out/img");

if(File.Exists("out/output.json"))
{
	using FileStream file = File.OpenRead("out/output.json");
	using StreamReader reader = new(file, Encoding.UTF8, false);
	string line;
	while ((line = reader.ReadLine()) != null)
	{
		Output output = JsonSerializer.Deserialize<Output>(line)!;
		downloaded.Add(output.Name);
	}
}

async Task<HandlerResult> HandleImage(
	string url,
	HttpResponseMessage response,
	Dictionary<string, object> state)
{
	string fileName = url
		.Split('/', StringSplitOptions.RemoveEmptyEntries)
		.Last();

	ChildProgressBar imageProgress = state["imageProgress"] as ChildProgressBar;

	using (FileStream file = File.Create($"out/img/{fileName}", 8192))
	{
		byte[] buffer = new byte[8192];

		Stream content = await response.Content.ReadAsStreamAsync();

		int totalBytesRead = 0;
		while(totalBytesRead < content.Length)
		{
			int bytesRead = await content.ReadAsync(buffer, 0, 8192);
			totalBytesRead += bytesRead;

			await file.WriteAsync(buffer, 0, bytesRead);
		}
	}

	Output output = new();
	output.Name = fileName;

	JsonSerializerOptions opt = new();
	opt.Encoder = JavaScriptEncoder.Create(UnicodeRanges.All);
	string outputStr = JsonSerializer.Serialize(output, opt);

	imageProgress.Tick();
	progress.Tick();

	if(imageProgress.Percentage >= 99.99)
		imageProgress.Dispose();

	return new(outputStr, null);
}

bool first = true;
async Task<HandlerResult> HandleRequest(
	string url,
	HttpResponseMessage response,
	Dictionary<string, object> state)
{
	Stream stream = await response.Content.ReadAsStreamAsync();

	byte[] buffer = new byte[stream.Length];
	int bytesRead = 0;
	while(bytesRead < stream.Length)
		bytesRead += await stream.ReadAsync(buffer, bytesRead, buffer.Length);

	ItemSearch search = JsonSerializer.Deserialize<ItemSearch>(Encoding.UTF8.GetString(buffer))!;

	if(first)
	{
		int items = Convert.ToInt32(search.Total);
		progress.MaxTicks = items;
		pages = (int)Math.Ceiling((double)items / search.Items.Count);
		first = false;
	}

	List<Request> nextRequests = new(search.Items.Count);

	// Defaults to true btw /s
	ProgressBarOptions options = new();
	options.CollapseWhenFinished = true;
	ChildProgressBar imageProgress = progress.Spawn(search.Items.Count, "Downloading images", options);

	state = new();
	state.Add("imageProgress", imageProgress);

	foreach(Item item in search.Items)
	{
		if(downloaded.Contains($"{item.CardNr}.jpg"))
		{
			imageProgress.Tick();
			progress.Tick();
			continue;
		}

		url = $"https://www.takaratomy.co.jp/products/en.wixoss/card/thumb/{item.CardNr}.jpg";
		nextRequests.Add(new(url, HandleImage, state: state));
	}

	if(nextRequests.Count == 0)
		imageProgress.Dispose();

	return new(null, nextRequests);
}

Request GenRequests()
{
	// Stupid hack to wait on the first request to finish...
	while(first && page == 0)
		Task.Delay(100).Wait(); // Make it work in the debugger.

	if(page < pages)
	{
		++page;
		return new($"/itemsearch.php?p={page}", HandleRequest);
	}

	return null;
}

try
{
	using StreamWriter writer = new("out/output.json", true);
	Scraper scraper = new("https://www.takaratomy.co.jp/products/en.wixoss/card", new(writer));

	ConsoleCancelEventHandler stopDelegate = delegate (object sender, ConsoleCancelEventArgs e)
	{
		e.Cancel = true;
		scraper.Stop();
	};

	Console.CancelKeyPress += stopDelegate;
	scraper.Run(GenRequests);
	Console.CancelKeyPress -= stopDelegate;

	progress.Dispose();
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
						Console.Error.WriteLine($"{e.GetType().ToString()}: {current.Message}");
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
