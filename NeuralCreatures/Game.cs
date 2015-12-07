using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;

namespace NeuralCreatures {
	/// <summary>
	/// This is the main type for your game
	/// </summary>
	public class Game : Microsoft.Xna.Framework.Game {

		GraphicsDeviceManager graphics;
		SpriteBatch spriteBatch;

		World world;

		KeyboardState lastKS;

		public Game () {
			graphics = new GraphicsDeviceManager(this);
			graphics.PreferredBackBufferWidth = 1152;
			graphics.PreferredBackBufferHeight = 864;

			this.Window.AllowUserResizing = true;
			Content.RootDirectory = "Content";
			this.IsMouseVisible = true;
			InactiveSleepTime = new TimeSpan(0);
			this.IsFixedTimeStep = false;
			graphics.SynchronizeWithVerticalRetrace = false;
		}

		/// <summary>
		/// Allows the game to perform any initialization it needs to before starting to run.
		/// This is where it can query for any required services and load any non-graphic
		/// related content.  Calling base.Initialize will enumerate through any components
		/// and initialize them as well.
		/// </summary>
		protected override void Initialize () {
			world = new World(Content, new Rectangle(-2000, -2000, 4000, 4000),
							  graphics.GraphicsDevice.PresentationParameters.BackBufferWidth,
							  graphics.GraphicsDevice.PresentationParameters.BackBufferHeight);

			base.Initialize();
		}

		/// <summary>
		/// LoadContent will be called once per game and is the place to load
		/// all of your content.
		/// </summary>
		protected override void LoadContent () {
			// Create a new SpriteBatch, which can be used to draw textures.
			spriteBatch = new SpriteBatch(GraphicsDevice);
		}

		/// <summary>
		/// UnloadContent will be called once per game and is the place to unload
		/// all content.
		/// </summary>
		protected override void UnloadContent () {
		}

		/// <summary>
		/// Allows the game to run logic such as updating the world,
		/// checking for collisions, gathering input, and playing audio.
		/// </summary>
		/// <param name="gameTime">Provides a snapshot of timing values.</param>
		protected override void Update (GameTime gameTime) {
			ProcessInput();

			world.Update();

			base.Update(gameTime);
		}

		/// <summary>
		/// This is called when the game should draw itself.
		/// </summary>
		/// <param name="gameTime">Provides a snapshot of timing values.</param>
		protected override void Draw (GameTime gameTime) {
			GraphicsDevice.Clear(Color.SteelBlue);

			world.Draw(spriteBatch, gameTime.ElapsedGameTime.TotalSeconds,
					   graphics.GraphicsDevice.PresentationParameters.BackBufferWidth,
					   graphics.GraphicsDevice.PresentationParameters.BackBufferHeight);

			base.Draw(gameTime);
		}

		private void ProcessInput () {
			if (!IsActive) {
				return;
			}

			KeyboardState currentKS = Keyboard.GetState();

			// Exit game
			if (currentKS.IsKeyUp(Keys.Escape) && lastKS.IsKeyDown(Keys.Escape)) {
				this.Exit();
			}

			// Dump weights
			if (currentKS.IsKeyUp(Keys.RightControl) && lastKS.IsKeyDown(Keys.RightControl)) {
				world.Dump();
			}

			// Read weights
			if (currentKS.IsKeyDown(Keys.RightShift) && currentKS.IsKeyDown(Keys.Enter)
				&& lastKS.IsKeyUp(Keys.Enter)) {
				world.Read();
			}

			// Toggle vsync
			if (currentKS.IsKeyUp(Keys.T) && lastKS.IsKeyDown(Keys.T)) {
				IsFixedTimeStep = !IsFixedTimeStep;
				graphics.SynchronizeWithVerticalRetrace = !graphics.SynchronizeWithVerticalRetrace;
				graphics.ApplyChanges();
			}

			// Toggle scene
			if (currentKS.IsKeyUp(Keys.B) && lastKS.IsKeyDown(Keys.B)) {
				world.DoDrawScene = !world.DoDrawScene;
			}

			// Toggle graph
			if (currentKS.IsKeyUp(Keys.G) && lastKS.IsKeyDown(Keys.G)) {
				world.DoDrawGraph = !world.DoDrawGraph;
			}

			// Add random obstacles
			if (currentKS.IsKeyUp(Keys.O) && lastKS.IsKeyDown(Keys.O)) {
				world.AddObstacles(0);
			}

			lastKS = currentKS;
		}
	}
}
