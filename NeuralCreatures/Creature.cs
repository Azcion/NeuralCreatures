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

		private Vector2 origin;
		private Texture2D texture;
		private int stripWidth;

		public Creature (Rectangle bounds, Texture2D texture, Color tint) {
			Brain = new NeuralNetwork(0, 4, 4, 3);
			Bounds = bounds;
			Position = new Vector2(Rand.Next(Bounds.Left, Bounds.Right),
								   Rand.Next(Bounds.Top, Bounds.Bottom));
			Tint = tint;

			stripWidth = texture.Width / texture.Height;
			Angle = Rand.Next(0, 360);
			Frame = Rand.Next(0, stripWidth);
			Life = 100;
			
			this.texture = texture;
			
			origin = new Vector2(texture.Height / 2, texture.Height / 2);
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

			double[] input = new double[4];

			Vector2 origin = new Vector2(32, 32);
			Vector2 leftSensor = ExtendedPoint(Position, Angle - 135, 100);
			Vector2 rightSensor = ExtendedPoint(Position, Angle - 45, 100);
			Food closestFoodItem = GetClosestFood(food, Position);
			Vector2 closestFood = closestFoodItem.Position;
			Vector2 closestObstacle = GetClosestObstacle(obstacles, Position).Position;
			double closestFoodLeft = GetDistance(closestFood, leftSensor);
			double closestFoodRight = GetDistance(closestFood, rightSensor);
			double closestObstacleLeft = GetDistance(closestObstacle, leftSensor);
			double closestObstacleRight = GetDistance(closestObstacle, rightSensor);
			double centerDistanceFood = GetDistance(closestFood, Position);
			double centerDistanceObstacle = GetDistance(closestObstacle, Position);

			if (centerDistanceFood < 50) {
				Life += 30;
				Fitness += 10;
				closestFoodItem.Position = new Vector2(Rand.Next(Bounds.Left, Bounds.Right),
										   Rand.Next(Bounds.Top, Bounds.Bottom));
			}

			Life -= .05;

			if (centerDistanceFood < centerDistanceObstacle) {
				if (closestFoodLeft > closestFoodRight) {
					input[0] = 1;
					input[1] = -1;
				} else {
					input[0] = -1;
					input[1] = 1;
				}
			} else {
				if (closestObstacleLeft > closestObstacleRight) {
					input[2] = 1;
					input[3] = -1;
				} else {
					input[2] = -1;
					input[3] = 1;
				}
			}

			double[] output = Brain.Run(input);

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
			double closestObstacleDist = 320000;

			foreach (Obstacle o in obstacles) {
				double dist = GetDistance(o.Position - origin, Position - origin);
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
				double dist = GetDistance(start, f.Position);
				if (dist < closest) {
					closest = dist;
					closestFood = f;
				}
			}

			return closestFood;
		}

		private Obstacle GetClosestObstacle (List<Obstacle> obstacles, Vector2 start) {
			Obstacle closestObstacle = new Obstacle(Bounds);
			double closest = 30000;

			foreach (Obstacle o in obstacles) {
				double dist = GetDistance(start, o.Position);
				if (dist < closest) {
					closest = dist;
					closestObstacle = o;
				}
			}

			return closestObstacle;
		}

		private double GetDistance (Vector2 start, Vector2 end) {
			Vector2 dist = start - end;

			return Math.Sqrt(dist.X * dist.X + dist.Y * dist.Y);
		}

		public Texture2D GetTexture () {
			return texture;
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

			Rectangle sourceRect = new Rectangle(Frame * texture.Height, 0, texture.Height, texture.Height);
			Rectangle destinRect = new Rectangle((int) Position.X, (int) Position.Y, 64 + 8 * Age, 64 + 8 * Age);

			batch.Draw(texture, destinRect, sourceRect, color, (float) (Angle * MathHelper.Pi / 180),
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