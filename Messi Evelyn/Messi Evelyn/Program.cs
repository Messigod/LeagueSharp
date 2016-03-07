using System;
using System.Collections.Generic;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;

namespace Messievelynn
{
    internal class Program
    {
        public static string ChampionName = "Evelynn";

        //오브워커
        public static Orbwalking.Orbwalker Orbwalker;

        //스펠
        public static List<Spell> SpellList = new List<Spell>();

        public static Spell Q;
        public static Spell W;
        public static Spell E;
        public static Spell R;

        //메뉴
        public static Menu Config;

        private static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        private static void Game_OnGameLoad(EventArgs args)
        {
         //   Player = ObjectManager.Player;
         //   if (player.ChampionName != ChampionName) return;

            Game.PrintChat("<font color='#5CD1E5'>[ Evelynn assem / test version 1.0 </font> <font color='#FF0000'> Q auto active is not yet. ] </font>");           


            //스펠시작
            Q = new Spell(SpellSlot.Q, 500f);
            W = new Spell(SpellSlot.W, Q.Range);
            E = new Spell(SpellSlot.E, 225f + 2 * 65f);
            R = new Spell(SpellSlot.R, 650f);

            R.SetSkillshot(0.25f, 350f, float.MaxValue, false, SkillshotType.SkillshotCircle);
            
            SpellList.Add(Q);
            SpellList.Add(W);
            SpellList.Add(E);
            SpellList.Add(R);

            //메뉴시작
            Config = new Menu(ChampionName, ChampionName, true);

            var targetSelectorMenu = new Menu("타겟 설정", "타겟 설정");
            TargetSelector.AddToMenu(targetSelectorMenu);
            Config.AddSubMenu(targetSelectorMenu);

            //오브워커 보조메뉴
            Config.AddSubMenu(new Menu("오브워킹", "오브워킹"));

            //오브웍메뉴,섭메뉴 로드
            Orbwalker = new Orbwalking.Orbwalker(Config.SubMenu("오브워킹"));


            Config.AddSubMenu(new Menu("콤보", "콤보"));
            Config.SubMenu("콤보").AddItem(new MenuItem("UseQCombo", "Q 사용").SetValue(true));
            Config.SubMenu("콤보").AddItem(new MenuItem("UseWCombo", "W 사용").SetValue(true));
            Config.SubMenu("콤보").AddItem(new MenuItem("UseECombo", "E 사용").SetValue(true));
            Config.SubMenu("콤보").AddItem(new MenuItem("UseRCombo", "R 사용").SetValue(true));
            Config.SubMenu("콤보")
                .AddItem(
                    new MenuItem("ComboActive", "콤보키 설정").SetValue(new KeyBind("A".ToCharArray()[0], KeyBindType.Press)));

            //Q오토설정
            Config.AddSubMenu(new Menu("Q 자동설정", "Q 자동설정"));
            Config.SubMenu("Q 자동설정").AddItem(new MenuItem("QAutoActive", "자동시전").SetValue(true));
            Config.SubMenu("Q 자동설정")
                .AddItem(
                    new MenuItem("QAutoActive", "자동시전").SetValue(new KeyBind("Z".ToCharArray()[0],
                        KeyBindType.Press)));
            //여기까지 자동설정

            Config.AddSubMenu(new Menu("라인정리", "라인정리"));
            Config.SubMenu("라인정리").AddItem(new MenuItem("UseQLaneClear", "Q 사용").SetValue(true));
            Config.SubMenu("라인정리").AddItem(new MenuItem("UseELaneClear", "E 사용").SetValue(true));
            Config.SubMenu("라인정리")
                .AddItem(
                    new MenuItem("LaneClearActive", "라인정리 키설정").SetValue(new KeyBind("V".ToCharArray()[0],
                        KeyBindType.Press)));

            Config.AddSubMenu(new Menu("정글먹기", "정글먹기"));
            Config.SubMenu("정글먹기").AddItem(new MenuItem("UseQJFarm", "Q 사용").SetValue(true));
            Config.SubMenu("정글먹기").AddItem(new MenuItem("UseEJFarm", "E 사용").SetValue(true));
            Config.SubMenu("정글먹기")
                .AddItem(
                    new MenuItem("JungleFarmActive", "정글파밍 키설정").SetValue(new KeyBind("V".ToCharArray()[0],
                        KeyBindType.Press)));

            //Config.addsubmenu(new Menu("어셈설명", "어셈설명"));
            //Config.Submenu("어셈설명").additem(new Menuitem("non","본 어셈은 테스트용으로 제작되었습니다.").SetValue(true));
            //Config.Submenu("어셈설명").additem(new Menuitem("non","1.0 버전에서는 Q자동시전 지원되지 않습니다.").SetValue(true));
            //Config.submenu("어셈설명");


            Config.AddSubMenu(new Menu("드로잉", "드로잉"));
            Config.SubMenu("드로잉")
                .AddItem(new MenuItem("QRange", "Q 사거리").SetValue(new Circle(true, Color.FromArgb(255, 255, 255, 255))));
           // Config.SubMenu("드로잉")
           //     .AddItem(new MenuItem("ERange", "E 사거리").SetValue(new Circle(false, Color.FromArgb(255, 255, 255, 255))));
           // Config.SubMenu("드로잉")
           //     .AddItem(new MenuItem("RRange", "R 사거리").SetValue(new Circle(false, Color.FromArgb(255, 255, 255, 255))));
            //ㄷ여기
           
            //끝
            Config.AddToMainMenu();

            //여기는 아님
            Drawing.OnDraw += Drawing_OnDraw;
            Game.OnUpdate += Game_OnGameUpdate;
            Spellbook.OnCastSpell += Spellbook_OnCastSpell;
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            foreach (var spell in SpellList)
            {
                if (spell.Slot == SpellSlot.W)
                {
                    return;
                }
                var menuItem = Config.Item(spell.Slot + "Range").GetValue<Circle>();
                if (menuItem.Active)
                    Render.Circle.DrawCircle(ObjectManager.Player.Position, spell.Range, menuItem.Color);
            }

        }


