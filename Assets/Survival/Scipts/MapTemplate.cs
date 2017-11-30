using UnityEngine;

namespace Survival {

	public abstract class MapTemplate : ScriptableObject {
		public abstract MapObject GetPrefab(int prefabIndex);
		public abstract void Generate(Map map);
	}
}
