using System;
using System.Collections.Generic;

namespace JobsApi.Models;

public partial class SendNotification
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public int JobId { get; set; }

    public string Type { get; set; } = null!;

    public bool IsNotificationSent { get; set; }

    public virtual Job Job { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}

public partial class SendNotificationDTO
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public int JobId { get; set; }

    public string Type { get; set; } = null!;

    public bool IsNotificationSent { get; set; }

}
