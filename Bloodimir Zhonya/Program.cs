using System;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Events;
using EloBuddy.SDK.Menu;
using EloBuddy.SDK.Menu.Values;

namespace Bloodimir_Zhonya
{
    internal static class Program

    {
        private static Menu ZhonyaMenu;
        private static AIHeroClient Player = ObjectManager.Player;
        private static Item Zhonia;

        private static void Main(string[] args)
        {
            Loading.OnLoadingComplete += Loading_OnLoadingComplete;
        }

        private static void Loading_OnLoadingComplete(EventArgs args)
        {
            Zhonia = new Item((int) ItemId.Zhonyas_Hourglass);
            ZhonyaMenu = MainMenu.AddMenu("Zhonya", "zhonyamenu");
            ZhonyaMenu.Add("zhonya", new CheckBox("Use Zhonya"));
            ZhonyaMenu.Add("zhealth", new Slider("Auto Zhonia Health %", 28));
            Game.OnUpdate += Tick;
        }

        private static void Zhonya()
        {
            var zhoniaon = ZhonyaMenu["zhonya"].Cast<CheckBox>().CurrentValue;
            var zhealth = ZhonyaMenu["zhealth"].Cast<Slider>().CurrentValue;

            if (!zhoniaon || !Zhonia.IsReady() || !Zhonia.IsOwned()) return;
            if (Player.HealthPercent <= zhealth)
            {
                Zhonia.Cast();
            }
        }

        private static void Tick(EventArgs args)
        {
            Zhonya();
        }
    }
}
