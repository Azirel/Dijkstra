//https://www.hackerrank.com/challenges/dijkstrashortreach/problem

using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Collections;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;
using System.Text;
using System;

class HackerRankSolution
{
	// Complete the shortestReach function below.
	static int[] shortestReach(int n, int[,] edges, int s)
	{
		var graph = new Graph();

		for (int i = 1; i <= n; ++i)
			graph.Add(i);

		for (int i = 0; i < edges.GetLength(0); ++i)
			graph.AddNotOrientedConnection(edges[i, 0], edges[i, 1], edges[i, 2]);

		var shortestWays = graph.ShortestLenghts(s);
		var result = new List<int>(shortestWays.Select((element) => element.Value == int.MaxValue ? -1 : element.Value));
		result.RemoveAt(s - 1);
		return result.ToArray();
	}

	protected class Graph
	{
		protected Dictionary<int, Dictionary<int, int>> net = new Dictionary<int, Dictionary<int, int>>();
		protected PrioritizedQueue queue; //prioritized queue
		protected HashSet<int> doneNodes = new HashSet<int>();

		public void Add(int vertex)
		{
			net.Add(vertex, new Dictionary<int, int>());
		}

		public void AddNotOrientedConnection(int from, int to, int weight)
		{
			if (net[from].ContainsKey(to) || net[to].ContainsKey(from))
			{
				net[from][to] = weight < net[from][to] ? weight : net[from][to];
				net[to][from] = weight < net[to][from] ? weight : net[to][from];
			}
			else
			{
				net[from].Add(to, weight);
				net[to].Add(from, weight);
			}
		}

		public Dictionary<int, int> ShortestLenghts(int from)
		{
			var distances = new Dictionary<int, int>(net.Count);
			queue = new PrioritizedQueue(net.Count, ref distances);
			foreach (var node in net)
				distances[node.Key] = int.MaxValue;
			distances[from] = 0;
			int currentNode = from;
			int currentLenght;
			do
			{
				foreach (var connection in net[currentNode])
				{
					currentLenght = distances[currentNode] + connection.Value;
					if (currentLenght < distances[connection.Key])
						distances[connection.Key] = currentLenght;
					if (!doneNodes.Contains(connection.Key))
					{
						queue.Enqueue(connection.Key);
						queue.UpdateValue(connection.Key);
					}
				}
				doneNodes.Add(currentNode);
				if (queue.Count < 1)
					break;
				currentNode = queue.Dequeue();
			} while (true);
			return distances;
		}
	}

	protected class PrioritizedQueue
	{
		protected List<int> mainArray;
		protected Dictionary<int, int> distances;
		protected Dictionary<int, int> elementsIndexes = new Dictionary<int, int>();
		protected int temp;

		public int Count { get { return mainArray.Count; } }

		public PrioritizedQueue(int maxCapacity, ref Dictionary<int, int> distances)
		{
			mainArray = new List<int>(maxCapacity);
			this.distances = distances;
		}

		protected void SiftDown(int currentIndex)
		{
			int left = 2 * currentIndex + 1;
			int right = 2 * currentIndex + 2;
			int minElementIndex = currentIndex;
			if (left < mainArray.Count && distances[mainArray[left]] > distances[mainArray[minElementIndex]])
				minElementIndex = left;
			if (right < mainArray.Count && distances[mainArray[right]] > distances[mainArray[minElementIndex]])
				minElementIndex = right;
			if (minElementIndex != currentIndex)
			{
				Swap(currentIndex, minElementIndex);
				SiftDown(minElementIndex);
			}
		}

		protected void SiftUp(int currentIndex)
		{
			while (currentIndex > -1)
			{
				int parentIndex = (currentIndex - 1) / 2;
				if (distances[mainArray[currentIndex]] >= distances[mainArray[parentIndex]])
					return;
				Swap(currentIndex, parentIndex);
				currentIndex = parentIndex;
			}
		}

		public void UpdateValue(int element)
		{
			if (elementsIndexes.ContainsKey(element))
			{
				int elementIndex = elementsIndexes[element];
				if (distances[mainArray[elementIndex]] < distances[mainArray[(elementIndex - 1) / 2]])
					SiftUp(elementIndex);
				else
					SiftDown(elementIndex);
			}
		}

		public void Enqueue(int element)
		{
			if (!elementsIndexes.ContainsKey(element))
			{
				mainArray.Add(element);
				elementsIndexes.Add(element, mainArray.Count - 1);
				SiftUp(mainArray.Count - 1);
			}
		}

		public int Dequeue()
		{
			int result = mainArray[0];
			Swap(0, mainArray.Count - 1);
			mainArray.RemoveAt(mainArray.Count - 1);
			elementsIndexes.Remove(result);
			SiftDown(0);
			return result;
		}

		protected void Swap(int aIndex, int bIndex)
		{
			elementsIndexes[mainArray[aIndex]] = bIndex;
			elementsIndexes[mainArray[bIndex]] = aIndex;
			temp = mainArray[aIndex];
			mainArray[aIndex] = mainArray[bIndex];
			mainArray[bIndex] = temp;
		}
	}

	static void _Main(string[] args)
	{
		TextWriter textWriter = new StreamWriter(@System.Environment.GetEnvironmentVariable("OUTPUT_PATH"), true);

		int t = Convert.ToInt32(Console.ReadLine());
		var fastVertexParse = new Dictionary<string, int>();
		for (int tItr = 0; tItr < t; tItr++)
		{
			string[] nm = Console.ReadLine().Split(' ');

			int n = ConvertToInt(nm[0]);

			int m = ConvertToInt(nm[1]);

			int[,] edges = new int[m, 3];

			for (int i = fastVertexParse.Count + 1; i <= n; ++i)
				fastVertexParse[i.ToString()] = i;

			string[] connection;
			for (int i = 0; i < m; i++)
			{
				connection = Console.ReadLine().Split(' ');
				edges[i, 0] = fastVertexParse[connection[0]];
				edges[i, 1] = fastVertexParse[connection[1]];
				edges[i, 2] = fastVertexParse.ContainsKey(connection[2]) ? fastVertexParse[connection[2]] : ConvertToInt(connection[2]);
			}

			int s = ConvertToInt(Console.ReadLine());

			int[] result = shortestReach(n, edges, s);

			textWriter.WriteLine(string.Join(" ", result));
		}

		textWriter.Flush();
		textWriter.Close();
	}

	public static int ConvertToInt(string s)
	{
		int y = 0;
		foreach (char c in s)
			y = y * 10 + (c - '0');
		return y;
	}
}
