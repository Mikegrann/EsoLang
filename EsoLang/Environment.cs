using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace EsoLang
{
    [Serializable]
    public class Environment : ICloneable, ISerializable
    {
        [NonSerialized]
        private Dictionary<string, Val> globalDict;
        [NonSerialized]
        private Dictionary<string, Val> localDict;
        [NonSerialized]
        public int scopeDepth;
        
        public Environment()
        {
            globalDict = new Dictionary<string, Val>();
            localDict = new Dictionary<string, Val>();
            scopeDepth = 0;
        }

        public Environment(SerializationInfo info, StreamingContext context)
        {
            globalDict = new Dictionary<string, Val>();

            int envLength = (int)info.GetValue("globLength", typeof(int));
            for (int i = 0; i < envLength; i++)
            {
                globalDict.Add((string)info.GetValue("glob" + i + "key", typeof(string)),
                    (Val)info.GetValue("glob" + i + "val", typeof(Val)));
            }

            localDict = new Dictionary<string, Val>();

            envLength = (int)info.GetValue("locLength", typeof(int));
            for (int i = 0; i < envLength; i++)
            {
                globalDict.Add((string)info.GetValue("loc" + i + "key", typeof(string)),
                    (Val)info.GetValue("loc" + i + "val", typeof(Val)));
            }

            scopeDepth = 0;
        }

        public object Clone()
        {
            Environment result = new Environment();
            foreach (KeyValuePair<string, Val> pair in globalDict)
            {
                result.globalDict.Add(pair.Key, (Val)pair.Value.Clone());
            }
            foreach (KeyValuePair<string, Val> pair in localDict)
            {
                result.localDict.Add(pair.Key, (Val)pair.Value.Clone());
            }

            result.scopeDepth = this.scopeDepth;

            return result;
        }

        public Environment withLocals(Environment other)
        {
            Environment result = new Environment();
            result.globalDict = globalDict; // same instance of globals
            result.localDict = other.localDict; // other's locals
            result.scopeDepth = this.scopeDepth;
            return result;
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            int i = 0;
            foreach (KeyValuePair<string, Val> entry in globalDict)
            {
                info.AddValue("glob" + i + "key", entry.Key, typeof(string));
                info.AddValue("glob" + i + "val", entry.Value, typeof(Val));
                i++;
            }
            info.AddValue("globLength", i, typeof(int));

            i = 0;
            foreach (KeyValuePair<string, Val> entry in localDict)
            {
                info.AddValue("loc" + i + "key", entry.Key, typeof(string));
                info.AddValue("loc" + i + "val", entry.Value, typeof(Val));
                i++;
            }
            info.AddValue("locLength", i, typeof(int));
        }

        public bool ContainsKey(string key)
        {
            return globalDict.ContainsKey(key) || localDict.ContainsKey(key);
        }

        public Val this[string key]
        {
            get
            {
                if (scopeDepth == 0 && globalDict.ContainsKey(key)) // in outermost scope
                {
                    return globalDict[key];
                }
                else if (localDict.ContainsKey(key))
                {
                    return localDict[key];
                }
                return globalDict[key];
            }

            set
            {
                if (scopeDepth == 0) // in outermost scope
                {
                    globalDict[key] = value;
                }
                else if (localDict.ContainsKey(key)) // try editing inner scope
                {
                    localDict[key] = value;
                }
                else if (globalDict.ContainsKey(key)) // check to edit outer scope
                {
                    globalDict[key] = value;
                }
                else // only add to inner scope if exists nowhere
                {
                    localDict.Add(key, value);
                }
            }
        }

        public Val GetLocal(string key)
        {
            return localDict[key];
        }

        public Val GetGlobal(string key)
        {
            return globalDict[key];
        }

        public void SetLocal(string key, Val value)
        {
            localDict[key] = value;
        }

        public void SetGlobal(string key, Val value)
        {
            globalDict[key] = value;
        }
    }
}
