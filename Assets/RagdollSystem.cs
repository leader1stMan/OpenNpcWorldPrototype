using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;
using UnityEditor;
using System.Collections.Generic;

public class RagdollSystem : MonoBehaviour
{

    public float ragdollCooldown = 5f;
    public float ragdollCooldownMax = 5f;
    public bool ForceRagdoll = false;
    public bool DisabledKinematic = false;
    public Rigidbody Rig;
    public int RigType = 0;
    public CharacterStats stats;
    public NavMeshAgent agent;
    public Animator anim;
    public float SRadius = 0.0008f;
    bool OverRode = false;
    public bool Debug = false;
    public int DollID = 15;
    int ORC = 0;
    bool RagSwitch = false;
    bool RagStarted = false;
    void Start()
    {
       
            Rig = GetComponent<Rigidbody>();
      
        agent = GetComponent<NavMeshAgent>();
        stats = GetComponent<CharacterStats>();
        anim = GetComponentInChildren<Animator>();
        if (gameObject.name.Contains("Player"))
        {
           // RigType = 1;
        }
        else if(GetComponent<NPC>() != null)
        {
            RigType = 2;
        }
        if (GetComponent<SkeletonAi>() != null || GetComponent<SkeletonArcherAI>() != null )
        {
            OverRode = true;
        }
            
    }







    void FixedUpdate()
    {
        if (OverRode == false || ForceRagdoll == true || stats.isDead == true && stats.isRagdolled == false)
        {
            if (stats.isRagdolled == true || stats.isDead == true && stats.isRagdolled == false || ForceRagdoll == true)
            {
                if (ForceRagdoll == true || stats.isDead == true)
                {
                    if(OverRode == true)
                    {
                        OverRode = false;
                        ORC = 1;
                    }
                    stats.isRagdolled = true;
                    ForceRagdoll = false;
                }
                DoRagdoll();
            }
            else 
            {
                DoGetUp();
            }
        }
        
        
    }
    public void DoRagdoll()
    {
        if (ragdollCooldown > 0)
        {

            //PlayAnimation("isRagdolled", false);
            RagEffect();
            ragdollCooldown -= Time.deltaTime;
        }
        else
        {
            if(ORC == 1)
            {
                OverRode = true;
                ORC = 0;
                
            }
            RagSwitch = true;
            stats.isRagdolled = false;
            ragdollCooldown = ragdollCooldownMax;
            
            // PlayAnimation("GetUp", false);
            //  ChangeState(EnemyState.GetUp);
        }


    }


   

    public void DoGetUp()
    {
        if (RagSwitch == true && stats.isDead == false)
        {
            RagSwitch = false;
            RagOff();
        }
           
        
    }


    public void RagEffect()
    {
       
        if (RagStarted == false)
        {
            RagStarted = true;
            if (DisabledKinematic == true)
            {
                Rig.isKinematic = false;
                
            }
            Rig.useGravity = false;
            CapsuleCollider BC = GetComponent<CapsuleCollider>();

            if (anim != null) { anim.enabled = false; }


            if (agent != null) { agent.enabled = false; }


            if (BC != null) { BC.enabled = false; }


            if (Rig != null) { Rig.useGravity = false; }





            if (RigType == 0)
            {

                Rig0();
            }
            else if (RigType == 1)
            {

                Rig1();
            }
            else if (RigType == 2)
            {

                Rig2();
            }
        }
    }




