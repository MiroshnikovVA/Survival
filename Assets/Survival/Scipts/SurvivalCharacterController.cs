using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Survival {

	[RequireComponent(typeof(CharacterController))]
	[RequireComponent(typeof(Animator))]
	public class SurvivalCharacterController : MonoBehaviour {

		public Vector3 Velocity;
		public float Speed = 10f;
		private Animator _animator;
		private CharacterController _character;
		
		void Awake() {
			_animator = GetComponent<Animator>();
			_character = GetComponent<CharacterController>();
		}

		public void Attack() {
			_animator.SetTrigger("Lumbering");
		}

		void Update() {
			var v = Velocity.magnitude;
			_animator.SetFloat("Velocity", v);
			var moving = v > 0.001f;
			_animator.SetBool("Moving", moving);
			_character.Move((Velocity + Vector3.down) * Time.deltaTime * Speed);
			if (moving) {
				transform.rotation = Quaternion.Lerp(
					transform.rotation,
					Quaternion.LookRotation(Velocity),
					5f * Time.deltaTime);
			}
		}
	}
}
