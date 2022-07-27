using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.Linq;

namespace SK.Utilities
{
    [System.Serializable]
    //[CanEditMultipleObjects]
    //[ExecuteInEditMode]
    public class SerializableDictionary<TKey, TValue> : Dictionary<TKey, TValue>, ISerializationCallbackReceiver
    {
        public List<TKey> inspectorKeys;
        public List<TValue> inspectorValues;

        public SerializableDictionary()
        {
            inspectorKeys = new List<TKey>();
            inspectorValues = new List<TValue>();
            SyncInspectorFromDictionary();
        }

        /// <summary>
        /// 새로운 KeyValuePair을 추가하며, 인스펙터도 업데이트
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public new void Add(TKey key, TValue value)
        {
            base.Add(key, value);
            SyncInspectorFromDictionary();
        }

        /// <summary>
        /// KeyValuePair을 삭제하며, 인스펙터도 업데이트
        /// </summary>
        /// <param name="key"></param>
        public new void Remove(TKey key)
        {
            base.Remove(key);
            SyncInspectorFromDictionary();
        }

        /// <summary>
        /// 인스펙터를 딕셔너리로 초기화
        /// </summary>
        public void SyncInspectorFromDictionary()
        {
            //인스펙터 키 밸류 리스트 초기화
            inspectorKeys.Clear();
            inspectorValues.Clear();

            foreach (KeyValuePair<TKey, TValue> pair in this)
            {
                inspectorKeys.Add(pair.Key); inspectorValues.Add(pair.Value);
            }
        }

        /// <summary>
        /// 딕셔너리를 인스펙터로 초기화
        /// </summary>
        public void SyncDictionaryFromInspector()
        {
            //딕셔너리 키 밸류 리스트 초기화
            this.Clear();

            for (int i = 0; i < inspectorKeys.Count; i++)
            {
                //중복된 키가 있다면 에러 출력
                if (this.ContainsKey(inspectorKeys[i]))
                {
                    Debug.LogError("중복된 키가 있습니다.");
                    break;
                }
                base.Add(inspectorKeys[i], inspectorValues[i]);
            }
        }

        public void OnBeforeSerialize()
        {
            SyncInspectorFromDictionary();
        }

        public void OnAfterDeserialize()
        {
            //인스펙터의 Key Value가 KeyValuePair 형태를 띌 경우
            if (inspectorKeys.Count == inspectorValues.Count)
            {
                SyncDictionaryFromInspector();
            }
        }
    }
}