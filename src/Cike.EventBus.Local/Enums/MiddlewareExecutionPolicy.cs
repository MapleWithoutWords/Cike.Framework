using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cike.EventBus.Local.Enums;

public enum MiddlewareExecutionPolicy
{
    Always = 1,
    OncePerTree = 2,
}
