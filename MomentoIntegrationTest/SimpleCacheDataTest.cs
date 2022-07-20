﻿using System;
using System.Threading.Tasks;
using Xunit;
using MomentoSdk;
using MomentoSdk.Responses;
using MomentoSdk.Exceptions;
using System.Text;
using System.Collections.Generic;

namespace MomentoIntegrationTest
{
    public class SimpleCacheDataTest : IDisposable
    {
        private string authKey = Environment.GetEnvironmentVariable("TEST_AUTH_TOKEN") ??
            throw new NullReferenceException("TEST_AUTH_TOKEN environment variable must be set.");
        private string cacheName = "client-sdk-csharp";
        private uint defaultTtlSeconds = 10;
        private SimpleCacheClient client;

        // Test initialization
        public SimpleCacheDataTest()
        {
            client = new SimpleCacheClient(authKey, defaultTtlSeconds);
            try
            {
                client.CreateCache(cacheName);
            }
            catch (AlreadyExistsException)
            {
            }
        }

        // Test cleanup
        public void Dispose()
        {
            client.DeleteCache(cacheName);
            client.Dispose();
        }

        private byte[] Utf8ToBytes(string s)
        {
            return Encoding.UTF8.GetBytes(s);
        }

        [Theory]
        [InlineData(null, new byte[] { 0x00 }, new byte[] { 0x00 })]
        [InlineData("cache", null, new byte[] { 0x00 })]
        [InlineData("cache", new byte[] { 0x00 }, null)]
        public async void SetAsync_NullChecksBytesBytes_ThrowsException(string cacheName, byte[] key, byte[] value)
        {
            await Assert.ThrowsAsync<ArgumentNullException>(async () => await client.SetAsync(cacheName, key, value));
            await Assert.ThrowsAsync<ArgumentNullException>(async () => await client.SetAsync(cacheName, key, value, defaultTtlSeconds));
        }

        // Tests SetAsyc(cacheName, byte[], byte[]) as well as GetAsync(cacheName, byte[])
        [Fact]
        public async void SetAsync_KeyIsBytesValueIsBytes_HappyPath()
        {
            byte[] key = Utf8ToBytes("key1");
            byte[] value = Utf8ToBytes("value1");
            await client.SetAsync(cacheName, key, value);
            byte[]? setValue = (await client.GetAsync(cacheName, key)).Bytes();
            Assert.Equal(value, setValue);

            key = Utf8ToBytes("key2");
            value = Utf8ToBytes("value2");
            await client.SetAsync(cacheName, key, value, ttlSeconds: 15);
            setValue = (await client.GetAsync(cacheName, key)).Bytes();
            Assert.Equal(value, setValue);
        }

        [Theory]
        [InlineData(null, new byte[] { 0x00 })]
        [InlineData("cache", null)]
        public async void GetAsync_NullChecksBytes_ThrowsException(string cacheName, byte[] key)
        {
            await Assert.ThrowsAsync<ArgumentNullException>(async () => await client.GetAsync(cacheName, key));
        }

        [Theory]
        [InlineData(null, "key", "value")]
        [InlineData("cache", null, "value")]
        [InlineData("cache", "key", null)]
        public async void SetAsync_NullChecksStringString_ThrowsException(string cacheName, string key, string value)
        {
            await Assert.ThrowsAsync<ArgumentNullException>(async () => await client.SetAsync(cacheName, key, value));
            await Assert.ThrowsAsync<ArgumentNullException>(async () => await client.SetAsync(cacheName, key, value, defaultTtlSeconds));
        }

        // Also tests GetAsync(cacheName, string)
        [Fact]
        public async void SetAsync_KeyIsStringValueIsString_HappyPath()
        {
            string key = "key3";
            string value = "value3";
            await client.SetAsync(cacheName, key, value);
            string? setValue = (await client.GetAsync(cacheName, key)).String();
            Assert.Equal(value, setValue);

            key = "key4";
            value = "value4";
            await client.SetAsync(cacheName, key, value, ttlSeconds: 15);
            setValue = (await client.GetAsync(cacheName, key)).String();
            Assert.Equal(value, setValue);
        }

