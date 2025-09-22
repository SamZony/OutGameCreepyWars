using UnityEngine;

public class OutVehicleController : MonoBehaviour
{
    [Header("Vehicle Configuration")]
    [Tooltip("Select the type of vehicle to automatically set default parameters. These can be overridden in the Inspector.")]
    public VehicleType vehicleType = VehicleType.Car;

    public enum VehicleType { Car, Truck, SUV, Bus }

    [Header("Wheel Objects (Visual Meshes)")]
    [Tooltip("Transform of the front-left visual wheel mesh.")]
    public Transform wheelFL;
    [Tooltip("Transform of the front-right visual wheel mesh.")]
    public Transform wheelFR;
    [Tooltip("Transform of the rear-left visual wheel mesh.")]
    public Transform wheelRL;
    [Tooltip("Transform of the rear-right visual wheel mesh.")]
    public Transform wheelRR;

    [Header("Wheel Colliders (Physics)")]
    [Tooltip("Transform of the GameObject with the front-left WheelCollider component.")]
    public Transform wheelColFL;
    [Tooltip("Transform of the GameObject with the front-right WheelCollider component.")]
    public Transform wheelColFR;
    [Tooltip("Transform of the GameObject with the rear-left WheelCollider component.")]
    public Transform wheelColRL;
    [Tooltip("Transform of the GameObject with the rear-right WheelCollider component.")]
    public Transform wheelColRR;

    [Header("Movement Parameters")]
    [Tooltip("Maximum steering angle for the front wheels in degrees.")]
    public float maxSteerAngle = 30f;
    [Tooltip("Maximum motor torque applied to driven wheels.")]
    public float maxMotorTorque = 2000f;
    [Tooltip("Maximum brake torque applied when decelerating.")]
    public float maxBrakeTorque = 3000f;
    [Tooltip("Downforce applied to keep the vehicle grounded at higher speeds.")]
    public float downforce = 100f;

    [Header("Gear Parameters")]
    [Tooltip("Array of gear ratios for forward gears (e.g., gear 1, gear 2, etc.).")]
    public float[] gearRatios = new float[] { 2.66f, 1.78f, 1.3f, 1.0f, 0.74f, 0.5f };
    [Tooltip("Gear ratio for reverse.")]
    public float reverseGearRatio = -2.9f;
    [Tooltip("Final drive ratio multiplier.")]
    public float finalDriveRatio = 3.42f;
    [Tooltip("Engine RPM threshold to shift up a gear.")]
    public float upShiftRPM = 5500f;
    [Tooltip("Engine RPM threshold to shift down a gear.")]
    public float downShiftRPM = 3000f;
    [Tooltip("Maximum engine RPM for sound and shifting calculations.")]
    public float maxEngineRPM = 6000f;

    [Header("Sound Parameters")]
    [Tooltip("AudioSource for the engine sound (pitch will vary with RPM).")]
    public AudioSource engineSound;
    [Tooltip("Audio clip for gear shifting sound.")]
    public AudioClip gearShiftClip;
    [Tooltip("Audio clip for collision impact sound.")]
    public AudioClip collisionClip;
    [Tooltip("Audio clip for vehicle starting sound.")]
    public AudioClip startClip;
    [Tooltip("Audio clip for braking/deceleration sound.")]
    public AudioClip brakeClip;
    [Tooltip("Multiplier for engine sound pitch based on RPM.")]
    public float pitchMultiplier = 1f;
    [Tooltip("Minimum relative velocity for collision sound to play.")]
    public float collisionSoundThreshold = 5f;

    [Header("Positions")]
    public Transform enterPosition;
    public Transform drivingPosition;

    [Header("Stabilization")]
    public float uprightTorque = 200f;   // how strong it fights roll
    public float uprightDamping = 5f;    // smooths oscillation



    // Private fields
    private WheelCollider wcFL, wcFR, wcRL, wcRR;
    private Rigidbody rb;
    private Vector2 moveInput;
    private InputSystem_Actions inputActions; // Assumes this script exists and has a public InputAction called Move (Vector2)
    private int currentGear = 1; // 1+ for forward, -1 for reverse, 0 for neutral (but auto, so minimal use)
    private float engineRPM;
    private bool isBraking;
    private AudioSource brakeSoundSource; // For looping brake sound if needed

    [SerializeField] private float steerSmoothSpeed = 5f;     // Higher = snappier steering
    [SerializeField] private float accelSmoothSpeed = 4f;     // Higher = faster throttle response
    [SerializeField] private float brakeSmoothSpeed = 6f;     // Higher = faster brake response

