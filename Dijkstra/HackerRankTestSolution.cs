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
		var graph = new Graph<Node>();
		var convertionStrcuture = new Dictionary<int, Node>();

		for (int i = 1; i <= n; ++i)
		{
			convertionStrcuture.Add(i, new Node() { Value = i });
			graph.Add(convertionStrcuture[i]);
		}

		for (int i = 0; i < edges.GetLength(0); ++i)
		{
			graph.AddNotOrientedConnection(convertionStrcuture[edges[i, 0]], convertionStrcuture[edges[i, 1]], edges[i, 2]);
		}

		var shortestWays = graph.ShortestLenghts(convertionStrcuture[s]);
		var result = new List<int>();
		for (int i = 1; i <= n; ++i)
		{
			if (i == s) continue;
			result.Add(shortestWays[convertionStrcuture[i]] == int.MaxValue ? -1 : shortestWays[convertionStrcuture[i]]);
		}
		return result.ToArray();
	}

	protected class Node
	{
		public int Value;
	}

	protected class Graph<T> where T : class
	{
		protected Dictionary<T, Dictionary<T, int>> net = new Dictionary<T, Dictionary<T, int>>();
		protected PrioritizedQueue<T> queue; //prioritized queue
		protected HashSet<T> doneNodes = new HashSet<T>();

		public void Add(T node)
		{
			net.Add(node, new Dictionary<T, int>());
		}

		public void AddNotOrientedConnection(T from, T to, int weight)
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

		public void AddOrientedConnection(T from, T to, int weight)
		{
			if (net[from].ContainsKey(to))
				net[from][to] = weight < net[from][to] ? weight : net[from][to];
			else net[from].Add(to, weight);
		}

		public Dictionary<T, int> ShortestLenghts(T from)
		{
			var distances = new Dictionary<T, int>(net.Count);
			queue = new PrioritizedQueue<T>() { Prioritizer = (a, b) => distances[b] - distances[a] };
			foreach (var node in net)
				distances[node.Key] = int.MaxValue;
			distances[from] = 0;
			var currentNode = from;
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
				currentNode = queue.ExtractTop();
			} while (currentNode != null);
			return distances;
		}
	}

	protected class PrioritizedQueue<T> where T : class
	{
		public Func<T, T, int> Prioritizer;
		public int Count { get { return mainArray.Count; } }

		protected List<T> mainArray = new List<T>();
		protected Dictionary<T, int> Positions = new Dictionary<T, int>();
		protected T temp;

		public bool Contains(T element)
		{
			return Positions.ContainsKey(element);
		}

		protected void SiftDown(int currentIndex)
		{
			int left = 2 * currentIndex + 1;
			int right = 2 * currentIndex + 2;
			int largestElementIndex = currentIndex;
			if (left <= mainArray.Count - 1 && Prioritizer(mainArray[left], mainArray[largestElementIndex]) > 0)
				largestElementIndex = left;
			if (right <= mainArray.Count - 1 && Prioritizer(mainArray[right], mainArray[largestElementIndex]) > 0)
				largestElementIndex = right;
			if (largestElementIndex != currentIndex)
			{
				Swap(currentIndex, largestElementIndex);
				SiftDown(largestElementIndex);
			}
		}

		protected void SiftUp(int currentIndex)
		{
			while (currentIndex > -1)
			{
				int parent = (currentIndex - 1) / 2;
				if (Prioritizer(mainArray[currentIndex], mainArray[parent]) <= 0)
					return;
				Swap(currentIndex, parent);
				currentIndex = parent;
			}
		}

		public void UpdateValue(T element)
		{
			if (Positions.ContainsKey(element))
			{
				int elementIndex = Positions[element];
				if (Prioritizer(mainArray[elementIndex], mainArray[(elementIndex - 1) / 2]) > 0)
					SiftUp(elementIndex);
				else
					SiftDown(elementIndex);
			}
		}

		protected void BuildHeap()
		{
			for (int i = (mainArray.Count - 1) / 2; i >= 0; --i)
				SiftDown(i);
		}

		public void Enqueue(T element)
		{
			if (!Contains(element))
			{
				mainArray.Add(element);
				Positions.Add(element, mainArray.Count - 1);
				SiftUp(mainArray.Count - 1);
			}
		}

		public T ExtractTop()
		{
			if (mainArray.Count < 1)
				return null;
			T result = mainArray[0];
			Swap(0, mainArray.Count - 1);
			mainArray.RemoveAt(mainArray.Count - 1);
			Positions.Remove(result);
			SiftDown(0);
			return result;
		}

		protected void Swap(int a, int b)
		{
			Positions[mainArray[a]] = b;
			Positions[mainArray[b]] = a;
			temp = mainArray[a];
			mainArray[a] = mainArray[b];
			mainArray[b] = temp;
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

			int n = Convert.ToInt32(nm[0]);

			int m = Convert.ToInt32(nm[1]);

			int[,] edges = new int[m, 3];

			for (int i = fastVertexParse.Count + 1; i <= n; ++i)
				fastVertexParse[i.ToString()] = i;

			string[] connection;
			for (int i = 0; i < m; i++)
			{
				connection = Console.ReadLine().Split(' ');
				edges[i, 0] = fastVertexParse[connection[0]];
				edges[i, 1] = fastVertexParse[connection[1]];
				edges[i, 2] = ConvertToInt(connection[2]);
			}

			int s = Convert.ToInt32(Console.ReadLine());

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
