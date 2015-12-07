using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using static NeuralCreatures.NeuralNetwork;

namespace NeuralCreatures {

	public class GeneticAlgorithm {

		public int Generation;
		public int TotalMutations;

		public double AverageFitness;
		public double HighestFitness;

		public double CrossOverChance;
		public double ElitismChance;
		public double MutationChance;

		public List<Creature> NextGeneration;

		public GeneticAlgorithm (int elitismChance, int mutationChance) {
			CrossOverChance = 100 - elitismChance;
			ElitismChance = elitismChance;
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
				if (HighestFitness > 0) {
					if (!c.Dead) {
						c.ParentChance = c.Fitness / HighestFitness * 100;
					} else {
						c.ParentChance = 0;
					}
				} else {
					c.ParentChance = 100;
				}
			}
		}

		private void Elitism (List<Creature> creatures) {
			creatures = creatures.OrderByDescending(c => c.Fitness).ToList();
			int elitesCount = (int) (creatures.Count * ElitismChance / 100);

			for (int i = 0; i < elitesCount; ++i) {
				NextGeneration.Add(creatures[i]);
			}
		}

		private Creature Selection () {
			NextGeneration = NextGeneration.OrderBy(c => Guid.NewGuid()).ToList();
			int parentThreshold = Rand.Next(0, 100);
			Creature bestC = NextGeneration[0];

			foreach (Creature c in NextGeneration) {
				if (c.ParentChance > bestC.ParentChance) {
					bestC = c;
				}
				if (c.ParentChance > parentThreshold) {
					return c;
				}
			}

			return bestC;
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

				Creature child = new Creature(bounds, parentA.Texture, parentA.Tint) {
					Position = new Vector2(parentA.Position.X - 10, parentA.Position.Y - 10)
				};

				child.Brain.SetWeights(childWeights);
				NextGeneration.Add(child);
			}
		}

		private void Mutate () {
			foreach (Creature c in NextGeneration) {
				if (!(Rand.Next(0, 100) < MutationChance)) {
					continue;
				}

				int mutationPoint = Rand.Next(0, c.Brain.DendriteCount);
				double[] weights = c.Brain.GetWeights();
				weights[mutationPoint] = Rand.NextDouble();

				c.Brain.SetWeights(weights);
				c.Tint = new Color(Rand.Next(0, 255), Rand.Next(0, 255), Rand.Next(0, 255));
				++TotalMutations;
			}
		}

		private void CopyCreatures (List<Creature> creatures) {
			for (int i = 0; i < creatures.Count; ++i) {
				Creature c = NextGeneration[i];
				c.Reset();
				creatures[i] = c;
			}
		}

	}

}