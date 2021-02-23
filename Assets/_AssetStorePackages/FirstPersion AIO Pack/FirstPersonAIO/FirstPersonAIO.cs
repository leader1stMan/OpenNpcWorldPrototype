using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
    using UnityEditor;
#endif

[RequireComponent(typeof(CapsuleCollider)),RequireComponent(typeof(Rigidbody)),AddComponentMenu("First Person AIO")]

public class FirstPersonAIO : MonoBehaviour {

    #region Variables

    #region Input Settings

    #endregion

    #region Look Settings
    public bool enableCameraMovement;
    public float verticalRotationRange = 170;
    public float mouseSensitivity = 10;
    public float mouseSensitivityInternal;
    public  float fOVToMouseSensitivity = 1;
    public float cameraSmoothing = 5f;
    public bool lockAndHideCursor = false;
    public Camera playerCamera;
    public bool enableCameraShake=false;
    internal Vector3 cameraStartingPosition;
    float baseCamFOV;
    

    public bool autoCrosshair = false;
    public bool drawStaminaMeter = true;
    float smoothRef;
    Image StaminaMeter;
    Image StaminaMeterBG;
    public Sprite Crosshair;
    public Vector3 targetAngles;
    private Vector3 followAngles;
    private Vector3 followVelocity;
    private Vector3 originalRotation;

    private Animator anim;
    #endregion

    #region Movement Settings

    public bool playerCanMove = true;
    public bool walkByDefault = true;
    public float walkSpeed = 4f;
    public float sprintSpeed = 8f;
    public float jumpPower = 5f;
    public bool canJump = true;
    public bool canHoldJump;
    public bool useStamina = true;
    public float staminaDepletionSpeed = 5f;
    public float staminaLevel = 50;
    public float speed;
    public float staminaInternal;
    internal float walkSpeedInternal;
    internal float sprintSpeedInternal;
    internal float jumpPowerInternal;

    [System.Serializable]
    public class CrouchModifiers {
        public bool useCrouch = true;
        public bool toggleCrouch = false;
        public KeyCode crouchKey = KeyCode.LeftControl;
        public float crouchWalkSpeedMultiplier = 0.5f;
        public float crouchJumpPowerMultiplier = 0f;
        public bool crouchOverride;
        internal float colliderHeight;
        
    }
    public CrouchModifiers _crouchModifiers = new CrouchModifiers();
    [System.Serializable]
    public class FOV_Kick
    {
        public bool useFOVKick = false;
        public float FOVKickAmount = 4;
        public float changeTime = 0.1f;
        public AnimationCurve KickCurve = new AnimationCurve();
        public float fovStart;
    }
    public FOV_Kick fOVKick = new FOV_Kick();
    public class AdvancedSettings {
        public float gravityMultiplier = 1.0f;
        public PhysicMaterial zeroFrictionMaterial;
        public PhysicMaterial highFrictionMaterial;
        public float maxSlopeAngle=70;
        public bool tooSteep;
        public RaycastHit surfaceAngleCheck;
    }
    public AdvancedSettings advanced = new AdvancedSettings();
    private CapsuleCollider capsule;
    private const float jumpRayLength = 0.7f;
    public bool IsGrounded { get; private set; }
    Vector2 inputXY;
    public bool isCrouching;
    bool isSprinting = false;

    public Rigidbody fps_Rigidbody;

    #endregion

    #region Headbobbing Settings
    public bool useHeadbob = true;
    public Transform head = null;
    public float headbobFrequency = 1.5f;
    public float headbobSwayAngle = 5f;
    public float headbobHeight = 3f;
    public float headbobSideMovement =5f;  
    public bool useJumdLandMovement = true;
    public float jumpAngle =3f;
    public float landAngle = 60;
    private Vector3 originalLocalPosition;
    private float nextStepTime = 0.5f;
    private float headbobCycle = 0.0f;
    private float headbobFade = 0.0f;
    private float springPosition = 0.0f;
    private float springVelocity = 0.0f;
    private float springElastic = 1.1f;
    private float springDampen = 0.8f;
    private float springVelocityThreshold = 0.05f;
    private float springPositionThreshold = 0.05f;
    Vector3 previousPosition;
    Vector3 previousVelocity = Vector3.zero;
    Vector3 miscRefVel;
    bool previousGrounded;
    AudioSource audioSource;

