using System;
using System.Collections.Generic;
using System.Linq;

namespace Wax
{
    public class JsonBasedObject
    {
        private Dictionary<string, object> data;
        public Dictionary<string, object> RawData { get { return this.data; } }

        public JsonBasedObject() : this(null) { }

        public JsonBasedObject(IDictionary<string, object> nullableStartingValues)
        {
            this.data = nullableStartingValues == null ? new Dictionary<string, object>() : new Dictionary<string, object>(nullableStartingValues);
        }

        private object GetValue(string key)
        {
            if (this.data.ContainsKey(key)) return this.data[key];
            return null;
        }

        protected string GetString(string key)
        {
            object value = this.GetValue(key);
            if (value == null) return null;
            return value.ToString();
        }

        protected bool GetBoolean(string key)
        {
            object value = this.GetValue(key);
            if (value == null) return false;
            if (value is int) return (int)value != 0;
            if (value is string) return (string)value != "";
            if (value is bool) return (bool)value;
            return false;
        }

        protected int GetInteger(string key)
        {
            object value = this.GetValue(key);
            if (value == null) return 0;
            if (value is int) return (int)value;
            if (value is string)
            {
                int n;
                if (int.TryParse((string)value, out n)) return n;
                return 0;
            }
            if (value is bool) return (bool)value ? 1 : 0;
            return 0;
        }

        protected string[] GetStrings(string key)
        {
            object value = this.GetValue(key);
            if (value is string[]) return new List<string>((string[])value).ToArray();
            if (value is IList<string>) return ((IList<string>)value).ToArray();
            return null;
        }

        private JsonBasedObject GetObject(string key)
        {
            object value = this.GetValue(key);
            if (value == null) return null;
            if (value is JsonBasedObject) return (JsonBasedObject)value;
            if (value is Dictionary<string, object>) return new JsonBasedObject((Dictionary<string, object>)value);
            return null;
        }

        protected T GetObjectAsType<T>(string key)
        {
            JsonBasedObject value = this.GetObject(key);
            if (value == null) return (T)(object)null;
            T typedValue = ConvertType<T>(value);
            this.SetObject(key, (JsonBasedObject)(object)typedValue);
            return typedValue;
        }

        private static T ConvertType<T>(JsonBasedObject value)
        {
            if (value is T) return (T)(object)value;
            JsonBasedObject valueAsType = (JsonBasedObject)typeof(T).GetConstructor(new Type[] { typeof(IDictionary<string, object>) }).Invoke(new object[] { value.RawData });
            return (T)(object)valueAsType;
        }

        protected void SetObject(string key, JsonBasedObject value)
        {
            this.SetValue(key, value);
        }

        protected bool HasObjects(string key)
        {
            JsonBasedObject[] objects = this.GetObjects(key);
            return objects != null && objects.Length > 0;
        }

        private JsonBasedObject[] GetObjects(string key)
        {
            object value = this.GetValue(key);
            if (value == null) return null;
            if (value is object[])
            {
                return ((object[])value).Select(o =>
                {
                    if (o == null) return null;
                    if (o is Dictionary<string, object>) return new JsonBasedObject((Dictionary<string, object>)o);
                    if (o is JsonBasedObject) return (JsonBasedObject)o;
                    return null;
                }).ToArray();
            }
            if (value is JsonBasedObject[]) return (JsonBasedObject[])value;
            if (value is Dictionary<string, object>[])
            {
                return ((Dictionary<string, object>[])value).Select(d => new JsonBasedObject(d)).ToArray();
            }
            return null;
        }

        protected T[] GetObjectsAsType<T>(string key)
        {
            JsonBasedObject[] values = this.GetObjects(key);
            if (values == null) return null;
            T[] typedValues = values.Select(jbo => ConvertType<T>(jbo)).ToArray();
            this.SetObjects(key, typedValues.Select(v => (JsonBasedObject)(object)v).ToArray());
            return typedValues;
        }

        protected void ClearValue(string key)
        {
            if (this.data.ContainsKey(key))
            {
                this.data.Remove(key);
            }
        }

        protected void SetObjects(string key, IEnumerable<JsonBasedObject> items)
        {
            if (items == null) this.ClearValue(key);
            else this.SetValue(key, items.ToArray());
        }

        protected void SetStrings(string key, IList<string> values)
        {
            if (values == null) this.ClearValue(key);
            else this.data[key] = values.ToArray();
        }

        protected void SetBoolean(string key, bool value)
        {
            this.SetValue(key, value);
        }

        protected void SetString(string key, string value)
        {
            if (value == null) this.ClearValue(key);
            else this.SetValue(key, value);
        }

        protected void SetInteger(string key, int value)
        {
            this.SetValue(key, value);
        }

        protected Dictionary<string, JsonBasedObject> GetDictionary(string key)
        {
            object value = this.GetValue(key);
            if (value == null) return null;
            if (value is Dictionary<string, JsonBasedObject>) return (Dictionary<string, JsonBasedObject>)value;
            return null;
        }

        protected void SetDictionary(string key, Dictionary<string, JsonBasedObject> value)
        {
            if (value == null) this.ClearValue(key);
            else this.SetValue(key, value);
        }

        private void SetValue(string key, object value)
        {
            if (value == null) this.ClearValue(key);
            else this.data[key] = value;
        }

        private void ItemToJson(System.Text.StringBuilder sb, object value)
        {
            if (value is bool)
            {
                sb.Append((bool)value ? "true" : "false");
            }
            else if (value is int)
            {
                sb.Append((int)value);
            }
            else if (value is string)
            {
                sb.Append('"');
                sb.Append(((string)value).Replace("\\", "\\\\").Replace("\"", "\\\""));
                sb.Append('"');
            }
            else if (value is string[])
            {
                string[] strings = (string[])value;
                sb.Append('[');
                for (int i = 0; i < strings.Length; i++)
                {
                    if (i > 0) sb.Append(',');
                    this.ItemToJson(sb, strings[i]);
                }
                sb.Append(']');
            }
            else if (value is JsonBasedObject)
            {
                ((JsonBasedObject)value).ToJsonImpl(sb);
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        internal void ToJsonImpl(System.Text.StringBuilder sb)
        {
            sb.Append('{');
            string[] keys = this.data.Keys.ToArray();
            bool isFirst = true;
            for (int i = 0; i < keys.Length; i++)
            {
                object value = this.GetValue(keys[i]);
                if (value == null) continue;

                if (isFirst) isFirst = false;
                else sb.Append(',');

                sb.Append('"');
                sb.Append(keys[i]);
                sb.Append("\":");
                this.ItemToJson(sb, value);
            }
            sb.Append('}');
        }

        public string ToJson()
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            this.ToJsonImpl(sb);
            return sb.ToString();
        }
    }
}
