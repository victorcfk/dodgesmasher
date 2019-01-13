#if UNITY_ANDROID || UNITY_IPHONE || UNITY_STANDALONE_OSX || UNITY_TVOS
// WARNING: Do not modify! Generated file.

namespace UnityEngine.Purchasing.Security {
    public class GooglePlayTangle
    {
        private static byte[] data = System.Convert.FromBase64String("ZjLzqJ4iUqQMGrtZfxTSb5ShFa8FoYZjrE2sSEmZkMYCei7jbrLtQBJdY3g9/nqPKMFhqLaMTPsH2yMxEL4ORRQe4KG0wls12yAuWl1CBH8RoxqDQrpp9B5PPrceEOdwza2yUJbP0Qx+xyBQ7mim6Pym75i9EhhmN7jIKB4XIwK6r2k9/gt2XSLWZbxYGxslF5wVHmb+Woz6LczOjJRfWyirWNEW+Qvbya9fsGwEb7e/dzqZskrR3RUzSWYyP67gZHLozV+mR70LP5Hx1XExes8BsFeLzrl8CRfDBBKgIwASLyQrCKRqpNUvIyMjJyIhS8Vh9m7K1FOssmphkbfWpVpUKCmgIy0iEqAjKCCgIyMijCl36iPlIv1tQAoeSPdSUSAhIyIj");
        private static int[] order = new int[] { 11,4,6,9,8,10,12,7,9,12,13,11,13,13,14 };
        private static int key = 34;

        public static readonly bool IsPopulated = true;

        public static byte[] Data() {
        	if (IsPopulated == false)
        		return null;
            return Obfuscator.DeObfuscate(data, order, key);
        }
    }
}
#endif
