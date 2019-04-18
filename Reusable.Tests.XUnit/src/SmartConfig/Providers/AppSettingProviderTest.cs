using System.Threading.Tasks;
using Reusable.IOnymous;
using Reusable.SmartConfig;
using Xunit;

namespace Reusable.Tests.XUnit.SmartConfig.Providers
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
                ("app:Environments", "test"),
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
            var c = new Configuration<IProgramConfig>(new AppSettingProvider(new UriStringToSettingIdentifierConverter()));
            var env = await c.GetItemAsync(x => x.Environment);
            
            Assert.Equal("test", env);
        }
        
        [ResourcePrefix("app")]
        [ResourceName(Level = ResourceNameLevel.Member)]
        internal interface IProgramConfig
        {
            string Environment { get; }
        }
    }
}