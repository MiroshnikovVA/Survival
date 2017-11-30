using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace Survival {

	[System.Serializable]
	public class Map {

		[System.Serializable]
		public class MapConfig {
			public float CellSize = 10f;
			public float SizeX = 500f;
			public float SizeZ = 500f;
			public Vector3 PlayerStartPosition = Vector3.zero;

			public Coordinate GetCoordinate(Vector3 position) {
				return new Coordinate {
					X = (System.Int16)(position.x / CellSize),
					Z = (System.Int16)(position.z / CellSize)
				};
			}
		}

		[System.Serializable]
		public class Element {
			public int PrefabIndex;
			public Vector3 Position;
			public float YAngle;
			public float Radius;
			public int ID;
			public Coordinate CellCoordinate;
		}

		[System.Serializable]
		public struct Coordinate {
			public System.Int16 X;
			public System.Int16 Z;
			public int HashCode { get { return CalcHashCode(X, Z); } }
			public override int GetHashCode() {	return HashCode; }
			public static int CalcHashCode(int x, int z) { return (x << 16) + z;}
		}

		[System.Serializable]
		public class Cell {
			public Coordinate Coordinate;
			public List<Element> Elements;
		}

		public MapConfig Config = new MapConfig();
		[UnityEngine.SerializeField]
		List<Cell> _cells = new List<Cell>();
		Dictionary<int, Cell> _cellsDic = null;
		[UnityEngine.SerializeField]
		private int _nextID = 0;

		void TryInitDic() {
			if (_cellsDic == null) {
				_cellsDic = new Dictionary<int, Cell>();
				foreach (var cell in _cells) {
					_cellsDic[cell.Coordinate.HashCode] = cell;
				}
			}
		}

		Cell GetCell(Coordinate coordinate) {
			TryInitDic();
			var hashCode = coordinate.HashCode;
			Cell cell;
			if (!_cellsDic.TryGetValue(hashCode, out cell)) {
				cell = new Cell() {
					Coordinate = coordinate,
					Elements = new List<Element>()
				};
				_cellsDic[hashCode] = cell;
				_cells.Add(cell);
			}
			return cell;
		}

		Cell GetCell(Vector3 position) {
			var coordinate = Config.GetCoordinate(position);
			return GetCell(coordinate);
		}

		IEnumerable<Element> GetNearestElements(Coordinate coordinate, int size) {
			TryInitDic();
			for (int x=coordinate.X - size; x<=coordinate.X + size; x++) {
				for (int z = coordinate.Z - size; z <=coordinate.Z + size; z++) {
					Cell cell;
					if (_cellsDic.TryGetValue(Coordinate.CalcHashCode(x, z), out cell)) {
						foreach (var element in cell.Elements) yield return element;
					}
				}
			}
		}

		public IEnumerable<Element> GetNearestElements(Vector3 position, int size) {
			return GetNearestElements(Config.GetCoordinate(position), size);
		}

		public IEnumerable<Element> GetNearestElements(Element element, int size) {
			return GetNearestElements(Config.GetCoordinate(element.Position), size);
		}

		public bool TryAddElement(Element element) {
			if ((element.Position - Config.PlayerStartPosition).sqrMagnitude < 5f*5f) return false;
			using (var nearestElements = GetNearestElements(element, 1).GetEnumerator()) {
				while (nearestElements.MoveNext()) {
					var otherElement = nearestElements.Current;
					var minDist = otherElement.Radius + element.Radius;
					var sqrMinDist = minDist * minDist;
					if ((element.Position - otherElement.Position).sqrMagnitude < sqrMinDist) {
						return false;
					}
				}
			}
			var cell = GetCell(element.Position);
			cell.Elements.Add(element);
			element.ID = _nextID++;
			element.CellCoordinate = cell.Coordinate;
			return true;
		}

		public void Clear() {
			_nextID = 0;
			_cells.Clear();
			if (_cellsDic!=null) _cellsDic.Clear();
		}
	}
}
