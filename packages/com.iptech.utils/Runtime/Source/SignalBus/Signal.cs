

namespace IPTech.Utils {
    public abstract class Signal : ISignal {
        public bool Sticky { get; protected set; }

        public Signal() : this(false) { }

        public Signal(bool sticky) {
            this.Sticky = sticky;
        }
    }
}