        private static void Game_OnGameUpdate(EventArgs args)
        {
            if (!Orbwalking.CanMove(40)) return;

            if (Config.Item("ComboActive").GetValue<KeyBind>().Active)
            {
                Combo();
                return;
            }

            if (Config.Item("LaneClearActive").GetValue<KeyBind>().Active)
                LaneClear();

            if (Config.Item("JungleFarmActive").GetValue<KeyBind>().Active)
                JungleFarm();
        }


        private static void Spellbook_OnCastSpell(Spellbook sender, SpellbookCastSpellEventArgs args)
        {
            if (sender.Owner.IsMe && args.Slot == SpellSlot.R)
            {
                if (ObjectManager.Get<Obj_AI_Hero>()
                .Count(
                    hero =>
                        hero.IsValidTarget() &&
                        hero.Distance(args.StartPosition.To2D()) <= R.Range) == 0)
                    args.Process = false;
            }
        }

        private static void Combo()
        {
            var target = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.True);

            if (target != null)
            {
                if (Config.Item("UseQCombo").GetValue<bool>())
                    Q.Cast();

                if (Config.Item("UseWCombo").GetValue<bool>() && W.IsReady() &&
                    ObjectManager.Player.HasBuffOfType(BuffType.Slow))
                    W.Cast();

                if (Config.Item("UseECombo").GetValue<bool>() && E.IsReady())
                    E.CastOnUnit(target);

                if (Config.Item("UseRCombo").GetValue<bool>() && R.IsReady() && GetComboDamage(target) > target.Health)
                    R.Cast(target, false, true);
                
            }
        }
        //Q오토
        private static void QAuto()
        {
            var target = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.True);

            if (target != null)
            {
                    if (Config.Item("QAutoActive").GetValue<bool>())
                    Q.Cast();
                                    
                        }
        }
       // private static void non()
       // { }

      
        private static void JungleFarm()
        {
            var mobs = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, Q.Range,
                MinionTypes.All,
                MinionTeam.Neutral, MinionOrderTypes.MaxHealth);

            if (mobs.Count > 0)
            {
                if (Config.Item("UseQJFarm").GetValue<bool>() && Q.IsReady())
                    Q.Cast();

                if (Config.Item("UseEJFarm").GetValue<bool>() && E.IsReady())
                    E.CastOnUnit(mobs[0]);
            }
        }

        private static void LaneClear()
        {
            var minions = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, Q.Range);

            foreach (var minion in minions.FindAll(minion => minion.IsValidTarget(Q.Range)))
            {
                if (Config.Item("UseQLaneClear").GetValue<bool>() && Q.IsReady())
                    Q.Cast();

                if (Config.Item("UseELaneClear").GetValue<bool>() && E.IsReady())
                    E.CastOnUnit(minion);
            }
        }

        private static float GetComboDamage(Obj_AI_Base target)
        {
            float comboDamage = 0;

            if ((ObjectManager.Player.Spellbook.GetSpell(SpellSlot.Q).Level) > 0)
                comboDamage += Q.GetDamage(target) * 3;
            if (E.IsReady())
                comboDamage += E.GetDamage(target);
            if (R.IsReady())
                comboDamage += R.GetDamage(target);

            return comboDamage;
        }
    }
}
