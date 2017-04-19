using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32;
using Reusable.ConfigWhiz.Data;
using Reusable.Extensions;

namespace Reusable.ConfigWhiz.Datastores
{
    public class Registry : Datastore
    {
        private readonly RegistryKey _baseKey;
        private readonly string _baseSubKeyName;

        private readonly IReadOnlyDictionary<Type, RegistryValueKind> _registryValueKinds = new Dictionary<Type, RegistryValueKind>
        {
            { typeof(string), RegistryValueKind.String },
            { typeof(int), RegistryValueKind.DWord },
            { typeof(byte[]), RegistryValueKind.Binary },
        };

        public Registry(string name, RegistryKey baseKey, string subKey)
            : base(name, new[]
            {
                typeof(int),
                typeof(byte[]),
                typeof(string)
            })
        {
            _baseKey = baseKey ?? throw new ArgumentNullException(nameof(baseKey));
            _baseSubKeyName = subKey.NullIfEmpty() ?? throw new ArgumentNullException(nameof(subKey));
        }

        public Registry(RegistryKey baseKey, string subKey) :this(CreateDefaultName<Registry>(), baseKey, subKey) { }

        public override ICollection<ISetting> Read(SettingPath settingPath)
        {
            var subKeyName = Path.Combine(_baseSubKeyName, string.Join("\\", settingPath.ConsumerNamespace));
            using (var subKey = _baseKey.OpenSubKey(subKeyName, false))
            {
                if (subKey == null) throw new SubKeyException(_baseKey.Name, _baseSubKeyName, subKeyName);

                var shortWeakPath = settingPath.ToShortWeakString();
                var settings =
                    from valueName in subKey.GetValueNames()
                    let valuePath = SettingPath.Parse(valueName)
                    where valuePath.ToShortWeakString().Equals(shortWeakPath, StringComparison.OrdinalIgnoreCase)
                    select new Setting
                    {
                        Path = valuePath,
                        Value = subKey.GetValue(valueName)
                    };
                return settings.Cast<ISetting>().ToList();
            }
        }

        public override int Write(IGrouping<SettingPath, ISetting> settings)
        {
            var settingsAffected = 0;

            void DeleteObsoleteSettings(RegistryKey registryKey)
            {
                var obsoleteNames =
                    from valueName in registryKey.GetValueNames()
                    where SettingPath.Parse(valueName).ToShortWeakString().Equals(settings.Key.ToShortWeakString(), StringComparison.OrdinalIgnoreCase)
                    select valueName;

                foreach (var obsoleteName in obsoleteNames)
                {
                    registryKey.DeleteValue(obsoleteName);
                    settingsAffected++;
                }
            }

            var subKeyName = Path.Combine(_baseSubKeyName, string.Join("\\", settings.Key.ConsumerNamespace));
            using (var subKey = _baseKey.OpenSubKey(subKeyName, true) ?? _baseKey.CreateSubKey(subKeyName))
            {
                if (subKey == null) throw new SubKeyException(_baseKey.Name, _baseSubKeyName, subKeyName);

                DeleteObsoleteSettings(subKey);

                foreach (var setting in settings)
                {
                    if (!_registryValueKinds.TryGetValue(setting.Value.GetType(), out RegistryValueKind registryValueKind))
                    {
                        throw new InvalidTypeException(setting.Value.GetType(), SupportedTypes);
                    }

                    subKey.SetValue(setting.Path.ToShortStrongString(), setting.Value, registryValueKind);
                    settingsAffected++;
                }
            }
            return settingsAffected;
        }

        //public const string DefaultKey = @"Software\SmartConfig";

        public static Registry CreateForCurrentUser(string name, string subRegistryKey)
        {
            return new Registry(name, Microsoft.Win32.Registry.CurrentUser, subRegistryKey);
        }

        public static Registry CreateForClassesRoot(string name, string subRegistryKey)
        {
            return new Registry(name, Microsoft.Win32.Registry.ClassesRoot, subRegistryKey);
        }

        public static Registry CreateForCurrentConfig(string name, string subRegistryKey)
        {
            return new Registry(name, Microsoft.Win32.Registry.CurrentConfig, subRegistryKey);
        }

        public static Registry CreateForLocalMachine(string name, string subRegistryKey)
        {
            return new Registry(name, Microsoft.Win32.Registry.LocalMachine, subRegistryKey);
        }

        public static Registry CreateForUsers(string name, string subRegistryKey)
        {
            return new Registry(name, Microsoft.Win32.Registry.Users, subRegistryKey);
        }
    }


}