using UnityEngine;

public static class Collider2DExtensions{

    public static int PushRigidbodiesAway(
        this CircleCollider2D inMyCircleCollider,
        float inForceToPushWith,
        Collider2D[] inWantedResults,
        int inLayerMaskOfObjectsToPushAway,
        bool inIsPushHarderIfCloser = true,
        float inMinDepthOfObjectsToCheck = -Mathf.Infinity,
        float inMaxDepthOfObjectsToCheck = Mathf.Infinity
        )
    {
        Vector2 myCollider2DPosition = inMyCircleCollider.offset + (Vector2)inMyCircleCollider.transform.position;

        for (int i = 0; i < inWantedResults.Length; i++)
        {
            inWantedResults[i] = null;
        }

        int numOfColl =
        Physics2D.OverlapCircleNonAlloc(
                myCollider2DPosition, inMyCircleCollider.radius, inWantedResults,
                inLayerMaskOfObjectsToPushAway, inMinDepthOfObjectsToCheck, inMaxDepthOfObjectsToCheck);
        
        //For each collider we touch, push them away
        for (int i = 0; i < inWantedResults.Length; i++)
        {
            Collider2D obj = inWantedResults[i];

            if (obj == inMyCircleCollider || obj == null) continue;

            Rigidbody2D otherRGB = obj.GetComponent<Rigidbody2D>();

            if (otherRGB == null) continue;

            Vector2 pushVector = (otherRGB.position - myCollider2DPosition);

            //If we push harder the closer it is, we multiply by the inverse distance
            if (inIsPushHarderIfCloser)
                pushVector *= 1 / pushVector.sqrMagnitude;
            else
                pushVector.Normalize();

            otherRGB.AddForce(inForceToPushWith * pushVector, ForceMode2D.Force);

            //myCollider2DPosition.DebugDrawCross();
            //otherRGB.position.DebugDrawCross();
            //Debug.DrawRay(myCollider2DPosition, pushVector, Color.magenta, 0.1f);
        }

        return numOfColl;
    }

    public static int PushRigidbodiesAway(
        this BoxCollider2D inMyBoxCollider,
        float inForceToPushWith,
        Collider2D[] inWantedResults,
        int inLayerMaskOfObjectsToPushAway,
        bool inIsPushHarderIfCloser = true,
        float inMinDepthOfObjectsToCheck = -Mathf.Infinity,
        float inMaxDepthOfObjectsToCheck = Mathf.Infinity
        )
    {

        Vector2 myCollider2DPosition = inMyBoxCollider.offset + (Vector2)inMyBoxCollider.transform.position;

        for (int i = 0; i < inWantedResults.Length; i++)
        {
            inWantedResults[i] = null;
        }

        int numOfColl =
            Physics2D.OverlapBoxNonAlloc(
                myCollider2DPosition, inMyBoxCollider.size, inMyBoxCollider.transform.rotation.z,inWantedResults,
                inLayerMaskOfObjectsToPushAway, inMinDepthOfObjectsToCheck, inMaxDepthOfObjectsToCheck);

        //For each collider we touch, push them away
        for (int i=0; i<inWantedResults.Length; i++)
        {
            Collider2D obj = inWantedResults[i];

            if (obj == inMyBoxCollider || obj == null) continue;

            Rigidbody2D otherRGB = obj.GetComponent<Rigidbody2D>();

            if (otherRGB == null) continue;

            Vector2 pushVector = (otherRGB.position - myCollider2DPosition);

            //If we push harder the closer it is, we multiply by the inverse distance
            if (inIsPushHarderIfCloser)
                pushVector *= 1/pushVector.sqrMagnitude;
            else
                pushVector.Normalize();

            otherRGB.AddForce(inForceToPushWith * pushVector, ForceMode2D.Force);

            //myCollider2DPosition.DebugDrawCross();
            //otherRGB.position.DebugDrawCross();
            //Debug.DrawRay(myCollider2DPosition, pushVector, Color.magenta, 0.1f);
        }

        return numOfColl;
    }

}
