using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace NeuralCreatures {

	internal class World {

		public GeneticAlgorithm GA;
		public Rectangle Bounds;
		public Camera Camera;
		public Texture2D TxCreature;
		public Texture2D TxPoint;
		public Texture2D TxFood;
		public SpriteFont Font;
		public BasicShapes Shapes;
		public FrameCounter Fps;

		public int Ticks;
		public int Deaths;
		public int TotalDeaths;
		public bool DoDrawScene;
		public bool DoDrawGraph;

		public readonly double[] GraphValues;

		public int CreatureCount = 100;
		public List<Creature> Creatures;

		public int FoodCount = 100;
		public List<Food> Food;

		public int ObstacleCount = 100;
		public List<Obstacle> Obstacles;


		public World (ContentManager content, Rectangle bounds, int width, int height) {
			LoadContent(content);

			Bounds = bounds;
			Shapes = new BasicShapes();
			Fps = new FrameCounter();

			Creatures = new List<Creature>();

			for (int i = 0; i < CreatureCount; ++i) {
				Creatures.Add(new Creature(bounds, TxCreature, Color.White));
			}

			Food = new List<Food>();
			Obstacles = new List<Obstacle>();

			AddFood(0);
			AddObstacles(0);

			Camera = new Camera(new Viewport(0, 0, width, height), width + width / 4, height);
			GA = new GeneticAlgorithm(75, 1);
			GraphValues = new double[30000];
			DoDrawScene = true;
			DoDrawGraph = true;
		}

		public void LoadContent (ContentManager content) {
			TxCreature = content.Load<Texture2D>("Butterfly");
			TxPoint = content.Load<Texture2D>("Point");
			TxFood = content.Load<Texture2D>("Food");
			Font = content.Load<SpriteFont>("Font");
		}

		public void Update () {
			Camera.Update();

			Deaths = 0;

			Creatures = Creatures.OrderByDescending(c => c.Age).ToList();

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
				if (GA.Generation < GraphValues.Length) {
					GraphValues[GA.Generation] = GA.Evolve(Creatures, Bounds);
				}
				if (GA.Generation % 10 == 0) {
					Obstacles.Clear();
					AddObstacles(0);
					if (GA.Generation == 1000) {
						DoDrawGraph = false;
					}
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

		public void Draw (SpriteBatch batch, double elapsedTime, int width, int height) {
			if (DoDrawScene) {
				batch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, null, null, null, null, Camera.Transform);

				foreach (Obstacle o in Obstacles) {
					o.Draw(batch, TxFood);
				}

				foreach (Food f in Food) {
					f.Draw(batch, TxFood);
				}

				foreach (Creature c in Creatures) {
					c.Draw(batch, Ticks);
				}

				Shapes.DrawLine(batch,
				                new Vector2(Bounds.Left - 100, Bounds.Top - 100),
				                new Vector2(Bounds.Right + 100, Bounds.Top - 100),
				                Color.Black, TxPoint, 4);
				Shapes.DrawLine(batch,
				                new Vector2(Bounds.Right + 100, Bounds.Top - 100),
				                new Vector2(Bounds.Right + 100, Bounds.Bottom + 100),
				                Color.Black, TxPoint, 4);
				Shapes.DrawLine(batch,
				                new Vector2(Bounds.Right + 100, Bounds.Bottom + 100),
				                new Vector2(Bounds.Left - 100, Bounds.Bottom + 100),
				                Color.Black, TxPoint, 4);
				Shapes.DrawLine(batch,
				                new Vector2(Bounds.Left - 100, Bounds.Bottom + 100),
				                new Vector2(Bounds.Left - 100, Bounds.Top - 100),
				                Color.Black, TxPoint, 4);

				batch.End();
			}

			batch.Begin();

			string stats = $"Generation: {GA.Generation}\n"
			               + $"Tick:       {Ticks}\n"
			               + $"Deaths:     {Deaths}\n"
			               + $"Elitism:    {GA.ElitismChance}%\n"
			               + $"Crossover:  {GA.CrossOverChance}%\n"
			               + $"Mutations:  {GA.TotalMutations}";

			batch.DrawString(Font, stats, new Vector2(10, 10), Color.Black);

			try {
				Fps.Update((float) elapsedTime);
				batch.DrawString(Font, "FPS: " + (int) Fps.AverageFramesPerSecond, new Vector2(10, 160), Color.Black);
			} catch (ArgumentException) {
			}

			const string controls = "Controls:\n"
			                        + " WASD   - pan\n"
			                        + " QE     - rotate\n"
			                        + " scroll - zoom\n"
			                        + " R      - reset view\n"
			                        + " T      - toggle vsync\n"
			                        + " B      - toggle scene\n"
			                        + " G      - toggle graph\n"
			                        + " O      - spawn obstacles\n"
			                        + " LMB    - place obstacle\n"
			                        + " RMB    - clear obstacles\n";

			batch.DrawString(Font, controls, new Vector2(10, 250), Color.LightGray);

			if (DoDrawGraph) {
				DrawGraph(batch, height);
			}

			batch.End();
		}

		private void DrawGraph (SpriteBatch batch, int height) {
			double scale = 1;
			double max = 0;

			for (int i = 0; i < GA.Generation; ++i) {
				if (GraphValues[i] > max) {
					max = GraphValues[i];
				}
			}

			if (max > 100) {
				scale = 100 / max;
			}

			Shapes.DrawLine(batch,
			                new Vector2(0, height),
			                new Vector2(2, (float) (height - GraphValues[0] * scale)),
			                Color.Black, TxPoint, 1);

			for (int i = 1; i < GA.Generation; ++i) {
				Shapes.DrawLine(batch,
				                new Vector2(i * 2, (float) (height - GraphValues[i - 1] * scale)),
				                new Vector2(i * 2 + 2, (float) (height - GraphValues[i] * scale)),
				                Color.Black, TxPoint, 1);
			}
		}

		public void Export () {
			try {
				int i = 0;
				string[] weightLines = new string[CreatureCount];
				string[] biasLines = new string[CreatureCount];

				foreach (Creature c in Creatures) {
					foreach (double weight in c.Brain.GetWeights()) {
						weightLines[i] += weight + " ";
					}
					foreach (double bias in c.Brain.GetBias()) {
						biasLines[i] += bias + " ";
					}

					++i;
				}

				File.WriteAllLines("_w.txt", weightLines);
				File.WriteAllLines("_b.txt", biasLines);

				Console.WriteLine("Export successful.");
			} catch (Exception e) {
				Console.WriteLine(e.StackTrace);
			}
		}

		public void Import () {
			try {
				int i = 0;
				string[] weightLines = File.ReadAllLines("_w.txt");
				string[] biasLines = File.ReadAllLines("_b.txt");

				foreach (Creature c in Creatures) {
					c.Kill();
					c.Reset();

					double[] weights = new double[c.Brain.DendriteCount];
					double[] biases = new double[c.Brain.NodeCount];

					string[] weightLine = weightLines[i].Split();
					string[] biasLine = biasLines[i].Split();

					for (int j = 0; j < c.Brain.DendriteCount; ++j) {
						weights[j] = double.Parse(weightLine[j]);
					}
					for (int j = 0; j < c.Brain.NodeCount; ++j) {
						biases[j] = double.Parse(biasLine[j]);
					}

					c.Brain.SetWeights(weights);
					c.Brain.SetBias(biases);
					++i;
				}

				Console.WriteLine("Import successful.");
			} catch (Exception e) {
				Console.WriteLine(e.StackTrace);
			}
		}

	}

}