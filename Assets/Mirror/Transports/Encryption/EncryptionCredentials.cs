using System;
using System.IO;
using Mirror.BouncyCastle.Asn1.Pkcs;
using Mirror.BouncyCastle.Asn1.X509;
using Mirror.BouncyCastle.Crypto;
using Mirror.BouncyCastle.Crypto.Digests;
using Mirror.BouncyCastle.Crypto.Generators;
using Mirror.BouncyCastle.X509;
using Mirror.BouncyCastle.Crypto.Parameters;
using Mirror.BouncyCastle.Pkcs;
using Mirror.BouncyCastle.Security;
using UnityEngine;

namespace Mirror.Transports.Encryption
{
    public class EncryptionCredentials
    {
        const int PrivateKeyBits = 256;
        // don't actually need to store this currently
        // but we'll need to for loading/saving from file maybe?
        // public ECPublicKeyParameters PublicKey;

        // The serialized public key, in DER format
        public byte[] PublicKeySerialized;
        public ECPrivateKeyParameters PrivateKey;
        public string PublicKeyFingerprint;

        EncryptionCredentials() {}

        // TODO: load from file
        public static EncryptionCredentials Generate()
        {
            var generator = new ECKeyPairGenerator();
            generator.Init(new KeyGenerationParameters(new SecureRandom(), PrivateKeyBits));
            AsymmetricCipherKeyPair keyPair = generator.GenerateKeyPair();
            var serialized = SerializePublicKey((ECPublicKeyParameters)keyPair.Public);
            return new EncryptionCredentials
            {
                // see fields above
                // PublicKey = (ECPublicKeyParameters)keyPair.Public,
                PublicKeySerialized = serialized,
                PublicKeyFingerprint = PubKeyFingerprint(new ArraySegment<byte>(serialized)),
                PrivateKey = (ECPrivateKeyParameters)keyPair.Private
            };
        }

        public static byte[] SerializePublicKey(AsymmetricKeyParameter publicKey)
        {
            // apparently the best way to transmit this public key over the network is to serialize it as a DER
            SubjectPublicKeyInfo publicKeyInfo = SubjectPublicKeyInfoFactory.CreateSubjectPublicKeyInfo(publicKey);
            return publicKeyInfo.ToAsn1Object().GetDerEncoded();
        }

        public static AsymmetricKeyParameter DeserializePublicKey(ArraySegment<byte> pubKey) =>
            // And then we do this to deserialize from the DER (from above)
            // the "new MemoryStream" actually saves an allocation, since otherwise the ArraySegment would be converted
            // to a byte[] first and then shoved through a MemoryStream
            PublicKeyFactory.CreateKey(new MemoryStream(pubKey.Array, pubKey.Offset, pubKey.Count, false));

        public static byte[] SerializePrivateKey(AsymmetricKeyParameter privateKey)
        {
            // Serialize privateKey as a DER
            PrivateKeyInfo privateKeyInfo = PrivateKeyInfoFactory.CreatePrivateKeyInfo(privateKey);
            return privateKeyInfo.ToAsn1Object().GetDerEncoded();
        }

        public static AsymmetricKeyParameter DeserializePrivateKey(ArraySegment<byte> privateKey) =>
            // And then we do this to deserialize from the DER (from above)
            // the "new MemoryStream" actually saves an allocation, since otherwise the ArraySegment would be converted
            // to a byte[] first and then shoved through a MemoryStream
            PrivateKeyFactory.CreateKey(new MemoryStream(privateKey.Array, privateKey.Offset, privateKey.Count, false));

        public static string PubKeyFingerprint(ArraySegment<byte> publicKeyBytes)
        {
            Sha256Digest digest = new Sha256Digest();
            byte[] hash = new byte[digest.GetDigestSize()];
            digest.BlockUpdate(publicKeyBytes.Array, publicKeyBytes.Offset, publicKeyBytes.Count);
            digest.DoFinal(hash, 0);

            return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
        }

