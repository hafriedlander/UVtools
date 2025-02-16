﻿/*
 *                     GNU AFFERO GENERAL PUBLIC LICENSE
 *                       Version 3, 19 November 2007
 *  Copyright (C) 2007 Free Software Foundation, Inc. <https://fsf.org/>
 *  Everyone is permitted to copy and distribute verbatim copies
 *  of this license document, but changing it is not allowed.
 */

using System;
using System.IO;
using System.Security.Cryptography;

namespace UVtools.Core.Extensions
{
    public static class CryptExtensions
    {
        public static SHA1CryptoServiceProvider SHA1 { get; } = new();
        public static string ComputeSHA1Hash(byte[] input)
        {
            return Convert.ToBase64String(SHA1.ComputeHash(input));
        }

        public static SHA256 SHA256 { get; } = SHA256.Create();
        public static byte[] ComputeSHA256Hash(byte[] input)
        {
            return SHA256.ComputeHash(input);
        }

        public static byte[] AesCryptBytes(byte[] data, byte[] key, CipherMode mode, PaddingMode paddingMode, bool encrypt, byte[] iv = null)
        {
            if (data.Length % 16 != 0)
            {
                var temp = new byte[((data.Length / 16) + 1) * 16];
                Array.Copy(data, 0, temp, 0, data.Length);
                data = temp;
            }

            var aes = new AesManaged
            {
                KeySize = key.Length * 8,
                Key = key,
                Padding = paddingMode,
                Mode = mode,
            };

            if (iv != null)
            {
                aes.IV = iv;
            }

            var cryptor = encrypt ? aes.CreateEncryptor() : aes.CreateDecryptor();

            using var msDecrypt = new MemoryStream(data);
            using var csDecrypt = new CryptoStream(msDecrypt, cryptor, CryptoStreamMode.Read);
            var outputBuffer = new byte[data.Length];
            csDecrypt.Read(outputBuffer, 0, data.Length);

            return outputBuffer;
        }

        public static MemoryStream AesCryptMemoryStream(byte[] data, byte[] key, CipherMode mode, PaddingMode paddingMode, bool encrypt, byte[] iv = null)
            => new(AesCryptBytes(data, key, mode, paddingMode, encrypt, iv));

        public static string Base64EncodeString(string plainText)
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return Convert.ToBase64String(plainTextBytes);
        }

        public static string Base64DecodeString(string base64EncodedData)
        {
            var base64EncodedBytes = Convert.FromBase64String(base64EncodedData);
            return System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
        }

        public static string XORCipherString(string text, string key)
        {
            var output = new char[text.Length];

            for (int i = 0; i < text.Length; i++)
            {
                output[i] = (char)(text[i] ^ key[i % key.Length]);
            }

            return new string(output);
        }

        public static string XORCipherString(byte[] bytes, string key)
        {
            var output = new char[bytes.Length];

            for (int i = 0; i < bytes.Length; i++)
            {
                output[i] = (char)(bytes[i] ^ key[i % key.Length]);
            }

            return new string(output);
        }

        public static byte[] XORCipher(string text, string key)
        {
            var output = new byte[text.Length];

            for (int i = 0; i < text.Length; i++)
            {
                output[i] = (byte)(text[i] ^ key[i % key.Length]);
            }

            return output;
        }

        public static byte[] XORCipher(byte[] bytes, string key)
        {
            var output = new byte[bytes.Length];

            for (int i = 0; i < bytes.Length; i++)
            {
                output[i] = (byte)(bytes[i] ^ key[i % key.Length]);
            }

            return output;
        }
    }
}
