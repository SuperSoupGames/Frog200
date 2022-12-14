using UnityEngine;
using UnityEngine.InputSystem;
using Fusion;
using Fusion.KCC;

namespace TPSBR
{
	public class AirplaneAgent : ContextBehaviour
	{
		// PUBLIC MEMBERS

		[Networked, HideInInspector]
		public Airplane Airplane { get; set; }

		// PRIVATE MEMBERS

		[SerializeField]
		private Transform _cameraTransform;

		[Networked]
		private Quaternion _lookRotation { get; set; }
		[Networked(nameof(AirplaneAgent))]
		private GameplayInput _lastKnownInput { get; set; }

		private bool _jump;
		private Vector2 _lookRotationDelta;
		private bool _isParented;
		private FrameRecord[] _frameRecords = new FrameRecord[32];

		// NetworkBehaviour INTERFACE

		public override void Spawned()
		{
			_frameRecords.Clear();

			if (Object.HasStateAuthority == true)
			{
				Object.SetInterestGroup(Object.InputAuthority, nameof(AirplaneAgent), true);
			}

			NetworkEvents networkEvents = Runner.GetComponent<NetworkEvents>();
			networkEvents.OnInput.RemoveListener(OnInput);
			networkEvents.OnInput.AddListener(OnInput);

			_lookRotation = transform.localRotation;

			if (Object.HasInputAuthority == true)
			{
				Context.WaitingAgentTransform = transform;
			}

			_isParented = false;
		}

		public override void FixedUpdateNetwork()
		{
			if (_isParented == false && Airplane != null)
			{
				transform.SetParent(Airplane.AgentPosition, false);
				_isParented = true;
			}

			if (Object.IsProxy == true)
				return;

			if (Runner.TryGetInputForPlayer(Object.InputAuthority, out GameplayInput input) == true)
			{
				_lookRotation = UpdateRotation(_lookRotation, input.LookRotationDelta);

				if (EGameplayInputAction.Jump.WasActivated(input, _lastKnownInput) == true)
				{
					if ((Context.GameplayMode as BattleRoyaleGameplayMode).RequestAirplaneJump(Object.InputAuthority, transform.rotation * Vector3.forward) == true)
					{
                        Context.Camera.Music.Play();
                        return;
					}
				}

				_lastKnownInput = input;
			}

			if (Object.HasStateAuthority == true)
			{
				Vector3 lookDirection = _lookRotation * Vector3.forward;

				Runner.AddPlayerAreaOfInterest(Object.InputAuthority, transform.position + lookDirection *  25.0f,  50.0f);
				Runner.AddPlayerAreaOfInterest(Object.InputAuthority, transform.position + lookDirection * 100.0f,  75.0f);
				Runner.AddPlayerAreaOfInterest(Object.InputAuthority, transform.position + lookDirection * 175.0f, 100.0f);
			}
		}

		public override void Despawned(NetworkRunner runner, bool hasState)
		{
			if (runner != null)
			{
				NetworkEvents networkEvents = runner.GetComponent<NetworkEvents>();
				networkEvents.OnInput.RemoveListener(OnInput);
			}

			if (Object.HasInputAuthority == true && Context.WaitingAgentTransform == transform)
			{
				Context.WaitingAgentTransform = null;
			}
		}

		public override void Render()
		{
			if (Context.ObservedPlayerRef != Object.InputAuthority)
				return;

			UpdateRotation(_lookRotation, _lookRotationDelta);

			Context.Camera.transform.position = _cameraTransform.position;
			Context.Camera.transform.rotation = _cameraTransform.rotation;
		}

		// MONOBEHAVIOUR

		protected void Update()
		{
			if (Object.HasInputAuthority == false || Context.HasInput == false)
				return;

			if (Context.Input.IsCursorVisible == true || Context.GameplayMode.State != GameplayMode.EState.Active)
				return;

			Vector2 mouseDelta = Mouse.current.delta.ReadValue() * 0.075f;

			_jump |= Keyboard.current.spaceKey.wasPressedThisFrame;
			_lookRotationDelta += InputUtility.ProcessLookRotationDelta(_frameRecords, new Vector2(-mouseDelta.y, mouseDelta.x), Global.RuntimeSettings.Sensitivity, 0.025f);
		}

		// PRIVATE METHODS

		private void OnInput(NetworkRunner runner, Fusion.NetworkInput networkInput)
		{
			if (Object.HasInputAuthority == false || Context.HasInput == false)
				return;

			GameplayInput gameplayInput = new GameplayInput();

			gameplayInput.Jump = _jump;
			gameplayInput.LookRotationDelta = _lookRotationDelta;

			networkInput.Set(gameplayInput);

			_jump = default;
			_lookRotationDelta = default;
		}

		private Quaternion UpdateRotation(Quaternion rotation, Vector2 rotationDelta)
		{
			if (rotationDelta == Vector2.zero)
			{
				transform.localRotation = rotation;
				return rotation;
			}

			Vector3 rotationEuler = KCCUtility.GetEulerLookRotation(rotation);

			float lookRotationDeltaX = Mathf.Clamp(rotationEuler.x + rotationDelta.x, -85f, 85f) - rotationEuler.x;
			float lookRotationDeltaY = rotationDelta.y;

			var newRotation = Quaternion.Euler(rotationEuler + new Vector3(lookRotationDeltaX, lookRotationDeltaY, 0f));

			transform.localRotation = newRotation;
			return newRotation;
		}
	}
}
