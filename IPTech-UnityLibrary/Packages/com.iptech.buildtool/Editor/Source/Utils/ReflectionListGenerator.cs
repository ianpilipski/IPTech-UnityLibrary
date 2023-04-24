
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace IPTech.BuildTool
{
    public class ReflectionListGenerator {
        static SortGenListByName sorter = new SortGenListByName();

        Dictionary<Type, GenList> lists;

        public ReflectionListGenerator(params Type[] types) {
            lists = new Dictionary<Type, GenList>();
            GenerateLists(types);
        }

        public IReadOnlyList<Type> GetListImmediate(Type type) {
            bool hasList = false;
            if(lists.TryGetValue(type, out GenList value)) {
                if(value.IsFinishedGenerating) {
                    return value;
                }
                hasList = true;
            }

            var ll = GenerateListsImmediate(new Type[] { type });
            var retVal = ll[type];
            if(!hasList) {
                lists[type] = retVal;
            }
            return retVal;
        }

        public async void GetList(Type type, Action<IReadOnlyList<Type>> list) {
            try {
                if(!lists.TryGetValue(type, out GenList value)) {
                    GenerateLists(new Type[] { type });
                    value = lists[type];
                }

                while(!value.IsFinishedGenerating) {
                    await Task.Yield();
                }

                list(value);
            } catch(Exception e) {
                Debug.LogException(e);
            }
        }

        async void GenerateLists(Type[] types) {
            try {
                for(int ii=0;ii<types.Length;ii++) {
                    lists.Add(types[ii], new GenList());
                }

                Dictionary<Type,GenList> retVal = null;
                await Task.Factory.StartNew(() => {
                     retVal = GenerateListsImmediate(types);
                });

                for(int i=0;i<types.Length;i++) {
                    var t = types[i];
                    var l = lists[t];
                    l.AddRange(retVal[t]);
                    l.Sort(sorter);
                    l.IsFinishedGenerating = true;
                }
            } catch(Exception e) {
                Debug.LogException(e);
            }
        }

        static Dictionary<Type,GenList> GenerateListsImmediate(Type[] types) {
            Dictionary<Type,GenList> lists = new Dictionary<Type, GenList>();
            for(int ii=0;ii<types.Length;ii++) {
                lists[types[ii]] = new GenList();
                lists[types[ii]].IsFinishedGenerating = true;
            }

            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach(var asm in assemblies) {
                var asmTypes = asm.GetTypes();
                for(int i=0;i<asmTypes.Length;i++) {    
                    var asmT = asmTypes[i];
                    if(!asmT.IsAbstract) {
                        for(int j=0;j<types.Length;j++) {
                            var targetType = types[j];
                            if(asmT.IsSubclassOf(targetType)) {
                                lists[targetType].Add(asmT);
                            }
                        }
                    }
                }
            }
        
            return lists;
        }

        

        public class GenList : List<Type> {
            public bool IsFinishedGenerating;
        }

        public class SortGenListByName : IComparer<Type> {
            public int Compare(Type x, Type y) {
                return x.Name.CompareTo(y.Name);
            }
        }
    }
}