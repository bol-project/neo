using Neo.Ledger;
using Neo.Network.P2P.Payloads;
using Neo.SmartContract;
using Neo.VM;
using System.Linq;

namespace Neo
{
    public class BolScriptUtility
    {
        public static InvocationTransaction BolToken => new InvocationTransaction
        {
            Version = 1,
            Script = BolScript,
            Gas = Fixed8.Zero,
            Attributes = new TransactionAttribute[0],
            Inputs = new CoinReference[0],
            Outputs = new TransactionOutput[0],
            Witnesses = new Witness[0]
        };

        public static InvocationTransaction BolDeploy => new InvocationTransaction
        {
            Version = 1,
            Script = BolDeployScript,
            Gas = Fixed8.Zero,
            Attributes = new TransactionAttribute[0],
            Inputs = new CoinReference[0],
            Outputs = new TransactionOutput[0],
            Witnesses = new Witness[0]
        };

        private static byte[] s_bolScript = null;
        public static byte[] BolScript
        {
            get
            {
                if (s_bolScript == null)
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
                        s_bolScript = sb.ToArray();
                    }
                }
                return s_bolScript;
            }
        }

        private static byte[] s_bolScriptHash = null;
        public static byte[] BolScriptHash
        {
            get
            {
                if (s_bolScriptHash == null)
                {
                    s_bolScriptHash = ProtocolSettings.Default
                        .BolSettings
                        .ScriptHash
                        .HexToBytes()
                        .Reverse()
                        .ToArray();
                }
                return s_bolScriptHash;
            }
        }

        private static byte[] s_bolDeployScript = null;
        public static byte[] BolDeployScript
        {
            get
            {
                if(s_bolDeployScript == null)
                {
                    using (var sb = new ScriptBuilder())
                    {
                        sb.EmitPush(0);
                        sb.Emit(OpCode.PACK);

                        sb.EmitPush("deploy");

                        sb.EmitAppCall(BolScriptHash, false);

                        s_bolDeployScript = sb.ToArray();
                    }
                }
                return s_bolDeployScript;
            }
        }

        public static string BolScriptHashString => ProtocolSettings.Default.BolSettings.ScriptHash;

        public static bool IsBolInvocation(byte[] script)
        {
            var currentScriptEnd = script.Skip(script.Length - 20).ToArray();
            return currentScriptEnd.SequenceEqual(BolScriptHash) || script.SequenceEqual(BolScript);
        }
    }
}
