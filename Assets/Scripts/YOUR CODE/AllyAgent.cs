using UnityEngine;

public class AllyAgent : SteeringAgent
{
	private Attack.AttackType attackType = Attack.AttackType.AllyGun;

	private AllyAgentRole agentRole = AllyAgentRole.Default;

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
			case AllyAgentRole.Default:
                var mouseInWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                mouseInWorld.z = 0.0f;
                transform.up = Vector3.Normalize(mouseInWorld - transform.position);
                break;
			case AllyAgentRole.Scout:
                if (GetComponent<ScoutWander>().enabled)
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
