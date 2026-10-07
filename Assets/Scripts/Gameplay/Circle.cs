using UnityEngine;
using UnityEngine.Rendering.Universal;

public class Circle : EruBehaviour
{
    [SerializeField] private Unit unit;
    [SerializeField] private Transform circleTransform;

    private CapsuleCollider unitCollider;
    private GameObject circleObject;
    private DecalProjector circleRenderer;
    void Awake()
    {
        unitCollider = unit.GetComponent<CapsuleCollider>();
        circleObject = circleTransform.gameObject;
        circleRenderer = circleObject.GetComponent<DecalProjector>();
        unit.SetCircle(this);

        circleRenderer.material = GameplayManager.GetUnitCircleMaterial(unit.Team, Player.ControllingTeam);
        SetActive(false);
    }

    public void SetActive(bool active)
    {
        if (circleObject == null)
            return;
        circleObject.SetActive(active);

        circleRenderer.material = GameplayManager.GetUnitCircleMaterial(unit.Team, Player.ControllingTeam);
    }

    public override void OnTick(float deltaInterval)
    {
        circleTransform.localScale = new Vector3(unitCollider.radius * 3, unitCollider.radius * 3, circleTransform.localScale.z);

    }
}
