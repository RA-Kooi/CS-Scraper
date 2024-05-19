using System.Collections.Generic;

namespace Networking;

#nullable enable

public class HandlerResult
{
	public string? json { get; private set; }
	public List<Request>? nextRequest { get; private set; }

	public HandlerResult(string? json = null, List<Request>? nextRequest = null)
	{
		this.json = json;
		this.nextRequest = nextRequest;
	}
}
