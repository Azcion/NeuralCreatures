using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using static NeuralCreatures.NeuralNetwork;

namespace NeuralCreatures {

	public class Obstacle {

		private readonly Vector2 _origin;

		public Vector2 Position;

		public Obstacle (Rectangle bounds) {
			Position = new Vector2(Rand.Next(bounds.Left, bounds.Right),
			                       Rand.Next(bounds.Top, bounds.Bottom));
			_origin = new Vector2(25, 25);
		}

		public void Draw (SpriteBatch batch, Texture2D texture) {
			batch.Draw(texture, Position - _origin, Color.DarkRed);
		}

	}

}