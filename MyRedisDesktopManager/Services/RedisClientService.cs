using MyRedisDesktopManager.Models;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
				ConnectTimeout = settings.Timeout * 1000, // ms 
				AsyncTimeout = settings.Timeout * 1000, // ms


				AllowAdmin = settings.AllowAdmin,
				ClientName = "MYRDM",
				AbortOnConnectFail = false,

				ConnectRetry = 10,
			};

			options.EndPoints.Add(settings.Host, settings.Port);

			ConnectionMultiplexer connection = await ConnectionMultiplexer.ConnectAsync(options);

			connection.ConnectionFailed += (sender, e) =>
			{
				Debug.WriteLine(e.Exception.ToString());

			};

			var isConnected = connection.IsConnected;

			if (!isConnected)
			{
				await Task.Delay(TimeSpan.FromSeconds(1.5));

				var server = connection.GetServer(connection.GetEndPoints().First());

				var ping = await server.PingAsync();

				isConnected = connection.IsConnected;
			}

			if (save)
				_connections[settings.Guid] = connection;
			else
			{
				//await connection.CloseAsync();
			}

			return isConnected;
		}

		public async Task CloseConnectionAsync(Guid guid)
		{
			var client = GetClient(guid);
			if (client.IsConnected || client.IsConnecting)
			{
				await client.CloseAsync();
			}
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


		public async Task FlushDatabaseAsync(Guid guid, int db)
		{
			var client = GetClient(guid);
			var server = client.GetServer(client.GetEndPoints()[0]);

			await server.FlushDatabaseAsync(db);
		}

		public async Task<RedisDbKeyValueModel> GetKeyValueAsync(Guid guid, int db, string fullKey)
		{
			var client = GetClient(guid);
			var server = client.GetServer(client.GetEndPoints()[0]);
			var database = client.GetDatabase(db);

			var keyType = await database.KeyTypeAsync(fullKey);

			var result = new RedisDbKeyValueModel()
			{
				RedisType = (Models.RedisType)keyType,
				Key = fullKey,
			};

			if (result.RedisType != Models.RedisType.None && result.RedisType != Models.RedisType.Unknown)
			{
				switch (result.RedisType)
				{
					case Models.RedisType.String:

						var value = await database.StringGetAsync(fullKey);
						var liveTime = await database.KeyTimeToLiveAsync(fullKey);

						result.Value = value;
						result.TTL = liveTime.HasValue ? liveTime.Value.TotalSeconds : -1;

						break;
				}
			}

			return result;
		}


		public async Task<bool> KeyRenameAsync(Guid guid, int db, string oldKey, string newKey)
		{
			var client = GetClient(guid);
			var database = client.GetDatabase(db);

			return await database.KeyRenameAsync(oldKey, newKey);
		}

		public async Task<bool> KeyDeleteAsync(Guid guid, int db, string key)
		{
			var client = GetClient(guid);
			var database = client.GetDatabase(db);

			return await database.KeyDeleteAsync(key);
		}

		public async Task<bool> KeyExpireAsync(Guid guid, int db, string key, TimeSpan? newTime)
		{
			var client = GetClient(guid);
			var database = client.GetDatabase(db);

			return await database.KeyExpireAsync(key, newTime);
		}

		public async Task<bool> StringSetAsync(Guid guid, int db, string key, string value)
		{
			var client = GetClient(guid);
			var database = client.GetDatabase(db);

			return await database.StringSetAsync(key, value);
		}
	}

}
