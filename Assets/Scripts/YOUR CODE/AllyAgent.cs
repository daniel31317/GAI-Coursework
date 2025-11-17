using UnityEngine;

public class AllyAgent : SteeringAgent
{
	private Attack.AttackType attackType = Attack.AttackType.AllyGun;

	public AllyAgentRole agentRole { get; private set; } = AllyAgentRole.Soldier;

	protected override void InitialiseFromAwake()
	{
		
	}


    protected override void CooperativeArbitration()
	{
		base.CooperativeArbitration();

		if (Input.GetKeyDown(KeyCode.Alpha1))
		{
			attackType = Attack.AttackType.Melee;
		}
		if (Input.GetKeyDown(KeyCode.Alpha2))
		{
			attackType = Attack.AttackType.AllyGun;
		}
		if (Input.GetKeyDown(KeyCode.Alpha3))
		{
			attackType = Attack.AttackType.Rocket;
		}
		if (Input.GetKey(KeyCode.Space))
		{
			if(attackType == Attack.AttackType.Rocket && GameData.Instance.AllyRocketsAvailable <= 0)
			{
				attackType = Attack.AttackType.AllyGun;
			}

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
                if (GetComponent<ScoutWander>().enabled)
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



}
