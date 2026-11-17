using System.Text.Json;
using Microsoft.JSInterop;
using EcoGuardian.Core;
namespace BrowserDemo;
public static class Engine
{
    static readonly JsonSerializerOptions Json=new(){IncludeFields=true};
    static SaveData save=new();static Simulation game;
    [JSInvokable]public static string Init(string stored){try{save=JsonSerializer.Deserialize<SaveData>(stored,Json)??new();save.Sanitize();}catch{save=new();}return JsonSerializer.Serialize(save,Json);}
    [JSInvokable]public static string Level(int index){return JsonSerializer.Serialize(Campaign.Create(index),Json);}
    [JSInvokable]public static string Begin(int index){if(index<1||index>save.unlocked)return "";game=new(Campaign.Create(index),save);return Snapshot();}
    [JSInvokable]public static string Frame(float delta){if(game==null)return "";float remaining=Math.Clamp(delta,0,.25f);while(remaining>0){float dt=Math.Min(remaining,1f/60);game.Tick(dt);remaining-=dt;}return Snapshot();}
    [JSInvokable]public static string Command(string action,int a,int b,float x,float y)
    {
        if(action=="buy"){save.Buy(a);return JsonSerializer.Serialize(save,Json);}
        if(game==null)return "";
        switch(action){case "plant":game.Plant((TowerKind)a,b);break;case "upgrade":if(a>=0&&a<game.Towers.Count)game.Upgrade(game.Towers[a]);break;case "sell":if(a>=0&&a<game.Towers.Count)game.Sell(game.Towers[a]);break;case "priority":if(a>=0&&a<game.Towers.Count&&game.State!=Phase.Paused)game.Towers[a].priority=(Priority)(((int)game.Towers[a].priority+1)%3);break;case "wave":game.StartWave();break;case "pause":game.TogglePause();break;case "repair":game.Repair();break;case "skill":game.Cast((Skill)a,new V(x,y));break;}
        return Snapshot();
    }
    static string Snapshot()=>JsonSerializer.Serialize(new{
        phase=(int)game.State,wave=game.Wave,energy=game.Energy,hp=game.Hp,maxHp=game.MaxHp,elapsed=game.Elapsed,prepare=game.PrepareLeft,remaining=game.Remaining,kills=game.Kills,planted=game.Planted,rewards=game.RewardTotal,tutorial=game.Tutorial,message=game.Message,
        cooldowns=game.Cooldowns,ultimate=game.UltimateLeft,towers=game.Towers,
        enemies=game.Enemies.Where(e=>!e.removed).Select(e=>new{e.id,e.kind,e.position,e.hp,e.maxHp,e.phase,e.rootTime,e.poisonTime,e.shockWarning}),
        fires=game.Fires.Select(f=>new{f.position,f.warning,f.life}),effects=game.Effects,save=JsonSerializer.Serialize(save,Json)
    },Json);
}
