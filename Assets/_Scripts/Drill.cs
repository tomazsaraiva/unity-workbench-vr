using System;
using System.Net.Sockets;

using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;

public class Drill : XRGrabInteractable
{
    [Header("Children")]
    [SerializeField] private Transform _bit;
    [SerializeField] private Transform _button;
    [SerializeField] private Transform _selector;
    [SerializeField] public InputActionReference _selectorActionRight;
    [SerializeField] public InputActionReference _selectorActionLeft;

    [Header("Configuration")]
    [SerializeField] private float _bitSpeed;
    [SerializeField] private float _fastenDuration;
    [SerializeField] private float _buttonMaxOffsetZ = 0.005f;

    private AudioSource _audioSource;
    private XRBaseController _controller;

    private Screw _screw;

    private bool _isActivated;
    private bool _isFastening = true;

    private Vector3 _buttonOffPosition;
    private Vector3 _buttonOnPosition;

    protected override void Awake()
    {
        base.Awake();

        _audioSource = GetComponent<AudioSource>();

        _buttonOffPosition = _button.transform.localPosition;
        _buttonOnPosition = _buttonOffPosition + Vector3.forward * _buttonMaxOffsetZ;
    }

    protected override void OnSelectEntered(SelectEnterEventArgs args)
    {
        base.OnSelectEntered(args);

        if (args.interactorObject.transform.TryGetComponent(out XRSocketInteractor socket)) { return; }

        _controller = ((XRBaseControllerInteractor)args.interactorObject).xrController;

        // HACK change this
        if(_controller.name.StartsWith("Left"))
        {
            _selectorActionLeft.action.performed += SelectorAction_Performed;
        }
        else
        {
            _selectorActionRight.action.performed += SelectorAction_Performed;
        }
    }
    protected override void OnSelectExited(SelectExitEventArgs args)
    {
        base.OnSelectExited(args);

        if (args.interactorObject.transform.TryGetComponent(out XRSocketInteractor socket)) { return; }

        // HACK change this
        if (_controller.name.StartsWith("Left"))
        {
            _selectorActionLeft.action.performed -= SelectorAction_Performed;
        }
        else
        {
            _selectorActionRight.action.performed -= SelectorAction_Performed;
        }

        _controller = null;
    }

    protected override void OnActivated(ActivateEventArgs args)
    {
        base.OnActivated(args);

        _button.transform.localPosition = _buttonOnPosition;

        _isActivated = true;
        _audioSource.Play();
    }
    protected override void OnDeactivated(DeactivateEventArgs args)
    {
        base.OnDeactivated(args);

        _button.transform.localPosition = _buttonOffPosition;

        _isActivated = false;
        _audioSource.Stop();
    }

    public override void ProcessInteractable(XRInteractionUpdateOrder.UpdatePhase updatePhase)
    {
        base.ProcessInteractable(updatePhase);

        if(!_isActivated) { return; }

        _bit.Rotate(Vector3.up, Time.deltaTime * _bitSpeed * (_isFastening ? 1 : -1), Space.Self);

        _controller.SendHapticImpulse(1, Time.deltaTime);

        if (_screw != null)
        {
            _screw.Fasten(_isFastening, _bitSpeed, _fastenDuration);
        }
    }

    private void SelectorAction_Performed(InputAction.CallbackContext obj)
    {
        _isFastening = !_isFastening;

        var selectorAngles = _selector.transform.localRotation.eulerAngles;
        selectorAngles.y *= -1;
        _selector.transform.localRotation= Quaternion.Euler(selectorAngles);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.transform.TryGetComponent(out Screw screw))
        {
            _screw = screw;
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.transform.TryGetComponent(out Screw screw))
        {
            _screw = null;
        }
    }
}