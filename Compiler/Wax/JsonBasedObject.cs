using System;
using System.Collections.Generic;
using System.Linq;

namespace Wax
{
    public abstract class JsonBasedObject
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

        protected JsonBasedObject GetObject(string key)
        {
            object value = this.GetValue(key);
            if (value == null) return null;
            if (value is JsonBasedObject) return (JsonBasedObject)value;
            return null;
        }

        protected void SetObject(string key, JsonBasedObject value)
        {
            this.SetValue(key, value);
        }

        protected JsonBasedObject[] GetObjects(string key)
        {
            object value = this.GetValue(key);
            if (value == null) return null;
            if (value is JsonBasedObject[]) return (JsonBasedObject[])value;
            return null;
        }

        protected void SetObjects(string key, IEnumerable<JsonBasedObject> items)
        {
            this.SetValue(key, items.ToArray());
        }

        protected void SetStrings(string key, IList<string> values)
        {
            this.data[key] = values.ToArray();
        }

        protected void SetBoolean(string key, bool value)
        {
            this.SetValue(key, value);
        }

        protected void SetString(string key, string value)
        {
            this.SetValue(key, value);
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
            this.SetValue(key, value);
        }

        private void SetValue(string key, object value)
        {
            this.data[key] = value;
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
