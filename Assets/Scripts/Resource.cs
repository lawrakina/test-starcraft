using UnityEngine;

public class Resource : MonoBehaviour
{
    [SerializeField] private int minCharges = 10;
    [SerializeField] private int maxCharges = 50;
    
    private int currentCharges;
    public bool IsAvailable => currentCharges > 0;
    public Vector3 Position => transform.position;

    private void Start()
    {
        currentCharges = Random.Range(minCharges, maxCharges + 1);
    }

    public bool Collect(int amount = 1)
    {
        if (currentCharges <= 0)
            return false;

        currentCharges -= amount;
        
        if (currentCharges <= 0)
        {
            OnDepleted();
        }
        
        return true;
    }

    private void OnDepleted()
    {
        gameObject.SetActive(false);
    }

    public int GetCharges()
    {
        return currentCharges;
    }
}