    Vector3 lastPosition;
    Transform myTransform;
    bool isMoving;

    public EventGameObject OnClickAttackable;
    #endregion

    #region Audio Settings

    public float Volume = 5f;
    public AudioClip jumpSound = null;
    public AudioClip landSound = null;
    public bool _useFootStepSounds = false;
    public List<AudioClip> footStepSounds = null;
    public enum FSMode{Static, Dynamic}
    public FSMode fsmode;
 
    [System.Serializable]
    public class DynamicFootStep{
        public PhysicMaterial woodPhysMat;
        public PhysicMaterial metalAndGlassPhysMat;
        public PhysicMaterial grassPhysMat;
        public PhysicMaterial dirtAndGravelPhysMat;
        public PhysicMaterial rockAndConcretePhysMat;
        public PhysicMaterial mudPhysMat;
        public PhysicMaterial customPhysMat;
        public List<AudioClip> currentClipSet;

        public List<AudioClip> woodClipSet;
        public List<AudioClip> metalAndGlassClipSet;
        public List<AudioClip> grassClipSet;
        public List<AudioClip> dirtAndGravelClipSet;
        public List<AudioClip> rockAndConcreteClipSet;
        public List<AudioClip> mudClipSet;
        public List<AudioClip> customClipSet;
    }
    public DynamicFootStep dynamicFootstep = new DynamicFootStep();

    #endregion

    #region BETA Settings
    /*
     [System.Serializable]
public class BETA_SETTINGS{

}

            [Space(15)]
    [Tooltip("Settings in this feild are currently in beta testing and can prove to be unstable.")]
    [Space(5)]
    public BETA_SETTINGS betaSettings = new BETA_SETTINGS();
     */
    
    #endregion

    #endregion

    private void Awake()
    {
        #region Look Settings - Awake
        originalRotation = transform.localRotation.eulerAngles;

        #endregion 

        #region Movement Settings - Awake
        walkSpeedInternal = walkSpeed;
        sprintSpeedInternal = sprintSpeed;
        jumpPowerInternal = jumpPower;
        capsule = GetComponent<CapsuleCollider>();
        IsGrounded = true;
        isCrouching = false;
        fps_Rigidbody = GetComponent<Rigidbody>();
        _crouchModifiers.colliderHeight = capsule.height;
        #endregion

        #region Headbobbing Settings - Awake

        #endregion

        #region BETA_SETTINGS - Awake

        #endregion

    }

