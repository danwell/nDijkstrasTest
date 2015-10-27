namespace nDijkstrasTest
{
	public class Tile
	{
		/// <summary>
		///		How many movement points are needed to move over this tile. If 0, this
		///		obstructs movement entirely
		/// </summary>
		public int MovementCost = 10;

		public bool IsObstruction
		{
			get { return MovementCost == 0;  }
		}

		public TileType TileType;
		public Entity OccupantEntity;
	}
}
