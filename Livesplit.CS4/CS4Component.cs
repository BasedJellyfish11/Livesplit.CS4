using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Xml;
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
        
        private bool _delegatesHooked; 
        
        // These two are related so you could make them a struct if you reeeeeeeeeeeeally wanted to but like it's 2 bools dude
        private bool _drawStartLoad;
        private bool _initFieldLoad;
        
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
            _delegatesHooked = false;
            _drawStartLoad = false;
            _initFieldLoad = false;
            
        }
        
        public void Update(IInvalidator invalidator, LiveSplitState state, float width, float height, LayoutMode mode)
        {
            _manager.Hook();
            if (!_manager.IsHooked)
            {
                _drawStartLoad = true;
                _initFieldLoad = false;
                _model.CurrentState.IsGameTimePaused = true;
                
                UnhookDelegates();
                
                return;
            }

            if (!_delegatesHooked) 
            {
                HookDelegates();
            }
            
            _manager.UpdateValues();

        }


        private void CheckStart(string text)
        {
            if (_model.CurrentState.CurrentSplitIndex != -1)
                return;
            
            if (!text.StartsWith("exitField(\"title00\") - start: nextMap(\"f1000\")")) return;
            Logger.Log("Starting timer");
            _model.CurrentState.IsGameTimePaused = true;
            _model.Start();
        }

        private void CheckLoading(string line)
        {
 
            if (!_model.CurrentState.IsGameTimePaused)
            {
                if (line.StartsWith("NOW LOADING Draw Start"))
                {
                    
                    Logger.Log("Pausing timer! Line was " + line);
                    _model.CurrentState.IsGameTimePaused = true;
                    _drawStartLoad = true;
                }

                else if (line.StartsWith("FieldMap::initField start") )
                {
                    
                    Logger.Log("Pausing timer! Line was " + line);
                    _model.CurrentState.IsGameTimePaused = true;
                    _initFieldLoad = true;

                }
                
                else if (line.StartsWith("exitField"))
                {
                    
                    Logger.Log("Pausing timer! Line was " + line);
                    _model.CurrentState.IsGameTimePaused = true;
                    
                }
            }

            else
            {
                if (!_initFieldLoad && !_drawStartLoad && line.StartsWith("exitField - end"))
                {
                    
                    Logger.Log("Unpausing timer! Line was " + line);
                    _model.CurrentState.IsGameTimePaused = false;
                    
                }
                
                else if (!_drawStartLoad && line.StartsWith("FieldMap::initField end"))
                {
                    
                    Logger.Log("Unpausing timer! Line was " + line);
                    _model.CurrentState.IsGameTimePaused = false;
                    _initFieldLoad = false;
                    
                }
                
                else if (line.StartsWith("NOW LOADING Draw End")){
                    
                    Logger.Log("Unpausing timer! Line was " + line);
                    _model.CurrentState.IsGameTimePaused = false;
                    _drawStartLoad = false;
                                   
                }
            }
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
            UnhookDelegates();
            _manager.Dispose();
        }

        #region UtilityMethods

        private void HookDelegates()
        {
            Logger.Log("Subscribing events...");
            if(_delegatesHooked)
                return;

            _manager.OnBattleEnd += CheckSplit;
            Logger.Log("OnBattleEnd hooked to Split!");

            // _manager.OnChapterEnd += CheckSplit;
            Logger.Log("OnChapterEnd hooked to Split!");
            
            _delegatesHooked = true;
            
            Logger.Log("Events subscribed!");
        }
        
        private void UnhookDelegates()
        {
            if(!_delegatesHooked)
                return;
            
            Logger.Log("Unsubscribing events...");
            
            _manager.OnBattleEnd -= CheckSplit;
            Logger.Log("OnBattleEnd unhooked from BattleSplit!");

            // _manager.OnChapterEnd -= CheckSplit;
            Logger.Log("OnBattleEnd unhooked from BattleSplit!");
            
            _delegatesHooked = false;

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