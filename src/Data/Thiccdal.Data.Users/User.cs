using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Thiccdal.Data.Users;
public class User
{
    public string Name { get; set; }
    public string DisplayName { get; set; }
    public UserSource Source { get; protected set; }
    public List<string> Channels { get; set; }
}