    void Rig0()
    {
        Transform[] allChildren = GetComponentsInChildren<Transform>();
        foreach (Transform t in allChildren)
        {
            GameObject g = t.gameObject;
            if (g.name.Contains("Pelvis"))
            {
                BoxCollider Cp = g.AddComponent<BoxCollider>();
                Rigidbody Rp = g.AddComponent<Rigidbody>();
                Rp.mass = 10f;
                Rp.drag = 5f;
                Rp.angularDrag = 1f;
                Cp.size = new Vector3(0.005f, 0.005f, 0.005f);
                g.layer = DollID;
            }
            if (g.name.Contains("Head") || g.name.Contains("R Foot") || g.name.Contains("L Foot") ||
                g.name.Contains("Calf") || g.name.Contains("Arm") || g.name.Contains("Clav") ||
                g.name.Contains("Spine") || g.name.Contains("Thigh") || g.name.Contains("Neck"))
            {
                if (g.name.Contains("Neck") == false)
                {
                    CapsuleCollider C = g.AddComponent<CapsuleCollider>();
                    // C.size = new Vector3(0.005f, 0.005f, 0.005f);
                    C.height = 0.02f;
                    C.radius = 0.01f;
                }

                CharacterJoint CJ = g.AddComponent<CharacterJoint>();
                // CJ.connectedBody = Rig;
                SoftJointLimit jointLimit = CJ.lowTwistLimit;
                jointLimit.limit = -5f;
                CJ.lowTwistLimit = jointLimit;

                SoftJointLimit jointLimitH = CJ.highTwistLimit;
                jointLimitH.limit = 25f;
                CJ.highTwistLimit = jointLimitH;

                SoftJointLimit jointLimit1 = CJ.swing1Limit;
                jointLimit1.limit = 25f;
                CJ.swing1Limit = jointLimit1;

                SoftJointLimit jointLimit2 = CJ.swing2Limit;
                jointLimit2.limit = 25f;
                CJ.swing2Limit = jointLimit2;


                g.layer = DollID; //LayerMask.NameToLayer(DollID);


            }

        }

        foreach (Transform t in allChildren)
        {
            GameObject g = t.gameObject;
            if (g.name.Contains("Head") || g.name.Contains("R Foot") || g.name.Contains("L Foot") ||
                 g.name.Contains("Calf") || g.name.Contains("Arm") || g.name.Contains("Clav") ||
                 g.name.Contains("Spine") || g.name.Contains("Thigh") || g.name.Contains("Neck"))
            {

                CharacterJoint CJ = g.GetComponent<CharacterJoint>();
                CJ.connectedBody = t.parent.gameObject.GetComponent<Rigidbody>();



                g.GetComponent<Rigidbody>().mass = 20f;
                g.GetComponent<Rigidbody>().drag = 5f;
                g.GetComponent<Rigidbody>().angularDrag = 1f;
            }

        }




    }

    void Rig1()
    {
        Transform[] allChildren = GetComponentsInChildren<Transform>();
        foreach (Transform t in allChildren)
        {
            GameObject g = t.gameObject;
            
            if (g.name.Contains("Rib") ||g.name.Contains("Skull") || g.name.Contains("Foot") ||
                g.name.Contains("Hand") || g.name.Contains("Arm") && g.name.Contains("Fix") == false  ||
                g.name.Contains("Spinne") || g.name.Contains("Pelvis") || g.name.Contains("Leg") || g.name.Contains("Neck"))
            {
                if (g.name.Contains("Neck") == false)
                {
                    SphereCollider C = g.AddComponent<SphereCollider>();
                    // C.size = new Vector3(0.005f, 0.005f, 0.005f);
                    //  C.height = 0.001f;
                    C.radius = SRadius;
                }
                if (g.name.Contains("Pelvis") == false)
                {
                    CharacterJoint CJ = g.AddComponent<CharacterJoint>();
                    // CJ.connectedBody = Rig;
                    SoftJointLimit jointLimit = CJ.lowTwistLimit;
                    jointLimit.limit = -5f;
                    CJ.lowTwistLimit = jointLimit;

                    SoftJointLimit jointLimitH = CJ.highTwistLimit;
                    jointLimitH.limit = 25f;
                    CJ.highTwistLimit = jointLimitH;

                    SoftJointLimit jointLimit1 = CJ.swing1Limit;
                    jointLimit1.limit = 25f;
                    CJ.swing1Limit = jointLimit1;

                    SoftJointLimit jointLimit2 = CJ.swing2Limit;
                    jointLimit2.limit = 25f;
                    CJ.swing2Limit = jointLimit2;
                }
                else
                {
                    g.AddComponent<Rigidbody>();
                    g.GetComponent<Rigidbody>().mass = 20f;
                    g.GetComponent<Rigidbody>().drag = 5f;
                    g.GetComponent<Rigidbody>().angularDrag = 1f;
                }






                g.layer = DollID; //g.layer = LayerMask.NameToLayer("RagBox");


            }

        }

        foreach (Transform t in allChildren)
        {
            GameObject g = t.gameObject;
            string name = g.name;
            if (g.name.Contains("Rib") || g.name.Contains("Skull") || g.name.Contains("Foot") ||
               g.name.Contains("Hand") || g.name.Contains("Arm") && g.name.Contains("Fix") == false  ||
               g.name.Contains("Spinne") || g.name.Contains("Leg") || g.name.Contains("Neck"))
            {
                

                CharacterJoint CJ = g.GetComponent<CharacterJoint>();
                if (name.Contains("Rib") && name.Contains("Fix") == false || name.Contains("ArmUpper"))
                {
                    CJ.connectedBody = t.parent.parent.gameObject.GetComponent<Rigidbody>();
                }
                else 
                {
                    CJ.connectedBody = t.parent.gameObject.GetComponent<Rigidbody>();
                }


               
                //g.GetComponent<Rigidbody>().isKinematic = true;
                g.GetComponent<Rigidbody>().mass = 20f;
                g.GetComponent<Rigidbody>().drag = 5f;
                g.GetComponent<Rigidbody>().angularDrag = 1f;
            }

        }




    }

