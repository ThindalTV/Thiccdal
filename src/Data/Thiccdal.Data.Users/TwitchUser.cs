using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Thiccdal.Data.Users;
public class TwitchUser : User
{
	public TwitchUser()
	{
		Source = UserSource.Twitch;
	}
}
