﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Security;
using System.Security.Cryptography;
using System.Runtime.InteropServices;
using System.Linq;
using System.IO;

namespace PWDTK_MOBILE_WP_8
{
    /// <summary>
    /// Password Toolkit Mobile created by Ian Harris for use with Windows Phone 8 or up
    /// This class facilitates creating crypto random salt and generating password hashes based on the PBKDF2 specification listed in RFC2898 and uses HMACSHA256 as the PRF
    /// Also includes optional password policy enforcement by regex
    /// harro84@yahoo.com.au
    /// v1.0.0.0
    /// </summary>
    public static class PWDTK
    {
        #region PWDTK Structs and Constants
        
        /// <summary>
        /// The default character length to create salt strings
        /// </summary>
        public const Int32 cDefaultSaltLength = 32;

        /// <summary>
        /// The default iteration count for key stretching
        /// </summary>
        public const Int32 cDefaultIterationCount = 1000;

        /// <summary>
        /// The minimum size in characters the password hashing function will allow for a salt string, salt must be always greater than 8 for PBKDF2 key derivitation to function
        /// </summary>
        public const Int32 cMinSaltLength = 8;

        /// <summary>
        /// The key length used in the PBKDF2 derive bytes and matches the output of the underlying HMACSHA512 psuedo random function
        /// </summary>
        public const Int32 cKeyLength = 32;

        /// <summary>
        /// A default password policy provided for use if you are unsure what to make your own PasswordPolicy
        /// </summary>
        public static PasswordPolicy cDefaultPasswordPolicy = new PasswordPolicy(1, 1, 2, 6, Int32.MaxValue);

        /// <summary>
        /// Below are regular expressions used for password to policy comparrisons
        /// </summary>
        private const String cNumbersRegex ="[\\d]";
        private const String cUppercaseRegex = "[A-Z]";
        private const String cNonAlphaNumericRegex = "[^0-9a-zA-Z]";

        /// <summary>
        /// A PasswordPolicy defines min and max password length and also minimum amount of Uppercase, Non-Alpanum and Numerics to be present in the password string
        /// </summary>
        public struct PasswordPolicy
        {
            private int aForceXUpperCase;            
            private int aForceXNonAlphaNumeric;
            private int aForceXNumeric;
            private int aPasswordMinLength;
            private int aPasswordMaxLength;

            /// <summary>
            /// Creates a new PasswordPolicy Struct
            /// </summary>
            /// <param name="XUpper">Forces at least this number of Uppercase characters</param>
            /// <param name="XNonAlphaNumeric">Forces at least this number of Special characters</param>
            /// <param name="XNumeric">Forces at least this number of Numeric characters</param>
            /// <param name="MinLength">Forces at least this number of characters</param>
            /// <param name="MaxLength">Forces at most this number of characters</param>
            public PasswordPolicy(int XUpper, int XNonAlphaNumeric, int XNumeric, int MinLength, int MaxLength)
            {
                aForceXUpperCase = XUpper;
                aForceXNonAlphaNumeric = XNonAlphaNumeric;
                aForceXNumeric = XNumeric;
                aPasswordMinLength = MinLength;
                aPasswordMaxLength = MaxLength;
            }

            public int ForceXUpperCase
            {
                get
                {
                    return aForceXUpperCase;
                }
            }

            public int ForceXNonAlphaNumeric
            {
                get
                {
                    return aForceXNonAlphaNumeric;
                }
            }

            public int ForceXNumeric
            {
                get
                {
                    return aForceXNumeric;
                }
            }

            public int PasswordMinLength
            {
                get
                {
                    return aPasswordMinLength;
                }
            }

            public int PasswordMaxLength
            {
                get
                {
                    return aPasswordMaxLength;
                }
            }
        }

        #endregion

        #region PWDTK Public Methods

        /// <summary>
        /// Crypto Randomly generates a byte array that can be used safely as salt
        /// </summary>
        /// <param name="SaltStringLength">Length of the generated byte array</param>
        /// <returns>A Byte Array to be used as Salt</returns>
        public static Byte[] GetRandomSalt(Int32 SaltLength)
        {
            return pGenerateRandomSalt(SaltLength);
        }

        /// <summary>
        /// Crypto Randomly generates a Byte Array that can be used safely as salt
        /// </summary>
        /// <returns>A Byte Array to be used as Salt</returns>
        public static Byte[] GetRandomSalt()
        {
            return pGenerateRandomSalt(cDefaultSaltLength);
        }