    void Rig2()
    {
        Transform[] allChildren = GetComponentsInChildren<Transform>();
        foreach (Transform t in allChildren)
        {
            GameObject g = t.gameObject;

            if (g.name.Contains("Hips") || g.name.Contains("Head") && g.name.Contains("HeadTop") == false && g.name.Contains("HeadJoint") == false || g.name.Contains("Foot") ||
                g.name.Contains("RightHand") && g.name.Contains("1") == false && g.name.Contains("2") == false && g.name.Contains("3") == false 
                || g.name.Contains("LeftHand") && g.name.Contains("1") == false && g.name.Contains("2") == false && g.name.Contains("3") == false 
                || g.name.Contains("Arm") || g.name.Contains("Shoulder") ||
                g.name.Contains("Spine2") || g.name.Contains("Leg") || g.name.Contains("Neck"))
            {
                if (g.name.Contains("Neck") == false)
                {
                    SphereCollider C = g.AddComponent<SphereCollider>();
                    // C.size = new Vector3(0.005f, 0.005f, 0.005f);
                    //  C.height = 0.001f;
                    C.radius = SRadius;
                }
                if (g.name.Contains("Hips") == false)
                {
                    CharacterJoint CJ = g.AddComponent<CharacterJoint>();
                    // CJ.connectedBody = Rig;
                    SoftJointLimit jointLimit = CJ.lowTwistLimit;
                    jointLimit.limit = -5f;
                    CJ.lowTwistLimit = jointLimit;

                    SoftJointLimit jointLimitH = CJ.highTwistLimit;
                    jointLimitH.limit = 25f;
                    CJ.highTwistLimit = jointLimitH;

                    SoftJointLimit jointLimit1 = CJ.swing1Limit;
                    jointLimit1.limit = 25f;
                    CJ.swing1Limit = jointLimit1;

                    SoftJointLimit jointLimit2 = CJ.swing2Limit;
                    jointLimit2.limit = 25f;
                    CJ.swing2Limit = jointLimit2;
                }
                else
                {
                    g.AddComponent<Rigidbody>();
                    g.GetComponent<Rigidbody>().mass = 20f;
                    g.GetComponent<Rigidbody>().drag = 5f;
                    g.GetComponent<Rigidbody>().angularDrag = 1f;
                }

                g.layer = DollID; //g.layer = LayerMask.NameToLayer("RagBox");







            }

        }

        foreach (Transform t in allChildren)
        {
            GameObject g = t.gameObject;
            string name = g.name;
            if (g.name.Contains("Hips") || g.name.Contains("Head") && g.name.Contains("HeadTop") == false && g.name.Contains("HeadJoint") == false || g.name.Contains("Foot") ||
               g.name.Contains("RightHand") && g.name.Contains("1") == false && g.name.Contains("2") == false && g.name.Contains("3") == false
               || g.name.Contains("LeftHand") && g.name.Contains("1") == false && g.name.Contains("2") == false && g.name.Contains("3") == false
               || g.name.Contains("Arm") || g.name.Contains("Shoulder") ||
               g.name.Contains("Spine2") || g.name.Contains("Leg") || g.name.Contains("Neck"))
            {


                CharacterJoint CJ = g.GetComponent<CharacterJoint>();
                if (CJ != null)
                {
                    if (t.parent.gameObject.GetComponent<Rigidbody>()!=null)
                    {
                        CJ.connectedBody = t.parent.gameObject.GetComponent<Rigidbody>();
                    }
                    else if (t.parent.parent.gameObject.GetComponent<Rigidbody>() != null)
                    {
                        CJ.connectedBody = t.parent.parent.gameObject.GetComponent<Rigidbody>();
                    }
                    else if (t.parent.parent.parent.gameObject.GetComponent<Rigidbody>() != null)
                    {
                        CJ.connectedBody = t.parent.parent.parent.gameObject.GetComponent<Rigidbody>();
                    }


                }
                
                


                if(Debug == true)
                {
                    g.GetComponent<Rigidbody>().isKinematic = true;
                }

                g.GetComponent<Rigidbody>().mass = 20f;
                g.GetComponent<Rigidbody>().drag = 5f;
                g.GetComponent<Rigidbody>().angularDrag = 1f;
            }

        }




    }

