using System;
using System.Collections.Generic;
namespace EcoGuardian.Core
{
    public sealed class Enemy
    {
        public int id,route,segment=1,phase=1,summons; public EnemyKind kind; public V position;
        public float hp,maxHp,progress,slow,slowTime,poison,poisonTime,rootTime,phaseStop,summonClock,shockClock,shockWarning;
        public bool removed; public bool Boss {get{return kind>=EnemyKind.Heavy;}}
        public void Slow(float strength,float duration){if(Boss){strength*=.5f;}slow=Math.Max(slow,strength);slowTime=Math.Max(slowTime,duration);}
        public void Poison(float damage,float duration){poison=Math.Max(poison,damage);poisonTime=duration;}
        public void Root(float duration){rootTime=Math.Max(rootTime,Boss?duration*.5f:duration);}
    }
    public sealed class Tower
    {
        public int pad,level=1,spent,shots; public TowerKind kind; public Priority priority; public float clock,locked; public bool jammed;
    }
    public sealed class FireCell {public V position;public float warning=3,life=20,spread=8;public int generation;}
    public sealed class Fx {public V from,to;public int kind;public float life=.3f;}
    public sealed class Simulation
    {
        public readonly Level Level;public readonly SaveData Save;public readonly List<Enemy> Enemies=new List<Enemy>();public readonly List<Tower> Towers=new List<Tower>();
        public readonly List<FireCell> Fires=new List<FireCell>();public readonly List<Fx> Effects=new List<Fx>();public readonly float[] Cooldowns=new float[3];
        public Phase State {get;private set;}=Phase.Prepare;Phase beforePause;
        public int Wave {get;private set;} public int Energy {get;private set;} public float Hp {get;private set;} public float MaxHp {get;private set;}
        public float PrepareLeft {get;private set;}=15;public float Elapsed {get;private set;}public float UltimateLeft {get;private set;}
        public int Kills {get;private set;}public int Planted {get;private set;}public int RewardTotal {get;private set;}public string Message {get;private set;}="Chọn Cây Gai để bắt đầu.";
        public int Tutorial {get;private set;}public float TreeFlash {get;private set;}
        public int Remaining {get {int n=0;foreach(var e in Enemies)if(!e.removed)n++;foreach(var q in queues)n+=q.Count;return n;}}
        public bool Repaired {get;private set;}
        readonly List<Queue<EnemyKind>> queues=new List<Queue<EnemyKind>>();float[] spawnTimers;float income,fireClock=20;int nextId;readonly Random rng;bool completed;
        public Simulation(Level level,SaveData save,int seed=42){Level=level;Save=save;save.Sanitize();rng=new Random(seed);MaxHp=save.upgrades[1]?120:100;Hp=MaxHp;Energy=save.upgrades[2]?180:150;Tutorial=level.index==1?0:4;}
        public void Notify(string message){Message=message;}
        public void TogglePause(){if(State==Phase.Paused){State=beforePause;return;}if(State==Phase.Prepare||State==Phase.Combat){beforePause=State;State=Phase.Paused;}}
        public bool Plant(TowerKind kind,int pad)
        {
            if(State!=Phase.Prepare&&State!=Phase.Combat)return false;
            if(pad<0||pad>=Level.pads.Count||!Rules.TowerUnlocked(kind,Level.index))return Fail("Tháp chưa mở khóa.");
            if(Tutorial==1||Tutorial==2)return Fail("Hướng dẫn: bắt đầu wave rồi nâng cấp Cây Gai.");
            var p=Level.pads[pad];if(p.polluted||p.trap!=(kind==TowerKind.Bomb)||Campaign.OnPath(Level,p.position))return Fail("Ô này không phù hợp với loại tháp.");
            foreach(var t in Towers)if(t.pad==pad)return Fail("Ô đã có tháp.");
            if(IsBurning(p.position))return Fail("Hãy dập lửa trước khi trồng.");
            if(Tutorial==0&&kind!=TowerKind.Thorn)return Fail("Hướng dẫn: hãy trồng Cây Gai trước.");
            int price=Rules.Build[(int)kind];if(Energy<price)return Fail("Không đủ Sinh Khí.");Energy-=price;
            Towers.Add(new Tower{kind=kind,pad=pad,spent=price});Planted++;if(Tutorial==0)Tutorial=1;Message="Đã đánh thức người bảo vệ.";return true;
        }
        bool Fail(string s){Message=s;return false;}
        public bool Upgrade(Tower t){if(Tutorial==1)return Fail("Hướng dẫn: nhấn Bắt đầu wave trước khi nâng cấp.");if(!CanEdit(t)||t.level>=3)return false;int cost=Rules.Upgrade[(int)t.kind,t.level-1];if(Energy<cost)return Fail("Không đủ Sinh Khí.");Energy-=cost;t.spent+=cost;t.level++;if(Tutorial==2)Tutorial=4;Message="Tháp đã được nâng cấp.";return true;}
        bool CanEdit(Tower t){return (State==Phase.Prepare||State==Phase.Combat)&&t!=null&&Towers.Contains(t);}
        public bool Sell(Tower t){if(!CanEdit(t)||Tutorial<4)return false;Energy+=(int)Math.Floor(t.spent*.6);Towers.Remove(t);return true;}
        public bool Repair(){if(State!=Phase.Prepare||Repaired||Hp>=MaxHp||Energy<60)return Fail("Sửa cây: 60 Sinh Khí, một lần giữa wave.");Energy-=60;Hp=Math.Min(MaxHp,Hp+15);Repaired=true;return true;}
        public bool StartWave(bool early=true)
        {
            if(State!=Phase.Prepare||Wave>=5||Tutorial==0||Tutorial==2)return false;
            if(early&&Wave>0)Energy+=10;
            if(Tutorial==1)Tutorial=2;
            // Tutorial pauses combat for the first upgrade; spending is still allowed.
            Wave++;queues.Clear();for(int i=0;i<Level.routes.Count;i++)queues.Add(new Queue<EnemyKind>());
            int cursor=0;foreach(var g in Level.waves[Wave-1].groups)for(int i=0;i<g.count;i++){int lane=g.kind>=EnemyKind.Heavy?0:cursor++%queues.Count;queues[lane].Enqueue(g.kind);}
            spawnTimers=new float[queues.Count];State=Phase.Combat;Message="Đợt tấn công "+Wave+" / 5";return true;
        }
        public bool Cast(Skill skill,V at)
        {
            int k=(int)skill;if(State!=Phase.Combat||Tutorial==2||!Rules.SkillUnlocked(skill,Level.index)||Cooldowns[k]>0||Energy<Rules.SkillCost[k])return Fail("Kỹ năng chưa sẵn sàng hoặc thiếu Sinh Khí.");
            if(skill!=Skill.Ancient&&(at.x<0||at.x>19||at.y<0||at.y>10))return false;
            Energy-=Rules.SkillCost[k];Cooldowns[k]=Rules.SkillCooldown[k];
            if(skill==Skill.Ancient)UltimateLeft=10;
            else{float radius=skill==Skill.Rain?3:2;foreach(var e in Enemies)if(!e.removed&&V.Distance(e.position,at)<=radius){if(skill==Skill.Rain)e.Slow(.3f,4);else e.Root(3);}if(skill==Skill.Rain)Fires.RemoveAll(f=>V.Distance(f.position,at)<=3);Effects.Add(new Fx{from=at,to=at,kind=10+k,life=.7f});}
            return true;
        }
        public void Tick(float dt)
        {
            if(dt<=0||State==Phase.Paused||State==Phase.Victory||State==Phase.Defeat||Tutorial==0||Tutorial==1||Tutorial==2)return;
            Elapsed+=dt;TreeFlash=Math.Max(0,TreeFlash-dt);
            for(int i=Effects.Count-1;i>=0;i--){Effects[i].life-=dt;if(Effects[i].life<=0)Effects.RemoveAt(i);}
            if(State==Phase.Prepare){PrepareLeft-=dt;if(PrepareLeft<=0)StartWave(false);return;}
            income+=dt*2;int gained=(int)income;Energy+=gained;income-=gained;
            for(int i=0;i<3;i++)Cooldowns[i]=Math.Max(0,Cooldowns[i]-dt);UltimateLeft=Math.Max(0,UltimateLeft-dt);
            for(int lane=0;lane<queues.Count;lane++){spawnTimers[lane]-=dt;if(queues[lane].Count>0&&spawnTimers[lane]<=0){var kind=queues[lane].Dequeue();Spawn(kind,lane);spawnTimers[lane]+=kind>=EnemyKind.Bulldozer?2:1.2f;}}
            UpdateFire(dt);
            // Snapshot allows Titan to summon during iteration without invalidating the list.
            var snapshot=Enemies.ToArray();foreach(var e in snapshot){if(e.removed)continue;UpdateEnemy(e,dt);if(State==Phase.Defeat)break;}
            if(State==Phase.Defeat)return;
            foreach(var t in Towers)UpdateTower(t,dt);
            Enemies.RemoveAll(e=>e.removed);
            if(Remaining==0){int reward=Rules.WaveReward[Wave-1];Energy+=reward;RewardTotal+=reward;if(Wave==5){State=Phase.Victory;if(!completed){Save.Complete(Level.index,Rules.Stars(Hp,MaxHp));completed=true;}Message=Level.ending;}else{State=Phase.Prepare;PrepareLeft=15;Repaired=false;Message="Đã giữ vững đợt "+Wave+". Chuẩn bị phòng thủ!";}}
        }
        public Enemy Spawn(EnemyKind kind,int lane)
        {
            var spec=Rules.Enemies[(int)kind];var e=new Enemy{id=++nextId,kind=kind,route=lane%Level.routes.Count,hp=spec.hp,maxHp=spec.hp};e.position=Level.routes[e.route].points[0];Enemies.Add(e);return e;
        }
        public void Damage(Enemy e,float amount,bool poison=false)
        {
            if(e.removed||State!=Phase.Combat||amount<=0)return;
            e.hp-=amount*(poison?1:1-Rules.Enemies[(int)e.kind].armor);
            if(e.hp<=0){e.hp=0;e.removed=true;Kills++;Energy+=Rules.Enemies[(int)e.kind].reward;Effects.Add(new Fx{from=e.position,to=e.position,kind=20,life=.4f});}
        }
        void UpdateEnemy(Enemy e,float dt)
        {
            if(e.poisonTime>0){float active=Math.Min(dt,e.poisonTime);Damage(e,e.poison*active,true);e.poisonTime=Math.Max(0,e.poisonTime-dt);if(e.poisonTime==0)e.poison=0;if(e.removed)return;}
            if(e.kind==EnemyKind.Titan)UpdateTitan(e,dt);
            float slow=e.slowTime>0?e.slow:0;e.slowTime=Math.Max(0,e.slowTime-dt);if(e.slowTime==0)e.slow=0;
            float stop=Math.Max(e.rootTime,e.phaseStop);e.rootTime=Math.Max(0,e.rootTime-dt);e.phaseStop=Math.Max(0,e.phaseStop-dt);
            float speed=e.kind==EnemyKind.Titan&&e.phase==3?.5f:Rules.Enemies[(int)e.kind].speed;
            float travel=Math.Max(0,dt-stop)*speed*(1-slow);var path=Level.routes[e.route].points;
            while(travel>0&&!e.removed){float distance=V.Distance(e.position,path[e.segment]);if(distance>travel){e.position=V.Lerp(e.position,path[e.segment],travel/distance);e.progress+=travel;travel=0;}else{e.position=path[e.segment];e.progress+=distance;travel-=distance;e.segment++;if(e.segment>=path.Count){e.removed=true;Hp=Math.Max(0,Hp-Rules.Enemies[(int)e.kind].damage);TreeFlash=.4f;if(Hp<=0){State=Phase.Defeat;Message="Cây Mẹ đã ngã xuống. Hãy thử một chiến thuật khác.";}}}}
        }
        void UpdateTitan(Enemy e,float dt)
        {
            int phase=e.hp/e.maxHp<=.3f?3:e.hp/e.maxHp<=.7f?2:1;
            if(phase>e.phase){e.phase=phase;e.summonClock=0;e.shockClock=0;e.shockWarning=0;if(phase==2)e.phaseStop=3;Message=phase==3?"TITAN OVERDRIVE":"TITAN • Giai đoạn 2";}
            if(e.phase==3)return;
            e.summonClock+=dt;
            if(e.summonClock>=(e.phase==1?12:15)){e.summonClock=0;e.summons++;int lane=Level.routes.Count>1?1:0;if(e.phase==1){for(int i=0;i<3;i++)Spawn(EnemyKind.Scout,lane);}else{for(int i=0;i<2;i++){Spawn(EnemyKind.Lumber,lane);Spawn(EnemyKind.Speed,(lane+1)%Level.routes.Count);}if(e.summons%2==0)Spawn(EnemyKind.Jammer,lane);}}
            if(e.phase==2){e.shockClock+=dt;if(e.shockClock>=16&&e.shockWarning<=0)e.shockWarning=2;if(e.shockWarning>0){e.shockWarning-=dt;if(e.shockWarning<=0){foreach(var t in Towers)if(V.Distance(Level.pads[t.pad].position,e.position)<=3)t.locked=Math.Max(t.locked,3);e.shockClock=0;Effects.Add(new Fx{from=e.position,to=e.position,kind=12,life=.6f});}}}
        }
        public bool IsBurning(V p){foreach(var f in Fires)if(f.warning<=0&&V.Distance(f.position,p)<.7f)return true;return false;}
        void UpdateFire(float dt)
        {
            if(Level.index!=3||Wave<2)return;
            fireClock-=dt;if(fireClock<=0){fireClock+=20;int pad=(Wave*3+(int)(Elapsed/20))%Level.pads.Count;Fires.Add(new FireCell{position=Level.pads[pad].position});Message="Lửa rừng • Sắp bùng cháy!";}
            var added=new List<FireCell>();for(int i=Fires.Count-1;i>=0;i--){var f=Fires[i];if(f.warning>0){f.warning-=dt;continue;}f.life-=dt;f.spread-=dt;if(f.spread<=0&&f.generation<2){f.spread=float.PositiveInfinity;V next=new V(f.position.x+1,f.position.y);if(next.x<19&&!Fires.Exists(x=>V.Distance(x.position,next)<.1f))added.Add(new FireCell{position=next,warning=0,generation=f.generation+1});}if(f.life<=0)Fires.RemoveAt(i);}Fires.AddRange(added);
        }
        public float DamageValue(Tower t){float d=t.kind==TowerKind.Thorn?new[]{8,12,18}[t.level-1]:t.kind==TowerKind.Frost?4:t.kind==TowerKind.Poison?6:new[]{60,90,130}[t.level-1];if(Save.upgrades[0])d*=1.1f;if(UltimateLeft>0)d*=1.5f;return d;}
        public float Range(Tower t){return t.kind==TowerKind.Bomb?(t.level==3?2:1.5f):3;}
        public float Interval(Tower t){return t.kind==TowerKind.Thorn?new[]{.8f,.7f,.6f}[t.level-1]:t.kind==TowerKind.Frost?1.5f:t.kind==TowerKind.Poison?1.2f:t.kind==TowerKind.Bloom?10:new[]{8,7,6}[t.level-1];}
        public float DistanceRemaining(Enemy e){var p=Level.routes[e.route].points;float sum=V.Distance(e.position,p[e.segment]);for(int i=e.segment+1;i<p.Count;i++)sum+=V.Distance(p[i-1],p[i]);return sum;}
        void UpdateTower(Tower t,float dt)
        {
            t.locked=Math.Max(0,t.locked-dt);var at=Level.pads[t.pad].position;t.jammed=false;
            foreach(var e in Enemies)if(!e.removed&&e.kind==EnemyKind.Jammer&&V.Distance(e.position,at)<=2.5f){t.jammed=true;break;}
            if(t.locked>0||IsBurning(at))return;
            float rate=t.jammed&&t.kind!=TowerKind.Bloom?.7f:1;if(UltimateLeft>0&&t.kind!=TowerKind.Bloom&&t.kind!=TowerKind.Bomb)rate*=1.3f;t.clock+=dt*rate;
            if(t.clock<Interval(t))return;
            if(t.kind==TowerKind.Bloom){Energy+=new[]{15,20,30}[t.level-1];t.clock=0;Effects.Add(new Fx{from=at,to=at,kind=13,life=.7f});return;}
            Enemy target=null;float best=float.PositiveInfinity;
            foreach(var e in Enemies){if(e.removed||V.Distance(e.position,at)>Range(t))continue;float score=t.priority==Priority.First?DistanceRemaining(e):t.priority==Priority.Nearest?V.Distance(e.position,at):-e.hp;if(score<best){best=score;target=e;}}
            if(target==null){t.clock=Interval(t);return;}t.clock=0;t.shots++;
            if(t.kind==TowerKind.Bomb){foreach(var e in Enemies)if(!e.removed&&V.Distance(e.position,at)<=Range(t))Damage(e,DamageValue(t));Effects.Add(new Fx{from=at,to=at,kind=14,life=.6f});return;}
            Damage(target,DamageValue(t));Effects.Add(new Fx{from=at,to=target.position,kind=(int)t.kind,life=.25f});
            if(t.kind==TowerKind.Thorn&&t.level==3&&rng.NextDouble()<.2)Damage(target,DamageValue(t));
            if(t.kind==TowerKind.Frost){target.Slow(t.level==1?.3f:.4f,2);if(t.level==3&&t.shots%5==0)target.Root(.8f);}
            if(t.kind==TowerKind.Poison)target.Poison(new[]{5,8,12}[t.level-1],t.level==3?6:5);
        }
    }
}
