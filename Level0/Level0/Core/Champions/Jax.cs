﻿using System;
using System.Collections.Generic;
using System.Linq;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Menu.Values;
using EloBuddy.SDK.Events;
using LevelZero.Controller;
using LevelZero.Model;
using LevelZero.Model.Values;
using LevelZero.Util;
using SharpDX;
using Circle = EloBuddy.SDK.Rendering.Circle;
using Activator = LevelZero.Controller.Activator;

namespace LevelZero.Core.Champions
{
    class Jax : PluginModel
    {
        readonly AIHeroClient Player = EloBuddy.Player.Instance;

        float ETime;
        float WardTick;

        Spell.Targeted Q { get { return (Spell.Targeted)Spells[0]; } }
        Spell.Active W { get { return (Spell.Active)Spells[1]; } }
        Spell.Active E { get { return (Spell.Active)Spells[2]; } }
        Spell.Active R { get { return (Spell.Active)Spells[3]; } }

        public override void Init()
        {
            InitVariables();
        }

        public override void InitVariables()
        {
            Activator = new Activator();

            Spells = new List<Spell.SpellBase>
            {
                new Spell.Targeted(SpellSlot.Q, 700),
                new Spell.Active(SpellSlot.W),
                new Spell.Active(SpellSlot.E, 187),
                new Spell.Active(SpellSlot.R)
            };

            DamageUtil.SpellsDamage = new List<SpellDamage>
            {
                new SpellDamage(Q, new float[] { 0, 70, 110, 150, 190, 230 }, new [] { 0, 1f, 1f, 1f, 1f, 1f }, DamageType.Physical),
                new SpellDamage(Q, new float[] { 0, 0, 0, 0, 0, 0 }, new [] { 0, 0.6f, 0.6f, 0.6f, 0.6f, 0.6f }, DamageType.Magical),
                new SpellDamage(W, new float[] { 0, 40, 75, 110, 145, 180 }, new [] { 0, 0.6f, 0.6f, 0.6f, 0.6f, 0.6f }, DamageType.Magical),
                new SpellDamage(E, new float[] { 50, 75, 100, 125, 150 }, new [] { 0, 0.5f, 0.5f, 0.5f, 0.5f, 0.5f }, DamageType.Physical)

            };

            InitMenu();

            DamageIndicator.Initialize(DamageUtil.GetComboDamage);

            new SkinController(11);
        }

