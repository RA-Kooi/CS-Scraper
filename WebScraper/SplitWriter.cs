using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Utils;

public class SplitWriter: TextWriter, IDisposable
{
	private readonly TextWriter l, r;

	private bool disposed;

	public SplitWriter(TextWriter l, TextWriter r)
	{
		this.l = l;
		this.r = r;
		disposed = false;
	}

	public override Encoding Encoding
	{
		get => l.Encoding;
	}

	public override IFormatProvider FormatProvider
	{
		get => l.FormatProvider;
	}

	public override string NewLine
	{
		get => l.NewLine;
		set
		{
			l.NewLine = value;
			r.NewLine = value;
		}
	}

	public override void Close()
	{
		l.Close();
		r.Close();
	}

	public new void Dispose()
	{
		Dispose(true);
		GC.SuppressFinalize(this);
	}

	protected override void Dispose(bool disposing)
	{
		if(!disposed && disposing)
		{
			l.Dispose();
			r.Dispose();

			disposed = true;
		}
	}

	public override async ValueTask DisposeAsync()
	{
		await l.DisposeAsync();
		await r.DisposeAsync();
		GC.SuppressFinalize(this);
	}

	public override void Flush()
	{
		l.Flush();
		r.Flush();
	}

	public override async Task FlushAsync()
	{
		await l.FlushAsync();
		await r.FlushAsync();
	}

#nullable enable
	public override void Write(string format, object? arg0)
	{
		l.Write(format, arg0);
		r.Write(format, arg0);
	}

	public override void Write(string format, object? arg0, object? arg1)
	{
		l.Write(format, arg0, arg1);
		r.Write(format, arg0, arg1);
	}

	public override void Write(string format, object? arg0, object? arg1, object? arg2)
	{
		l.Write(format, arg0, arg1, arg2);
		r.Write(format, arg0, arg1, arg2);
	}

	public override void Write(string format, params object?[] args)
	{
		l.Write(format, args);
		r.Write(format, args);
	}

	public override void Write(StringBuilder? value)
	{
		l.Write(value);
		r.Write(value);
	}

	public override void Write(string? value)
	{
		l.Write(value);
		r.Write(value);
	}

	public override void Write(object? value)
	{
		l.Write(value);
		r.Write(value);
	}

	public override void Write(char[]? buffer)
	{
		l.Write(buffer);
		r.Write(buffer);
	}

	public override async Task WriteAsync(StringBuilder? value, CancellationToken cancellationToken = default)
	{
		await l.WriteAsync(value, cancellationToken);
		await r.WriteAsync(value, cancellationToken);
	}

	public override async Task WriteAsync(string? value)
	{
		await l.WriteAsync(value);
		await r.WriteAsync(value);
	}

	public override void WriteLine(string format, object? arg0, object? arg1, object? arg2)
	{
		l.WriteLine(format, arg0, arg1, arg2);
		r.WriteLine(format, arg0, arg1, arg2);
	}

	public override void WriteLine(string format, object? arg0, object? arg1)
	{
		l.WriteLine(format, arg0, arg1);
		r.WriteLine(format, arg0, arg1);
	}

	public override void WriteLine(string format, object? arg0)
	{
		l.WriteLine(format, arg0);
		r.WriteLine(format, arg0);
	}

	public override void WriteLine(string format, params object?[] args)
	{
		l.WriteLine(format, args);
		r.WriteLine(format, args);
	}

	public override void WriteLine(StringBuilder? value)
	{
		l.WriteLine(value);
		r.WriteLine(value);
	}

	public override void WriteLine(string? value)
	{
		l.WriteLine(value);
		r.WriteLine(value);
	}

	public override void WriteLine(object? value)
	{
		l.WriteLine(value);
		r.WriteLine(value);
	}

	public override void WriteLine(char[]? buffer)
	{
		l.WriteLine(buffer);
		r.WriteLine(buffer);
	}

