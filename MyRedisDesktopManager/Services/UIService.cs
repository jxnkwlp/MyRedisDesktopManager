using MyRedisDesktopManager.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyRedisDesktopManager.Services
{
	public class UIService
	{
		private readonly SettingsService _settingsService = new SettingsService();

		public bool TestConnection(ConnectionSettingModel connection)
		{
			throw new NotImplementedException();
		}

		public IList<RedisConnectModel> GetRedisConnects()
		{
			var list = _settingsService.GetConnectionSettings();
			if (list == null) return new List<RedisConnectModel>();

			var list2 = list.Select(t => new RedisConnectModel() { Guid = t.Guid, ConnectionSetting = t, }).ToList();

			//foreach (var item in list2)
			//{
			//	item.Databases = Enumerable.Range(1, 10).Select(t => new RedisDbModel() { Index = t, KeyCount = 0 }).ToList();

			//	foreach (var db in item.Databases)
			//	{
			//		db.Keys = new List<RedisDbKeyModel>();

			//		RedisDbKeyModel key = null;

			//		for (int i = 0; i < 6; i++)
			//		{
			//			if (key == null)
			//			{
			//				key = new RedisDbKeyModel() { FullKey = "a" };
			//				db.Keys.Add(key);
			//			}
			//			else
			//			{
			//				key.FullKey += $":s{i}";
			//				var key2 = new RedisDbKeyModel()
			//				{
			//					FullKey = key.FullKey,
			//					Childrens = new List<RedisDbKeyModel>() { key },
			//				};

			//				db.Keys.Add(key2);
			//			}


			//		}
			//	}

			//}


			return list2;
		}
	}
}
