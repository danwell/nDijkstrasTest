namespace nDijkstrasTest
{
	public class Map
	{
		private readonly Tile[,] _tileMap;
		public readonly int Width;
		public readonly int Height;

		public Map(int width, int height)
		{
			_tileMap = new Tile[width, height];
			for (var y = 0; y < height; y++)
			{
				for (var x = 0; x < width; x++)
				{
					_tileMap[x, y] = new Tile();
				}
			}
			Height = height;
			Width = width;
		}

		public Tile this[int x, int y]
		{
			get { return _tileMap[x, y]; }
			private set { _tileMap[x, y] = value; } 
		}

		public bool PointIsWithinMap(int x, int y)
		{
			return XPointIsWithinMap(x) && YPointIsWithinMap(y);
		}

		public bool XPointIsWithinMap(int x)
		{
			return x >= 0 && x < Height;
		}

		public bool YPointIsWithinMap(int y)
		{
			return y >= 0 && y < Width;
		}
	}
}
