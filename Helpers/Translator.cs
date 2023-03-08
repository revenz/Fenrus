using System.Dynamic;
using System.Text.RegularExpressions;
using Fenrus.Converters;
using Jeffijoe.MessageFormat;

namespace Fenrus.Helpers;

/// <summary>
/// Translator is responsible for language translations
/// </summary>
public class Translator
{
    private MessageFormatter Formatter;
    private Dictionary<string, string> Language { get; set; } = new Dictionary<string, string>();
    private static Regex rgxNeedsTranslating = new Regex(@"^([\w\d_\-]+\.)+[\w\d_\-]+$");

    private static Dictionary<string, Translator> Translators = new();
    /// <summary>
    /// Gets a translator for a given language
    /// </summary>
    /// <param name="language">the language</param>
    /// <returns>an instance of the translator</returns>
    public static Translator GetForLanguage(string language)
    {
        language ??= "en";
        if (Translators.ContainsKey(language))
            return Translators[language];
        var translator = new Translator(language);
        Translators.TryAdd(language, translator);
        return translator;
    }

    /// <summary>
    /// Constructs a new translator
    /// </summary>
    /// <param name="language">the translator</param>
    public Translator(string language)
    {
        string[] jsonFiles;
        if (language != "en" && Regex.IsMatch(language, "^[a-z]+$") && File.Exists("i18n/" + language + ".json"))
            jsonFiles = new [] { "i18n/en.json", $"i18n/{language}.json" };
        else
            jsonFiles = new[] { "i18n/en.json" };

        Init(jsonFiles);
    }

    /// <summary>
    /// Basic translator used to avoid null exceptions
    /// </summary>
    public Translator()
    {
        Formatter = new MessageFormatter();
    }

    private static Dictionary<string, string> _AvailableLanguages;
    /// <summary>
    /// Gets all available languages
    /// </summary>
    /// <returns></returns>
    public static Dictionary<string, string> GetLanguages()
    {
        if (_AvailableLanguages == null)
        {
            var dict = new Dictionary<string, string>();
            foreach (var file in new DirectoryInfo("i18n").GetFiles("*.json"))
            {
                try
                {
                    var json = File.ReadAllText(file.FullName);
                    var eo = JsonSerializer.Deserialize<ExpandoObject>(json) as IDictionary<string, object>;
                    if (eo.ContainsKey("Language"))
                    {
                        var lang = eo["Language"].ToString();
                        if (string.IsNullOrWhiteSpace(lang) == false && dict.ContainsKey(lang) == false)
                            dict.Add(lang, file.Name.Substring(0, file.Name.LastIndexOf(file.Extension, StringComparison.Ordinal)));
                    }
                }
                catch (Exception)
                {
                    Console.WriteLine("Failed to parse language file: " + file);
                }
            }
            _AvailableLanguages = dict;
        }
        return _AvailableLanguages;
    }


    /// <summary>
    /// Translates a string if the string needs translating
    /// </summary>
    /// <param name="value">The string to translate if needed</param>
    /// <returns>The translated string if needed, otherwise the original string</returns>
    public string TranslateIfNeeded(string value)
    {
        if (string.IsNullOrEmpty(value))
            return string.Empty;
        if (NeedsTranslating(value) == false)
            return value;
        return Instant(value);
    }

    /// <summary>
    /// Gets if the translator has been initialized
    /// </summary>
    public bool InitDone => Formatter != null;

    /// <summary>
    /// Checks if a string needs translating
    /// </summary>
    /// <param name="label">The string to test</param>
    /// <returns>if the string needs to be translated or not</returns>
    public bool NeedsTranslating(string label) => rgxNeedsTranslating.IsMatch(label ?? "");
    
    /// <summary>
    /// Initializes the translator
    /// </summary>
    /// <param name="jsonFiles">a list of translation json files</param>
    public void Init(params string[] jsonFiles)
    {
        Formatter = new MessageFormatter();

        foreach (var file in jsonFiles)
        {
            try
            {
                string json = System.IO.File.ReadAllText(file);
                var dict = DeserializeAndFlatten(json);
                foreach (var key in dict.Keys)
                {
                    if (Language.ContainsKey(key))
                        Language[key] = dict[key];
                    else
                        Language.Add(key, dict[key]);
                }
            }
            catch (Exception) { }
        }

        Logger.DLog("Language keys found: " + Language.Keys.Count);
    }
    
