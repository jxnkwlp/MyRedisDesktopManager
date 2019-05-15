using System.ComponentModel;

namespace MyRedisDesktopManager.Models
{
	public class RedisDbKeyValueModel: INotifyPropertyChanged
	{
		public RedisDbKeyModel RedisDbKey { get; set; }

		public RedisType RedisType { get; set; }

		public string Key { get; set; }

		public double TTL { get; set; }
		 
		public string Value { get; set; }


		public event PropertyChangedEventHandler PropertyChanged;

		public void OnPropertyChanged(string propertyName)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}
