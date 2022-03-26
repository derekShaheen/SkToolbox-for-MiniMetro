using SkToolbox.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SkToolbox.Utility;

namespace SkToolbox.SkModules
{
    class ModAsset : SkBaseModule, IModule
    {
        Game game;
        GameController gameController;

        public ModAsset() : base()
        {
            base.ModuleName = "Asset";
            base.Loading();
        }

        public void Start()
        {
            BeginMenu();
            base.CallerEntry = new SkMenuItem("Asset Menu\t►", () => base.SkMC.RequestSubMenu(base.FlushMenu()));
            base.Ready(); // Must be called when the module has completed initialization. // End of Start

        }

        public void BeginMenu()
        {
            SkMenu GenericMenu = new SkMenu();
            GenericMenu.AddItem("Reach Milestone", new Action(ReachMilestone));
            foreach (string asset in Enum.GetNames(typeof(AssetType)))
            {
                if (!asset.Equals("None") && !asset.Equals("Count"))
                {
                    GenericMenu.AddItem("Add " + asset, new Action<string>(AddAsset));
                }
            }
            MenuOptions = GenericMenu;
        }

        public void ReachMilestone()
        {
            GetObjects();
            SkUtilities.InvokePrivateMethod(game, "ReachMilestone", new object[] { });
        }

        public void AddAsset(string pAssetType)
        {
            GetObjects();
            //public enum AssetType
            //{
            //    None = 0, // Excluded
            //    Line = 1,
            //    Locomotive = 2,
            //    Shinkansen = 3,
            //    Tram = 4,
            //    Ferry = 5,
            //    Carriage = 6,
            //    Crossing = 7,
            //    Interchange = 8,
            //    Bridge = 9,
            //    Count = 10 // Excluded
            //}
            pAssetType = pAssetType.Substring(3);
            AssetType assetType = AssetType.None;
            
            Enum.TryParse(pAssetType, out assetType);

            if (game != null)
            {
                game.AssetDatabase.AddAsset(assetType);
            }
        }

        public void GetObjects()
        {
            if (Main.Instance != null)
            {
                gameController = SkUtilities.GetPrivateField<GameController>(Main.Instance, "controller");
                if (gameController == null)
                {
                    SkUtilities.Logz("Could not find game controller.");
                }

                game = SkUtilities.GetPrivateField<Game>(gameController, "game");
                if (gameController == null)
                {
                    SkUtilities.Logz("Could not find game.");
                }
            }
        }
    }
}
