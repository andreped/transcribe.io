// <copyright file="WhisperLanguage.cs" company="Drastic Actions">
// Copyright (c) Drastic Actions. All rights reserved.
// </copyright>

using System.Globalization;
using System.Runtime.CompilerServices;

namespace transcribe.io.Models;

public enum WhisperLanguages : uint
{
    /// <summary>Afrikaans.</summary>
    Afrikaans = 0x6661,  // "af"

    /// <summary>Albanian.</summary>
    Albanian = 0x7173,  // "sq"

    /// <summary>Amharic.</summary>
    Amharic = 0x6D61,  // "am"

    /// <summary>Arabic.</summary>
    Arabic = 0x7261,  // "ar"

    /// <summary>Armenian.</summary>
    Armenian = 0x7968,  // "hy"

    /// <summary>Assamese.</summary>
    Assamese = 0x7361,  // "as"

    /// <summary>Azerbaijani.</summary>
    Azerbaijani = 0x7A61,  // "az"

    /// <summary>Bashkir.</summary>
    Bashkir = 0x6162,  // "ba"

    /// <summary>Basque.</summary>
    Basque = 0x7565,  // "eu"

    /// <summary>Belarusian.</summary>
    Belarusian = 0x6562,  // "be"

    /// <summary>Bengali.</summary>
    Bengali = 0x6E62,  // "bn"

    /// <summary>Bosnian.</summary>
    Bosnian = 0x7362,  // "bs"

    /// <summary>Breton.</summary>
    Breton = 0x7262,  // "br"

    /// <summary>Bulgarian.</summary>
    Bulgarian = 0x6762,  // "bg"

    /// <summary>Catalan.</summary>
    Catalan = 0x6163,  // "ca"

    /// <summary>Chinese.</summary>
    Chinese = 0x687A,  // "zh"

    /// <summary>Croatian.</summary>
    Croatian = 0x7268,  // "hr"

    /// <summary>Czech.</summary>
    Czech = 0x7363,  // "cs"

    /// <summary>Danish.</summary>
    Danish = 0x6164,  // "da"

    /// <summary>Dutch.</summary>
    Dutch = 0x6C6E,  // "nl"

    /// <summary>English.</summary>
    English = 0x6E65,  // "en"

    /// <summary>Estonian.</summary>
    Estonian = 0x7465,  // "et"

    /// <summary>Faroese.</summary>
    Faroese = 0x6F66,  // "fo"

    /// <summary>Finnish.</summary>
    Finnish = 0x6966,  // "fi"

    /// <summary>French.</summary>
    French = 0x7266,  // "fr"

    /// <summary>Galician.</summary>
    Galician = 0x6C67,  // "gl"

    /// <summary>Georgian.</summary>
    Georgian = 0x616B,  // "ka"

    /// <summary>German.</summary>
    German = 0x6564,  // "de"

    /// <summary>Greek.</summary>
    Greek = 0x6C65,  // "el"

    /// <summary>Gujarati.</summary>
    Gujarati = 0x7567,  // "gu"

    /// <summary>Haitian Creole.</summary>
    HaitianCreole = 0x7468,  // "ht"

    /// <summary>Hausa.</summary>
    Hausa = 0x6168,  // "ha"

    /// <summary>Hawaiian.</summary>
    Hawaiian = 0x776168,  // "haw"

    /// <summary>Hebrew.</summary>
    Hebrew = 0x7769,  // "iw"

    /// <summary>Hindi.</summary>
    Hindi = 0x6968,  // "hi"

    /// <summary>Hungarian.</summary>
    Hungarian = 0x7568,  // "hu"

    /// <summary>Icelandic.</summary>
    Icelandic = 0x7369,  // "is"

    /// <summary>Indonesian.</summary>
    Indonesian = 0x6469,  // "id"

    /// <summary>Italian.</summary>
    Italian = 0x7469,  // "it"

    /// <summary>Japanese.</summary>
    Japanese = 0x616A,  // "ja"

    /// <summary>Javanese.</summary>
    Javanese = 0x776A,  // "jw"

    /// <summary>Kannada.</summary>
    Kannada = 0x6E6B,  // "kn"

    /// <summary>Kazakh.</summary>
    Kazakh = 0x6B6B,  // "kk"

    /// <summary>Khmer.</summary>
    Khmer = 0x6D6B,  // "km"

    /// <summary>Korean.</summary>
    Korean = 0x6F6B,  // "ko"

    /// <summary>Lao.</summary>
    Lao = 0x6F6C,  // "lo"

    /// <summary>Latin.</summary>
    Latin = 0x616C,  // "la"

    /// <summary>Latvian.</summary>
    Latvian = 0x766C,  // "lv"

    /// <summary>Lingala.</summary>
    Lingala = 0x6E6C,  // "ln"

    /// <summary>Lithuanian.</summary>
    Lithuanian = 0x746C,  // "lt"

    /// <summary>Luxembourgish.</summary>
    Luxembourgish = 0x626C,  // "lb"

    /// <summary>Macedonian.</summary>
    Macedonian = 0x6B6D,  // "mk"

    /// <summary>Malagasy.</summary>
    Malagasy = 0x676D,  // "mg"

    /// <summary>Malay.</summary>
    Malay = 0x736D,  // "ms"

    /// <summary>Malayalam.</summary>
    Malayalam = 0x6C6D,  // "ml"

    /// <summary>Maltese.</summary>
    Maltese = 0x746D,  // "mt"

    /// <summary>Maori.</summary>
    Maori = 0x696D,  // "mi"

    /// <summary>Marathi.</summary>
    Marathi = 0x726D,  // "mr"

