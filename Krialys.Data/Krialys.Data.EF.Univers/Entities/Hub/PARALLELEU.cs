using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace Krialys.Data.EF.Univers;

[Keyless]
public partial class PARALLELEU
{
    [Editable(false)]
    public int Id { get; set; }
}