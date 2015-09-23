namespace ExBuddy.Navigation
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Runtime.Caching;

	using Clio.Utilities;

	using ExBuddy.Logging;

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
			this.forwardNormal = this.Forward = direction;
			this.Forward *= forwardRange;
			this.Direction2D = new Vector3(direction.X, 0, direction.Z);
			this.Direction2D.Normalize();
			this.Position = position;
			this.forwardRange = forwardRange;
		}

		public bool IsFowardCollision()
		{
			Vector3 hit;
			Vector3 distances;

			//  Forward
			if (!WorldManager.Raycast(this.Position, this.Position + this.Forward, out hit, out distances))
			{
				return false;
			}

			this.Flags |= CollisionFlags.Forward;
			this.ForwardHit = hit;

			return true;
		}

		public void BuildCollider()
		{
			// Find raycast vectors
			this.Right = Vector3.Cross(this.Forward, this.Direction2D);
			this.Left = new Vector3(-this.Right.X, this.Right.Y, -this.Right.Z);
			this.Up = Vector3.Cross(this.Right, this.Forward);

			// If greater than 0, we ar heading up.
			if (this.Forward.Y > 0)
			{
				this.Down = this.Up;
				this.Up = -this.Up;
			}
			else
			{
				this.Down = -this.Up;
			}

			this.Right.Normalize();
			this.Up.Normalize();
			this.Down.Normalize();
			this.Left.Normalize();

			this.ForwardRight = Vector3.Blend(this.Right, this.forwardNormal, 0.5f);
			this.ForwardUp = Vector3.Blend(this.Up, this.forwardNormal, 0.5f);
			this.ForwardLeft = Vector3.Blend(this.Left, this.forwardNormal, 0.5f);
			this.ForwardDown = Vector3.Blend(this.Down, this.forwardNormal, 0.5f);

			this.ForwardRight.Normalize();
			this.ForwardUp.Normalize();
			this.ForwardLeft.Normalize();
			this.ForwardDown.Normalize();

			var diagonalMagnitude = this.forwardRange / (float)Math.Cos(45.0f * Math.PI / 180);
			this.Right *= this.forwardRange;
			this.Up *= this.forwardRange;
			this.Left *= this.forwardRange;
			this.Down *= this.forwardRange;

			this.ForwardRight *= diagonalMagnitude;
			this.ForwardUp *= diagonalMagnitude;
			this.ForwardLeft *= diagonalMagnitude;
			this.ForwardDown *= diagonalMagnitude;
		}

		public bool FindClosestDeviation(ICollection<FlightPoint> previousFlightPoints, out Vector3 travelDeviation)
		{
			var deviation = travelDeviation = Vector3.Zero;
			var valueFound = false;

			// Making the rays 1.5x as long as our detection range to ensure it is clear
			var forwardRay = this.Position + this.Forward * 1.5f;
			var forwardRightRay = this.Position + this.ForwardRight * 1.5f;
			var forwardLeftRay = this.Position + this.ForwardLeft * 1.5f;
			var forwardUpRay = Vector3.Zero;
			var forwardDownRay = Vector3.Zero;
			Vector3 hit;
			Vector3 distances;

			for (var i = 0.2f; i < 1; i += 0.2f)
			{
				if (
					!WorldManager.Raycast(
						this.Position,
						deviation = Vector3.Blend(forwardRay, forwardRightRay, i),
						out hit,
						out distances) && !previousFlightPoints.Any(fp => fp.FuzzyEquals(deviation))
					&& !MemoryCache.Default.Contains(deviation.ToString()))
				{
					valueFound = true;
					this.Flags |= CollisionFlags.ForwardRight;
					break;
				}

				if (
					!WorldManager.Raycast(
						this.Position,
						deviation = Vector3.Blend(forwardRay, forwardLeftRay, i),
						out hit,
						out distances) && !previousFlightPoints.Any(fp => fp.FuzzyEquals(deviation))
					&& !MemoryCache.Default.Contains(deviation.ToString()))
				{
					valueFound = true;
					this.Flags |= CollisionFlags.ForwardLeft;
					break;
				}
			}

			if (!valueFound)
			{
				forwardUpRay = this.Position + this.ForwardUp * 1.5f;
				forwardDownRay = this.Position + this.ForwardDown * 1.5f;

				for (var i = 0.2f; i < 1; i += 0.2f)
				{
					if (
						!WorldManager.Raycast(
							this.Position,
							deviation = Vector3.Blend(forwardRay, forwardUpRay, i),
							out hit,
							out distances) && !previousFlightPoints.Any(fp => fp.FuzzyEquals(deviation))
						&& !MemoryCache.Default.Contains(deviation.ToString()))
					{
						valueFound = true;
						this.Flags |= CollisionFlags.ForwardUp;
						break;
					}

					if (
						!WorldManager.Raycast(
							this.Position,
							deviation = Vector3.Blend(forwardRay, forwardDownRay, i),
							out hit,
							out distances) && !previousFlightPoints.Any(fp => fp.FuzzyEquals(deviation))
						&& !MemoryCache.Default.Contains(deviation.ToString()))
					{
						valueFound = true;
						this.Flags |= CollisionFlags.ForwardDown;
						break;
					}
				}
			}

			if (!valueFound)
			{
				var upRay = this.Position + this.Up * 1.5f;
				var downRay = this.Position + this.Down * 1.5f;

				for (var i = 0.2f; i < 1; i += 0.2f)
				{
					if (!WorldManager.Raycast(this.Position, deviation = Vector3.Blend(forwardUpRay, upRay, i), out hit, out distances)
						&& !previousFlightPoints.Any(fp => fp.FuzzyEquals(deviation))
						&& !MemoryCache.Default.Contains(deviation.ToString()))
					{
						valueFound = true;
						this.Flags |= CollisionFlags.ForwardUp;
						break;
					}

					if (
						!WorldManager.Raycast(
							this.Position,
							deviation = Vector3.Blend(forwardDownRay, downRay, i),
							out hit,
							out distances) && !previousFlightPoints.Any(fp => fp.FuzzyEquals(deviation))
						&& !MemoryCache.Default.Contains(deviation.ToString()))
					{
						valueFound = true;
						this.Flags |= CollisionFlags.ForwardDown;
						break;
					}
				}
			}

			if (!valueFound)
			{
				var leftRay = this.Position + this.Left * 2;
				var rightRay = this.Position + this.Right * 2;

				for (var i = 0.2f; i < 1; i += 0.2f)
				{
					if (
						!WorldManager.Raycast(
							this.Position,
							deviation = Vector3.Blend(forwardLeftRay, leftRay, i),
							out hit,
							out distances) && !previousFlightPoints.Any(fp => fp.FuzzyEquals(deviation))
						&& !MemoryCache.Default.Contains(deviation.ToString()))
					{
						valueFound = true;
						this.Flags |= CollisionFlags.ForwardLeft;
						break;
					}

					if (
						!WorldManager.Raycast(
							this.Position,
							deviation = Vector3.Blend(forwardRightRay, rightRay, i),
							out hit,
							out distances) && !previousFlightPoints.Any(fp => fp.FuzzyEquals(deviation))
						&& !MemoryCache.Default.Contains(deviation.ToString()))
					{
						valueFound = true;
						this.Flags |= CollisionFlags.ForwardRight;
						break;
					}
				}
			}

			if (!valueFound)
			{
				this.Flags |= CollisionFlags.Error;
			}
			else
			{
				travelDeviation = deviation;

				Logger.Instance.Verbose("Direction of deviation: " + (Flags ^ CollisionFlags.Forward));
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
			//DestinationCollider = new Collider(position + ray3, -ray3, forwardRange);
		}

		public CollisionFlags CollisionResult(ICollection<FlightPoint> previousFlightPoints, out Vector3 deviation)
		{
			deviation = Vector3.Zero;

			if (!this.PlayerCollider.IsFowardCollision())
			{
				return this.PlayerCollider.Flags;
			}

			this.PlayerCollider.BuildCollider();
			//DestinationCollider.BuildCollider();

			Vector3 playerDeviation;
			this.PlayerCollider.FindClosestDeviation(previousFlightPoints, out playerDeviation);

			deviation = playerDeviation;

			return this.PlayerCollider.Flags;
		}
	}
}