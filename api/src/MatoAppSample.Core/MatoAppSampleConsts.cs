using MatoAppSample.Debugging;

namespace MatoAppSample
{
    public class MatoAppSampleConsts
    {
        public const string LocalizationSourceName = "MatoAppSample";

        public const string ConnectionStringName = "Default";

        public const bool MultiTenancyEnabled = true;


        /// <summary>
        /// Default pass phrase for SimpleStringCipher decrypt/encrypt operations
        /// </summary>
        public static readonly string DefaultPassPhrase =
            DebugHelper.IsDebug ? "gsKxGZ012HLL3MI5" : "6bdcaeee00f84612a6fa171d1016128c";
    }
}
