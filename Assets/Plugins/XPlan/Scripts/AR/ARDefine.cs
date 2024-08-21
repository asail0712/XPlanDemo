using System;
using System.Collections.Generic;
using UnityEngine;

using XPlan.Observe;

namespace XPlan.AR
{
    [Serializable]
    public class ModelInfo
    {
        [SerializeField] public string imgKey;
        [SerializeField] public GameObject arPrefab;
    }

    public class ARModelSpawnMsg : MessageBase
	{
        public string imgKey;
        public Vector3 spawnPos;

        public ARModelSpawnMsg(string imgKey, Vector3 spawnPos)
		{
            this.imgKey     = imgKey;
            this.spawnPos   = spawnPos;
        }
    }

    public class ARModelTrackMsg : MessageBase
    {
        public string imgKey;
        public bool bOn;
        public Vector3 trackPos;

        public ARModelTrackMsg(string imgKey, bool bOn, Vector3 trackPos)
        {
            this.imgKey     = imgKey;
            this.bOn        = bOn;
            this.trackPos   = trackPos;
        }

        public static bool operator ==(ARModelTrackMsg m1, ARModelTrackMsg m2)
        {
            // 如果兩個對象的引用相同，則返回 true
            if (ReferenceEquals(m1, m2))
            {
                return true;
            }

            // 如果其中一個對象為 null，則返回 false
            if (ReferenceEquals(m1, null) || ReferenceEquals(m2, null))
            {
                return false;
            }

            // 比較 X 和 Y 屬性值
            return m1.imgKey == m2.imgKey && m1.bOn == m2.bOn && m1.trackPos == m2.trackPos;
        }

        // 重載 != 運算符
        public static bool operator !=(ARModelTrackMsg p1, ARModelTrackMsg p2)
        {
            return !(p1 == p2);
        }

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            ARModelTrackMsg p = (ARModelTrackMsg)obj;
            return imgKey == p.imgKey && bOn == p.bOn && trackPos == p.trackPos;
        }

        // 覆寫 GetHashCode 方法
        public override int GetHashCode()
        {
            return HashCode.Combine(imgKey, bOn, trackPos);
        }
    }

    public class ARPlaneMsg : MessageBase
    {
        public Vector3 pos;
        public bool bFind;

        public ARPlaneMsg(bool bFind, Vector3 pos)
        {
            this.pos    = pos;
            this.bFind  = bFind;
        }
    }
}
