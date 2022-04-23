using System;
using System.Diagnostics;

namespace Livesplit.CS4
{
    // Reads from memory using a pointer path with the game's address as a starting point to the offsets
    // Changes to the values are easily detected due to the buffer system of Last and Current values.
    
    // Also able to fixate on an address, instead of following a path every time, which is highly discouraged
    // but proved to be a needed hack for some games such as ToCS3 where the address didn't move around but there was no constant path to it
    public class PointerPath<T> where T: struct
    {
        private readonly Process _game;
        private readonly int[] _offsets;
        private T _lastValue;

        private T _currentValue;

        private readonly bool fixateOnAddress;
        private readonly T _lastValueCondition;
        private readonly T _currentValueCondition;
        private IntPtr _address;

        private readonly bool _continuouslyFire;
       
        public Action<T,T> OnPointerChange;

        /**
         * Should never be called before game is hooked or at least launched
         */

        public PointerPath(Process game, int[] offsets, T lastValueCondition = default, T currentValueCondition = default , bool continuouslyFire = false)
        {
            _game = game;
            _offsets = offsets;
            _continuouslyFire = continuouslyFire;

            if (lastValueCondition.Equals(default(T)) && currentValueCondition.Equals(default(T)))
                return;
            
            fixateOnAddress = true;
            _lastValueCondition = lastValueCondition;
            _currentValueCondition = currentValueCondition;

        }

        // Updates the values and fires the hook if they have changed
        public void UpdateAddressValue()
        {
            _lastValue = _currentValue;
            
            if (!fixateOnAddress || _address == default)
                FollowPath();
            else
                ReadFromAddressDirectly();
            
            if(!_lastValue.Equals(_currentValue) || _continuouslyFire)
                OnPointerChange.Invoke(_lastValue, _currentValue);
        }
        
        private void FollowPath()
        {
            _currentValue = _game.Read<T>(_game.MainModule.BaseAddress, _offsets);
            if(!_lastValue.Equals(_lastValueCondition) || !_currentValue.Equals(_currentValueCondition))
                return;
            
            // Hardset the address if needed
            _address = _game.ResolveAddress(_game.MainModule.BaseAddress, _offsets);

        }
        
        private void ReadFromAddressDirectly()
        {
            _currentValue = _game.ReadFromAddress<T>(_address);
        }

     

    }
    
}