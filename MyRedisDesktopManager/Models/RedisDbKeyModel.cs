using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyRedisDesktopManager.Models
{
	public class RedisDbKeyModel : INotifyPropertyChanged
	{
		public string Name { get; set; }

		public string KeyPrefix { get; set; }

		public string FullKey { get; set; }


		public IList<RedisDbKeyModel> Childrens { get; set; } = new List<RedisDbKeyModel>();

		public bool HasChildren
		{
			get
			{
				if (Childrens == null || Childrens.Count == 0)
					return false;

				return true;
			}
		}

		public int ChildrenCount
		{
			get
			{
				if (Childrens == null) return 0;
				return Childrens.Count;
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;

		public void OnPropertyChanged(String propertyName)
		{
			if (PropertyChanged != null)
				PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
		}

		public override string ToString()
		{
			return Name;
		}
	}
}
