using System;
using System.Diagnostics;
using Livesplit.CS4.Enums;

namespace Livesplit.CS4
{
    // The point of this class is to connect the paths and console to the game and update the paths.
    // Values should be gotten from the paths directly but through this class (hence why they're public)
    
    public class PointerAndConsoleManager : IDisposable
    {

        private const string PROCESS_NAME = "ed8_4_PC";

        private DateTime _nextHookAttempt = DateTime.MinValue;
        private Process _game;
        private bool _disablePointer;

        private PointerPath<byte> _loadValue1;
        public Action<byte> OnLoad1Change;
        
        private PointerPath<byte> _loadValue6;
        public Action<byte> OnLoad6Change;

        private PointerPath<ushort> _battleID;
        public Action<BattleEnums> OnBattleEnd;
        
        private PointerPath<ushort> _cutsceneID;
        public Action<CutsceneEnums> OnCutsceneStart;
        
        private PointerPath<int> _chapterNumber;
        public Action<ChapterEnums> OnChapterEnd;
        
        
        public bool IsHooked => _game != null && IsAlive(_game);
        

        public void Hook() // This is honestly just a constructor I hate this design where the constructor is not a real constructor dude. 5 months ago me is an idiot
        {
            if (IsHooked || DateTime.Now < _nextHookAttempt)
            {
                return;
            }

            if (!IsHooked)
            {
                _disablePointer = false;
            }

            _nextHookAttempt = DateTime.Now.AddSeconds(1);
            
            Process[] processes = Process.GetProcessesByName(PROCESS_NAME);
            if (processes.Length == 0)
            {
                _game = null;
                return;
            }

            _game = processes[0];
            MemoryReader.Update64Bit(_game);

            switch (_game.MainModule?.ModuleMemorySize)
            {
                case 0x3811000:
                    Logger.Log("Identified version 1.2");
                    // Got these through the disassembler so they should be universal
                    _loadValue1 = new PointerPath<byte>(_game, new[] { 0x37867D0, 0x1CB8, 0x17E98, 0x0, 0x25 });
                    _loadValue6 = new PointerPath<byte>(_game, new[] { 0x37867D0, 0x1CB8, 0x17E98, 0x0, 0x20 });
                    _battleID = new PointerPath<ushort>(_game, new []{ 0x37867D0, 0x1CB8, 0x49608 });
                    _cutsceneID = new PointerPath<ushort>(_game, new[] { 0xD0B9B8, 0x1CB8, 0x49308 });
                    _chapterNumber = new PointerPath<int>(_game, new []{ 0xD1AB64 });
                    break;
                default: 
                    Logger.Log($"New version identified! Module size is {_game.MainModule?.ModuleMemorySize:X2}");
                    break;
            }
            
            _game.Exited += OnGameExit;
            if(_disablePointer)
                return;

            _loadValue1.OnPointerChange += CheckLoad1;
            _loadValue6.OnPointerChange += CheckLoad6;
            _battleID.OnPointerChange += CheckBattleSplit;
            _cutsceneID.OnPointerChange += CheckCutsceneSplit;
            _chapterNumber.OnPointerChange += CheckChapterSplit;
            
            
        }

        
        public void UpdateValues()
        {
            if (_disablePointer) return;
            
            _loadValue1.UpdateAddressValue();
            _loadValue6.UpdateAddressValue();
            _battleID.UpdateAddressValue();
            _cutsceneID.UpdateAddressValue();
            _chapterNumber.UpdateAddressValue();

        }
        
        private void CheckLoad1(byte oldValue, byte newValue)
        {
            OnLoad1Change.Invoke(newValue);
        }
        
        private void CheckLoad6(byte oldValue, byte newValue)
        {
            OnLoad6Change.Invoke(newValue);
        }
        
        private void CheckCutsceneSplit(ushort oldID, ushort newID)
        {           
            Logger.Log($"Cutscene ID changed from {oldID} to {newID}");
            if(newID == 0) return; // A cutscene has ended, not started
            
            if (!Enum.IsDefined(typeof(CutsceneEnums), newID))
            {
                Logger.Log($"The cutscene ID value {newID} isn't defined!");
                return;
            }

            Logger.Log($"Firing the Cutscene Start Action! Enum is {(CutsceneEnums)newID}");
            OnCutsceneStart.Invoke((CutsceneEnums)newID);
        }

        private void CheckBattleSplit(ushort oldID, ushort newID)
        {
            Logger.Log($"Battle ID changed from {oldID} to {newID}");
            if(oldID == 0 || newID != 0) return; // A battle has started, not ended
            
            if (!Enum.IsDefined(typeof(BattleEnums), oldID))
            {
                Logger.Log($"The battle ID value {oldID} isn't defined!");
                return;
            }

            Logger.Log($"Firing the Battle End Action! Enum is {(BattleEnums)oldID}");
            OnBattleEnd.Invoke((BattleEnums)oldID);
            
        }

        private void CheckChapterSplit(int oldChapter, int newChapter)
        {
            if(newChapter != oldChapter+1) return; // If the chapter jumps we probably loaded a save or something
            
            if (!Enum.IsDefined(typeof(ChapterEnums), oldChapter))
            {
                Logger.Log($"The chapter ID value {oldChapter} isn't defined!");
                return;
            }

            Logger.Log($"Firing the Chapter End Action! Enum is {(ChapterEnums)oldChapter}");
            OnChapterEnd.Invoke((ChapterEnums)oldChapter);
            
        }
        

        private void OnGameExit(object sender, EventArgs e)
        {
            Dispose();
        }
        
        public void Dispose()
        {
            try
            {
                
                if (!_disablePointer)
                {
                    // ReSharper disable DelegateSubtraction
                    _loadValue1.OnPointerChange -= CheckLoad1;
                    _loadValue6.OnPointerChange -= CheckLoad6;
                    _battleID.OnPointerChange -= CheckBattleSplit;
                    _cutsceneID.OnPointerChange -= CheckCutsceneSplit;
                    _chapterNumber.OnPointerChange -= CheckChapterSplit;
                    // ReSharper restore DelegateSubtraction
                }
                
                _game.Exited -= OnGameExit;
            }
            catch
            {
                // ignored, else the component refuses to close
            }

            _game?.Dispose();

        }

        private static bool IsAlive(Process game)
        {
            try
            {
                Process.GetProcessById(game.Id);

            }
            catch (ArgumentException)
            {
                return false;
            }

            return true;
        }
        


    }
}