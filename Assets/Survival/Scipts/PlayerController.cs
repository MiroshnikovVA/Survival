using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Survival {
	public class PlayerController : MonoBehaviour {

		public SimpleTouchController TouchController;
		public SurvivalCharacterController Character;
		public Transform CameraTransform;
		public MapControoller MapControoller;
		public Button AttackButton;
		private Vector3 _cameraDeltaPos;
		private Map.Coordinate _characterPosition;

		void OnValidate() {
			if (!TouchController) Debug.LogError("TouchController is null", this);
			if (!Character) Debug.LogError("Character is null", this);
			if (!CameraTransform) Debug.LogError("CameraTransform is null", this);
			if (!MapControoller) Debug.LogError("MapControoller is null", this);
			if (!AttackButton) Debug.LogError("AttackButton is null", this);
		}

		void Awake() {
			TouchController.TouchEvent += TouchController_TouchEvent;
			TouchController.TouchStateEvent += TouchController_TouchStateEvent;
			_cameraDeltaPos = CameraTransform.position - Character.transform.position;
			AttackButton.onClick.AddListener(() => {
				Character.Attack();
			});
		}

		private void TouchController_TouchStateEvent(bool touchPresent) {
			if (!touchPresent) {
				Character.Velocity = Vector3.zero;
			}
		}

		private void TouchController_TouchEvent(Vector2 value) {
			var velocity = CameraTransform.forward * value.y + CameraTransform.right * value.x;
			Character.Velocity = velocity;
		}

		// Update is called once per frame
		void Update() {
			CameraTransform.position = _cameraDeltaPos + Character.transform.position;
			var newPos = MapControoller.Map.Config.GetCoordinate(Character.transform.position);
			var dx = _characterPosition.X - newPos.X;
			var dz = _characterPosition.Z - newPos.Z;
			if (Mathf.Abs(dx)>=2 || Mathf.Abs(dz)>=2) {
				MapControoller.SetPlayerPosition(Character.transform.position);
				_characterPosition = newPos;
			}
		}
	}
}
