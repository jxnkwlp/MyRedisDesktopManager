using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyRedisDesktopManager.Models
{
	public class RedisDbKeyModel : INotifyPropertyChanged
	{
		private bool _deleted;

		public RedisDbKeyModel ParentKey { get; set; }

		public RedisDbModel RedisDb { get; set; }

		public string Name { get; set; }

		public string KeyPrefix { get; set; }

		public string FullKey { get; set; }

		public bool Deleted
		{
			get { return _deleted; }
			set { _deleted = value; OnPropertyChanged(nameof(Deleted)); }
		}

		public string KeyId
		{
			get { return RedisDb.RedisConnect.Guid + ":" + FullKey; }
		}

		public ObservableCollection<RedisDbKeyModel> Childrens { get; set; } = new ObservableCollection<RedisDbKeyModel>();

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


		public RedisDbKeyModel(RedisDbModel dbModel, RedisDbKeyModel parent)
		{
			this.RedisDb = dbModel;
			this.ParentKey = parent;
		}

		public void Remove()
		{
			if (this.ChildrenCount > 0)
			{
				Childrens.Clear();
			}

			this.Deleted = true;

			// CheckKeyChildrenCount(this);

			if (ParentKey != null)
			{
				ParentKey.Childrens.Remove(this);

				// CheckKeyChildrenCount(this);
			}
			else
			{
				RedisDb.Keys.Remove(this);
			}

			OnPropertyChanged(nameof(Childrens));
			OnPropertyChanged(nameof(Deleted));
		}

		static void CheckKeyChildrenCount(RedisDbKeyModel key)
		{
			if (key.ChildrenCount == 0)
			{
				if (key.ParentKey != null)
				{
					key.ParentKey.Childrens.Remove(key);

					CheckKeyChildrenCount(key.ParentKey);
				}
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
			return $"{Name}-{FullKey}";
		}
	}
}
