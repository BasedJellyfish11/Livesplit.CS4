using System;
using System.Reflection;
using LiveSplit.Model;
using LiveSplit.UI.Components;

namespace Livesplit.CS4
{
    public class CS4Factory : IComponentFactory
    {
        // ReSharper disable once UnusedMember.Global
      
        public IComponent Create(LiveSplitState state)
        {
            return new CS4Component(state, ComponentName);
        }

        public string UpdateName => ComponentName;
        public string XMLURL => UpdateURL + "Components/LiveSplit.CS4.Updates.xml";
        public string UpdateURL => "https://raw.githubusercontent.com/BasedJellyfish11/LiveSplit.CS4/master/Livesplit.CS4/";
        public Version Version => Assembly.GetExecutingAssembly().GetName().Version;

        public string ComponentName => "The Legend of Heroes: Trails of Cold Steel 4 Autosplitter v" + Version;
        public string Description => "The Legend of Heroes: Trails of Cold Steel 4 Autosplitter";
        public ComponentCategory Category => ComponentCategory.Control;
    }
    
}