        [Theory]
        [InlineData(null, "key")]
        [InlineData("cache", null)]
        public async void GetAsync_NullChecksString_ThrowsException(string cacheName, string key)
        {
            await Assert.ThrowsAsync<ArgumentNullException>(async () => await client.GetAsync(cacheName, key));
        }

        [Theory]
        [InlineData(null, "key", new byte[] { 0x00 })]
        [InlineData("cache", null, new byte[] { 0x00 })]
        [InlineData("cache", "key", null)]
        public async void SetAsync_NullChecksStringBytes_ThrowsException(string cacheName, string key, byte[] value)
        {
            await Assert.ThrowsAsync<ArgumentNullException>(async () => await client.SetAsync(cacheName, key, value));
            await Assert.ThrowsAsync<ArgumentNullException>(async () => await client.SetAsync(cacheName, key, value, defaultTtlSeconds));
        }

        // Also tests GetAsync(cacheName, string)
        [Fact]
        public async void SetAsync_KeyIsStringValueIsBytes_HappyPath()
        {
            string key = "key5";
            byte[] value = Utf8ToBytes("value5");
            await client.SetAsync(cacheName, key, value);
            byte[]? setValue = (await client.GetAsync(cacheName, key)).Bytes();
            Assert.Equal(value, setValue);

            key = "key6";
            value = Utf8ToBytes("value6");
            await client.SetAsync(cacheName, key, value, ttlSeconds: 15);
            setValue = (await client.GetAsync(cacheName, key)).Bytes();
            Assert.Equal(value, setValue);
        }

        [Fact]
        public async void GetMultiAsync_NullCheckBytes_ThrowsException()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(async () => await client.GetMultiAsync(null!, new List<byte[]>()));
            await Assert.ThrowsAsync<ArgumentNullException>(async () => await client.GetMultiAsync("cache", (List<byte[]>)null!));

            var badList = new List<byte[]>(new byte[][] { Utf8ToBytes("asdf"), null! });
            await Assert.ThrowsAsync<ArgumentNullException>(async () => await client.GetMultiAsync("cache", badList));

            await Assert.ThrowsAsync<ArgumentNullException>(async () => await client.GetMultiAsync("cache", Utf8ToBytes("key1"), null!));
        }

        [Fact]
        public async void GetMultiAsync_KeysAreBytes_HappyPath()
        {
            string cacheKey1 = "key1";
            string cacheValue1 = "value1";
            string cacheKey2 = "key2";
            string cacheValue2 = "value2";
            client.Set(cacheName, cacheKey1, cacheValue1);
            client.Set(cacheName, cacheKey2, cacheValue2);

            List<byte[]> keys = new() { Utf8ToBytes(cacheKey1), Utf8ToBytes(cacheKey2) };

            CacheGetMultiResponse result = await client.GetMultiAsync(cacheName, keys);
            string? stringResult1 = result.Strings()[0];
            string? stringResult2 = result.Strings()[1];
            Assert.Equal(cacheValue1, stringResult1);
            Assert.Equal(cacheValue2, stringResult2);
        }

        [Fact]
        public async void GetMultiAsync_KeysAreBytes_HappyPath2()
        {
            string cacheKey1 = "key1";
            string cacheValue1 = "value1";
            string cacheKey2 = "key2";
            string cacheValue2 = "value2";
            client.Set(cacheName, cacheKey1, cacheValue1);
            client.Set(cacheName, cacheKey2, cacheValue2);

            CacheGetMultiResponse result = await client.GetMultiAsync(cacheName, cacheKey1, cacheKey2);
            string? stringResult1 = result.Strings()[0];
            string? stringResult2 = result.Strings()[1];
            Assert.Equal(cacheValue1, stringResult1);
            Assert.Equal(cacheValue2, stringResult2);
        }

