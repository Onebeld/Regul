namespace Regul.ModuleSystem;

/// <summary>
/// Language structure
/// </summary>
public struct Language
{
    /// <summary>
    /// Name of language.
    /// <para>
    ///     The first place should display the name in the selected language, and in brackets specify the English name.
    /// </para>
    /// <para>
    ///     For example, Français (French)
    /// </para>
    /// </summary>
    public string Name { get; }
    /// <summary>
    /// Language key.
    /// <para>
    ///     For example, fr
    /// </para>
    /// </summary>
    public string Key { get; }
    /// <summary>
    /// Additional language keys. Some languages use more than one key, so there is such a parameter.
    /// </summary>
    public string[] AdditionalKeys { get; }

    /// <summary>
    /// Creates an instance of <see cref="Language"/>
    /// </summary>
    /// <param name="name">Name of language.</param>
    /// <param name="key">Language key.</param>
    /// <param name="additionalKeys">Additional language keys.</param>
    public Language(string name, string key, params string[] additionalKeys)
    {
        Name = name;
        Key = key;
        AdditionalKeys = additionalKeys;
    }
}