using UnityEngine;

public class AllyAgent : SteeringAgent
{
	private Attack.AttackType attackType = Attack.AttackType.AllyGun;

	public AllyAgentRole agentRole { get; private set; } = AllyAgentRole.GroupLeader;


	public GroupLeader groupLeader { get; private set; }
	public ScoutLeader scoutLeader { get; private set; }
	public FollowLeader followLeader { get; private set; }
	public Idle idle { get; private set; }

    protected override void InitialiseFromAwake()
	{
		
	}


    protected override void CooperativeArbitration()
	{
		base.CooperativeArbitration();

        if ((agentRole == AllyAgentRole.GroupLeader && groupLeader.canShoot)
			|| (agentRole == AllyAgentRole.FollowLeader && followLeader.canShoot))
		{
			AttackWith(attackType);
			if(attackType == Attack.AttackType.Rocket)
			{
				if (agentRole == AllyAgentRole.GroupLeader)
				{
					groupLeader.shootRocket = false;
					groupLeader.atShootPosition = false;
					attackType = Attack.AttackType.AllyGun;
                }
            }
        }
	}

	protected override void UpdateDirection()
	{
        base.UpdateDirection();
	}


	public void SwitchAgentRole(AllyAgentRole role, AllyAgent leader)
	{
		agentRole = role;

        groupLeader.enabled = false;
        followLeader.enabled = false;
        scoutLeader.enabled = false;
        idle.enabled = false;
		groupLeader.atShootPosition = false;
        followLeader.atShootPosition = false;

        switch (agentRole)
		{
			case AllyAgentRole.GroupLeader:
				groupLeader.enabled = true;
                break;
			case AllyAgentRole.FollowLeader:
                followLeader.enabled = true;
                followLeader.SetLeader(leader);
                break;
			case AllyAgentRole.Idle:
				idle.enabled = true;
                break;
            case AllyAgentRole.LeadScout:
                scoutLeader.enabled = true;
                break;
        }
        
    }




	public void AddAllComponents()
	{
        gameObject.AddComponent<GroupLeader>();
        gameObject.AddComponent<ScoutLeader>();
        gameObject.AddComponent<FollowLeader>();
        gameObject.AddComponent<Idle>();

		groupLeader = GetComponent<GroupLeader>();
		scoutLeader = GetComponent<ScoutLeader>();
		followLeader = GetComponent<FollowLeader>();
		idle = GetComponent<Idle>();

        groupLeader.SetAllyAgent(this);
        followLeader.SetAllyAgent(this);
    }



	public void SetAttackType(Attack.AttackType type)
	{
		attackType = type;
    }

}
