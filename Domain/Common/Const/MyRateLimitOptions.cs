using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Common.Const;

public class MyRateLimitOptions
{
    public const string MyRateLimit = "MyRateLimit";

    public int PermitLimit { get; set; } = 1000;
    public int QueueLimit { get; set; } = 100;
}
