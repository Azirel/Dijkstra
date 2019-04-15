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
using Dijkstra.NET.Graph;
using Dijkstra.NET.ShortestPath;

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
		{
			graph.AddNotOrientedConnection(convertionStrcuture[edge[0]], convertionStrcuture[edge[1]], edge[2]);
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
		protected Dictionary<T, Dictionary<T, int>> Net = new Dictionary<T, Dictionary<T, int>>();
		protected SortedSet<T> Queue; //prioritized queue
		protected HashSet<T> DoneNodes = new HashSet<T>();

		public void Add(T node)
		{
			Net.Add(node, new Dictionary<T, int>());
		}

		public void AddNotOrientedConnection(T from, T to, int weight)
		{
			if (Net[from].ContainsKey(to) || Net[to].ContainsKey(from))
			{
				Net[from][to] = weight < Net[from][to] ? weight : Net[from][to];
				Net[to][from] = weight < Net[to][from] ? weight : Net[to][from];
			}
			else
			{
				Net[from].Add(to, weight);
				Net[to].Add(from, weight);
			}
		}

		public void AddOrientedConnection(T from, T to, int weight)
		{
			if (Net[from].ContainsKey(to))
				Net[from][to] = weight < Net[from][to] ? weight : Net[from][to];
			else Net[from].Add(to, weight);
		}

		public Dictionary<T, int> ShortestLenghts(T from)
		{
			var result = new Dictionary<T, int>(Net.Count);
			Queue = new SortedSet<T>(new Comparer<T>(ref result));

			foreach (var node in Net)
				result[node.Key] = int.MaxValue;

			result[from] = 0;

			var currentNode = from;
			int currentLenght;
			do
			{
				foreach (var connection in Net[currentNode])
				{
					currentLenght = result[currentNode] + connection.Value;
					if (currentLenght < result[connection.Key])
					{
						Queue.Remove(connection.Key);
						result[connection.Key] = currentLenght;
						if (!DoneNodes.Contains(connection.Key))
							Enqueue(ref Queue, connection.Key);
					}
				}
				DoneNodes.Add(currentNode);
				currentNode = Queue.Count == 0 ? null : Dequeue(ref Queue);
			} while (currentNode != null);

			return result;
		}

		protected class Comparer<T> : IComparer<T>
		{
			protected Dictionary<T, int> Distances;
			protected int distanceDifference;
			public Comparer(ref Dictionary<T, int> distances)
			{
				Distances = distances;
			}
			int IComparer<T>.Compare(T x, T y)
			{
				distanceDifference = Distances[x] - Distances[y];
				if (x.GetHashCode() == y.GetHashCode())
					return 0;
				else if (distanceDifference == 0)
					return -1;
				return distanceDifference;
			}
		}

		protected void Enqueue<T>(ref SortedSet<T> source, T element)
		{
			source.Add(element);
		}

		protected T Dequeue<T>(ref SortedSet<T> source)
		{
			T result = source.First();
			bool removeResult = source.Remove(result);
			if (removeResult == false)
				Console.WriteLine("We are so fucked up");
			return result;
		}
	}

	static void Main(string[] args)
	{
		RunGraphTest();

		//var sortedSet = new SortedSet<int>();
		//sortedSet.Add(5);
		//sortedSet.Add(-5);
		//sortedSet.Add(0);
		//foreach (var item in sortedSet)
		//	Console.Write(item + " ");
		//Console.ReadKey();
	}



	protected static void RunGraphTest()
	{
		//string FilePath = "F:\\Temp\\CustomGraph1";
		//string FilePath = "F:\\Temp\\CustomGraph";
		string FilePath = Path.Combine(Directory.GetCurrentDirectory(), @"GraphCase6");

		Console.WriteLine(Path.GetFullPath(FilePath));

		var _inputs = new List<int>();
		foreach (var line in File.ReadAllText(FilePath).Split('\n'))
			foreach (var @int in line.Split(' '))
				_inputs.Add(Convert.ToInt32(@int));

		int currentReadPosition = 0;
		for (int graphsCount = 0; graphsCount < _inputs[0]; ++graphsCount)
		{
			int n = _inputs[++currentReadPosition];
			int m = _inputs[++currentReadPosition];
			int[][] edges = new int[m][];
			for (int i = 0; i < m; ++i)
				edges[i] = new int[] { _inputs[++currentReadPosition], _inputs[++currentReadPosition], _inputs[++currentReadPosition] };
			int s = _inputs[++currentReadPosition];

			int[] result = shortestReach(n, edges, s);
			Console.WriteLine(string.Join(" ", result));
		}

		Console.ReadKey();
	}
}