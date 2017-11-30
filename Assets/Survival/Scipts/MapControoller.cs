using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Survival {
	public class MapControoller : MonoBehaviour {

		public Map Map;
		public MapTemplate MapTemplate;
		public bool AutoGenerateNewMap = true;
		[Range(2,25)]
		public int PlayerViewRange = 3;
		HashSet<Map.Element> _visibleElements = new HashSet<Map.Element>();
		Dictionary<int, MapObject> _visibleObjects = new Dictionary<int, MapObject>();

		private void OnDrawGizmosSelected() {
			GizmosDrawFrame();
			GizmosDrawCells();
		}

		void GizmosDrawFrame() {
			var right = transform.right;
			var forward = transform.forward;
			var zero = transform.position;
			var xhl = right * Map.Config.SizeX * 0.5f;
			var zhl = forward * Map.Config.SizeZ * 0.5f;
			Gizmos.color = Color.white;
			Gizmos.DrawLine(zero - xhl + zhl, zero + xhl + zhl);
			Gizmos.DrawLine(zero + xhl - zhl, zero + xhl + zhl);
			Gizmos.DrawLine(zero - xhl - zhl, zero + xhl - zhl);
			Gizmos.DrawLine(zero - xhl - zhl, zero - xhl + zhl);
		}

		void GizmosDrawCells() {
			var right = transform.right;
			var forward = transform.forward;
			var zero = transform.position;
			var xhl = right * Map.Config.SizeX * 0.5f;
			var zhl = forward * Map.Config.SizeZ * 0.5f;
			var xSize = right * Map.Config.CellSize;
			var zSize = forward * Map.Config.CellSize;
			var xC = (int)(Map.Config.SizeX / Map.Config.CellSize);
			var zC = (int)(Map.Config.SizeZ / Map.Config.CellSize);

			Gizmos.color = Color.black;

			Gizmos.DrawLine(zero - zhl, zero + zhl);
			for (int x = 1; x < xC / 2; x++) {
				var xx = x * xSize;
				Gizmos.DrawLine(zero - zhl + xx, zero + zhl + xx);
				Gizmos.DrawLine(zero - zhl - xx, zero + zhl - xx);
			}

			Gizmos.DrawLine(zero - xhl, zero + xhl);
			for (int z = 1; z < zC / 2; z++) {
				var zz = z * zSize;
				Gizmos.DrawLine(zero - xhl + zz, zero + xhl + zz);
				Gizmos.DrawLine(zero - xhl - zz, zero + xhl - zz);
			}
		}

		public void GenerateNewMap() {
			MapTemplate.Generate(Map);
		}

		public void ClearMap() {
			Map.Clear();
		}

		public void SetPlayerPosition(Vector3 position) {
			var actualElements = Map.GetNearestElements(position, PlayerViewRange);
			var oldVisibleElements = _visibleElements;
			var newVisibleElements = new HashSet<Map.Element>();

			AddNewMapObjects(actualElements, oldVisibleElements, newVisibleElements);

			RemoveOldMapObjects(oldVisibleElements, newVisibleElements);

			_visibleElements = newVisibleElements;
		}

		private void RemoveOldMapObjects(HashSet<Map.Element> oldVisibleElements, 
			HashSet<Map.Element> newVisibleElements) {
			foreach (var oldElement in oldVisibleElements) {
				if (!newVisibleElements.Contains(oldElement)) {
					MapObject obj;
					if (_visibleObjects.TryGetValue(oldElement.ID, out obj)) {
						_visibleObjects.Remove(oldElement.ID);
						MapObjectPool.Instance.Return(obj);
					}
				}
			}
		}

		private void AddNewMapObjects(IEnumerable<Map.Element> actualElements, 
			HashSet<Map.Element> oldVisibleElements, HashSet<Map.Element> newVisibleElements) {
			
			foreach (var element in actualElements) {
				if (!oldVisibleElements.Contains(element)) {
					var newObject = MapObjectPool.Instance.GetObject(
						prefab: MapTemplate.GetPrefab(element.PrefabIndex),
						localPosition: element.Position,
						localRotation: Quaternion.Euler(0f, element.YAngle, 0f),
						parent: this.transform
					);
					newObject.ID = element.ID;
					newObject.Coordinate = element.CellCoordinate;
					_visibleObjects.Add(element.ID, newObject);
				}
				newVisibleElements.Add(element);
			}
		}

		void Start() {
			if (AutoGenerateNewMap) {
				ClearMap();
				GenerateNewMap();
			}
			SetPlayerPosition(Vector3.zero);
		}
	}

	#region UNITY_EDITOR
	#if UNITY_EDITOR
	[UnityEditor.CustomEditor(typeof(MapControoller))]
	public class MapControollerEditor : UnityEditor.Editor {
		public override void OnInspectorGUI() {
			DrawDefaultInspector();
			var script = (MapControoller)target;
			if (GUILayout.Button("Generate new Map")) {
				script.GenerateNewMap();
			}
			if (GUILayout.Button("Clear map")) {
				script.ClearMap();
			}
		}
	}
	#endif
	#endregion


}

