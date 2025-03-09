using System;
using System.Collections.Generic;

namespace JobsApi.Models;

public partial class User
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string PhoneNumber { get; set; } = null!;

    public string EmailId { get; set; } = null!;

    public string Password { get; set; } = null!;

    public int? RoleId { get; set; }

    public string? OrganisationName { get; set; }

    public string? Qualification { get; set; }

    public DateTime LastUpdatedTime { get; set; }

    public string? SubscriptionType { get; set; }

    public bool IsSubscribed { get; set; }

    public string? Description { get; set; }

    public string? Location { get; set; }

    public virtual Role? Role { get; set; }

    public virtual ICollection<SendNotification> SendNotifications { get; set; } = new List<SendNotification>();
}
