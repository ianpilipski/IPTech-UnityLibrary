namespace IPTech.Audio.EditorExtensions {
    using IPTech.Audio;
    using System;
    
    
    public partial class AudioConfigGenerator : AudioConfigGeneratorBase {
        
        public virtual string TransformText() {
            this.GenerationEnvironment = null;

            this.WriteLine("");

            this.WriteLine("namespace IPTech.Audio {");
            using(new TabIndent(this)) {
                this.WriteLine("");
                this.WriteLine("");
                this.WriteLine($"public partial class {this.ToStringHelper.ToStringWithCulture(className)} {{");
                using(new TabIndent(this)) {
                    this.WriteLine("AudioEngine audioEngine;");
                    this.WriteLine("AudioEngineConfig config;");
                    this.WriteLine("");
                    this.WriteLine("");
                    // constructor
                    this.WriteLine($"public {this.ToStringHelper.ToStringWithCulture(className)}(AudioEngine audioEngine, AudioEngineConfig config) {{");
                    using(new TabIndent(this)) {
                        this.WriteLine("this.audioEngine = audioEngine;");
                        this.WriteLine("this.config = config;");
                    }
                    this.WriteLine("}");
                    
                    for(int i = 0; i < config.AudioClips.Count; i++) {
                        AudioClipsCollection acc = config.AudioClips[i];
                        this.WriteLine($"public AudioHandle Play{this.ToStringHelper.ToStringWithCulture(MakeSafeCodeName(acc.name))}(bool looping = false) {{");
                        using(new TabIndent(this)) {
                            this.WriteLine($"return audioEngine.Play(config.AudioClips[{this.ToStringHelper.ToStringWithCulture(i)}], looping);");
                        }
                        this.WriteLine("}");
                    }

                    this.WriteLine("");
                    this.WriteLine("");

                    this.WriteLine("public void Stop(AudioHandle audioHandle) {");
                    using(new TabIndent(this)) {
                        this.WriteLine("audioEngine.Stop(audioHandle);");
                    }
                    this.WriteLine("}");
                }
                this.WriteLine("}");
            }
            this.WriteLine("}");
            
            return this.GenerationEnvironment.ToString();
        }
        
        public virtual void Initialize() {
        }
    }
    
    public class AudioConfigGeneratorBase {
        
        private global::System.Text.StringBuilder builder;
        
        private global::System.Collections.Generic.IDictionary<string, object> session;
        
        private global::System.CodeDom.Compiler.CompilerErrorCollection errors;
        
        private string currentIndent = string.Empty;
        
        private global::System.Collections.Generic.Stack<int> indents;
        
        private ToStringInstanceHelper _toStringHelper = new ToStringInstanceHelper();
        
        public virtual global::System.Collections.Generic.IDictionary<string, object> Session {
            get {
                return this.session;
            }
            set {
                this.session = value;
            }
        }
        
        public global::System.Text.StringBuilder GenerationEnvironment {
            get {
                if ((this.builder == null)) {
                    this.builder = new global::System.Text.StringBuilder();
                }
                return this.builder;
            }
            set {
                this.builder = value;
            }
        }
        
        protected global::System.CodeDom.Compiler.CompilerErrorCollection Errors {
            get {
                if ((this.errors == null)) {
                    this.errors = new global::System.CodeDom.Compiler.CompilerErrorCollection();
                }
                return this.errors;
            }
        }
        
        public string CurrentIndent {
            get {
                return this.currentIndent;
            }
        }
        
        private global::System.Collections.Generic.Stack<int> Indents {
            get {
                if ((this.indents == null)) {
                    this.indents = new global::System.Collections.Generic.Stack<int>();
                }
                return this.indents;
            }
        }
        
        public ToStringInstanceHelper ToStringHelper {
            get {
                return this._toStringHelper;
            }
        }
        
        public void Error(string message) {
            this.Errors.Add(new global::System.CodeDom.Compiler.CompilerError(null, -1, -1, null, message));
        }
        
        public void Warning(string message) {
            global::System.CodeDom.Compiler.CompilerError val = new global::System.CodeDom.Compiler.CompilerError(null, -1, -1, null, message);
            val.IsWarning = true;
            this.Errors.Add(val);
        }
        
        public string PopIndent() {
            if ((this.Indents.Count == 0)) {
                return string.Empty;
            }
            int lastPos = (this.currentIndent.Length - this.Indents.Pop());
            string last = this.currentIndent.Substring(lastPos);
            this.currentIndent = this.currentIndent.Substring(0, lastPos);
            return last;
        }
        
        public void PushIndent(string indent) {
            this.Indents.Push(indent.Length);
            this.currentIndent = (this.currentIndent + indent);
        }
        
        public void ClearIndent() {
            this.currentIndent = string.Empty;
            this.Indents.Clear();
        }
        
        public void Write(string textToAppend) {
            this.GenerationEnvironment.Append(textToAppend);
        }
        
        public void Write(string format, params object[] args) {
            this.GenerationEnvironment.AppendFormat(format, args);
        }
        
        public void WriteLine(string textToAppend) {
            this.GenerationEnvironment.Append(this.currentIndent);
            this.GenerationEnvironment.AppendLine(textToAppend);
        }
        
        public void WriteLine(string format, params object[] args) {
            this.GenerationEnvironment.Append(this.currentIndent);
            this.GenerationEnvironment.AppendFormat(format, args);
            this.GenerationEnvironment.AppendLine();
        }

        public class TabIndent : IDisposable {
            readonly AudioConfigGeneratorBase _gen;
            public TabIndent(AudioConfigGeneratorBase gen) {
                _gen = gen;
                _gen.PushIndent("\t");
            }

            public void Dispose() {
                _gen.PopIndent();
            }
        }
        
        public class ToStringInstanceHelper {
            
            private global::System.IFormatProvider formatProvider = global::System.Globalization.CultureInfo.InvariantCulture;
            
            public global::System.IFormatProvider FormatProvider {
                get {
                    return this.formatProvider;
                }
                set {
                    if ((value != null)) {
                        this.formatProvider = value;
                    }
                }
            }
            
            public string ToStringWithCulture(object objectToConvert) {
                if ((objectToConvert == null)) {
                    throw new global::System.ArgumentNullException("objectToConvert");
                }
                global::System.Type type = objectToConvert.GetType();
                global::System.Type iConvertibleType = typeof(global::System.IConvertible);
                if (iConvertibleType.IsAssignableFrom(type)) {
                    return ((global::System.IConvertible)(objectToConvert)).ToString(this.formatProvider);
                }
                global::System.Reflection.MethodInfo methInfo = type.GetMethod("ToString", new global::System.Type[] {
                            iConvertibleType});
                if ((methInfo != null)) {
                    return ((string)(methInfo.Invoke(objectToConvert, new object[] {
                                this.formatProvider})));
                }
                return objectToConvert.ToString();
            }
        }
    }
}
