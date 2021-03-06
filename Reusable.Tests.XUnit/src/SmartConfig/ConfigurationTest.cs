using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Reusable.Data;
using Reusable.IOnymous;
using Reusable.SmartConfig;
using Xunit;
using cfg = Reusable.SmartConfig.Configuration;

//[assembly: SettingProvider(SettingNameStrength.Medium, Prefix = "TestPrefix")]
//[assembly: SettingProvider(SettingNameStrength.Low, typeof(AppSettingProvider), Prefix = "abc")]
//[assembly: SettingProvider(SettingNameStrength.Low, nameof(AppSettingProvider), Prefix = "abc")]

/*
 
  - can find setting by name
  - can find first setting by name
  - can find setting by name and provider
  - can find setting by name and provider-type 
  - can override type-name
  - can override member-name
  - can override setting name length
  - can add setting name prefix
  - can disable setting name prefix
  - can find setting for base type
  
 */
namespace Reusable.Tests.XUnit.SmartConfig
{
    public class ConfigurationTest
    {
        [Fact]
        public void Can_get_setting_by_name()
        {
            var u = new User
            {
                Configuration = new cfg(new InMemoryProvider(new UriStringToSettingIdentifierConverter(), new SoftString[] { "setting" })
                {
                    { "User.Name", "Bob" }
                })
            };
            Assert.Equal("Bob", u.Name);
        }

        [Fact]
        public void Can_get_first_setting_by_name()
        {
            var u = new User
            {
                Configuration = new cfg(new CompositeProvider(new IResourceProvider[]
                {
                    new InMemoryProvider(new UriStringToSettingIdentifierConverter(), new SoftString[] { "setting" })
                    {
                        { "Person.Name", "Joe" }
                    },
                    new InMemoryProvider(new UriStringToSettingIdentifierConverter(), new SoftString[] { "setting" })
                    {
                        { "User.Name", "Bob" }
                    },
                    new InMemoryProvider(new UriStringToSettingIdentifierConverter(), new SoftString[] { "setting" })
                    {
                        { "User.Name", "Tom" }
                    }
                }))
            };
            Assert.Equal("Bob", u.Name);
        }

        [Fact]
        public void Can_find_setting_by_provider()
        {            
            var u = new Map
            {
                Configuration = new cfg(new CompositeProvider(new IResourceProvider[]
                {
                    new InMemoryProvider(new UriStringToSettingIdentifierConverter(), new SoftString[] { "setting" }, ImmutableSession.Empty.Set(Use<IProviderNamespace>.Namespace,x => x.CustomName, "OtherOne"))
                    {
                        { "Map.City", "Joe" }
                    },
                    new InMemoryProvider(new UriStringToSettingIdentifierConverter(), new SoftString[] { "setting" })
                    {
                        { "Map.City", "Tom" }
                    },
                    new InMemoryProvider(new UriStringToSettingIdentifierConverter(), new SoftString[] { "setting" }, ImmutableSession.Empty.Set(Use<IProviderNamespace>.Namespace,x => x.CustomName, "ThisOne"))
                    {
                        { "Map.City", "Bob" }
                    },
                }))
            };
            Assert.Equal("Bob", u.City);
        }

        [Fact]
        public void Can_find_setting_by_provider_type() { }

        [Fact]
        public void Can_override_type_and_member_names()
        {
            var u = new Forest
            {
                Configuration = new cfg(new CompositeProvider(new IResourceProvider[]
                {
                    new InMemoryProvider(new UriStringToSettingIdentifierConverter(), new SoftString[] { "setting" })
                    {
                        { "User.Name", "Joe" }
                    },
                    new InMemoryProvider(new UriStringToSettingIdentifierConverter(), new SoftString[] { "setting" })
                    {
                        { "Forest.Tree", "Tom" }
                    },
                    new InMemoryProvider(new UriStringToSettingIdentifierConverter(), new SoftString[] { "setting" })
                    {
                        { "Amazon.Timber", "Bob" }
                    },
                }))
            };
            Assert.Equal("Bob", u.Tree);
        }

        [Fact]
        public void Can_override_setting_name_length()
        {
            var u = new Key
            {
                Configuration = new cfg(new CompositeProvider(new IResourceProvider[]
                {
                    new InMemoryProvider(new UriStringToSettingIdentifierConverter(), new SoftString[] { "setting" })
                    {
                        { "User.Name", "Joe" }
                    },
                    new InMemoryProvider(new UriStringToSettingIdentifierConverter(), new SoftString[] { "setting" })
                    {
                        { "Reusable.Tests.XUnit.SmartConfig+Key.Location", "Tom" }
                    },
                    new InMemoryProvider(new UriStringToSettingIdentifierConverter(), new SoftString[] { "setting" })
                    {
                        { "Door", "Bob" }
                    },
                }))
            };
            Assert.Equal("Tom", u.Location);
            Assert.Equal("Bob", u.Door);
        }

        [Fact]
        public void Can_use_setting_name_prefix()
        {
            var u = new Greeting
            {
                Configuration = new cfg(new CompositeProvider(new IResourceProvider[]
                {
                    new InMemoryProvider(new UriStringToSettingIdentifierConverter(), new SoftString[] { "setting" })
                    {
                        { "User.Name", "Joe" }
                    },
                    new InMemoryProvider(new UriStringToSettingIdentifierConverter(), new SoftString[] { "setting" })
                    {
                        { "day:Greeting.Morning", "Bob" }
                    },
                    new InMemoryProvider(new UriStringToSettingIdentifierConverter(), new SoftString[] { "setting" })
                    {
                        { "Greeting.Morning", "Tom" }
                    },
                }))
            };
            Assert.Equal("Bob", u.Morning);
        }