        public override void InitMenu()
        {
            var feature = new Feature
            {
                NameFeature = "Draw",
                MenuValueStyleList = new List<ValueAbstract>
                {
                    new ValueCheckbox(false, "disable", "Disable"),
                    new ValueCheckbox(true, "dmgIndicator", "Show Damage Indicator"),
                    new ValueCheckbox(true, "draw.q", "Draw Q")
                }
            };

            feature.ToMenu();
            Features.Add(feature);

            feature = new Feature
            {
                NameFeature = "Combo",
                MenuValueStyleList = new List<ValueAbstract>
                {
                    new ValueCheckbox(true,  "combo.q", "Combo Q"),
                    new ValueCheckbox(true, "combo.q.aarange?qondash", "Enemy AARange ? Just Q on dash!"),
                    new ValueSlider(1900, 0, 1500, "combo.q.delay", "Use E and after some milliseconds use Q (1000ms = 1sec):"),
                    new ValueCheckbox(true, "combo.w", "Combo W", true),
                    new ValueCheckbox(true, "combo.w.aareset", "Use W AA Reset"),
                    new ValueCheckbox(true,  "combo.e", "Combo E", true),
                    new ValueCheckbox(true,  "combo.r", "Combo R", true),
                    new ValueCheckbox(true,  "combo.r.1v1logic", "Use 1v1 R Logic"),
                    new ValueSlider(5, 1, 2, "combo.r.minenemies", "Min enemies R")
                }
            };

            feature.ToMenu();
            Features.Add(feature);

            feature = new Feature
            {
                NameFeature = "Harass",
                MenuValueStyleList = new List<ValueAbstract>
                {
                    new ValueCheckbox(true,  "harass.q", "Harass Q"),
                    new ValueCheckbox(true, "harass.q.aarange?justqondash", "Enemy AARange ? Just Q on dash!"),
                    new ValueSlider(1900, 0, 1500, "harass.q.delay", "Use E and after some milliseconds use Q (1000ms = 1sec):", true),
                    new ValueCheckbox(true, "harass.w", "Harass W", true),
                    new ValueCheckbox(true, "harass.w.aareset", "Use W AA Reset"),
                    new ValueCheckbox(true,  "harass.e", "Harass E", true),
                    new ValueCheckbox(true,  "harass.r", "Harass R", true),
                    new ValueSlider(100, 1, 30, "harass.minmana%", "Harass, MinMana%", true)
                }
            };

            feature.ToMenu();
            Features.Add(feature);

            feature = new Feature
            {
                NameFeature = "Lane Clear",
                MenuValueStyleList = new List<ValueAbstract>
                {
                    new ValueCheckbox(true,  "laneclear.q", "Lane Clear Q"),
                    new ValueCheckbox(true,  "laneclear.q.jimwd", "Just Q if minion will die"),
                    new ValueCheckbox(true,  "laneclear.w", "Lane Clear W", true),
                    new ValueCheckbox(true,  "laneclear.e", "Lane Clear E", true),
                    new ValueSlider(7, 1, 3,  "laneclear.e.minminions", "Min minions E"),
                    new ValueSlider(100, 1, 30, "laneclear.mana%", "Lane Clear MinMana%", true)
                }
            };

            feature.ToMenu();
            Features.Add(feature);

            feature = new Feature
            {
                NameFeature = "Last Hit",
                MenuValueStyleList = new List<ValueAbstract>
                {
                    new ValueCheckbox(true,  "lasthit.q", "Last Hit Q"),
                    new ValueCheckbox(true,  "lasthit.w", "Last Hit W"),
                    new ValueSlider(100, 1, 30, "lasthit.mana%", "Last Hit MinMana%", true)
                }
            };

            feature.ToMenu();
            Features.Add(feature);

            feature = new Feature
            {
                NameFeature = "Jungle Clear",
                MenuValueStyleList = new List<ValueAbstract>
                {
                    new ValueCheckbox(true,  "jungleclear.q", "Jungle Clear Q"),
                    new ValueCheckbox(true,  "jungleclear.w", "Jungle Clear W"),
                    new ValueCheckbox(true,  "jungleclear.e", "Jungle Clear E"),
                    new ValueSlider(100, 1, 30, "jungleclear.mana%", "Jungle Clear MinMana%", true)
                }
            };

            feature.ToMenu();
            Features.Add(feature);

            feature = new Feature
            {
                NameFeature = "Misc",
                MenuValueStyleList = new List<ValueAbstract>
                {
                    new ValueCheckbox(true,  "misc.ks", "KS"),
                    new ValueKeybind(false, "misc.wardjump", "Ward Jump", KeyBind.BindTypes.HoldActive)
                }
            };

            feature.ToMenu();
            Features.Add(feature);
        }

        public override void PermaActive()
        {
            var misc = Features.First(it => it.NameFeature == "Misc");

            //----------------------------------------------Ward Jump---------------------------------------

            if (Q.IsReady() && misc.IsChecked("misc.wardjump") && Environment.TickCount - WardTick >= 2000)
            {
                var CursorPos = Game.CursorPos;

                Obj_AI_Base JumpPlace = EntityManager.Heroes.Allies.FirstOrDefault(it => it.Distance(CursorPos) <= 250 && Q.IsInRange(it));

                if (JumpPlace != default(Obj_AI_Base)) Q.Cast(JumpPlace);
                else
                {
                    JumpPlace = EntityManager.MinionsAndMonsters.Minions.FirstOrDefault(it => it.Distance(CursorPos) <= 250 && Q.IsInRange(it));

                    if (JumpPlace != default(Obj_AI_Base)) Q.Cast(JumpPlace);
                    else if (JumpWard() != default(InventorySlot))
                    {
                        var Ward = JumpWard();
                        CursorPos = Player.Position.Extend(CursorPos, 600).To3D();
                        Ward.Cast(CursorPos);
                        WardTick = Environment.TickCount;
                        EloBuddy.SDK.Core.DelayAction(() => WardJump(CursorPos), Game.Ping + 100);
                    }
                }

            }

            //------------------------------------------------KS------------------------------------------------

            if (misc.IsChecked("misc.ks") && EntityManager.Heroes.Enemies.Any(it => Q.IsInRange(it))) KS();
        }

