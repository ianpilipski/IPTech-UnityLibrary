#if !(UNITY_EDITOR || DEVELOPMENT_BUILD || QA_BUILD)
#define CONTAINERDEBUGSERVICE_DISABLED
#endif

#if !CONTAINERDEBUGSERVICE_DISABLED

using System;
using System.Collections.Generic;
using System.Linq;

namespace IPTech.DebugConsoleService
{
    public enum MemoryDataStatus {
        INDETERMINANT,
        CALCULATED,
    }

    public abstract class MemoryData {
        public abstract string Name { get; }
        public abstract MemoryDataStatus Status { get; }
        public abstract long SizeInBytes { get; }
        public abstract float SizeInMegabytes { get; }

        public override string ToString() {
            return string.Format("[MemoryData: Name={0}, SizeInBytes={1}, SizeInMegabytes={2}, Status={3}]", Name, SizeInBytes, SizeInMegabytes, Status);
        }
    }

    public abstract class MemoryDataCollection : MemoryData, IEnumerable<MemoryData> {
        public override long SizeInBytes {
            get {
                return this.Sum( item => item.SizeInBytes );
            }
        }

        public override float SizeInMegabytes {
            get {
                return this.Sum( item => item.SizeInMegabytes );
            }
        }

        public override MemoryDataStatus Status {
            get {
                return this.Any( item => item.Status == MemoryDataStatus.INDETERMINANT ) ? MemoryDataStatus.INDETERMINANT : MemoryDataStatus.CALCULATED;
            }
        }
        #region IEnumerable implementation

        public abstract IEnumerator<MemoryData> GetEnumerator();

        #endregion

        #region IEnumerable implementation

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }

        #endregion
    }
}

#endif