    private void Start()
    {
        #region Look Settings - Start

        if(autoCrosshair || drawStaminaMeter){
            Canvas canvas = new GameObject("AutoCrosshair").AddComponent<Canvas>();
            canvas.gameObject.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.pixelPerfect = true;
            canvas.transform.SetParent(playerCamera.transform);
            canvas.transform.position = Vector3.zero;

            if(autoCrosshair){
                Image crossHair = new GameObject("Crosshair").AddComponent<Image>();
                crossHair.sprite = Crosshair;
                crossHair.rectTransform.sizeDelta = new Vector2(25,25);
                crossHair.transform.SetParent(canvas.transform);
                crossHair.transform.position = Vector3.zero;
            }

            if(drawStaminaMeter){
                StaminaMeterBG = new GameObject("StaminaMeter").AddComponent<Image>();
                StaminaMeter = new GameObject("Meter").AddComponent<Image>();
                StaminaMeter.transform.SetParent(StaminaMeterBG.transform);
                StaminaMeterBG.transform.SetParent(canvas.transform);
                StaminaMeterBG.transform.position = Vector3.zero;
                StaminaMeterBG.rectTransform.anchorMax = new Vector2(0.5f,0);
                StaminaMeterBG.rectTransform.anchorMin = new Vector2(0.5f,0);
                StaminaMeterBG.rectTransform.anchoredPosition = new Vector2(0,15);
                StaminaMeterBG.rectTransform.sizeDelta = new Vector2(250,6);
                StaminaMeterBG.color = new Color(0,0,0,0);
                StaminaMeter.rectTransform.sizeDelta = new Vector2(250,6);
                StaminaMeter.color = new Color(0,0,0,0);
            }
        }
        mouseSensitivityInternal = mouseSensitivity;
        cameraStartingPosition = playerCamera.transform.localPosition;
        if(lockAndHideCursor) { Cursor.lockState = CursorLockMode.Locked; Cursor.visible = false; }
        baseCamFOV = playerCamera.fieldOfView;
        #endregion

        #region Movement Settings - Start  
        staminaInternal = staminaLevel;
        advanced.zeroFrictionMaterial = new PhysicMaterial("Zero_Friction");
        advanced.zeroFrictionMaterial.dynamicFriction =0;
        advanced.zeroFrictionMaterial.staticFriction =0;
        advanced.zeroFrictionMaterial.frictionCombine = PhysicMaterialCombine.Minimum;
        advanced.zeroFrictionMaterial.bounceCombine = PhysicMaterialCombine.Minimum;
        advanced.highFrictionMaterial = new PhysicMaterial("Max_Friction");
        advanced.highFrictionMaterial.dynamicFriction =1;
        advanced.highFrictionMaterial.staticFriction =1;
        advanced.highFrictionMaterial.frictionCombine = PhysicMaterialCombine.Maximum;
        advanced.highFrictionMaterial.bounceCombine = PhysicMaterialCombine.Average;

        anim = GetComponent<Animator>();
        myTransform = transform;
        lastPosition = myTransform.position;
        isMoving = false;
        #endregion

        #region Headbobbing Settings - Start
        originalLocalPosition = head.localPosition;
        if(GetComponent<AudioSource>() == null) { gameObject.AddComponent<AudioSource>(); }
        previousPosition = fps_Rigidbody.position;
        audioSource = GetComponent<AudioSource>();
        #endregion

        #region BETA_SETTINGS - Start
        fOVKick.fovStart = playerCamera.fieldOfView;
        #endregion
    }

    private void Update()
    {
        #region Look Settings - Update

            if(enableCameraMovement){
            float mouseXInput;
            float mouseYInput;
            float camFOV = playerCamera.fieldOfView;
            mouseXInput = Input.GetAxis("Mouse Y");
            mouseYInput = Input.GetAxis("Mouse X");
            if(targetAngles.y > 180) { targetAngles.y -= 360; followAngles.y -= 360; } else if(targetAngles.y < -180) { targetAngles.y += 360; followAngles.y += 360; }
            if(targetAngles.x > 180) { targetAngles.x -= 360; followAngles.x -= 360; } else if(targetAngles.x < -180) { targetAngles.x += 360; followAngles.x += 360; }
            targetAngles.y += mouseYInput * (mouseSensitivityInternal - ((baseCamFOV-camFOV)*fOVToMouseSensitivity)/6f);
            targetAngles.x += mouseXInput * (mouseSensitivityInternal - ((baseCamFOV-camFOV)*fOVToMouseSensitivity)/6f);
            targetAngles.y = Mathf.Clamp(targetAngles.y, -0.5f * Mathf.Infinity, 0.5f * Mathf.Infinity);
            targetAngles.x = Mathf.Clamp(targetAngles.x, -0.5f * verticalRotationRange, 0.5f * verticalRotationRange);
            followAngles = Vector3.SmoothDamp(followAngles, targetAngles, ref followVelocity, (cameraSmoothing)/100);
            playerCamera.transform.localRotation = Quaternion.Euler(-followAngles.x + originalRotation.x,0,0);
            transform.localRotation =  Quaternion.Euler(0, followAngles.y+originalRotation.y, 0);
        }

        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Input.GetMouseButtonDown(0))
        {
            anim.SetBool("isAttacking", true);
            int layerMask = LayerMask.GetMask("Player");
            layerMask = ~layerMask;
            if (Physics.Raycast(ray, out hit, Mathf.Infinity, layerMask))
            {
                GameObject attackable = hit.collider.gameObject;
                OnClickAttackable.Invoke(attackable);
            }
        }
        else
        {
            anim.SetBool("isAttacking", false);
        }