        public override void OnDraw(EventArgs args)
        {
            var draw = Features.Find(f => f.NameFeature == "Draw");

            if (draw.IsChecked("disable"))
            {
                DamageIndicator.Enabled = false;
                return;
            }

            if (draw.IsChecked("draw.q"))
                Circle.Draw(Q.IsReady() ? Color.Blue : Color.Red, Q.Range, Player.Position);

            DamageIndicator.Enabled = draw.IsChecked("dmgIndicator");

            return;
        }

        public override void OnCombo()
        {
            var Target = TargetSelector.GetTarget(900, DamageType.Physical);

            if (Target == null) return;

            var mode = Features.First(it => it.NameFeature == "Combo");

            if (Q.IsInRange(Target))
            {
                if (!Player.HasBuff("JaxCounterStrike") && E.IsReady() && mode.IsChecked("combo.e") && E.Cast()) ETime = Environment.TickCount;

                if (Q.IsReady() && mode.IsChecked("combo.q"))
                {
                    if (Player.Distance(Target) <= Player.GetAutoAttackRange(Target) + 100 && Target.IsDashing() && mode.IsChecked("combo.q.aarange?qondash")) Q.Cast(Target);
                    else if (Environment.TickCount - ETime >= mode.SliderValue("combo.q.delay")) Q.Cast(Target);
                }

                if (W.IsReady() && !mode.IsChecked("combo.w.aareset") && (Q.IsReady() || Player.IsInAutoAttackRange(Target)) && W.Cast()) Orbwalker.ResetAutoAttack();
            }

            if (R.IsReady() && mode.IsChecked("combo.r"))
            {
                if (Player.CountEnemiesInRange(650) >= mode.SliderValue("combo.r.minenemies")) R.Cast();
                else if (mode.IsChecked("combo.r.1v1logic") && (Player.HealthPercent <= 42 || Target.HealthPercent > 40)) R.Cast();
            }

            return;
        }

        public override void OnHarass()
        {
            var mode = Features.First(it => it.NameFeature == "Harass");
            var Target = TargetSelector.GetTarget(900, DamageType.Physical);

            if (Target == null || Player.ManaPercent < mode.SliderValue("harass.mana%")) return;

            if (Q.IsInRange(Target))
            {
                if (!Player.HasBuff("JaxCounterStrike") && E.IsReady() && mode.IsChecked("harass.e") && E.Cast()) ETime = Environment.TickCount;

                if (Q.IsReady() && mode.IsChecked("harass.q") && Environment.TickCount - ETime >= mode.SliderValue("harass.q.delay")) Q.Cast(Target);

                if (W.IsReady() && !mode.IsChecked("harass.w.aareset") && (Q.IsReady() || Player.IsInAutoAttackRange(Target)) && W.Cast()) Orbwalker.ResetAutoAttack();
            }

            return;
        }

        public override void OnLaneClear()
        {
            var mode = Features.First(it => it.NameFeature == "Lane Clear");

            var minions = EntityManager.MinionsAndMonsters.GetLaneMinions(EntityManager.UnitTeam.Enemy, Player.Position, Q.Range);
            if (!minions.Any()) return;

            bool UseItem = minions.Count(it => it.IsValidTarget(400)) >= 3;
            if (UseItem) { Activator.tiamat.Cast(); Activator.hydra.Cast(); }

            if (Player.ManaPercent < mode.SliderValue("laneclear.mana%")) return;

            if (Q.IsReady() && mode.IsChecked("laneclear.q"))
            {
                if (mode.IsChecked("laneclear.q.jimwd"))
                {
                    var QMinions = minions.Where(it => !Player.IsInAutoAttackRange(it) && DamageUtil.GetSpellDamage(it, SpellSlot.Q) >= it.Health).OrderBy(it => it.Health);
                    if (QMinions.Any()) Q.Cast(QMinions.First());
                }
                else Q.Cast(minions.First());
            }

            if (!Player.HasBuff("JaxCounterStrike") && E.IsReady() && mode.IsChecked("laneclear.e") && minions.Count(it => it.IsValidTarget(E.Range)) >= mode.SliderValue("laneclear.e.minminions")) E.Cast();

            return;
        }

