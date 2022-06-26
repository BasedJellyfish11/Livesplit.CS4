using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Xml;
using Livesplit.CS4.Enums;

namespace Livesplit.CS4
{
    public partial class Settings : UserControl
    {
        private readonly Dictionary<string, Enum> displayedSettings;
        public readonly HashSet<Enum> currentSplitSettings;

        public Settings()
        {
            displayedSettings = new Dictionary<string, Enum>();
            // Since Serialization happens on a loop because Livesplit is bad, it's better to write "_" instead of spaces so that String.Replace() is used less often
            foreach (BattleEnums enums in Enum.GetValues(typeof(BattleEnums)))
            {
                displayedSettings.Add($"Split_{enums.ToString()}", enums);
            }
            
            foreach (ChapterEnums enums in Enum.GetValues(typeof(ChapterEnums)))
            {
                displayedSettings.Add($"Split_{enums.ToString()}", enums);
            }
            
            foreach (CutsceneEnums enums in Enum.GetValues(typeof(CutsceneEnums)).OfType<CutsceneEnums>().Where(x => x != CutsceneEnums.Start_Cutscene))
            {
                displayedSettings.Add($"Split_{enums.ToString()}", enums);
            }

            InitializeComponent();
            if (displayedSettings != null) 
                SplitsCollection.Items.AddRange(displayedSettings.Keys.Select(x => x.Replace("_", " ")).ToArray());
            
            Load += LoadLayout;
            currentSplitSettings = new HashSet<Enum>();
            
        }
        
        private void SplitsCollection_ItemCheckChanged(object sender, ItemCheckEventArgs e)
        {
            Enum setting = displayedSettings[SplitsCollection.Items[e.Index].ToString().Replace(' ', '_')];
            if (e.NewValue == CheckState.Checked)
                currentSplitSettings.Add(setting);
            else
                currentSplitSettings.Remove(setting);
        }

        private void LoadLayout(object sender, EventArgs e)
        {
            for (int i = 0; i < SplitsCollection.Items.Count; ++i)
            {
                if (currentSplitSettings.Contains(displayedSettings[ ((string)SplitsCollection.Items[i]).Replace(' ', '_')])) 
                    SplitsCollection.SetItemChecked(i, true);
            }
        }


        private void Settings_Load(object sender, EventArgs e)
        {
            LoadLayout(sender, e);
        }

        public XmlNode Serialize(XmlDocument document)
        {
            XmlElement xmlSettings = document.CreateElement("Settings");

            foreach (string splitSetting in displayedSettings.Keys)
            {
                XmlElement element = document.CreateElement(splitSetting);
                element.InnerText = currentSplitSettings.Contains(displayedSettings[splitSetting]).ToString();
                xmlSettings.AppendChild(element);
            }
            
            return xmlSettings;
        }

        public void Deserialize(XmlNode settings)
        {

            foreach (string battleSplit in displayedSettings.Keys)
            {
                XmlNode node = settings.SelectSingleNode(".//" + battleSplit);
                if (!bool.TryParse(node?.InnerText, out bool splitSetting)) continue;
                if (splitSetting)
                    currentSplitSettings.Add(displayedSettings[battleSplit]);
            }
        }
    }
}