﻿using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using Reusable.OmniLog.Abstractions;
using Reusable.OmniLog.Abstractions.Data;

namespace Reusable.OmniLog.Attachments
{
    public class AppSetting : LogAttachment
    {
        private readonly string _key;

        public AppSetting(string name, string key) : base(name)
        {
            _key = key;
        }

        public override object Compute(Log log)
        {
            return ConfigurationManager.AppSettings[_key];
        }

        public static IEnumerable<AppSetting> CreateMany(string prefix, params string[] keys)
        {
            return
                from key in keys
                select new AppSetting(key, $"{prefix}{key}");
        }        
    }
}