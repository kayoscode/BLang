using System.Diagnostics;

namespace BLang.Semantics
{
    public class SymbolTable
    {
        public SymbolTable()
        {
        }

        public bool SymbolExists(string identifier)
        {
            return mSymbols.ContainsKey(identifier);
        }

        public void RegisterSymbol(string identifier)
        {
            Trace.Assert(!SymbolExists(identifier));
            SymbolInfo info = new();
            mSymbols.Add(identifier, info);
        }

        SymbolInfo GetSymbolInfo(string identifier)
        {
            Trace.Assert(SymbolExists(identifier));
            return mSymbols[identifier];
        }

        private readonly Dictionary<string, SymbolInfo> mSymbols = new();
    }
}