    /// <summary>
    /// Deserialized a json string and flattens it into a Dictionary using dot notation
    /// </summary>
    /// <param name="json">The json string to flatten</param>
    /// <returns>a dictionary of the flatten json string</returns>
    private static Dictionary<string, string> DeserializeAndFlatten(string json)
    {
        Dictionary<string, string> dict = new Dictionary<string, string>();
        var options = new JsonSerializerOptions
        {
            AllowTrailingCommas = true,
            Converters = { new LanguageConverter() }
        };
        dynamic d = JsonSerializer.Deserialize<ExpandoObject>(json, options) ?? throw new InvalidOperationException();
        FillDictionaryFromExpando(dict, d, "");
        return dict;
    }

    private static void FillDictionaryFromExpando(Dictionary<string, string> dict, ExpandoObject expando, string prefix)
    {
        IDictionary<string, object> dictExpando = expando as IDictionary<string, object>;
        foreach (string key in dictExpando.Keys)
        {
            if (dictExpando[key] is ExpandoObject eo)
            {
                FillDictionaryFromExpando(dict, eo, Join(prefix, key));
            }
            else if (dictExpando[key] is string str)
                dict.Add(Join(prefix, key), str);

        }
    }
    private static string Join(string prefix, string name)
    {
        return (string.IsNullOrEmpty(prefix) ? name : prefix + "." + name);
    }

    private string Lookup(string[] possibleKeys, bool supressWarnings = false)
    {
        foreach (string key in possibleKeys)
        {
            if (String.IsNullOrWhiteSpace(key))
                continue;
            if (Language.ContainsKey(key))
                return Language[key];
        }
        if (possibleKeys[0].EndsWith("-Help") || possibleKeys[0].EndsWith("-Placeholder") || possibleKeys[0].EndsWith("-Suffix") || possibleKeys[0].EndsWith("-Prefix") || possibleKeys[0].EndsWith(".Description"))
            return "";

        if (possibleKeys[0].EndsWith(".Name") && Language.ContainsKey("Labels.Name"))
            return Language["Labels.Name"];

        string result = possibleKeys?.FirstOrDefault() ?? "";
        if(supressWarnings == false)
            Logger.WLog("Failed to lookup key: " + result);
        result = result.Substring(result.LastIndexOf(".") + 1);

        return result;
    }

    /// <summary>
    /// Translates a string
    /// </summary>
    /// <param name="key">The string to translate</param>
    /// <param name="parameters">any translation parameters</param>
    /// <param name="supressWarnings">if translation warnings should be suppressed and not printed to the log</param>
    /// <returns>the translated string</returns>
    public string Instant(string key, object? parameters = null, bool supressWarnings = false)
        => Instant(new[] { key }, parameters, supressWarnings: supressWarnings);

    /// <summary>
    /// Attempts to translate from a range of possible keys.
    /// The first key found in the translation dictionary will be returned
    /// </summary>
    /// <param name="possibleKeys">a list of possible translation keys</param>
    /// <param name="parameters">any translation parameters</param>
    /// <param name="supressWarnings">if translation warnings should be suppressed and not printed to the log</param>
    /// <returns>the translated string</returns>
    public string Instant(string[] possibleKeys, object? parameters = null, bool supressWarnings = false)
    {
        try
        {
            string msg = Lookup(possibleKeys, supressWarnings: supressWarnings);
            if (msg == "")
                return "";
            if (parameters is IDictionary<string, object?> dict)
                return Formatter.FormatMessage(msg, dict);

            return Formatter.FormatMessage(msg, parameters ?? new { });
        }
        catch (Exception ex)
        {
            if(supressWarnings == false)
                Logger.WLog("Failed to translating key: " + possibleKeys[0] + ", " + ex.Message);
            return possibleKeys[0];
        }
    }

    /// <summary>
    /// Translates a string if there is a translation for it
    /// </summary>
    /// <param name="key">the key to translate</param>
    /// <param name="default">the default string to return if no translation is found</param>
    /// <returns>the translated string or default if not found</returns>
    public string TranslateIfHasTranslation(string key, string @default)
    {
        try
        {
            if (Language.ContainsKey(key) == false)
                return @default;
            string msg = Language[key];
            return Formatter.FormatMessage(msg, new { });
        }
        catch (Exception)
        {
            return @default;
        }
    }
}