        /// <summary>
        ///  Input a password and a hash value in bytes and it uses PBKDF2 HMACSHA256 to hash the password and compare it to the supplied hash, uses PWDTK default iterations (cDefaultIterationCount)
        /// </summary>
        /// <param name="Password">The text password to be hashed for comparrison</param>
        /// <param name="Salt">The salt to be added to the  password pre hash</param>
        /// <param name="Hash">The existing hash byte array you have stored for comparison</param>
        /// <returns>True if Password matches Hash else returns  false</returns>
        public static Boolean ComparePasswordToHash(Byte[] Salt, String Password, Byte[] Hash)
        {
            if (pPasswordToHash(Salt, StringToUTF8Bytes(Password), cDefaultIterationCount).SequenceEqual(Hash))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        ///  Input a password and a hash value in bytes and it uses PBKDF2 HMACSHA256 to hash the password and compare it to the supplied hash
        /// </summary>
        /// <param name="Password">The text password to be hashed for comparrison</param>
        /// <param name="Salt">The salt to be added to the  password pre hash</param>
        /// <param name="Hash">The existing hash byte array you have stored for comparison</param>
        /// <param name="IterationCount">The number of times you have specified to hash the password for key stretching</param>
        /// <returns>True if Password matches Hash else returns  false</returns>
        public static Boolean ComparePasswordToHash(Byte[] Salt, String Password, Byte[] Hash, Int32 IterationCount)
        {
            if (pPasswordToHash(Salt, StringToUTF8Bytes(Password), IterationCount).SequenceEqual(Hash))
            {
                return true;
            }

            return false;
        }
      
        /// <summary>
        ///  Converts Salt + Password into a Hash using PWDTK defined default iterations (cDefaultIterationCount)
        /// </summary>
        /// <param name="Salt">The salt to add infront of the password before processing the hash (Anti-Rainbow Table tactic)</param>
        /// <param name="Password">The password used to compute the hash</param>
        /// <returns>The Hash value of the salt + password as a Byte Array</returns>
        public static Byte[] PasswordToHash(Byte[] Salt, String Password)
        {
            pCheckSaltCompliance(Salt);

            return pPasswordToHash(Salt, StringToUTF8Bytes(Password), cDefaultIterationCount);
        }

        /// <summary>
        ///  Converts Salt + Password into a Hash
        /// </summary>
        /// <param name="Salt">The salt to add infront of the password before processing the hash (Anti-Rainbow Table tactic)</param>
        /// <param name="Password">The password used to compute the hash</param>
        /// <param name="IterationCount">Repeat the PBKDF2 dunction this many times (Anti-Rainbow Table tactic), higher value = more CPU usage which is better defence against cracking</param>
        /// <returns>The Hash value of the salt + password as a Byte Array</returns>
        public static Byte[] PasswordToHash(Byte[] Salt, String Password, Int32 IterationCount)
        {
            pCheckSaltCompliance(Salt);

            return pPasswordToHash(Salt, StringToUTF8Bytes(Password), IterationCount);
        }

        /// <summary>
        /// Converts the Byte array Hash into a Human Friendly HEX String
        /// </summary>
        /// <param name="Hash">The Hash value to convert</param>
        /// <returns>A HEX String representation of the Hash value</returns>
        public static String HashBytesToHexString(Byte[] Hash)
        {
            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < Hash.Length; i++)
            {
                sb.Append(Hash[i].ToString("X2"));
            }

            return sb.ToString();
        }

        /// <summary>
        /// Converts the Hash Hex String into a Byte[] for computational processing
        /// </summary>
        /// <param name="HashHexString">The Hash Hex String to convert back to bytes</param>
        /// <returns>Esentially reverses the HashToHexString function, turns the String back into Bytes</returns>
        public static Byte[] HashHexStringToBytes(String HashHexString)
        {
            int length = HashHexString.Length / 2;
            byte[] Buffer = new Byte[length];
            for (int i = 0; i < length; i++)
            {
                Buffer[i] = Convert.ToByte(HashHexString.Substring(i * 2, 2), 16);
            }

            return Buffer;
        }

        /// <summary>
        /// Tests the password for compliance against the supplied password policy
        /// </summary>
        /// <param name="Password">The password to test for compliance</param>
        /// <param name="PwdPolicy">The PasswordPolicy that we are testing that the Password complies with</param>
        /// <returns>True for Password Compliance with the Policy</returns>
        public static Boolean TryPasswordPolicyCompliance(String Password, PasswordPolicy PwdPolicy)
        {
            Boolean isCompliant = true;

            try
            {
                pCheckPasswordPolicyCompliance(Password, PwdPolicy);
            }
            catch
            {
                isCompliant = false;
            }

            return isCompliant;
        }

