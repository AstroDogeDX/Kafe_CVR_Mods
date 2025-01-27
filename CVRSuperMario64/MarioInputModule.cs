﻿using ABI_RC.Core;
using ABI_RC.Core.InteractionSystem;
using ABI_RC.Core.Player;
using ABI_RC.Core.Savior;
using UnityEngine;

using Valve.VR;

namespace Kafe.CVRSuperMario64;

public class MarioInputModule : CVRInputModule {
    internal static MarioInputModule Instance;

    public int controllingMarios;
    public bool canMoveOverride = false;

    public float vertical;
    public float horizontal;
    public bool jump;
    public bool kick;
    public bool stomp;

    public float cameraRotation;
    public float cameraPitch;

    public new void Start() {
        _inputManager = CVRInputManager.Instance;
        Instance = this;
        base.Start();

        CVRSM64Context.UpdateMarioCount();
    }

    private bool CanMove() {
        return controllingMarios == 0 || canMoveOverride;
    }

    public override void UpdateInput() {
        if (controllingMarios > 0 && Input.GetKeyDown(KeyCode.LeftShift)) {
            canMoveOverride = true;
        }

        if (Input.GetKeyUp(KeyCode.LeftShift)) {
            canMoveOverride = false;
        }

        // Normalize the movement vector, this is done after the modules run, but I'm saving the values before so...
        var movementVector = _inputManager.movementVector;
        if (movementVector.magnitude > movementVector.normalized.magnitude) {
            movementVector.Normalize();
        }

        // Normalize the look vector, this is done after the modules run, but I'm saving the values before so...
        var lookVector = _inputManager.lookVector;
        if (lookVector.magnitude > lookVector.normalized.magnitude) {
            lookVector.Normalize();
        }

        // Save current input
        horizontal = movementVector.x;
        vertical = movementVector.z;
        jump = _inputManager.jump;
        kick = _inputManager.interactRightValue > 0.25f;
        stomp = _inputManager.gripRightValue > 0.25f;

        cameraRotation = lookVector.x;
        cameraPitch = lookVector.y;

        // Prevent moving if we're controlling marios
        if (!CanMove()) {

            // Prevent the player from moving, doing the NotAKidoS way so he doesn't open Issues ;_;
            _inputManager.movementVector = Vector3.zero;
            _inputManager.jump = false;
            _inputManager.interactRightValue = 0f;
            _inputManager.gripRightValue = 0f;

            // Attempt to do a free look control, let's prevent our control from being able to rotate
            if (MarioCameraMod.IsControllingAMario(out var mario) && MarioCameraMod.IsFreeCamEnabled()) {
                _inputManager.lookVector = Vector2.zero;
                // Return because if we're doing the free control we don't want to be able to move around
                return;
            }

            // Thanks NotAKidS for finding the issue and suggesting the fix!
            if (!ViewManager.Instance.isGameMenuOpen() && !CVR_MenuManager.Instance._quickMenuOpen) {

                // Lets attempt to do a left hand only movement (let's ignore vive wants because it messes the jump)
                if (MetaPort.Instance.isUsingVr && !PlayerSetup.Instance._trackerManager.TrackedObjectsContains("vive_controller")) {
                    _inputManager.movementVector.z = CVRTools.AxisDeadZone(
                        InputModuleSteamVR.Instance.vrLookAction.GetAxis(SteamVR_Input_Sources.Any).y,
                        MetaPort.Instance.settings.GetSettingInt("ControlDeadZoneRight") / 100f);
                }
            }
        }
    }

    public override void UpdateImportantInput() {
        // Prevent Mario from moving while we're using the menu
        horizontal = 0;
        vertical = 0;
        jump = false;
        kick = false;
        stomp = false;

        cameraRotation = 0f;
        cameraPitch = 0f;
    }
}
