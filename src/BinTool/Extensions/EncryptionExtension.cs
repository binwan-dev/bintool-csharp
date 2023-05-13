namespace System;

public static class EncryptionExtension
{
    public static string SHA1Encrypt(this string? payload, Encoding? coding = null)
    {
        if (string.IsNullOrWhiteSpace(payload))
        {
            return string.Empty;
        }
        coding = coding ?? Encoding.UTF8;

        var encryptData = SHA1.HashData(payload.ToBytes(coding));
        return encryptData.ToStr(coding);
    }
}
