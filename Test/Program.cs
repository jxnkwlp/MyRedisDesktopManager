using StackExchange.Redis;
using System;

namespace Test
{
	class Program
	{
		static async System.Threading.Tasks.Task Main(string[] args)
		{
			ConnectionMultiplexer connection = await ConnectionMultiplexer.ConnectAsync("localhost");

			for (int index = 0; index < 3; index++)
			{
				var db = connection.GetDatabase(index);

				for (int i = 0; i < 10; i++)
				{
					db.StringSet($"a{i}", i);
				}

				for (int i = 0; i < 10; i++)
				{
					db.StringSet($"a:b{i}", i);
				}

				for (int i = 0; i < 10; i++)
				{
					db.StringSet($"a:b{i}:c{i}", i);
				}
			}



			Console.WriteLine("end");
		}
	}
}
