using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Survival {
	public class WoodlandsMapTemplate : MapTemplate {
		public MapObject[] Trees;
		public MapObject[] MeadowObjects;
		public MapObject[] TownObjects;
		public MapObject[] Stowns;

		public int ForestCount = 30;
		public int TownCount = 2;
		public int StownsCount = 7;
		public float TownMaxRadius = 50f;
		public float TownMinRadius = 20f;
		public float ForestMinRadius = 10f;
		public float ForestMaxRadius = 50f;
		public float StownsMinRadius = 2f;
		public float StownsMaxRadius = 5f;
		public float TreesPerSquareMeter = 0.1f;
		public float MeadowObjectInSquareMeter = 0.01f;
		public float TownObjectsPerSquareMeter = 4.0f;
		public float StownsPerSquareMeter = 2f;

		#if UNITY_EDITOR
		[UnityEditor.MenuItem("Assets/Create/Survival/WoodlandsMapTemplate")]
		public static void CreateAsset() {
			WoodlandsMapTemplate asset = ScriptableObject.CreateInstance<WoodlandsMapTemplate>();
			UnityEditor.AssetDatabase.CreateAsset(asset, "Assets/WoodlandsMapTemplate.asset");
			UnityEditor.AssetDatabase.SaveAssets();
			UnityEditor.EditorUtility.FocusProjectWindow();
			UnityEditor.Selection.activeObject = asset;
		}
		#endif

		public override void Generate(Map map) {
			var hxSize = map.Config.SizeX * 0.5f;
			var hzSize = map.Config.SizeZ * 0.5f;

			CircleGenerated(map, hxSize, hzSize,
				minRadius: ForestMinRadius,
				maxRadius: ForestMaxRadius,
				indexOffset: 0,
				objPerSquareMeter: TreesPerSquareMeter,
				prefabs : Trees,
				count: ForestCount);

			CircleGenerated(map, hxSize, hzSize,
				minRadius: StownsMinRadius,
				maxRadius: StownsMaxRadius,
				indexOffset: Trees.Length + MeadowObjects.Length + TownObjects.Length,
				objPerSquareMeter: StownsPerSquareMeter,
				prefabs: Stowns,
				count: StownsCount);

			CircleGenerated(map, hxSize, hzSize,
				minRadius: TownMinRadius,
				maxRadius: TownMaxRadius,
				indexOffset: Trees.Length + MeadowObjects.Length,
				objPerSquareMeter: TownObjectsPerSquareMeter,
				prefabs: TownObjects,
				count: TownCount);
						
			var meadowObjectCount = map.Config.SizeX * map.Config.SizeZ * MeadowObjectInSquareMeter;
			for (int iMeadowObject = 0; iMeadowObject < meadowObjectCount; iMeadowObject++) {
				var x = Random.Range(-hxSize, hxSize);
				var z = Random.Range(-hzSize, hzSize);
				var pos = new Vector3(x, 0, z);
				var prefabIndex = Random.Range(0, MeadowObjects.Length);
				var element = new Map.Element() {
					Position = pos,
					PrefabIndex = Trees.Length + prefabIndex,
					Radius = MeadowObjects[prefabIndex].Radius,
					YAngle = Random.Range(0f, 360f)
				};
				if (map.TryAddElement(element)) {
					//Debug.Log($"Добавлен куст {MeadowObjects[prefabIndex].name}");
				}
			}
		}

		private void CircleGenerated(Map map, float hxSize, float hzSize, 
			float minRadius, float maxRadius, int indexOffset, float objPerSquareMeter,
			MapObject[] prefabs, int count) {
			for (int iCircle = 0; iCircle < count; iCircle++) {
				var forestRadius = Random.Range(minRadius, maxRadius);
				var centerX = Random.Range(-hxSize, hxSize);
				var centerZ = Random.Range(-hzSize, hzSize);
				var objCount = Mathf.PI * forestRadius * forestRadius * objPerSquareMeter;
				for (int iObj = 0; iObj < objCount; iObj++) {
					var angle = Random.Range(0, 2f * Mathf.PI);
					var r = Random.Range(0, 2f * forestRadius);
					var x = centerX + Mathf.Cos(angle) * r;
					var z = centerZ + Mathf.Sin(angle) * r;
					x = Mathf.Min(hxSize, Mathf.Max(-hxSize, x));
					z = Mathf.Min(hzSize, Mathf.Max(-hzSize, z));
					var pos = new Vector3(x, 0, z);
					var prefabIndex = Random.Range(0, prefabs.Length);
					var element = new Map.Element() {
						Position = pos,
						PrefabIndex = prefabIndex + indexOffset,
						Radius = prefabs[prefabIndex].Radius,
						YAngle = Random.Range(0f, 360f)
					};
					map.TryAddElement(element);
				}
			}
		}

		public override MapObject GetPrefab(int prefabIndex) {
			if (prefabIndex < Trees.Length) {
				return Trees[prefabIndex];
			} else if (prefabIndex < Trees.Length + MeadowObjects.Length) {
				return MeadowObjects[prefabIndex - Trees.Length];
			} else if (prefabIndex < Trees.Length + MeadowObjects.Length + TownObjects.Length) {
				return TownObjects[prefabIndex - Trees.Length - MeadowObjects.Length];
			}
			else {
				return Stowns[prefabIndex - Trees.Length - MeadowObjects.Length - TownObjects.Length];
			}
		}
	}
}
