using UnityEngine;

[System.Serializable]
public class SingleUnityLayer
{
	[SerializeField]
	private int m_LayerIndex = 0;
	public int LayerIndex
	{
		get { return m_LayerIndex; }
	}

	public SingleUnityLayer(int layerIndex)
	{
		Set(layerIndex);
	}

	public void Set(int layerIndex)
	{
		if (layerIndex > 0 && layerIndex < 32)
		{
			m_LayerIndex = layerIndex;
		}
	}

	public int Mask
	{
		get { return 1 << m_LayerIndex; }
	}
}