        #endregion

        #region Movement Settings - Update

        #endregion

        #region Headbobbing Settings - Update

        #endregion

        #region BETA_SETTINGS - Update

        #endregion
    }

    private void FixedUpdate()
    {
        #region Look Settings - FixedUpdate

        #endregion

        #region Movement Settings - FixedUpdate
        
        bool wasWalking = !isSprinting;
        if (useStamina){
            isSprinting = Input.GetKey(KeyCode.LeftShift) && !isCrouching && staminaInternal > 0 && (Mathf.Abs(fps_Rigidbody.velocity.x) > 0.01f || Mathf.Abs(fps_Rigidbody.velocity.x) > 0.01f);
            if(isSprinting){
                staminaInternal -= (staminaDepletionSpeed*2)*Time.deltaTime;
                if(drawStaminaMeter){
                    StaminaMeterBG.color = Vector4.MoveTowards(StaminaMeterBG.color, new Vector4(0,0,0,0.5f),0.15f);
                    StaminaMeter.color = Vector4.MoveTowards(StaminaMeter.color, new Vector4(1,1,1,1),0.15f);
                }
            }else if((!Input.GetKey(KeyCode.LeftShift)||Mathf.Abs(fps_Rigidbody.velocity.x)< 0.01f || Mathf.Abs(fps_Rigidbody.velocity.x)< 0.01f || isCrouching)&&staminaInternal<staminaLevel){
                staminaInternal += staminaDepletionSpeed*Time.deltaTime;
            }
                if(drawStaminaMeter&&staminaInternal==staminaLevel){
                    StaminaMeterBG.color = Vector4.MoveTowards(StaminaMeterBG.color, new Vector4(0,0,0,0),0.15f);
                    StaminaMeter.color = Vector4.MoveTowards(StaminaMeter.color, new Vector4(1,1,1,0),0.15f);
                }
                staminaInternal = Mathf.Clamp(staminaInternal,0,staminaLevel);
                float x = Mathf.Clamp(Mathf.SmoothDamp(StaminaMeter.transform.localScale.x,(staminaInternal/staminaLevel)*StaminaMeterBG.transform.localScale.x,ref smoothRef,(1)*Time.deltaTime,1),0.001f, StaminaMeterBG.transform.localScale.x);
                StaminaMeter.transform.localScale = new Vector3(x,1,1); 
        } else{isSprinting = Input.GetKey(KeyCode.LeftShift);}

        advanced.tooSteep = false;
        float inrSprintSpeed;
        inrSprintSpeed = sprintSpeedInternal;
        Vector3 dMove = Vector3.zero;
        speed = walkByDefault ? isCrouching ? walkSpeedInternal : (isSprinting ? inrSprintSpeed : walkSpeedInternal) : (isSprinting ? walkSpeedInternal : inrSprintSpeed);
        Ray ray = new Ray(transform.position - new Vector3(0,(capsule.height/2)-0.01f,0) , -transform.up);
        Debug.DrawLine(ray.origin,ray.origin - new Vector3(0,0.05f,0),Color.black);
        if(IsGrounded || fps_Rigidbody.velocity.y < 0.1) {
            RaycastHit[] hits = Physics.RaycastAll(ray, 0.05f);
            float nearest = float.PositiveInfinity;
            IsGrounded = false;
            for(int i = 0; i < hits.Length; i++) {
                if(!hits[i].collider.isTrigger && hits[i].distance < nearest) {
                    IsGrounded = true;
                    nearest = hits[i].distance;
                }
            }
        }
  


       
    if(advanced.maxSlopeAngle>0){
        if(Physics.Raycast(new Vector3(transform.position.x,transform.position.y-0.75f,transform.position.z+0.1f), Vector3.down,out advanced.surfaceAngleCheck,1f)){
        
            if(Vector3.Angle(advanced.surfaceAngleCheck.normal, Vector3.up)<89){
                        advanced.tooSteep = false;                       
                        dMove = transform.forward * inputXY.y * speed + transform.right * inputXY.x * speed;           
              if(Vector3.Angle(advanced.surfaceAngleCheck.normal, Vector3.up)>advanced.maxSlopeAngle){
                        advanced.tooSteep = true;
                         isSprinting=false;
                        dMove = new Vector3(0,-4,0);
                        
            }else if(Vector3.Angle(advanced.surfaceAngleCheck.normal, Vector3.up)>44){
                        advanced.tooSteep = true;
                        isSprinting=false;
                        dMove = (transform.forward * inputXY.y * speed + transform.right * inputXY.x) + new Vector3(0,-4,0);
                }
            }    
    }
    
      else  if(Physics.Raycast( new Vector3(transform.position.x-0.086f,transform.position.y-0.75f,transform.position.z-0.05f), Vector3.down,out advanced.surfaceAngleCheck,1f)){
       
            if(Vector3.Angle(advanced.surfaceAngleCheck.normal, Vector3.up)<89){
                        advanced.tooSteep = false;             
                        dMove = transform.forward * inputXY.y * speed + transform.right * inputXY.x * walkSpeedInternal;           
              if(Vector3.Angle(advanced.surfaceAngleCheck.normal, Vector3.up)>70){
                        advanced.tooSteep = true;
                         isSprinting=false;
                        dMove = new Vector3(0,-4,0);
                        
            }else if(Vector3.Angle(advanced.surfaceAngleCheck.normal, Vector3.up)>45){
                        advanced.tooSteep = true;
                        isSprinting=false;
                        dMove = (transform.forward * inputXY.y * speed + transform.right * inputXY.x) + new Vector3(0,-4,0);
                       
                }
            }    
            else  if(Physics.Raycast( new Vector3(transform.position.x+0.086f,transform.position.y-0.75f,transform.position.z-0.05f), Vector3.down,out advanced.surfaceAngleCheck,1f)){
        
            if(Vector3.Angle(advanced.surfaceAngleCheck.normal, Vector3.up)<89){
                        advanced.tooSteep = false;                   
                        dMove = transform.forward * inputXY.y * speed + transform.right * inputXY.x * walkSpeedInternal;
              if(Vector3.Angle(advanced.surfaceAngleCheck.normal, Vector3.up)>70){
                        advanced.tooSteep = true;
                         isSprinting=false;
                        dMove = new Vector3(0,-4,0);
                        
            }else if(Vector3.Angle(advanced.surfaceAngleCheck.normal, Vector3.up)>45){
                        advanced.tooSteep = true;
                        isSprinting=false;
                        dMove = (transform.forward * inputXY.y * speed + transform.right * inputXY.x) + new Vector3(0,-4,0);
                    }
                }
            }
        }else{advanced.tooSteep = false;
                        dMove = transform.forward * inputXY.y * speed + transform.right * inputXY.x * walkSpeedInternal;
            }    
    }
         else{advanced.tooSteep = false;
                        dMove = transform.forward * inputXY.y * speed + transform.right * inputXY.x * walkSpeedInternal;
            }


        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");
        inputXY = new Vector2(horizontalInput, verticalInput);
        if(inputXY.magnitude > 1) { inputXY.Normalize(); }
       
        float yv = fps_Rigidbody.velocity.y;
        bool didJump = canHoldJump?Input.GetButton("Jump"): Input.GetButton("Jump");

        if (!canJump) didJump = false;

        if(IsGrounded && didJump && jumpPowerInternal > 0)
        {
            yv += jumpPowerInternal;
            IsGrounded = false;
            didJump=false;
        }

        if(playerCanMove)
        {
            fps_Rigidbody.velocity = dMove + Vector3.up * yv;
        } else{fps_Rigidbody.velocity = Vector3.zero;}

        if(dMove.magnitude > 0 || !IsGrounded || advanced.tooSteep) {
            capsule.sharedMaterial = advanced.zeroFrictionMaterial;
        } else { capsule.sharedMaterial = advanced.highFrictionMaterial; }

        fps_Rigidbody.AddForce(Physics.gravity * (advanced.gravityMultiplier - 1));
        /* if(fOVKick.useFOVKick && wasWalking == isSprinting && fps_Rigidbody.velocity.magnitude > 0.1f && !isCrouching){
            StopAllCoroutines();
            StartCoroutine(wasWalking ? FOVKickOut() : FOVKickIn());
        } */

        if(_crouchModifiers.useCrouch) {
            if(!_crouchModifiers.toggleCrouch){ isCrouching = _crouchModifiers.crouchOverride || Input.GetKey(_crouchModifiers.crouchKey);}
            else{if(Input.GetKeyDown(_crouchModifiers.crouchKey)){isCrouching = !isCrouching || _crouchModifiers.crouchOverride;}}

            if(isCrouching) {
                    capsule.height = Mathf.MoveTowards(capsule.height, _crouchModifiers.colliderHeight/2, 5*Time.deltaTime);
                        walkSpeedInternal = walkSpeed*_crouchModifiers.crouchWalkSpeedMultiplier;
                        jumpPowerInternal = jumpPower* _crouchModifiers.crouchJumpPowerMultiplier;
                } else {
                capsule.height = Mathf.MoveTowards(capsule.height, _crouchModifiers.colliderHeight, 5*Time.deltaTime);    
                walkSpeedInternal = walkSpeed;
                jumpPowerInternal = jumpPower;
            }
        }

        if (myTransform.position != lastPosition)
            isMoving = true;
        else
            isMoving = false;

        if (isMoving)
        {
            anim.SetBool("IsSprinting", true);
        }
        else
        {
            anim.SetBool("IsSprinting", false);
        }
        lastPosition = myTransform.position;
        #endregion

        #region BETA_SETTINGS - FixedUpdate

        #endregion

        #region Headbobbing Settings - FixedUpdate
        float yPos = 0;
        float xPos = 0;
        float zTilt = 0;
        float xTilt = 0;
        float bobSwayFactor = 0;
        float bobFactor = 0;
        float strideLangthen = 0;
        float flatVel = 0;

        if(useHeadbob == true || fsmode == FSMode.Dynamic || _useFootStepSounds == true){
            Vector3 vel = (fps_Rigidbody.position - previousPosition) / Time.deltaTime;
            Vector3 velChange = vel - previousVelocity;
            previousPosition = fps_Rigidbody.position;
            previousVelocity = vel;
            springVelocity -= velChange.y;
            springVelocity -= springPosition * springElastic;
            springVelocity *= springDampen;
            springPosition += springVelocity * Time.deltaTime;
            springPosition = Mathf.Clamp(springPosition, -0.3f, 0.3f);

            if(Mathf.Abs(springVelocity) < springVelocityThreshold && Mathf.Abs(springPosition) < springPositionThreshold) { springPosition = 0; springVelocity = 0; }
            flatVel = new Vector3(vel.x, 0.0f, vel.z).magnitude;
            strideLangthen = 1 + (flatVel * ((headbobFrequency*2)/10));
            headbobCycle += (flatVel / strideLangthen) * (Time.deltaTime / headbobFrequency);
            bobFactor = Mathf.Sin(headbobCycle * Mathf.PI * 2);
            bobSwayFactor = Mathf.Sin(Mathf.PI * (2 * headbobCycle + 0.5f));
            bobFactor = 1 - (bobFactor * 0.5f + 1);
            bobFactor *= bobFactor;

            yPos = 0;
            xPos = 0;
            zTilt = 0;
            if(useJumdLandMovement){xTilt = -springPosition * landAngle;}
            else{xTilt = -springPosition;}

            if(IsGrounded)
            {
                if(new Vector3(vel.x, 0.0f, vel.z).magnitude < 0.1f) { headbobFade = Mathf.MoveTowards(headbobFade, 0.0f,0.5f); } else { headbobFade = Mathf.MoveTowards(headbobFade, 1.0f, Time.deltaTime); }
                float speedHeightFactor = 1 + (flatVel * 0.3f);
                xPos = -(headbobSideMovement/10) * headbobFade *bobSwayFactor;
                yPos = springPosition * (jumpAngle/10) + bobFactor * (headbobHeight/10) * headbobFade * speedHeightFactor;
                zTilt = bobSwayFactor * (headbobSwayAngle/10) * headbobFade;
            }
        }

            if(useHeadbob == true){
                if(fps_Rigidbody.velocity.magnitude >0.1f){
                    head.localPosition = Vector3.MoveTowards(head.localPosition, originalLocalPosition + new Vector3(xPos, yPos, 0),0.5f);
                }else{
                    head.localPosition = Vector3.SmoothDamp(head.localPosition, originalLocalPosition,ref miscRefVel, 0.15f);
                }
                head.localRotation = Quaternion.Euler(xTilt, 0, zTilt);
                
           
        }

            if(fsmode == FSMode.Dynamic)
            {
                Vector3 dwn = Vector3.down;
                RaycastHit hit = new RaycastHit();
                if(Physics.Raycast(transform.position, dwn, out hit))
                {
                    dynamicFootstep.currentClipSet = (dynamicFootstep.woodPhysMat && hit.collider.sharedMaterial == dynamicFootstep.woodPhysMat && dynamicFootstep.woodClipSet.Any()) ? // If standing on Wood
                    dynamicFootstep.woodClipSet : ((dynamicFootstep.grassPhysMat && hit.collider.sharedMaterial == dynamicFootstep.grassPhysMat && dynamicFootstep.grassClipSet.Any()) ? // If standing on Grass
                    dynamicFootstep.grassClipSet : ((dynamicFootstep.metalAndGlassPhysMat && hit.collider.sharedMaterial == dynamicFootstep.metalAndGlassPhysMat && dynamicFootstep.metalAndGlassClipSet.Any()) ? // If standing on Metal/Glass
                    dynamicFootstep.metalAndGlassClipSet : ((dynamicFootstep.rockAndConcretePhysMat && hit.collider.sharedMaterial == dynamicFootstep.rockAndConcretePhysMat && dynamicFootstep.rockAndConcreteClipSet.Any()) ? // If standing on Rock/Concrete
                    dynamicFootstep.rockAndConcreteClipSet : ((dynamicFootstep.dirtAndGravelPhysMat && hit.collider.sharedMaterial == dynamicFootstep.dirtAndGravelPhysMat && dynamicFootstep.dirtAndGravelClipSet.Any()) ? // If standing on Dirt/Gravle
                    dynamicFootstep.dirtAndGravelClipSet : ((dynamicFootstep.mudPhysMat && hit.collider.sharedMaterial == dynamicFootstep.mudPhysMat && dynamicFootstep.mudClipSet.Any())? // If standing on Mud
                    dynamicFootstep.mudClipSet : ((dynamicFootstep.customPhysMat && hit.collider.sharedMaterial == dynamicFootstep.customPhysMat && dynamicFootstep.customClipSet.Any())? // If standing on the custom material 
                    dynamicFootstep.customClipSet : footStepSounds)))))); // If material is unknown, fall back

                    if(IsGrounded)
                    {
                        if(!previousGrounded)
                        {
                            if(_useFootStepSounds && dynamicFootstep.currentClipSet.Any()) { audioSource.PlayOneShot(dynamicFootstep.currentClipSet[Random.Range(0, dynamicFootstep.currentClipSet.Count)],Volume/10); }
                            nextStepTime = headbobCycle + 0.5f;
                        } else
                        {
                            if(headbobCycle > nextStepTime)
                            {
                                nextStepTime = headbobCycle + 0.5f;
                                if(_useFootStepSounds && dynamicFootstep.currentClipSet.Any()){ audioSource.PlayOneShot(dynamicFootstep.currentClipSet[Random.Range(0, dynamicFootstep.currentClipSet.Count)],Volume/10); }
                            }
                        }
                        previousGrounded = true;
                    } else
                    {
                        if(previousGrounded)
                        {
                            if(_useFootStepSounds && dynamicFootstep.currentClipSet.Any()){ audioSource.PlayOneShot(dynamicFootstep.currentClipSet[Random.Range(0, dynamicFootstep.currentClipSet.Count)],Volume/10); }
                        }
                        previousGrounded = false;
                    }

                } else {
                    dynamicFootstep.currentClipSet = footStepSounds;
                    if(IsGrounded)
                    {
                        if(!previousGrounded)
                        {
                            if(_useFootStepSounds && landSound){ audioSource.PlayOneShot(landSound,Volume/10); }
                            nextStepTime = headbobCycle + 0.5f;
                        } else
                        {
                            if(headbobCycle > nextStepTime)
                            {
                                nextStepTime = headbobCycle + 0.5f;
                                int n = Random.Range(0, footStepSounds.Count);
                                if(_useFootStepSounds && footStepSounds.Any()){ audioSource.PlayOneShot(footStepSounds[n],Volume/10); }
                                footStepSounds[n] = footStepSounds[0];
                            }
                        }
                        previousGrounded = true;
                    } else
                    {
                        if(previousGrounded)
                        {
                            if(_useFootStepSounds && jumpSound){ audioSource.PlayOneShot(jumpSound,Volume/10); }
                        }
                        previousGrounded = false;
                    }
                }
                
            } else
            {
                if(IsGrounded)
                {
                    if(!previousGrounded)
                    {
                        if(_useFootStepSounds && landSound) { audioSource.PlayOneShot(landSound,Volume/10); }
                        nextStepTime = headbobCycle + 0.5f;
                    } else
                    {
                        if(headbobCycle > nextStepTime)
                        {
                            nextStepTime = headbobCycle + 0.5f;
                            int n = Random.Range(0, footStepSounds.Count);
                            if(_useFootStepSounds && footStepSounds.Any()){ audioSource.PlayOneShot(footStepSounds[n],Volume/10);}
                            
                        }
                    }
                    previousGrounded = true;
                } else
                {
                    if(previousGrounded)
                    {
                        if(_useFootStepSounds && jumpSound) { audioSource.PlayOneShot(jumpSound,Volume/10); }
                    }
                    previousGrounded = false;
                }
            }

        
        #endregion

    }

