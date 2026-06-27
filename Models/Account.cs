using System;
using System.Collections.Generic;

namespace Server.Models;

public partial class Account
{
    public int Idaccount { get; set; }

    public string Username { get; set; } = null!;

    public string Password { get; set; } = null!;

    public string Role { get; set; } = null!;

    public string State { get; set; } = null!;

    public virtual ICollection<GroupMessage> GroupMessages { get; set; } = new List<GroupMessage>();

    public virtual ICollection<Group> Groups { get; set; } = new List<Group>();

    public virtual ICollection<Message> MessageIdaccountFromNavigations { get; set; } = new List<Message>();

    public virtual ICollection<Message> MessageIdaccountToNavigations { get; set; } = new List<Message>();
}
