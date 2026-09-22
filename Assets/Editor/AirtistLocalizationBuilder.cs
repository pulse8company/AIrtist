using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Xml.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.Localization;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Localization;
using UnityEngine.Localization.Components;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.Tables;

namespace Airtist.Prototype.Editor
{
    public static class AirtistLocalizationBuilder
    {
        public const string Table = "AIrtistUI";
        public const string FontPath = "Assets/Fonts/NotoSerif/NotoSerif UI.asset";
        public static Dictionary<string, string> ReadWorkbook()
        {
            var result = new Dictionary<string, string>();
            using var file = File.OpenRead("Localization/AIrtist_Localization.xlsx");
            using var zip = new ZipArchive(file, ZipArchiveMode.Read);
            XNamespace ns = "http://schemas.openxmlformats.org/spreadsheetml/2006/main";
            var shared = new List<string>();
            var strings = zip.GetEntry("xl/sharedStrings.xml");
            if (strings != null)
            {
                using var stream = strings.Open();
                shared = XDocument.Load(stream).Descendants(ns + "si").Select(s => string.Concat(s.Descendants(ns + "t").Select(t => t.Value))).ToList();
            }
            using var sheet = zip.GetEntry("xl/worksheets/sheet1.xml").Open();
            foreach (var row in XDocument.Load(sheet).Descendants(ns + "row"))
            {
                if ((int)row.Attribute("r") <= 6) continue;
                string Cell(string col)
                {
                    var cell = row.Elements(ns + "c").FirstOrDefault(c => ((string)c.Attribute("r")).TrimEnd("0123456789".ToCharArray()) == col);
                    if (cell == null) return "";
                    var value = cell.Element(ns + "v")?.Value ?? "";
                    return (string)cell.Attribute("t") == "s" ? shared[int.Parse(value)] :
                        (string)cell.Attribute("t") == "inlineStr" ? string.Concat(cell.Descendants(ns + "t").Select(t => t.Value)) : value;
                }
                string key = Cell("A"), value = Cell("D");
                if (string.IsNullOrWhiteSpace(key)) continue;
                if (string.IsNullOrWhiteSpace(value) || result.ContainsKey(key)) throw new InvalidDataException("Empty or duplicate localization key: " + key);
                result.Add(key, value);
            }
            // This is a content counter, not translated prose; keep the workbook key and derive its numbers.
            if (result.ContainsKey("home.route.summary"))
                result["home.route.summary"] = AirtistLandscapePrototypeController.RouteSummary;
            return result;
        }
        public static string Build()
        {
            if (EditorApplication.isPlaying) throw new InvalidOperationException("Stop Play first.");
            if (!AssetDatabase.IsValidFolder("Assets/Localization")) AssetDatabase.CreateFolder("Assets", "Localization");
            var settings = LocalizationEditorSettings.ActiveLocalizationSettings;
            if (!settings)
            {
                settings = ScriptableObject.CreateInstance<LocalizationSettings>();
                AssetDatabase.CreateAsset(settings, "Assets/Localization/LocalizationSettings.asset");
                LocalizationEditorSettings.ActiveLocalizationSettings = settings;
            }
            var ru = LocalizationEditorSettings.GetLocales().FirstOrDefault(l => l.Identifier.Code == "ru");
            if (!ru)
            {
                ru = Locale.CreateLocale("ru");
                AssetDatabase.CreateAsset(ru, "Assets/Localization/Russian.asset");
                LocalizationEditorSettings.AddLocale(ru);
            }
            settings.GetStartupLocaleSelectors().Clear();
            settings.GetStartupLocaleSelectors().Add(new SpecificLocaleSelector { LocaleId = ru.Identifier });
            var collection = LocalizationEditorSettings.GetStringTableCollection(Table) ??
                LocalizationEditorSettings.CreateStringTableCollection(Table, "Assets/Localization");
            var table = collection.GetTable("ru") as StringTable;
            if (!table) table = collection.AddNewTable("ru") as StringTable;
            var values = ReadWorkbook();
            foreach (var entry in values)
            {
                var item = table.GetEntry(entry.Key) ?? table.AddEntry(entry.Key, entry.Value);
                item.Value = entry.Value;
            }
            EditorUtility.SetDirty(settings); EditorUtility.SetDirty(table); EditorUtility.SetDirty(collection.SharedData); EditorUtility.SetDirty(collection);
            LocalizationEditorSettings.EditorEvents.RaiseCollectionModified(null, collection);
            if (LocalizationEditorSettings.GetAssetTableCollection("AIrtistAssets") == null)
                LocalizationEditorSettings.CreateAssetTableCollection("AIrtistAssets", "Assets/Localization");
            var font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FontPath);
            if (!font)
            {
                font = TMP_FontAsset.CreateFontAsset(AssetDatabase.LoadAssetAtPath<Font>("Assets/Fonts/NotoSerif/NotoSerif-Regular.ttf"), 90, 9,
                    UnityEngine.TextCore.LowLevel.GlyphRenderMode.SDFAA, 2048, 2048);
                font.name = "NotoSerif UI";
                font.atlasPopulationMode = AtlasPopulationMode.Dynamic;
                font.isMultiAtlasTexturesEnabled = true;
                AssetDatabase.CreateAsset(font, FontPath);
                font.TryAddCharacters(string.Concat(values.Values) + "0123456789+-ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz");
                foreach (var atlas in font.atlasTextures) if (!AssetDatabase.Contains(atlas)) AssetDatabase.AddObjectToAsset(atlas, font);
                if (!AssetDatabase.Contains(font.material)) AssetDatabase.AddObjectToAsset(font.material, font);
                font.material.mainTexture = font.atlasTexture;
                EditorUtility.SetDirty(font); EditorUtility.SetDirty(font.material);
            }
            AssetDatabase.SaveAssets();
            if (!AssetDatabase.LoadAllAssetsAtPath(FontPath).OfType<Material>().Any()) throw new InvalidOperationException("Font material not saved.");
            return $"Imported {values.Count} Russian keys from Excel; RU only, EN/FR deliberately not enabled.";
        }
        public static LocalizeStringEvent Bind(TextMeshProUGUI text, string key)
        {
            var binding = text.gameObject.GetComponent<LocalizeStringEvent>() ?? text.gameObject.AddComponent<LocalizeStringEvent>();
            binding.StringReference = new LocalizedString(Table, key);
            while (binding.OnUpdateString.GetPersistentEventCount() > 0) UnityEventTools.RemovePersistentListener(binding.OnUpdateString, 0);
            var setter = (UnityAction<string>)Delegate.CreateDelegate(typeof(UnityAction<string>), text, "set_text");
            UnityEventTools.AddPersistentListener(binding.OnUpdateString, setter);
            binding.OnUpdateString.SetPersistentListenerState(0, UnityEventCallState.EditorAndRuntime);
            binding.RefreshString();
            return binding;
        }
    }
}
