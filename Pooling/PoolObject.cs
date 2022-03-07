using UnityEngine;
using DG.Tweening;

public class PoolObject : MonoBehaviour 
{
	public string poolKey;

	public virtual void OnAwake() { }

	protected void Done(float time) 
	{
		DOVirtual.DelayedCall(time, () => { if (gameObject.activeInHierarchy) Done(); });
	}

	protected void Done(bool isUI = false) 
	{
		gameObject.transform.DOKill();

		if (!isUI) PoolManager.instance.ReturnObjectToQueue(gameObject, this);
		else UIPoolManager.instance.ReturnObjectToQueue(gameObject, this);
	}
}
