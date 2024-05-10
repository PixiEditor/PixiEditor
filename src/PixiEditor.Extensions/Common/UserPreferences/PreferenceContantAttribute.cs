namespace PixiEditor.Extensions.Common.UserPreferences;

[AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Property | AttributeTargets.Field)]
public abstract class PreferenceConstantAttribute : Attribute
{ }

public class LocalPreferenceConstantAttribute : PreferenceConstantAttribute
{ }

public class SyncedPreferenceConstantAttribute : PreferenceConstantAttribute
{ }
