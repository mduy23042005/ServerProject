using System;
using System.Collections.Generic;

namespace Server.Models;

public partial class Message
{
    public int Idmessage { get; set; }

    public int IdaccountFrom { get; set; }

    public int IdaccountTo { get; set; }

    public string Contents { get; set; } = null!;

    public virtual Account IdaccountFromNavigation { get; set; } = null!;

    public virtual Account IdaccountToNavigation { get; set; } = null!;
}
