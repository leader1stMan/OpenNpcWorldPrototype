using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;
using UnityEditor;
using System.Collections.Generic;

public class RagdollSystem : MonoBehaviour
{

    public float ragdollCooldown = 5f;
    public float ragdollCooldownMax = 5f;
    public Rigidbody Rig;
    public int RigType = 0;
    public CharacterStats stats;
    public NavMeshAgent agent;
    public Animator anim;
    public float SRadius = 0.0008f;
    bool OverRode = false;
    public bool Debug = false;

    void Start()
    {
        Rig = GetComponent<Rigidbody>();
        agent = GetComponent<NavMeshAgent>();
        stats = GetComponent<CharacterStats>();
        anim = GetComponentInChildren<Animator>();
        if (gameObject.name.Contains("Player"))
        {
            RigType = 1;
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







    void Update()
    {
        if (OverRode == false)
        {
            if (stats.isRagdolled == true)
            {
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

            PlayAnimation("isRagdolled", false);
            RagEffect();
            ragdollCooldown -= Time.deltaTime;
        }
        else
        {
            stats.isRagdolled = false;
            ragdollCooldown = ragdollCooldownMax;
            
          //  ChangeState(EnemyState.GetUp);
        }


    }


   

    public void DoGetUp()
    {
        RagOff();
        
    }


    public void RagEffect()
    {
       
        if (Rig.isKinematic == true)
        {
            Rig.isKinematic = false;
            Rig.useGravity = false;
            CapsuleCollider BC = GetComponent<CapsuleCollider>();
            //NavMeshAgent nma = GetComponent<NavMeshAgent>();
            anim.enabled = false;
            agent.enabled = false;
            //nma.enabled = false;
            BC.enabled = false;
          






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
                jointLimit1.limit = 5f;
                CJ.swing1Limit = jointLimit1;

                SoftJointLimit jointLimit2 = CJ.swing2Limit;
                jointLimit2.limit = 5f;
                CJ.swing2Limit = jointLimit2;





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
                    jointLimit1.limit = 5f;
                    CJ.swing1Limit = jointLimit1;

                    SoftJointLimit jointLimit2 = CJ.swing2Limit;
                    jointLimit2.limit = 5f;
                    CJ.swing2Limit = jointLimit2;
                }
                else
                {
                    g.AddComponent<Rigidbody>();
                    g.GetComponent<Rigidbody>().mass = 20f;
                    g.GetComponent<Rigidbody>().drag = 5f;
                    g.GetComponent<Rigidbody>().angularDrag = 1f;
                }
              
                
                   
                





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

            if (g.name.Contains("Hips") || g.name.Contains("Head") && g.name.Contains("HeadTop") == false || g.name.Contains("Foot") ||
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
                    jointLimit1.limit = 5f;
                    CJ.swing1Limit = jointLimit1;

                    SoftJointLimit jointLimit2 = CJ.swing2Limit;
                    jointLimit2.limit = 5f;
                    CJ.swing2Limit = jointLimit2;
                }
                else
                {
                    g.AddComponent<Rigidbody>();
                    g.GetComponent<Rigidbody>().mass = 20f;
                    g.GetComponent<Rigidbody>().drag = 5f;
                    g.GetComponent<Rigidbody>().angularDrag = 1f;
                }

                g.layer = LayerMask.NameToLayer("RagBox");







            }

        }

        foreach (Transform t in allChildren)
        {
            GameObject g = t.gameObject;
            string name = g.name;
            if (g.name.Contains("Hips") || g.name.Contains("Head") && g.name.Contains("HeadTop") == false || g.name.Contains("Foot") ||
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
     
        if (Rig.isKinematic == false)
        {
            Rig.isKinematic = true;

            Transform[] allChildren = GetComponentsInChildren<Transform>();
            foreach (Transform t in allChildren)
            {
                GameObject g = t.gameObject;
                if (g.name.Contains("Pelvis"))
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
                    }
                }
                else if (RigType == 2)
                {
                    if (g.name.Contains("Hips") || g.name.Contains("Head") && g.name.Contains("HeadTop") == false || g.name.Contains("Foot") ||
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
                    }
                }
                g.layer = LayerMask.NameToLayer("Default");
            }


            CapsuleCollider BC = GetComponent<CapsuleCollider>();
            ///  NavMeshAgent nma = GetComponent<NavMeshAgent>();
            //Animator a = GetComponent<Animator>();
            anim.enabled = true;
            agent.enabled = true;
            BC.enabled = true;
            Rig.useGravity = true;

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
