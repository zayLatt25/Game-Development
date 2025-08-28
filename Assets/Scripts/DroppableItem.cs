using UnityEngine;

public class DroppableItem : MonoBehaviour
{
    public enum Type
    {
        Vaccine = 0,
        SubMachineGun = 1,
        Ak47 = 2,
        Knife = 3,
        Axe = 4,
    }
    
    [SerializeField] private Type _itemType;
    [SerializeField] private float _pickupRange = 1.5f;
    
    public Type ItemType => _itemType;
    public float PickupRange => _pickupRange;
}