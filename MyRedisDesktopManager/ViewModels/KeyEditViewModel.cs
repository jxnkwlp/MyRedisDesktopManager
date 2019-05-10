using MyRedisDesktopManager.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyRedisDesktopManager.ViewModels
{
	public class KeyEditViewModel : ViewModelBase
	{
		public string Id { get; set; }

		public string Title { get; set; }


		public RedisDbKeyValueModel KeyValue { get; set; }


		public override string ToString()
		{
			return Title;
		}
	}
}
