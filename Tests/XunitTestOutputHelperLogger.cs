using System;
using System.Net;
using Orleans.Runtime;
using Xunit.Abstractions;

namespace OrleansSerializationExceptionTests
{
	internal class XunitTestOutputHelperLogger : IFlushableLogConsumer
	{
		private ITestOutputHelper output;

		public XunitTestOutputHelperLogger(ITestOutputHelper output)
		{
			this.output = output;
		}

		public void Flush()
		{
			//throw new NotImplementedException();
		}

		public void Log(Severity severity, LoggerType loggerType, string caller, string message, IPEndPoint myIPEndPoint, Exception exception, int eventCode = 0)
		{
			try
			{
				this.output.WriteLine($"{severity} {loggerType} {caller} {eventCode} {myIPEndPoint} {message} Exception:{exception}");

			}
			catch (Exception e)
			{
				Console.WriteLine(e);
				//throw;
			}
		}
	}
}