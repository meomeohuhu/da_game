using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using EcoGuardian.Core;
namespace EcoGuardian.Tests
{
    // Exercise public player actions and normal simulation damage; no resource/HP injection.
    public static class ScenarioChecks
    {
        public sealed class Result {public int sessions,steps,victories,defeats,timeouts,deterministicReplays;}
        static readonly CultureInfo C=CultureInfo.InvariantCulture;
        static void Require(bool condition,string message,int level,int seed,int step)
        {if(!condition)throw new Exception("Scenario invariant: "+message+"; level="+level+", seed="+seed+", step="+step);}
        public static Result Run(Action<string> log,int seeds=20)
        {
            var result=new Result();
            for(int level=1;level<=5;level++)for(int seed=0;seed<seeds;seed++){
                string first=Exercise(level,seed,result,true);string replay=Exercise(level,seed,result,false);
                Require(first==replay,"same seed/action sequence must replay identically",level,seed,-1);result.deterministicReplays++;
            }
            log("PASS scenario invariants: "+result.sessions+" sessions, "+result.steps+" steps, "+result.deterministicReplays+" deterministic replays.");
            log("Observed outcomes (not a win-rate/balance guarantee): victories="+result.victories+", defeats="+result.defeats+", timeouts="+result.timeouts);
            return result;
        }
        static string Exercise(int level,int seed,Result result,bool record)
        {
            var random=new Random(seed*7919+level);var s=new Simulation(Campaign.Create(level),new SaveData(),seed);int step;
            for(step=0;step<6000;step++){
                if(s.State==Phase.Victory||s.State==Phase.Defeat)break;
                if(s.State==Phase.Paused)s.TogglePause();
                // Complete mandatory tutorial only through the same actions the UI exposes.
                if(s.Tutorial==0)s.Plant(TowerKind.Thorn,s.Level.pads.FindIndex(p=>!p.trap&&!p.polluted));
                else if(s.Tutorial==1)s.StartWave();
                else if(s.Tutorial==2)s.Upgrade(s.Towers[0]);
                else if(step%3==0){
                    int operation=random.Next(8);int before=s.Energy;bool success=false;
                    switch(operation){
                        case 0:case 1:success=s.Plant((TowerKind)random.Next(5),random.Next(-1,s.Level.pads.Count+1));break;
                        case 2:if(s.Towers.Count>0)success=s.Upgrade(s.Towers[random.Next(s.Towers.Count)]);break;
                        case 3:if(s.Towers.Count>0)success=s.Sell(s.Towers[random.Next(s.Towers.Count)]);break;
                        case 4:success=s.Cast((Skill)random.Next(3),new V(random.Next(-2,22),random.Next(-2,13)));break;
                        case 5:success=s.Repair();break;
                        case 6:success=s.StartWave();break;
                        case 7:s.TogglePause();string paused=Fingerprint(s);s.Tick(.5f);Require(paused==Fingerprint(s),"pause changes simulation",level,seed,step);s.TogglePause();success=true;break;
                    }
                    if(!success)Require(s.Energy==before,"rejected action changed currency",level,seed,step);
                }
                s.Tick(.05f+(float)random.NextDouble()*.05f);
                Validate(s,level,seed,step);
            }
            if(record){result.sessions++;result.steps+=step;if(s.State==Phase.Victory)result.victories++;else if(s.State==Phase.Defeat)result.defeats++;else result.timeouts++;}
            if(s.State==Phase.Victory||s.State==Phase.Defeat){string ended=Fingerprint(s);s.Tick(10);s.TogglePause();Require(ended==Fingerprint(s),"result state must be immutable",level,seed,step);}
            return Fingerprint(s);
        }
        static void Validate(Simulation s,int level,int seed,int step)
        {
            Require(s.Energy>=0,"negative currency",level,seed,step);
            Require(s.Hp>=0&&s.Hp<=s.MaxHp&&!float.IsNaN(s.Hp),"invalid tree HP",level,seed,step);
            Require(s.Wave>=0&&s.Wave<=5,"wave index outside campaign",level,seed,step);
            Require(s.State!=Phase.Victory||(s.Wave==5&&s.Hp>0&&s.Remaining==0),"premature victory",level,seed,step);
            Require(s.State!=Phase.Defeat||s.Hp==0,"defeat with living tree",level,seed,step);
            var occupied=new HashSet<int>();
            foreach(var t in s.Towers){Require(occupied.Add(t.pad),"duplicate occupied pad",level,seed,step);var p=s.Level.pads[t.pad];Require(!p.polluted&&p.trap==(t.kind==TowerKind.Bomb),"tower on forbidden pad",level,seed,step);Require(t.level>=1&&t.level<=3,"invalid tower level",level,seed,step);int expected=Rules.Build[(int)t.kind];for(int i=1;i<t.level;i++)expected+=Rules.Upgrade[(int)t.kind,i-1];Require(t.spent==expected,"refund basis differs from actual investment",level,seed,step);}
            int alive=0;var ids=new HashSet<int>();foreach(var e in s.Enemies){Require(ids.Add(e.id),"duplicate enemy identity",level,seed,step);if(e.removed)continue;alive++;Require(e.hp>0&&e.hp<=e.maxHp,"invalid enemy HP",level,seed,step);Require(!float.IsNaN(e.position.x)&&!float.IsNaN(e.position.y),"nonfinite position",level,seed,step);Require(e.segment>0&&e.segment<s.Level.routes[e.route].points.Count,"active enemy past route end",level,seed,step);}
            Require(s.Remaining>=alive,"remaining count smaller than live enemies",level,seed,step);
            if(s.State==Phase.Prepare)Require(alive==0,"live enemies in preparation",level,seed,step);
            foreach(float cd in s.Cooldowns)Require(cd>=0&&!float.IsNaN(cd),"invalid cooldown",level,seed,step);
        }
        static string Fingerprint(Simulation s)
        {
            var b=new StringBuilder();b.Append(s.State).Append('|').Append(s.Wave).Append('|').Append(s.Energy).Append('|').Append(s.Hp.ToString("R",C)).Append('|').Append(s.Elapsed.ToString("R",C)).Append('|').Append(s.PrepareLeft.ToString("R",C)).Append('|').Append(s.Kills).Append('|').Append(s.Remaining);
            foreach(float cd in s.Cooldowns)b.Append('|').Append(cd.ToString("R",C));
            foreach(var t in s.Towers)b.Append("/t").Append(t.pad).Append(':').Append(t.kind).Append(':').Append(t.level).Append(':').Append(t.clock.ToString("R",C)).Append(':').Append(t.locked.ToString("R",C));
            foreach(var e in s.Enemies)b.Append("/e").Append(e.id).Append(':').Append(e.hp.ToString("R",C)).Append(':').Append(e.position.x.ToString("R",C)).Append(':').Append(e.position.y.ToString("R",C)).Append(':').Append(e.slowTime.ToString("R",C)).Append(':').Append(e.poisonTime.ToString("R",C)).Append(':').Append(e.rootTime.ToString("R",C)).Append(':').Append(e.phase);
            foreach(var f in s.Fires)b.Append("/f").Append(f.position.x).Append(':').Append(f.position.y).Append(':').Append(f.warning.ToString("R",C)).Append(':').Append(f.life.ToString("R",C));
            return b.ToString();
        }
    }
}