    public void RagOff()
    {
     
        if (RagStarted == true)
        {
            RagStarted = false;
            if (DisabledKinematic == true)
            {
                Rig.isKinematic = true;
            }
           

            Transform[] allChildren = GetComponentsInChildren<Transform>();
            foreach (Transform t in allChildren)
            {
                GameObject g = t.gameObject;
                if (g.name.Contains("Pelvis")|| g.name.Contains("Hip"))
                {
                    CapsuleCollider Cp = g.GetComponent<CapsuleCollider>();
                    Rigidbody Rp = g.GetComponent<Rigidbody>();
                    Destroy(Cp);
                    Destroy(Rp);
                    g.transform.localPosition = new Vector3(0, 0, 0);

                }
                if (RigType == 0)
                {
                    if (g.name.Contains("Head") || g.name.Contains("R Foot") || g.name.Contains("L Foot") ||
                     g.name.Contains("Calf") || g.name.Contains("Arm") || g.name.Contains("Clav") ||
                     g.name.Contains("Spine") || g.name.Contains("Thigh") || g.name.Contains("Neck"))
                    {
                        SphereCollider C = g.GetComponent<SphereCollider>();
                        Destroy(C);
                        CharacterJoint CJ = g.GetComponent<CharacterJoint>();
                        Destroy(CJ);
                        Rigidbody grig = g.GetComponent<Rigidbody>();
                        Destroy(grig);
                        g.layer = LayerMask.NameToLayer("Default");
                    }
                }
                else if (RigType == 1)
                {
                    if (g.name.Contains("Rib") || g.name.Contains("Skull") || g.name.Contains("Foot") ||
                g.name.Contains("Hand") || g.name.Contains("Arm") && g.name.Contains("Fix") == false ||
                g.name.Contains("Spinne") || g.name.Contains("Pelvis") || g.name.Contains("Leg") || g.name.Contains("Neck"))
                    {
                        SphereCollider C = g.GetComponent<SphereCollider>();
                        Destroy(C);
                        CharacterJoint CJ = g.GetComponent<CharacterJoint>();
                        Destroy(CJ);
                        Rigidbody grig = g.GetComponent<Rigidbody>();
                        Destroy(grig);
                        g.layer = LayerMask.NameToLayer("Default");
                    }
                }
                else if (RigType == 2)
                {
                    if (g.name.Contains("Hips") || g.name.Contains("Head") && g.name.Contains("HeadTop") == false && g.name.Contains("HeadJoint") == false || g.name.Contains("Foot") ||
                 g.name.Contains("RightHand") && g.name.Contains("1") == false && g.name.Contains("2") == false && g.name.Contains("3") == false
                 || g.name.Contains("LeftHand") && g.name.Contains("1") == false && g.name.Contains("2") == false && g.name.Contains("3") == false
                 || g.name.Contains("Arm") || g.name.Contains("Shoulder") ||
                 g.name.Contains("Spine2") || g.name.Contains("Leg") || g.name.Contains("Neck"))
                    {
                        SphereCollider C = g.GetComponent<SphereCollider>();
                        Destroy(C);
                        CharacterJoint CJ = g.GetComponent<CharacterJoint>();
                        Destroy(CJ);
                        Rigidbody grig = g.GetComponent<Rigidbody>();
                        Destroy(grig);
                        g.layer = LayerMask.NameToLayer("Default");
                    }
                }
               
            }
            CapsuleCollider BC = GetComponent<CapsuleCollider>();
           
           
            if (anim != null) { anim.enabled = true; }
            

            if (agent != null) { agent.enabled = true; }
                

            if (BC != null) { BC.enabled = true; }
                

            if (Rig != null) { Rig.useGravity = true; }
               

        }


    }



    protected virtual void StopAnimation(string AnimationName)
    {
        anim.SetBool(AnimationName, false);
    }

    protected virtual void PlayAnimation(string AnimationName, bool ovride)
    {
        if (ovride == false)
        {
            anim.SetBool(AnimationName, true);
        }
        else if (anim.GetBool(AnimationName) == false)
        {

            anim.SetBool(AnimationName, true);
        }

    }


}