	public override async Task WriteLineAsync(StringBuilder? value, CancellationToken cancellationToken = default)
	{
		await l.WriteLineAsync(value, cancellationToken);
		await r.WriteLineAsync(value, cancellationToken);
	}

	public override async Task WriteLineAsync(string? value)
	{
		await l.WriteLineAsync(value);
		await r.WriteLineAsync(value);
	}
#nullable disable

	public override void Write(char[] buffer, int index, int count)
	{
		l.Write(buffer, index, count);
		r.Write(buffer, index, count);
	}

	public override void Write(ulong value)
	{
		l.Write(value);
		r.Write(value);
	}

	public override void Write(uint value)
	{
		l.Write(value);
		r.Write(value);
	}

	public override void Write(float value)
	{
		l.Write(value);
		r.Write(value);
	}

	public override void Write(long value)
	{
		l.Write(value);
		r.Write(value);
	}

	public override void Write(int value)
	{
		l.Write(value);
		r.Write(value);
	}

	public override void Write(double value)
	{
		l.Write(value);
		r.Write(value);
	}

	public override void Write(decimal value)
	{
		l.Write(value);
		r.Write(value);
	}

	public override void Write(char value)
	{
		l.Write(value);
		r.Write(value);
	}

	public override void Write(bool value)
	{
		l.Write(value);
		r.Write(value);
	}

	public override void Write(ReadOnlySpan<char> value)
	{
		l.Write(value);
		r.Write(value);
	}

	public override async Task WriteAsync(char[] buffer, int index, int count)
	{
		await l.WriteAsync(buffer, index, count);
		await r.WriteAsync(buffer, index, count);
	}

	public override async Task WriteAsync(ReadOnlyMemory<char> buffer, CancellationToken cancellationToken = default)
	{
		await l.WriteAsync(buffer, cancellationToken);
		await r.WriteAsync(buffer, cancellationToken);
	}

	public override async Task WriteAsync(char value)
	{
		await l.WriteAsync(value);
		await r.WriteAsync(value);
	}

	public override void WriteLine(char[] buffer, int index, int count)
	{
		l.WriteLine(buffer, index, count);
		r.WriteLine(buffer, index, count);
	}

	public override void WriteLine(ulong value)
	{
		l.WriteLine(value);
		r.WriteLine(value);
	}

	public override void WriteLine(uint value)
	{
		l.WriteLine(value);
		r.WriteLine(value);
	}

	public override void WriteLine(float value)
	{
		l.WriteLine(value);
		r.WriteLine(value);
	}

	public override void WriteLine(long value)
	{
		l.WriteLine(value);
		r.WriteLine(value);
	}

	public override void WriteLine(int value)
	{
		l.WriteLine(value);
		r.WriteLine(value);
	}

	public override void WriteLine(double value)
	{
		l.WriteLine(value);
		r.WriteLine(value);
	}

	public override void WriteLine(decimal value)
	{
		l.WriteLine(value);
		r.WriteLine(value);
	}

	public override void WriteLine(char value)
	{
		l.WriteLine(value);
		r.WriteLine(value);
	}

	public override void WriteLine(bool value)
	{
		l.WriteLine(value);
		r.WriteLine(value);
	}

	public override void WriteLine(ReadOnlySpan<char> value)
	{
		l.WriteLine(value);
		r.WriteLine(value);
	}

	public override async Task WriteLineAsync(char[] buffer, int index, int count)
	{
		await l.WriteLineAsync(buffer, index, count);
		await r.WriteLineAsync(buffer, index, count);
	}

	public override async Task WriteLineAsync(ReadOnlyMemory<char> buffer, CancellationToken cancellationToken = default)
	{
		await l.WriteLineAsync(buffer, cancellationToken);
		await r.WriteLineAsync(buffer, cancellationToken);
	}

	public override async Task WriteLineAsync(char value)
	{
		await l.WriteLineAsync(value);
		await r.WriteLineAsync(value);
	}
}
