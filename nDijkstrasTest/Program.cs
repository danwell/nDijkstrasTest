using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Permissions;
using System.Text;

namespace nDijkstrasTest
{
	static class Program
	{
		private const char ObstructionChar = '#';
		private const char EntityChar = 'S';
		private const char MeanieChar = 'R';
		private const char EmptyChar = ' ';
		private const char NavigableChar = '~';
		private const char RoughChar = ':';
		private const char HillChar = '@';
		private const char RoadChar = '=';
		private const char WaterChar = '~'; 
			
		private static volatile bool _sigTerm = false;

		private static int Main(string[] argv)
		{
			Console.CancelKeyPress += (sender, args) =>
			{
				_sigTerm = true;
			};

			if (argv.Length == 0)
			{
				Console.Error.WriteLine("Expecting one parameter for map file");
				return 1;
			}

			string strMap;
			int strMapWidth;
			int strMapHeight;
			{
				var mapStringBuilder = new StringBuilder();
				var mapReader = File.ReadLines(argv[0]).GetEnumerator();

				if (!mapReader.MoveNext())
				{
					Console.Error.WriteLine("Map is empty?");
					return 1;
				}

				strMapWidth = mapReader.Current.Length;
				strMapHeight = 1;
				mapStringBuilder.Append(mapReader.Current);
				while (mapReader.MoveNext())
				{
					if (mapReader.Current.Length == 0)
						break;

					if (mapReader.Current.Length != strMapWidth)
					{
						Console.Error.WriteLine("Inconsistent map width");
						return 1;
					}

					mapStringBuilder.Append(mapReader.Current);
					strMapHeight++;
				}
				strMap = mapStringBuilder.ToString();
			}

			var map = new Map(strMapWidth, strMapHeight);
			var entities = new List<Entity>();

			var tileIndex = 0;
			for (var y = 0; y < strMapHeight; y++)
			{
				for (var x = 0; x < strMapWidth; x++)
				{
					var tile = strMap[tileIndex];
					switch (tile)
					{
						case WaterChar:
							map[x, y].MovementCost = 255;
							map[x, y].TileType = TileType.Water;
							break;
						case RoadChar:
							map[x, y].MovementCost = 9;
							map[x, y].TileType = TileType.Road;
							break;
						case RoughChar:
							map[x, y].MovementCost = 15;
							map[x, y].TileType = TileType.Forest;
							break;
						case HillChar:
							map[x, y].MovementCost = 30;
							map[x, y].TileType = TileType.Hills;
							break;
						case ObstructionChar:
							map[x, y].MovementCost = 0;
							map[x, y].TileType = TileType.Mountain;
							break;
						case EntityChar:
							var entityS = new Entity()
							{
								X = x,
								Y = y,
								IsMean = false
							};
							map[x, y].OccupantEntity = entityS;
							entities.Add(entityS);
							break;
						case MeanieChar:
							var entityR = new Entity()
							{
								X = x,
								Y = y,
								IsMean = true
							};
							map[x, y].OccupantEntity = entityR;
							entities.Add(entityR);
							break;
					}

					tileIndex += 1;
				}
			}

			// Basic method:
			// Move up, right, down then left. Continue for as long as movement points remain and path
			//	is not obstructed by obstacle or opponents
			// If you collide with a tile that has equal or more movement points, stop
			// if you collide with a tile that has less movement points, overwrite it
			
			foreach(var theEntity in entities.Where(e => !e.IsMean))
			{
				if(_sigTerm) break;

				var visits = new int[strMapWidth, strMapHeight];
				for (var y = 0; y < strMapHeight; y++)
				{
					for (var x = 0; x < strMapWidth; x++)
					{
						visits[x, y] = -1;
					}
				}

				// KEY:
				//		-1: Never visited
				//		-2: Obstructed
				//		0: Exhausted
				//		> 0: Number of remaining moves

				var toVisit = new Queue<Transformable>(theEntity.Range*2*2);

				// Add starting visit nodes
				if (theEntity.Range > 0)
				{
					visits[theEntity.X, theEntity.Y] = theEntity.RangeRaw;
					toVisit.Enqueue(new Transformable()
					{
						X = theEntity.X,
						Y = theEntity.Y
					});
				}

				while (toVisit.Count > 0)
				{
					var visitLocation = toVisit.Dequeue();
					var visitScore = visits[visitLocation.X, visitLocation.Y];

					// Visit above, to the right, below then left
					if ((visitLocation.Y - 1) >= 0)
					{
						ProcessTile(visits, visitLocation.X, visitLocation.Y - 1, visitScore, toVisit, map, theEntity);
					}
					// Visit right
					if ((visitLocation.X + 1) < strMapWidth)
					{
						ProcessTile(visits, visitLocation.X + 1, visitLocation.Y, visitScore, toVisit, map, theEntity);
					}
					// Visit below
					if ((visitLocation.Y + 1) < strMapHeight)
					{
						ProcessTile(visits, visitLocation.X, visitLocation.Y + 1, visitScore, toVisit, map, theEntity);
					}
					// Visit left
					if ((visitLocation.X - 1) >= 0)
					{
						ProcessTile(visits, visitLocation.X - 1, visitLocation.Y, visitScore, toVisit, map, theEntity);
					}
				}

				//var outputMap = new char[strMapWidth, strMapHeight];
				for (var y = 0; y < strMapHeight; y++)
				{
					for (var x = 0; x < strMapWidth; x++)
					{
						var canVisit = visits[x, y] != -1;
						if (canVisit)
						{
							Console.Out.Write("\u001b[46m");
						}

						var tile = map[x, y];
						if (tile.IsObstruction)
						{
							Console.Out.Write("\u001b[41m" + ObstructionChar + "\u001b[0m");
						}
						else if (tile.MovementCost == 255)
						{
							if(canVisit)
								Console.Out.Write(WaterChar);
							else
								Console.Out.Write("\u001b[44m" + WaterChar + "\u001b[0m");
						}
						else if (tile.MovementCost == 9)
						{
							if(canVisit)
								Console.Out.Write(RoadChar);
							else
								Console.Out.Write("\u001b[47m" + RoadChar + "\u001b[0m");
						}
						else if (tile.MovementCost == 15)
						{
							if(canVisit)
								Console.Out.Write(RoughChar);
							else
								Console.Out.Write("\u001b[42m" + RoughChar + "\u001b[0m");
	
						}
						else if (tile.MovementCost == 30)
						{
							if(canVisit)
								Console.Out.Write(HillChar);
							else
								Console.Out.Write("\u001b[43m" + HillChar + "\u001b[0m");
						}
						else if (tile.OccupantEntity != null)
						{
							if (tile.OccupantEntity == theEntity)
								Console.Out.Write('H');
							else 
								Console.Out.Write(tile.OccupantEntity.IsMean ? MeanieChar : EntityChar);
						}
						else
						{
							Console.Out.Write(EmptyChar);
						}

						if (canVisit)
						{
							Console.Out.Write("\u001b[0m");
						}

						//Console.Out.Write(visits[x, y] != -1 && tf % 2 == 0 ? '~' : ' ');
					}
					Console.Out.WriteLine();
				}

				//Thread.Sleep(3000);
				//Console.CursorTop -= strMapHeight;
				Console.Out.WriteLine();
			}

			return 0;
		}

