using System;
using System.Collections.Generic;

namespace Networking;

public class ScrapeException: Exception
{
	public List<Exception> exceptions { get; private set; }

	public ScrapeException(List<Exception> exceptions)
	{
		this.exceptions = exceptions;
	}
}
