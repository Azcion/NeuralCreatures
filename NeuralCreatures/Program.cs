namespace NeuralCreatures {

	#if WINDOWS
	internal static class Program {

		/// <summary>
		///     The main entry point for the application.
		/// </summary>
		private static void Main (string[] args) {
			using (Game game = new Game()) {
				game.Run();
			}
		}

	}
	#endif
}