		private static void ProcessTile(int[,] visits, int targetX, int targetY, int visitScore, Queue<Transformable> toVisit, Map map,
			Entity theEntity)
		{
			var visitTile = visits[targetX, targetY];
			var targetTile = map[targetX, targetY];
			// Already visited?
			if (visitTile != -1)
			{
				// Assuming not obstructed
				// Do we have a higher movement score?
				if (visitScore <= visitTile) return;

				// Overwrite it and flag it for revisit
				visits[targetX, targetY] = visitScore - targetTile.MovementCost;
				toVisit.Enqueue(new Transformable() {X = targetX, Y = targetY});
			}
			else
			{
				if (targetTile.IsObstruction) return;

				// No obstructions - any other entity on this tile and does it allow movement?
				if (targetTile.OccupantEntity != null)
				{
					if (targetTile.OccupantEntity.IsMean != theEntity.IsMean)
					{
						// Wrong place for visit set?
						visits[targetX, targetY] = -1;
						return;
					}
				}

				// visits[targetX, targetY] = visitScore - targetTile.MovementCost;
				// toVisit.Enqueue(new Transformable() {X = targetX, Y = targetY});
				if(map[targetX, targetY].MovementCost <= visitScore) {
					visits[targetX, targetY] = visitScore - map[targetX, targetY].MovementCost;
					toVisit.Enqueue(new Transformable() {X = targetX, Y = targetY});
				}
			}
		}

		private static T[,] Generate2DArrayWithDefaultValue<T>(int w, int h, T val)
		{
			var array = new T[w, h];
			for (var y = 0; y < h; y++)
			{
				for (var x = 0; x < w; x++)
				{
					array[x, y] = val;
				}
			}
			return array;
		}


		private static void InitArray<T>(ref T[,] array, int w, int h, T val)
		{
			for (var y = 0; y < h; y++)
			{
				for (var x = 0; x < w; x++)
				{
					array[x, y] = val;
				}
			}
		}

		private static void InitArray<T>(ref T[,] array, int w, int h, Func<int, int, T> setterFunc)
		{
			for (var y = 0; y < h; y++)
			{
				for (var x = 0; x < w; x++)
				{
					array[x, y] = setterFunc(x, y);
				}
			}
		} 
	}
}
