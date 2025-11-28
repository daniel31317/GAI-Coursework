using UnityEngine;

public class AllyAgent : SteeringAgent
{
	private Attack.AttackType attackType = Attack.AttackType.AllyGun;

	public AllyAgentRole agentRole { get; private set; } = AllyAgentRole.Soldier;


	public RunToLocatedEnemy runToLocatedEnemy { get; private set; }
	public ScoutLeader scoutLeader { get; private set; }
	public ScoutFollow scoutFollow { get; private set; }
	public Idle idle { get; private set; }

    protected override void InitialiseFromAwake()
	{
		
	}


    protected override void CooperativeArbitration()
	{
		base.CooperativeArbitration();

		if(agentRole == AllyAgentRole.Soldier && runToLocatedEnemy.atShootPosition)
		{
			AttackWith(attackType);
		}
	}

	protected override void UpdateDirection()
	{
        base.UpdateDirection();
	}


	public void SwitchAgentRole(AllyAgentRole role)
	{
		agentRole = role;

        runToLocatedEnemy.enabled = false;
        scoutLeader.enabled = false;
        scoutFollow.enabled = false;
        idle.enabled = false;
		runToLocatedEnemy.atShootPosition = false;

        switch (agentRole)
		{
			case AllyAgentRole.Soldier:
				runToLocatedEnemy.enabled = true;
                break;
			case AllyAgentRole.OnBreak:
				idle.enabled = true;
                break;
            case AllyAgentRole.LeadScout:
                scoutLeader.enabled = true;
                break;
            case AllyAgentRole.FollowerScout:
                scoutFollow.enabled = true;
                break;
        }
        
    }




	public void AddAllComponents()
	{
        gameObject.AddComponent<RunToLocatedEnemy>();
        gameObject.AddComponent<ScoutLeader>();
        gameObject.AddComponent<ScoutFollow>();
        gameObject.AddComponent<Idle>();

		runToLocatedEnemy = GetComponent<RunToLocatedEnemy>();
		scoutLeader = GetComponent<ScoutLeader>();
		scoutFollow = GetComponent<ScoutFollow>();
		idle = GetComponent<Idle>();
    }

}
