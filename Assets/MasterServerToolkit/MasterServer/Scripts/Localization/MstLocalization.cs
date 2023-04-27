using MasterServerToolkit.MasterServer;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace MasterServerToolkit.Localization
{
    public class MstLocalization
    {
        private string selectedLang = "en";
        private Dictionary<string, Dictionary<string, string>> _localization = new Dictionary<string, Dictionary<string, string>>();

        /// <summary>
        /// Current selected language
        /// </summary>
        public string Lang
        {
            get
            {
                return selectedLang;
            }
            set
            {
                string prevLanguage = selectedLang;
                selectedLang = !string.IsNullOrEmpty(value) ? value.ToLower() : "en";

                if (prevLanguage != selectedLang)
                {
                    LanguageChangedEvent?.Invoke(selectedLang);
                }
            }
        }

        /// <summary>
        /// Returns translated string by key
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public string this[string key]
        {
            get
            {
                if (_localization.TryGetValue(selectedLang, out var dictionary) && dictionary != null && dictionary.ContainsKey(key))
                {
                    return dictionary[key];
                }
                else
                {
                    return key;
                }
            }
        }

        /// <summary>
        /// Invoked when the language changes
        /// </summary>
        public event Action<string> LanguageChangedEvent;

        public MstLocalization()
        {
            selectedLang = Mst.Args.AsString(Mst.Args.Names.DefaultLanguage, selectedLang);
            _localization[selectedLang] = new Dictionary<string, string>();
            LoadLocalization();
        }

        private void LoadLocalization()
        {
            var localizationFile = Resources.Load<TextAsset>("Localization/localization");

            if (localizationFile != null && !string.IsNullOrEmpty(localizationFile.text))
            {
                string[] rows = localizationFile.text.Split("\n", StringSplitOptions.RemoveEmptyEntries);
                string[] langCols = rows[0].Split(";", StringSplitOptions.RemoveEmptyEntries);

                for (int i = 1; i < rows.Length; i++)
                {
                    string[] valueCols = rows[i].Trim().Split(";", StringSplitOptions.RemoveEmptyEntries);

                    for (int j = 1; j < valueCols.Length; j++)
                    {
                        RegisterKey(langCols[j].Trim(), valueCols[0].Trim(), valueCols[j].Trim());
                    }
                }
            }
        }

        /// <summary>
        /// Registers localization key-value by given language
        /// </summary>
        /// <param name="lang"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void RegisterKey(string lang, string key, string value)
        {
            string lankValue = lang.ToLower();

            if (!_localization.ContainsKey(lankValue))
                _localization[lankValue] = new Dictionary<string, string>();

            _localization[lankValue][key] = value;
        }
    }
}