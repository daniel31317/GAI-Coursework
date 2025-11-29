using UnityEngine;

public class AllyAgent : SteeringAgent
{
	private Attack.AttackType attackType = Attack.AttackType.AllyGun;

	public AllyAgentRole agentRole { get; private set; } = AllyAgentRole.GroupLeader;


	public GroupLeader groupLeader { get; private set; }
	public GroupMember groupMember { get; private set; }
	public ScoutLeader scoutLeader { get; private set; }
	public ScoutFollow scoutFollow { get; private set; }
	public Idle idle { get; private set; }

    protected override void InitialiseFromAwake()
	{
		
	}


    protected override void CooperativeArbitration()
	{
		base.CooperativeArbitration();

		if((agentRole == AllyAgentRole.GroupLeader && groupLeader.atShootPosition)
			|| (agentRole == AllyAgentRole.GroupMember && groupMember.atShootPosition))
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

        groupLeader.enabled = false;
        groupMember.enabled = false;
        scoutLeader.enabled = false;
        scoutFollow.enabled = false;
        idle.enabled = false;
		groupLeader.atShootPosition = false;
		groupMember.atShootPosition = false;

        switch (agentRole)
		{
			case AllyAgentRole.GroupLeader:
				groupLeader.enabled = true;
                break;
			case AllyAgentRole.GroupMember:
				groupMember.enabled = true;
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
        gameObject.AddComponent<GroupLeader>();
        gameObject.AddComponent<GroupMember>();
        gameObject.AddComponent<ScoutLeader>();
        gameObject.AddComponent<ScoutFollow>();
        gameObject.AddComponent<Idle>();

		groupLeader = GetComponent<GroupLeader>();
		groupMember = GetComponent<GroupMember>();
		scoutLeader = GetComponent<ScoutLeader>();
		scoutFollow = GetComponent<ScoutFollow>();
		idle = GetComponent<Idle>();
    }

}
