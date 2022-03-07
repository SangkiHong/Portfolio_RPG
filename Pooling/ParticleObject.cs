using UnityEngine;

public class ParticleObject : PoolObject
{
	[SerializeField]
	private bool isOnShot;
	[SerializeField]
	private float lifetime;

	public override void OnAwake()
	{
		if (isOnShot) Done(GetComponent<ParticleSystem>().main.duration);
		else Done(lifetime);
	}
}
