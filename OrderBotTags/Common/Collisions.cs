namespace ExBuddy.OrderBotTags.Common
{
    using System;

    using Clio.Utilities;

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

        private Vector3 forwardNormal;

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

            // Apply magnitude create 1.25* diagonals in case object is convex
            var diagonalMagnitude = forwardRange * 1.25f / (float)Math.Cos(45.0f * Math.PI / 180);
            Right *= forwardRange;
            Up *= forwardRange;
            Left *= forwardRange;
            Down *= forwardRange;

            ForwardRight *= diagonalMagnitude;
            ForwardUp *= diagonalMagnitude;
            ForwardLeft *= diagonalMagnitude;
            ForwardDown *= diagonalMagnitude;
        }

        public bool FindClosestDeviation(out Vector3 travelDeviation)
        {
            travelDeviation = Vector3.Zero;
            var valueFound = false;
            for (float i = 0.1f; i < 1; i += 0.1f)
            {
                Vector3 hit;
                Vector3 distances;
                if (!WorldManager.Raycast(Position, travelDeviation = Vector3.Blend(Position + Forward, Position + ForwardRight, i), out hit, out distances))
                {
                    valueFound = true;
                    Flags |= CollisionFlags.ForwardRight;
                    break;
                }

                if (!WorldManager.Raycast(Position, travelDeviation = Vector3.Blend(Position + Forward, Position + ForwardLeft, i), out hit, out distances))
                {
                    valueFound = true;
                    Flags |= CollisionFlags.ForwardLeft;
                    break;
                }
            }

            if (!valueFound)
            {
                for (float i = 0.1f; i < 1; i += 0.1f)
                {
                    Vector3 hit;
                    Vector3 distances;
                    if (!WorldManager.Raycast(Position, travelDeviation = Vector3.Blend(Position + Forward, Position + ForwardUp, i), out hit, out distances))
                    {
                        valueFound = true;
                        Flags |= CollisionFlags.ForwardUp;
                        break;
                    }

                    if (!WorldManager.Raycast(Position, travelDeviation = Vector3.Blend(Position + Forward, Position + ForwardDown, i), out hit, out distances))
                    {
                        valueFound = true;
                        Flags |= CollisionFlags.ForwardDown;
                        break;
                    }
                }
            }

            if (!valueFound)
            {
                for (float i = 0.1f; i < 1; i += 0.1f)
                {
                    Vector3 hit;
                    Vector3 distances;
                    if (!WorldManager.Raycast(Position, travelDeviation = Vector3.Blend(Position + ForwardUp, Position + Up, i), out hit, out distances))
                    {
                        valueFound = true;
                        Flags |= CollisionFlags.ForwardUp;
                        break;
                    }

                    if (!WorldManager.Raycast(Position, travelDeviation = Vector3.Blend(Position + ForwardDown, Position + Down, i), out hit, out distances))
                    {
                        valueFound = true;
                        Flags |= CollisionFlags.ForwardDown;
                        break;
                    }
                }
            }

            if (!valueFound)
            {
                for (float i = 0.1f; i < 1; i += 0.1f)
                {
                    Vector3 hit;
                    Vector3 distances;
                    if (!WorldManager.Raycast(Position, travelDeviation = Vector3.Blend(Position + ForwardLeft, Position + Left, i), out hit, out distances))
                    {
                        valueFound = true;
                        Flags |= CollisionFlags.ForwardLeft;
                        break;
                    }

                    if (!WorldManager.Raycast(Position, travelDeviation = Vector3.Blend(Position + ForwardRight, Position + Right, i), out hit, out distances))
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
                ApplyMagnitude(ref travelDeviation);
            }

            return valueFound;
        }

        private void ApplyMagnitude(ref Vector3 deviationVector)
        {
            var direction = deviationVector - Position;

            direction.Normalize();

            // we used 4 the distance per waypoint
            direction *= forwardRange / 3.0f;

            deviationVector = Position + direction;
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
        private Vector3 ray2;

        private readonly float forwardRange;

        public readonly Collider PlayerCollider;

        public readonly Collider DestinationCollider;

        public Collisions(Vector3 position, Vector3 ray3, float forwardRange = 30.0f)
        {
            this.PlayerCollider = new Collider(position, ray3, forwardRange);
            //this.DestinationCollider = new Collider(position + ray3, -ray3, forwardRange);
        }

        public CollisionFlags CollisionResult(out Vector3 deviation)
        {
            deviation = Vector3.Zero;
            
            if (!PlayerCollider.IsFowardCollision())
            {
                return PlayerCollider.Flags;
            }

            this.PlayerCollider.BuildCollider();
            //this.DestinationCollider.BuildCollider();

            Vector3 playerDeviation;
            this.PlayerCollider.FindClosestDeviation(out playerDeviation);
            //Vector3 destinationDeviation;
            //this.DestinationCollider.FindClosestLateralDeviation(out destinationDeviation);

            // If no intersection, they are opposite directions...

            deviation = playerDeviation;

            return PlayerCollider.Flags;
        }
    }
}
