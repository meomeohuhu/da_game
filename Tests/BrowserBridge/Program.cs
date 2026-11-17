using System.Text.Json;
using BrowserDemo;
int checks=0;
void Check(bool condition,string message){if(!condition)throw new Exception(message);checks++;Console.WriteLine("PASS "+message);}
JsonElement Read(string json)=>JsonDocument.Parse(json).RootElement;
var save=Read(Engine.Init("broken JSON"));Check(save.GetProperty("unlocked").GetInt32()==1,"Corrupt browser save falls back to fresh progress");
Check(Engine.Begin(2)=="","Browser bridge enforces locked stages");
for(int i=1;i<=5;i++){var l=Read(Engine.Level(i));Check(l.GetProperty("waves").GetArrayLength()==5,"Browser receives five waves for stage "+i);}
var s=Read(Engine.Begin(1));Check(s.GetProperty("energy").GetInt32()==150,"Browser begins with 150 energy");Check(s.GetProperty("towers").GetArrayLength()==0,"Fresh browser session contains no towers");
var l1=Read(Engine.Level(1));int pad=0;while(l1.GetProperty("pads")[pad].GetProperty("trap").GetBoolean())pad++;
s=Read(Engine.Command("plant",0,pad,0,0));Check(s.GetProperty("energy").GetInt32()==100,"Browser placement charges correct price");Check(s.GetProperty("towers")[0].GetProperty("kind").GetInt32()==0,"Tower enum and fields serialize for Canvas UI");
s=Read(Engine.Command("wave",0,0,0,0));Check(s.GetProperty("tutorial").GetInt32()==2,"Browser enters upgrade tutorial");
s=Read(Engine.Command("upgrade",0,0,0,0));Check(s.GetProperty("towers")[0].GetProperty("level").GetInt32()==2&&s.GetProperty("energy").GetInt32()==40,"Browser upgrade costs 60 and resumes combat");
s=Read(Engine.Frame(.2f));Check(s.GetProperty("enemies").GetArrayLength()>0,"Browser Frame serializes a spawned enemy");
s=Read(Engine.Command("pause",0,0,0,0));float elapsed=s.GetProperty("elapsed").GetSingle();s=Read(Engine.Frame(10));Check(s.GetProperty("elapsed").GetSingle()==elapsed,"Browser pause stops simulation through frame bridge");
s=Read(Engine.Begin(1));Check(s.GetProperty("towers").GetArrayLength()==0&&s.GetProperty("energy").GetInt32()==150,"Browser retry resets session");
Engine.Init("{\"unlocked\":5,\"stars\":[3,3,3,3,3],\"upgrades\":[false,false,false]}");
var purchased=Read(Engine.Command("buy",1,0,0,0));Check(purchased.GetProperty("upgrades")[1].GetBoolean(),"Browser permanent upgrade persists in save JSON");
s=Read(Engine.Begin(5));Check(s.GetProperty("maxHp").GetInt32()==120,"Browser level uses purchased tree upgrade");Check(s.GetProperty("cooldowns").GetArrayLength()==3,"Browser receives all skill cooldowns");
Console.WriteLine("PASS: "+checks+" browser bridge assertions. Executed on .NET; does not cover actual browser rendering or WebAssembly execution.");
