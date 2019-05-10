using MyRedisDesktopManager.Models;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MyRedisDesktopManager.Services
{
	public class RedisClientService
	{
		private static Dictionary<Guid, ConnectionMultiplexer> _connections = new Dictionary<Guid, ConnectionMultiplexer>();


		public ConnectionMultiplexer GetClient(Guid guid)
		{
			if (_connections.ContainsKey(guid))
			{
				return _connections[guid];
			}
			else
			{
				throw new Exception("The Connection not found.");
			}
		}

		public async Task<bool> TryConnectAsync(ConnectionSettingModel settings, bool save = true)
		{
			var options = new ConfigurationOptions()
			{
				Password = settings.Password,
				ConnectTimeout = settings.Timeout,
				//AllowAdmin = true,
				ClientName = "MYRDM",
			};

			options.EndPoints.Add(settings.Host, settings.Port);

			ConnectionMultiplexer connection = await ConnectionMultiplexer.ConnectAsync(options);

			var isConnected = connection.IsConnected;

			if (save)
				_connections[settings.Guid] = connection;
			else
			{
				//await connection.CloseAsync();
			}

			return isConnected;
		}


		public int GetDatabaseCount(Guid guid)
		{
			var client = GetClient(guid);
			var server = client.GetServer(client.GetEndPoints()[0]);

			var count = server.DatabaseCount;

			//var infos = server.Info();
			////var r = client.GetDatabase().ScriptEvaluate("CONFIG GET databases");
			//var s = client.GetStatus();
			//var s2 = client.OperationCount;
			//var s3 = client.GetCounters();

			return count;
		}

		public IList<string> ScanKeys(Guid guid, int db, string pattern = null)
		{
			var client = GetClient(guid);
			var server = client.GetServer(client.GetEndPoints()[0]);

			var keys = server.Keys(db, pattern);

			return keys.Select(t => t.ToString()).OrderBy(t => t).ToArray();
		}
	}
}
