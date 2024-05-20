using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Data;

public class JSONWriter
{
	private readonly SemaphoreSlim sem;
	private readonly StreamWriter writer;

	public JSONWriter(StreamWriter writer)
	{
		sem = new(1, 1);
		this.writer = writer;
	}

	public async Task Write(string json)
	{
		await sem.WaitAsync();
		await writer.WriteAsync(json);
		await writer.WriteAsync('\n');
		sem.Release();
	}
}
