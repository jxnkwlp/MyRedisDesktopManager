using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyRedisDesktopManager.Services
{
	public class RedisCommandLogWriter : IDisposable
	{
		private string path;
		private static RedisCommandLogWriter _instance = new RedisCommandLogWriter();


		public StreamWriter Writer { get; }

		public static RedisCommandLogWriter Instance => _instance;

		public RedisCommandLogWriter()
		{
			path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory + $"/logs/{DateTime.Now.ToString("yyyyMMdd")}.log");

			if (!Directory.Exists(Path.GetDirectoryName(path)))
			{
				Directory.CreateDirectory(Path.GetDirectoryName(path));
			}

			Writer = new StreamWriter(path, false, Encoding.UTF8, 128);

			Writer.AutoFlush = true;
		}

		public void Dispose()
		{
			Writer.Close();
		}
	}
}
