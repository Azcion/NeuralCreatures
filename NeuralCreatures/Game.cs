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

		KeyboardState lastKeyState;

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

			// TODO: use this.Content to load your game content here
		}

		/// <summary>
		/// UnloadContent will be called once per game and is the place to unload
		/// all content.
		/// </summary>
		protected override void UnloadContent () {
			// TODO: Unload any non ContentManager content here
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
			GraphicsDevice.Clear(Color.CornflowerBlue);

			world.Draw(spriteBatch, gameTime.ElapsedGameTime.TotalSeconds,
					   graphics.GraphicsDevice.PresentationParameters.BackBufferWidth,
					   graphics.GraphicsDevice.PresentationParameters.BackBufferHeight);

			base.Draw(gameTime);
		}

		private void ProcessInput () {
			if (!IsActive) {
				return;
			}

			// Allows the game to exit
			if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed) {
				this.Exit();
			}

			// Exit game
			if (Keyboard.GetState().IsKeyUp(Keys.Escape) && lastKeyState.IsKeyDown(Keys.Escape)) {
				this.Exit();
			}

			// Dump weights
			if (Keyboard.GetState().IsKeyUp(Keys.RightControl) && lastKeyState.IsKeyDown(Keys.RightControl)) {
				world.Dump();
			}

			// Read weights
			if (Keyboard.GetState().IsKeyDown(Keys.RightShift) && Keyboard.GetState().IsKeyDown(Keys.Enter)
				&& lastKeyState.IsKeyUp(Keys.Enter)) {
				world.Read();
			}

			// Toggle vsync
			if (Keyboard.GetState().IsKeyUp(Keys.T) && lastKeyState.IsKeyDown(Keys.T)) {
				IsFixedTimeStep = !IsFixedTimeStep;
				graphics.SynchronizeWithVerticalRetrace = !graphics.SynchronizeWithVerticalRetrace;
				graphics.ApplyChanges();
			}

			// Toggle scene
			if (Keyboard.GetState().IsKeyUp(Keys.B) && lastKeyState.IsKeyDown(Keys.B)) {
				world.DoDraw = !world.DoDraw;
			}

			// Add random obstacles
			if (Keyboard.GetState().IsKeyUp(Keys.O) && lastKeyState.IsKeyDown(Keys.O)) {
				world.AddObstacles(0);
			}

			// Cycle creatures
			if (Keyboard.GetState().IsKeyUp(Keys.Tab) && lastKeyState.IsKeyDown(Keys.Tab)) {
				world.CycleCreatures();
			}

			lastKeyState = Keyboard.GetState();
		}
	}
}
