using System.Security.Cryptography;

public static class PasswordGenerator
{
    public static string Genrate_Temporary_Password()
    {
        const string upper = "ABCDEFGHJKLMNPQRSTUVWXYZ";
        const string lower = "abcdefghijkmnopqrstuvwxyz";
        const string digits = "23456789";
        const string special = "!@#$%";

        var password = new List<char>
        {
            upper[RandomNumberGenerator.GetInt32(upper.Length)],
            lower[RandomNumberGenerator.GetInt32(lower.Length)],
            digits[RandomNumberGenerator.GetInt32(digits.Length)],
            special[RandomNumberGenerator.GetInt32(special.Length)]
        };

        const string all = upper + lower + digits + special;

        while (password.Count < 10)
        {
            password.Add(
                all[RandomNumberGenerator.GetInt32(all.Length)]
            );
        }
        for (int i = password.Count - 1; i > 0; i--)
        {
            int j = RandomNumberGenerator.GetInt32(i + 1);

            (password[i], password[j]) =
                (password[j], password[i]);
        }

        return new string(password.ToArray());
    }
}