        // ========== [LOGGING ADDED] ==========
        // Добавлено логирование ошибок сохранения ключей шифрования в файл
        // Критично для безопасности - проблемы с сохранением ключей могут привести к потере доступа
        public void SaveToFile(string path)
        {
            try
            {
                // Проверка на пустой путь
                if (string.IsNullOrEmpty(path))
                {
                    LoggerUtility.Error("Cannot save encryption credentials: path is null or empty");
                    throw new ArgumentException("Path cannot be null or empty", nameof(path));
                }

                string json = JsonUtility.ToJson(new SerializedPair
                {
                    PublicKeyFingerprint = PublicKeyFingerprint,
                    PublicKey = Convert.ToBase64String(PublicKeySerialized),
                    PrivateKey= Convert.ToBase64String(SerializePrivateKey(PrivateKey))
                });

                // Создание директории если её нет
                var directory = Path.GetDirectoryName(path);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                File.WriteAllText(path, json);
                LoggerUtility.Info($"Encryption credentials saved to: {path}");
            }
            catch (Exception ex)
            {
                // Логируем ошибку сохранения ключей шифрования
                LoggerUtility.Error($"Failed to save encryption credentials to {path}: {ex.Message}\n{ex.StackTrace}");
                throw;
            }
        }

        // ========== [LOGGING ADDED] ==========
        // Добавлено логирование ошибок загрузки ключей шифрования из файла
        // Критично для безопасности - проблемы с загрузкой ключей могут привести к потере доступа
        public static EncryptionCredentials LoadFromFile(string path)
        {
            try
            {
                // Проверка на пустой путь
                if (string.IsNullOrEmpty(path))
                {
                    LoggerUtility.Error("Cannot load encryption credentials: path is null or empty");
                    throw new ArgumentException("Path cannot be null or empty", nameof(path));
                }

                // Проверка существования файла
                if (!File.Exists(path))
                {
                    LoggerUtility.Error($"Encryption credentials file not found: {path}");
                    throw new FileNotFoundException($"File not found: {path}", path);
                }

                string json = File.ReadAllText(path);
                // Проверка на пустой файл
                if (string.IsNullOrEmpty(json))
                {
                    LoggerUtility.Error($"Encryption credentials file is empty: {path}");
                    throw new InvalidDataException($"File is empty: {path}");
                }

                SerializedPair serializedPair = JsonUtility.FromJson<SerializedPair>(json);
                // Проверка на null результат десериализации
                if (serializedPair == null)
                {
                    LoggerUtility.Error($"Failed to deserialize encryption credentials from: {path}");
                    throw new InvalidDataException($"Failed to deserialize JSON from: {path}");
                }

                byte[] publicKeyBytes = Convert.FromBase64String(serializedPair.PublicKey);
                byte[] privateKeyBytes = Convert.FromBase64String(serializedPair.PrivateKey);

                // Проверка целостности ключа по отпечатку
                if (serializedPair.PublicKeyFingerprint != PubKeyFingerprint(new ArraySegment<byte>(publicKeyBytes)))
                {
                    LoggerUtility.Error($"Public key fingerprint mismatch in file: {path}");
                    throw new Exception("Saved public key fingerprint does not match public key.");
                }

                LoggerUtility.Info($"Encryption credentials loaded from: {path}");
                return new EncryptionCredentials
                {
                    PublicKeySerialized = publicKeyBytes,
                    PublicKeyFingerprint = serializedPair.PublicKeyFingerprint,
                    PrivateKey = (ECPrivateKeyParameters) DeserializePrivateKey(new ArraySegment<byte>(privateKeyBytes))
                };
            }
            catch (Exception ex)
            {
                // Логируем ошибку загрузки ключей шифрования
                LoggerUtility.Error($"Failed to load encryption credentials from {path}: {ex.Message}\n{ex.StackTrace}");
                throw;
            }
        }

        class SerializedPair
        {
            public string PublicKeyFingerprint;
            public string PublicKey;
            public string PrivateKey;
        }
    }
}
