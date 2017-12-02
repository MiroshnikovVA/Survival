using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace Survival {

	public class MapControoller : NetworkBehaviour {


		public Map.MapConfig MapConfig;

		[SyncVar]
		public int RandomMapIndex = -1;

		public MapTemplate MapTemplate;
		[Range(2,25)]
		public int PlayerViewRange = 3;
		HashSet<Map.Element> _visibleElements = new HashSet<Map.Element>();
		Dictionary<int, MapObject> _visibleObjects = new Dictionary<int, MapObject>();
		Map _map = new Map();
		private Coroutine _mapChangeCoroutine;

		private void OnDrawGizmosSelected() {
			GizmosDrawFrame();
			GizmosDrawCells();
		}

		void GizmosDrawFrame() {
			var right = transform.right;
			var forward = transform.forward;
			var zero = transform.position;
			var xhl = right * _map.Config.SizeX * 0.5f;
			var zhl = forward * _map.Config.SizeZ * 0.5f;
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
			var xhl = right * _map.Config.SizeX * 0.5f;
			var zhl = forward * _map.Config.SizeZ * 0.5f;
			var xSize = right * _map.Config.CellSize;
			var zSize = forward * _map.Config.CellSize;
			var xC = (int)(_map.Config.SizeX / _map.Config.CellSize);
			var zC = (int)(_map.Config.SizeZ / _map.Config.CellSize);

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
			_map.Config = MapConfig;
			Random.InitState(RandomMapIndex);
			MapTemplate.Generate(_map);
		}

		public void ClearMap() {
			_map.Clear();
			//foreach (var obj in _visibleObjects) {
			//	MapObjectPool.Instance.Return(obj.Value);
			//}
			//_visibleObjects.Clear();
		}

		public void SetPlayerPosition(Vector3 position) {
			var actualElements = _map.GetNearestElements(position, PlayerViewRange);
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

		private void OnEnable() {
			StartCoroutine(MapChange());
		}

		public void Start() {
			if (isServer) {
				RandomMapIndex = Random.Range(0, int.MaxValue);
			};
		}

		IEnumerator MapChange() {
			var oldRandomMapIndex = RandomMapIndex;
			yield return new WaitWhile(() => oldRandomMapIndex != RandomMapIndex);
			ClearMap();
			GenerateNewMap();
			PlayerController.Inctance.MapControoller = this;
			SetPlayerPosition(PlayerController.Inctance.Character.transform.position);
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

