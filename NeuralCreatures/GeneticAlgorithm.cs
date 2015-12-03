using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using static NeuralCreatures.NeuralNetwork;

namespace NeuralCreatures {

	public class GeneticAlgorithm {

		public int Generation;
		public double ElitismChance;
		public double CrossOverChance;
		public double MutationChance;
		public double AverageFitness;
		public double HighestFitness;

		public List<Creature> NextGeneration;

		public GeneticAlgorithm (int crossOverChance, int mutationChance) {
			CrossOverChance = crossOverChance;
			ElitismChance = 100 - crossOverChance;
			MutationChance = mutationChance;
		}

		public double Evolve (List<Creature> creatures, Rectangle bounds) {
			NextGeneration = new List<Creature>();

			CalculateFitness(creatures);
			Elitism(creatures);
			CrossOver(creatures, bounds);
			Mutate();
			CopyCreatures(creatures);

			++Generation;

			NextGeneration.Clear();

			return AverageFitness;
		}

		private void CalculateFitness (List<Creature> creatures) {
			HighestFitness = 0;
			AverageFitness = 0;

			foreach (Creature c in creatures) {
				++c.Age;
				AverageFitness += c.Fitness;
				if (c.Fitness > HighestFitness) {
					HighestFitness = c.Fitness;
				}
			}

			AverageFitness /= creatures.Count;

			foreach (Creature c in creatures) {
				c.ParentChance = HighestFitness == 0 ? 100 : c.Fitness / HighestFitness * 100;
			}
		}

		private void Elitism (List<Creature> creatures) {
			creatures = creatures.OrderByDescending(Creature => Creature.Fitness).ToList();
			int elitesCount = (int) (creatures.Count * ElitismChance / 100);

			for (int i = 0; i < elitesCount; ++i) {
				NextGeneration.Add(creatures[i]);
			}
		}

		private Creature Selection () {
			NextGeneration = NextGeneration.OrderBy(Creature => Guid.NewGuid()).ToList();
			int parentThreshold = Rand.Next(0, 100);

			foreach (Creature c in NextGeneration) {
				if (c.ParentChance > parentThreshold) {
					return c;
				}
			}

			return null;
		}

		private void CrossOver (List<Creature> creatures, Rectangle bounds) {
			int crossOverCount = (int) (creatures.Count * CrossOverChance / 100);

			for (int i = 0; i < crossOverCount; ++i) {
				Creature parentA = Selection();
				Creature parentB = Selection();

				double[] parentAWeights = parentA.Brain.GetWeights();
				double[] parentBWeights = parentB.Brain.GetWeights();

				double[] childWeights = new double[parentAWeights.Length];

				int crossOverPoint = Rand.Next(0, parentAWeights.Length);

				for (int j = 0; j < crossOverPoint; ++j) {
					childWeights[j] = parentAWeights[j];
				}

				for (int j = crossOverPoint; j < parentAWeights.Length; ++j) {
					childWeights[j] = parentBWeights[j];
				}

				Creature child = new Creature(bounds);
				child.Position = new Vector2(parentA.Position.X - 10, parentA.Position.Y - 10);
				child.Brain.SetWeights(childWeights);
				NextGeneration.Add(child);
			}
		}

		private void Mutate () {
			foreach (Creature c in NextGeneration) {
				if (Rand.Next(0, 100) < MutationChance) {
					int MutationPoint = Rand.Next(0, c.Brain.GetDendriteCount());
					double[] weights = c.Brain.GetWeights();
					weights[MutationPoint] = Rand.NextDouble();
					c.Brain.SetWeights(weights);
				}
			}
		}

		private void CopyCreatures (List<Creature> creatures) {
			for (int i = 0; i < creatures.Count; ++i) {
				creatures[i] = NextGeneration[i];
				creatures[i].Reset();
			}
		}
	}
}