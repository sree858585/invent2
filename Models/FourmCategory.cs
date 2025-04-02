using System;
using System.Collections.Generic;

namespace HIVTraining_Vue.Server.Models;

public partial class FourmCategory
{
    public int CatId { get; set; }

    public string CatName { get; set; } = null!;

    public string? Description { get; set; }
}
