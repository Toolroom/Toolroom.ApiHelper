using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using StackExchange.Redis;

namespace Toolroom.ApiHelper
{
    public class RedisCache
    {
        private readonly string _connectionString;
        private readonly IContractResolver _contractResolver;
        private readonly Lazy<ConnectionMultiplexer> _lazyConnection;

        public RedisCache(string connectionString, IContractResolver contractResolver = null)
        {
            _connectionString = connectionString;
            _contractResolver = contractResolver;
            _lazyConnection = new Lazy<ConnectionMultiplexer>(() => ConnectionMultiplexer.Connect(_connectionString));
        }

        public RedisCacheInfo GetInfo()
        {
            var ret = new RedisCacheInfo();
            ret.IsConnected = IsConnected;
            if (!ret.IsConnected)
                return ret;

            var result = Db.Execute("INFO", "memory");
            if (!result.IsNull)
            {
                var str = result.ToString();
                {
                    Regex r = new Regex(@"used_memory_dataset:(\d*)?");
                    var m = r.Match(str);
                    if (m.Success)
                    {
                        if (m.Groups.Count > 1 && long.TryParse(m.Groups[1].Value, out var size))
                            ret.UsedMemory = size;
                    }
                }
                {
                    Regex r = new Regex(@"maxmemory:(\d*)?");
                    if (r.IsMatch(str))
                    {
                        var m = r.Match(str);
                        if (m.Groups.Count > 1 && long.TryParse(m.Groups[1].Value, out var size))
                            ret.MaximumMemory = size;
                    }
                }
            }
            return ret;
        }

        private ConnectionMultiplexer Connection => _lazyConnection.Value;

        private IDatabase Db => Connection.GetDatabase();

        private bool IsConnected => Connection.IsConnected;

        public bool Clear()
        {
            if (!IsConnected) return false;
            Db.Execute("FLUSHALL");
            return true;

        }

        public long DeleteAll(string prefix)
        {
            var server = Connection.GetServer(_connectionString.Split(',')[0]);
            if (!server.IsConnected) return -1;

            var keys = server.Keys(Db.Database, pattern: GetKey(prefix, "*").ToString()).ToArray();
            if (!IsConnected) return -1;
            var deletedItems = Db.KeyDelete(keys);
            return deletedItems;
        }

        public bool Delete<TKey>(string prefix, TKey id)
        {
            if (!IsConnected) return false;
            var key = GetKey(prefix, id);
            return !Db.KeyExists(key) || Db.KeyDelete(key);
        }

        public long Delete<TKey>(string prefix, IEnumerable<TKey> ids)
        {
            if (!IsConnected) return 0;
            var keys = new List<RedisKey>();

            foreach (var id in ids)
            {
                keys.Add(GetKey(prefix, id));
            }
            return Db.KeyDelete(keys.ToArray());
        }

        private RedisKey GetKey<TKey>(string prefix, TKey id)
        {
            return $"{prefix}:{id}";
        }
        private IEnumerable<KeyValuePair<TKey, RedisKey>> GetKeys<TKey>(string prefix, IEnumerable<TKey> ids)
        {
            foreach (var id in ids)
            {
                yield return new KeyValuePair<TKey, RedisKey>(id, $"{prefix}:{id}");
            }
        }

        private bool Set<TKey, T>(string prefix, TKey id, T item, TimeSpan? expiry = null)
        {
            var key = GetKey(prefix, id);
            return Set(key, item, expiry);
        }

        private bool Set<T>(RedisKey key, T item, TimeSpan? expiry = null)
        {
            if (!IsConnected) return false;
            var serializedItem = SerializeObject(item);
            return Db.StringSet(key, serializedItem, expiry);
        }

        private bool SetMany<TKey, T>(string prefix, IDictionary<TKey, T> items)
        {
            if (!IsConnected) return false;
            var list = new List<KeyValuePair<RedisKey, RedisValue>>();
            foreach (var item in items)
            {
                list.Add(new KeyValuePair<RedisKey, RedisValue>(GetKey(prefix, item.Key), SerializeObject(item.Value)));
            }
            return Db.StringSet(list.ToArray());
        }

        public void AddOrUpdate<TKey, T>(string prefix, TKey id, T item)
        {
            if (id == null)
                throw new ArgumentNullException(nameof(id));
            if (item == null)
                throw new ArgumentNullException(nameof(item));

            if (!IsConnected) return;

            var key = GetKey(prefix, id);
            var storedVal = SerializeObject(item);
            Db.StringSet(key, storedVal);
        }

        public async Task AddOrUpdate<TKey, T>(string prefix, TKey id, Func<TKey, Task<T>> valueFactory)
        {
            if (id == null)
                throw new ArgumentNullException(nameof(id));
            if (valueFactory == null)
                throw new ArgumentNullException(nameof(valueFactory));

            if (!IsConnected) return;

            var key = GetKey(prefix, id);
            var storedVal = SerializeObject(await valueFactory(id));
            Db.StringSet(key, storedVal);
        }

        public T GetOrAdd<TKey, T>(string prefix, TKey id, Func<TKey, T> valueFactory, TimeSpan? expiry = null)
        {
            if (id == null)
                throw new ArgumentNullException(nameof(id));
            if (valueFactory == null)
                throw new ArgumentNullException(nameof(valueFactory));

            if (!IsConnected) return valueFactory(id);

            var key = GetKey(prefix, id);

            var storedVal = Db.StringGet(key);
            if (storedVal.HasValue)
            {
                try
                {
                    var ret = DeserializeObject<T>(storedVal);
                    return ret;
                }
                catch
                {
                    // ignored - cannot deserialize - must be refreshed
                }
            }

            var newVal = valueFactory(id);
            Set(prefix, id, newVal, expiry);
            return newVal;
        }

