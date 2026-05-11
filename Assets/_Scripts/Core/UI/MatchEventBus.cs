using System;
using Data;

namespace Core.UI
{
    public static class MatchEventBus
    {
        // The 'int' is the player index
        public static Action<int, FixedFloat> OnCharacterDamageChanged;
        
        public static Action<int, int> OnCharacterStocksChanged;
        
        public static Action<long, int> OnTimerUpdated;
    }
}