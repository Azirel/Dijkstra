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
		protected Dictionary<T, Dictionary<T, int>> Net = new Dictionary<T, Dictionary<T, int>>();
		protected Heap<T> Queue; //prioritized queue
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
			var distances = new Dictionary<T, int>(Net.Count);
			Queue = new Heap<T>(Net.Count);
			Queue.Comparer = (a, b) => distances[b] - distances[a];
			var comparer = new Comparer<T>(ref distances);

			foreach (var node in Net)
				distances[node.Key] = int.MaxValue;

			distances[from] = 0;

			var currentNode = from;
			int currentLenght;
			do
			{
				foreach (var connection in Net[currentNode])
				{
					currentLenght = distances[currentNode] + connection.Value;
					if (currentLenght < distances[connection.Key])
						distances[connection.Key] = currentLenght;
					if (!DoneNodes.Contains(connection.Key) && !Queue.Contains(connection.Key))
						Queue.Add(connection.Key);
				}
				DoneNodes.Add(currentNode);
				if (Queue.Count == 0)
					break;
				currentNode = Min(Queue, (element) => distances[element]);
				Queue.Remove(currentNode);
			} while (currentNode != null);

			return distances;
		}

		public T Min<T>(IEnumerable<T> sequence, Func<T, int> comperor)
		{
			float currentMinValue = float.MaxValue;
			T currentMinItem = sequence.First();
			float comperissonValue;
			foreach (var item in sequence)
			{
				comperissonValue = comperor(item);
				if (comperissonValue < currentMinValue)
				{
					currentMinItem = item;
					currentMinValue = comperissonValue;
				}
			}
			return currentMinItem;
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
				return Distances[x] - Distances[y];
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

	public class Heap<T> where T : class
	{
		public Func<T, T, int> Comparer;
		public int Count { get { return lastElementIndex + 1; } }

		protected T[] mainArray;
		protected T temp;
		protected int lastElementIndex = -1;

		public Heap(int capacity)
		{
			mainArray = new T[capacity];
		}

		public bool Contains(T element)
		{
			return mainArray.Contains(element);
		}

		protected void SiftDown(int currentIndex)
		{
			int left = 2 * currentIndex + 1;
			int right = 2 * currentIndex + 2;
			int largestElementIndex = currentIndex;
			if (left <= lastElementIndex && Comparer(mainArray[left], mainArray[largestElementIndex]) > 0)
				largestElementIndex = left;
			if (right <= lastElementIndex && Comparer(mainArray[right], mainArray[largestElementIndex]) > 0)
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
				if (Comparer(mainArray[currentIndex], mainArray[parent]) <= 0)
					return;
				Swap(currentIndex, parent);
				currentIndex = parent;
			}
		}

		protected void BuildHeap()
		{
			for (int i = lastElementIndex / 2; i >= 0; --i)
				SiftDown(i);
		}

		public void Add(T element)
		{
			mainArray[++lastElementIndex] = element;
			SiftUp(lastElementIndex);
		}

		public T ExtractTop()
		{
			if (lastElementIndex < 0)
				return null;
			T result = (T)mainArray[0];
			Swap(0, lastElementIndex);
			mainArray[lastElementIndex] = null;
			--lastElementIndex;
			SiftDown(0);
			return result;
		}

		protected void Swap(int a, int b)
		{
			temp = mainArray[a];
			mainArray[a] = mainArray[b];
			mainArray[b] = temp;
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
}