
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MSPlayground.Core.UI
{
    /// <summary>
    /// Panel model for spawning complex panels
    /// </summary>
    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public struct PanelModel
    {
        // Ignore property list when iterating through arbitrarily-named localization key properties
        static string[] KEYPATH_IGNORE_LIST = {"id", "prefab", "override"};
        
        [JsonProperty("id")] public string ID;
        /// <summary>
        /// Path to the prefab
        /// </summary>
        [JsonProperty("prefab")] public string PrefabPath;
        /// <summary>
        /// On certain platforms you may want to replace/override the localization on certain texts.
        /// </summary>
        [JsonProperty("override")] public Dictionary<string, Dictionary<string,string>> PlatformOverrideKeys;
        /// <summary>
        /// Key: Keypath (data binding),
        /// Value: Localization Key
        /// </summary>
        public Dictionary<string, string> LocalizationKeys;

        /// <summary>
        /// Process a JObject and convert it to a PanelModel
        /// </summary>
        /// <param name="jObject"></param>
        public PanelModel(JObject jObject)
        {
            ID = jObject.GetValue("id")?.ToString();
            PrefabPath = jObject.GetValue("prefab")?.ToString();
            LocalizationKeys = new Dictionary<string, string>();

            if (jObject.TryGetValue("override", out JToken token))
            {
                Dictionary<string, Dictionary<string, string>> overrideDictionary = token.ToObject<Dictionary<string, Dictionary<string,string>>>();
                PlatformOverrideKeys = overrideDictionary;
            }
            else
            {
                PlatformOverrideKeys = null;
            }
            
            foreach (KeyValuePair<string, JToken> property in jObject)
            {
                if (!KEYPATH_IGNORE_LIST.Contains(property.Key))
                {
                    LocalizationKeys[property.Key] = property.Value.ToString();
                }
            }
        }
    }
}