        /// <summary>
        /// Tests the password for compliance against the supplied password policy
        /// </summary>
        /// <param name="Password">The password to test for compliance</param>
        /// <param name="PwdPolicy">The PasswordPolicy that we are testing that the Password complies with</param>
        /// <param name="PwdPolicyException">The exception that will contain why the Password does not meet the PasswordPolicy</param>
        /// <returns>True for Password Compliance with the Policy</returns>
        public static Boolean TryPasswordPolicyCompliance(String Password, PasswordPolicy PwdPolicy, ref PasswordPolicyException PwdPolicyException)
        {
            Boolean isCompliant = true;

            try
            {
                pCheckPasswordPolicyCompliance(Password, PwdPolicy);
            }
            catch(PasswordPolicyException ex)
            {
                PwdPolicyException = ex;
                isCompliant = false;
            }

            return isCompliant;
        }

        /// <summary>
        /// Converts String to UTF8 friendly Byte Array
        /// </summary>
        /// <param name="stringToConvert">String to convert to Byte Array</param>
        /// <returns>A UTF8 decoded string as Byte Array</returns>
        public static Byte[] StringToUTF8Bytes(String stringToConvert)
        {
            return new UTF8Encoding(false).GetBytes(stringToConvert);
        }

        /// <summary>
        /// Converts UTF8 friendly Byte Array to String
        /// </summary>
        /// <param name="bytesToConvert">Byte Array to convert to String</param>
        /// <returns>A UTF8 encoded Byte Array as String</returns>
        public static String UTF8BytesToString(Byte[] bytesToConvert)
        {
            return new UTF8Encoding(false).GetString(bytesToConvert,0,bytesToConvert.Length);
        }

        #endregion

        #region PWDTK Private Methods

        private static void pCheckPasswordPolicyCompliance(String Password, PasswordPolicy PwdPolicy)
        {            
            if (new Regex(cNumbersRegex).Matches(Password).Count<PwdPolicy.ForceXNumeric)
            {
                throw new PasswordPolicyException("The password must contain "+PwdPolicy.ForceXNumeric+" numeric [0-9] characters");
            }

            if (new Regex(cNonAlphaNumericRegex).Matches(Password).Count < PwdPolicy.ForceXNonAlphaNumeric)
            {
                throw new PasswordPolicyException("The password must contain "+PwdPolicy.ForceXNonAlphaNumeric+" special characters");
            }

            if (new Regex(cUppercaseRegex).Matches(Password).Count < PwdPolicy.ForceXUpperCase)
            {
                throw new PasswordPolicyException("The password must contain "+PwdPolicy.ForceXUpperCase+" uppercase characters");
            }

            if (Password.Length < PwdPolicy.PasswordMinLength)
            {
                throw new PasswordPolicyException("The password does not have a length of at least "+PwdPolicy.PasswordMinLength+" characters");
            }

            if (Password.Length > PwdPolicy.PasswordMaxLength)
            {
                throw new PasswordPolicyException("The password is longer than "+PwdPolicy.PasswordMaxLength+" characters");
            }
        }

        private static Byte[] pGenerateRandomSalt(Int32 SaltLength)
        {
            Byte[] Salt = new byte[SaltLength];
            new RNGCryptoServiceProvider().GetBytes(Salt);
            return Salt;
        }

        private static void pCheckSaltCompliance(Byte[] Salt)
        {
            if (Salt.Length < cMinSaltLength)
            {
                throw new SaltTooShortException("The supplied salt is too short, it must be at least " + cMinSaltLength + " bytes long as defined by cDefaultMinSaltStringLength");
            }
        }
        
        private static byte[] pPasswordToHash(Byte[] Salt, Byte[] Password, Int32 IterationCount)
        {
            return new Rfc2898(Password, Salt, IterationCount).GetDerivedKeyBytes_PBKDF2_HMACSHA256(cKeyLength);                 
        }

        #endregion
    }

    #region PWDTK Custom Exceptions

    public class SaltTooShortException : Exception
    {
        public SaltTooShortException(String Message):base(Message)
        {

        }
    }

    public class PasswordPolicyException : Exception
    {
        public PasswordPolicyException(String Message):base(Message)
        {
      
        }
    }

    #endregion
}