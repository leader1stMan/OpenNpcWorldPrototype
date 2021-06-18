using UnityEngine;

public class Health : MonoBehaviour
{
    public float MaxHealth = 100;
    public float CurrentHealth;

    public bool ShowDebugMessages = true;
    
    //To be called from enemy script
    public void Damage(float amt)
    {
        if (CurrentHealth > amt)
        {
            CurrentHealth -= amt;
            if(ShowDebugMessages)
                Debug.Log("Health of " + transform.name + " is " + CurrentHealth);
        }
        else
        {
            if(ShowDebugMessages)
                Debug.Log(transform.name + " is Dead");

            if (gameObject.tag == "Player" || gameObject.tag == "Enemy")
            {
                transform.position = new Vector3(0, 1, -20);
                Physics.SyncTransforms();
                if (ShowDebugMessages)
                    Debug.Log(gameObject.name + " is respawned!!");
                CurrentHealth = MaxHealth;
            }
            else
            {
                Destroy(gameObject);
            }
            
        }
    }
}
