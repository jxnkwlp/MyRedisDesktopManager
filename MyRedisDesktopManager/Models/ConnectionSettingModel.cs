using System;

namespace MyRedisDesktopManager.Models
{
	/// <summary>
	///  表示一个链接设置
	/// </summary>
	[Serializable]
	public class ConnectionSettingModel
	{
		public Guid Guid { get; set; } = Guid.NewGuid();

		public string Name { get; set; }

		public string Host { get; set; }

		public int Port { get; set; } = 6379;

		public string Password { get; set; }

		public int Timeout { get; set; } = 30;

		public bool AllowAdmin { get; set; }

		public override string ToString()
		{
			return $"[{Name}][{Guid.ToString()}]";
		}
	}
}
