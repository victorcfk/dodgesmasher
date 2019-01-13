static class RandomExtensions
{
    public static int NextExcluding(this System.Random inRandom, int inMin, int inMax, int inExclude)
    {
        //The exclusion is past our boundaries
        if (inExclude < inMin || inExclude >= inMax)
            return inRandom.Next(inMin, inMax);
        else
        {
            if (inExclude == inMin)
                return inRandom.Next(inMin + 1, inMax);
            else
                 if (inExclude == inMax - 1)
                return inRandom.Next(inMin, inMax - 1);
            else
            {
                //50% chance to get either end
                if (inRandom.Next(2) == 0)
                    return inRandom.Next(inMin, inExclude);
                else
                    return inRandom.Next(inExclude + 1, inMax);
            }

        }
    }
}