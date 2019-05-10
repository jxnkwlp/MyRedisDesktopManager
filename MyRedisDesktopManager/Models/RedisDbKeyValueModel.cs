namespace MyRedisDesktopManager.Models
{
	public class RedisDbKeyValueModel
	{
		public RedisDbKeyModel RedisDbKey { get; set; }

		public RedisType RedisType { get; set; }

		public string Key { get; set; }

		public double TTL { get; set; }


		public string Value { get; set; }

	}
}