    /// <summary>Mongolian.</summary>
    Mongolian = 0x6E6D,  // "mn"

    /// <summary>Myanmar.</summary>
    Myanmar = 0x796D,  // "my"

    /// <summary>Nepali.</summary>
    Nepali = 0x656E,  // "ne"

    /// <summary>Norwegian.</summary>
    Norwegian = 0x6F6E,  // "no"

    /// <summary>Nynorsk.</summary>
    Nynorsk = 0x6E6E,  // "nn"

    /// <summary>Occitan.</summary>
    Occitan = 0x636F,  // "oc"

    /// <summary>Pashto.</summary>
    Pashto = 0x7370,  // "ps"

    /// <summary>Persian.</summary>
    Persian = 0x6166,  // "fa"

    /// <summary>Polish.</summary>
    Polish = 0x6C70,  // "pl"

    /// <summary>Portuguese.</summary>
    Portuguese = 0x7470,  // "pt"

    /// <summary>Punjabi.</summary>
    Punjabi = 0x6170,  // "pa"

    /// <summary>Romanian.</summary>
    Romanian = 0x6F72,  // "ro"

    /// <summary>Russian.</summary>
    Russian = 0x7572,  // "ru"

    /// <summary>Sanskrit.</summary>
    Sanskrit = 0x6173,  // "sa"

    /// <summary>Serbian.</summary>
    Serbian = 0x7273,  // "sr"

    /// <summary>Shona.</summary>
    Shona = 0x6E73,  // "sn"

    /// <summary>Sindhi.</summary>
    Sindhi = 0x6473,  // "sd"

    /// <summary>Sinhala.</summary>
    Sinhala = 0x6973,  // "si"

    /// <summary>Slovak.</summary>
    Slovak = 0x6B73,  // "sk"

    /// <summary>Slovenian.</summary>
    Slovenian = 0x6C73,  // "sl"

    /// <summary>Somali.</summary>
    Somali = 0x6F73,  // "so"

    /// <summary>Spanish.</summary>
    Spanish = 0x7365,  // "es"

    /// <summary>Sundanese.</summary>
    Sundanese = 0x7573,  // "su"

    /// <summary>Swahili.</summary>
    Swahili = 0x7773,  // "sw"

    /// <summary>Swedish.</summary>
    Swedish = 0x7673,  // "sv"

    /// <summary>Tagalog.</summary>
    Tagalog = 0x6C74,  // "tl"

    /// <summary>Tajik.</summary>
    Tajik = 0x6774,  // "tg"

    /// <summary>Tamil.</summary>
    Tamil = 0x6174,  // "ta"

    /// <summary>Tatar.</summary>
    Tatar = 0x7474,  // "tt"

    /// <summary>Telugu.</summary>
    Telugu = 0x6574,  // "te"

    /// <summary>Thai.</summary>
    Thai = 0x6874,  // "th"

    /// <summary>Tibetan.</summary>
    Tibetan = 0x6F62,  // "bo"

    /// <summary>Turkish.</summary>
    Turkish = 0x7274,  // "tr"

    /// <summary>Turkmen.</summary>
    Turkmen = 0x6B74,  // "tk"

    /// <summary>Ukrainian.</summary>
    Ukrainian = 0x6B75,  // "uk"

    /// <summary>Urdu.</summary>
    Urdu = 0x7275,  // "ur"

    /// <summary>Uzbek.</summary>
    Uzbek = 0x7A75,  // "uz"

    /// <summary>Vietnamese.</summary>
    Vietnamese = 0x6976,  // "vi"

    /// <summary>Welsh.</summary>
    Welsh = 0x7963,  // "cy"

    /// <summary>Yiddish.</summary>
    Yiddish = 0x6979,  // "yi"

    /// <summary>Yoruba.</summary>
    Yoruba = 0x6F79,  // "yo"
}
public class WhisperLanguage
{
    public WhisperLanguage(CultureInfo info)
    {
        this.CultureInfo = info;
        this.Language = info.DisplayName;
        this.LanguageCode = info.IetfLanguageTag;
    }

    public WhisperLanguage(WhisperLanguages language)
    {
        var code = GetCode(language);
        this.CultureInfo = System.Globalization.CultureInfo.GetCultureInfo(code);
        this.Language = this.CultureInfo.DisplayName;
        this.LanguageCode = this.CultureInfo.IetfLanguageTag;
    }

    public WhisperLanguage()
    {
        this.IsAutomatic = true;
        this.CultureInfo = CultureInfo.CurrentCulture;
        this.LanguageCode = CultureInfo.CurrentCulture.TwoLetterISOLanguageName;
        this.Language = "Auto";
    }

    public string Language { get; }

    public bool IsAutomatic { get; }

    public CultureInfo CultureInfo { get; }

    public string LanguageCode { get; }

    public static IReadOnlyList<WhisperLanguage> GenerateWhisperLangauages()
    {
        var list = new List<WhisperLanguage>() { };

        foreach (WhisperLanguages value in Enum.GetValues(typeof(WhisperLanguages)))
        {
            list.Add(new WhisperLanguage(value));
        }

        var orderedList = list.OrderBy(n => n.Language);
        var newList = orderedList.ToList();
        newList.Insert(0, new WhisperLanguage());
        return newList.AsReadOnly();
    }

    [SkipLocalsInit]
    public static string GetCode(WhisperLanguages lang)
    {
        unsafe
        {
            sbyte* ptr = stackalloc sbyte[5];
            *(uint*)ptr = (uint)lang;
            ptr[4] = 0;
            return new string(ptr);
        }
    }
}
