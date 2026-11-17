using System;
using EcoGuardian.Core;
namespace EcoGuardian.Tests
{
    public static class RuleChecks
    {
        static int checks;
        static void Check(bool value,string name,Action<string> log){if(!value)throw new Exception("FAIL: "+name);checks++;log("PASS "+name);}
        static Simulation Game(int level=2){return new Simulation(Campaign.Create(level),new SaveData());}
        static void Advance(Simulation s,float seconds){for(int i=0;i<(int)(seconds*60);i++)s.Tick(1f/60);}
        static int Pad(Simulation s,bool trap=false){return s.Level.pads.FindIndex(p=>p.trap==trap&&!p.polluted);}
        public static int Run(Action<string> log)
        {
            checks=0;
            for(int level=1;level<=5;level++){
                var l=Campaign.Create(level);Check(l.waves.Count==5,"Five waves in level "+level,log);Check(l.pads.FindAll(p=>!p.polluted).Count==8+(level-1)*2,"Build-pad count level "+level,log);
                foreach(var p in l.pads)Check(!Campaign.OnPath(l,p.position),"Pad is off-path "+level+":"+p.position.x+","+p.position.y,log);
                for(int i=0;i<l.pads.Count;i++)for(int j=i+1;j<l.pads.Count;j++)if(V.Distance(l.pads[i].position,l.pads[j].position)<.1)throw new Exception("Duplicate pad");
            }
            var s=Game();int pad=Pad(s);Check(s.Hp==100&&s.Energy==150,"Fresh stage resources",log);
            Check(s.Plant(TowerKind.Thorn,pad),"Plant valid tower",log);int energy=s.Energy;Check(!s.Plant(TowerKind.Thorn,pad)&&s.Energy==energy,"Overlap does not charge",log);
            Check(!s.Plant(TowerKind.Bomb,Pad(s))&&s.Energy==energy,"Bomb rejected on normal pad",log);
            Check(!s.Plant(TowerKind.Thorn,Pad(s,true))&&s.Energy==energy,"Thorn rejected on trap pad",log);
            Check(!s.Plant(TowerKind.Bloom,3)&&s.Energy==energy,"Locked tower rejected",log);
            Check(!s.Plant(TowerKind.Thorn,-1)&&s.Energy==energy,"Invalid index does not charge",log);
            int poll=s.Level.pads.FindIndex(p=>p.polluted);Check(!s.Plant(TowerKind.Thorn,poll)&&s.Energy==energy,"Pollution blocks placement",log);
            var t=s.Towers[0];Check(s.Upgrade(t)&&t.level==2&&s.Energy==40,"Upgrade correct cost",log);Check(!s.Upgrade(t)&&s.Energy==40,"Unaffordable upgrade does not charge",log);
            Check(s.Sell(t)&&s.Energy==106,"Sell refunds floor of 60 percent invested",log);Check(!s.Sell(t)&&s.Energy==106,"Cannot sell twice",log);
            s=Game();Advance(s,5);Check(s.Energy==150,"No passive income in preparation",log);s.TogglePause();float prep=s.PrepareLeft;Advance(s,10);Check(s.PrepareLeft==prep&&s.Energy==150,"Pause freezes preparation",log);s.TogglePause();Check(s.StartWave()&&s.Energy==150,"First start has no early bonus",log);Check(!s.StartWave()&&s.Energy==150,"Repeated start rejected",log);Advance(s,2);Check(s.Energy>=153&&s.Energy<=154,"Combat income is 2 per second",log);
            var e=s.Enemies[0];float hp=e.hp;var position=e.position;float elapsed=s.Elapsed;s.TogglePause();Advance(s,20);Check(e.hp==hp&&V.Distance(position,e.position)==0&&s.Elapsed==elapsed,"Pause freezes combat movement and clocks",log);s.TogglePause();
            int money=s.Energy;s.Damage(e,1000);int kills=s.Kills;s.Damage(e,1000);Check(s.Energy==money+5&&s.Kills==kills,"Death grants exactly one reward",log);
            var armor=s.Spawn(EnemyKind.Bulldozer,0);s.Damage(armor,100);Check(Math.Abs(armor.hp-230)<.01,"Armor reduces direct damage by 30 percent",log);s.Damage(armor,10,true);Check(Math.Abs(armor.hp-220)<.01,"Poison ignores armor",log);
            armor.Slow(.4f,2);armor.Slow(.3f,2);Check(Math.Abs(armor.slow-.4f)<.001,"Slow does not stack",log);armor.Poison(12,6);armor.Poison(5,5);Check(armor.poison==12&&armor.poisonTime==5,"Poison strongest and duration refresh",log);
            var boss=s.Spawn(EnemyKind.Heavy,0);boss.Slow(.4f,2);boss.Root(3);Check(Math.Abs(boss.slow-.2)<.001&&boss.rootTime==1.5,"Boss status resistance",log);
            Check(s.State==Phase.Combat&&s.Remaining>0,"Wave remains active while queued or live enemies exist",log);
            s=Game(5);s.StartWave();var titan=s.Spawn(EnemyKind.Titan,0);s.Damage(titan,1500);s.Tick(.01f);Check(titan.phase==2&&titan.phaseStop>2.9,"Titan 70 percent transition pauses three seconds",log);s.Damage(titan,2000);s.Tick(.01f);Check(titan.phase==3,"Titan 30 percent transition",log);int summons=titan.summons;Advance(s,19);Check(titan.summons==summons,"Overdrive stops summoning",log);
            s=Game(5);s.StartWave();titan=s.Spawn(EnemyKind.Titan,0);s.Damage(titan,3600);s.Tick(.01f);Check(titan.phase==3,"Titan skips directly to correct phase",log);
            s=Game(5);s.StartWave();titan=s.Spawn(EnemyKind.Titan,0);titan.segment=s.Level.routes[0].points.Count-1;titan.position=new V(18.999f,5);s.Tick(.1f);Check(s.State==Phase.Defeat&&s.Hp==0,"Titan reaching tree defeats before victory",log);int frozenMoney=s.Energy;Advance(s,20);Check(s.Energy==frozenMoney,"No income after defeat",log);
            s=Game(3);s.StartWave();energy=s.Energy;Check(!s.Cast(Skill.Rain,new V(-2,4))&&s.Energy==energy&&s.Cooldowns[0]==0,"Invalid skill target does not charge",log);Check(!s.Cast(Skill.Roots,new V(4,4))&&s.Energy==energy,"Locked skill does not charge",log);Check(s.Cast(Skill.Rain,new V(4,4))&&s.Energy==energy-30&&s.Cooldowns[0]==30,"Rain cost and cooldown",log);s.TogglePause();Advance(s,4);Check(s.Cooldowns[0]==30,"Pause freezes skill cooldown",log);s.TogglePause();Advance(s,2);Check(s.Cooldowns[0]<29,"Cooldown advances in combat",log);
            s=Game(1);Advance(s,20);Check(s.Wave==0&&s.Elapsed==0,"Tutorial freezes time before first plant",log);Check(s.Plant(TowerKind.Thorn,Pad(s)),"Tutorial first plant",log);energy=s.Energy;Check(!s.Plant(TowerKind.Thorn,3)&&s.Energy==energy,"Tutorial reserves money for upgrade",log);Check(!s.Upgrade(s.Towers[0]),"Tutorial reserves first upgrade until prompted",log);s.StartWave();Advance(s,20);Check(s.Enemies.Count==0,"Tutorial freezes wave pending upgrade",log);Check(!s.Sell(s.Towers[0]),"Tutorial prevents selling its required tower",log);Check(s.Upgrade(s.Towers[0]),"Tutorial upgrade unlocks combat",log);Advance(s,1);Check(s.Enemies.Count>0,"Tutorial combat resumes",log);
            var save=new SaveData();save.Complete(1,3);save.Complete(1,1);Check(save.Earned==3&&save.stars[0]==3&&save.unlocked==2,"Replay preserves best stars without farming",log);Check(save.Buy(0)&&save.Available==0&&!save.Buy(0)&&!save.Buy(1),"Upgrade spends stars only once",log);
            Check(Rules.Stars(96,120)==3&&Rules.Stars(60,120)==2&&Rules.Stars(59,120)==1,"Star thresholds use upgraded maximum HP",log);save.upgrades[1]=true;save.upgrades[2]=true;save.Complete(2,3);save.Complete(3,3);s=new Simulation(Campaign.Create(2),save);Check(s.MaxHp==120&&s.Energy==180,"Permanent health and economy bonuses apply",log);
            // Kill enemies through the public damage pipeline to verify wave bookkeeping, not balance.
            s=Game(2);s.StartWave();for(int i=0;i<20000&&s.State!=Phase.Victory;i++){
                if(s.State==Phase.Prepare){if(s.Wave==1){float cd=s.Cooldowns[0];int cash=s.Energy;s.Tick(1);Check(s.Cooldowns[0]==cd&&s.Energy==cash,"Preparation freezes skill cooldown and income",log);}int cashBefore=s.Energy;Check(s.StartWave()&&s.Energy==cashBefore+10,"Next wave early bonus once",log);}
                s.Tick(.1f);foreach(var enemy in s.Enemies.ToArray())s.Damage(enemy,100000);
            }
            Check(s.State==Phase.Victory&&s.Wave==5&&s.Hp>0,"Five waves yield victory",log);Check(s.RewardTotal==280,"All wave rewards include final reward",log);Check(s.Save.stars[1]==3,"Victory records actual result",log);
            s=Game(2);s.StartWave();e=s.Spawn(EnemyKind.Lumber,0);e.segment=s.Level.routes[0].points.Count-1;e.position=new V(18.999f,5);s.Tick(.1f);Check(s.Hp==90&&e.removed,"Escape damages tree and removes enemy",log);
            for(int i=0;i<2000&&s.State==Phase.Combat;i++){s.Tick(.1f);foreach(var enemy in s.Enemies.ToArray())s.Damage(enemy,100000);}
            Check(s.State==Phase.Prepare&&s.Repair()&&s.Hp==100,"Repair caps at max HP",log);energy=s.Energy;Check(!s.Repair()&&s.Energy==energy,"Repair once per preparation",log);
            // Environmental warnings, silence windows, and fixed-time resource producers.
            s=Game(3);s.StartWave();for(int i=0;i<3000&&s.Wave==1;i++){s.Tick(.1f);foreach(var enemy in s.Enemies.ToArray())s.Damage(enemy,100000);}s.StartWave();
            Advance(s,20.2f);Check(s.Fires.Count>0&&s.Fires[0].warning>0,"Fire warns before ignition",log);
            var fire=s.Fires[0];Advance(s,3.2f);Check(s.IsBurning(fire.position),"Fire activates after warning",log);
            Check(s.Cast(Skill.Rain,fire.position)&&!s.IsBurning(fire.position),"Rain extinguishes a burning cell",log);
            float cooldown=s.Cooldowns[0];for(int i=0;i<3000&&s.State==Phase.Combat;i++){s.Tick(.05f);foreach(var enemy in s.Enemies.ToArray())s.Damage(enemy,100000);}
            cooldown=s.Cooldowns[0];Advance(s,2);Check(s.State==Phase.Prepare&&s.Cooldowns[0]==cooldown,"Nonzero skill cooldown freezes between waves",log);
            s=Game(5);pad=Pad(s);s.Plant(TowerKind.Thorn,pad);s.StartWave();t=s.Towers[0];var baseline=s.DamageValue(t);Check(s.Cast(Skill.Ancient,new V())&&Math.Abs(s.DamageValue(t)-baseline*1.5f)<.01,"Ultimate boosts attack damage 50 percent",log);
            var bloom=new Tower{kind=TowerKind.Bloom};var bomb=new Tower{kind=TowerKind.Bomb};Check(s.Interval(bloom)==10&&s.Interval(bomb)==8,"Ultimate leaves income and bomb intervals unchanged",log);Check(Math.Abs(s.DamageValue(bomb)-90)<.01,"Ultimate boosts bomb damage without changing recovery",log);
            s=Game(5);s.Plant(TowerKind.Thorn,Pad(s));s.StartWave();titan=s.Spawn(EnemyKind.Titan,0);titan.hp=3500;s.Tick(.01f);titan.position=s.Level.pads[s.Towers[0].pad].position;titan.Root(100);Advance(s,16.2f);Check(titan.shockWarning>0&&s.Towers[0].locked==0,"Shockwave gives a two-second warning",log);Advance(s,2);Check(s.Towers[0].locked>0,"Shockwave temporarily disables nearby towers",log);
            s=Game(5);s.StartWave();titan=s.Spawn(EnemyKind.Titan,0);Advance(s,12.1f);Check(titan.summons==1,"Titan phase one summons every twelve seconds",log);int population=s.Enemies.Count;s.Damage(titan,10000);Advance(s,.1f);Check(titan.removed&&s.State==Phase.Combat&&s.Remaining>0,"Boss death leaves existing support alive",log);
            // Validate that expiry really removes strong effects rather than retaining them forever.
            s=Game(2);s.StartWave();e=s.Spawn(EnemyKind.Lumber,0);e.Slow(.4f,1);e.Poison(5,1);Advance(s,1.1f);Check(e.slow==0&&e.poison==0&&e.slowTime==0&&e.poisonTime==0,"Slow and poison expire",log);
            var retry=Game(2);Check(retry.Enemies.Count==0&&retry.Towers.Count==0&&retry.Wave==0&&retry.Cooldowns[0]==0&&retry.Energy==150,"Retry starts with clean runtime state",log);
            return checks;
        }
    }
}