        [Fact]
        public async void GetMultiAsync_NullCheckString_ThrowsException()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(async () => await client.GetMultiAsync(null!, new List<string>()));
            await Assert.ThrowsAsync<ArgumentNullException>(async () => await client.GetMultiAsync("cache", (List<string>)null!));

            List<string> strings = new(new string[] { "key1", "key2", null! });
            await Assert.ThrowsAsync<ArgumentNullException>(async () => await client.GetMultiAsync("cache", strings));

            await Assert.ThrowsAsync<ArgumentNullException>(async () => await client.GetMultiAsync("cache", "key1", "key2", null!));
        }

        [Fact]
        public async void GetMultiAsync_KeysAreString_HappyPath()
        {
            string cacheKey1 = "key1";
            string cacheValue1 = "value1";
            string cacheKey2 = "key2";
            string cacheValue2 = "value2";
            client.Set(cacheName, cacheKey1, cacheValue1);
            client.Set(cacheName, cacheKey2, cacheValue2);

            List<string> keys = new() { cacheKey1, cacheKey2, "key123123" };
            CacheGetMultiResponse result = await client.GetMultiAsync(cacheName, keys);

            Assert.Equal(result.Strings(), new string[] { cacheValue1, cacheValue2, null! });
            Assert.Equal(result.Status(), new CacheGetStatus[] { CacheGetStatus.HIT, CacheGetStatus.HIT, CacheGetStatus.MISS });
        }

        [Fact]
        public void GetMultiAsync_Failure()
        {
            // Set very small timeout for dataClientOperationTimeoutMilliseconds
            SimpleCacheClient simpleCacheClient = new SimpleCacheClient(authKey, defaultTtlSeconds, 1);
            List<string> keys = new() { "key1", "key2", "key3", "key4" };
            Assert.ThrowsAsync<MomentoSdk.Exceptions.TimeoutException>(() => simpleCacheClient.GetMultiAsync(cacheName, keys));
        }

        [Fact]
        public async void SetMultiAsync_NullCheckBytes_ThrowsException()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(async () => await client.SetMultiAsync(null!, new Dictionary<byte[], byte[]>()));
            await Assert.ThrowsAsync<ArgumentNullException>(async () => await client.SetMultiAsync("cache", (Dictionary<byte[], byte[]>)null!));

            var badDictionary = new Dictionary<byte[], byte[]>() { { Utf8ToBytes("asdf"), null! } };
            await Assert.ThrowsAsync<ArgumentNullException>(async () => await client.SetMultiAsync("cache", badDictionary));
        }

        [Fact]
        public async void SetMultiAsync_ItemsAreBytes_HappyPath()
        {
            var dictionary = new Dictionary<byte[], byte[]>() {
                { Utf8ToBytes("key1"), Utf8ToBytes("value1") },
                { Utf8ToBytes("key2"), Utf8ToBytes("value2") }
            };
            CacheSetMultiResponse response = await client.SetMultiAsync(cacheName, dictionary);
            Assert.Equal(dictionary, response.Bytes());

            var getResponse = await client.GetAsync(cacheName, Utf8ToBytes("key1"));
            Assert.Equal("value1", getResponse.String());

            getResponse = await client.GetAsync(cacheName, Utf8ToBytes("key2"));
            Assert.Equal("value2", getResponse.String());
        }

        [Fact]
        public async void SetMultiAsync_NullCheckStrings_ThrowsException()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(async () => await client.SetMultiAsync(null!, new Dictionary<string, string>()));
            await Assert.ThrowsAsync<ArgumentNullException>(async () => await client.SetMultiAsync("cache", (Dictionary<string, string>)null!));

            var badDictionary = new Dictionary<string, string>() { { "asdf", null! } };
            await Assert.ThrowsAsync<ArgumentNullException>(async () => await client.SetMultiAsync("cache", badDictionary));
        }

        [Fact]
        public async void SetMultiAsync_KeysAreString_HappyPath()
        {
            var dictionary = new Dictionary<string, string>() {
                { "key1", "value1" },
                { "key2", "value2" }
            };
            CacheSetMultiResponse response = await client.SetMultiAsync(cacheName, dictionary);
            Assert.Equal(dictionary, response.Strings());

            var getResponse = await client.GetAsync(cacheName, "key1");
            Assert.Equal("value1", getResponse.String());

            getResponse = await client.GetAsync(cacheName, "key2");
            Assert.Equal("value2", getResponse.String());
        }

        [Theory]
        [InlineData(null, new byte[] { 0x00 }, new byte[] { 0x00 })]
        [InlineData("cache", null, new byte[] { 0x00 })]
        [InlineData("cache", new byte[] { 0x00 }, null)]
        public void Set_NullChecksBytesBytes_ThrowsException(string cacheName, byte[] key, byte[] value)
        {
            Assert.Throws<ArgumentNullException>(() => client.Set(cacheName, key, value));
            Assert.Throws<ArgumentNullException>(() => client.Set(cacheName, key, value, defaultTtlSeconds));
        }

        // Tests Set(cacheName, byte[], byte[]) as well as Get(cacheName, byte[])
        [Fact]
        public void Set_KeyIsBytesValueIsBytes_HappyPath()
        {
            byte[] key = Utf8ToBytes("key10");
            byte[] value = Utf8ToBytes("value10");
            client.Set(cacheName, key, value);
            byte[]? setValue = client.Get(cacheName, key).Bytes();
            Assert.Equal(value, setValue);

            key = Utf8ToBytes("key11");
            value = Utf8ToBytes("value11");
            client.Set(cacheName, key, value, ttlSeconds: 15);
            setValue = client.Get(cacheName, key).Bytes();
            Assert.Equal(value, setValue);
        }

        [Fact]
        public void Get_NullChecksBytes_ThrowsException()
        {
            Assert.Throws<ArgumentNullException>(() => client.Get("cache", (byte[])null!));
            Assert.Throws<ArgumentNullException>(() => client.Get(null!, new byte[] { 0x00 }));
        }

        [Fact]
        public async void Get_ExpiredTtl_HappyPath()
        {
            string cacheKey = "some cache key";
            string cacheValue = "some cache value";
            client.Set(cacheName, cacheKey, cacheValue, 1);
            await Task.Delay(3000);
            CacheGetResponse result = client.Get(cacheName, cacheKey);
            Assert.Equal(CacheGetStatus.MISS, result.Status);
        }

        [Fact]
        public void Get_Miss_HappyPath()
        {
            CacheGetResponse result = client.Get(cacheName, Guid.NewGuid().ToString());
            Assert.Equal(CacheGetStatus.MISS, result.Status);
            Assert.Null(result.String());
            Assert.Null(result.Bytes());
        }

        [Fact]
        public void Get_CacheDoesntExist_ThrowsException()
        {
            uint defaultTtlSeconds = 10;
            SimpleCacheClient client = new SimpleCacheClient(authKey, defaultTtlSeconds);
            Assert.Throws<NotFoundException>(() => client.Get("non-existent-cache", Guid.NewGuid().ToString()));
        }

        [Fact]
        public void Set_CacheDoesntExist_ThrowsException()
        {
            uint defaultTtlSeconds = 10;
            SimpleCacheClient client = new SimpleCacheClient(authKey, defaultTtlSeconds);
            Assert.Throws<NotFoundException>(() => client.Set("non-existent-cache", Guid.NewGuid().ToString(), Guid.NewGuid().ToString()));
        }

        [Theory]
        [InlineData(null, "key", "value")]
        [InlineData("cache", null, "value")]
        [InlineData("cache", "key", null)]
        public void Set_NullChecksStringString_ThrowsException(string cacheName, string key, string value)
        {
            Assert.Throws<ArgumentNullException>(() => client.Set(cacheName, key, value));
            Assert.Throws<ArgumentNullException>(() => client.Set(cacheName, key, value, defaultTtlSeconds));
        }

        // Tests Set(cacheName, string, string) as well as Get(cacheName, string)
        [Fact]
        public void Set_KeyIsStringValueIsString_HappyPath()
        {
            string key = "key12";
            string value = "value12";
            client.Set(cacheName, key, value);
            string? setValue = client.Get(cacheName, key).String();
            Assert.Equal(value, setValue);

            key = "key13";
            value = "value13";
            client.Set(cacheName, key, value, ttlSeconds: 15);
            setValue = client.Get(cacheName, key).String();
            Assert.Equal(value, setValue);
        }

        [Theory]
        [InlineData(null, "key")]
        [InlineData("cache", null)]
        public void Get_NullChecksString_ThrowsException(string cacheName, string key)
        {
            Assert.Throws<ArgumentNullException>(() => client.Get(cacheName, key));
        }

        [Theory]
        [InlineData(null, "key", new byte[] { 0x00 })]
        [InlineData("cache", null, new byte[] { 0x00 })]
        [InlineData("cache", "key", null)]
        public void Set_NullChecksStringBytes_ThrowsException(string cacheName, string key, byte[] value)
        {
            Assert.Throws<ArgumentNullException>(() => client.Set(cacheName, key, value));
            Assert.Throws<ArgumentNullException>(() => client.Set(cacheName, key, value, defaultTtlSeconds));
        }

        // Tests Set(cacheName, string, byte[]) as well as Get(cacheName, string)
        [Fact]
        public void Set_KeyIsStringValueIsBytes_HappyPath()
        {
            string key = "key14";
            byte[] value = Utf8ToBytes("value14");
            client.Set(cacheName, key, value);
            byte[]? setValue = client.Get(cacheName, key).Bytes();
            Assert.Equal(value, setValue);

            key = "key15";
            value = Utf8ToBytes("value15");
            client.Set(cacheName, key, value, ttlSeconds: 15);
            setValue = client.Get(cacheName, key).Bytes();
            Assert.Equal(value, setValue);
        }

        [Fact]
        public void GetMulti_NullCheckBytes_ThrowsException()
        {
            Assert.Throws<ArgumentNullException>(() => client.GetMulti(null!, new List<byte[]>()));
            Assert.Throws<ArgumentNullException>(() => client.GetMulti("cache", (List<byte[]>)null!));

            var badList = new List<byte[]>(new byte[][] { Utf8ToBytes("asdf"), null! });
            Assert.Throws<ArgumentNullException>(() => client.GetMulti("cache", badList));

            Assert.Throws<ArgumentNullException>(() => client.GetMulti("cache", Utf8ToBytes("key1"), null!));
        }

        [Fact]
        public void GetMulti_KeysAreBytes_HappyPath()
        {
            string cacheKey1 = "key1";
            string cacheValue1 = "value1";
            string cacheKey2 = "key2";
            string cacheValue2 = "value2";
            client.Set(cacheName, cacheKey1, cacheValue1);
            client.Set(cacheName, cacheKey2, cacheValue2);

            List<byte[]> keys = new() { Utf8ToBytes(cacheKey1), Utf8ToBytes(cacheKey2) };

            CacheGetMultiResponse result = client.GetMulti(cacheName, keys);
            string? stringResult1 = result.Strings()[0];
            string? stringResult2 = result.Strings()[1];
            Assert.Equal(cacheValue1, stringResult1);
            Assert.Equal(cacheValue2, stringResult2);
        }

        [Fact]
        public void GetMulti_KeysAreBytes_HappyPath2()
        {
            string cacheKey1 = "key1";
            string cacheValue1 = "value1";
            string cacheKey2 = "key2";
            string cacheValue2 = "value2";
            client.Set(cacheName, cacheKey1, cacheValue1);
            client.Set(cacheName, cacheKey2, cacheValue2);

            CacheGetMultiResponse result = client.GetMulti(cacheName, cacheKey1, cacheKey2);
            string? stringResult1 = result.Strings()[0];
            string? stringResult2 = result.Strings()[1];
            Assert.Equal(cacheValue1, stringResult1);
            Assert.Equal(cacheValue2, stringResult2);
        }

        [Fact]
        public void GetMulti_NullCheckString_ThrowsException()
        {
            Assert.Throws<ArgumentNullException>(() => client.GetMulti(null!, new List<string>()));
            Assert.Throws<ArgumentNullException>(() => client.GetMulti("cache", (List<string>)null!));

            List<string> strings = new(new string[] { "key1", "key2", null! });
            Assert.Throws<ArgumentNullException>(() => client.GetMulti("cache", strings));

            Assert.Throws<ArgumentNullException>(() => client.GetMulti("cache", "key1", "key2", null!));
        }

        [Fact]
        public void GetMulti_KeysAreString_HappyPath()
        {
            string cacheKey1 = "key1";
            string cacheValue1 = "value1";
            string cacheKey2 = "key2";
            string cacheValue2 = "value2";
            client.Set(cacheName, cacheKey1, cacheValue1);
            client.Set(cacheName, cacheKey2, cacheValue2);

            List<string> keys = new() { cacheKey1, cacheKey2, "key123123" };
            CacheGetMultiResponse result = client.GetMulti(cacheName, keys);

            Assert.Equal(result.Strings(), new string[] { cacheValue1, cacheValue2, null! });
            Assert.Equal(result.Status(), new CacheGetStatus[] { CacheGetStatus.HIT, CacheGetStatus.HIT, CacheGetStatus.MISS });
        }

        [Fact]
        public void GetMulti_Failure()
        {
            // Set very small timeout for dataClientOperationTimeoutMilliseconds
            SimpleCacheClient simpleCacheClient = new SimpleCacheClient(authKey, defaultTtlSeconds, 1);
            List<string> keys = new() { "key1", "key2", "key3", "key4" };
            Assert.Throws<MomentoSdk.Exceptions.TimeoutException>(() => simpleCacheClient.GetMulti(cacheName, keys));
        }

        [Fact]
        public void SetMulti_NullCheckBytes_ThrowsException()
        {
            Assert.Throws<ArgumentNullException>(() => client.SetMulti(null!, new Dictionary<byte[], byte[]>()));
            Assert.Throws<ArgumentNullException>(() => client.SetMulti("cache", (Dictionary<byte[], byte[]>)null!));

            var badDictionary = new Dictionary<byte[], byte[]>() { { Utf8ToBytes("asdf"), null! } };
            Assert.Throws<ArgumentNullException>(() => client.SetMulti("cache", badDictionary));
        }

        [Fact]
        public void SetMulti_ItemsAreBytes_HappyPath()
        {
            var dictionary = new Dictionary<byte[], byte[]>() {
                    { Utf8ToBytes("key1"), Utf8ToBytes("value1") },
                    { Utf8ToBytes("key2"), Utf8ToBytes("value2") }
                };
            CacheSetMultiResponse response = client.SetMulti(cacheName, dictionary);
            Assert.Equal(dictionary, response.Bytes());

            var getResponse = client.Get(cacheName, Utf8ToBytes("key1"));
            Assert.Equal("value1", getResponse.String());

            getResponse = client.Get(cacheName, Utf8ToBytes("key2"));
            Assert.Equal("value2", getResponse.String());
        }


        [Fact]
        public void SetMulti_NullCheckString_ThrowsException()
        {
            Assert.Throws<ArgumentNullException>(() => client.SetMulti(null!, new Dictionary<string, string>()));
            Assert.Throws<ArgumentNullException>(() => client.SetMulti("cache", (Dictionary<string, string>)null!));

            var badDictionary = new Dictionary<string, string>() { { "asdf", null! } };
            Assert.Throws<ArgumentNullException>(() => client.SetMulti("cache", badDictionary));
        }

        [Fact]
        public void SetMulti_KeysAreString_HappyPath()
        {
            var dictionary = new Dictionary<string, string>() {
                    { "key1", "value1" },
                    { "key2", "value2" }
                };
            CacheSetMultiResponse response = client.SetMulti(cacheName, dictionary);
            Assert.Equal(dictionary, response.Strings());

            var getResponse = client.Get(cacheName, "key1");
            Assert.Equal("value1", getResponse.String());

            getResponse = client.Get(cacheName, "key2");
            Assert.Equal("value2", getResponse.String());
        }

        [Theory]
        [InlineData(null, new byte[] { 0x00 })]
        [InlineData("cache", null)]
        public void Delete_NullChecksBytes_ThrowsException(string cacheName, byte[] key)
        {
            Assert.Throws<ArgumentNullException>(() => client.Delete(cacheName, key));
        }

        [Fact]
        public void Delete_KeyIsBytes_HappyPath()
        {
            // Set a key to then delete
            byte[] key = new byte[] { 0x01, 0x02, 0x03, 0x04 };
            byte[] value = new byte[] { 0x05, 0x06, 0x07, 0x08 };
            client.Set(cacheName, key, value, ttlSeconds: 60);
            CacheGetResponse getResponse = client.Get(cacheName, key);
            Assert.Equal(CacheGetStatus.HIT, getResponse.Status);

            // Delete
            client.Delete(cacheName, key);

            // Check deleted
            getResponse = client.Get(cacheName, key);
            Assert.Equal(CacheGetStatus.MISS, getResponse.Status);
        }

        [Theory]
        [InlineData(null, new byte[] { 0x00 })]
        [InlineData("cache", null)]
        public async void DeleteAsync_NullChecksByte_ThrowsException(string cacheName, byte[] key)
        {
            await Assert.ThrowsAsync<ArgumentNullException>(async () => await client.DeleteAsync(cacheName, key));
        }

        [Fact]
        public async void DeleteAsync_KeyIsBytes_HappyPath()
        {
            // Set a key to then delete
            byte[] key = new byte[] { 0x01, 0x02, 0x03, 0x04 };
            byte[] value = new byte[] { 0x05, 0x06, 0x07, 0x08 };
            await client.SetAsync(cacheName, key, value, ttlSeconds: 60);
            CacheGetResponse getResponse = await client.GetAsync(cacheName, key);
            Assert.Equal(CacheGetStatus.HIT, getResponse.Status);

            // Delete
            await client.DeleteAsync(cacheName, key);

            // Check deleted
            getResponse = await client.GetAsync(cacheName, key);
            Assert.Equal(CacheGetStatus.MISS, getResponse.Status);
        }

        [Theory]
        [InlineData(null, "key")]
        [InlineData("cache", null)]
        public void Delete_NullChecksString_ThrowsException(string cacheName, string key)
        {
            Assert.Throws<ArgumentNullException>(() => client.Delete(cacheName, key));
        }

        [Fact]
        public void Delete_KeyIsString_HappyPath()
        {
            // Set a key to then delete
            string key = "key";
            string value = "value";
            client.Set(cacheName, key, value, ttlSeconds: 60);
            CacheGetResponse getResponse = client.Get(cacheName, key);
            Assert.Equal(CacheGetStatus.HIT, getResponse.Status);

            // Delete
            client.Delete(cacheName, key);

            // Check deleted
            getResponse = client.Get(cacheName, key);
            Assert.Equal(CacheGetStatus.MISS, getResponse.Status);
        }

        [Theory]
        [InlineData(null, "key")]
        [InlineData("cache", null)]
        public async void DeleteAsync_NullChecksString_ThrowsException(string cacheName, string key)
        {
            await Assert.ThrowsAsync<ArgumentNullException>(async () => await client.DeleteAsync(cacheName, key));
        }

        [Fact]
        public async void DeleteAsync_KeyIsString_HappyPath()
        {
            // Set a key to then delete
            string key = "key";
            string value = "value";
            await client.SetAsync(cacheName, key, value, ttlSeconds: 60);
            CacheGetResponse getResponse = await client.GetAsync(cacheName, key);
            Assert.Equal(CacheGetStatus.HIT, getResponse.Status);

            // Delete
            await client.DeleteAsync(cacheName, key);

            // Check deleted
            getResponse = await client.GetAsync(cacheName, key);
            Assert.Equal(CacheGetStatus.MISS, getResponse.Status);
        }
    }
}
