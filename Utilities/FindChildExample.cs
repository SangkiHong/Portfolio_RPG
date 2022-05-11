using System.Collections;
using UnityEngine;

namespace SK.Assets.Scripts.Utilities
{
    public class FindChildExample : MonoBehaviour
    {
        // 이름으로 자식 오브젝트 찾기
        public Transform FindChildByName(string findNodeName, Transform root)
        {
            if (root.name == findNodeName)
                return root;

            for (int i = 0; i < root.childCount; i++)
            {
                Transform findNode = FindChildByName(findNodeName, root.GetChild(i));
                if (findNode != null)
                    return findNode;
            }
            return null;
        }
    }
}