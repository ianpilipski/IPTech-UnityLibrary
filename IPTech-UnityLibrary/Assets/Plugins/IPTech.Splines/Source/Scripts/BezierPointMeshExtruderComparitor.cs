using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace IPTech.Splines {
    class BezierPointMeshExtruderComparitor : Comparitor<BezierPointMeshExtruderComparitorInfo> {
        [SerializeField]
        private BezierPointMeshExtruderComparitorInfo Result;

        public override bool UpdateComparitor(BezierPointMeshExtruderComparitorInfo info) {
            if (!info.Equals(Result)) {
                Result = info.Clone();
#if UNITY_EDITOR
                UnityEditor.EditorUtility.SetDirty(this);
#endif
                return true;
            }

            return false;
        }
    }

    [Serializable]
    public class BezierPointMeshExtruderComparitorInfo {

        public BezierPointComparitorInfo PointComparitorInfo;

        public float ExtrusionWidth { get; set; }

        public bool Equals(BezierPointMeshExtruderComparitorInfo info) {
            if (info == null) {
                return false;
            }

            return (this.ExtrusionWidth == info.ExtrusionWidth) && this.PointComparitorInfo.Equals(info.PointComparitorInfo);
        }

        public BezierPointMeshExtruderComparitorInfo Clone() {
            BezierPointMeshExtruderComparitorInfo bpci = (BezierPointMeshExtruderComparitorInfo)this.MemberwiseClone();
            bpci.PointComparitorInfo = bpci.PointComparitorInfo.Clone();
            return bpci;
        }
    }
}
