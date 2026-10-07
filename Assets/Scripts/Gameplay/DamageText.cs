using TMPro;
using UnityEngine;

public class DamageText : EruBehaviour
{
    private Transform target;
    private Camera viewCamera;
    private RectTransform rect;
    [SerializeField] private Color physicalDamageColor = Color.red;
    [SerializeField] private Color magicDamageColor = Color.blue;
    [SerializeField] private Color pureDamageColor = Color.white;
    [SerializeField] private TextMeshProUGUI text;
    private bool initialized;

    public void Initialize(DamageShortInfo damage, Transform target, Camera view)
    {
        switch (damage.damageType)
        {
            case DamageType.Physical:
                text.color = physicalDamageColor;
                break;

            case DamageType.Magical:
                text.color = magicDamageColor;
                break;

            case DamageType.Pure:
                text.color = pureDamageColor;
                break;
        }
        text.text = $"{Mathf.Ceil(damage.damageAmount)}{(damage.critCoefficient > 1 ? "!!!" : "")}";
        this.target = target;
        viewCamera = view;
        rect = GetComponent<RectTransform>();
        rect.anchoredPosition = viewCamera.WorldToScreenPoint(target.position);
        initialized = true;
        Destroy(gameObject, 1f);
    }

    public override void OnTick(float deltaInterval)
    {
        if (initialized == false)
            return;
        rect.anchoredPosition = viewCamera.WorldToScreenPoint(target.position);
    }
}
public struct DamageShortInfo
{
    public float damageAmount;
    public float critCoefficient;
    public DamageType damageType;

    public DamageShortInfo(float damageAmount, float critCoefficient, DamageType damageType)
    {
        this.damageAmount = damageAmount;
        this.critCoefficient = critCoefficient;
        this.damageType = damageType;
    }
}