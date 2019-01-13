using System.Collections.Generic;
using System;
using System.Linq;

static class IListExtensions
{
	public static void Swap<T>(
		this IList<T> list,
		int firstIndex,
		int secondIndex
	)
	{
		if (list == null) throw new Exception("empty list" + list);
		if (firstIndex < 0 || firstIndex >= list.Count) throw new Exception("first index OutOfBounds: "+ firstIndex + list);
		if (secondIndex < 0 || secondIndex >= list.Count) throw new Exception("second index OutOfBounds: " + secondIndex + list);

		if (firstIndex == secondIndex)
		{
			return;
		}

		T temp = list[firstIndex];
		list[firstIndex] = list[secondIndex];
		list[secondIndex] = temp;
	}

	static string delimiter = " | ";
	static string startCap = " [ ";
	static string endCap = " ] ";
    public static string ListToString<T>(this IList<T> list, bool isNumbered = false, bool isNumberingFromOne = false)
	{
		System.Text.StringBuilder sb = new System.Text.StringBuilder();

		sb.Append(startCap);

		if (list == null)
		{
			sb.Append("--NULL List--");
		}
		else
			if (list.Count == 0)
		{
			sb.Append("--EMPTY List--");
		}
		else
		{
			for (int i = 0; i < list.Count; i++)
			{
				if (isNumbered)
				{
                    if(isNumberingFromOne)
                        sb.Append(i+1);
                    else
					    sb.Append(i);

					sb.Append(": ");
				}

				if (list[i] != null)
					sb.Append(list[i].ToString());
				else
					sb.Append("--NULL--");

				if (i < list.Count - 1)
					sb.Append(delimiter);
			}
		}

		sb.Append(endCap);
		return sb.ToString();
	}

    public static string[] ListToStringArray<T>(this IList<T> list, bool isNumbered = false, bool isNumberingFromOne = false)
    {
        System.Text.StringBuilder sb;

        List<string> strArray = new List<string>();
        //sb.Append(startCap);

        if (list == null)
        {
            strArray.Add("--NULL List--");
        }
        else
            if (list.Count == 0)
        {
            strArray.Add("--EMPTY List--");
        }
        else
        {
            for (int i = 0; i < list.Count; i++)
            {
                sb = new System.Text.StringBuilder();
                if (isNumbered)
                {
                    if (isNumberingFromOne)
                        sb.Append(i + 1);
                    else
                        sb.Append(i);

                    sb.Append(": ");
                }

                if (list[i] != null)
                {
                    sb.Append(list[i].ToString());
                    strArray.Add(sb.ToString());
                }
                else
                {
                    sb.Append("--NULL--");
                    strArray.Add(sb.ToString());
                }
            }
        }

        return strArray.ToArray();
    }

    public static T GetRandElementInList<T>(this IList<T> list, Random inRandom = null)
	{
        //If we are provided a random, use it.
        if (inRandom == null)
            inRandom = new Random();

        return list[inRandom.Next(list.Count)];
	}

	public static T GetRandElementInListAndRemoveRandElement<T>(this IList<T> list, Random inRandom = null)
	{
		if (list.Count == 0)
		{
#if UNITY_EDITOR
			UnityEngine.Debug.LogError("Trying to get random element from empty list");
#endif
			return default(T);
		}

        //If we are provided a random, use it.
        if (inRandom == null)
            inRandom = new Random();

        int elementPtr = inRandom.Next(0, list.Count);

		T element = list[elementPtr];

		list.RemoveAt(elementPtr);

		return element;
	}
    
