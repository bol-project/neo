using Microsoft.Extensions.Configuration;
using Neo.Network.P2P.Payloads;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Neo
{
    public class ProtocolSettings
    {
        public uint Magic { get; }
        public byte AddressVersion { get; }
        public string[] StandbyValidators { get; }
        public string[] SeedList { get; }
        public IReadOnlyDictionary<TransactionType, Fixed8> SystemFee { get; }
        public Fixed8 LowPriorityThreshold { get; }
        public uint SecondsPerBlock { get; }
        public uint FreeGasChangeHeight { get; }

        public uint StateRootEnableIndex { get; }
        public Fixed8 MinimumNetworkFee { get; }
        static ProtocolSettings _default;
        public BolContractSetings BolSettings { get; }

        static bool UpdateDefault(IConfiguration configuration)
        {
            var settings = new ProtocolSettings(configuration.GetSection("ProtocolConfiguration"));
            return null == Interlocked.CompareExchange(ref _default, settings, null);
        }

        public static bool Initialize(IConfiguration configuration)
        {
            return UpdateDefault(configuration);
        }

        public static ProtocolSettings Default
        {
            get
            {
                if (_default == null)
                {
                    var configuration = new ConfigurationBuilder().AddJsonFile("protocol.json", true).AddEnvironmentVariables().Build();
                    UpdateDefault(configuration);
                }

                return _default;
            }
        }

        private ProtocolSettings(IConfigurationSection section)
        {
            this.Magic = section.GetValue("Magic", 0x746E41u);
            this.AddressVersion = section.GetValue("AddressVersion", (byte)0x17);
            IConfigurationSection section_sv = section.GetSection("StandbyValidators");
            if (section_sv.Exists())
                this.StandbyValidators = section_sv.GetChildren().Select(p => p.Get<string>()).ToArray();
            else
                throw new ArgumentException("StandbyValidators section in protocol is empty.");
            IConfigurationSection section_sl = section.GetSection("SeedList");
            if (section_sl.Exists())
                this.SeedList = section_sl.GetChildren().Select(p => p.Get<string>()).ToArray();
            else
                throw new ArgumentException("SeedList section in protocol is empty.");
            Dictionary<TransactionType, Fixed8> sys_fee = new Dictionary<TransactionType, Fixed8>
            {
                [TransactionType.EnrollmentTransaction] = Fixed8.FromDecimal(1000),
                [TransactionType.IssueTransaction] = Fixed8.FromDecimal(500),
                [TransactionType.PublishTransaction] = Fixed8.FromDecimal(500),
                [TransactionType.RegisterTransaction] = Fixed8.FromDecimal(10000)
            };
            foreach (IConfigurationSection child in section.GetSection("SystemFee").GetChildren())
            {
                TransactionType key = (TransactionType)Enum.Parse(typeof(TransactionType), child.Key, true);
                sys_fee[key] = Fixed8.Parse(child.Value);
            }
            this.SystemFee = sys_fee;
            this.SecondsPerBlock = section.GetValue("SecondsPerBlock", 15u);
            this.StateRootEnableIndex = section.GetValue("StateRootEnableIndex", 0u);
            this.LowPriorityThreshold = Fixed8.Parse(section.GetValue("LowPriorityThreshold", "0.001"));
            this.MinimumNetworkFee = Fixed8.Parse(section.GetValue("MinimumNetworkFee", "0"));
            this.FreeGasChangeHeight = section.GetValue("FreeGasChangeHeight", 100000000u);
            this.BolSettings = section.GetSection("BolContract").Get<BolContractSetings>();
        }
        
        public class BolContractSetings
        {
            public string ScriptHash { get; set; }
            public string Name { get; set; }
            public string Version { get; set; }
            public string Author { get; set; }
            public string Email { get; set; }
            public string Description { get; set; }
            public string Path { get; set; }
        }
    }
}