/*     public IEnumerator FOVKickOut()
    {
        float t = Mathf.Abs((playerCamera.fieldOfView - fOVKick.fovStart) / fOVKick.FOVKickAmount);
        while(t < fOVKick.changeTime)
        {
            playerCamera.fieldOfView = fOVKick.fovStart + (fOVKick.KickCurve.Evaluate(t / fOVKick.changeTime) * fOVKick.FOVKickAmount);
            t += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
    }

    public IEnumerator FOVKickIn()
    {
        float t = Mathf.Abs((playerCamera.fieldOfView - fOVKick.fovStart) / fOVKick.FOVKickAmount);
        while(t > 0)
        {
            playerCamera.fieldOfView = fOVKick.fovStart + (fOVKick.KickCurve.Evaluate(t / fOVKick.changeTime) * fOVKick.FOVKickAmount);
            t -= Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
        playerCamera.fieldOfView = fOVKick.fovStart;
    } */

    public IEnumerator CameraShake(float Duration, float Magnitude){
        float elapsed =0;
        while(elapsed<Duration && enableCameraShake){
            playerCamera.transform.localPosition =Vector3.MoveTowards(playerCamera.transform.localPosition, new Vector3(cameraStartingPosition.x+ Random.Range(-1,1)*Magnitude,cameraStartingPosition.y+Random.Range(-1,1)*Magnitude,cameraStartingPosition.z), Magnitude*2);
            yield return new WaitForSecondsRealtime(0.001f);
            elapsed += Time.deltaTime;
            yield return null;
        }
        playerCamera.transform.localPosition = cameraStartingPosition;
    }

}

[System.Serializable]
public class EventGameObject : UnityEvent<GameObject> { }

