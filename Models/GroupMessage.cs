using System;
using System.Collections.Generic;

namespace Server.Models;

public partial class GroupMessage
{
    public int IdgroupMessage { get; set; }

    public int Idgroup { get; set; }

    public int IdaccountFrom { get; set; }

    public string Contents { get; set; } = null!;

    public virtual Account IdaccountFromNavigation { get; set; } = null!;

    public virtual Group IdgroupNavigation { get; set; } = null!;
}
