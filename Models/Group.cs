using System;
using System.Collections.Generic;

namespace Server.Models;

public partial class Group
{
    public int Idgroup { get; set; }

    public int Idaccount { get; set; }

    public virtual ICollection<GroupMessage> GroupMessages { get; set; } = new List<GroupMessage>();

    public virtual Account IdaccountNavigation { get; set; } = null!;
}
