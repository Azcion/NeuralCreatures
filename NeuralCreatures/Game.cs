using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace NeuralCreatures {

	/// <summary>
	///     This is the main type for your game
	/// </summary>
	public class Game : Microsoft.Xna.Framework.Game {

		private KeyboardState _lastKeyState;
		private SpriteBatch _spriteBatch;
		private World _world;

		protected GraphicsDeviceManager Graphics;

		public Game () {
			Graphics = new GraphicsDeviceManager(this) {
				PreferredBackBufferWidth = 1152,
				PreferredBackBufferHeight = 864
			};

			Window.AllowUserResizing = true;
			Content.RootDirectory = "Content";
			IsMouseVisible = true;
			InactiveSleepTime = new TimeSpan(0);
			IsFixedTimeStep = false;
			Graphics.SynchronizeWithVerticalRetrace = false;
		}

		/// <summary>
		///     Allows the game to perform any initialization it needs to before starting to run.
		///     This is where it can query for any required services and load any non-graphic
		///     related content.  Calling base.Initialize will enumerate through any components
		///     and initialize them as well.
		/// </summary>
		protected override void Initialize () {
			_world = new World(Content, new Rectangle(-2000, -2000, 4000, 4000),
			                   Graphics.GraphicsDevice.PresentationParameters.BackBufferWidth,
			                   Graphics.GraphicsDevice.PresentationParameters.BackBufferHeight);

			base.Initialize();
		}

		/// <summary>
		///     LoadContent will be called once per game and is the place to load
		///     all of your content.
		/// </summary>
		protected override void LoadContent () {
			// Create a new SpriteBatch, which can be used to draw textures.
			_spriteBatch = new SpriteBatch(GraphicsDevice);
		}

		/// <summary>
		///     UnloadContent will be called once per game and is the place to unload
		///     all content.
		/// </summary>
		protected override void UnloadContent () {
		}

		/// <summary>
		///     Allows the game to run logic such as updating the _world,
		///     checking for collisions, gathering input, and playing audio.
		/// </summary>
		/// <param name="gameTime">Provides a snapshot of timing values.</param>
		protected override void Update (GameTime gameTime) {
			ProcessInput();

			_world.Update();

			base.Update(gameTime);
		}

		/// <summary>
		///     This is called when the game should draw itself.
		/// </summary>
		/// <param name="gameTime">Provides a snapshot of timing values.</param>
		protected override void Draw (GameTime gameTime) {
			GraphicsDevice.Clear(Color.SteelBlue);

			_world.Draw(_spriteBatch, gameTime.ElapsedGameTime.TotalSeconds,
			            Graphics.GraphicsDevice.PresentationParameters.BackBufferWidth,
			            Graphics.GraphicsDevice.PresentationParameters.BackBufferHeight);

			base.Draw(gameTime);
		}

		private void ProcessInput () {
			if (!IsActive) {
				return;
			}

			KeyboardState currentKeyState = Keyboard.GetState();

			// Exit game
			if (currentKeyState.IsKeyUp(Keys.Escape) && _lastKeyState.IsKeyDown(Keys.Escape)) {
				Exit();
			}

			// Dump weights
			if (currentKeyState.IsKeyUp(Keys.RightControl) && _lastKeyState.IsKeyDown(Keys.RightControl)) {
				_world.Export();
			}

			// Read weights
			if (currentKeyState.IsKeyDown(Keys.RightShift) && currentKeyState.IsKeyDown(Keys.Enter)
			    && _lastKeyState.IsKeyUp(Keys.Enter)) {
				_world.Import();
			}

			// Toggle vsync
			if (currentKeyState.IsKeyUp(Keys.T) && _lastKeyState.IsKeyDown(Keys.T)) {
				IsFixedTimeStep = !IsFixedTimeStep;
				Graphics.SynchronizeWithVerticalRetrace = !Graphics.SynchronizeWithVerticalRetrace;
				Graphics.ApplyChanges();
			}

			// Toggle scene
			if (currentKeyState.IsKeyUp(Keys.B) && _lastKeyState.IsKeyDown(Keys.B)) {
				_world.DoDrawScene = !_world.DoDrawScene;
			}

			// Toggle graph
			if (currentKeyState.IsKeyUp(Keys.G) && _lastKeyState.IsKeyDown(Keys.G)) {
				_world.DoDrawGraph = !_world.DoDrawGraph;
			}

			// Add random obstacles
			if (currentKeyState.IsKeyUp(Keys.O) && _lastKeyState.IsKeyDown(Keys.O)) {
				_world.AddObstacles(0);
			}

			_lastKeyState = currentKeyState;
		}

	}

}