        public override void OnJungleClear()
        {
            var mode = Features.First(it => it.NameFeature == "Jungle Clear");

            if (Player.ManaPercent >= mode.SliderValue("jungleclear.mana%"))
            {
                var monsters = EntityManager.MinionsAndMonsters.GetJungleMonsters(Player.Position, Q.Range);

                if (monsters.Any())
                {
                    if (Q.IsReady() && mode.IsChecked("jungleclear.q")) Q.Cast(monsters.First());

                    if (!Player.HasBuff("JaxCounterStrike") && E.IsReady() && monsters.Any(it => Player.IsInAutoAttackRange(it)) && mode.IsChecked("jungleclear.e")) E.Cast();
                }
            }


            bool UseItem = EntityManager.MinionsAndMonsters.GetJungleMonsters(Player.Position, 400).Count() >= 1;
            if (UseItem) { Activator.tiamat.Cast(); Activator.hydra.Cast(); }

            return;
        }

        public override void OnLastHit()
        {
            var mode = Features.First(it => it.NameFeature == "Last Hit");

            var minions = EntityManager.MinionsAndMonsters.GetLaneMinions(EntityManager.UnitTeam.Enemy, Player.Position, Q.Range);
            if (!minions.Any()) return;

            if (Player.ManaPercent <= mode.SliderValue("lasthit.mana%")) return;

            if (Q.IsReady() && mode.IsChecked("lasthit.q"))
            {
                var QMinion = minions.FirstOrDefault(it => !Player.IsInAutoAttackRange(it) && DamageUtil.GetSpellDamage(it, SpellSlot.Q) > it.Health);
                if (QMinion != null) Q.Cast(QMinion);
            }

            if (W.IsReady() && mode.IsChecked("lasthit.w"))
            {
                var WMinion = minions.FirstOrDefault(it => Player.IsInAutoAttackRange(it) && it.Health > Player.GetAutoAttackDamage(it));
                if (WMinion != null)
                {
                    W.Cast();
                    EloBuddy.Player.IssueOrder(GameObjectOrder.AttackTo, WMinion);
                }
            }

            return;
        }

        public override void OnAfterAttack(AttackableUnit target, EventArgs args)
        {
            if (W.IsReady())
            {
                if (Player.Distance(target) <= Player.GetAutoAttackRange())
                {
                    var combo = Features.First(it => it.NameFeature == "Combo");

                    if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Combo) && combo.IsChecked("combo.w") && combo.IsChecked("combo.w.aareset"))
                    {
                        if (W.Cast())
                        {
                            Orbwalker.ResetAutoAttack();
                            EloBuddy.Player.IssueOrder(GameObjectOrder.AttackTo, target);
                        }

                        return;
                    }

                    var harass = Features.First(it => it.NameFeature == "Harass");

                    if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Harass) && harass.IsChecked("harass.w") && harass.IsChecked("harass.w.aareset") && Player.ManaPercent >= harass.SliderValue("harass.mana%"))
                    {
                        if (W.Cast())
                        {
                            Orbwalker.ResetAutoAttack();
                            EloBuddy.Player.IssueOrder(GameObjectOrder.AttackTo, target);
                        }

                        return;
                    }

                    var laneclear = Features.First(it => it.NameFeature == "Lane Clear");

