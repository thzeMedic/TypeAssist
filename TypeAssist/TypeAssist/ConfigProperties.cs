using System;
using System.Collections.Generic;
using System.Configuration;
using System.Text;

namespace TypeAssist
{
    internal class ConfigProperties:ConfigurationSection
    {

        [ConfigurationProperty("Mode", DefaultValue = "Wörter")]
        public string Mode
        {
            get { return (string)this["Mode"]; }
            set { this["Mode"] = value; }
        }

        [ConfigurationProperty("UseTravelDistance", DefaultValue = true)]
        public bool UseTravelDistance
        {
            get { return (bool)this["UseTravelDistance"]; }
            set { this["UseTravelDistance"] = value; }
        }

        [ConfigurationProperty("UseRemoteLlm", DefaultValue = true)]
        public bool UseRemoteLlm
        {
            get { return (bool)this["UseRemoteLlm"]; }
            set { this["UseRemoteLlm"] = value; }
        }

        [ConfigurationProperty("SuggestionPosition", DefaultValue = "Maus")]
        public string SuggestionPosition
        {
            get { return (string)this["SuggestionPosition"]; }
            set { this["SuggestionPosition"] = value; }
        }

        [ConfigurationProperty("UseLimitedContext", DefaultValue = true)]
        public bool UseLimitedContext
        {
            get { return (bool)this["UseLimitedContext"]; }
            set { this["UseLimitedContext"] = value; }
        }

        [ConfigurationProperty("ContextLength", DefaultValue = 200)]
        public int ContextLength
        {
            get { return (int)this["ContextLength"]; }
            set { this["ContextLength"] = value; }
        }
    }
}
