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
//using Dijkstra.NET.Graph;
//using Dijkstra.NET.ShortestPath;

class Solution
{
	// Complete the shortestReach function below.
	static int[] shortestReach(int n, int[][] edges, int s)
	{
		var graph = new Graph<Node>();
		var convertionStrcuture = new Dictionary<int, Node>();

		for (int i = 1; i <= n; ++i)
		{
			convertionStrcuture.Add(i, new Node() { Value = i });
			graph.Add(convertionStrcuture[i]);
		}

		foreach (var edge in edges)
			graph.AddNotOrientedConnection(convertionStrcuture[edge[0]], convertionStrcuture[edge[1]], edge[2]);

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

	static void Main(string[] args)
	{
		//RunGraphTest();
		int a = 0;
		int b = 0;
		var test = new Dictionary<int, int> { { a, 0 }, { b, 0 } };
		Console.WriteLine(test.Count);
		//Console.WriteLine(CustomReadLine());
	}

	protected static void RunGraphTest()
	{
		string InputFilePath = Path.Combine(Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.FullName, "GraphTest1[Input]");
		string OutputFilePath = Path.Combine(Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.FullName, "GraphTest1[Output]");

		var _inputs = new List<int>();
		foreach (var line in File.ReadAllText(InputFilePath).Split('\n'))
			foreach (var @int in line.Split(' '))
				_inputs.Add(Convert.ToInt32(@int));

		var givenOutput = new List<List<int>>();
		int counter = 0;
		foreach (var line in File.ReadAllText(OutputFilePath).Split('\n'))
		{
			givenOutput.Add(new List<int>());
			foreach (var @int in line.Split(' '))
				givenOutput[counter].Add(Convert.ToInt32(@int));
			++counter;
		}

		int currentReadPosition = 0;
		int[][] calculatedOutput = new int[_inputs[0]][];
		for (int graphsCount = 0; graphsCount < _inputs[0]; ++graphsCount)
		{
			int n = _inputs[++currentReadPosition];
			int m = _inputs[++currentReadPosition];
			int[][] edges = new int[m][];
			for (int i = 0; i < m; ++i)
				edges[i] = new int[] { _inputs[++currentReadPosition], _inputs[++currentReadPosition], _inputs[++currentReadPosition] };
			int s = _inputs[++currentReadPosition];

			int[] result = shortestReach(n, edges, s);
			calculatedOutput[graphsCount] = result;
			Console.WriteLine(string.Join(" ", result));
		}

		for (int i = 0; i < calculatedOutput.Length; i++)
			for (int j = 0; j < calculatedOutput[i].Length; ++j)
				if (calculatedOutput[i][j] != givenOutput[i][j])
					Console.WriteLine(string.Format("[{0}] : [{1}]", i, j));

		Console.ReadKey();
	}

	static StringBuilder stringBuilder = new StringBuilder();
	public static string CustomReadLine()
	{
		stringBuilder.Clear();
		char input;
		do
		{
			input = Console.ReadKey(true).KeyChar;
			stringBuilder.Append(input, 1);
			Console.Write((int)input + ' ');
		} while (input != '\n');
		return stringBuilder.ToString();
	}
}