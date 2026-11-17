using System;
using System.Collections.Generic;
using System.Globalization;
using EcoGuardian.Core;
public static class StrategyProbe
{
    public static void Run(Action<string> log)
    {
        log("STRATEGY_PROBE,level,strategy,result,wave,hp,seconds,kills,planted");
        for(int level=1;level<=5;level++)for(int strategy=0;strategy<6;strategy++){
            var s=new Simulation(Campaign.Create(level),new SaveData(),42);int tick;
            for(tick=0;tick<36000&&s.State!=Phase.Victory&&s.State!=Phase.Defeat;tick++){
                if(tick%5==0)Act(s,strategy);s.Tick(.05f);
            }
            if(strategy==5&&s.State!=Phase.Victory)throw new Exception("Full-stage strategy smoke failed in stage "+level+"; seed=42, strategy=upgrade-four");
            log(string.Join(",",new[]{"STRATEGY_PROBE",level.ToString(),new[]{"thorn","mixed","economy","late-poison","upgrade-two","upgrade-four"}[strategy],s.State==Phase.Victory?"Victory":s.State==Phase.Defeat?"Defeat":"Timeout",s.Wave.ToString(),s.Hp.ToString("0.#",CultureInfo.InvariantCulture),s.Elapsed.ToString("0.#",CultureInfo.InvariantCulture),s.Kills.ToString(),s.Planted.ToString()}));
        }
        log("PASS: upgrade-four wins each of the five full-stage simulations with ordinary player actions (seed 42).");
        log("Strategy probes use ordinary placement/upgrades/skills and Tick; no injected currency, HP or Damage. Results describe these bots only, not human balance or Unity playtesting.");
    }
    static void Act(Simulation s,int strategy)
    {
        if(s.Tutorial==0){s.Plant(TowerKind.Thorn,BestPad(s));return;}
        if(s.Tutorial==1){s.StartWave();return;}
        if(s.Tutorial==2){s.Upgrade(s.Towers[0]);return;}
        // Put recovery ahead of spending on offensive upgrades.
        if(s.State==Phase.Prepare&&s.Hp<s.MaxHp-10&&s.Energy>=100)s.Repair();
        if(s.State==Phase.Combat){
            var titan=s.Enemies.Find(e=>!e.removed&&e.kind==EnemyKind.Titan);
            if(titan!=null&&s.Energy>=80&&s.Towers.Exists(t=>V.Distance(s.Level.pads[t.pad].position,titan.position)<=3))s.Cast(Skill.Ancient,titan.position);
            if(titan!=null&&titan.phase==3&&s.Energy>=40)s.Cast(Skill.Roots,titan.position);
            if(s.Fires.Count>0&&s.Energy>=30)s.Cast(Skill.Rain,s.Fires[0].position);
            if(s.Enemies.Count>8&&s.Energy>=120){Enemy first=null;float best=float.MaxValue;foreach(var e in s.Enemies){if(e.removed)continue;float left=s.DistanceRemaining(e);if(left<best){first=e;best=left;}}if(first!=null&&best<10)s.Cast(Skill.Roots,first.position);}
        }
        if(strategy>=4&&s.Towers.Count>=(strategy==4?2:4)){
            var core=s.Towers.Find(t=>t.kind==TowerKind.Thorn&&t.level<3);
            if(core!=null){s.Upgrade(core);if(s.State==Phase.Prepare)s.StartWave();return;}
        }
        int pad=BestPad(s);
        if(pad>=0){
            TowerKind type=TowerKind.Thorn;
            bool frost=s.Towers.Exists(t=>t.kind==TowerKind.Frost),poison=s.Towers.Exists(t=>t.kind==TowerKind.Poison),bloom=s.Towers.Exists(t=>t.kind==TowerKind.Bloom);
            if(strategy>=4&&s.Level.index>=3&&!poison&&s.Towers.Count>=6)type=TowerKind.Poison;
            else if(strategy>=4&&s.Level.index>=2&&!frost&&s.Towers.Count>=5)type=TowerKind.Frost;
            else if(strategy==3&&s.Level.index>=3&&s.Towers.Count>=6)type=TowerKind.Poison;
            else if(strategy==2&&s.Level.index>=3&&!bloom&&s.Towers.Count>=2&&s.Wave<=2)type=TowerKind.Bloom;
            else if((strategy==1||strategy==2)&&s.Level.index>=2&&!frost&&s.Towers.Count>=2)type=TowerKind.Frost;
            else if((strategy==1||strategy==2)&&s.Level.index>=3&&!poison&&s.Towers.Count>=3)type=TowerKind.Poison;
            s.Plant(type,pad);
        }else{
            foreach(var t in s.Towers)if(t.level<3&&t.kind!=TowerKind.Bloom&&s.Upgrade(t))break;
        }
        if(s.State==Phase.Prepare)s.StartWave();
    }
    static int BestPad(Simulation s)
    {
        int best=-1;float score=float.MinValue;
        for(int i=0;i<s.Level.pads.Count;i++){
            var p=s.Level.pads[i];if(p.trap||p.polluted||s.Towers.Exists(t=>t.pad==i))continue;
            float coverage=0;
            foreach(var r in s.Level.routes)for(int k=1;k<r.points.Count;k++){
                float length=V.Distance(r.points[k-1],r.points[k]);for(float d=0;d<length;d+=.5f)if(V.Distance(V.Lerp(r.points[k-1],r.points[k],d/length),p.position)<=3)coverage+=.5f;
            }
            // Prefer a concentrated defense at shared bends; avoid income flowers in prime spots later.
            if(coverage>score){score=coverage;best=i;}
        }
        return best;
    }
}
