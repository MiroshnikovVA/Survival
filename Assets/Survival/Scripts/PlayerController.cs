using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Survival {
	public class PlayerController : MonoBehaviour {

		public SimpleTouchController TouchController;
		public SurvivalCharacterController Character;
		public Transform CameraTransform;
		public MapControoller MapControoller { get; set; }
		public Button AttackButton;
		private Vector3 _cameraDeltaPos;
		private Map.Coordinate _characterPosition;

		static PlayerController _inctance;

		public static PlayerController Inctance {
			get {
				if (!_inctance) {
					_inctance = FindObjectOfType<PlayerController>();
				}
				return _inctance;
			}

		}

		void OnValidate() {
			if (!TouchController) Debug.LogError("TouchController is null", this);
			//if (!Character) Debug.LogError("Character is null", this);
			if (!CameraTransform) Debug.LogError("CameraTransform is null", this);
			//if (!MapControoller) Debug.LogError("MapControoller is null", this);
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

		void Update() {
			if (Character && MapControoller) {
				var pos = Character.transform.position;
				CameraTransform.position = new Vector3(_cameraDeltaPos.x + pos.x, _cameraDeltaPos.y, _cameraDeltaPos.z + pos.z);
				var newPos = MapControoller.MapConfig.GetCoordinate(Character.transform.position);
				var dx = _characterPosition.X - newPos.X;
				var dz = _characterPosition.Z - newPos.Z;
				if (Mathf.Abs(dx) >= 2 || Mathf.Abs(dz) >= 2) {
					MapControoller.SetPlayerPosition(Character.transform.position);
					_characterPosition = newPos;
				}
			}
		}
	}
}
