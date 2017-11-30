using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Survival {
	public abstract class ObjectPool<T, This> : MonoBehaviour 
		where T : MonoBehaviour 
		where This: ObjectPool<T, This> {

		static This _instance;

		public static This Instance {
			get {
				if (!_instance) {
					GameObject obj = new GameObject();
					obj.name = "["+typeof(This).ToString()+"]";
					_instance = obj.AddComponent<This>();
				}
				return _instance;
			}
		}

		public Dictionary<T, Stack<T>> _dic = new Dictionary<T, Stack<T>>();
		public Dictionary<T, T> _dicObjToPrefab = new Dictionary<T, T>();

		public T GetObject(T prefab, Transform parent, Vector3 localPosition, Quaternion localRotation) {
			Stack<T> stack;
			if (_dic.TryGetValue(prefab, out stack)) {
				if (stack.Count > 0) {
					var rez = stack.Pop();
					rez.transform.SetParent(parent);
					rez.transform.localPosition = localPosition;
					rez.transform.localRotation = localRotation;
					rez.gameObject.SetActive(true);
					return rez;
				}
			}
			var obj = Instantiate(prefab, parent);
			_dicObjToPrefab[obj] = prefab;
			obj.transform.localPosition = localPosition;
			obj.transform.localRotation = localRotation;
			return obj;
		}

		public void Return(T obj) {
			var prefab = _dicObjToPrefab[obj];
			Stack<T> stack;
			if (!_dic.TryGetValue(prefab, out stack)) {
				stack = new Stack<T>();
				_dic[prefab] = stack;
			}
			stack.Push(obj);
			obj.gameObject.SetActive(false);
			obj.transform.SetParent(transform);
		}

	}
}
