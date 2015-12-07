using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using static NeuralCreatures.NeuralNetwork;

namespace NeuralCreatures {

	public class Creature {

		public int Age;
		public Vector2 Position;
		public double Angle;
		public NeuralNetwork Brain;
		public int Frame;
		public Rectangle Bounds;
		public Color Tint;

		public bool Dead;
		public double Life;
		public double Fitness;
		public double ParentChance;

		protected Texture2D _texture;

		public Texture2D Texture {
			get { return _texture; }
			set {
				_texture = value;
				stripWidth = value.Width / value.Height;
				origin = new Vector2(value.Height / 2, value.Height / 2);
			}
		}

		private Vector2 origin;
		private Vector2[,] edges;
		private float[] edgeLengths;
		private int stripWidth;

		public Creature (Rectangle bounds, Texture2D texture, Color tint) {
			Brain = new NeuralNetwork(0, 4, 4, 3);
			Bounds = bounds;
			Position = new Vector2(Rand.Next(Bounds.Left, Bounds.Right),
								   Rand.Next(Bounds.Top, Bounds.Bottom));
			
			edges = new Vector2[4, 2] {
				{ new Vector2(bounds.Left, bounds.Top), new Vector2(bounds.Right, bounds.Top)},
				{ new Vector2(bounds.Right, bounds.Top), new Vector2(bounds.Right, bounds.Bottom)},
				{ new Vector2(bounds.Right, bounds.Bottom), new Vector2(bounds.Left, bounds.Bottom)},
				{ new Vector2(bounds.Left, bounds.Bottom), new Vector2(bounds.Left, bounds.Top)}
			};

			edgeLengths = new float[4] {
				Vector2.DistanceSquared(edges[0, 0], edges[0, 1]),
				Vector2.DistanceSquared(edges[1, 0], edges[1, 1]),
				Vector2.DistanceSquared(edges[2, 0], edges[2, 1]),
				Vector2.DistanceSquared(edges[3, 0], edges[3, 1])
			};

			Angle = Rand.Next(0, 360);
			Frame = Rand.Next(0, stripWidth);
			Life = 100;
			
			Texture = texture;
			Tint = tint;
		}

		public void Reset () {
			Rand = new Random(Guid.NewGuid().GetHashCode());
			Angle = Rand.Next(0, 360);
			Frame = Rand.Next(0, 8);
			Dead = false;
			Fitness = 0;
			Life = 100;
		}

		public void Kill () {
			Dead = true;
			Fitness = 0;
			Life = 0;
			ParentChance = 0;
			Age = 0;
		}

		public void Update (List<Food> food, List<Obstacle> obstacles) {
			if (!Dead && Life <= 0) {
				Kill();
				return;
			} else if (Dead) {
				return;
			}

			Vector2 origin = new Vector2(32, 32);

			Vector2 sensorL = ExtendedPoint(Position, Angle - 135, 100);
			Vector2 sensorR = ExtendedPoint(Position, Angle - 45, 100);

			Food closestFoodItem = GetClosestFood(food, Position);
			Vector2 closestFood = closestFoodItem.Position;
			Vector2 closestObst = GetClosestObstacle(obstacles, Position).Position;

			double closestFoodL = Vector2.Distance(closestFood, sensorL);
			double closestFoodR = Vector2.Distance(closestFood, sensorR);
			double closestObstL = Vector2.Distance(closestObst, sensorL);
			double closestObstR = Vector2.Distance(closestObst, sensorR);
			double centerDistFood = Vector2.Distance(closestFood, Position);
			double centerDistObst = Vector2.Distance(closestObst, Position);

			if (centerDistFood < 50) {
				Life += 30;
				Fitness += 10;
				closestFoodItem.Position = new Vector2(Rand.Next(Bounds.Left, Bounds.Right),
										   Rand.Next(Bounds.Top, Bounds.Bottom));
			}

			Life -= .075;

			// {food left, food right, obstacle left, obstacle right}
			double[] input = new double[4];

			if (centerDistFood < centerDistObst) {
				if (closestFoodL > closestFoodR) {
					input[0] = 1;
					input[1] = -1;
				} else {
					input[0] = -1;
					input[1] = 1;
				}
			} else {
				if (closestObstL > closestObstR) {
					input[2] = 1;
					input[3] = -1;
				} else {
					input[2] = -1;
					input[3] = 1;
				}
			}

			// {rotate left, rotate right, forward}
			double[] output = Brain.Run(input);

			ProcessOutput(output, obstacles);
		}

