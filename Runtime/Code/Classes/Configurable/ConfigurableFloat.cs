using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RiskOfOptions;
using RiskOfOptions.Options;
using RiskOfOptions.OptionConfigs;

namespace Moonstorm.Config
{
    public class ConfigurableFloat : ConfigurableVariable<float>
    {
        
        protected override void OnConfigured()
        {
            base.OnConfigured();
            if(MSUtil.RiskOfOptionsInstalled)
            {
            }
        }
        public ConfigurableFloat(float defaultVal) : base(defaultVal)
        {
        }
    }

}