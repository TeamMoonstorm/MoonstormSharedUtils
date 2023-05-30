using System.Collections;
using System.Collections.Generic;
using ThunderKit.Integrations.Thunderstore;
using UnityEngine;

namespace Moonstorm.EditorUtils.Importers
{
    public class InstallRiskOfOptions : ThunderstorePackageInstaller
    {
        public override string DependencyId => "Rune580-Risk_Of_Options";

        public override string ThunderstoreAddress => "https://thunderstore.io";

        public override int Priority => Constants.Priority.InstallRiskOfOptions;

        public override string Description => "Installs Risk of Options, a framework that allows MSU to display your mod's configs in the game's options menu.";
    }
}
