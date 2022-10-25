using Abp.Configuration.Startup;
using Abp.Localization.Dictionaries;
using Abp.Localization.Dictionaries.Xml;
using Abp.Reflection.Extensions;

namespace MatoAppSample.Localization
{
    public static class MatoAppSampleLocalizationConfigurer
    {
        public static void Configure(ILocalizationConfiguration localizationConfiguration)
        {
            localizationConfiguration.Sources.Add(
                new DictionaryBasedLocalizationSource(MatoAppSampleConsts.LocalizationSourceName,
                    new XmlEmbeddedFileLocalizationDictionaryProvider(
                        typeof(MatoAppSampleLocalizationConfigurer).GetAssembly(),
                        "MatoAppSample.Localization.SourceFiles"
                    )
                )
            );
        }
    }
}
