using UnityEngine;

public class PoolObject : MonoBehaviour 
{
	public string poolKey;

	private const string _methodName = "DelayDone";

	public virtual void OnAwake() { }

	protected void Done(float time) => Invoke(_methodName, time);	

	private void DelayDone() { if (gameObject.activeInHierarchy) Done(); }

	protected void Done(bool isUI = false) 
	{

		if (!isUI) PoolManager.Instance.ReturnObjectToQueue(gameObject, this);
		else UIPoolManager.Instance.ReturnObjectToQueue(gameObject, this);
	}
}
