using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

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

		public int FoodCount = 100;
		public List<Food> Food;

		public int ObstacleCount = 100;
		public List<Obstacle> Obstacles;

		private double[] graphValue;
		private int selectedCreature;

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
			Obstacles = new List<Obstacle>();

			AddFood(0);
			AddObstacles(0);

			Camera = new Camera(new Viewport(0, 0, width, height), width + width / 4, height);
			GA = new GeneticAlgorithm(75, 1);
			graphValue = new double[32000];
			DoDraw = true;
		}

		public void Update () {
			Camera.Update();

			Deaths = 0;

			foreach (Creature c in Creatures) {
				c.Update(Food, Obstacles);
				if (c.Life <= 0) {
					++Deaths;
				}
			}

			++Ticks;

			if (Ticks >= 1500 || Deaths == Creatures.Count) {
				TotalDeaths += Deaths;
				Ticks = 0;
				graphValue[GA.Generation] = GA.Evolve(Creatures, Bounds);

				if (GA.Generation % 10 == 0) {
					Obstacles.Clear();
					AddObstacles(0);
				}
			}

			ProcessInput();
		}

		private Point Origin (Vector2 v) {
			return new Point(Convert.ToInt32(v.X), Convert.ToInt32(v.Y));
		}

		private void ProcessInput () {
			if ((Mouse.GetState().LeftButton == ButtonState.Pressed) && (Ticks % 10 == 0)) {
				Vector2 mouse = new Vector2(Mouse.GetState().X, Mouse.GetState().Y);
				Obstacle obstacle = new Obstacle(Bounds);
				obstacle.Position = Vector2.Transform(mouse, Camera.InverseTransform);
				if (Bounds.Contains(Origin(obstacle.Position))) {
					Obstacles.Add(obstacle);
				}
			}

			if (Mouse.GetState().RightButton == ButtonState.Pressed) {
				Vector2 mouse = new Vector2(Mouse.GetState().X, Mouse.GetState().Y);
				mouse = Vector2.Transform(mouse, Camera.InverseTransform);
				if (Bounds.Contains(Origin(mouse))) {
					Obstacles.Clear();
				}
			}
		}

		public void AddFood (int style) {
			switch (style) {
				case 0:
					for (int i = 0; i < FoodCount; ++i) {
						Food.Add(new Food(Bounds));
					}
					break;
			}
		}

		public void AddObstacles (int style) {
			switch (style) {
				case 0:
					for (int i = 0; i < ObstacleCount; ++i) {
						Obstacles.Add(new Obstacle(Bounds));
					}
					break;
			}
		}

		public void CycleCreatures () {
			if (selectedCreature == CreatureCount - 1) {
				selectedCreature = 0;
			} else {
				++selectedCreature;
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
					c.Draw(batch, TxCreature, Color.White);
				}

				Creatures[selectedCreature].Draw(batch, TxCreature, Color.Red);

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

			string stats = string.Format( "Generation: {0}\n"
										+ "Tick:       {1}\n"
										+ "Deaths:     {2}\n"
										+ "Elitism:    {3}%\n"
										+ "Crossover:  {4}%\n"
										+ "Mutations:  {5}",
										GA.Generation, Ticks, Deaths, GA.ElitismChance,
										GA.CrossOverChance, GA.TotalMutations);
			batch.DrawString(Font, stats, new Vector2(10, 10), Color.Black);

			try {
				Fps.Update((float) elapsedTime);
				batch.DrawString(Font, "FPS: " + (int) Fps.AverageFramesPerSecond, new Vector2(10, 160), Color.Black);
			} catch (ArgumentException) {
			}

			string controls = "Controls:\n"
					+ " WASD   - pan\n"
					+ " QE     - rotate\n"
					+ " scroll - zoom\n"
					+ " R      - reset view\n"
					+ " T      - toggle vsync\n"
					+ " B      - toggle scene\n"
					+ " O      - spawn obstacles\n"
					+ " LMB    - place obstacle\n"
					+ " RMB    - clear obstacles\n"
					+ " Tab    - cycle creatures";

			batch.DrawString(Font, controls, new Vector2(10, 250), Color.DarkSlateGray);

			batch.DrawString(Font, Creatures[selectedCreature].ToString(), new Vector2(10, 550), Color.DarkRed);

			DrawGraph(batch, width, height);

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

		public void Dump () {
			try {
				int i = 0;
				string[] lines = new string[CreatureCount];

				foreach (Creature c in Creatures) {
					foreach (double weight in c.Brain.GetWeights()) {
						lines[i] += weight + " ";
					}
					++i;
				}

				File.WriteAllLines("weights.txt", lines);

				Console.WriteLine("Weights writing successful.");
			} catch (Exception) {
				return;
			}
		}

		public void Read () {
			try {
				int i = 0;
				string[] lines = File.ReadAllLines("weights.txt");
				double[] weights = new double[CreatureCount];

				foreach (Creature c in Creatures) {
					c.Kill();
					c.Reset();

					string[] line = lines[i].Split();

					for (int j = 0; j < c.Brain.GetDendriteCount(); ++j) {
						weights[j] = double.Parse(line[j]);
					}
					
					c.Brain.SetWeights(weights);
				}

				Console.WriteLine("Weights reading successful.");
			} catch (Exception) {
				return;
			}
		}
	}
}