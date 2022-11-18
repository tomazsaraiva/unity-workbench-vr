using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class Screw : XRGrabInteractable
{
    [Header("Configuration")]
    [SerializeField] private float _fastenDistanceZ = 0.05f;

    private XRSocketInteractor _socket;

    public float _fastenAmount;
    private Vector3 _unfastenedPosition;
    private Vector3 _fastenedPosition;

    public override void ProcessInteractable(XRInteractionUpdateOrder.UpdatePhase updatePhase)
    {
        base.ProcessInteractable(updatePhase);
    }

    protected override void OnSelectEntered(SelectEnterEventArgs args)
    {
        base.OnSelectEntered(args);

        if(args.interactorObject.transform.TryGetComponent(out XRSocketInteractor socket))
        {
            _socket = socket;

            var pose = _socket.GetAttachPoseOnSelect(this);
            _unfastenedPosition = pose.position;
            _fastenedPosition = _unfastenedPosition + Vector3.forward * _fastenDistanceZ;

            transform.SetPositionAndRotation(pose.position, pose.rotation);
        }

        trackPosition = _socket == null;
        trackRotation = _socket == null;
    }
    protected override void OnSelectExited(SelectExitEventArgs args)
    {
        base.OnSelectExited(args);

        if (args.interactorObject.transform.TryGetComponent(out XRSocketInteractor socket))
        {
            _fastenAmount = 0;
            _socket = null;

            trackPosition = true;
            trackRotation = true;
        }
    }

    public void Fasten(bool fasten, float rotateSpeed, float fastenDuration)
    {
        if(_socket == null) { return; }

        var fastenSpeed = 1f / fastenDuration;
        _fastenAmount += fastenSpeed * (fasten ? 1 : -1) * Time.deltaTime;
        _fastenAmount = Mathf.Clamp01(_fastenAmount);

        if (_fastenAmount == 0 && !fasten)
        {
            transform.position = _unfastenedPosition;
            return;
        }

        if (_fastenAmount == 1 && fasten)
        {
            transform.position = _fastenedPosition;
            return;
        }

        transform.position = Vector3.Lerp(_unfastenedPosition, _fastenedPosition, _fastenAmount);

        var speed = rotateSpeed * (fasten ? -1 : 1);
        transform.Rotate(Vector3.forward, Time.deltaTime * speed, Space.Self);
    }
}
