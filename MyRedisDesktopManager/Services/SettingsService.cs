using MyRedisDesktopManager.Models;
using MyRedisDesktopManager.Properties;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace MyRedisDesktopManager.Services
{
	public class SettingsService
	{
		private static List<ConnectionSettingModel> _cache = new List<ConnectionSettingModel>();

		private static object _key = new object();

		public SettingsService()
		{
			Load();
		}

		public void Add(ConnectionSettingModel model)
		{
			lock (_key)
			{
				_cache.Add(model);

				Save();
			}
		}

		public void Remove(Guid guid)
		{
			lock (_key)
			{
				var temp = _cache.FirstOrDefault(t => t.Guid == guid);

				_cache.Remove(temp);

				Save();
			}
		}

		public void Update(ConnectionSettingModel model)
		{
			lock (_key)
			{
				var temp = _cache.FirstOrDefault(t => t.Guid == model.Guid);
				temp.Name = model.Name;
				temp.Host = model.Host;
				temp.Password = model.Password;
				temp.Port = model.Port;
				temp.Timeout = model.Timeout;
				temp.AllowAdmin = model.AllowAdmin;

				Save();
			}
		}

		public IList<ConnectionSettingModel> GetConnectionSettings()
		{
			return _cache;
		}

		static void Save()
		{
			Settings.Default.ConnectionList = Serialize(_cache);
			Settings.Default.Save();
		}

		static void Load()
		{
			var xml = Settings.Default.ConnectionList;
			if (string.IsNullOrEmpty(xml))
				return;

			XmlSerializer xmlSerializer = new XmlSerializer(typeof(List<ConnectionSettingModel>));
			using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(xml)))
			{
				_cache = (List<ConnectionSettingModel>)xmlSerializer.Deserialize(ms);
			}
		}

		static string Serialize(IList<ConnectionSettingModel> list)
		{
			XmlSerializer xmlSerializer = new XmlSerializer(typeof(List<ConnectionSettingModel>));
			using (var ms = new MemoryStream())
			{
				xmlSerializer.Serialize(ms, list);
				return Encoding.UTF8.GetString(ms.ToArray());
			}
		}


	}
}
