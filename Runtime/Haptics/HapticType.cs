namespace SiPVLib.Vibrate.Haptics
{
    /// <summary>
    /// Provider-agnostic haptic preset. Every <see cref="IHapticProvider"/> maps these onto its own
    /// backend (system haptics, MOST_IN_ONE, a custom rumble driver, etc.). Names mirror the common
    /// iOS/Android impact-and-notification vocabulary so mappings stay lossless on most backends.
    /// </summary>
    public enum HapticType
    {
        None,
        Selection,
        Success,
        Warning,
        Failure,
        LightImpact,
        MediumImpact,
        HeavyImpact,
        RigidImpact,
        SoftImpact
    }
}
