using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace Survival {

	[RequireComponent(typeof(CharacterController))]
	[RequireComponent(typeof(Animator))]
	public class SurvivalCharacterController : NetworkBehaviour {

		public Vector3 Velocity;
		public float Speed = 10f;
		private Animator _animator;
		private CharacterController _character;
		
		void Awake() {
			_animator = GetComponent<Animator>();
			_character = GetComponent<CharacterController>();
		}

		private void Start() {
			if (isLocalPlayer) {
				PlayerController.Inctance.Character = this;
			} else {
				enabled = false;
			}
		}

		[Command]
		public void CmdAtack() {
			RpcAtack();
		}

		[ClientRpc]
		public void RpcAtack() {
			_animator.SetTrigger("Lumbering");
		}


		public void Attack() {
			CmdAtack();
		}

		void Update() {
			var v = Velocity.magnitude;
			_animator.SetFloat("Velocity", v);
			var moving = v > 0.001f;
			_animator.SetBool("Moving", moving);
			_character.Move((Velocity + (_character.isGrounded?Vector3.zero:Vector3.down)) * Time.deltaTime * Speed);
			if (moving) {
				transform.rotation = Quaternion.Lerp(
					transform.rotation,
					Quaternion.LookRotation(Velocity),
					5f * Time.deltaTime);
			}
		}
	}
}
