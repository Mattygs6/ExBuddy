namespace ExBuddy.OrderBotTags.Common
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.Caching;

    using Clio.Common;
    using Clio.Utilities;

    using ExBuddy.OrderBotTags.Navigation;

    using ff14bot.Helpers;
    using ff14bot.Managers;

    [Flags]
    public enum CollisionFlags
    {
        Error = -1,
        None = 0,
        Forward = 1 << 0,
        Up = 1 << 1,
        Down = 1 << 2,
        Left = 1 << 3,
        Right = 1 << 4,
        ForwardLeft = 1 << 5,
        ForwardRight = 1 << 6,
        ForwardUp = 1 << 7,
        ForwardDown = 1 << 8
    }

    public class Collider
    {

        private readonly float forwardRange;

        private readonly Vector3 forwardNormal;

        public Collider(Vector3 position, Vector3 direction, float forwardRange)
        {
            direction.Normalize();
            forwardNormal = Forward = direction;
            Forward *= forwardRange;
            Direction2D = new Vector3(direction.X, 0, direction.Z);
            Direction2D.Normalize();
            Position = position;
            this.forwardRange = forwardRange;
        }

        public bool IsFowardCollision()
        {
            Vector3 hit;
            Vector3 distances;

            //  Forward
            if (!WorldManager.Raycast(Position, Position + Forward, out hit, out distances))
            {
                return false;
            }

            Flags |= CollisionFlags.Forward;
            ForwardHit = hit;

            return true;
        }

        public void BuildCollider()
        {
            // Find raycast vectors
            Right = Vector3.Cross(Forward, Direction2D);
            Left = new Vector3(-Right.X, Right.Y, -Right.Z);
            Up = Vector3.Cross(Right, Forward);

            // If greater than 0, we ar heading up.
            if (Forward.Y > 0)
            {
                Down = Up;
                Up = -Up;
            }
            else
            {
                Down = -Up;
            }

            Right.Normalize();
            Up.Normalize();
            Down.Normalize();
            Left.Normalize();

            ForwardRight = Vector3.Blend(Right, forwardNormal, 0.5f);
            ForwardUp = Vector3.Blend(Up, forwardNormal, 0.5f);
            ForwardLeft = Vector3.Blend(Left, forwardNormal, 0.5f);
            ForwardDown = Vector3.Blend(Down, forwardNormal, 0.5f);

            ForwardRight.Normalize();
            ForwardUp.Normalize();
            ForwardLeft.Normalize();
            ForwardDown.Normalize();

            var diagonalMagnitude = forwardRange / (float)Math.Cos(45.0f * Math.PI / 180);
            Right *= forwardRange;
            Up *= forwardRange;
            Left *= forwardRange;
            Down *= forwardRange;

            ForwardRight *= diagonalMagnitude;
            ForwardUp *= diagonalMagnitude;
            ForwardLeft *= diagonalMagnitude;
            ForwardDown *= diagonalMagnitude;
        }

        public bool FindClosestDeviation(ICollection<FlightPoint> previousFlightPoints, out Vector3 travelDeviation)
        {
            var deviation = travelDeviation = Vector3.Zero;
            var valueFound = false;

            // Making the rays 1.5x as long as our detection range to ensure it is clear
            var forwardRay = Position + Forward * 1.5f;
            var forwardRightRay = Position + ForwardRight * 1.5f;
            var forwardLeftRay = Position + ForwardLeft * 1.5f;
            var forwardUpRay = Vector3.Zero;
            var forwardDownRay = Vector3.Zero;
            Vector3 lastHit = ForwardHit;
            Vector3 hit = ForwardHit;
            Vector3 distances;

            for (float i = 0.2f; i < 1; i += 0.2f)
            {
                lastHit = hit;
                if (!WorldManager.Raycast(Position, deviation = Vector3.Blend(forwardRay, forwardRightRay, i), out hit, out distances) && !previousFlightPoints.Any(fp => fp.FuzzyEquals(deviation)) && !MemoryCache.Default.Contains(deviation.ToString()))
                {
                    valueFound = true;
                    Flags |= CollisionFlags.ForwardRight;
                    break;
                }

                lastHit = hit;
                if (!WorldManager.Raycast(Position, deviation = Vector3.Blend(forwardRay, forwardLeftRay, i), out hit, out distances) && !previousFlightPoints.Any(fp => fp.FuzzyEquals(deviation)) && !MemoryCache.Default.Contains(deviation.ToString()))
                {
                    valueFound = true;
                    Flags |= CollisionFlags.ForwardLeft;
                    break;
                }
            }

            if (!valueFound)
            {
                forwardUpRay = Position + ForwardUp * 1.5f;
                forwardDownRay = Position + ForwardDown * 1.5f;

                for (float i = 0.2f; i < 1; i += 0.2f)
                {
                    lastHit = hit;
                    if (!WorldManager.Raycast(Position, deviation = Vector3.Blend(forwardRay, forwardUpRay, i), out hit, out distances) && !previousFlightPoints.Any(fp => fp.FuzzyEquals(deviation)) && !MemoryCache.Default.Contains(deviation.ToString()))
                    {
                        valueFound = true;
                        Flags |= CollisionFlags.ForwardUp;
                        break;
                    }

                    lastHit = hit;
                    if (!WorldManager.Raycast(Position, deviation = Vector3.Blend(forwardRay, forwardDownRay, i), out hit, out distances) && !previousFlightPoints.Any(fp => fp.FuzzyEquals(deviation)) && !MemoryCache.Default.Contains(deviation.ToString()))
                    {
                        valueFound = true;
                        Flags |= CollisionFlags.ForwardDown;
                        break;
                    }
                }
            }

            if (!valueFound)
            {
                var upRay = Position + Up * 1.5f;
                var downRay = Position + Down * 1.5f;

                for (float i = 0.2f; i < 1; i += 0.2f)
                {
                    lastHit = hit;
                    if (!WorldManager.Raycast(Position, deviation = Vector3.Blend(forwardUpRay, upRay, i), out hit, out distances) && !previousFlightPoints.Any(fp => fp.FuzzyEquals(deviation)) && !MemoryCache.Default.Contains(deviation.ToString()))
                    {
                        valueFound = true;
                        Flags |= CollisionFlags.ForwardUp;
                        break;
                    }

                    lastHit = hit;
                    if (!WorldManager.Raycast(Position, deviation = Vector3.Blend(forwardDownRay, downRay, i), out hit, out distances) && !previousFlightPoints.Any(fp => fp.FuzzyEquals(deviation)) && !MemoryCache.Default.Contains(deviation.ToString()))
                    {
                        valueFound = true;
                        Flags |= CollisionFlags.ForwardDown;
                        break;
                    }
                }
            }

            if (!valueFound)
            {
                var leftRay = Position + Left * 2;
                var rightRay = Position + Right * 2;

                for (float i = 0.2f; i < 1; i += 0.2f)
                {
                    lastHit = hit;
                    if (!WorldManager.Raycast(Position, deviation = Vector3.Blend(forwardLeftRay, leftRay, i), out hit, out distances) && !previousFlightPoints.Any(fp => fp.FuzzyEquals(deviation)) && !MemoryCache.Default.Contains(deviation.ToString()))
                    {
                        valueFound = true;
                        Flags |= CollisionFlags.ForwardLeft;
                        break;
                    }

                    lastHit = hit;
                    if (!WorldManager.Raycast(Position, deviation = Vector3.Blend(forwardRightRay, rightRay, i), out hit, out distances) && !previousFlightPoints.Any(fp => fp.FuzzyEquals(deviation)) && !MemoryCache.Default.Contains(deviation.ToString()))
                    {
                        valueFound = true;
                        Flags |= CollisionFlags.ForwardRight;
                        break;
                    }
                }
            }

            if (!valueFound)
            {
                Flags |= CollisionFlags.Error;
            }
            else
            {
                travelDeviation = deviation;
                var direction = deviation - Position;

                var dot = Vector3.Dot(forwardRay, direction);
                var angle = MathEx.ToDegrees((float)Math.Acos(dot / (forwardRay.Magnitude * direction.Magnitude)));

                angle = Math.Abs(angle - 90);

                Logging.Write("Angle of deviation: " + angle);
            }

            return valueFound;
        }

        public CollisionFlags Flags;

        public Vector3 Direction2D;

        public Vector3 Position;

        public Vector3 Forward;

        public Vector3 ForwardHit;

        public Vector3 Up;

        public Vector3 Down;

        public Vector3 Left;

        public Vector3 Right;

        public Vector3 ForwardLeft;

        public Vector3 ForwardRight;

        public Vector3 ForwardUp;

        public Vector3 ForwardDown;
    }
    public class Collisions
    {
        public readonly Collider PlayerCollider;

        //public readonly Collider DestinationCollider;

        public Collisions(Vector3 position, Vector3 ray3, float forwardRange = 30.0f)
        {
            this.PlayerCollider = new Collider(position, ray3, forwardRange);
            //this.DestinationCollider = new Collider(position + ray3, -ray3, forwardRange);
        }

        public CollisionFlags CollisionResult(ICollection<FlightPoint> previousFlightPoints, out Vector3 deviation)
        {
            deviation = Vector3.Zero;
            
            if (!PlayerCollider.IsFowardCollision())
            {
                return PlayerCollider.Flags;
            }

            this.PlayerCollider.BuildCollider();
            //this.DestinationCollider.BuildCollider();

            Vector3 playerDeviation;
            this.PlayerCollider.FindClosestDeviation(previousFlightPoints, out playerDeviation);
            //Vector3 destinationDeviation;
            //this.DestinationCollider.FindClosestLateralDeviation(out destinationDeviation);

            // If no intersection, they are opposite directions...

            deviation = playerDeviation;

            return PlayerCollider.Flags;
        }
    }
}
