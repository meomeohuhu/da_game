using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using EcoGuardian.Core;
namespace EcoGuardian
{
    // Presentation is deliberately independent of the deterministic simulation.
    public sealed class GuardianGame:MonoBehaviour
    {
        const float Width=1600,Height=1000,Cell=56,BX=48,BY=190;
        readonly Dictionary<string,Texture2D> art=new Dictionary<string,Texture2D>();
        readonly string[] towerNames={"Cây Gai","Nấm Băng","Hoa Sinh Khí","Quả Nổ","Cây Độc"};
        readonly string[] towerHints={"Sát thương đơn • tầm 3","Làm chậm • đóng băng","Sinh Khí mỗi 10 giây","Bẫy diện rộng • ô vàng","Độc xuyên giáp • tầm 3"};
        readonly string[] skillNames={"Mưa Rừng","Rễ Đại Ngàn","Sức Mạnh Cổ Thụ"};
        readonly string[] enemySprites={"enemy_scout","enemy_lumber","enemy_drone","enemy_bulldozer","enemy_jammer","enemy_heavy","enemy_toxic","enemy_fire","enemy_titan"};
        readonly string[] towerSprites={"thorn","frost","bloom","bomb","poison"};
        SaveData save; Simulation game;string screen="menu";int choosing=-1,skill=-1;Tower selected;
        public string entryScreen="menu";
        bool resultSaved,showHelp,showSettings,titanSeen;float shakeLeft;int introStep=-1;float menuTime;int lastKills,lastWave;AudioSource music,sound;string saveWarning="";
        GUIStyle label,small,title,button;Texture2D white;Vector2 pointer;float scale;bool stylesReady;
        readonly Color ink=new Color(.83f,.91f,.83f),muted=new Color(.49f,.65f,.59f),gold=new Color(.94f,.76f,.46f),mint=new Color(.52f,.84f,.66f);
        string SavePath {get{return Path.Combine(string.IsNullOrEmpty(Environment.GetEnvironmentVariable("ECO_GUARDIAN_SAVE_DIR"))?Application.persistentDataPath:Environment.GetEnvironmentVariable("ECO_GUARDIAN_SAVE_DIR"),"eco-guardian-save.json");}}
        void Awake()
        {
            Application.targetFrameRate=60;white=Texture2D.whiteTexture;
            save=Load();music=gameObject.AddComponent<AudioSource>();sound=gameObject.AddComponent<AudioSource>();music.clip=Resources.Load<AudioClip>("Audio/forest");music.loop=true;music.volume=.45f;if(save.music)music.Play();
            screen=entryScreen;game=new Simulation(Campaign.Create(1),save);var cam=Camera.main;if(cam==null){var cameraObject=new GameObject("Main Camera");cameraObject.tag="MainCamera";cam=cameraObject.AddComponent<Camera>();cam.orthographic=true;cameraObject.AddComponent<AudioListener>();}cam.backgroundColor=new Color(.025f,.08f,.075f);
        }
        SaveData Load(){try{if(File.Exists(SavePath)){var s=JsonUtility.FromJson<SaveData>(File.ReadAllText(SavePath));if(s!=null){s.Sanitize();return s;}}}catch(Exception ex){Debug.LogWarning("Save could not be loaded: "+ex.Message);saveWarning="Không đọc được bản lưu. Đang dùng tiến trình mới.";}return new SaveData();}
        void Persist(){try{Directory.CreateDirectory(Path.GetDirectoryName(SavePath));string temp=SavePath+".tmp";File.WriteAllText(temp,JsonUtility.ToJson(save,true));if(File.Exists(SavePath))File.Copy(SavePath,SavePath+".bak",true);File.Copy(temp,SavePath,true);File.Delete(temp);}catch(Exception ex){saveWarning="Không lưu được tiến trình: "+ex.Message;Debug.LogWarning(saveWarning);}}
        void Cue(string name){if(save.sfx){var c=Resources.Load<AudioClip>("Audio/"+name);if(c!=null)sound.PlayOneShot(c);}}
        void Update()
        {
            menuTime+=Time.unscaledDeltaTime;
            if(screen=="play"){
                shakeLeft=Mathf.Max(0,shakeLeft-Time.unscaledDeltaTime);
                if(introStep>=0)return;
                if(Input.GetKeyDown(KeyCode.Escape)){if(choosing>=0||skill>=0||selected!=null){choosing=-1;skill=-1;selected=null;}else game.TogglePause();}
                if(Input.GetMouseButtonDown(1)){choosing=-1;skill=-1;selected=null;}
                // Fixed-size substeps keep spawn cadence/status integration stable on frame hitches.
                float remaining=Mathf.Min(Time.unscaledDeltaTime,.25f);while(remaining>0){float dt=Mathf.Min(remaining,1f/60);game.Tick(dt);remaining-=dt;}
                if(!titanSeen&&game.Enemies.Exists(e=>e.kind==EnemyKind.Titan)){titanSeen=true;shakeLeft=1;}
                if(game.TreeFlash>0)shakeLeft=Mathf.Max(shakeLeft,.2f);
                if(game.Kills>lastKills){Cue("hit");lastKills=game.Kills;}if(game.Wave>lastWave){Cue("wave");lastWave=game.Wave;}
                if(game.State==Phase.Victory&&!resultSaved){Persist();resultSaved=true;}
            }
        }
        public Simulation Session {get{return game;}}
        public void StartStage(int stage){if(stage<1||stage>5||stage>save.unlocked)return;var asset=Resources.Load<LevelData>("Levels/Level"+stage.ToString("00"));game=new Simulation(asset!=null?asset.configuration:Campaign.Create(stage),save);screen="play";selected=null;choosing=-1;skill=-1;resultSaved=false;lastKills=0;lastWave=0;titanSeen=false;introStep=stage==1?0:-1;}
        void EnsureStyles()
        {
            if(stylesReady)return;stylesReady=true;
            var font=Resources.Load<Font>("Fonts/GuardianSans");if(font==null)font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            label=new GUIStyle(GUI.skin.label){font=font,fontSize=21,normal={textColor=ink},wordWrap=true};small=new GUIStyle(label){fontSize=16,normal={textColor=muted}};
            title=new GUIStyle(label){fontSize=42,fontStyle=FontStyle.Bold};button=new GUIStyle(label){alignment=TextAnchor.MiddleCenter,fontSize=19};
        }
        void OnGUI()
        {
            EnsureStyles();scale=Mathf.Min(Screen.width/Width,Screen.height/Height);var offset=new Vector2((Screen.width-Width*scale)/2,(Screen.height-Height*scale)/2);
            if(save.shake&&shakeLeft>0)offset+=new Vector2(Mathf.Sin(menuTime*65),Mathf.Cos(menuTime*49))*4;
            GUI.matrix=Matrix4x4.TRS(offset,Quaternion.identity,Vector3.one*scale);pointer=(new Vector2(Input.mousePosition.x,Screen.height-Input.mousePosition.y)-offset)/scale;
            Fill(new Rect(0,0,Width,Height),new Color(.025f,.075f,.071f));
            bool overlay=showHelp||showSettings;GUI.enabled=!overlay;
            if(screen=="play"){GUI.enabled=!overlay&&introStep<0&&game.State!=Phase.Paused&&game.State!=Phase.Victory&&game.State!=Phase.Defeat;DrawGame();GUI.enabled=!overlay;if(introStep>=0)Intro();if(game.State==Phase.Paused)PausePanel();if(game.State==Phase.Victory||game.State==Phase.Defeat)ResultPanel();}
            else if(screen=="menu")Menu();else if(screen=="levels")LevelSelect();else if(screen=="upgrades")Upgrades();
            GUI.enabled=true;if(showHelp)Help();if(showSettings)Settings();if(saveWarning!="")Text(new Rect(40,970,1500,25),saveWarning,small);
        }
        void Fill(Rect r,Color c){GUI.color=c;GUI.DrawTexture(r,white);GUI.color=Color.white;}
        void Text(Rect r,string s,GUIStyle style=null,Color? color=null){var st=style??label;Color old=st.normal.textColor;if(color.HasValue)st.normal.textColor=color.Value;GUI.Label(r,s,st);st.normal.textColor=old;}
        bool Btn(Rect r,string text,bool enabled=true,bool accent=false)
        {
            Color c=accent?new Color(.21f,.45f,.34f):new Color(.075f,.17f,.15f);if(!enabled)c=new Color(.07f,.11f,.1f);else if(r.Contains(pointer))c+=new Color(.05f,.07f,.06f,0);
            Fill(r,c);Fill(new Rect(r.x,r.y,r.width,2),enabled?(accent?mint:new Color(.22f,.34f,.28f)):new Color(.1f,.18f,.15f));
            bool before=GUI.enabled;GUI.enabled=before&&enabled;bool hit=GUI.Button(r,text,button);GUI.enabled=before;return hit;
        }
        Texture2D Art(string name){if(!art.ContainsKey(name))art[name]=Resources.Load<Texture2D>("Art/"+name);return art[name];}
        void Sprite(string name,Rect r,Color? tint=null){var t=Art(name);if(t==null)return;GUI.color=tint??Color.white;GUI.DrawTexture(r,t,ScaleMode.ScaleToFit);GUI.color=Color.white;}
        Vector2 Pos(V v){return new Vector2(BX+(v.x+.5f)*Cell,BY+(v.y+.5f)*Cell);}
        void Line(Vector2 a,Vector2 b,float thickness,Color color){var old=GUI.matrix;float angle=Mathf.Atan2(b.y-a.y,b.x-a.x)*Mathf.Rad2Deg;GUIUtility.RotateAroundPivot(angle,a);Fill(new Rect(a.x,a.y-thickness/2,(b-a).magnitude,thickness),color);GUI.matrix=old;}
        void Circle(Vector2 p,float radius,Color c){for(int i=0;i<48;i++){float a=i*Mathf.PI/24,b=(i+1)*Mathf.PI/24;Line(p+new Vector2(Mathf.Cos(a),Mathf.Sin(a))*radius,p+new Vector2(Mathf.Cos(b),Mathf.Sin(b))*radius,2,c);}}
        void DrawMap(bool interactive)
        {
            var bounds=new Rect(BX,BY,Cell*20,Cell*11);Fill(new Rect(BX-8,BY-8,bounds.width+16,bounds.height+16),new Color(.12f,.25f,.20f));Fill(bounds,new Color(.12f,.25f,.20f));
            GUI.BeginGroup(bounds);
            // A restrained palette makes build pads, route branches and hazards readable.
            for(int x=0;x<20;x++)for(int y=0;y<11;y++){
                float n=((x*17+y*31)%7)*.003f;Fill(new Rect(x*Cell,y*Cell,Cell,Cell),new Color(.07f+n,.12f+n,.16f+n));
                if((x*13+y*7)%11==0)Sprite("kenney_20",new Rect(x*Cell+8,y*Cell+7,40,40),new Color(.7f,1,.8f,.4f));
            }
            foreach(var route in game.Level.routes)for(int i=1;i<route.points.Count;i++){
                var a=Pos(route.points[i-1])-new Vector2(BX,BY);var b=Pos(route.points[i])-new Vector2(BX,BY);
                Line(a,b,43,new Color(.20f,.29f,.23f));Line(a,b,32,new Color(.39f,.37f,.25f));Line(a,b,2,new Color(.55f,.49f,.30f));
            }
            for(int x=0;x<20;x++)for(int y=0;y<11;y++){
                var v=new V(x,y);if(Campaign.OnPath(game.Level,v)||game.Level.pads.Exists(p=>V.Distance(p.position,v)<1)||V.Distance(v,new V(19,5))<2)continue;
                if((x*7+y*11)%5==0)Sprite((x+y)%3==0?"kenney_136":"kenney_130",new Rect(x*Cell-9,y*Cell-16,72,72),new Color(.60f,.79f,.63f));
                else if((x*11+y*3)%17==0)Sprite("kenney_131",new Rect(x*Cell+10,y*Cell+14,38,38));
            }
            GUI.EndGroup();
            foreach(var p in game.Level.pads){var v=Pos(p.position);Color c=p.polluted?new Color(.45f,.26f,.48f):p.trap?new Color(.64f,.48f,.26f):new Color(.31f,.48f,.35f);Fill(new Rect(v.x-24,v.y-24,48,48),c);Fill(new Rect(v.x-21,v.y-21,42,42),new Color(.14f,.27f,.21f));if(p.polluted)Text(new Rect(v.x-18,v.y-17,40,40),"×",label);else{Line(v+new Vector2(-8,0),v+new Vector2(8,0),2,c);Line(v+new Vector2(0,-8),v+new Vector2(0,8),2,c);}}
            foreach(var t in game.Towers){var p=Pos(game.Level.pads[t.pad].position);float size=68+t.level*4;Sprite(towerSprites[(int)t.kind],new Rect(p.x-size/2,p.y-size+30,size,size));Text(new Rect(p.x-22,p.y+23,62,22),new string('•',t.level),small,gold);if(t.jammed)Text(new Rect(p.x-23,p.y-62,80,25),"NHIỄU",small,gold);if(t.locked>0)Text(new Rect(p.x-26,p.y-65,90,25),"KHÓA "+t.locked.ToString("0.0"),small,gold);}
            foreach(var f in game.Fires){var p=Pos(f.position);if(f.warning>0){Circle(p,26,new Color(1,.6f,.2f));Text(new Rect(p.x-20,p.y-18,70,30),f.warning.ToString("0"),label,gold);}else{Sprite("kenney_296",new Rect(p.x-25,p.y-40,50,64));Circle(p,27,new Color(.95f,.35f,.15f));}}
            foreach(var e in game.Enemies){if(e.removed)continue;var p=Pos(e.position);float size=e.kind==EnemyKind.Titan?100:e.Boss?72:44;Color tint=e.rootTime>0?new Color(.5f,.85f,1):e.poisonTime>0?new Color(.55f,1,.45f):Color.white;
                Sprite(enemySprites[(int)e.kind],new Rect(p.x-size/2,p.y-size/2,size,size),tint);if(e.kind==EnemyKind.Jammer)Circle(p,Cell*2.5f,new Color(.55f,.48f,.7f,.3f));if(e.shockWarning>0)Circle(p,Cell*3,new Color(1,.5f,.25f));
                Fill(new Rect(p.x-19,p.y-size/2-10,38,4),new Color(.1f,.15f,.13f));Fill(new Rect(p.x-19,p.y-size/2-10,38*e.hp/e.maxHp,4),e.Boss?gold:mint);
                if(e.rootTime>0)Text(new Rect(p.x-10,p.y-20,40,28),"❄",small);}
            var tree=Pos(new V(19,5));Sprite("ancient",new Rect(tree.x-80,tree.y-124,158,158),game.TreeFlash>0?new Color(1,.65f,.6f):Color.white);
            foreach(var fx in game.Effects){if(fx.kind<5){var to=Vector2.Lerp(Pos(fx.from),Pos(fx.to),1-fx.life/.25f);Sprite("kenney_22",new Rect(to.x-7,to.y-7,14,14),fx.kind==1?new Color(.6f,.9f,1):mint);}else Circle(Pos(fx.to),(1-fx.life/.8f)*Cell*(fx.kind==10?3:fx.kind==11?2:1.5f),fx.kind==14?gold:mint);}
            if(!interactive)return;
            if(selected!=null)Circle(Pos(game.Level.pads[selected.pad].position),game.Range(selected)*Cell,new Color(.7f,.86f,.62f,.6f));
            var mouseV=new V((pointer.x-BX)/Cell-.5f,(pointer.y-BY)/Cell-.5f);
            if(bounds.Contains(pointer)&&(choosing>=0||skill>=0)){
                if(skill>=0)Circle(pointer,(skill==0?3:2)*Cell,new Color(.5f,.85f,1,.7f));
                else{int p=FindPad(mouseV);bool valid=p>=0&&!game.Level.pads[p].polluted&&game.Level.pads[p].trap==(choosing==3)&&!game.Towers.Exists(t=>t.pad==p)&&game.Energy>=Rules.Build[choosing]&&!game.IsBurning(game.Level.pads[p].position);Circle(p>=0?Pos(game.Level.pads[p].position):pointer,(choosing==3?1.5f:3)*Cell,valid?mint:new Color(.95f,.4f,.35f));Sprite(towerSprites[choosing],new Rect(pointer.x-30,pointer.y-50,60,60),new Color(1,1,1,.7f));}
            }
            if(Event.current.type==EventType.MouseUp&&Event.current.button==0&&bounds.Contains(pointer)&&GUI.enabled&&game.State!=Phase.Paused&&game.State!=Phase.Victory&&game.State!=Phase.Defeat){
                if(skill>=0){if(game.Cast((Skill)skill,mouseV)){Cue("plant");skill=-1;}}
                else{int p=FindPad(mouseV);if(choosing>=0){if(game.Plant((TowerKind)choosing,p))Cue("plant");}else selected=game.Towers.Find(t=>t.pad==p);}
                Event.current.Use();
            }
        }
        int FindPad(V p){for(int i=0;i<game.Level.pads.Count;i++)if(V.Distance(game.Level.pads[i].position,p)<.48f)return i;return -1;}
        void DrawGame()
        {
            Text(new Rect(48,22,850,44),"ECO GUARDIAN",title);Text(new Rect(50,74,900,30),"CHƯƠNG "+game.Level.index.ToString("00")+"  /  "+game.Level.title,small,gold);
            if(Btn(new Rect(1380,32,172,48),"Tạm dừng"))game.TogglePause();
            Fill(new Rect(48,120,1120,47),new Color(.07f,.16f,.13f));Text(new Rect(65,129,275,32),"CÂY MẸ  "+Mathf.CeilToInt(game.Hp)+" / "+game.MaxHp,label,mint);
            Text(new Rect(442,129,395,32),"WAVE "+game.Wave+" / 5     •     CÒN "+game.Remaining,small);Text(new Rect(915,129,250,32),"✦ "+game.Energy+" Sinh Khí",label,gold);
            DrawMap(true);Sidebar();
            for(int i=0;i<5;i++){float x=48+i*228;bool unlocked=Rules.TowerUnlocked((TowerKind)i,game.Level.index);Fill(new Rect(x,834,216,112),choosing==i?new Color(.19f,.36f,.27f):new Color(.07f,.16f,.13f));Sprite(towerSprites[i],new Rect(x+8,844,62,70),unlocked?Color.white:new Color(.35f,.4f,.36f));Text(new Rect(x+77,845,134,28),towerNames[i],small,unlocked?ink:muted);Text(new Rect(x+77,874,130,25),unlocked?Rules.Build[i]+" Sinh Khí":"Mở ở màn "+(i==1?2:3),small,gold);
                if(Btn(new Rect(x+8,914,200,28),choosing==i?"ĐANG CHỌN":"TRỒNG",unlocked&&game.State!=Phase.Paused)){choosing=i;skill=-1;selected=null;}}
            Text(new Rect(48,963,1130,28),"Ô xanh: tháp   •   Ô vàng: bẫy   •   Chuột phải / Esc: hủy   •   "+(int)(game.Elapsed/60)+" phút "+(int)(game.Elapsed%60)+" giây",small);
            foreach(var e in game.Enemies)if(!e.removed&&e.kind==EnemyKind.Titan){Fill(new Rect(240,90,690,17),new Color(.18f,.20f,.18f));Fill(new Rect(240,90,690*e.hp/e.maxHp,17),gold);Text(new Rect(760,68,500,24),"TITAN LUMBER-X • GIAI ĐOẠN "+e.phase,small,gold);break;}
        }
        void Sidebar()
        {
            const float x=1210;Text(new Rect(x,128,340,40),game.State==Phase.Prepare?"CHUẨN BỊ":"BẢO VỆ ĐẠI NGÀN",label,gold);
            Text(new Rect(x,191,338,88),game.Message,label);
            if(game.Tutorial<4){string text=game.Tutorial==0?"01 • Chọn Cây Gai rồi nhấp một ô xanh.":game.Tutorial==1?"02 • Nhấn Bắt đầu. Thời gian đang dừng để bạn làm quen.":"03 • Nhấp tháp đã trồng rồi Nâng cấp. Chiến đấu bắt đầu sau thao tác này.";Text(new Rect(x,286,335,110),text,small,mint);}
            else Text(new Rect(x,286,335,95),game.Level.subtitle,small);
            if(game.State==Phase.Prepare){Text(new Rect(x,394,330,30),"Đợt tiếp theo: "+Mathf.CeilToInt(game.PrepareLeft)+" giây",small);if(Btn(new Rect(x,434,340,53),game.Wave==0?"Bắt đầu wave 1":"Bắt đầu sớm  +10",true,true))game.StartWave();if(Btn(new Rect(x,497,340,40),"Sửa cây +15 HP   /   60",!game.Repaired&&game.Hp<game.MaxHp&&game.Energy>=60))game.Repair();}
            else if(selected==null)Text(new Rect(x,403,335,70),"Chọn một tháp để nâng cấp, đổi ưu tiên hoặc bán.",small);
            if(selected!=null){var t=selected;Fill(new Rect(x,550,340,250),new Color(.075f,.17f,.14f));Text(new Rect(x+14,561,320,32),towerNames[(int)t.kind]+" • Cấp "+t.level,label,mint);Text(new Rect(x+14,600,315,50),t.kind==TowerKind.Bloom?"Tạo "+new[]{15,20,30}[t.level-1]+" Sinh Khí / 10 giây":"Sát thương "+game.DamageValue(t).ToString("0.#")+" • Tầm "+game.Range(t)+"\nChu kỳ "+game.Interval(t).ToString("0.0")+" giây",small);
                if(Btn(new Rect(x+12,660,316,36),"Ưu tiên: "+new[]{"Gần cây","Gần nhất","Máu cao"}[(int)t.priority],t.kind!=TowerKind.Bloom&&t.kind!=TowerKind.Bomb))t.priority=(Priority)(((int)t.priority+1)%3);
                if(Btn(new Rect(x+12,705,185,42),t.level==3?"Đã tối đa":"Nâng cấp / "+Rules.Upgrade[(int)t.kind,t.level-1],t.level<3,true)){if(game.Upgrade(t))Cue("plant");}
                if(Btn(new Rect(x+206,705,122,42),"Bán +"+(int)(t.spent*.6),game.Tutorial>=4)){game.Sell(t);selected=null;}
            }
            for(int i=0;i<3;i++){bool unlocked=Rules.SkillUnlocked((Skill)i,game.Level.index);float y=820+i*51;string text=skillNames[i]+"  /  "+Rules.SkillCost[i]+(game.Cooldowns[i]>0?"  •  "+game.Cooldowns[i].ToString("0")+"s":"");if(Btn(new Rect(x,y,340,43),unlocked?text:skillNames[i]+" • Màn "+(3+i),unlocked&&game.State==Phase.Combat&&game.Cooldowns[i]<=0&&game.Energy>=Rules.SkillCost[i],skill==i)){if(i==2)game.Cast(Skill.Ancient,new V());else{skill=i;choosing=-1;selected=null;}}}
        }
        void Backdrop(){DrawMap(false);Fill(new Rect(0,0,1600,1000),new Color(.015f,.05f,.045f,.88f));}
        void Menu()
        {
            Backdrop();Sprite("ancient",new Rect(920,235,470,470));Text(new Rect(100,130,850,38),"LỜI THÌ THẦM CỦA ĐẠI NGÀN",label,gold);Text(new Rect(95,187,920,90),"ECO GUARDIAN",new GUIStyle(title){fontSize=70});Text(new Rect(100,290,710,90),"Hãy đánh thức những người bảo vệ.\nGiữ lấy nguồn sống cuối cùng của khu rừng.",label);
            string[] opts={"Chơi tiếp","Chọn màn","Nâng cấp vĩnh viễn","Hướng dẫn","Cài đặt","Thoát"};
            for(int i=0;i<opts.Length;i++)if(Btn(new Rect(100,420+i*65,420,51),opts[i],true,i==0))switch(i){case 0:StartStage(ContinueStage());break;case 1:screen="levels";break;case 2:screen="upgrades";break;case 3:showHelp=true;break;case 4:showSettings=true;break;case 5:Application.Quit();break;}
            Text(new Rect(970,740,420,60),"AN & LINH HỒN RỪNG\n5 chương  /  25 đợt tấn công",small,mint);Text(new Rect(100,927,1300,32),"UNITY 2D  •  PC / CHUỘT  •  NGƯỜI GÁC RỪNG",small);
        }
        int ContinueStage(){for(int i=0;i<save.unlocked;i++)if(save.stars[i]==0)return i+1;return save.unlocked;}
        void LevelSelect()
        {
            Backdrop();Text(new Rect(80,70,1000,80),"HÀNH TRÌNH CỦA AN",title);Text(new Rect(82,140,1200,40),"Mỗi chiến thắng mở chương tiếp theo. Giữ ít nhất 80% HP để nhận 3 sao.",small);
            for(int i=0;i<5;i++){var l=Campaign.Create(i+1);float y=220+i*126;bool open=i+1<=save.unlocked;Fill(new Rect(80,y,1440,105),new Color(.07f,.16f,.13f));Text(new Rect(105,y+19,950,40),(i+1).ToString("00")+"   "+l.title,label,open?ink:muted);Text(new Rect(108,y+61,950,30),l.subtitle,small);Text(new Rect(1110,y+22,180,50),new string('★',save.stars[i])+new string('☆',3-save.stars[i]),label,gold);if(Btn(new Rect(1300,y+23,190,57),open?"Chơi":"Đang khóa",open))StartStage(i+1);}
            if(Btn(new Rect(80,900,240,50),"Về menu"))screen="menu";
        }
        void Upgrades()
        {
            Backdrop();Text(new Rect(80,80,1300,65),"PHÚC LÀNH CỦA ĐẠI NGÀN",title);Text(new Rect(84,163,1200,45),"Sao có thể dùng: "+save.Available+"  /  Tổng sao tốt nhất: "+save.Earned,label,gold);
            string[] names={"Tấn công","Phòng thủ","Kinh tế"};string[] desc={"+10% sát thương trực tiếp cho tháp tấn công.","+20 HP tối đa cho Cây Cổ Thụ.","+30 Sinh Khí khi bắt đầu mỗi màn."};
            for(int i=0;i<3;i++){float x=80+i*490;Fill(new Rect(x,300,460,350),new Color(.075f,.17f,.14f));Sprite(new[]{"thorn","ancient","bloom"}[i],new Rect(x+160,320,140,140));Text(new Rect(x+25,475,410,40),names[i],label,mint);Text(new Rect(x+25,524,410,65),desc[i],small);if(Btn(new Rect(x+25,591,410,43),save.upgrades[i]?"Đã sở hữu":"Mở khóa • 3 sao",!save.upgrades[i]&&save.Available>=3,true)){save.Buy(i);Persist();}}
            if(Btn(new Rect(80,900,240,50),"Về menu"))screen="menu";
        }
        void Intro()
        {
            GUI.enabled=true;Overlay();Sprite("ancient",new Rect(875,190,240,230));
            Text(new Rect(470,225,420,80),"LỜI GỌI CỦA CÂY MẸ",title);
            string[] lines={"Dòng suối chảy qua những tán cây. Ở trung tâm Đại Ngàn, Cây Cổ Thụ vẫn tỏa sáng.\n\nRồi tiếng máy vang lên. Một cây nhỏ ngã xuống. Ánh sáng xanh gọi Linh Hồn Rừng đến trước An.","Linh Hồn Rừng: ‘Nếu Cây Mẹ ngã xuống, sự sống ở Đại Ngàn cũng sẽ tắt. Hãy dùng Sinh Khí để đánh thức những người bảo vệ khu rừng.’\n\nAn: ‘Tôi sẽ giữ con đường này. Chỉ cho tôi cách bắt đầu.’"};
            Text(new Rect(470,430,650,235),lines[introStep],label);
            if(Btn(new Rect(470,710,660,55),introStep==0?"Lắng nghe Linh Hồn Rừng":"Bắt đầu hướng dẫn",true,true)){introStep++;if(introStep>=2)introStep=-1;}
        }
        void Overlay(){Fill(new Rect(0,0,1600,1000),new Color(0,.025f,.02f,.87f));Fill(new Rect(420,180,760,640),new Color(.06f,.14f,.115f));}
        void PausePanel(){Overlay();Text(new Rect(470,225,650,70),"ĐẠI NGÀN ĐANG CHỜ",title);Text(new Rect(470,315,650,80),"Tất cả đồng hồ, địch và tài nguyên đã dừng.",label);if(Btn(new Rect(470,450,660,65),"Tiếp tục",true,true))game.TogglePause();if(Btn(new Rect(470,535,660,65),"Chơi lại"))StartStage(game.Level.index);if(Btn(new Rect(470,620,660,65),"Menu chính"))screen="menu";}
        void ResultPanel()
        {
            Overlay();bool won=game.State==Phase.Victory;Text(new Rect(470,218,665,105),won?(game.Level.index==5?"KHU RỪNG ĐÃ ĐƯỢC BẢO VỆ":"ĐẠI NGÀN VẪN ĐỨNG VỮNG"):"HÃY THỬ LẠI",title);Text(new Rect(470,334,650,90),game.Message,label);
            Text(new Rect(470,442,650,100),"Địch tiêu diệt: "+game.Kills+"   •   Tháp đã trồng: "+game.Planted+"\nHP cây: "+Mathf.CeilToInt(game.Hp)+" / "+game.MaxHp+"   •   Thời gian: "+(int)(game.Elapsed/60)+"m "+(int)(game.Elapsed%60)+"s\nThưởng wave thực nhận: "+game.RewardTotal+"   •   Sao: "+(won?Rules.Stars(game.Hp,game.MaxHp):0),label,gold);
            if(Btn(new Rect(470,570,660,55),won&&game.Level.index<5?"Chương tiếp theo":"Thử lại",true,true))StartStage(won&&game.Level.index<5?game.Level.index+1:game.Level.index);
            if(Btn(new Rect(470,643,316,50),"Chọn màn"))screen="levels";if(Btn(new Rect(804,643,326,50),"Menu chính"))screen="menu";
        }
        void Help(){Overlay();Text(new Rect(470,220,650,65),"GIỮ LẤY NGUỒN SỐNG",title);Text(new Rect(470,310,650,340),"Trồng tháp trên ô xanh; Quả Nổ dùng ô vàng sát đường. Ô tím bị ô nhiễm không thể xây.\n\nNhấp một tháp để nâng cấp, đổi ưu tiên hoặc bán. Dùng Nấm Băng cùng Cây Độc để giữ địch trong vùng bắn.\n\nSinh Khí chỉ tự hồi trong wave. Sửa cây giữa các đợt. Kỹ năng mở từ màn 3; chọn rồi nhấp lên bản đồ.\n\nEsc để hủy thao tác hoặc tạm dừng. Chuột phải để bỏ chọn.",label);if(Btn(new Rect(470,715,660,55),"Đã hiểu",true,true))showHelp=false;}
        void Settings(){Overlay();Text(new Rect(470,230,650,70),"CÀI ĐẶT",title);if(Btn(new Rect(470,360,660,60),"Nhạc nền: "+(save.music?"Bật":"Tắt"))){save.music=!save.music;if(save.music)music.Play();else music.Stop();Persist();}if(Btn(new Rect(470,445,660,60),"Hiệu ứng âm thanh: "+(save.sfx?"Bật":"Tắt"))){save.sfx=!save.sfx;Persist();}if(Btn(new Rect(470,530,660,60),"Rung màn hình: "+(save.shake?"Bật":"Tắt"))){save.shake=!save.shake;Persist();}if(Btn(new Rect(470,715,660,55),"Đóng",true,true))showSettings=false;}
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]static void Bootstrap(){if(FindObjectOfType<GuardianGame>()==null)new GameObject("Eco Guardian").AddComponent<GuardianGame>();}
    }
}
