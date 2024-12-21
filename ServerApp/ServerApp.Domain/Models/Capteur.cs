using System;
using System.Collections.Generic;

namespace ServerApp.Domain.Models;

public partial class Capteur
{
    public int Id { get; set; }

    public string? Label { get; set; }

    public string? Type { get; set; }

    public bool? Active { get; set; }

    public DateTime? CreatedAt { get; set; }
}
