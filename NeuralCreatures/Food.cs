using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using static NeuralCreatures.NeuralNetwork;

namespace NeuralCreatures {

	public class Food {

		public Vector2 Position;

		private Vector2 origin;

		public Food (Rectangle bounds) {
			Position = new Vector2(Rand.Next(bounds.Left, bounds.Right),
			                       Rand.Next(bounds.Top, bounds.Bottom));
			origin = new Vector2(25, 25);
		}

		public void Draw (SpriteBatch batch, Texture2D texture) {
			batch.Draw(texture, Position - origin, Color.DarkSeaGreen);
		}
	}
}