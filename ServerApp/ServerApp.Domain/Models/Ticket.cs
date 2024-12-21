using System;
using System.Collections.Generic;

namespace ServerApp.Domain.Models;

public partial class Ticket
{
    public int Id { get; set; }

    public string? Description { get; set; }

    public byte? Status { get; set; }

    public DateTime? CreatedAt { get; set; }
}
