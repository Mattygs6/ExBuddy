namespace ExBuddy.Interfaces
{
    using System;
    using System.Threading.Tasks;

    using Clio.Utilities;

    using ff14bot.Interfaces;

    public interface IFlightEnabledPlayerMover : IPlayerMover, IDisposable
    {
        bool CanFly { get; }

        IFlightMovementArgs FlightMovementArgs { get; }

        bool ShouldFlyTo(Vector3 destination);

        Task SetShouldFlyAsync(Task<Func<Vector3, bool>> shouldFlyToFunc);
    }
}