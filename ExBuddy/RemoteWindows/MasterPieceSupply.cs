namespace ExBuddy.RemoteWindows
{
    using System.Threading.Tasks;

    using Buddy.Coroutines;

    using ExBuddy.Enums;
    using ExBuddy.Helpers;

    using ff14bot.Enums;
    using ff14bot.Managers;
    using ff14bot.RemoteWindows;

    public sealed class MasterPieceSupply : Window<MasterPieceSupply>
    {
        public MasterPieceSupply()
            : base("MasterPieceSupply")
        {
        }

        public static uint GetClassIndex(ClassJobType classJobType)
        {
            return (uint)classJobType - 8;
        }

        public SendActionResult SelectClass(ClassJobType classJobType)
        {
            return SelectClass(GetClassIndex(classJobType));
        }

        public SendActionResult SelectClass(uint index)
        {
            return TrySendAction(2, 1, 2, 1, index);
        }

        public SendActionResult TurnIn(uint index)
        {
            return TrySendAction(2, 0, 0, 1, index);
        }

        public async Task<bool> TurnIn(uint index, BagSlot bagSlot, byte attempts = 20, ushort interval = 200)
        {
            var result = SendActionResult.None;
            var requestAttempts = 0;
            while (result != SendActionResult.Success && !Request.IsOpen && requestAttempts++ < attempts && Behaviors.ShouldContinue)
            {
                result = TurnIn(index);
                if (result == SendActionResult.InjectionError)
                {
                    if (interval <= 33)
                    {
                        await Coroutine.Yield();
                    }
                    else
                    {
                        await Coroutine.Sleep(interval);    
                    }
                }

                if (interval <= 33)
                {
                    await Coroutine.Yield();
                }
                else
                {
                    await Coroutine.Wait(interval, () => Request.IsOpen);
                }
            }

            if (result != SendActionResult.Success || requestAttempts > attempts)
            {
                return false;
            }

            // Try waiting an extra second for it to open in case interval was set really low.
            if (!Request.IsOpen)
            {
                await Coroutine.Wait(1000, () => Request.IsOpen);    
            }

            requestAttempts = 0;
            while (Request.IsOpen && requestAttempts++ < attempts && Behaviors.ShouldContinue && bagSlot.Item != null)
            {
                bagSlot.Handover();
                if (interval <= 33)
                {
                    await Coroutine.Yield();
                }
                else
                {
                    await Coroutine.Wait(interval, () => Request.HandOverButtonClickable);
                }

                Request.HandOver();

                if (interval <= 33)
                {
                    await Coroutine.Yield();
                }
                else
                {
                    await Coroutine.Wait(interval, () => !Request.IsOpen || SelectYesno.IsOpen);
                }
                
            }

            return !Request.IsOpen;
        }
    }
}