    private float currentSteer;  // Smoothed steer value
    private float currentAccel;  // Smoothed acceleration value
    private float currentBrake;  // Smoothed brake value



#if UNITY_EDITOR
    private void OnValidate()
    {
        // Auto-tune smoothing based on vehicle type
        switch (vehicleType)
        {
            case VehicleType.Car:
                steerSmoothSpeed = 5f;
                accelSmoothSpeed = 3.5f;
                brakeSmoothSpeed = 5f;
                break;

            case VehicleType.Truck:
                steerSmoothSpeed = 3f;
                accelSmoothSpeed = 2f;
                brakeSmoothSpeed = 3.5f;
                break;

            case VehicleType.SUV:
                steerSmoothSpeed = 4f;
                accelSmoothSpeed = 3f;
                brakeSmoothSpeed = 4.5f;
                break;

            case VehicleType.Bus:
                steerSmoothSpeed = 2.5f;
                accelSmoothSpeed = 1.8f;
                brakeSmoothSpeed = 3f;
                break;
        }
    }
#endif

    void Awake()
    {
        // Get WheelCollider components from transforms
        wcFL = wheelColFL.GetComponent<WheelCollider>();
        wcFR = wheelColFR.GetComponent<WheelCollider>();
        wcRL = wheelColRL.GetComponent<WheelCollider>();
        wcRR = wheelColRR.GetComponent<WheelCollider>();

        rb = GetComponent<Rigidbody>();

        // Find the input actions script (assume it's in the scene)
        inputActions = new InputSystem_Actions();

        // Set default parameters based on vehicle type (can be overridden in Inspector)
        switch (vehicleType)
        {
            case VehicleType.Car:
                maxSteerAngle = 30f;
                maxMotorTorque = 2000f;
                maxBrakeTorque = 3000f;
                downforce = 100f;
                gearRatios = new float[] { 2.66f, 1.78f, 1.3f, 1.0f, 0.74f, 0.5f };
                break;
            case VehicleType.Truck:
                maxSteerAngle = 25f;
                maxMotorTorque = 4000f;
                maxBrakeTorque = 5000f;
                downforce = 50f;
                gearRatios = new float[] { 3.5f, 2.5f, 1.5f, 1.0f };
                break;
            case VehicleType.SUV:
                maxSteerAngle = 35f;
                maxMotorTorque = 2500f;
                maxBrakeTorque = 3500f;
                downforce = 150f;
                gearRatios = new float[] { 2.8f, 1.9f, 1.4f, 1.0f, 0.8f };
                break;
            case VehicleType.Bus:
                maxSteerAngle = 20f;
                maxMotorTorque = 3000f;
                maxBrakeTorque = 4000f;
                downforce = 80f;
                gearRatios = new float[] { 4.0f, 2.8f, 1.8f, 1.2f, 0.9f };
                break;
        }

        // Setup brake sound if clip provided
        if (brakeClip != null)
        {
            brakeSoundSource = gameObject.AddComponent<AudioSource>();
            brakeSoundSource.clip = brakeClip;
            brakeSoundSource.loop = true;
        }
    }

    void OnEnable()
    {
        inputActions.Enable();
        if (inputActions != null)
        {
            inputActions.Player.Move.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
            inputActions.Player.Move.canceled += ctx => moveInput = Vector2.zero;
        }

    }

    void OnDisable()
    {
        inputActions.Disable();
        if (inputActions != null)
        {
            inputActions.Player.Move.performed -= ctx => moveInput = ctx.ReadValue<Vector2>();
            inputActions.Player.Move.canceled -= ctx => moveInput = Vector2.zero;
        }
        engineSound.Stop();
    }

    void Start()
    {
        // Play starting sound if provided
        if (startClip != null)
        {
            AudioSource.PlayClipAtPoint(startClip, transform.position);
        }
        if (engineSound != null) engineSound.Play();
    }

    void Update()
    {
        // Update visual wheel positions and rotations to match colliders
        UpdateWheelPose(wcFL, wheelFL);
        UpdateWheelPose(wcFR, wheelFR);
        UpdateWheelPose(wcRL, wheelRL);
        UpdateWheelPose(wcRR, wheelRR);

        // Update engine sound pitch
        UpdateEngineSound();
    }