                    if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.LaneClear) && laneclear.IsChecked("laneclear.w") && Player.ManaPercent >= laneclear.SliderValue("laneclear.mana%"))
                    {
                        if (target.Health > Player.GetAutoAttackDamage((Obj_AI_Base)target) && W.Cast())
                        {
                            Orbwalker.ResetAutoAttack();
                            EloBuddy.Player.IssueOrder(GameObjectOrder.AttackTo, target);
                        }

                        return;
                    }

                    var jungleclear = Features.First(it => it.NameFeature == "Jungle Clear");

                    if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.JungleClear) && jungleclear.IsChecked("jungleclear.w") && Player.ManaPercent >= jungleclear.SliderValue("jungleclear.mana%"))
                    {
                        if (target.Health > Player.GetAutoAttackDamage((Obj_AI_Base)target) && W.Cast())
                        {
                            Orbwalker.ResetAutoAttack();
                            EloBuddy.Player.IssueOrder(GameObjectOrder.AttackTo, target);
                        }

                        return;
                    }
                }
            }

            return;
        }

        //------------------------------------|| Extension ||--------------------------------------

        //---------------------------------------------WardJump()-------------------------------------------------

        void WardJump(Vector3 cursorpos)
        {
            var Ward = ObjectManager.Get<Obj_AI_Base>().FirstOrDefault(it => it.IsValidTarget(Q.Range) && it.Distance(cursorpos) <= 250);
            if (Ward != null) Q.Cast(Ward);
        }

        //---------------------------------------------JumpWard()--------------------------------------------------

        InventorySlot JumpWard()
        {
            var Inventory = Player.InventoryItems;

            if (Item.CanUseItem(3340)) return Inventory.First(it => it.Id == ItemId.Warding_Totem_Trinket);
            if (Item.CanUseItem(2049)) return Inventory.First(it => it.Id == ItemId.Sightstone);
            if (Item.CanUseItem(2045)) return Inventory.First(it => it.Id == ItemId.Ruby_Sightstone);
            if (Item.CanUseItem(3711)) return Inventory.First(it => (int)it.Id == 3711); //Tracker's Knife
            if (Item.CanUseItem(2301)) return Inventory.First(it => (int)it.Id == 2301); //Eye of the Watchers
            if (Item.CanUseItem(2302)) return Inventory.First(it => (int)it.Id == 2302); //Eye of the Oasis
            if (Item.CanUseItem(2303)) return Inventory.First(it => (int)it.Id == 2303); //Eye of the Equinox
            if (Item.CanUseItem(2043)) return Inventory.First(it => it.Id == ItemId.Vision_Ward);

            return default(InventorySlot);
        }

        //------------------------------------GetSmiteDamage()-------------------------------------

        float GetSmiteDamage()
        {
            float damage = new float(); //Arithmetic Progression OP :D

            if (Player.Level < 10) damage = 360 + (Player.Level - 1) * 30;

            else if (Player.Level < 15) damage = 280 + (Player.Level - 1) * 40;

            else if (Player.Level < 19) damage = 150 + (Player.Level - 1) * 50;

            return damage;
        }

        //-------------------------------------------KS--------------------------------------------

        void KS()
        {
            if (Q.IsReady())
            {
                var bye = EntityManager.Heroes.Enemies.FirstOrDefault(enemy => enemy.IsValidTarget(Q.Range) && DamageUtil.GetSpellDamage(enemy, SpellSlot.Q) >= enemy.Health);
                if (bye != null) { Q.Cast(bye); return; }
            }

            if (W.IsReady())
            {
                var bye = EntityManager.Heroes.Enemies.FirstOrDefault(enemy => enemy.IsValidTarget(W.Range) && DamageUtil.GetSpellDamage(enemy, SpellSlot.W) + Player.GetAutoAttackDamage(enemy) >= enemy.Health);
                if (bye != null) { W.Cast(); EloBuddy.Player.IssueOrder(GameObjectOrder.AttackTo, bye); return; }
            }

            if (Q.IsReady() && W.IsReady())
            {
                var bye = EntityManager.Heroes.Enemies.FirstOrDefault(enemy => enemy.IsValidTarget(W.Range) && DamageUtil.GetSpellDamage(enemy, SpellSlot.Q) + DamageUtil.GetSpellDamage(enemy, SpellSlot.W) >= enemy.Health);
                if (bye != null) { W.Cast(); EloBuddy.SDK.Core.DelayAction(() => Q.Cast(bye), 100); return; }
            }
        }
    }
}