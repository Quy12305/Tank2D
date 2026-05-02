using UnityEngine;

public class OrbitBladeDamageDealer : MonoBehaviour
{
    private PlayerOrbitBladeSkill owner;

    public void Initialize(PlayerOrbitBladeSkill orbitBladeSkill)
    {
        owner = orbitBladeSkill;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (owner == null)
        {
            return;
        }

        BotTank botTank = other.GetComponent<BotTank>();
        if (botTank != null)
        {
            owner.TryDamage(botTank);
        }
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        OnTriggerEnter2D(other);
    }
}
