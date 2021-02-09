using Neo.Ledger;
using Neo.SmartContract;
using Neo.VM;
using System.Linq;

namespace Neo
{
    public class BolScriptUtility
    {
        public static byte[] _bolScript = null;
        public static byte[] BolScript
        {
            get
            {
                if (_bolScript == null)
                {
                    var bolSettings = ProtocolSettings.Default.BolSettings;
                    byte[] script = System.IO.File.ReadAllBytes(bolSettings.Path);
                    byte[] parameter_list = "0710".HexToBytes();
                    var return_type = ContractParameterType.ByteArray;
                    var properties = ContractPropertyState.HasStorage;

                    using (var sb = new ScriptBuilder())
                    {
                        sb.EmitSysCall(
                            "Neo.Contract.Create",
                            script, parameter_list,
                            return_type, properties,
                            bolSettings.Name,
                            bolSettings.Version,
                            bolSettings.Author,
                            bolSettings.Email,
                            bolSettings.Description
                        );
                        _bolScript = sb.ToArray();
                    }
                }
                return _bolScript;
            }
        }

        public static byte[] _bolScriptHash = null;
        public static byte[] BolScriptHash
        {
            get
            {
                if (_bolScriptHash == null)
                {
                    _bolScriptHash = ProtocolSettings.Default
                        .BolSettings
                        .ScriptHash
                        .HexToBytes()
                        .Reverse()
                        .ToArray();
                }
                return _bolScriptHash;
            }
        }
        
        public static bool IsBolInvocation(byte[] script)
        {
            var currentScriptEnd = script.Skip(script.Length - 20).ToArray();
            return currentScriptEnd.SequenceEqual(BolScriptHash) || script.SequenceEqual(BolScript);
        }
    }
}
