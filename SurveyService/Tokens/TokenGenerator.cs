// Copyright (c) Microsoft. All rights reserved. Licensed under the MIT license. See LICENSE.txt in the project root for license information.
using System;
using System.Security.Cryptography;

namespace SurveyService.Tokens
{
    public static class TokenGenerator
    {
        private const int SaltSize = 32;
        private const int TokenSize = 32;
        private const int Iterations = 1000;

        public static string GenerateNewTokenSalt()
        {
            var rng = new RNGCryptoServiceProvider();
            byte[] randomBytes = new byte[SaltSize];
            rng.GetBytes(randomBytes);
            return Convert.ToBase64String(randomBytes);
        }

        public static string GenerateLimitedToken(int surveyId, string email, string salt)
        {
            byte[] saltBytes = Convert.FromBase64String(salt);
            var pbkdf2 = new Rfc2898DeriveBytes(string.Format("{0}{1}", surveyId, email), saltBytes, Iterations);
            byte[] hashBytes = pbkdf2.GetBytes(TokenSize);

            return Convert.ToBase64String(hashBytes);
        }

        public static bool VerifyLimitedToken(string token, int surveyId, string email, string salt)
        {
            return (token == GenerateLimitedToken(surveyId, email, salt));
        }
    }
}