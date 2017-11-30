using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Survival {
	public class MapObject : MonoBehaviour {

		public float Radius = 0.5f;
		public Map.Coordinate Coordinate; // <- это отладочная информация
		public int ID { get; set; }
		
		private void OnDrawGizmosSelected() {
			Gizmos.color = Color.green;
			Gizmos.DrawWireSphere(transform.position, Radius);
		}
	}
}
