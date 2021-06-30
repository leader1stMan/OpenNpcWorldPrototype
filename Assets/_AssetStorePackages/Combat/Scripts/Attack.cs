
public class Attack
{
    private readonly int _damage;
    private readonly bool _critical;

    public Attack(int dam, bool crit)
    {
        _damage = dam;
        _critical = crit;
    }

    public int Damage
    {
        get { return _damage; }
    }

    public bool isCritical
    {
        get { return _critical; }
    }
}