    public static void Shuffle<T>(this IList<T> list, Random inRandom = null)
    {
        //If we are provided a random, use it.
        if (inRandom == null)
            inRandom = new Random();

        int n = list.Count;

        while (n > 1)
        {
            n--;
            int k = inRandom.Next(n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }

    public static List<UnityEngine.Vector3> DeepCopyWorldPositionsFromTransformList(this IList<UnityEngine.Transform> list)
    {
        List<UnityEngine.Vector3> vectorListCopy = new List<UnityEngine.Vector3>();

        foreach (UnityEngine.Transform trans in list)
        {
            vectorListCopy.Add(trans.position);
        }

        return vectorListCopy;
    }

    public static List<UnityEngine.Vector3> DeepCopyVector3List(this IList<UnityEngine.Vector3> list)
    {
        if (list.Count > 0)
        {
            List<UnityEngine.Vector3> vectorListCopy = new List<UnityEngine.Vector3>();

            foreach (UnityEngine.Vector3 vec in list)
            {
                UnityEngine.Vector3 newVec = vec;
                vectorListCopy.Add(newVec);
            }

            return vectorListCopy;
        }

        return null;
    }

    public static List<string> DeepCopyStringList(this IList<string> list)
	{
		if (list.Count > 0)
		{
			List<string> stringListCopy = new List<string>();

			foreach (string s in list)
			{
				stringListCopy.Add(string.Copy(s));
			}

			return stringListCopy;
		}

		return null;
	}

	public static List<int> DeepCopyIntList(this IList<int> list)
	{
		if (list.Count > 0)
		{
			List<int> intListCopy = new List<int>();

			foreach (int s in list)
			{
				intListCopy.Add(s);
			}

			return intListCopy;
		}

		return null;
	}

    public static int NumberOf<T>(this IList<T> list, Func<T,bool> inPredicate)
    {
        int countOfItems = 0;
        for (int i = 0; i < list.Count; i++)
        {
            if (inPredicate(list[i])) countOfItems++;
        }

        return countOfItems;
    }

    public static int NumberOf<T>(this IList<T> list, T inItemWeAreLookingToBeEqualsTo)
    {
        int countOfItems = 0;
        for (int i = 0; i < list.Count; i++)
        {
            if (inItemWeAreLookingToBeEqualsTo.Equals(list[i])) countOfItems++;
        }

        return countOfItems;
    }


    public static string ListToString<T>(this T[] list)
	{
		System.Text.StringBuilder sb = new System.Text.StringBuilder();
		
		sb.Append(startCap);
		
		if (list == null)
		{
			sb.Append("--Null List--");
		}
		else
			if (list.Length == 0)
		{
			sb.Append("--Empty List--");
		}
		else
		{
			for (int i = 0; i < list.Length; i++)
			{
                if(list[i] == null)
                    sb.Append("-null-");
                else
				    sb.Append(list[i].ToString());

				if (i < list.Length - 1)
					sb.Append(delimiter);
			}
		}

		sb.Append(endCap);
		return sb.ToString();
	}

    /// <summary>
    /// Sets list to default. null
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="list"></param>
    public static void Clear<T>(this T[] list)
    {
        for (int i = 0; i < list.Length; i++)
        {
            list[i] = default(T);
        }
    }

    public static List<T> ShallowCopy<T>(this IList<T> list)
	{
		List<T> tempList = new List<T>();
		foreach(T temp in list)
		{
			tempList.Add(temp);
		}

		return tempList;
	}

	public static List<T> ShallowCopyIgnoring<T>(this IList<T> list, T inIgnoreThisElement)
	{
		List<T> tempList = new List<T>();
		foreach (T temp in list)
		{
			if(!temp.Equals(inIgnoreThisElement))
				tempList.Add(temp);
		}

		return tempList;
	}

	public static IList<T> Clone<T>(this IList<T> inList) where T : ICloneable
	{
		return inList.Select(item => (T)item.Clone()).ToList();
	}

    public static float AddedTotal(this IList<float> list)
    {
        float total = 0;

        foreach (float s in list)
        {
            total += s;
        }

        return total;
    }

    public static int AddedTotal(this IList<int> list)
    {
        int total = 0;
        foreach (int s in list)
        {
            total += s;
        }

        return total;
    }
}