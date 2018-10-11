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

        public long DeleteAll<T>()
        {
            var server = Connection.GetServer(_connectionString.Split(',')[0]);
            if (!server.IsConnected) return -1;

            var keys = server.Keys(Db.Database, pattern: GetKey(typeof(T), "*").ToString()).ToArray();
            if (!IsConnected) return -1;
            var deletedItems = Db.KeyDelete(keys);
            return deletedItems;
        }

        public bool Delete<TKey>(TKey id, Type type)
        {
            if (!IsConnected) return false;
            var key = GetKey(type, id);
            return !Db.KeyExists(key) || Db.KeyDelete(key);
        }

        private IEnumerable<KeyValuePair<TKey, RedisKey>> GetKeys<TKey>(Type type, IEnumerable<TKey> ids)
        {
            return GetKeys(type.FullName, ids);
        }

        private RedisKey GetKey<TKey>(Type type, TKey id)
        {
            return GetKey(type.FullName, id);
        }
        private RedisKey GetKey<TKey>(string typeName, TKey id)
        {
            return $"{typeName}:{id}";
        }
        private IEnumerable<KeyValuePair<TKey, RedisKey>> GetKeys<TKey>(string typeName, IEnumerable<TKey> ids)
        {
            foreach (var id in ids)
            {
                yield return new KeyValuePair<TKey, RedisKey>(id, $"{typeName}:{id}");
            }
        }

        private bool Set<TKey, T>(TKey id, T item)
        {
            var key = GetKey(typeof(T), id);
            return Set(key, item);
        }

        private bool Set<T>(RedisKey key, T item)
        {
            if (!IsConnected) return false;
            var serializedItem = SerializeObject(item);
            return Db.StringSet(key, serializedItem);
        }

        private bool SetMany<TKey, T>(IEnumerable<T> items, Func<T, TKey> idSelector)
        {
            if (!IsConnected) return false;
            var list = new List<KeyValuePair<RedisKey, RedisValue>>();
            var typeName = typeof(T).FullName;
            foreach (var item in items)
            {
                list.Add(new KeyValuePair<RedisKey, RedisValue>(GetKey(typeName, idSelector(item)), SerializeObject(item)));
            }
            return Db.StringSet(list.ToArray());
        }

        public void AddOrUpdate<TKey, T>(TKey id, T item)
        {
            if (id == null)
                throw new ArgumentNullException(nameof(id));
            if (item == null)
                throw new ArgumentNullException(nameof(item));

            if (!IsConnected) return;

            var key = GetKey(typeof(T).FullName, id);
            var storedVal = SerializeObject(item);
            Db.StringSet(key, storedVal);
        }

        public T GetOrAdd<TKey, T>(TKey id, Func<TKey, T> valueFactory)
        {
            if (id == null)
                throw new ArgumentNullException(nameof(id));
            if (valueFactory == null)
                throw new ArgumentNullException(nameof(valueFactory));

            if (!IsConnected) return valueFactory(id);

            var key = GetKey(typeof(T), id);

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
            Set(id, newVal);
            return newVal;
        }

        public async Task<ICollection<T>> GetOrAdd<TKey, T>(IEnumerable<TKey> ids, Func<IEnumerable<TKey>, Task<ICollection<T>>> valueFactory, Func<T, TKey> idSelector)
        {
            var ret = new List<T>();
            if (ids == null)
                throw new ArgumentNullException(nameof(ids));

            if (valueFactory == null)
                throw new ArgumentNullException(nameof(valueFactory));

            if (idSelector == null)
                throw new ArgumentNullException(nameof(idSelector));

            if (!IsConnected)
            {
                foreach (var retValue in await valueFactory(ids))
                {
                    ret.Add(retValue);
                }
                return ret;
            }

            var keys = GetKeys(typeof(T), ids).ToDictionary(_ => _.Key, _ => _.Value);
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
                        break;
                    }
                    if (element != null && !element.Equals(default(T)))
                        ret.Add(element);
                    else
                        missingIds.Add(keys.ElementAt(i).Key);
                }
                else
                    missingIds.Add(keys.ElementAt(i).Key);
            }
            if (missingIds.Any())
            {
                var list = new List<T>();
                foreach (var missingElement in await valueFactory(missingIds))
                {
                    list.Add(missingElement);
                    ret.Add(missingElement);
                }
                SetMany(list, idSelector);
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
