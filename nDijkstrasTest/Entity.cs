namespace nDijkstrasTest
{
	public class Entity : Transformable
	{
		/// <summary>
		///		Detemines if entity is mean. Meanies obstruct movement of non-meanies and vice versa.
		/// </summary>
		public bool IsMean;

		/// <summary>
		///		Number of tiles an entity can move. This is represented as *10 to save on floating
		///		point values
		/// </summary>
		private int _range = 70;

		public int RangeRaw
		{
			get { return _range; }
		}

		/// <summary>Determines whether this entity can move on land tiles</summary>
		public bool CanMoveOnLand;
		/// <summary>Determines whether this entity can move on water tiles</summary>
		public bool CanMoveOnWater;
		// FIXME: This supercedes the above two
		public bool CanMoveInAir;

		/// <summary>
		///		Number of tiles an entity can move
		/// </summary>
		public int Range
		{
			get { return _range / 10; }
			set { _range = value * 10; }
		}
	}
}
