using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Krialys.Orkestra.Common.Models.Admin;
public class UpdateDetails
{
    public Version UpdateVersionNumber { get; set; }
    public bool IsUpdateAvailable { get; set; }
}
