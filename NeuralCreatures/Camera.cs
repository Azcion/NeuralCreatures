using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace NeuralCreatures {

	public class Camera {

		#region Fields

		protected float _zoom;
		protected Matrix _transform;
		protected Matrix _inverseTransform;
		protected Vector2 _pos;
		protected float _rotation;
		protected Viewport _viewport;
		protected MouseState _mouseState;
		protected KeyboardState _keyboardState;
		protected int _scroll;
		protected int _width;
		protected int _height;

		#endregion

		#region Properties

		public float Zoom {
			get { return _zoom; }
			set { _zoom = value; }
		}

		/// <summary>
		/// Camera view matrix property
		/// </summary>
		public Matrix Transform {
			get { return _transform; }
			set { _transform = value; }
		}

		/// <summary>
		/// Inverse of the view matrix. Can be used to get object's
		/// screen coordinates from its object coordinates
		/// </summary>
		public Matrix InverseTransform {
			get { return _inverseTransform; }
		}

		public Vector2 Pos {
			get { return _pos; }
			set { _pos = value; }
		}

		public float Rotation {
			get { return _rotation; }
			set { _rotation = value; }
		}

		#endregion

		#region Constructor

		public Camera (Viewport viewport, int width, int height) {
			_zoom = .2f;
			_scroll = 1;
			_rotation = 0f;
			_pos = new Vector2(width / 2, height / 2);
			_viewport = viewport;
			_width = width;
			_height = height;
		}

		#endregion

		#region Methods

		/// <summary>
		///  Update the camera view
		/// </summary>
		public void Update () {
			Input();
			_zoom = MathHelper.Clamp(_zoom, 0f, 10f);
			_rotation = ClampAngle(_rotation);
			_transform = Matrix.CreateRotationZ(_rotation) 
			           * Matrix.CreateScale(new Vector3(_zoom, _zoom, 1))
			           * Matrix.CreateTranslation(_pos.X, _pos.Y, 0);
			_inverseTransform = Matrix.Invert(_transform);
		}

		public Matrix Translate (Vector2 position, Vector2 origin) {
			return Matrix.CreateTranslation(new Vector3(-position, 0f))
				 * Matrix.CreateTranslation(new Vector3(-origin, 0f))
				 * Matrix.CreateRotationZ(_rotation)
				 * Matrix.CreateScale(_zoom, _zoom, 1)
				 * Matrix.CreateTranslation(new Vector3(origin, 0f));
		}

		/// <summary>
		/// Example input method. Rotates using cursor keys and zooms using mouse wheel
		/// </summary>
		protected virtual void Input () {
			_mouseState = Mouse.GetState();
			_keyboardState = Keyboard.GetState();

			// Check zoom
			if (_mouseState.ScrollWheelValue != _scroll) {
				_zoom += _mouseState.ScrollWheelValue > _scroll ? .01f : -.01f;
				_scroll = _mouseState.ScrollWheelValue;
			}

			foreach (Keys key in _keyboardState.GetPressedKeys()) {
				switch (key) {
					// Rotation
					case Keys.Q: _rotation -= .01f; break;
					case Keys.E: _rotation += .01f; break;
					// Movement
					case Keys.A: _pos.X += 4f; break;
					case Keys.D: _pos.X -= 4f; break;
					case Keys.W: _pos.Y += 4f; break;
					case Keys.S: _pos.Y -= 4f; break;
					// Reset
					case Keys.R:
						_zoom = .2f;
						_scroll = 1;
						_rotation = 0f;
						_pos = new Vector2(_width / 2, _height / 2);
						break;
				}
			}
		}

		/// <summary>
		/// Clamps a radian value between -pi and pi
		/// </summary>
		/// <param name="radians">Angle to be clamped</param>
		/// <returns>Clamped angle</returns>
		protected float ClampAngle (float radians) {
			while (radians < -MathHelper.Pi) {
				radians += MathHelper.TwoPi;
			}
			while (radians > MathHelper.Pi) {
				radians -= MathHelper.TwoPi;
			}
			return radians;
		}

		#endregion
	}
}