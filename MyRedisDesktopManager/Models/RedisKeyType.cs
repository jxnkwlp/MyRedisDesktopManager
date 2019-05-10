using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyRedisDesktopManager.Models
{
	public enum RedisType
	{
		None = 0,
		String = 1,
		List = 2,
		Set = 3,
		SortedSet = 4,
		Hash = 5,
		Stream = 6,
		Unknown = 7
	}
}