		private void ProcessOutput (double[] output, List<Obstacle> obstacles) {
			if (output[0] > output[1]) {
				Angle += output[0] * 4;
			} else {
				Angle -= output[1] * 4;
			}

			double speed = output[2] * 2;
			float radians = (float) (Angle - 90) * MathHelper.Pi / 180;
			Vector2 oldPos = Position;
			Position.X += (float) (Math.Cos(radians) * speed);
			Position.Y += (float) (Math.Sin(radians) * speed);
			double closestObstacleDist = 30000;

			foreach (Obstacle o in obstacles) {
				double dist = Vector2.Distance(o.Position - origin, Position - origin);
				if (dist < closestObstacleDist) {
					closestObstacleDist = dist;
				}
			}

			if (closestObstacleDist < 50) {
				Life -= 1;
				Fitness -= 10;
				Fitness = Fitness < 0 ? 0 : Fitness;
				Position = oldPos;
			}

			if (Position.X < Bounds.Left) {
				Position.X = Bounds.Left;
			}
			if (Position.X > Bounds.Right) {
				Position.X = Bounds.Right;
			}
			if (Position.Y < Bounds.Top) {
				Position.Y = Bounds.Top;
			}
			if (Position.Y > Bounds.Bottom) {
				Position.Y = Bounds.Bottom;
			}
		}

		private Vector2 ExtendedPoint (Vector2 center, double directionAngle, int length) {
			float radians = (float) directionAngle * MathHelper.Pi / 180;
			Vector2 position;
			position.X = (float) (center.X + Math.Cos(radians) * length);
			position.Y = (float) (center.Y + Math.Sin(radians) * length);

			return position;
		}

		private Food GetClosestFood (List<Food> food, Vector2 start) {
			Food closestFood = new Food(Bounds);
			double closest = 30000;

			foreach (Food f in food) {
				double dist = Vector2.Distance(start, f.Position);
				if (dist < closest) {
					closest = dist;
					closestFood = f;
				}
			}

			return closestFood;
		}

		private Obstacle GetClosestObstacle (List<Obstacle> obstacles, Vector2 start) {
			Obstacle closestObst = new Obstacle(Bounds);
			Obstacle closestEdge = GetClosestEdge(start);
			double closest = 30000;
			double distEdge = Vector2.Distance(start, closestEdge.Position);

			foreach (Obstacle o in obstacles) {
				double dist = Vector2.Distance(start, o.Position);
				if (dist < closest) {
					closest = dist;
					closestObst = o;
				}
			}

			if (distEdge < closest) {
				closestObst = closestEdge;
			}

			return closestObst;
		}

		private Obstacle GetClosestEdge (Vector2 start) {
			Obstacle edgeObst = new Obstacle(Bounds);
			float closest = 30000;

			for (int i = 0; i < 4; ++i) {
				Vector2 projection = ProjectionOnEdge(start, edges[i, 0], edges[i, 1], edgeLengths[i]);
				float dist = Vector2.Distance(start, projection);
				if (dist < closest) {
					closest = dist;
					edgeObst.Position = projection;
				}
			}

			return edgeObst;
		}

		private Vector2 ProjectionOnEdge (Vector2 start, Vector2 cornerA, Vector2 cornerB, float distanceSquared) {
			float t = Vector2.Dot(start - cornerA, cornerB - cornerA) / distanceSquared;

			if (t < 0) {
				return cornerA;
			} else if (t > 1) {
				return cornerB;
			}

			return cornerA + t * (cornerB - cornerA);
		}

		public void Draw (SpriteBatch batch, Color color, int ticks) {
			if (Life <= 0) {
				return;
			}

			// Animation
			if (ticks % 5 == 0) {
				++Frame;
				if (Frame > stripWidth - 1) {
					Frame = 0;
				}
			}

			Rectangle sourceRect = new Rectangle(Frame * Texture.Height, 0, Texture.Height, Texture.Height);
			Rectangle destinRect = new Rectangle((int) Position.X, (int) Position.Y, 64 + 8 * Age, 64 + 8 * Age);

			batch.Draw(Texture, destinRect, sourceRect, color, (float) (Angle * MathHelper.Pi / 180),
				       origin, SpriteEffects.None, 0f);
		}

		public override string ToString () {
			return string.Format("Dead: {0}\n"
								+ "Life: {1}\n"
								+ "Fitness: {2}\n"
								+ "Parent: {3}%",
								Dead, (float) Life, Fitness, (float) ParentChance);
		}

		public void Draw (SpriteBatch batch, int ticks) {
			Draw(batch, Tint, ticks);
		}
	}
}