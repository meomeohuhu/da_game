using System;
using System.Collections.Generic;
namespace EcoGuardian.Core
{
    [Serializable] public struct V
    {
        public float x, y;
        public V(float x, float y) { this.x = x; this.y = y; }
        public static float Distance(V a, V b) { float x=a.x-b.x,y=a.y-b.y; return (float)Math.Sqrt(x*x+y*y); }
        public static V Lerp(V a,V b,float t) { return new V(a.x+(b.x-a.x)*t,a.y+(b.y-a.y)*t); }
    }
    public enum Phase { Prepare, Combat, Paused, Victory, Defeat }
    public enum TowerKind { Thorn, Frost, Bloom, Bomb, Poison }
    public enum EnemyKind { Scout, Lumber, Speed, Bulldozer, Jammer, Heavy, Toxic, Fire, Titan }
    public enum Priority { First, Nearest, Strongest }
    public enum Skill { Rain, Roots, Ancient }
    [Serializable] public class Group { public EnemyKind kind; public int count; public Group(EnemyKind k,int n) {kind=k;count=n;} }
    [Serializable] public class Wave { public List<Group> groups=new List<Group>(); public Wave(params Group[] g) {groups.AddRange(g);} }
    [Serializable] public class Route { public List<V> points=new List<V>(); public Route(params V[] p){points.AddRange(p);} }
    [Serializable] public class Pad { public V position; public bool trap, polluted; public Pad(V p,bool t=false,bool polluted=false){position=p;trap=t;this.polluted=polluted;} }
    [Serializable] public class Level
    {
        public int index; public string title, subtitle, ending;
        public List<Route> routes=new List<Route>(); public List<Pad> pads=new List<Pad>(); public List<Wave> waves=new List<Wave>();
    }
    public class EnemySpec
    {
        public float hp,speed,armor; public int damage,reward;
        public EnemySpec(float h,float s,int d,int r,float a=0){hp=h;speed=s;damage=d;reward=r;armor=a;}
    }
    public static class Rules
    {
        public static readonly int[] Build={50,80,100,120,110};
        public static readonly int[,] Upgrade={{60,90},{80,120},{100,150},{100,150},{100,140}};
        public static readonly int[] WaveReward={30,40,50,60,100};
        public static readonly int[] SkillCost={30,40,80};
        public static readonly float[] SkillCooldown={30,40,90};
        public static readonly EnemySpec[] Enemies={
            new EnemySpec(30,1.2f,5,5),new EnemySpec(80,.8f,10,10),new EnemySpec(45,1.8f,5,8),
            new EnemySpec(300,.45f,20,30,.3f),new EnemySpec(100,1,10,15),new EnemySpec(500,.4f,25,50),
            new EnemySpec(700,.4f,30,70),new EnemySpec(900,.4f,30,80),new EnemySpec(5000,.25f,100,200)};
        public static bool TowerUnlocked(TowerKind k,int level){return k==TowerKind.Thorn||k==TowerKind.Bomb||((k==TowerKind.Frost)&&level>=2)||((k==TowerKind.Bloom)&&level>=3)||((k==TowerKind.Poison)&&level>=3);}
        public static bool SkillUnlocked(Skill s,int level){return level>=3+(int)s;}
        public static int Stars(float hp,float max){return hp<=0?0:hp/max>=.8f?3:hp/max>=.5f?2:1;}
    }
    [Serializable] public class SaveData
    {
        public int unlocked=1; public int[] stars=new int[5]; public bool[] upgrades=new bool[3]; public bool music=true,sfx=true,shake=true;
        public int Earned {get {int n=0;foreach(int s in stars)n+=s;return n;}}
        public int Available {get {int n=Earned;foreach(bool b in upgrades)if(b)n-=3;return n;}}
        public bool Buy(int branch){if(branch<0||branch>=3||upgrades[branch]||Available<3)return false;upgrades[branch]=true;return true;}
        public void Complete(int level,int result){stars[level-1]=Math.Max(stars[level-1],result);unlocked=Math.Max(unlocked,Math.Min(5,level+1));}
        public void Sanitize(){if(stars==null||stars.Length!=5)stars=new int[5];if(upgrades==null||upgrades.Length!=3)upgrades=new bool[3];unlocked=Math.Max(1,Math.Min(5,unlocked));for(int i=0;i<5;i++)stars[i]=Math.Max(0,Math.Min(3,stars[i]));if(Available<0)upgrades=new bool[3];}
    }
}
