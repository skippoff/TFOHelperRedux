namespace TFOHelperRedux
{
    /// <summary>
    /// Флаг для XAML: true в DEBUG, false в RELEASE.
    /// </summary>
    public static class DebugFlags
    {
        public static bool IsDebug
        {
            get
            {
#if DEBUG
                return true;
#else
                return false;
#endif
            }
        }
    }
}