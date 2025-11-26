using UnityEngine;

public class AllyAgent : SteeringAgent
{
	private Attack.AttackType attackType = Attack.AttackType.AllyGun;

	public AllyAgentRole agentRole { get; private set; } = AllyAgentRole.Soldier;

	private bool shouldAllyAttack = false;

	protected override void InitialiseFromAwake()
	{
		
	}


    protected override void CooperativeArbitration()
	{
		base.CooperativeArbitration();

		if(shouldAllyAttack)
		{
			AttackWith(attackType);
		}
	}

	protected override void UpdateDirection()
	{
		switch(agentRole)
		{
			case AllyAgentRole.Soldier:
                if (GetComponent<RunToLocatedEnemy>().enabled)
                {
                    base.UpdateDirection();
                }
                break;
			case AllyAgentRole.LeadScout:
                if (GetComponent<ScoutLeader>().enabled)
                {
                    base.UpdateDirection();
                }
                break;
            case AllyAgentRole.FollowerScout:
                if (GetComponent<ScoutFollow>().enabled)
                {
                    base.UpdateDirection();
                }
                break;
		}
	}


	public void SetAgentRole(AllyAgentRole role)
	{
		agentRole = role;
	}


    public void StartAttacking()
    {
		shouldAllyAttack = true;
    }



}
