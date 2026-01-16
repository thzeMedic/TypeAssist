using System;
using System.Collections.Generic;
using System.Configuration;
using System.Text;

namespace TypeAssist
{
    internal class ModeSettings:ConfigurationSection
    {

        [ConfigurationProperty("Mode", DefaultValue = "Wörter")]
        public string Mode
        {
            get { return (string)this["Mode"]; }
            set { this["Mode"] = value; }
        }

        [ConfigurationProperty("SuggestionPosition", DefaultValue = "Maus")]
        public string SuggestionPosition
        {
            get { return (string)this["SuggestionPosition"]; }
            set { this["SuggestionPosition"] = value; }
        }
    }
}
