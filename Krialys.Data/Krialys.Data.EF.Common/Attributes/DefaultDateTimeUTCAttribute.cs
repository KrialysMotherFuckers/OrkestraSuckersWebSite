using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Krialys.Data.EF.Attributes;

public class DefaultDateTimeUTCAttribute : DefaultValueAttribute
{
    public DefaultDateTimeUTCAttribute() : base(DateTime.UtcNow)
    {
    }
}