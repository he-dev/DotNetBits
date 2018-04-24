﻿using System.Collections.Generic;
using Reusable.Collections;

namespace Reusable.OmniLog.Collections
{
    public interface ILog : IDictionary<SoftString, object> { }

    public class Log : PainlessDictionary<SoftString, object>, ILog { }
}