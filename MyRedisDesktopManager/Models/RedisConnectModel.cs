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
	///  表示一个链接
	/// </summary>
	public class RedisConnectModel : INotifyPropertyChanged
	{
		private bool _isConnection = false;
		private bool _isLoading;

		public Guid Guid { get; set; }

		public ConnectionSettingModel ConnectionSetting { get; set; }

		public ObservableRangeCollection<RedisDbModel> Databases { get; set; } = new ObservableRangeCollection<RedisDbModel>();


		public bool IsLoading
		{
			get { return _isLoading; }
			set { _isLoading = value; OnPropertyChanged(nameof(IsLoading)); }
		}

		public bool IsConnection
		{
			get { return _isConnection; }
			set { _isConnection = value; OnPropertyChanged(nameof(IsConnection)); }
		}


		public event PropertyChangedEventHandler PropertyChanged;

		public void OnPropertyChanged(string propertyName)
		{
			if (PropertyChanged != null)
				PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}
