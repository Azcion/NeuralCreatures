using System;

namespace NeuralCreatures {

	public class NeuralNetwork {

		public static Random Rand = new Random(Guid.NewGuid().GetHashCode());

		public double Fitness;
		public int DendriteCount;
		public int NodeCount;

		public struct Dendrite {

			public double Weight;

		}

		public struct Neuron {

			public Dendrite[] Dendrites;
			public int DendriteCount;
			public double Bias;
			public double Value;
			public double Delta;

		}

		public struct Layer {

			public Neuron[] Neurons;
			public int NeuronCount;

		}

		public struct Network {

			public Layer[] Layers;
			public int LayerCount;
			public double LearningRate;

		}

		public Network Brain;

		public NeuralNetwork (double learningRate, int inputs, int hidden, int outputs) {
			double[] layerArr = new double[3];
			layerArr[0] = inputs;
			layerArr[1] = hidden;
			layerArr[2] = outputs;

			Brain.LayerCount = layerArr.Length;

			if (Brain.LayerCount < 2) {
				return;
			}

			Fitness = 0;
			Brain.LearningRate = learningRate;
			Brain.Layers = new Layer[Brain.LayerCount];

			for (int i = 0; i < Brain.LayerCount; ++i) {
				Brain.Layers[i].NeuronCount = (int) layerArr[i];
				Brain.Layers[i].Neurons = new Neuron[(int) layerArr[i]];

				for (int j = 0; j < layerArr[i]; ++j) {
					if (i == 0) {
						continue;
					}
					Brain.Layers[i].Neurons[j].Bias = GetRand();
					Brain.Layers[i].Neurons[j].DendriteCount = (int) layerArr[i - 1];
					Brain.Layers[i].Neurons[j].Dendrites = new Dendrite[(int) layerArr[i - 1]];

					for (int k = 0; k < layerArr[i - 1]; ++k) {
						Brain.Layers[i].Neurons[j].Dendrites[k].Weight = GetRand();
					}
				}
			}

			DendriteCount = GetDendriteCount();
			NodeCount = hidden + outputs;
		}

		public double[] Run (double[] inputArr) {
			if (inputArr.Length != Brain.Layers[0].NeuronCount) {
				return null;
			}

			for (int i = 0; i < Brain.LayerCount; ++i) {
				for (int j = 0; j < Brain.Layers[i].NeuronCount; ++j) {
					if (i == 0) {
						Brain.Layers[0].Neurons[j].Value = inputArr[j];
					} else {
						Brain.Layers[i].Neurons[j].Value = 0;

						for (int k = 0; k < Brain.Layers[i - 1].NeuronCount; ++k) {
							Brain.Layers[i].Neurons[j].Value += Brain.Layers[i - 1].Neurons[k].Value
							                                    * Brain.Layers[i].Neurons[j].Dendrites[k].Weight;
						}
						Brain.Layers[i].Neurons[j].Value = BipolarSigmoid(Brain.Layers[i].Neurons[j].Value);
					}
				}
			}

			double[] outResult = new double[Brain.Layers[Brain.LayerCount - 1].NeuronCount];

			for (int i = 0; i < Brain.Layers[Brain.LayerCount - 1].NeuronCount; ++i) {
				double value = Brain.Layers[Brain.LayerCount - 1].Neurons[i].Value;
				outResult[i] = value > 1 ? 1 : value < -1 ? -1 : value;
			}

			return outResult;
		}

		public static double GetRand () {
			return Rand.NextDouble() * 2 - 1;
		}

		private double BipolarSigmoid (double x) {
			return 1 / (1 + Math.Exp(-x));
		}

		public void Randomize () {
			for (int i = 0; i < Brain.LayerCount; ++i) {
				for (int j = 0; j < Brain.Layers[i].NeuronCount; ++j) {
					if (i == 0) {
						continue;
					}
					Brain.Layers[i].Neurons[j].Bias = GetRand();
					for (int k = 0; k < Brain.Layers[i - 1].NeuronCount; ++k) {
						Brain.Layers[i].Neurons[j].Dendrites[k].Weight = GetRand();
					}
				}
			}
		}

		public double[] GetWeights () {
			double[] weights = new double[DendriteCount];
			int dendriteCount = 0;

			for (int i = 0; i < Brain.LayerCount; ++i) {
				for (int j = 0; j < Brain.Layers[i].NeuronCount; ++j) {
					if (i == 0) {
						continue;
					}
					for (int k = 0; k < Brain.Layers[i - 1].NeuronCount; ++k) {
						weights[dendriteCount] = Brain.Layers[i].Neurons[j].Dendrites[k].Weight;
						++dendriteCount;
					}
				}
			}

			return weights;
		}

		public void SetWeights (double[] weights) {
			int dendriteCount = 0;

			for (int i = 0; i < Brain.LayerCount; ++i) {
				for (int j = 0; j < Brain.Layers[i].NeuronCount; ++j) {
					if (i == 0) {
						continue;
					}
					for (int k = 0; k < Brain.Layers[i - 1].NeuronCount; ++k) {
						Brain.Layers[i].Neurons[j].Dendrites[k].Weight = weights[dendriteCount];
						++dendriteCount;
					}
				}
			}
		}

		private int GetDendriteCount () {
			int dendriteCount = 0;

			for (int i = 0; i < Brain.LayerCount; ++i) {
				for (int j = 0; j < Brain.Layers[i].NeuronCount; ++j) {
					if (i == 0) {
						continue;
					}
					for (int k = 0; k < Brain.Layers[i - 1].NeuronCount; ++k) {
						++dendriteCount;
					}
				}
			}

			return dendriteCount;
		}

		public double[] GetBias () {
			int biasCount = 0;
			double[] bias = new double[GetBiasCount()];

			for (int i = 0; i < Brain.LayerCount; ++i) {
				for (int j = 0; j < Brain.Layers[i].NeuronCount; ++j) {
					if (i == 0) {
						continue;
					}
					bias[biasCount] = Brain.Layers[i].Neurons[j].Bias;
					++biasCount;
				}
			}

			return bias;
		}

		public void SetBias (double[] bias) {
			int biasCount = 0;

			for (int i = 0; i < Brain.LayerCount; ++i) {
				for (int j = 0; j < Brain.Layers[i].NeuronCount; ++j) {
					if (i == 0) {
						continue;
					}
					Brain.Layers[i].Neurons[j].Bias = bias[biasCount];
					++biasCount;
				}
			}
		}

		private int GetBiasCount () {
			int biasCount = 0;

			for (int i = 0; i < Brain.LayerCount; ++i) {
				for (int j = 0; j < Brain.Layers[i].NeuronCount; ++j) {
					if (i != 0) {
						++biasCount;
					}
				}
			}

			return biasCount;
		}

	}

}