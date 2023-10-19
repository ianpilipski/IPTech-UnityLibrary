

namespace IPTech.SlotEngine.Unity.Model.Editor {
    using Api;
    using SlotEngine.Model.Api;
    using SlotEngine.Api;
    using IPTech.SlotEngine.Model;

    public class Context {
        static Context _inst;

        public static Context Inst {
            get {
                if(_inst==null) {
                    _inst = new Context();
                }
                return _inst;
            }
        }

        public IPaylineEntryEditor PaylineEntryEditor { get; private set; }
        public ObjectToEditorMap<IPayline, IPaylineEditor> PaylineEditorMap { get; private set; }
        public ObjectToEditorMap<IPayoutTableEntry, IPayoutTableEntryEditor> PayoutTableEntryEditorMap { get; private set;}
        public ObjectToEditorMap<IReel,IReelEditor> ReelEditorMap { get; private set; }
        public ObjectToEditorMap<IWildSymbol,IWildSymbolEditor> WildSymbolEditorMap { get; private set; }
        public ISymbolEditor SymbolEditor { get; private set; }
		public IReelSetEditor reelSetEditor { get; set; }
		public IPaylineSetEditor paylineSetEditor { get; set; }
		public IPayoutTableEditor payoutTableEditor { get; set; }
		public IWildSymbolSetEditor wildSymbolSetEditor { get; set; }
		public ISlotEngine slotEngine { get; set; }
		public ISlotEngineSimulator slotEngineSimulator { get; set; }
        public ISlotModelEditor SlotModelEditor { get; set; }

        public Context() {
            PaylineEntryEditor = new PaylineEntryEditor();
            PaylineEditorMap = new ObjectToEditorMap<IPayline, IPaylineEditor>(typeof(PaylineEditor));
            PayoutTableEntryEditorMap = new ObjectToEditorMap<IPayoutTableEntry, IPayoutTableEntryEditor>(typeof(PayoutTableEntryEditor));
            ReelEditorMap = new ObjectToEditorMap<IReel, IReelEditor>(typeof(ReelEditor));
            WildSymbolEditorMap = new ObjectToEditorMap<IWildSymbol, IWildSymbolEditor>(typeof(WildSymbolEditor));
            SymbolEditor = new SymbolEditor();
            reelSetEditor = new ReelSetEditor();
            paylineSetEditor = new PaylineSetEditor();
            payoutTableEditor = new PayoutTableEditor();
            wildSymbolSetEditor = new WildSymbolSetEditor();
            slotEngine = new SlotEngineBasic();
            slotEngineSimulator = new SlotEngineSimulator();
            SlotModelEditor = new SlotModelEditor();
        }
    }
}
