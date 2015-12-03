using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;

namespace NeuralCreatures {

	internal class World {

		public Rectangle Bounds;
		public Camera Camera;
		public Texture2D TxCreature;
		public Texture2D TxPoint;
		public Texture2D TxFood;
		public SpriteFont Font;
		public BasicShapes Shapes;
		public int Ticks;
		public GeneticAlgorithm GA;
		public int Deaths;
		public int TotalDeaths;
		public bool DoDraw;
		public FrameCounter Fps;

		public int CreatureCount = 100;
		public List<Creature> Creatures;

		public int FoodCount = 200;
		public List<Food> Food;

		public int ObstacleCount = 0;
		public List<Obstacle> Obstacles;

		private double[] graphValue;

		public World (ContentManager content, Rectangle bounds, int width, int height) {
			Bounds = bounds;
			TxCreature = content.Load<Texture2D>("Bug");
			TxPoint = content.Load<Texture2D>("Point");
			TxFood = content.Load<Texture2D>("Food");
			Font = content.Load<SpriteFont>("Font");
			Shapes = new BasicShapes();
			Fps = new FrameCounter();

			Creatures = new List<Creature>();

			for (int i = 0; i < CreatureCount; ++i) {
				Creatures.Add(new Creature(bounds));
			}

			Food = new List<Food>();

			for (int i = 0; i < FoodCount; ++i) {
				Food.Add(new Food(bounds));
			}

			Obstacles = new List<Obstacle>();

			for (int i = 0; i < ObstacleCount; ++i) {
				Obstacles.Add(new Obstacle(bounds));
			}

			Camera = new Camera(new Viewport(0, 0, width, height), width + width / 4, height);
			GA = new GeneticAlgorithm(30, 1);
			Deaths = 0;
			TotalDeaths = 0;
			graphValue = new double[32000];
			DoDraw = true;
		}

		public void Update () {
			Camera.Update();

			Deaths = 0;

			foreach (Creature c in Creatures) {
				c.Update(Food, Obstacles);
				Deaths += c.Life <= 0 ? 1 : 0;
			}

			TotalDeaths += Deaths;

			++Ticks;

			if (Ticks >= 1000 || Deaths == Creatures.Count) {
				Ticks = 0;
				graphValue[GA.Generation] = GA.Evolve(Creatures, Bounds);
			}

			if (Mouse.GetState().LeftButton == ButtonState.Pressed) {
				if (Ticks % 10 == 0) {
					Obstacle newObstacle = new Obstacle(Bounds);
					newObstacle.Position = Vector2.Transform(new Vector2(Mouse.GetState().X, Mouse.GetState().Y),
					                                         Camera.InverseTransform);
					Obstacles.Add(newObstacle);
				}
			}

			if (Mouse.GetState().RightButton == ButtonState.Pressed) {
				Obstacles.Clear();
			}
		}

		public void Draw (SpriteBatch batch, double elapsedTime, int width, int height) {
			if (DoDraw) {
				batch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, null, null, null, null, Camera.Transform);

				foreach (Obstacle o in Obstacles) {
					o.Draw(batch, TxFood);
				}

				foreach (Food f in Food) {
					f.Draw(batch, TxFood);
				}

				foreach (Creature c in Creatures) {
					c.Draw(batch, TxCreature);
				}

				Shapes.DrawLine(batch, new Vector2(Bounds.Left - 100, Bounds.Top - 100),
									   new Vector2(Bounds.Right + 100, Bounds.Top - 100),
									   Color.Black, TxPoint, 4);
				Shapes.DrawLine(batch, new Vector2(Bounds.Right + 100, Bounds.Top - 100),
									   new Vector2(Bounds.Right + 100, Bounds.Bottom + 100),
									   Color.Black, TxPoint, 4);
				Shapes.DrawLine(batch, new Vector2(Bounds.Right + 100, Bounds.Bottom + 100),
									   new Vector2(Bounds.Left - 100, Bounds.Bottom + 100),
									   Color.Black, TxPoint, 4);
				Shapes.DrawLine(batch, new Vector2(Bounds.Left - 100, Bounds.Bottom + 100),
									   new Vector2(Bounds.Left - 100, Bounds.Top - 100),
									   Color.Black, TxPoint, 4);

				batch.End();
			}

			batch.Begin();

			batch.DrawString(Font, "Generation: " + GA.Generation, new Vector2(10, 10), Color.Black);
			batch.DrawString(Font, "Tick:       " + Ticks, new Vector2(10, 30), Color.Black);
			batch.DrawString(Font, "Deaths:     " + TotalDeaths, new Vector2(10, 50), Color.Black);
			batch.DrawString(Font, "Elitism:    " + GA.ElitismChance + "%", new Vector2(10, 70), Color.Black);
			batch.DrawString(Font, "Crossover:  " + GA.CrossOverChance + "%", new Vector2(10, 90), Color.Black);
			batch.DrawString(Font, "Mutation:   " + GA.MutationChance + "%", new Vector2(10, 110), Color.Black);
			DrawGraph(batch, width, height);

			Fps.Update((float) elapsedTime);
			try {
				batch.DrawString(Font, "FPS: " + (int) Fps.AverageFramesPerSecond, new Vector2(10, 130), Color.Black);
			} catch (System.ArgumentException) {
			}

			batch.End();
		}

		private void DrawGraph (SpriteBatch batch, int width, int height) {
			double scale = 1;
			double max = 0;
			double total = 0;

			for (int i = 0; i < GA.Generation; ++i) {
				total += graphValue[i];
				if (graphValue[i] > max) {
					max = graphValue[i];
				}
			}

			double average = total / GA.Generation;

			scale = max > 100 ? 100 / max : scale;

			Shapes.DrawLine(batch, new Vector2(0, height),
								   new Vector2(2, (float) (height - graphValue[0] * scale)),
								   Color.Black, TxPoint, 1);

			for (int i = 1; i < GA.Generation; ++i) {
				Shapes.DrawLine(batch, new Vector2(i * 2, (float) (height - graphValue[i - 1] * scale)),
									   new Vector2(i * 2 + 2, (float) (height - graphValue[i] * scale)),
									   Color.Black, TxPoint, 1);
			}

			Shapes.DrawLine(batch, new Vector2(0, (float) (height - average * scale)),
								   new Vector2(2, (float) (width - average * scale)),
								   Color.Blue, TxPoint, 1);
		}
	}
}