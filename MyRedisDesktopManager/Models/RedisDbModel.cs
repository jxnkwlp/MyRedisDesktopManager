using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyRedisDesktopManager.Models
{
	/// <summary>
	///  表示一个DB
	/// </summary>
	public class RedisDbModel : INotifyPropertyChanged
	{
		private bool _isLoading;
		private bool _hasLoadChidren;
		private long _keyCount;

		public RedisConnectModel RedisConnect { get; set; }

		public int Index { get; set; }

		public long KeyCount
		{
			get { return _keyCount; }
			set { _keyCount = value; OnPropertyChanged(nameof(KeyCount)); }
		}

		public ObservableRangeCollection<RedisDbKeyModel> Keys { get; set; } = new ObservableRangeCollection<RedisDbKeyModel>();

		public bool IsLoading
		{
			get { return _isLoading; }
			set { _isLoading = value; OnPropertyChanged(nameof(IsLoading)); }
		}

		public bool HasLoadChidren
		{
			get { return _hasLoadChidren; }
			set { _hasLoadChidren = value; OnPropertyChanged(nameof(HasLoadChidren)); }
		}


		public event PropertyChangedEventHandler PropertyChanged;

		public void OnPropertyChanged(string propertyName)
		{
			if (PropertyChanged != null)
				PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
		}

		public override string ToString()
		{
			return $"DB {Index}";
		}
	}
}
