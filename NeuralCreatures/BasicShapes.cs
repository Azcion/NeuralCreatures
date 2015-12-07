using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace NeuralCreatures {

	public class BasicShapes {

		public void DrawLine (SpriteBatch batch, Vector2 start, Vector2 end,
		                      Color color, Texture2D texture, int size) {
			Vector2 edge = end - start;
			float angle = (float) Math.Atan2(edge.Y, edge.X);
			batch.Draw(texture, new Rectangle((int) start.X, (int) start.Y, (int) edge.Length(), size),
			           null, color, angle, new Vector2(0, 0), SpriteEffects.None, 0);
		}

	}

}