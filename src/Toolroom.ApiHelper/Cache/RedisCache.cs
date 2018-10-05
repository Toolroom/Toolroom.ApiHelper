using System;
using System.Linq;
using Newtonsoft.Json;
using StackExchange.Redis;

namespace Toolroom.ApiHelper
{
    public class RedisCache
    {
        private readonly string _connectionString;
        private readonly Lazy<ConnectionMultiplexer> _lazyConnection;

        public RedisCache(string connectionString)
        {
            _connectionString = connectionString;
            _lazyConnection = new Lazy<ConnectionMultiplexer>(() => ConnectionMultiplexer.Connect(_connectionString));
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

        public bool DeleteAll<T>()
        {
            var server = Connection.GetServer(_connectionString.Split(',')[0]);
            if (!server.IsConnected) return false;

            var keys = server.Keys(Db.Database, pattern: GetKey(typeof(T), "*")).ToArray();
            if (!IsConnected) return false;
            var deletedItems = Db.KeyDelete(keys);
            return true;
        }

        public bool Delete<TKey, T>(TKey id, T item)
        {
            if (!IsConnected) return false;
            var key = GetKey(typeof(T), id);
            return !Db.KeyExists(key) || Db.KeyDelete(key);
        }

        private string GetKey<TKey>(Type type, TKey id)
        {
            return GetKey(type.FullName, id);
        }
        private string GetKey<TKey>(string typeName, TKey id)
        {
            return $"{typeName}:{id}";
        }

        public bool Set<TKey, T>(TKey id, T item)
        {
            var key = GetKey(typeof(T), item);
            return Set(key, item);
        }

        private bool Set<T>(string key, T item)
        {
            if (!IsConnected) return false;
            var serializedItem = SerializeObject(item);
            return Db.StringSet(key, serializedItem);
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
                    return DeserializeObject<T>(storedVal);
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

        private string SerializeObject(object objectToCache)
        {
            return JsonConvert.SerializeObject(objectToCache
                , Formatting.Indented
                , new JsonSerializerSettings
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Serialize,
                    PreserveReferencesHandling = PreserveReferencesHandling.Objects,
                    TypeNameHandling = TypeNameHandling.All
                });
        }
        private T DeserializeObject<T>(string serializedObject)
        {
            return JsonConvert.DeserializeObject<T>(serializedObject
                , new JsonSerializerSettings
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Serialize,
                    PreserveReferencesHandling = PreserveReferencesHandling.Objects,
                    TypeNameHandling = TypeNameHandling.All
                });
        }
    }
}
