using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using System.Xml;
using Livesplit.CS4.Enums;
using LiveSplit.Model;
using LiveSplit.UI;
using LiveSplit.UI.Components;
// ReSharper disable DelegateSubtraction


namespace Livesplit.CS4
{
    public class CS4Component : IComponent
    {
        private readonly TimerModel _model;
        private readonly PointerAndConsoleManager _manager;
        private readonly Settings _settings = new Settings();
        
        private bool _actionsHooked; 
        
        private bool _isLoading6;
        private bool _isLoading1;
        
        public string ComponentName { get; }


        public CS4Component(LiveSplitState state, string name)
        {
            ComponentName = name;
            _manager = new PointerAndConsoleManager();
            
            if (state == null)
            {
                throw new ArgumentNullException(nameof(state));
            }

            _model = new TimerModel()
            {
                CurrentState = state
            };
            
            _model.InitializeGameTime();
            _actionsHooked = false;
            
        }
        
        public void Update(IInvalidator invalidator, LiveSplitState state, float width, float height, LayoutMode mode)
        {
            _manager.Hook();
            if (!_manager.IsHooked)
            {
                _model.CurrentState.IsGameTimePaused = true;
                _isLoading1 = false;
                _isLoading6 = false;
                UnhookActions();
                return;
            }

            if (!_actionsHooked) 
            {
                HookActions();
            }
            
            _manager.UpdateValues();

        }


        private void CheckStart(CutsceneEnums cutsceneID)
        {
            if (_model.CurrentState.CurrentSplitIndex != -1 || cutsceneID != CutsceneEnums.Start)
                return;
            
            Logger.Log("Starting timer");
            _model.Start();
        }

        private void CheckLoading()
        {
            Logger.Log("Checking load");
            _model.CurrentState.IsGameTimePaused = _isLoading1 || _isLoading6;
            
        }
        
        private void CheckSplit<T>(T split) where T: Enum
        {
            
            if (!_settings.currentSplitSettings.Contains(split)) return; // If the setting is false, or it doesn't exist, return

            Logger.Log("Running a split with enum " + split);
            _model.Split();

        }


        public Control GetSettingsControl(LayoutMode mode)
        {
            return _settings;
        }

        public XmlNode GetSettings(XmlDocument document)
        {
            // I have no idea why you're supposed to serialize on something called GetSettings but take it up with Livesplit and not me
            return _settings.Serialize(document);
        }

        public void SetSettings(XmlNode settings)
        {
            // Same as above but with Deserializing
            _settings.Deserialize(settings);
        }

        public void Dispose()
        {
            UnhookActions();
            _manager.Dispose();
        }

        #region UtilityMethods

        private void HookActions()
        {
            Logger.Log("Subscribing events...");
            if(_actionsHooked)
                return;

            _manager.OnBattleEnd += CheckSplit;
            Logger.Log("OnBattleEnd hooked to Split!");

            _manager.OnChapterEnd += CheckSplit;
            Logger.Log("OnChapterEnd hooked to Split!");

            _manager.OnCutsceneStart += CheckStart;
            Logger.Log("OnCutsceneStart hooked to Start!");
            _manager.OnCutsceneStart += CheckSplit;
            Logger.Log("OnCutsceneStart hooked to Split!");

            _manager.OnLoad1Change += newValue => { _isLoading1 = newValue != 1;  CheckLoading(); };
            Logger.Log("OnLoad1Change hooked to load lambda!");
            _manager.OnLoad6Change += newValue =>  { _isLoading6 = newValue != 6;  CheckLoading(); };
            Logger.Log("OnLoad6Change hooked to load lambda!");

            _actionsHooked = true;
            
            Logger.Log("Events subscribed!");
        }
        
        private void UnhookActions()
        {
            if(!_actionsHooked)
                return;
            
            Logger.Log("Unsubscribing events...");
            
            _manager.OnBattleEnd -= CheckSplit;
            Logger.Log("OnBattleEnd unhooked from BattleSplit!");

            _manager.OnChapterEnd -= CheckSplit;
            Logger.Log("OnBattleEnd unhooked from BattleSplit!");
            
            _manager.OnCutsceneStart -= CheckStart;
            Logger.Log("OnCutsceneStart unhooked to Start!");
            _manager.OnCutsceneStart -= CheckSplit;
            Logger.Log("OnCutsceneStart unhooked to Split!");
            
            _actionsHooked = false;

        }

        #endregion
        
        #region Unused interface stuff
        
        public void DrawHorizontal(Graphics g, LiveSplitState state, float height, Region clipRegion)
        {
  
        }

        public void DrawVertical(Graphics g, LiveSplitState state, float width, Region clipRegion)
        {
            
        }

        public float                       HorizontalWidth     => 0;
        public float                       MinimumHeight       => 0;
        public float                       VerticalHeight      => 0;
        public float                       MinimumWidth        => 0;
        public float                       PaddingTop          => 0;
        public float                       PaddingBottom       => 0;
        public float                       PaddingLeft         => 0;
        public float                       PaddingRight        => 0;
        public IDictionary<string, Action> ContextMenuControls => null;
        #endregion
    }
}