    void FixedUpdate()
    {
        // Target orientation is car's forward, but with world up
        Quaternion targetRotation = Quaternion.LookRotation(transform.forward, Vector3.up);

        // Figure out how far off we are
        Quaternion delta = targetRotation * Quaternion.Inverse(rb.rotation);

        // Convert to axis-angle
        delta.ToAngleAxis(out float angle, out Vector3 axis);
        if (angle > 180f) angle -= 360f;

        // Normalize axis in world space
        axis = axis.normalized;

        // Apply corrective torque (proportional to angle)
        Vector3 correctiveTorque = axis * (angle * Mathf.Deg2Rad * uprightTorque);

        // Add damping (fight angular velocity around roll & pitch only)
        Vector3 localAngVel = transform.InverseTransformDirection(rb.angularVelocity);
        localAngVel.x *= uprightDamping; // pitch damping
        localAngVel.z *= uprightDamping; // roll damping
        Vector3 dampingTorque = transform.TransformDirection(localAngVel);

        rb.AddTorque(correctiveTorque - dampingTorque, ForceMode.Acceleration);

        // Steering (horizontal input)
        float steer = moveInput.x * maxSteerAngle;
        wcFL.steerAngle = steer;
        wcFR.steerAngle = steer;

        // Acceleration/deceleration (vertical input)
        float accel = moveInput.y;

        // Calculate current engine RPM
        CalculateEngineRPM();

        // Handle forward movement
        if (accel > 0)
        {
            if (currentGear <= 0)
            {
                currentGear = 1; // Switch from reverse/neutral to first gear
            }
            ShiftGears();
            ApplyMotorTorque(accel);
            ApplyBrakeTorque(0f);
            isBraking = false;
        }
        // Handle reverse or braking
        else if (accel < 0)
        {
            if (currentGear > 0 && rb.linearVelocity.magnitude < 1f)
            {
                currentGear = -1; // Enter reverse if stopped
                ApplyMotorTorque(accel);
            }
            else if (currentGear < 0)
            {
                ApplyMotorTorque(accel);
            }
            else
            {
                // Brake if moving forward
                ApplyMotorTorque(0f);
                ApplyBrakeTorque(-accel * maxBrakeTorque);
                isBraking = true;
            }
        }
        // Coast (no input)
        else
        {
            ApplyMotorTorque(0f);
            ApplyBrakeTorque(0f);
            isBraking = false;
        }

        // Handle braking sound
        if (isBraking && brakeSoundSource != null)
        {
            if (!brakeSoundSource.isPlaying) brakeSoundSource.Play();
        }
        else if (brakeSoundSource != null && brakeSoundSource.isPlaying)
        {
            brakeSoundSource.Stop();
        }

        // Apply downforce for stability
        rb.AddForce(-transform.up * downforce * rb.linearVelocity.magnitude);
    }

    float NormalizeAngle(float angle)
    {
        if (angle > 180f) angle -= 360f;
        return angle;
    }

    private void UpdateWheelPose(WheelCollider collider, Transform wheelTransform)
    {
        if (collider != null && wheelTransform != null)
        {
            collider.GetWorldPose(out Vector3 pos, out Quaternion rot);
            wheelTransform.position = pos;
            wheelTransform.rotation = rot;
        }
    }

    private void CalculateEngineRPM()
    {
        // Average RPM from rear wheels (assuming RWD)
        float avgWheelRPM = (wcRL.rpm + wcRR.rpm) / 2f;
        engineRPM = Mathf.Abs(avgWheelRPM * GetCurrentGearRatio() * finalDriveRatio);
        engineRPM = Mathf.Clamp(engineRPM, 1000f, maxEngineRPM); // Idle min RPM
    }

    private float GetCurrentGearRatio()
    {
        if (currentGear > 0 && currentGear <= gearRatios.Length)
        {
            return gearRatios[currentGear - 1];
        }
        else if (currentGear < 0)
        {
            return reverseGearRatio;
        }
        return 0f; // Neutral
    }

    private void ShiftGears()
    {
        if (engineRPM > upShiftRPM && currentGear < gearRatios.Length)
        {
            currentGear++;
            PlayGearShiftSound();
        }
        else if (engineRPM < downShiftRPM && currentGear > 1)
        {
            currentGear--;
            PlayGearShiftSound();
        }
    }

    private void ApplyMotorTorque(float accel)
    {
        float gearRatio = GetCurrentGearRatio();
        float torquePerWheel = accel * (maxMotorTorque / Mathf.Abs(gearRatio)) / 2f;
        wcRL.motorTorque = torquePerWheel;
        wcRR.motorTorque = torquePerWheel;
    }


    private void ApplyBrakeTorque(float brake)
    {
        wcFL.brakeTorque = brake;
        wcFR.brakeTorque = brake;
        wcRL.brakeTorque = brake;
        wcRR.brakeTorque = brake;
    }

    private void UpdateEngineSound()
    {
        if (engineSound != null)
        {
            engineSound.pitch = (engineRPM / maxEngineRPM) * pitchMultiplier + 0.5f; // Base idle pitch
        }
    }

    private void PlayGearShiftSound()
    {
        if (gearShiftClip != null)
        {
            AudioSource.PlayClipAtPoint(gearShiftClip, transform.position);
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.relativeVelocity.magnitude > collisionSoundThreshold && collisionClip != null)
        {
            AudioSource.PlayClipAtPoint(collisionClip, collision.contacts[0].point);
        }
    }

    [Header("Driver Door")]
    public HingeJoint doorHinge;
    public float openMotorSpeed = 100f;
    public float openMotorForce = 200f;

    public void OpenDoor()
    {
        var motor = doorHinge.motor;
        motor.force = openMotorForce;
        motor.targetVelocity = openMotorSpeed;
        motor.freeSpin = false;
        doorHinge.motor = motor;
        doorHinge.useMotor = true;
    }

    public void StopDoorMotor()
    {
        doorHinge.useMotor = false;
    }
}