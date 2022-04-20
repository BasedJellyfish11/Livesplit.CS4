using System;
using System.Diagnostics;
using System.Threading;
using Livesplit.CS4.Enums;

namespace Livesplit.CS4
{
    // The point of this class is to connect the paths and console to the game and update the paths.
    // Values should be gotten from the paths directly but through this class (hence why they're public)
    // A reminder that the DebugMonitor raises an event whenever a line is captured, and that's how the lines should be gotten (from the monitor's event directly)
    
    public class PointerAndConsoleManager : IDisposable
    {

        private const string PROCESS_NAME = "ed8_4_PC";


        private DateTime _nextHookAttempt = DateTime.MinValue;
        private Process _game;
        private bool _disablePointer;

        private PointerPath<ushort> _battleID;
        public delegate void OnBattleEndHandler(BattleEnums endedBattle);
        public OnBattleEndHandler OnBattleEnd;
        
        private PointerPath<int> _chapterNumber;
        // public delegate void OnChapterEndHandler(ChapterEnums oldChapter);
        // public OnChapterEndHandler OnChapterEnd;

        private PointerPath<byte> _cheating;
        public delegate void OnBattleAnimationStartHandler();
        public OnBattleAnimationStartHandler OnBattleAnimationStart;
        
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
                case 0x1DEA000:
                    _battleID = new PointerPath<ushort>(_game, new []{0xC53330, 0x1CE8, 0x5B1C0}); // Got these through the disassembler so they should be universal
                    _chapterNumber = new PointerPath<int>(_game, new []{0x16C48B8}); // Static yes!!!!
                    _cheating = new PointerPath<byte>(_game, new []{0x00C53210, 0x8, 0x28, 0x1AA8,0x8, 0x2F98, 0x290, 0x278, 0x278, 0x2C8, 0x2A0}, 0, 1, true);
                    break;
                default: _disablePointer = true;
                    break;
            }
            
            Thread.Sleep(500);
            _game.Exited += OnGameExit;
            if(_disablePointer)
                return;
            
            _battleID.OnPointerChange += CheckBattleSplit;
            // _chapterNumber.OnPointerChange += CheckChapterSplit;
            _cheating.OnPointerChange += CheckSkipAnimation;



        }

        public void UpdateValues()
        {
            if (_disablePointer) return;
            _battleID.UpdateAddressValue();
            _cheating.UpdateAddressValue();
            _chapterNumber.UpdateAddressValue();

        }

        private void CheckBattleSplit(ushort oldID, ushort newID)
        {
            Logger.Log($"Hello lol {oldID} {newID}");
            if(oldID == 0 || newID != 0) return; // A battle has started, not ended
            
            if (!Enum.IsDefined(typeof(BattleEnums), oldID))
            {
                Logger.Log($"The battle ID value {oldID} isn't defined!");
                return;
            }

            Logger.Log($"Firing the Battle End Delegate! Enum is {(BattleEnums)oldID}");
            OnBattleEnd.Invoke((BattleEnums)oldID);
            
        }
        
        /*
        private void CheckChapterSplit(int oldChapter, int newChapter)
        {
            if(newChapter != oldChapter+1) return; // If the chapter jumps we probably loaded a save or something
            
            if (!Enum.IsDefined(typeof(ChapterEnums), oldChapter))
            {
                Logger.Log($"The chapter ID value {oldChapter} isn't defined!");
                return;
            }

            Logger.Log($"Firing the Chapter End Delegate! Enum is {(ChapterEnums)oldChapter}");
            OnChapterEnd.Invoke((ChapterEnums)oldChapter);
            
        }
        
        */
        
        private void CheckSkipAnimation(byte lastvalue, byte currentvalue)
        {
            if(currentvalue != 1) return;
            
            Logger.Log("Firing the Animation Start Delegate!");
            OnBattleAnimationStart.Invoke();   
            
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
                    _battleID.OnPointerChange -= CheckBattleSplit;
                    // _chapterNumber.OnPointerChange -= CheckChapterSplit;
                    _cheating.OnPointerChange -= CheckSkipAnimation;
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