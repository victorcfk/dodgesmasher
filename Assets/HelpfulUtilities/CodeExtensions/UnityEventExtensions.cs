namespace UnityEngine.Events
{
    public static class UnityEventExtensions
    {
        public static string ListOfAllMethodsToString(this UnityEvent inUEvent)
        {
            string temp = string.Empty;
            int totalEventCount = inUEvent.GetPersistentEventCount();
            //if(totalEventCount > 0)
            //{
            //    temp = "No events";
            //}
            //else
            for (int i =0; i< totalEventCount; i++)
            {
                temp += inUEvent.GetPersistentTarget(i) +"."+ inUEvent.GetPersistentMethodName(i) +"\n";
            }
            
            return temp;
        }

        public static string ListOfAllMethodsToString<T>(this UnityEvent<T> inUEvent)
        {
            string temp = string.Empty;
            int totalEventCount = inUEvent.GetPersistentEventCount();
            //if(totalEventCount > 0)
            //{
            //    temp = "No events";
            //}
            //else
            for (int i = 0; i < totalEventCount; i++)
            {
                temp += inUEvent.GetPersistentTarget(i) + "." + inUEvent.GetPersistentMethodName(i) + "\n";
            }

            return temp;
        }
		
		public static string UnityEventsToString(this UnityEvent inUEvent)
		{
			int eventCount = inUEvent.GetPersistentEventCount();
			string temp = string.Empty;

			if (eventCount > 0)
			{
				for (int i = 0; i < eventCount; i++)
				{
					temp += inUEvent.GetPersistentTarget(i).name+"["+inUEvent.GetPersistentTarget(i).GetType().ToString() + "." + inUEvent.GetPersistentMethodName(i).ToString() + "]\n";
				}
			}
			//else
			//{
				
			//}

			return temp;
		}
		
		public static void DebugListOfAllMethodsToString(this UnityEvent inUEvent)
        {
            Debug.Log(ListOfAllMethodsToString(inUEvent));
        }
    }
}