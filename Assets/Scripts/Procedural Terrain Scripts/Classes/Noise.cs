using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Noise 
{

	int seed;

	public int Seed {
		get {
			return seed;
		}
		set {
			seed = value;
		}
	}

	float frequency;

	public float Frequency {
		get {
			return frequency;
		}
		set {
			frequency = value;
		}
	}

	float amplitude;

	public float Amplitude {
		get {
			return amplitude;
		}
		set {
			amplitude = value;
		}
	}

	float lacunarity;

	public float Lacunarity {
		get {
			return lacunarity;
		}
		set {
			lacunarity = value;
		}
	}

	float persistance;

	public float Persistance {
		get {
			return persistance;
		}
		set {
			persistance = value;
		}
	}

	int octaves;

	public int Octaves {
		get {
			return octaves;
		}
		set {
			octaves = value;
		}
	}

	public Noise(int seed, float frequency, float amplitude, float lacunarity, float persistance, int octaves)
	{
		this.seed = seed;
		this.frequency = frequency;
		this.amplitude = amplitude;
		this.lacunarity = lacunarity;
		this.octaves = octaves;
	}

	public float[,] GetNoiseValues (int width, int height)
	{
		float[,] noiseValues = new float[width, height];

		float max = 0f;
		float min = float.MaxValue;

		for (int i = 0; i < width; i++)
		{
			for (int j = 0; j < height; j++)
			{
				noiseValues [i, j] = 0f;

				float tempA = amplitude;
				float tempF = frequency;

				for (int k = 0; k < octaves; k++)
				{
					noiseValues [i, j] += Mathf.PerlinNoise ((i  + seed) / (float)width * frequency, j / (float)height * frequency) * amplitude;
					frequency *= lacunarity;
					amplitude *= persistance;
				}
				amplitude = tempA;
				frequency = tempF;

				if (noiseValues [i, j] > max)
					max = noiseValues [i, j];
				if (noiseValues [i, j] < min)
					min = noiseValues [i, j];
			}
		}

		for(int i = 0; i <width; i++)
		{
			for(int j = 0; j <height; j++)
			{

				noiseValues[i,j] = Mathf.InverseLerp(max, min, noiseValues[i,j]);
			}
		}
		return noiseValues;
	}
}
