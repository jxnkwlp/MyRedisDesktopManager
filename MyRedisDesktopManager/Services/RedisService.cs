using MyRedisDesktopManager.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyRedisDesktopManager.Services
{
	public class RedisService
	{
		private readonly RedisClientService _redisClientService = new RedisClientService();
		private readonly SettingsService _settingsService = new SettingsService();

		const string SPLITTER = ":";

		public async Task<bool> TestConnectionAsync(ConnectionSettingModel connection)
		{
			var result = await _redisClientService.TryConnectAsync(connection, false);

			return result;
		}

		public async Task<bool> ConnectionAsync(RedisConnectModel connection)
		{
			var connected = await _redisClientService.TryConnectAsync(connection.ConnectionSetting);

			return connected;
		}

		public async Task CloseConnectionAsync(RedisConnectModel connection)
		{
			await _redisClientService.CloseConnectionAsync(connection.Guid);
		}

		public IList<RedisDbModel> GetRedisDbs(RedisConnectModel connect)
		{
			var count = _redisClientService.GetDatabaseCount(connect.Guid);

			return Enumerable.Range(0, count)
				.Select(t => new RedisDbModel()
				{
					RedisConnect = connect,
					Index = t,
				}).ToArray();
		}


		public async Task<(long, IList<RedisDbKeyModel>)> GetKeysAsync(Guid guid, int db)
		{
			var keys = await Task.Run<IList<string>>(() =>
			{
				return _redisClientService.ScanKeys(guid, db);
			});

			var list = await Task.Run<List<RedisDbKeyModel>>(() =>
			{
				var temp = keys.Select(t => SplitKeys(t)).ToList();

				var result = new List<RedisDbKeyModel>();

				foreach (var key in temp)
				{
					ResolveKeys(key, result);
				}

				return result;
			});


			return (keys.Count, list);
		}

		public void ResolveKeys(RedisDbKeyModel key, IList<RedisDbKeyModel> keys)
		{
			var item = keys.FirstOrDefault(t => t.KeyPrefix == key.KeyPrefix);

			if (item == null)
			{
				keys.Add(key);
				item = key;
			}

			if (key.HasChildren)
			{
				foreach (var ckey in key.Childrens)
				{
					ResolveKeys(ckey, item.Childrens);
				}
			}
		}

		/// <summary>
		///  分割 key
		/// </summary> 
		static RedisDbKeyModel SplitKeys(string key)
		{
			if (key.IndexOf(SPLITTER) > 0)
			{
				var name = key.Substring(0, key.IndexOf(SPLITTER));
				var keyModel = new RedisDbKeyModel()
				{
					FullKey = key,
					Name = name,
					KeyPrefix = name,
				};

				ResolveKey(keyModel);

				return keyModel;
			}
			else
			{
				return new RedisDbKeyModel()
				{
					FullKey = key,
					Name = key,
					KeyPrefix = key,
				};
			}

		}

		static void ResolveKey(RedisDbKeyModel key)
		{
			var fullKey = key.FullKey;
			if (fullKey == key.KeyPrefix)
				return;

			var currentPrefix = key.KeyPrefix;

			var nextSplitterIndex = fullKey.IndexOf(SPLITTER, currentPrefix.Length + 1);

			string nextPrefix = string.Empty;
			string nextName = string.Empty;

			if (nextSplitterIndex == -1)
			{
				// key end 
				nextPrefix = fullKey;
				nextName = nextPrefix.Remove(0, currentPrefix.Length + 1);
			}
			else
			{
				nextPrefix = fullKey.Substring(0, nextSplitterIndex);
				nextName = nextPrefix.Remove(0, currentPrefix.Length + 1);
			}

			var nextKey = new RedisDbKeyModel()
			{
				FullKey = key.FullKey,
				KeyPrefix = nextPrefix,
				Name = nextName,
			};

			key.Childrens.Add(nextKey);

			ResolveKey(nextKey);
		}

		public void RemoveConnection(RedisConnectModel model)
		{
			_settingsService.Remove(model.Guid);
		}

		public async Task FlushDBAsync(RedisDbModel db)
		{
			await _redisClientService.FlushDatabaseAsync(db.RedisConnect.Guid, db.Index);
		}
	}
}
