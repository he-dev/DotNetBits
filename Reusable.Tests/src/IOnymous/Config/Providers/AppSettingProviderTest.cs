using System.Threading.Tasks;
using Reusable.IOnymous;
using Reusable.IOnymous.Config;
using Reusable.Quickey;
using Xunit;

namespace Reusable.Tests.IOnymous.Config.Providers
{
    public class AppSettingProviderTest
    {
        public AppSettingProviderTest()
        {
            SeedAppSettings();
        }
        
        private static void SeedAppSettings()
        {
            var exeConfiguration = System.Configuration.ConfigurationManager.OpenExeConfiguration(System.Configuration.ConfigurationUserLevel.None);

            exeConfiguration.AppSettings.Settings.Clear();
            //exeConfiguration.ConnectionStrings.ConnectionStrings.Clear();
            
            var data = new (string Key, string Value)[]
            {
                ("app:Environment", "test"),
            };

            foreach (var (key, value) in data)
            {
                exeConfiguration.AppSettings.Settings.Add(key, value);
            }

            exeConfiguration.Save(System.Configuration.ConfigurationSaveMode.Minimal);
        }

        [Fact]
        public async Task Can_get_setting()
        {
            var c = CompositeProvider.Empty.Add(new AppSettingProvider());
            var env = await c.ReadSettingAsync(From<IProgramConfig>.Select(x => x.Environment));
            
            Assert.Equal("test", env);
        }
        
        //[ResourcePrefix("app")]
        //[ResourceName(Level = ResourceNameLevel.Member)]
        [UseScheme("app"), UseMember]
        [SettingSelectorFormatter]
        private interface IProgramConfig
        {
            string Environment { get; }
        }
    }
}