        public async Task<T> GetOrAdd<TKey, T>(string prefix, TKey id, Func<TKey, Task<T>> valueFactory, TimeSpan? expiry = null)
        {
            if (id == null)
                throw new ArgumentNullException(nameof(id));
            if (valueFactory == null)
                throw new ArgumentNullException(nameof(valueFactory));

            if (!IsConnected) return await valueFactory(id);

            var key = GetKey(prefix, id);

            var storedVal = Db.StringGet(key);
            if (storedVal.HasValue)
            {
                try
                {
                    var ret = DeserializeObject<T>(storedVal);
                    return ret;
                }
                catch
                {
                    // ignored - cannot deserialize - must be refreshed
                }
            }

            var newVal = await valueFactory(id);
            Set(prefix, id, newVal, expiry);
            return newVal;
        }

        public async Task<ICollection<T>> GetOrAddMany<TKey, T>(string prefix, IEnumerable<TKey> ids, Func<IEnumerable<TKey>, Task<IDictionary<TKey, T>>> valueFactory, int valueFactoryBatchSize = 500)
        {
            var ret = new List<T>();
            if (ids == null)
                throw new ArgumentNullException(nameof(ids));

            if (valueFactory == null)
                throw new ArgumentNullException(nameof(valueFactory));

            var distinctIds = ids.Distinct();
            if (!IsConnected)
            {
                return (await valueFactory(distinctIds)).Values;
            }

            var keys = GetKeys(prefix, distinctIds).ToDictionary(_ => _.Key, _ => _.Value);
            var missingIds = new List<TKey>();
            var storedVals = Db.StringGet(keys.Values.ToArray());
            for (int i = 0; i < storedVals.Length; i++)
            {
                var storedVal = storedVals[i];
                if (storedVal.HasValue)
                {
                    T element;
                    try
                    {
                        element = DeserializeObject<T>(storedVal);
                    }
                    catch
                    {
                        // ignored - cannot deserialize - must be refreshed
                        missingIds.Add(keys.ElementAt(i).Key);
                        continue;
                    }
                    if (element != null && !element.Equals(default(T)))
                        ret.Add(element);
                    else
                        missingIds.Add(keys.ElementAt(i).Key);
                }
                else
                    missingIds.Add(keys.ElementAt(i).Key);
            }
            while (missingIds.Any())
            {
                var missingElements = await valueFactory(missingIds.Take(valueFactoryBatchSize));
                ret.AddRange(missingElements.Values);
                SetMany(prefix, missingElements);
                missingIds.RemoveRange(0, Math.Min(valueFactoryBatchSize, missingIds.Count));
            }
            return ret;
        }

        public async Task<IDictionary<TKey, T>> GetOrAddManyWithKeys<TKey, T>(string prefix, IEnumerable<TKey> ids, Func<IEnumerable<TKey>, Task<IDictionary<TKey, T>>> valueFactory, int valueFactoryBatchSize = 500)
        {
            var ret = new Dictionary<TKey, T>();
            if (ids == null)
                throw new ArgumentNullException(nameof(ids));

            if (valueFactory == null)
                throw new ArgumentNullException(nameof(valueFactory));

            var distinctIds = ids.Distinct();
            if (!IsConnected)
            {
                return await valueFactory(distinctIds);
            }

            var keys = GetKeys(prefix, distinctIds).ToDictionary(_ => _.Key, _ => _.Value);
            var missingIds = new List<TKey>();
            var storedVals = Db.StringGet(keys.Values.ToArray());
            for (int i = 0; i < storedVals.Length; i++)
            {
                var storedVal = storedVals[i];
                var key = keys.ElementAt(i).Key;
                if (storedVal.HasValue)
                {
                    T element;
                    try
                    {
                        element = DeserializeObject<T>(storedVal);
                    }
                    catch
                    {
                        // ignored - cannot deserialize - must be refreshed
                        missingIds.Add(key);
                        continue;
                    }
                    if (element != null && !element.Equals(default(T)))
                        ret.Add(key, element);
                    else
                        missingIds.Add(key);
                }
                else
                    missingIds.Add(keys.ElementAt(i).Key);
            }
            if (missingIds.Any())
            {
                var missingElements = await valueFactory(missingIds.Take(valueFactoryBatchSize));
                foreach (var missingElement in missingElements)
                {
                    ret.Add(missingElement.Key, missingElement.Value);
                }
                SetMany(prefix, missingElements);
                missingIds.RemoveRange(0, Math.Min(valueFactoryBatchSize, missingIds.Count));
            }

            return ret;
        }

        private JsonSerializerSettings _serializerSettings;

        private JsonSerializerSettings SerializerSettings =>
            _serializerSettings ?? (_serializerSettings = new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Serialize,
                PreserveReferencesHandling = PreserveReferencesHandling.Objects,
                TypeNameHandling = TypeNameHandling.All,
                ContractResolver = _contractResolver
            });
        private string SerializeObject(object objectToCache)
        {
            return JsonConvert.SerializeObject(objectToCache
                , Formatting.Indented
                , SerializerSettings);
        }
        private T DeserializeObject<T>(string serializedObject)
        {
            return JsonConvert.DeserializeObject<T>(serializedObject
                , SerializerSettings);
        }
    }
}
