namespace System;

public static class EncryptionExtension
{
    public static string SHA1Encrypt(this string? payload, Encoding? coding = null)
    {
        if (string.IsNullOrWhiteSpace(payload))
        {
            return string.Empty;
        }

        if (coding == null)
        {
            coding = Encoding.UTF8;
        }

        var sha1 = SHA1.Create();
        var originalPwd = coding.GetBytes(payload);
        var encryPwd = sha1.ComputeHash(originalPwd);
        return string.Join("", encryPwd.Select(b => string.Format("{0:x2}", b)).ToArray()).ToUpper();
    }
}