        [Fact]
        public async Task Can_use_setting_with_handle()
        {
            var c = new Configuration<User>(new CompositeProvider(new IResourceProvider[]
            {
                new InMemoryProvider(new UriStringToSettingIdentifierConverter(), new SoftString[] { "setting" })
                {
                    { "User.Name", "Joe" }
                },
                new InMemoryProvider(new UriStringToSettingIdentifierConverter(), new SoftString[] { "setting" })
                {
                    { "User.Name,this", "Bob" }
                },
                new InMemoryProvider(new UriStringToSettingIdentifierConverter(), new SoftString[] { "setting" })
                {
                    { "Greeting.Morning", "Tom" }
                },
            }));
            Assert.Equal("Bob", await c.GetItemAsync(x => x.Name, "this"));
        }

        [Fact]
        public void Can_find_setting_on_base_type()
        {
            var u = new Admin
            {
                Configuration = new cfg(new CompositeProvider(new IResourceProvider[]
                {
                    new InMemoryProvider(new UriStringToSettingIdentifierConverter(), new SoftString[] { "setting" })
                    {
                        { "User.Name", "Joe" }
                    },
                    new InMemoryProvider(new UriStringToSettingIdentifierConverter(), new SoftString[] { "setting" })
                    {
                        { "Admin.Enabled", true }
                    },
                    new InMemoryProvider(new UriStringToSettingIdentifierConverter(), new SoftString[] { "setting" })
                    {
                        { "Admin.Skill", "Tom" }
                    },
                }))
            };
            Assert.Equal(true, u.Enabled);
            Assert.Equal("Tom", u.Skill);
        }

        [Fact]
        public async Task Can_save_setting()
        {
            var c = new Configuration<Admin>(new CompositeProvider(new IResourceProvider[]
            {
                new InMemoryProvider(new UriStringToSettingIdentifierConverter(), new SoftString[] { "setting" })
                {
                    { "Admin.Name", "Joe" },
                    { "Admin.Enabled", true }
                }
            }));

            Assert.Equal("Joe", await c.GetItemAsync(x => x.Name));
            Assert.Equal(true, await c.GetItemAsync(x => x.Enabled));

            await c.SetItemAsync(x => x.Name, "Tom");
            await c.SetItemAsync(x => x.Enabled, false);

            Assert.Equal("Tom", await c.GetItemAsync(x => x.Name));
            Assert.Equal(false, await c.GetItemAsync(x => x.Enabled));
        }
    }


    internal class Nothing
    {
        public IConfiguration Configuration { get; set; }

        public bool Enabled => Configuration.GetItem(() => Enabled);
    }

    // tests defaults
    internal class User : Nothing
    {
        public string Name => Configuration.GetItem(() => Name);
    }

    internal class Admin : User
    {
        public string Skill => Configuration.GetItem(() => Skill);
    }

    [ResourceName("Amazon")]
    internal class Forest : Nothing
    {
        [ResourceName("Timber")]
        public string Tree => Configuration.GetItem(() => Tree);
    }

    [ResourcePrefix("day")]
    internal class Greeting : Nothing
    {
        public string Morning => Configuration.GetItem(() => Morning);
    }

    // tests assembly annotations -- they are no longer supported
    // internal class Test6 : Nothing
    // {
    //     public string Member1 { get; set; }
    //
    //     //[SettingMember(Strength = SettingNameStrength.Low, PrefixHandling = PrefixHandling.Disable)]
    //     public string Member2 { get; set; }
    // }

    [ResourceProvider("ThisOne")]
    internal class Map : Nothing
    {
        public string City => Configuration.GetItem(() => City);
    }

    [ResourceName(Level = ResourceNameLevel.NamespaceTypeMember)]
    internal class Key : Nothing
    {
        public string Location => Configuration.GetItem(() => Location);

        [ResourceName(Level = ResourceNameLevel.Member)]
        public string Door => Configuration.GetItem(() => Door);
    }

    internal class CustomTypes : Nothing
    {
        public TimeSpan TimeSpan => Configuration.GetItem(() => TimeSpan);
    }

    public class ConfigurationTestGeneric
    {
        [Fact]
        public async Task Can_find_setting_on_base_type()
        {
            var c = new Reusable.SmartConfig.Configuration<ISubConfig>(new CompositeProvider(new IResourceProvider[]
            {
                new InMemoryProvider(new UriStringToSettingIdentifierConverter(), new SoftString[] { "setting" })
                {
                    { "User.Name", "Joe" }
                },
                new InMemoryProvider(new UriStringToSettingIdentifierConverter(), new SoftString[] { "setting" })
                {
                    { "root:Sub.Enabled", true }
                },
                new InMemoryProvider(new UriStringToSettingIdentifierConverter(), new SoftString[] { "setting" })
                {
                    { "Sub.Name", "Tom" }
                },
            }));

            Assert.Equal(true, await c.GetItemAsync(x => x.Enabled));
            Assert.Equal("Tom", await c.GetItemAsync(x => x.Name));
        }
    }

    [ResourcePrefix("root")]
    public interface IBaseConfig
    {
        bool Enabled { get; }
    }

    public interface ISubConfig : IBaseConfig
    {
        [ResourcePrefix("")]
        string Name { get; }
    }

    public interface ITypeConfig
    {
        string String { get; }
        bool Bool { get; }
        int Int { get; }
        double Double { get; }
        DateTime DateTime { get; }
        TimeSpan TimeSpan { get; }
        List<int> ListOfInt { get; }

        int Edit { get; }
    }
}